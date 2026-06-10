# Documented File: HiLoAnalysis2021.cs
**Repository Path:** `_Utillib\HiLoAnalysis2021.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
﻿using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace _Utillib
{
    /// <summary>
    /// This class provides a set of static methods that, together, perform a HiLo analysis
    /// on a user's PDF table set; the HiLo algorithm implemented by this class is the 'new'
    /// HiLo, released in August 2021.
    /// </summary>
    /// <remarks>
    /// Documentation of the new HiLo analysis algorithm and its software implementation is
    /// provided by Technical Note AH-0042.
    /// </remarks>
    public class HiLoAnalysis2021
    {
        /// <summary>
        /// Create an array of space-tabs to simplify the alignment of lines of text
        /// in the HiLo output report.
        /// </summary>
        public static string[] tabs = MakeArrayOfTabs(4, 12);

        /// <summary>
        /// This boolean determines whether HiLo2021 or the legacy HiLo is used.
        /// </summary>
        private static bool mUseLegacyHiLo = false;

        /// <summary>
        /// The static class constructor is used to get the value of the environment variable
        /// "UseLegacyHiLo"; if this environment variable has been set to "true' then the legacy
        /// HiLo claculation is used.
        /// </summary>
        static HiLoAnalysis2021() 
        {
            string value = Environment.GetEnvironmentVariable("UseLegacyHiLo");
            if ((value != null) && value.ToUpper().Equals("TRUE")) mUseLegacyHiLo = true;
        }

        /// <summary>
        /// This is the method called from the MICS# programs FtValidate, TpRunTsip and CheckHiLo 
        /// that performs a HiLo analysis on all the sites (and their channels) as prescribed in the user's
        /// import PDF table set.
        /// </summary>
        /// <param name="pdfName"> - root name of the user's PDF table set.</param>
        /// <param name="isVerbose"> - TRUE if caller wants verbose reporting.</param>
        /// <param name="dDistKm"> - HiLo coordination distance in Km.</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <returns></returns>
        public static int HiloCheckFunc(string pdfName, bool isVerbose, double dDistKm, TextWriter tw)
        {
            //...Log2.v("\n\nHiLoAnalysis.HiloCheckFunc(): Entry");

            // The following code block is to enable the legacy HiLo calculation to be instead
            // of HiLo2021 (the default).
            if (mUseLegacyHiLo)
            {
                int rv = HiloCheckSupp.HiloCheckFunc(pdfName, isVerbose, dDistKm, tw);
                return rv;
            }

            int nSites = 0;
            string msg = "";

            int internalSameBandViolationsCount = 0;
            int internalAdjacentBandViolationsCount = 0;
            int totalInternalViolationsForSite = 0;
            int totalInternalViolationsForPDF = 0;

            int externalSameBandViolationsCount = 0;
            int externalAdjacentBandViolationsCount = 0;
            int totalExternalViolationsForSite = 0;
            int totalExternalViolationsForPDF = 0;

            string pdfSiteTableName = "";
            string whereClause = "";
            string orderByClause = "";

            // Check that the table name is not empty.
            if (String.IsNullOrWhiteSpace(pdfName))
            {
                return Error.HILOTABLENAMEISEMPTY;
            }

            // Create the full TS site table name from the PDF name.
            GenUtil.UtCvtName(Constant.FT_SITE, pdfName, out pdfSiteTableName);

            // Create the full TS chan table name from the PDF name.
            string pdfChanTableName;
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out pdfChanTableName);

            // Get the list of all sites in the PDF table set.
            List<FtSite> listOfAllFtSitesInPDF;

            whereClause = "";
            orderByClause = "call1";

            DynSite.GetListOfFtSites(pdfSiteTableName, whereClause, orderByClause, out listOfAllFtSitesInPDF);

            // Check that some FtSites were actually found in the PDF table set.
            if (listOfAllFtSitesInPDF.Count == 0) return Constant.FAILURE;

            // For each site in the user's PDF table set, perform the HiLo analysis.
            foreach (FtSite pdfSite in listOfAllFtSitesInPDF)
            {
                // Increment the counter for the number of PDF sites that are analyzed.
                nSites++;

                // Get the list of all channels for this site in the PDF table set.
                // It is wise to get the channels on a site-by-site basis because the 
                // import PDF might be an enormous TAFL import.
                List<FtChan> listOfAllFtChansForPDFsite;

                whereClause = String.Format("call1 = '{0}'", pdfSite.call1);
                orderByClause = "call1, bndcde, call2, chid";

                DynChannel.GetListOfFtChans(pdfChanTableName, whereClause, orderByClause, out listOfAllFtChansForPDFsite);

                List<ChanHiLo> pdfSiteChanHiLoList = ChanHiLo.ConvertList(listOfAllFtChansForPDFsite);

                // AH: REMOVE
                //foreach (ChanHiLo chanHiLo in pdfSiteChanHiLoList) Console.Write("\nZULU {0}", chanHiLo.ToStringAll());

                // Create a list of those PDF channels that are being deleted.
                List<ChanHiLo> deletedChanHiLoList = SelectRecordsWithCMD(pdfSiteChanHiLoList, "D");

                // Exclude PDF chanHiLo records that are marked for deletion.
                pdfSiteChanHiLoList = ExcludeRecordsWithCMD(pdfSiteChanHiLoList, "D");

                // AH: REMOVE
                // foreach (ChanHiLo chanHiLo in pdfSiteChanHiLoList) Console.Write("\nTOAD {0}", chanHiLo.ToStringAll());

                msg = String.Format("=========== HiLo analysis of site {0} ({1}/{2}) in proposed file. ===========", pdfSite.call1, pdfSite.name.Trim(), pdfSite.oper.Trim());
                string line = Strings.RepeatedChar('=', msg.Length);
                tw.Write("\n\n{0}\n{1}\n{0}", line, msg);

                msg = String.Format("Internal to PDF file; site {0}:", pdfSite.call1);
                tw.Write("\n\n" + msg);

                List<ChanHiLoGroup> pdfChanHiLoGroupList = null;

                //===================================================================================================
                // The following method call performs the checking for HiLo violations on a prescribed PDF file site.
                //===================================================================================================
                CheckHiLoPdfSite(pdfSite.call1, pdfSiteChanHiLoList, isVerbose, tw, out internalSameBandViolationsCount, out internalAdjacentBandViolationsCount, out pdfChanHiLoGroupList);

                // Write out the numerical summaries of the PDF site/channels HiLo analysis.
                msg = String.Format("\n\nInternal HiLo summary for site {0} : ", pdfSite.call1);
                msg += String.Format("\n{0}[a] Primary  band potential VIOLATIONs : {1}", tabs[11], internalSameBandViolationsCount);

                if (internalSameBandViolationsCount == 0)
                {
                    msg += String.Format("\n{0}[b] Adjacent band potential VIOLATIONs : {1}", tabs[11], internalAdjacentBandViolationsCount);
                }
                else
                {
                    msg += String.Format("\n{0}[b] Adjacent band potential VIOLATIONs : N/A.", tabs[11]);
                }

                msg = Tabize(msg, 1);
                tw.Write(msg);

                totalInternalViolationsForSite = internalSameBandViolationsCount + internalAdjacentBandViolationsCount;
                totalInternalViolationsForPDF += totalInternalViolationsForSite;

                // Write header for start of external analysis of MDB sites.
                msg = String.Format("Existing MDB channels for site {0} and nearby sites:", pdfSite.call1);
                tw.Write("\n\n" + msg);

                // Only perform HiLo analysis of nearby MDB sites if the intra-site analysis
                // found no violations.
                if (totalInternalViolationsForSite != 0)
                {
                    msg = String.Format("\n\nFile site {0} has internal HiLo violations: existing MDB sites were not checked. ", pdfSite.call1);

                    tw.Write(Tabize(msg, 1));
                }
                else
                {
                    // This block provides the top-level control-flow for the HiLo analysis of sites 
                    // external to the PDF.

                    List<MtSite> mtSiteList;
                    List<MtChan> mtChanListAllSites = new List<MtChan>();

                    GetCondition(out whereClause, pdfSite, dDistKm);
                    //...Log2.v("\nHiLoAnalysis.nHiLoCheckFunc(): whereClause: " + whereClause);

                    orderByClause = "call1";

                    DynMdbSite.GetListOfMtSites(whereClause, orderByClause, out mtSiteList);

                    msg = String.Format("\n\nThere are {0} existing MDB sites that are closer than {1:F0} m ...", mtSiteList.Count, dDistKm * 1000);
                    tw.Write(Tabize(msg, 1));

                    foreach (MtSite mtSite in mtSiteList)
                    {
                        whereClause = String.Format("call1='{0}' ", mtSite.call1);
                        orderByClause = "bndcde, call2, chid";

                        List<MtChan> mtChanListForOneSite;
                        DynMdbChannel.GetListOfMtChans(whereClause, orderByClause, out mtChanListForOneSite);

                        mtChanListAllSites.AddRange(mtChanListForOneSite);
                    }

                    List<ChanHiLo> mtChanHiLoListAllSites = ChanHiLo.ConvertList(mtChanListAllSites);

                    // The following method call performs all the MDB site HiLo analysis.
                    CheckHiLoMdbSites(pdfSite.call1, mtChanHiLoListAllSites, isVerbose, dDistKm, tw, out externalSameBandViolationsCount, out externalAdjacentBandViolationsCount, pdfChanHiLoGroupList, deletedChanHiLoList);

                    // Write out the numerical summaries of the MDB site/channels HiLo analysis.
                    msg = String.Format("\n\nExternal HiLo summary for site {0} : ", pdfSite.call1);
                    msg += String.Format("\n{0}[a] Same     band potential VIOLATIONs : {1}", tabs[11], externalSameBandViolationsCount);

                    if (externalSameBandViolationsCount == 0)
                    {
                        msg += String.Format("\n{0}[b] Adjacent band potential VIOLATIONs : {1}", tabs[11], externalAdjacentBandViolationsCount);
                    }
                    else
                    {
                        msg += String.Format("\n{0}[b] Adjacent band potential VIOLATIONs : N/A.", tabs[11]);
                    }

                    msg = Tabize(msg, 1);
                    tw.Write(msg);
                }

                // Accumulate the violation counts.
                totalExternalViolationsForSite = externalSameBandViolationsCount + externalAdjacentBandViolationsCount;
                totalExternalViolationsForPDF += totalExternalViolationsForSite;

            } // pdfSite

            tw.Write("\n");
            tw.Write("\n===================================================================");
            tw.Write("\n===== Summary of HiLo Analysis for PDF file {0,-16}  =====", pdfName);
            tw.Write("\n=====                                                         =====");
            tw.Write("\n=====     Number of proposed file sites analyzed : {0,6}     =====", nSites);
            tw.Write("\n=====     Coordination distance for MDB sites    : {0,6:F0} m   =====", dDistKm * 1000);
            tw.Write("\n=====                                                         =====");
            tw.Write("\n=====     Potential HiLo violations encountered:              =====");
            tw.Write("\n=====         [a] channels internal to PDF file  : {0,6}     =====", totalInternalViolationsForPDF);
            tw.Write("\n=====         [b] existing/nearby MDB channels   : {0,6}     =====", totalExternalViolationsForPDF);
            tw.Write("\n===================================================================\n");

            //...Log2.v("\n\nHiLoAnalysis.HiloCheckFunc(): Exit");
            return totalInternalViolationsForPDF + totalExternalViolationsForPDF;
        }

        /// <summary>
        /// This method performs a same-band and adjacent-band HiLo analysis for a prescribed site in a
        /// PDF file; the site's channels are only compared with other site's channels in the PDF.
        /// </summary>
        /// <param name="call1"> - call sign of the prescribed PDF file site.</param>
        /// <param name="chanHiLoList"> - a list of all the ChanHiLo objects for the site.</param>
        /// <param name="IsVerbose"> - TRUE if caller wants verbose reporting.</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <param name="sameBandViolationsCount"> - output count of same-band violations.</param>
        /// <param name="adjcBandViolationsCount"> - output count of adjacent band violations.</param>
        /// <param name="pdfChanHiLoGroupList"> - output list of ChanHiLoGroup objects to be used in subsequent call to CheckHiLoMdbSites().</param>
        /// <returns></returns>
        public static void CheckHiLoPdfSite(string call1,
                                                List<ChanHiLo> chanHiLoList,
                                                bool IsVerbose,
                                                TextWriter tw,
                                                out int sameBandViolationsCount,
                                                out int adjcBandViolationsCount,
                                                out List<ChanHiLoGroup> pdfChanHiLoGroupList)
        {
            //...Log2.v(String.Format("\n\nHiLoAnalysis.HiLoPdfFileSiteCheck(): Entry: PDF Site {0}.", call1));

            // 'out' requirements.
            sameBandViolationsCount = 0;
            adjcBandViolationsCount = 0;
            pdfChanHiLoGroupList = null;

            // If there are no FtChans for this site we have nothing to do so return;
            if (chanHiLoList.Count == 0)
            {
                //...Log2.v(String.Format("\nHiLoAnalysis.HiLoPdfFileSiteCheck(): Exit: TS PDF site {0} has no channels.\n"));
                return; // No violations.
            }

            pdfChanHiLoGroupList = new List<ChanHiLoGroup>();

            // Create a list of NonOrderedPair objects so that we can keep track of adjacent band
            // HiLo violations that have already been reported.
            List<NonOrderedPair> violationPairs = new List<NonOrderedPair>();

            string msg = "";
            bool pdfSiteHasPrimaryBandViolations = false;

            // Create a list of FtChanHiLo objects.
            // Afterwards, these can then be used for the MDB_SITE mode.
            // Initially, the HiLoSensePair value for each individual FtChan is NotSet/NotSet.
            CreateListOfChanHiLoGroups(chanHiLoList, out pdfChanHiLoGroupList);

            // Now calculate the HiLoSensePair value for each individual ChannelHiLo object.
            foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
            {
                foreach (ChanHiLo chanHiLo in chanHiLoGroup.chanHiLoList)
                {
                    SdBand sdBand = DynSdbBand.GetSdBand(chanHiLoGroup.bndcde);

                    chanHiLo.hiLoSensePair = GetHiLoSensePairValue(sdBand, chanHiLo);
                }
            }

            // Calculate the group HiLo state and sense for every channel group.
            // This must be done prior to any subsequent HiLo analysis of 
            // adjacent bands and MDB sites.
            foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
            {
                //===============================================================================================
                // The following call performs the intra-band HiLo analysis of a bndcde group of FtChans.
                //===============================================================================================
                sameBandViolationsCount += GetStateOfChanHiLoGroup_PDF(chanHiLoGroup);

                if (chanHiLoGroup.IsInViolation()) pdfSiteHasPrimaryBandViolations = true;
            }

            // Iterate through the list of channel groups for the current PDF site.
            foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
            {
                // Report the results of the intra-band HiLo analysis.
                // Write a list of chanels for this band group.
                SdBand sdBand = DynSdbBand.GetSdBand(chanHiLoGroup.bndcde);
                msg = String.Format("\n\nBand {0} is used by {1} channel(s); HiLo (Tx/Rx) analysis for this channel group using bmidf = {2} KHz:", chanHiLoGroup.bndcde, chanHiLoGroup.chanHiLoList.Count, sdBand.bmidf);
                tw.Write(Tabize(msg, 1));

                msg = chanHiLoGroup.ToString();
                msg += "\nChannel group: " + chanHiLoGroup.GroupToString();
                tw.Write(Tabize(msg, 2));

                // Report any inter-band HiLo violations.
                if (chanHiLoGroup.IsInViolation())
                {
                    msg = chanHiLoGroup.message;
                    msg = Tabize(msg, 2);
                    tw.Write("\n" + msg);
                }
                // If the site has no primary band violations then proceed with the HiLo analysis of adjacent bands.
                else if (!pdfSiteHasPrimaryBandViolations)
                {

                    msg = String.Format("\nBand {0} OK, checking adjacent bands:", chanHiLoGroup.bndcde);
                    msg = Tabize(msg, 2);
                    tw.Write(msg);

                    //===============================================================================================
                    // The following call performs the HiLo analysis of adjacent bands.
                    //===============================================================================================
                    adjcBandViolationsCount += HiLoCheckOfPdfAdjacentBands_ALT(chanHiLoGroup, pdfChanHiLoGroupList, tw, ref violationPairs);
                }

            } // loop over chanHiLoGroup

            if (pdfSiteHasPrimaryBandViolations)
            {

                msg = String.Format("\n\nFile site {0} has potential primary band HiLo violations: adjacent bands were not checked.", call1);
                msg = Tabize(msg, 1);
                tw.Write(msg);
            }

            //...Log2.v(String.Format("\n\nHiLoAnalysis.HiLoPdfFileSiteCheck(): Exit: PDF Site {0}, Violations Same/Adj = {1}/{2}.", call1, sameBandViolationsCount, adjcBandViolationsCount));
            return;
        }

        /// <summary>
        /// This method performs same-band and adjacent band HiLo analysis for a prescribed list of
        /// PDF file site/channel groups relative to MDB sites/channels that are within a prescribed
        /// coordination distance.
        /// </summary>
        /// <param name="pdfSiteCall1"> - call sign of the prescribed PDF file site.</param>
        /// <param name="mdbChanHiLoListAllSites"> - prescribed list of all ChanHiLo objects for all MDB sites within the coordination distance.</param>
        /// <param name="IsVerbose"> - TRUE if caller wants verbose reporting.</param>
        /// <param name="dDistKm"> - HiLo coordination distance in Km.</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <param name="sameBandViolationsCount"> - number of same band HiLo violations detected.</param>
        /// <param name="adjcBandViolationsCount"> - number of adjacent band HiLo violations detected.</param>
        /// <param name="pdfChanHiLoGroupList"> - a list of all the ChanHiLo objects for the site.</param>
        /// <param name="pdfChanHiLoDeletionsList"> - a list of all the ChanHiLo objects that are marked for deletion in the PDF.</param>
        public static void CheckHiLoMdbSites(string pdfSiteCall1,
                                            List<ChanHiLo> mdbChanHiLoListAllSites,
                                            bool IsVerbose,
                                            double dDistKm,
                                            TextWriter tw,
                                            out int sameBandViolationsCount,
                                            out int adjcBandViolationsCount,
                                            List<ChanHiLoGroup> pdfChanHiLoGroupList,
                                            List<ChanHiLo> pdfChanHiLoDeletionsList)
        {
            //...Log2.v(String.Format("\n\nHiLoAnalysis.HiLoMdbSiteCheck(): Entry: PDF Site {0}.", pdfSiteCall1));

            // 'out' requirements.
            sameBandViolationsCount = 0;
            adjcBandViolationsCount = 0;

            // AH: REMOVE
            // foreach (ChanHiLo chanHiLo in mdbChanHiLoListAllSites) Console.Write("\nLION {0}", chanHiLo.ToStringAll());

            // pdfChanHiLoGroupList should provide the channel groups of PDF file site being analyzed.
            // Check that pdfChanHiLoGroupList is not null and contains something.
            if (pdfChanHiLoGroupList == null)
            {
                Log2.e("\n\nHiLoAnalysis.HiLoMdbSiteCheck(): ERROR: pdfChanHiLoGroupList is NULL.");
                return;
            }
#if true
            // mdbChanHiLoList should provide the list of all channels for all the qualified MDB sites.
            // If there are no channels to be found in mdbChanHiLoList we have nothing to do so return;
            if (mdbChanHiLoListAllSites.Count == 0)
            {
                //...Log2.v(String.Format("\nHiLoAnalysis.HiLoMdbSiteCheck(): Exit: TS MDB channel list is empty.\n"));
                return; // No violations.
            }

            // The mdbChanHiLoListAllSites will include all existing channels cited in the PDF.
            // We should exclude these PDF-channels from the MDB HiLo analysis.

            // Exclude any existing PDF channels whose cmd is "N" or "U" from the list of MDB channels.        
            foreach (ChanHiLoGroup pdfChanHiLoGroup in pdfChanHiLoGroupList)
            {
                mdbChanHiLoListAllSites = ExcludeFromChanHiLoList(mdbChanHiLoListAllSites, pdfChanHiLoGroup.chanHiLoList);
            }

            // Exclude any PDF channels that are marked for deletion.
            mdbChanHiLoListAllSites = ExcludeFromChanHiLoList(mdbChanHiLoListAllSites, pdfChanHiLoDeletionsList);

#endif
            List<NonOrderedPair> violationPairs = new List<NonOrderedPair>();

            string msg = "";
            bool pdfSiteHasPrimaryBandViolationsWithMdb = false;

            // Compute the HiLoSensePair value for each individual MDB ChanHiLo object.
            foreach (ChanHiLo mdbChanHiLo in mdbChanHiLoListAllSites)
            {
                SdBand sdBand = DynSdbBand.GetSdBand(mdbChanHiLo.bndcde);
                mdbChanHiLo.hiLoSensePair = GetHiLoSensePairValue(sdBand, mdbChanHiLo);
            }

            // Create and populate a list of MDB ChanHiLoGroup objects.
            // Note that the channels in a specific bndcde group can be from multiple MDB sites.
            List<ChanHiLoGroup> mdbChanHiLoGroupList = new List<ChanHiLoGroup>();

            foreach (ChanHiLoGroup pdfChanHiLoGroup in pdfChanHiLoGroupList)
            {
                List<ChanHiLo> mdbChanHiLoList = ChanHiLo.GetSubListForBndcde(mdbChanHiLoListAllSites, pdfChanHiLoGroup.bndcde);

                ChanHiLoGroup mdbChanHiLoGroup = new ChanHiLoGroup(pdfChanHiLoGroup.bndcde, mdbChanHiLoList);

                mdbChanHiLoGroup.hiLoGroupState = ChanHiLoGroup.HiLoGroupState.OK;

                // Set the MDB channel group's reference HiLo to that of the pdfChanHiLoGroup.
                mdbChanHiLoGroup.referenceSensePair = pdfChanHiLoGroup.hiLoGroupSensePair;

                mdbChanHiLoGroupList.Add(mdbChanHiLoGroup);
            }

            // Perform HiLo analysis of the PDF site's 'primary' band codes versus nearby MDB sites.
            // Calculate the group HiLo state and sense for every MDB channel group.
            foreach (ChanHiLoGroup mdbChanHiLoGroup in mdbChanHiLoGroupList)
            {
                //================================================================================================
                // The following call performs the primary, intra-band, HiLo analysis of a bndcde group of MtChans.
                //================================================================================================
                sameBandViolationsCount += GetStateOfChanHiLoGroup_MDB(mdbChanHiLoGroup);

                if (mdbChanHiLoGroup.IsInViolation()) pdfSiteHasPrimaryBandViolationsWithMdb = true;
            }

            // Loop through the list of MDB channel groups and report the results of the 
            // primary band analysis. If there are no MDB primary band violations for the
            // whole PDF site then commence the analysis of adjacent MDB bands.
            foreach (ChanHiLoGroup mdbChanHiLoGroup in mdbChanHiLoGroupList)
            {
                // To avoid using excessively long object field specifiers...
                List<ChanHiLo> mdbChanHiLoList = mdbChanHiLoGroup.chanHiLoList;

                msg = String.Format("\n\nBand {0} is used by {1} MDB channels in {2} existing sites:",
                                    mdbChanHiLoGroup.bndcde, mdbChanHiLoList.Count, ChanHiLo.CountSites(mdbChanHiLoList));
                tw.Write(Tabize(msg, 1));

                if (mdbChanHiLoList.Count != 0)
                {
                    int index = 1;
                    foreach (ChanHiLo mdbChanHiLo in mdbChanHiLoList)
                    {
                        string note = "";

                        if (mdbChanHiLo.call1.Equals(pdfSiteCall1))
                        {
                            note = String.Format("  <-- Existing channel of file site {0}.", pdfSiteCall1);
                        }

                        msg = String.Format("\n#{0}. {1}  {2}", index++, mdbChanHiLo.ToString(), note);
                        tw.Write(Tabize(msg, 2));
                    }

                    msg = String.Format("\nFile site {0,9} channel group with bndcde {1,4} has HiLo = {2}", pdfSiteCall1, mdbChanHiLoGroup.bndcde, mdbChanHiLoGroup.referenceSensePair.ToString());
                    tw.Write(Tabize(msg, 2));

                    tw.Write(Tabize(mdbChanHiLoGroup.message, 2));
                }

                if (!pdfSiteHasPrimaryBandViolationsWithMdb)
                {
                    msg = String.Format("\nBand {0} OK ... now checking adjacent MDB channels for HiLo conflict w.r.t. file site's band {0}:", mdbChanHiLoGroup.bndcde);
                    msg = Tabize(msg, 2);
                    tw.Write(msg);

                    //===============================================================================================
                    // The following call performs the HiLo analysis of the MDB adjacent bands.
                    //===============================================================================================
                    adjcBandViolationsCount += HiLoCheckOfMdbAdjacentBands_ALT(pdfSiteCall1, mdbChanHiLoGroup, mdbChanHiLoListAllSites, tw);
                }

            }

            if (pdfSiteHasPrimaryBandViolationsWithMdb)
            {
                msg = String.Format("\n\nFile site {0} has primary band potential HiLo violations with existing MDB channels: adjacent bands were not checked.", pdfSiteCall1);
                msg = Tabize(msg, 1);
                tw.Write(msg);
            }


            //...Log2.v(String.Format("\n\nHiLoAnalysis.HiLoMdbSiteCheck(): Exit: PDF Site {0}, Violations Same/Adj = {1}/{2}.", pdfSiteCall1, sameBandViolationsCount, adjcBandViolationsCount));
            return;
        }

        /// <summary>
        /// This method inputs a list of ChanHiLo objects and outputs a list of ChanHiLoGroup objects; this
        /// method is needed to group together channels with the same bndcde for a prescribed site.
        /// </summary>
        /// <param name="chanHiLoList"> - prescribed list of ChanHiLo objects.</param>
        /// <param name="chanHiLoGroupList"> - returned list of ChanHiLoGroup objects.</param>
        public static void CreateListOfChanHiLoGroups(List<ChanHiLo> chanHiLoList, out List<ChanHiLoGroup> chanHiLoGroupList)
        {
            // 'out' requirement.
            chanHiLoGroupList = new List<ChanHiLoGroup>();

            // Create a list of bndcde values with no duplicates.
            List<string> bndcdeList = new List<string>();

            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                bndcdeList.Add(chanHiLo.bndcde);
            }

            bndcdeList = Strings.RemoveDuplicates(bndcdeList);

            // Create a list of ChannelHiLoGroups.
            foreach (string bndcde in bndcdeList)
            {
                chanHiLoGroupList.Add(new ChanHiLoGroup(bndcde));
            }

            // Fully-populate the list of ChannelHiLoGroups.
            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                foreach (ChanHiLoGroup chanHiLoGroup in chanHiLoGroupList)
                {
                    if (chanHiLoGroup.bndcde == chanHiLo.bndcde)
                    {
                        chanHiLoGroup.chanHiLoList.Add(chanHiLo);
                        break; // Goto next channelHiLo.
                    }
                }
            }

            return;
        }

        /// <summary>
        /// This method creates a string containing the distance and bndcde conditions for a SQL 'WHERE' clause
        /// for a prescribed site and prescribed HiLo analysis distance from this site;
        /// the 'WHERE' clause includes bitwise AND operations applied to the site's
        /// bandwd1, ... bandwd8 bitmap band indicators such that <b> only the bndcde's used
        /// by pSite and all of their adjacent bands are selected</b> in the WHERE clause.
        /// </summary>
        /// <param name="cSQL"> - distance condition for 'WHERE' clause.</param>
        /// <param name="pdfFtSite"> - prescribed FtSite object for a specific PDF file site.</param>
        /// <param name="dDistKm"> - HiLo coordination distance in Km.</param>
        public static void GetCondition(out string cSQL, FtSite pdfFtSite, double dDistKm)
        {
            //...Log2.v("\nHiLoAnalysis.GetCondition(): Entry: call1 = " + pdfFtSite.call1);

            // Satisfy 'out' condition.
            cSQL = "";

            string cBandLogic;

            if (pdfFtSite != null)
            {
                //	Get the band bits with adjacency.
                FtUtils.GenBandClause(pdfFtSite, out cBandLogic, true);

                cSQL = String.Format("tsip.distance_hs({0}, {1}, latit, longit) <= {2:#0.000} and ({3}) ",
                                        pdfFtSite.latit, pdfFtSite.longit, dDistKm, cBandLogic);
            }

            //...Log2.v("\nHiLoAnalysis.GetCondition(): Exit: cSQL = " + cSQL);
            return;
        }

        /// <summary>
        /// This method returns a HiLoSensePair object for a prescribed bndcde, band middle-frequency and Tx/Rx frequencies; if bndcde 
        /// is '7B' then special logic is applied.
        /// </summary>
        /// <param name="bndcde"> - prescribed bndcde.</param>
        /// <param name="chanHiLo"> - prescribed channel whose HiLo sense is to be calculated.</param>
        /// <returns></returns>
        public static HiLoSensePair GetHiLoSensePairValue(string bndcde, ChanHiLo chanHiLo)
        {
            SdBand sdBand = DynSdbBand.GetSdBand(bndcde);

            return GetHiLoSensePairValue(sdBand, chanHiLo);
        }

        /// <summary>
        /// This method returns a HiLoSensePair object for a prescribed SdBand object and ChanHiLo object.
        /// </summary>
        /// <param name="sdBand"> - prescribed SdBand object.</param>
        /// <param name="chanHiLo"> - prescribed ChanHiLo object.</param>
        /// <returns></returns>
        public static HiLoSensePair GetHiLoSensePairValue(SdBand sdBand, ChanHiLo chanHiLo)
        {
            HiLoSense txSense = new HiLoSense();
            HiLoSense rxSense = new HiLoSense();

            if (sdBand.bndcde.Equals("7B"))
            {
                txSense = BandCheck_7B(chanHiLo.freqtx);
                rxSense = BandCheck_7B(chanHiLo.freqrx);
            }
            else  // Band has no sub-bands.
            {
                txSense = BandCheck(sdBand.bmidf, chanHiLo.freqtx);
                rxSense = BandCheck(sdBand.bmidf, chanHiLo.freqrx);
            }

            // Apply additional constraint that the Tx and/or Rx frequencies must actually 
            // lie inside the prescribed band.
            if (!IsInBand(chanHiLo.freqtx, sdBand)) txSense.Value = HiLoSense.Sense.NotSet;
            if (!IsInBand(chanHiLo.freqrx, sdBand)) rxSense.Value = HiLoSense.Sense.NotSet;

            return new HiLoSensePair(txSense, rxSense);
        }

        /// <summary>
        /// This method returns a HiLoSense value for a prescribed band middle-frequency and channel Tx or Rx frequency.
        /// </summary>
        /// <param name="bmidf"> - prescribed band center-frequency.</param>
        /// <param name="freq"> - prescribed frequency.</param>
        /// <returns></returns>
        public static HiLoSense BandCheck(double bmidf, double freq)
        {
            HiLoSense hiLoSense = new HiLoSense();

            if (freq > 0.0)
            {
                hiLoSense.Value = (freq > bmidf) ? HiLoSense.HI : HiLoSense.LO;
            }

            return hiLoSense;
        }

        /// <summary>
        /// This method returns a HiLoSense value for a prescribed Tx or Rx frequency 
        /// for the special case of bndcde = 7B; this band cooresponds to ISED's SRSP
        /// 307-1 ISED that divides the band into two sub-plans designated I and II.
        /// </summary>
        /// <remarks>
        /// <code>
        /// sub-plan I  : comprises two discrete, non-contiguous sub-intervals: 
        ///                      lower:   [7125, 7250] MHz
        ///                      upper    [7300, 7425] MHz.
        ///                      
        /// sub-plan II : is the interval [7425, 7725] Mhz divided into two contiguous 
        ///               sub-intervals (lower and upper) with equal width.
        ///
        /// For either sub-plan the HiLo convention is that if the freqtx is in the upper sub-interval
        /// then the freqrx should be in the lower sub-interval and vice versa.
        /// </code>
        /// </remarks>
        /// <param name="freq"> - prescribed frequency.</param>
        /// <returns></returns>
        private static HiLoSense BandCheck_7B(double freq)
        {
            //...Log2.v(String.Format("\n\nHiLoSenseNew.BandCheck_7B(): Entry: freq = {0}", freq));

            HiLoSense hiLoSense = new HiLoSense();

            // If freq is zero we return HiLoSense.NotSet
            if (freq <= Double.Epsilon)
            {
                return hiLoSense;
            }

            // Define the constant frequencies that define band 7B's special characteristics.
            const double SUB_PLAN_I_LOWER_INTERVAL_FREQ_LO = 7125000;
            const double SUB_PLAN_I_LOWER_INTERVAL_FREQ_HI = 7250000;
            const double SUB_PLAN_I_UPPER_INTERVAL_FREQ_LO = 7300000;
            const double SUB_PLAN_I_UPPER_INTERVAL_FREQ_HI = 7425000;

            const double SUB_PLAN_II_FREQ_LO = 7425000;
            const double SUB_PLAN_II_FREQ_HI = 7725000;
            const double SUB_PLAN_II_MID_FREQ = 7575000;

            // Determine if the freq is actually within band 7B.
            // This is being checked by FtValidate but is repeated here fpr completeness.
            bool IsInBand7B = IsInRange(freq, SUB_PLAN_I_LOWER_INTERVAL_FREQ_LO, SUB_PLAN_II_FREQ_HI);

            // Determine whether sub-plan I applies.
            // This is not checked by FtValidate.
            bool IsInSubPlanILowerInterval = IsInRange(freq, SUB_PLAN_I_LOWER_INTERVAL_FREQ_LO, SUB_PLAN_I_LOWER_INTERVAL_FREQ_HI);
            bool IsInSubPlanIUpperInterval = IsInRange(freq, SUB_PLAN_I_UPPER_INTERVAL_FREQ_LO, SUB_PLAN_I_UPPER_INTERVAL_FREQ_HI);
            bool IsInSubPlanI = IsInSubPlanILowerInterval || IsInSubPlanIUpperInterval;

            // Determine whether sub-plan II applies.
            // This is not checked by FtValidate.
            bool IsInSubPlanIILowerInterval = IsInRange(freq, SUB_PLAN_II_FREQ_LO, SUB_PLAN_II_MID_FREQ);
            bool IsInSubPlanIIUpperInterval = IsInRange(freq, SUB_PLAN_II_MID_FREQ, SUB_PLAN_II_FREQ_HI);
            bool IsInSubPlanII = IsInSubPlanIILowerInterval || IsInSubPlanIIUpperInterval;

            // Handle all the distinct logical cases.
            if (!IsInBand7B)
            {
                Log2.e("\n\nHiLoSenseNew.BandCheck_7B(): ERROR: frequency is > 0 but not in band 7B : " + freq);
            }
            // Check that the freq is not in the forbidden zone of sub-plan I.
            else if (!IsInSubPlanI && !IsInSubPlanII)
            {
                Log2.e("\n\nHiLoSenseNew.BandCheck_7B(): ERROR: frequency is in the forbidden zone of band 7B : " + freq);
            }
            // Handle the case that sub-plan I applies.
            else if (IsInSubPlanI)
            {
                if (IsInSubPlanIUpperInterval)
                {
                    hiLoSense.Value = HiLoSense.HI;
                }
                else if (IsInSubPlanILowerInterval)
                {
                    hiLoSense.Value = HiLoSense.LO;
                }
            }
            // Handle the case that sub-plan II applies.
            else if (IsInSubPlanII)
            {
                if (IsInSubPlanIIUpperInterval)
                {
                    hiLoSense.Value = HiLoSense.HI;
                }
                else if (IsInSubPlanIILowerInterval)
                {
                    hiLoSense.Value = HiLoSense.LO;
                }
            }
            // We should never get here.
            else
            {
                Log2.e("\n\nHiLoSenseNew.BandCheck_7B(): ERROR: frequency not handled by any cases : " + freq);
            }

            //...Log2.v("\nHiLoSenseNew.BandCheck_7B(): Exit; eRetHiLo = " + hiLoSense + "\n");
            return hiLoSense;
        }

        /// <summary>
        /// This method performs the primary, intra-band, HiLo analysis of a prescribed bndcde group of 
        /// MtChan objects w.r.t. the 'reference' HiLo sense pair equal of the file site
        /// channel group with the same bndcde.
        /// </summary>
        /// <param name="mdbChanHiLoGroup"> - a ChanHiLoGroup object for nearby MDB sites.</param>
        /// <returns></returns>
        public static int GetStateOfChanHiLoGroup_MDB(ChanHiLoGroup mdbChanHiLoGroup)
        {
            //...Log2.v(String.Format("\nHiLoAnalysis.GetStateOfChanHiLoGroup_MDB(): Entry: bndcde = {0}", mdbChanHiLoGroup.bndcde));

            mdbChanHiLoGroup.message = String.Format("\nThere are no same-band PDF file <--> MDB violations for bndcde {0}.", mdbChanHiLoGroup.bndcde);

            StringBuilder sb = new StringBuilder();
            string errorStr;
            int index;
            int nViolations = 0;

            // Look for txSense violations over all of the channels in the group.
            HiLoSense referenceTxSense = mdbChanHiLoGroup.referenceSensePair.txSense;
            HiLoSense referenceRxSense = mdbChanHiLoGroup.referenceSensePair.rxSense;

            sb.Clear();

            index = 0;
            foreach (ChanHiLo chanHiLo in mdbChanHiLoGroup.chanHiLoList)
            {
                index++;

                // Check the Tx sense of the MDB channel versus the reference Tx sense.
                if (!chanHiLo.hiLoSensePair.txSense.IsCompatibleWith(referenceTxSense))
                {
                    nViolations++;

                    mdbChanHiLoGroup.SetViolation();

                    errorStr = String.Format("\nTx HiLo potential VIOLATION starting at channel #{0}, above.", index);

                    sb.Append(errorStr);

                    break;
                }
            }

            index = 0;
            foreach (ChanHiLo chanHiLo in mdbChanHiLoGroup.chanHiLoList)
            {
                index++;

                // Check the Rx sense of the MDB channel versus the reference Rx sense.
                if (!chanHiLo.hiLoSensePair.rxSense.IsCompatibleWith(referenceRxSense))
                {
                    nViolations++;

                    mdbChanHiLoGroup.SetViolation();

                    errorStr = String.Format("\nRx HiLo potential VIOLATION starting at channel #{0}, above.", index);

                    sb.Append(errorStr);

                    break;
                }
            }

            if (mdbChanHiLoGroup.IsInViolation())
            {
                mdbChanHiLoGroup.message = sb.ToString();
            }

            //...Log2.v("\nHiLoAnalysis.GetStateOfChanHiLoGroup_MDB(): Exit: retVal = " + retVal);
            return nViolations;
        }

        /// <summary>
        /// This methods performs the intra-band HiLo analysis of a prescribed bndcde group of FtChans.
        /// </summary>
        /// <param name="chanHiLoGroup"> - prescribed bndcde channel group object.</param>
        /// <returns></returns>
        public static int GetStateOfChanHiLoGroup_PDF(ChanHiLoGroup chanHiLoGroup)
        {
            //...Log2.v(String.Format("\nHiLoAnalysis.GetStateOfChanHiLoGroup(): Entry: bndcde = {0}", chanHiLoGroup.bndcde));

            string prolog = String.Format("Band {0} ", chanHiLoGroup.bndcde);

            chanHiLoGroup.message = prolog + "OK.";

            // Initialize group analysis parameters.
            chanHiLoGroup.hiLoGroupSensePair = new HiLoSensePair();
            chanHiLoGroup.hiLoGroupState = ChanHiLoGroup.OK;

            StringBuilder sb = new StringBuilder();
            string errorStr;
            int retVal = 0;

            // First apply the engineering rule that, for an individual channel, 
            // the HiSensePair can't be Lo/Lo or Hi/Hi.
            int index = 0;
            string cr = "";

            foreach (ChanHiLo chanHiLo in chanHiLoGroup.chanHiLoList)
            {
                index++;

                if (chanHiLo.hiLoSensePair.IsLoLo())
                {
                    chanHiLoGroup.SetViolation();

                    errorStr = String.Format("{0}{1}Lo/Lo potential VIOLATION on single channel #{2}, above.", cr, prolog, index);

                    sb.Append(errorStr);

                    cr = "\r\n";

                    retVal++;
                }

                if (chanHiLo.hiLoSensePair.IsHiHi())
                {
                    chanHiLoGroup.SetViolation();

                    errorStr = String.Format("{0}{1}Hi/Hi potential VIOLATION on single channel #{2}, above.", cr, prolog, index);

                    sb.Append(errorStr);

                    cr = "\r\n";

                    retVal++;
                }

            }

            if (chanHiLoGroup.IsInViolation())
            {
                chanHiLoGroup.message = sb.ToString();
                //...Log2.v("\nHiLoAnalysis.GetStateOfChanHiLoGroup(): Exit: retVal = " + retVal);
                return retVal;
            }

            // Next, look for txSense violations over all of the channels in the group.
            HiLoSense accumulatedHiLoSense = new HiLoSense();

            sb.Clear();

            index = 0;
            foreach (ChanHiLo chanHiLo in chanHiLoGroup.chanHiLoList)
            {
                index++;

                if (accumulatedHiLoSense.Value == HiLoSense.NOTSET)
                {
                    if (chanHiLo.hiLoSensePair.txSense.Value != HiLoSense.NOTSET)
                    {
                        accumulatedHiLoSense.Value = chanHiLo.hiLoSensePair.txSense.Value;
                    }
                }
                else
                {
                    if (chanHiLo.hiLoSensePair.txSense.Value != HiLoSense.NOTSET)
                    {
                        if (chanHiLo.hiLoSensePair.txSense.Value != accumulatedHiLoSense.Value)
                        {
                            chanHiLoGroup.SetViolation();

                            errorStr = String.Format("{0}Tx HiLo potential VIOLATION starting at channel #{1}, above.", prolog, index);

                            sb.Append(errorStr);

                            break;
                        }
                    }
                }
            }

            // Store the accumulated group sense for the Tx frequencies.
            chanHiLoGroup.hiLoGroupSensePair.txSense.Value = accumulatedHiLoSense.Value;

            // Look for rxSense violations over all of the channels in the group.
            accumulatedHiLoSense.Value = HiLoSense.NOTSET; ;

            index = 0;
            foreach (ChanHiLo chanHiLo in chanHiLoGroup.chanHiLoList)
            {
                index++;

                if (accumulatedHiLoSense.Value == HiLoSense.NOTSET)
                {
                    if (chanHiLo.hiLoSensePair.rxSense.Value != HiLoSense.NOTSET)
                    {
                        accumulatedHiLoSense.Value = chanHiLo.hiLoSensePair.rxSense.Value;
                    }
                }
                else
                {
                    if (chanHiLo.hiLoSensePair.rxSense.Value != HiLoSense.NOTSET)
                    {
                        if (chanHiLo.hiLoSensePair.rxSense.Value != accumulatedHiLoSense.Value)
                        {
                            chanHiLoGroup.SetViolation();

                            errorStr = String.Format("{0}Rx HiLo potential VIOLATION starting at channel #{1}, above.", prolog, index);

                            sb.Append(errorStr);

                            break;
                        }
                    }
                }
            }

            // Store the accumulated group sense for the Rx frequencies.
            chanHiLoGroup.hiLoGroupSensePair.rxSense.Value = accumulatedHiLoSense.Value;

            if (chanHiLoGroup.IsInViolation())
            {
                chanHiLoGroup.message = sb.ToString();
                retVal = 1;
            }

            //...Log2.v("\nHiLoAnalysis.GetStateOfChanHiLoGroup(): Exit: retVal = " + retVal);
            return retVal;
        }

        /// <summary>
        /// This method performs the HiLo analysis of a prescribed file site channel group (termed the 'primary' group)
        /// w.r.t. the transmit and receive frequencies of channels in bands adjacent to the primary group's bndcde.
        /// </summary>
        /// <param name="primaryGroup"> - a prescribed file site ChanHiLoGroup object.</param>
        /// <param name="pdfChanHiLoGroupList"> a list of all ChanHiLoGroup objects for the file site.</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <param name="violationPairs"> - a list of NonOrderedPair objects used to keep track of adjacent band channel HiLo violations that have already been reported.</param>
        /// <returns></returns>
        public static int HiLoCheckOfPdfAdjacentBands(ChanHiLoGroup primaryGroup, List<ChanHiLoGroup> pdfChanHiLoGroupList, TextWriter tw, ref List<NonOrderedPair> violationPairs)
        {
            //...Log2.v("\n\nHiLoNew.HiLoCheckOfAdjacentBands(): Entry: checking primary bndcde = " + primaryGroup.bndcde);

            // The working of this method is based on the assumption that there are 
            // no same-band violations. Check for this ...
            foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
            {
                if (chanHiLoGroup.IsInViolation())
                {
                    Log2.e("\n\nHiLoNew.HiLoCheckOfAdjacentBands(): ERROR: ftChanHiLoGroupList contains same-band violation(s).");
                    return (Constant.FAILURE);
                }
            }

            int nViolations = 0;
            string msg = "";
            StringBuilder sb = new StringBuilder();

            SdBand sdBand = DynSdbBand.GetSdBand(primaryGroup.bndcde);

            List<SdBand> listOfadjacentBndcdes = DynSdbBand.GetAdjSdBands(primaryGroup.bndcde);

            foreach (SdBand adjSdBand in listOfadjacentBndcdes)
            {
                // Find the FtChanHiLoGroup for the adjacent band.
                // It is possible that it does not find one.
                ChanHiLoGroup adjacentChanHiLoGroup = null;

                foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
                {
                    if (chanHiLoGroup.bndcde == adjSdBand.bndcde)
                    {
                        adjacentChanHiLoGroup = chanHiLoGroup;
                        break;
                    }
                }

                // The .badj list of adjacent bands contains its own bndcde.
                // So we need to exclude it.
                if (!adjSdBand.bndcde.Equals(primaryGroup.bndcde))
                {
                    if (adjacentChanHiLoGroup == null)
                    {
                        // Do nothing.
                        //sb.Append(String.Format("\nAdjacent band {0} is not used by any channels at this site.", adjSdBand.bndcde));
                    }
                    else
                    {
                        sb.Append(String.Format("\nAdjacent band {0} is used by {1} channel(s) at this site.", adjSdBand.bndcde, adjacentChanHiLoGroup.chanHiLoList.Count));

                        // If this adjacent FtChan is not opposite to the primary FtChanHiLoGroup sense
                        // then we have a violation.

                        int listItem = 0;
                        foreach (ChanHiLo adjacentChanHiLo in adjacentChanHiLoGroup.chanHiLoList)
                        {
                            sb.Append(String.Format("\n{0}{1,2}.  {2}", tabs[1], ++listItem, adjacentChanHiLo.ToString()));

                            string fault;
                            if (adjacentChanHiLo.hiLoSensePair.IsNotOppositeTo(primaryGroup.hiLoGroupSensePair, out fault))
                            {
                                //NonOrderedPair nop = new NonOrderedPair(primaryGroup.bndcde, adjSdBand.bndcde);

                                NonOrderedPair nop;

                                bool alreadyWritten = false;

                                foreach (ChanHiLo primaryHiLoChan in primaryGroup.chanHiLoList)
                                {
                                    nop = new NonOrderedPair(primaryHiLoChan.UniqueKey(), adjacentChanHiLo.UniqueKey());

                                    if (nop.IsInList(violationPairs))
                                    {
                                        if (!alreadyWritten)
                                        {
                                            sb.Append(String.Format("    Violation: already reported."));
                                            alreadyWritten = true;
                                        }
                                    }
                                    else
                                    {
                                        violationPairs.Add(nop);

                                        if (!alreadyWritten)
                                        {
                                            nViolations++;
                                            sb.Append(String.Format("    potential VIOLATION: HiLo not opposite to file channel group {0}", primaryGroup.bndcde));
                                            alreadyWritten = true;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                sb.Append("    OK.");
                            }

                        }
                    }
                }
            }

            msg = Tabize(sb.ToString(), 3);
            tw.Write(msg);

            //...Log2.v("\nHiLoNew.HiLoCheckOfAdjacentBands(): Exit: nViolations = " + nViolations);
            return nViolations;
        }

        /// <summary>
        /// This method performs the HiLo analysis of a prescribed file site channel group (termed the 'primary' group)
        /// w.r.t. the transmit and receive frequencies of channels in bands adjacent to the primary group's bndcde.
        /// </summary>
        /// <param name="primaryGroup"> - a prescribed file site ChanHiLoGroup object.</param>
        /// <param name="pdfChanHiLoGroupList"> a list of all ChanHiLoGroup objects for the file site.</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <param name="violationPairs"> - a list of NonOrderedPair objects used to keep track of adjacent band channel HiLo violations that have already been reported.</param>
        /// <returns></returns>
        public static int HiLoCheckOfPdfAdjacentBands_ALT(ChanHiLoGroup primaryGroup, List<ChanHiLoGroup> pdfChanHiLoGroupList, TextWriter tw, ref List<NonOrderedPair> violationPairs)
        {
            //...Log2.v("\n\nHiLoNew.HiLoCheckOfAdjacentBands(): Entry: checking primary bndcde = " + primaryGroup.bndcde);

            // The working of this method is based on the assumption that there are 
            // no same-band violations. Check for this ...
            foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
            {
                if (chanHiLoGroup.IsInViolation())
                {
                    Log2.e("\n\nHiLoNew.HiLoCheckOfAdjacentBands(): ERROR: ftChanHiLoGroupList contains same-band violation(s).");
                    return (Constant.FAILURE);
                }
            }

            int nViolations = 0;
            string msg = "";
            StringBuilder sb = new StringBuilder();

            SdBand sdBand = DynSdbBand.GetSdBand(primaryGroup.bndcde);

            List<SdBand> listOfadjacentBndcdes = DynSdbBand.GetAdjSdBands(primaryGroup.bndcde);

            foreach (SdBand adjSdBand in listOfadjacentBndcdes)
            {
                // Find the FtChanHiLoGroup for the adjacent band.
                // It is possible that it does not find one.
                ChanHiLoGroup adjacentChanHiLoGroup = null;

                foreach (ChanHiLoGroup chanHiLoGroup in pdfChanHiLoGroupList)
                {
                    if (chanHiLoGroup.bndcde == adjSdBand.bndcde)
                    {
                        adjacentChanHiLoGroup = chanHiLoGroup;
                        break;
                    }
                }

                // The .badj list of adjacent bands contains its own bndcde.
                // So we need to exclude it.
                if (!adjSdBand.bndcde.Equals(primaryGroup.bndcde))
                {
                    if (adjacentChanHiLoGroup == null)
                    {
                        // Do nothing.
                        //sb.Append(String.Format("\nAdjacent band {0} is not used by any channels at this site.", adjSdBand.bndcde));
                    }
                    else
                    {
                        sb.Append(String.Format("\nAdjacent band {0} is used by {1} channel(s) at this site.", adjSdBand.bndcde, adjacentChanHiLoGroup.chanHiLoList.Count));

                        // If this adjacent FtChan is not opposite to the primary FtChanHiLoGroup sense
                        // then we have a violation.

                        int listItem = 0;
                        foreach (ChanHiLo adjacentChanHiLo in adjacentChanHiLoGroup.chanHiLoList)
                        {
                            sb.Append(String.Format("\n{0}{1,2}.  {2}", tabs[1], ++listItem, adjacentChanHiLo.ToString()));

                            // The following code detects HiLo violations in the cases where the adjacent band
                            // has a channel whose Tx and Rx frequencies also lie within the primary band.
                            string fault;
                            string layout = "";
                            bool txWithinPrimary = IsInBand(adjacentChanHiLo.freqtx, primaryGroup.bndcde);
                            bool rxWithinPrimary = IsInBand(adjacentChanHiLo.freqrx, primaryGroup.bndcde);
                            if (txWithinPrimary && rxWithinPrimary)
                            {
                                layout = String.Format("Tx and Rx within band {0}. ", primaryGroup.bndcde);
                            }
                            else if (txWithinPrimary)
                            {
                                layout = String.Format("Tx within band {0}. ", primaryGroup.bndcde);
                            }
                            else if (rxWithinPrimary)
                            {
                                layout = String.Format("Rx within band {0}. ", primaryGroup.bndcde);
                            }

                            // Check the adjacentChanHiLo HiLoSense w.r.t. the primary channel group.
                            HiLoSensePair HiLoSensePairWrtPrimary = GetHiLoSensePairValue(primaryGroup.bndcde, adjacentChanHiLo);

                            if (!primaryGroup.hiLoGroupSensePair.IsCompatibleWith(HiLoSensePairWrtPrimary, out fault))
                            {
                                sb.Append(String.Format("{0}. {1}", fault, layout));
                                nViolations++;
                                continue;
                            }

                            // The following code detects HiLo violations in the cases where the adjacent band
                            // has a channel whose Tx and Rx frequencies lie outside the primary band.
                            //if (adjacentChanHiLo.hiLoSensePair.IsNotOppositeTo(primaryGroup.hiLoGroupSensePair, out fault))
                            if (IsAdjacentViolation(primaryGroup.bndcde, primaryGroup.hiLoGroupSensePair, adjacentChanHiLo, out layout, out fault))
                            {
                                //NonOrderedPair nop = new NonOrderedPair(primaryGroup.bndcde, adjSdBand.bndcde);

                                NonOrderedPair nop;

                                bool alreadyWritten = false;

                                foreach (ChanHiLo primaryHiLoChan in primaryGroup.chanHiLoList)
                                {
                                    nop = new NonOrderedPair(primaryHiLoChan.UniqueKey(), adjacentChanHiLo.UniqueKey());

                                    if (nop.IsInList(violationPairs))
                                    {
                                        if (!alreadyWritten)
                                        {
                                            sb.Append(String.Format("     viol.({0}) - already reported. {1}", fault, layout));
                                            alreadyWritten = true;
                                        }
                                    }
                                    else
                                    {
                                        violationPairs.Add(nop);

                                        if (!alreadyWritten)
                                        {
                                            nViolations++;
                                            sb.Append(String.Format(" potential VIOLATION ({0}). {1}", fault, layout));
                                            alreadyWritten = true;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                sb.Append("          OK." + " " + layout);
                            }

                        }
                    }
                }
            }

            msg = Tabize(sb.ToString(), 3);
            tw.Write(msg);

            //...Log2.v("\nHiLoNew.HiLoCheckOfAdjacentBands(): Exit: nViolations = " + nViolations);
            return nViolations;
        }

        /// <summary>
        /// This method performs the HiLo analysis of a prescribed file site channel group (termed the 'primary' group)
        /// w.r.t. the transmit and receive frequencies of channels in nearny MDB sites whose bands are adjacent to the primary group's bndcde.        /// </summary>
        /// <param name="pdfCall1"> - call sign of the prescribed PDF file site.</param>
        /// <param name="mdbPrimaryGroup"> - a prescribed file site ChanHiLoGroup object.</param>
        /// <param name="mdbChanHiLoListAllSites"> - a list of all ChanHiLoGroup objects for MDB sites that are nearby the prescribed file site..</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <returns></returns>
        public static int HiLoCheckOfMdbAdjacentBands(string pdfCall1, ChanHiLoGroup mdbPrimaryGroup, List<ChanHiLo> mdbChanHiLoListAllSites, TextWriter tw)
        {
            //...Log2.v("\n\nHiLoNew.HiLoCheckOfMdbAdjacentBands(): Entry: checking primary bndcde = " + primaryGroup.bndcde);

            int nViolations = 0;
            string msg = "";
            StringBuilder sb = new StringBuilder();

            SdBand sdBand = DynSdbBand.GetSdBand(mdbPrimaryGroup.bndcde);

            List<SdBand> listOfadjacentBndcdes = DynSdbBand.GetAdjSdBands(mdbPrimaryGroup.bndcde);

            foreach (SdBand adjSdBand in listOfadjacentBndcdes)
            {
                // Find the MtChanHiLoGroup for the adjacent band.
                // It is possible that it does not find one.
                //ChanHiLoGroup adjacentChanHiLoGroup = null;

                List<ChanHiLo> mdbChanHiLoListForBndcde = ChanHiLo.GetSubListForBndcde(mdbChanHiLoListAllSites, adjSdBand.bndcde);

                // The .badj list of adjacent bands contains its own bndcde.
                // So we need to exclude it.
                if (!adjSdBand.bndcde.Equals(mdbPrimaryGroup.bndcde))
                {
                    if (mdbChanHiLoListForBndcde.Count != 0)
                    {
                        sb.Append(String.Format("\n\nAdjacent band {0} is used by {1} channel(s) in {2} nearby existing sites:",
                                                adjSdBand.bndcde, mdbChanHiLoListForBndcde.Count, ChanHiLo.CountSites(mdbChanHiLoListForBndcde)));
                    }
                    else
                    {
                        continue; // to next adjacent bndcde.
                    }

                    int listItem = 0;

                    int adjBandTxViolationCount = 0;
                    int adjBandRxViolationCount = 0;

                    foreach (ChanHiLo mdbChanHiLoWithBndcde in mdbChanHiLoListForBndcde)
                    {
                        sb.Append(String.Format("\n{0}{1,2}.   {2}", tabs[1], ++listItem, mdbChanHiLoWithBndcde.ToString()));

                        // If this adjacent MtChan is not opposite to the primary FtChanHiLoGroup sense
                        // then we have a violation.
                        string fault;
                        string rightText = "";

                        if (mdbChanHiLoWithBndcde.hiLoSensePair.IsNotOppositeTo(mdbPrimaryGroup.referenceSensePair, out fault))
                        {
                            //nViolations++;

                            //sb.Append(String.Format("   potential VIOLATION: HiLo not opposite to file site channel group {0}.", mdbPrimaryGroup.bndcde));
                            switch (fault)
                            {
                                case "TX":
                                    if (adjBandTxViolationCount == 0)
                                    {
                                        rightText = "Tx potential VIOLATION w.r.t. file channel group " + mdbPrimaryGroup.bndcde + ".";
                                        adjBandTxViolationCount++;
                                        nViolations++;
                                    }
                                    else
                                    {
                                        rightText = "Tx violation already reported.";
                                    }
                                    break;
                                case "RX":
                                    if (adjBandRxViolationCount == 0)
                                    {
                                        rightText = "Rx potential VIOLATION w.r.t. file channel group " + mdbPrimaryGroup.bndcde + ".";
                                        adjBandRxViolationCount++;
                                        nViolations++;
                                    }
                                    else
                                    {
                                        rightText = "Rx violation already reported.";
                                    }
                                    break;
                                case "TXRX":
                                    if (adjBandTxViolationCount == 0 && adjBandRxViolationCount == 0)
                                    {
                                        rightText = "Tx and Rx potential VIOLATION w.r.t. file channel group " + mdbPrimaryGroup.bndcde + ".";
                                        adjBandTxViolationCount++;
                                        adjBandRxViolationCount++;
                                        nViolations += 2;
                                    }
                                    else
                                    {
                                        rightText = "Tx and Rx violation already reported.";
                                    }
                                    break;
                            }

                            sb.Append("   " + rightText);
                        }
                        else
                        {
                            sb.Append("   OK.");
                        }

                        sb.Append(msg);

                    } // loop over mdbChanHiLoWithBndcde.

                }  // if (!adjSdBand.bndcde.Equals(primaryGroup.bndcde))

            }  // loop over adjSdBand.

            msg = Tabize(sb.ToString(), 3);
            tw.Write(msg);

            //...Log2.v("\nHiLoNew.HiLoCheckOfAdjacentBands(): Exit: nViolations = " + nViolations);
            return nViolations;
        }

        /// <summary>
        /// This method performs the HiLo analysis of a prescribed file site channel group (termed the 'primary' group)
        /// w.r.t. the transmit and receive frequencies of channels in nearny MDB sites whose bands are adjacent to the primary group's bndcde.        /// </summary>
        /// <param name="pdfCall1"> - call sign of the prescribed PDF file site.</param>
        /// <param name="mdbChanHiLoGroup"> - a prescribed MDB site primary ChanHiLoGroup object.</param>
        /// <param name="mdbChanHiLoListAllSites"> - a list of all ChanHiLoGroup objects for MDB sites that are nearby the prescribed file site..</param>
        /// <param name="tw"> - a TextWriter object prescribed by the caller into which report messages will be written.</param>
        /// <returns></returns>
        public static int HiLoCheckOfMdbAdjacentBands_ALT(string pdfCall1, ChanHiLoGroup mdbChanHiLoGroup, List<ChanHiLo> mdbChanHiLoListAllSites, TextWriter tw)
        {
            //...Log2.v("\n\nHiLoNew.HiLoCheckOfMdbAdjacentBands(): Entry: checking primary bndcde = " + primaryGroup.bndcde);

            int nViolations = 0;
            string msg = "";
            StringBuilder sb = new StringBuilder();

            SdBand sdBand = DynSdbBand.GetSdBand(mdbChanHiLoGroup.bndcde);

            List<SdBand> listOfadjacentBndcdes = DynSdbBand.GetAdjSdBands(mdbChanHiLoGroup.bndcde);

            foreach (SdBand adjSdBand in listOfadjacentBndcdes)
            {
                // Find the MtChanHiLoGroup for the adjacent band.
                // It is possible that it does not find one.
                //ChanHiLoGroup adjacentChanHiLoGroup = null;

                List<ChanHiLo> adjBandMdbChanHiLoList = ChanHiLo.GetSubListForBndcde(mdbChanHiLoListAllSites, adjSdBand.bndcde);

                // The .badj list of adjacent bands contains its own bndcde.
                // So we need to exclude it.
                if (!adjSdBand.bndcde.Equals(mdbChanHiLoGroup.bndcde))
                {
                    if (adjBandMdbChanHiLoList.Count != 0)
                    {
                        sb.Append(String.Format("\n\nAdjacent band {0} is used by {1} channel(s) in {2} nearby existing sites:",
                                                adjSdBand.bndcde, adjBandMdbChanHiLoList.Count, ChanHiLo.CountSites(adjBandMdbChanHiLoList)));
                    }
                    else
                    {
                        continue; // to next adjacent bndcde.
                    }

                    int listItem = 0;

                    int adjBandTxViolationCount = 0;
                    int adjBandRxViolationCount = 0;

                    foreach (ChanHiLo adjBandMdbChanHiLo in adjBandMdbChanHiLoList)
                    {
                        sb.Append(String.Format("\n{0}{1,2}.   {2}", tabs[1], ++listItem, adjBandMdbChanHiLo.ToString()));

                        // The following code detects HiLo violations in the cases where the adjacent band
                        // has a channel whose Tx and/or Rx frequencies also lie within the primary band.
                        string fault;
                        string layout = "";

                        bool txWithinPrimary = IsInBand(adjBandMdbChanHiLo.freqtx, mdbChanHiLoGroup.bndcde);
                        bool rxWithinPrimary = IsInBand(adjBandMdbChanHiLo.freqrx, mdbChanHiLoGroup.bndcde);

                        if (txWithinPrimary && rxWithinPrimary)
                        {
                            layout = String.Format("Tx and Rx within band {0}. ", mdbChanHiLoGroup.bndcde);
                        }
                        else if (txWithinPrimary)
                        {
                            layout = String.Format("Tx within band {0}. ", mdbChanHiLoGroup.bndcde);
                        }
                        else if (rxWithinPrimary)
                        {
                            layout = String.Format("Rx within band {0}. ", mdbChanHiLoGroup.bndcde);
                        }

                        // Check the adjacentChanHiLo HiLoSense w.r.t. the primary channel group.
                        HiLoSensePair hiLoSensePairWrtPrimary = GetHiLoSensePairValue(mdbChanHiLoGroup.bndcde, adjBandMdbChanHiLo);

                        if (!mdbChanHiLoGroup.referenceSensePair.IsCompatibleWith(hiLoSensePairWrtPrimary, out fault))
                        {
                            sb.Append(String.Format("{0}. {1}", fault, layout));
                            nViolations++;
                            continue;
                        }

                        // The following code detects HiLo violations in the cases where the adjacent band
                        // has a channel whose Tx and Rx frequencies do not lie within the primary band.
                        fault = "";
                        string rightText = "";

                        //if (adjBandMdbChanHiLo.hiLoSensePair.IsNotOppositeTo(mdbChanHiLoGroup.referenceSensePair, out fault))
                        if (IsAdjacentViolation(mdbChanHiLoGroup.bndcde, mdbChanHiLoGroup.referenceSensePair, adjBandMdbChanHiLo, out layout, out fault))
                        {
                            //nViolations++;

                            //sb.Append(String.Format("   potential VIOLATION: HiLo not opposite to file site channel group {0}.", mdbPrimaryGroup.bndcde));
                            switch (fault)
                            {
                                case "TX":
                                    if (adjBandTxViolationCount == 0)
                                    {
                                        rightText = " potential VIOLATION (Tx).";
                                        adjBandTxViolationCount++;
                                        nViolations++;
                                    }
                                    else
                                    {
                                        rightText = "     viol.(Tx) - already reported.";
                                    }
                                    break;
                                case "RX":
                                    if (adjBandRxViolationCount == 0)
                                    {
                                        rightText = " potential VIOLATION (Rx).";
                                        adjBandRxViolationCount++;
                                        nViolations++;
                                    }
                                    else
                                    {
                                        rightText = "     viol.(Rx) - already reported.";
                                    }
                                    break;
                                case "TXRX":
                                    if (adjBandTxViolationCount == 0 && adjBandRxViolationCount == 0)
                                    {
                                        rightText = " potential VIOLATION (Tx & Rx).";
                                        adjBandTxViolationCount++;
                                        adjBandRxViolationCount++;
                                        nViolations += 2;
                                    }
                                    else
                                    {
                                        rightText = "     viol.(Tx & Rx) - already reported.";
                                    }
                                    break;
                            }

                            sb.Append(rightText + " " + layout);
                        }
                        else
                        {
                            sb.Append("       OK." + " " + layout);
                        }

                        sb.Append(msg);

                    } // loop over mdbChanHiLoWithBndcde.

                }  // if (!adjSdBand.bndcde.Equals(primaryGroup.bndcde))

            }  // loop over adjSdBand.

            msg = Tabize(sb.ToString(), 3);
            tw.Write(msg);

            //...Log2.v("\nHiLoNew.HiLoCheckOfAdjacentBands(): Exit: nViolations = " + nViolations);
            return nViolations;
        }

        /// <summary>
        /// This method returns TRUE is a prescribed 'test' value lies within the closed
        /// interval defined by upper and lower numerical bounds; otherwise negative.
        /// </summary>
        /// <param name="testValue"> - prescribed value to be tested.</param>
        /// <param name="lower"> - prescribed lower bound.</param>
        /// <param name="upper"> - prescribed upper bound</param>
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

        /// <summary>
        /// This method returns true if a prescribed frequency lies within the
        /// frequency range defined for the prescribed band code (inclusive of the
        /// frequency range end points).
        /// </summary>
        /// <param name="testFrequency"></param>
        /// <param name="bndcde"></param>
        /// <returns></returns>
        private static bool IsInBand(double testFrequency, string bndcde)
        {
            bool result = false;

            SdBand sdBand = DynSdbBand.GetSdBand(bndcde);

            if ((testFrequency >= sdBand.blo) && (testFrequency <= sdBand.bhi))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This method returns true if a prescribed frequency lies within the
        /// frequency range defined for the prescribed SdBand object (inclusive of the
        /// frequency range end points).
        /// </summary>
        /// <param name="testFrequency"></param>
        /// <param name="sdBand"></param>
        /// <returns></returns>
        private static bool IsInBand(double testFrequency, SdBand sdBand)
        {
            bool result = false;

            if ((testFrequency >= sdBand.blo) && (testFrequency <= sdBand.bhi))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This method provides an easy way to indent (from the left) a string that may
        /// contain multiple line breaks.
        /// </summary>
        /// <param name="str"> - prescribed string to be indented.</param>
        /// <param name="n"> - prescribed number of indenting 'tabs' to be inserted.</param>
        /// <returns></returns>
        private static string Tabize(string str, int n)
        {
            string result = "";

            result = str.Replace("\n", "\n" + tabs[n]);

            result = tabs[n] + result;

            return result;
        }

        /// <summary>
        /// This method creates and fully populates a string array whose elements are strings
        /// containing only space characters whose string lengths are proportional to their
        /// array element index.
        /// </summary>
        /// <param name="spacesPerTab"> - prescribed number of space characters per 'tab' string.</param>
        /// <param name="numOfTabs"></param>
        /// <returns></returns>
        private static string[] MakeArrayOfTabs(int spacesPerTab, int numOfTabs)
        {
            string[] tabs = new string[numOfTabs];

            for (int i = 0; i < numOfTabs; i++) tabs[i] = new String(' ', spacesPerTab * i);

            return tabs;
        }

        /// <summary>
        /// This method is passed a List<ChanHiLo> and returns a List<ChanHiLo> from which all
        /// ChanHiLo objects that have their 'cmd' member equal to a prescribed string are removed.
        /// </summary>
        /// <param name="chanHiLoList"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static List<ChanHiLo> ExcludeRecordsWithCMD(List<ChanHiLo> chanHiLoList, string cmd)
        {
            List<ChanHiLo> filteredList = new List<ChanHiLo>();

            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                bool toBeKept = true;

                if (chanHiLo.cmd.Equals(cmd))
                {
                    toBeKept = false;
                }

                if (toBeKept) filteredList.Add(chanHiLo);
            }

            return filteredList;
        }

        /// <summary>
        /// This method is passed a List<ChanHiLo> and returns a List<ChanHiLo> that includes only
        /// ChanHiLo objects that have their 'cmd' member equal to a prescribed string.
        /// </summary>
        /// <param name="chanHiLoList"></param>
        /// <param name="cmd"></param>
        /// <returns></returns>
        private static List<ChanHiLo> SelectRecordsWithCMD(List<ChanHiLo> chanHiLoList, string cmd)
        {
            List<ChanHiLo> selectedList = new List<ChanHiLo>();

            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                bool toBeKept = false;

                if (chanHiLo.cmd.Equals(cmd))
                {
                    toBeKept = true;
                }

                if (toBeKept) selectedList.Add(chanHiLo);
            }

            return selectedList;
        }

        /// <summary>
        /// Remove chanHiLo objects from a list.
        /// </summary>
        /// <param name="chanHiLoList"></param>
        /// <param name="RemoveChanHiLoList"></param>
        /// <returns></returns>
        private static List<ChanHiLo> ExcludeFromChanHiLoList(List<ChanHiLo> chanHiLoList, List<ChanHiLo> RemoveChanHiLoList)
        {
            List<ChanHiLo> remainingChanHiLoList = new List<ChanHiLo>();

            // Create the list of unique keys to be removed.
            List<string> removalsUniqueKeyList = new List<string>();

            foreach (ChanHiLo removeChanHiLo in RemoveChanHiLoList)
            {
                removalsUniqueKeyList.Add(removeChanHiLo.UniqueKey());
            }

            // Perform the removal.
            foreach (ChanHiLo chanHiLo in chanHiLoList)
            {
                if (Strings.IsInList(removalsUniqueKeyList, chanHiLo.UniqueKey()))
                {
                    // Don't add chanHiLo to the returned list.
                }
                else
                {
                    remainingChanHiLoList.Add(chanHiLo);
                }
            }

            return remainingChanHiLoList;
        }

        /// <summary>
        /// This method return true if a prescribed adjacent ChanHiLo object is in violation 
        /// with a prescribed reference HiLoSensePair object, otherwise false; the method also outputs
        /// strings that describe the details of the adjacent channel violation.
        /// </summary>
        /// <param name="primaryBndcde"></param>
        /// <param name="refHiLoSensePair"></param>
        /// <param name="adjChanHiLo"></param>
        /// <param name="layout"></param>
        /// <param name="fault"></param>
        /// <returns></returns>
        public static bool IsAdjacentViolation(string primaryBndcde, HiLoSensePair refHiLoSensePair, ChanHiLo adjChanHiLo, out string layout, out string fault)
        {
            // 'out' requirement.
            fault = "";
            layout = "";

            bool isAdjacentViolation = false;

            bool txIsAbove = false;
            bool txInPband = false;
            bool txIsBelow = false;
            bool rxIsAbove = false;
            bool rxInPband = false;
            bool rxIsBelow = false;

            string txFault = "";
            string rxFault = "";

            bool hasTx = adjChanHiLo.freqtx > 0.0;
            bool hasRx = adjChanHiLo.freqrx > 0.0;

            SdBand primarySdBand = DynSdbBand.GetSdBand(primaryBndcde);

            if (hasTx) txInPband = IsInBand(adjChanHiLo.freqtx, primarySdBand);
            if (hasRx) rxInPband = IsInBand(adjChanHiLo.freqrx, primarySdBand);

            // Adjacent channel has both Tx and Rx frequencies.
            if (hasTx && hasRx)
            {
                // The adjacent channel is above the primarySdBand.
                if ((adjChanHiLo.freqtx > primarySdBand.bhi) && (adjChanHiLo.freqrx > primarySdBand.bhi))
                {
                    txIsAbove = true;
                    rxIsAbove = true;
                    if (refHiLoSensePair.txSense.IsHi() && adjChanHiLo.hiLoSensePair.rxSense.IsLo()) { isAdjacentViolation = true; rxFault = "RX"; }
                    if (refHiLoSensePair.rxSense.IsHi() && adjChanHiLo.hiLoSensePair.txSense.IsLo()) { isAdjacentViolation = true; txFault = "TX"; }
                }
                // The adjacent channel is below the primarySdBand.
                else if ((adjChanHiLo.freqtx < primarySdBand.blo) && (adjChanHiLo.freqrx < primarySdBand.blo))
                {
                    txIsBelow = true;
                    rxIsBelow = true;
                    if (refHiLoSensePair.txSense.IsLo() && adjChanHiLo.hiLoSensePair.rxSense.IsHi()) { isAdjacentViolation = true; rxFault = "RX"; }
                    if (refHiLoSensePair.rxSense.IsLo() && adjChanHiLo.hiLoSensePair.txSense.IsHi()) { isAdjacentViolation = true; txFault = "TX"; }
                }
            }
            // Adjacent channel only has Tx frequency.
            else if (hasTx && !hasRx)
            {
                // The adjacent channel is above the primarySdBand.
                if (adjChanHiLo.freqtx > primarySdBand.bhi)
                {
                    txIsAbove = true;
                    if (refHiLoSensePair.rxSense.IsHi() && adjChanHiLo.hiLoSensePair.txSense.IsLo()) { isAdjacentViolation = true; txFault = "TX"; }
                }
                // The adjacent channel is below the primarySdBand.
                else if (adjChanHiLo.freqtx < primarySdBand.blo)
                {
                    txIsBelow = true;
                    if (refHiLoSensePair.rxSense.IsLo() && adjChanHiLo.hiLoSensePair.txSense.IsHi()) { isAdjacentViolation = true; txFault = "TX"; }
                }
            }
            // Adjacent channel only has Rx frequency.
            else if (!hasTx && hasRx)
            {
                // The adjacent channel is above the primarySdBand.
                if (adjChanHiLo.freqrx > primarySdBand.bhi)
                {
                    rxIsAbove = true;
                    if (refHiLoSensePair.txSense.IsHi() && adjChanHiLo.hiLoSensePair.rxSense.IsLo()) { isAdjacentViolation = true; rxFault = "RX"; }
                }
                // The adjacent channel is below the primarySdBand.
                else if (adjChanHiLo.freqrx < primarySdBand.blo)
                {
                    rxIsBelow = true;
                    if (refHiLoSensePair.txSense.IsLo() && adjChanHiLo.hiLoSensePair.rxSense.IsHi()) { isAdjacentViolation = true; rxFault = "RX"; }
                }
            }

            if (txIsAbove && rxIsAbove) layout = "Tx above / Rx above band " + primaryBndcde;
            else if (txIsBelow && rxIsBelow) layout = "Tx below / Rx below band " + primaryBndcde;
            else if (txIsAbove && rxIsBelow) layout = "Tx above / Rx below band " + primaryBndcde;
            else if (txIsBelow && rxIsAbove) layout = "Tx below / Rx above band " + primaryBndcde;
            else if (txIsAbove && !hasRx) layout = "Tx above band " + primaryBndcde;
            else if (txIsBelow && !hasRx) layout = "Tx below band " + primaryBndcde;
            else if (!hasTx && rxIsAbove) layout = "Rx above band " + primaryBndcde;
            else if (!hasTx && rxIsBelow) layout = "Rx below band " + primaryBndcde;
            else if (txInPband && rxInPband) layout = "Tx within / Rx within " + primaryBndcde;
            else if (txInPband && !hasRx) layout = "Tx within " + primaryBndcde;
            else if (!hasTx && rxInPband) layout = "Rx within " + primaryBndcde;
            else if (txInPband && rxIsAbove) layout = "Tx within / Rx above " + primaryBndcde;
            else if (txInPband && rxIsBelow) layout = "Tx within / Rx below " + primaryBndcde;
            else if (txIsAbove && rxInPband) layout = "Tx above / Rx within " + primaryBndcde;
            else if (txIsBelow && rxInPband) layout = "Tx below / Rx within " + primaryBndcde;

            layout += ".";

            fault = txFault + rxFault;

            //Console.Write("\nFOX: {0}  {1}  {2}  {3}  {4}  {5}  {6}", primaryBndcde, refHiLoSensePair, adjChanHiLo, isAbove, isBelow, isAdjacentViolation, fault);

            return isAdjacentViolation;
        }

    }
}

```
