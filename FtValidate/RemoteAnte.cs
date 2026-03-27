using _DataStructures;

using _NewLib;
using System;
using _Utillib;

namespace FtValidate
{
    /// <summary>
    /// Provides several methods that relate to remote antennae.
    /// </summary>
    public class RemoteAnte
    {

#if PINVOKE
        [DllImport("FtValidate.dll", CharSet = CharSet.Ansi)]
        private extern static double getHighestRemMainAntenna([In] string pdfName, [In, Out] FtAnte ftAnte);
#endif
        //-------------------------------------------------------------------------------
        private const int eAnt_sz = 10;
        //-------------------------------------------------------------------------------
#if PINVOKE
        public static double GetHighestRemMainAntenna_NATIVE(string pdfName, ref FtAnte ftAnte)
        {
            return getHighestRemMainAntenna(pdfName, ftAnte);
        }
#endif
        /// <summary>
        /// Given a site antenna structure, return the height of the highest 
        /// main antenna at the remote end of the link.
        /// </summary>
        /// <param name="pdfName">- name of the PDF file.</param>
        /// <param name="ftAnte">- antenna object.</param>
        /// <returns>Height of the highest main antenna at the remote end of the link.
        /// </returns>
        public static double GetHighestRemMainAntenna(string pdfName, ref FtAnte ftAnte)
        {
            //...Log2.v("\n\nRemoteAnte.GetHighestRemMainAntenna(): Entry");

            //&&Console.Error.Write("\nGetHighestRemMainAntenna(): Entry: " + ftAnte.KeysToString());

            MtSiteStr ptMFarSite;
            MtSiteStr ptMThisSite;
            FtSiteStr ptFFarSite;
            FtSiteStr ptFThisSite;
            int nRet;
            int nRetM;
            int nRetF;
            int nRetMHere;
            int nRetFHere;
            int nInd;
            double dHeight = -1.0;

            //char            (*aChids)[][L_CHID];
            string[] aChids;
            int nChids = 0;
            TantKey[] tAnts = Arrays.CreateArrayUsingDefaultElementConstructor<TantKey>(eAnt_sz);
            int nAntkeys = 0;

            /*	Get this site from the pdf, the other end from both the mdb and the pdf. */
            nRetFHere = FtUtils.FtGetSite(ftAnte.call1, out ptFThisSite, 3, pdfName);
            nRetMHere = MtUtils.MtGetSite(ftAnte.call1, out ptMThisSite, 3);

            nRetM = MtUtils.MtGetSite(ftAnte.call2, out ptMFarSite, 3); /* Get channels just in case. */
            nRetF = FtUtils.FtGetSite(ftAnte.call2, out ptFFarSite, 3, pdfName);

            /*	First, get the chids for this antenna from the database. */
            nRet = GetMDBChids(ftAnte, ptMThisSite, out aChids, out nChids);

            //...Log2.v("\r\nRemoteAnte.GetHighestRemMainAntenna(): After GetMDBChids(): " + Arrays.ToString<string>(aChids, "aChids"));

            /*	Then merge in the chids for this antenna from this pdf. */
            nRet = FtMergeChids(ftAnte, ref aChids, ref nChids, ptFThisSite);

            //...Log2.v("\r\nRemoteAnte.GetHighestRemMainAntenna(): After FtMergeChids(): " + Arrays.ToString<string>(aChids, "aChids"));

            /*	Get the antennas at the other end in the database. */
            nRet = GetMDBAntsFromChids(ftAnte.call1, ftAnte.bndcde, ftAnte.ause,
                                                                 aChids, nChids, ptMFarSite, ref tAnts, out nAntkeys);

            //&&Console.Error.Write("\nH-1: nAntkeys = {0}", nAntkeys);

            //...Log2.v("\r\nRemoteAnte.GetHighestRemMainAntenna(): A: nAntkeys = " + nAntkeys);

            /*	Merge the pdf antennas with the mdb antennas selected. */
            nRet = FtMergeAnts(ptFFarSite, ftAnte, aChids, nChids, ref tAnts, ref nAntkeys);

            //...Log2.v("\r\nRemoteAnte.GetHighestRemMainAntenna(): After FtMergeAnts(): " + Arrays.ToString<string>(aChids, "aChids"));

            //...Log2.v("\r\nRemoteAnte.GetHighestRemMainAntenna(): B: nAntkeys = " + nAntkeys);

            /*	At this point tAnts non-null elements contain the antennas at the far end. */

            //&&Console.Error.Write("\nH-2: nAntkeys = {0}", nAntkeys);

            for (nInd = 0; nInd < nAntkeys; nInd++)
            {
                //&&Console.Error.Write("\nF: tAnts[{0}]: {1}", nInd, tAnts[nInd].ToString());

                //...Log2.v("\r\nRemoteAnte.GetHighestRemMainAntenna(): nInd = " + nInd);
                if (tAnts[nInd].aht > 0.0)
                {
                    /*	It has not been removed. */
                    if (tAnts[nInd].aht > dHeight)
                    {
                        dHeight = tAnts[nInd].aht;
                        //&&Console.Error.Write("\nG: dHeight = {0:F6}", dHeight);
                    }
                }
            }

            //...Log2.v("\n\nRemoteAnte.GetHighestRemMainAntenna(): Exit, dHeight = " + dHeight);
            return dHeight;
        }

        /// <summary>
        /// Get an array of chids of the channels that use the specified antenna for anything.
        /// </summary>
        /// <param name="ftAnte"></param>
        /// <param name="ptThisSite"></param>
        /// <param name="aChids"></param>
        /// <param name="nChids"></param>
        /// <returns></returns>
        private static int GetMDBChids(FtAnte ftAnte,
                                MtSiteStr ptThisSite,
                                out string[] aChids,
                                out int nChids)
        {
            //...Log2.v("\n\nRemoteAnte.GetMDBChids(): Entry");

            //Satisfy 'out' requirements.
            aChids = null;
            nChids = 0;

            int nInd;
            int nNext;

            /*	Count the number of channels that use this */
            if (ptThisSite != null)
            {
                //...Log2.v("\r\nRemoteAnte.GetMDBChids(): ptThisSite != null");

                nChids = MtUtils.MtChansInAnte1(ftAnte.call2, ftAnte.bndcde, ftAnte.anum,
                                                                 ptThisSite.stChanPtr, ptThisSite.nNumChans);
                /*	We add one so that we don't run the risk of malloc returning NULL */
                //aChids = (char(*)[][L_CHID]) malloc((*nChids + 1) * Constant.L_CHID);
                //aChids = Arrays.CreateFillStringArray(nChids + 1, "");

                if (nChids > 0)
                {
                    aChids = new string[nChids];
                }

                /*	Now go through those channels and load in the chids */
                nInd = 0;
                nNext = MtUtils.MtNextChan(ptThisSite.stChanPtr, ptThisSite.nNumChans,
                                                     ftAnte.call2, ftAnte.bndcde, ftAnte.anum,
                                                     -1); /* Start off the scan */

                //...Log2.v("\r\nRemoteAnte.GetMDBChids(): nNext = " + nNext);

                while (nNext >= 0)
                {

                    aChids[nInd] = ptThisSite.stChanPtr[nNext].chid;
                    //...Log2.v("\r\nRemoteAnte.GetMDBChids(): aChids[" + nInd + "] = " + aChids[nInd]);
                    nInd++;

                    nNext = MtUtils.MtNextChan(ptThisSite.stChanPtr, ptThisSite.nNumChans,
                                                         ftAnte.call2, ftAnte.bndcde, ftAnte.anum,
                                                         nNext); /*	Scan for the next one */
                }
            }
            else
            {
                //...Log2.v("\r\nRemoteAnte.GetMDBChids(): ptThisSite is null");
            }

            //...Log2.v("\n\nRemoteAnte.GetMDBChids(): Exit");
            return nChids; /*	Return code is the number found. */
        }

        /// <summary>
        /// Merge the chids from a list of chids with the chids from a pdf for a given antenna.
        /// </summary>
        /// <param name="ftAnte"></param>
        /// <param name="aChids"></param>
        /// <param name="nChids"></param>
        /// <param name="ptFSite"></param>
        /// <returns></returns>
        private static int FtMergeChids(FtAnte ftAnte, ref string[] aChids, ref int nChids, FtSiteStr ptFSite)
        {
            //...Log2.v("\n\nRemoteAnte.FtMergeChids(): Entry: nChids = " + nChids);

            int nNext;
            int nChidInd;

            if (ptFSite != null)
            {
                //...Log2.v("\r\nRemoteAnte.FtMergeChids(): A");
                /*	Start the pdf channels */
                nNext = FtUtils.FtNextChan(ptFSite.stChanPtr, ptFSite.nNumChans,   /*	Size of array  */
                                            ftAnte.call2, ftAnte.bndcde, ftAnte.anum, -1);
                while (nNext >= 0)
                {
                    //...Log2.v("\r\nRemoteAnte.FtMergeChids(): nNext = " + nNext);
                    /*	Got one.  Find the chid in the list if it is there. */
                    if ((nChidInd = ChIdScan(aChids, nChids, ptFSite.stChanPtr[nNext].chid)) >= 0)
                    {
                        //...Log2.v("\r\nRemoteAnte.FtMergeChids(): We have a chid in the list");
                        /*	We have a chid in the list.  Just check to see if the channel is
                        *		being deleted. */
                        if (ptFSite.stChanPtr[nNext].cmd.Equals("D"))
                        {
                            /*	It is being deleted.  Set the chid to the empty string. */
                            aChids[nChidInd] = "";
                        }
                    }
                    else
                    {
                        /*	Channel is not in the list.  Add the chid at the end. */
                        //...Log2.v("\r\nRemoteAnte.FtMergeChids(): " + Arrays.ToString<string>(aChids, "aChids"));
                        //...Log2.v("\r\nRemoteAnte.FtMergeChids(): ptFSite.stChanPtr[nNext].chid = " + ptFSite.stChanPtr[nNext].chid);
                        if (aChids == null)
                        {
                            aChids = new string[1];
                            aChids[0] = ptFSite.stChanPtr[nNext].chid;
                            nChids = 1;
                        }
                        else
                        {
                            Arrays.AppendElement(ref aChids, ptFSite.stChanPtr[nNext].chid);
                            nChids++;
                        }
                    }

                    nNext = FtUtils.FtNextChan(ptFSite.stChanPtr, ptFSite.nNumChans,   /* Size of array */
                                                ftAnte.call2, ftAnte.bndcde, ftAnte.anum, nNext);
                }
            }

            //...Log2.v("\n\nRemoteAnte.FtMergeChids(): Exit: nChids = " + nChids);
            return 0;
        }

        /// <summary>
        /// This method searches an array of chids for a given chid. It returns
        /// the index of the chid found or -1 if not found.
        /// </summary>
        /// <param name="aChids"></param>
        /// <param name="nChids"></param>
        /// <param name="cChid"></param>
        /// <returns></returns>
        private static int ChIdScan(string[] aChids, int nChids, string cChid)
        {
            //...Log2.v("\n\nRemoteAnte.ChIdScan(): Entry:");
            //...Log2.v("\r\nRemoteAnte.ChIdScan(): " + Arrays.ToString(aChids, "aChids"));

            for (int nInd = 0; nInd < nChids; nInd++)
            {
                // It is possible that the aChids array element is null.
                if (aChids[nInd] != null)
                {
                    if (aChids[nInd].Equals(cChid))
                    {
                        //...Log2.v("\r\nRemoteAnte.ChIdScan(): returned " + nInd);
                        return nInd;
                    }
                }
            }

            //...Log2.v("\n\nRemoteAnte.ChIdScan(): Exit: returned -1");
            return -1;
        }

        /// <summary>
        /// Find the remote mdb antennas referred to by this list of chids and merge them with the 
        /// remote antennas in the pdf that refer to this same list.
        /// </summary>
        /// <param name="ptFSite">- Other end site.</param>
        /// <param name="ftAnte">- This end antenna.</param>
        /// <param name="aChids"></param>
        /// <param name="nChids"></param>
        /// <param name="tAnts"></param>
        /// <param name="nAntkeys"></param>
        /// <returns></returns>
        private static int FtMergeAnts(FtSiteStr ptFSite,    /*	Other end site  */
                                             FtAnte ftAnte,        /*	This end antenna  */
                                             string[] aChids, int nChids, ref TantKey[] tAnts, ref int nAntkeys)
        {
            //...Log2.v("\n\nRemoteAnte.FtMergeAnts(): Entry, nChids = " + nChids);

            int nInd;
            int nChan;

            /*	Go through the antennas in the pdf, merging them with the other end.
            *		They will either be modified or deleted if found, and added if not
            *		found.  */
            if (ptFSite != null)
            {
                //...Log2.v("\r\nRemoteAnte.FtMergeAnts(): A");
                /*	Go through the chids and find all the antennas pointed to. */
                for (nInd = 0; nInd < nChids; nInd++)
                {
                    //...Log2.v("\r\nRemoteAnte.FtMergeAnts(): nInd = " + nInd);
                    nChan = FtUtils.FtFindChanChid(ptFSite, ftAnte.call1, ftAnte.bndcde, aChids[nInd]);
                    if (nChan >= 0)
                    {
                        //...Log2.v("\r\nRemoteAnte.FtMergeAnts(): B");

                        if (ptFSite.stChanPtr[nChan].cmd.Equals("D"))
                        {
                            //...Log2.v("\r\nRemoteAnte.FtMergeAnts(): C");
                            continue;       /*	Skip deleted channels. */
                        }

                        //...Log2.v("\r\nRemoteAnte.FtMergeAnts(): D");
                        /*	Found the channel in the pdf.  Add its antennas if necessary */
                        FtAddAnt(ref tAnts, ref nAntkeys, eAnt_sz, ptFSite.stChanPtr[nChan], ftAnte.ause, ptFSite);
                    }
                }
            }

            //...Log2.v("\n\nRemoteAnte.FtMergeAnts(): Exit");
            return 0;
        }

        /// <summary>
        /// Get the antennas in the mdb that have a channel referring to them from the list of chids.
        /// </summary>
        /// <param name="cCall2"></param>
        /// <param name="cBand"></param>
        /// <param name="cFarAuse"></param>
        /// <param name="aChids"></param>
        /// <param name="nChids"></param>
        /// <param name="ptMSite"></param>
        /// <param name="tAnts"></param>
        /// <param name="nAntKeys"></param>
        /// <returns></returns>
        private static int GetMDBAntsFromChids(string cCall2, string cBand, string cFarAuse, string[] aChids,
                                                int nChids, MtSiteStr ptMSite, ref TantKey[] tAnts, out int nAntKeys)
        {
            //...Log2.v("\n\nRemote.Ante.GetMDBAntsFromChids(): Entry");

            //&&Console.Error.Write("\nGetMDBAntsFromChids(): Entry: cCall2 = {0}; cBand = {1}; cFarAuse = {2}; nChids = {3}", cCall2, cBand, cFarAuse, nChids);

            for (int j = 0; j < nChids; j++)
            {
                //&&Console.Error.Write("\n        aChids[{0}] = {1}", j, aChids[j]);
            }

            int nInd;
            int nChan;
            int nRet;

            nAntKeys = 0;

            if (ptMSite != null)
            {
                for (nInd = 0; nInd < nChids; nInd++)
                {
                    if (String.IsNullOrWhiteSpace(aChids[nInd]))
                    {
                        /*	Deleted chid */
                        continue;
                    }
                    nChan = MtUtils.FindmtChid(ptMSite, cCall2, cBand, aChids[nInd]);

                    //&&Console.Error.Write("\nJ: nInd = {0};  nChids = {1}; aChids[nInd] = {2};  nChan = {3}", nInd, nChids, aChids[nInd], nChan);

                    if (nChan >= 0)
                    {
                        /*	Found it.  Now add all the antennas it refers to. */
                        nRet = MtAddAnt(ref tAnts, ref nAntKeys, eAnt_sz, ptMSite.stChanPtr[nChan],
                                                        cFarAuse, ptMSite);

                        //&&Console.Error.Write("\nK: nAntKeys = {0}", nAntKeys);
                    }
                }
            }

            //...Log2.v("\n\nRemote.Ante.GetMDBAntsFromChids(): Exit");
            return 0;
        }

        /// <summary>
        /// Given a channel structure from the mdb, add all the antennas referred to by the channel.
        /// </summary>
        /// <remarks>
        /// <b>14-Aug-2019</b>: The C# code for this method has been changed to exactly replicate the
        /// legacy C++ subroutine mtAddAnt(); see the comments in the C# code for specific details. 
        /// The result of the change is that antenae whose cFarAuse is "TR", "TX" or "RX" will not be
        /// added to the TantKey[]. This seems strange. This method should be re-examined to determine 
        /// its algorithmic correctness.
        /// </remarks>
        /// <param name="tAnts"></param>
        /// <param name="nAntkeys"></param>
        /// <param name="nMax"></param>
        /// <param name="pChan"></param>
        /// <param name="cFarAuse"></param>
        /// <param name="ptMSite"></param>
        /// <returns></returns>
        private static int MtAddAnt(ref TantKey[] tAnts, ref int nAntkeys, int nMax, MtChan pChan, string cFarAuse, MtSiteStr ptMSite)
        {
            //...Log2.v("\n\nRemoteAnte.MtAddAnt(): Entry");

            //&&Console.Error.Write("\nmtAddAnt(): entry: cFarAuse = |{0}|; MtChan: {1}", cFarAuse, pChan.KeysToString());
#if true
            // New code, as of 14-Aug-19, that replicates the error in the legacy C++ code.
            // Observe the trailing space in "TR ", "TX " and "RX "; this must be a bug?!
            // The effect of this bug is that neither of the two 'if' statements are ever triggered
            // for ause codes of "TR", "TX" or "RX";
            // consequently antennae in the MDB table main.mt_ante that are assigned to the prescribed
            // channel fail to be added to the TantKey[] (the list of all antennae assigned to the channel).
            // Also, "ST1" is incorrect: it should be "STX".

            if (cFarAuse.Trim().Equals("TR ") || cFarAuse.Trim().Equals("TX ") || cFarAuse.Equals("ST1"))
            {
                //&&Console.Error.Write("\nmtAddAnt(): A");

                /*	Add the rx antennas */
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx1, ptMSite);
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx2, ptMSite);
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx3, ptMSite);
            }

            if (cFarAuse.Trim().Equals("TR ") || cFarAuse.Equals("RX ") || cFarAuse.Equals("DV1") || cFarAuse.Equals("DV2"))
            {
                /*	Add the tx. */

                //&&Console.Error.Write("\nmtAddAnt(): B");

                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbtx1, ptMSite);
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbtx2, ptMSite);
            }
#endif

#if false
            // The code below is the original code, prior to 13-Aug-19.

            //if (cFarAuse.Equals("TR ") || cFarAuse.Equals("TX ") || cFarAuse.Equals("ST1"))
            if (cFarAuse.Trim().Equals("TR") || cFarAuse.Trim().Equals("TX") || cFarAuse.Equals("ST1"))
            {
                //&&Console.Error.Write("\nmtAddAnt(): A");

                /*	Add the rx antennas */
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx1, ptMSite);
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx2, ptMSite);
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx3, ptMSite);
            }
            if (cFarAuse.Trim().Equals("TR") || cFarAuse.Equals("RX") || cFarAuse.Equals("DV1") || cFarAuse.Equals("DV2"))
            {
                /*	Add the tx. */

                //&&Console.Error.Write("\nmtAddAnt(): B");

                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbtx1, ptMSite);
                MtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbtx2, ptMSite);
            }
#endif
            //...Log2.v("\n\nRemoteAnte.MtAddAnt(): Exit");
            return 0;
        }

        /// <summary>
        /// If an antenna number is non-zero, try to find it in the array; if it is not found, 
        /// add it to the array.
        /// </summary>
        /// <param name="tAnts"></param>
        /// <param name="nAntKeys"></param>
        /// <param name="nMax"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <param name="ptSite"></param>
        /// <returns></returns>
        private static int MtAddAnt1(ref TantKey[] tAnts, ref int nAntKeys, int nMax, string call1, string call2,
                                            string bndcde, int anum, MtSiteStr ptSite)
        {
            //...Log2.v("\n\nRemoteAnte.MtAddAnt1(): Entry");

            int nInd;
            int nAnt;

            //&&Console.Error.Write("\nmtAddAnt1(): entry: anum = {0}", anum);

            if (anum != 0)
            {
                nAnt = MtUtils.MtFindAnte(ptSite, call2, bndcde, anum);

                /*	Find it in the list. */
                for (nInd = 0; nInd < nAntKeys; nInd++)
                {
                    if (tAnts[nInd].anum == anum)
                    {
                        tAnts[nInd].aht = ptSite.stAntsPtr[nAnt].aht;
                        break;
                    }
                }

                /*	If we didn't find it, add it. */
                if (nInd >= nAntKeys)
                {
                    //...Log2.v("\r\nRemoteAnte.MtAddAnt1(): key not found");
                    if (nAnt >= 0)
                    {
                        if (nAntKeys >= nMax)
                        {
                            /*	Overflowed the array */
                            GenUtil.SetError(1100, "\r\nmtAddAnt1 - Too many antennas.");
                            ValErrs.AddMess("mtAddAnt1 - Too many antennas.",
                                            ValErrs.MakeKeyLine(call1, call2, bndcde, anum, ""), "E");
                            return -1;
                        }

                        tAnts[nAntKeys] = new TantKey();

                        tAnts[nAntKeys].call1 = call1;
                        tAnts[nAntKeys].call2 = call2;
                        tAnts[nAntKeys].bndcde = bndcde;
                        tAnts[nAntKeys].anum = anum;
                        tAnts[nAntKeys].aht = ptSite.stAntsPtr[nAnt].aht;

                        //&&Console.Error.Write("\nMtAddAnt1(): ADDED: " + tAnts[nAntKeys].ToString());

                        nAntKeys++;

                        //...Log2.v("\r\nRemoteAnte.MtAddAnt1(): key added, nAntKeys = " + nAntKeys);
                    }
                }
            }
            //...Log2.v("\n\nRemoteAnte.MtAddAnt1(): Exit");
            return 0;
        }

        /// <summary>
        /// Given a channel structure from the pdf, add all the antennas referred to by the channel, 
        /// making sure they are compatible with the antenna use at the other end.
        /// </summary>
        /// <param name="tAnts"></param>
        /// <param name="nAntkeys"></param>
        /// <param name="nMax"></param>
        /// <param name="pChan"></param>
        /// <param name="cFarAuse"></param>
        /// <param name="ptFSite"></param>
        /// <returns></returns>
        private static int FtAddAnt(ref TantKey[] tAnts, ref int nAntkeys, int nMax, FtChan pChan, string cFarAuse, FtSiteStr ptFSite)
        {
            //...Log2.v("\n\nRemoteAnte.FtAddAnt(): Entry");

            if (cFarAuse.Equals("TR") || cFarAuse.Equals("TX") || cFarAuse.Equals("STX"))
            {
                /*	Add the rx antennas */

                FtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx1, ptFSite);
                FtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx2, ptFSite);
                FtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbrx3, ptFSite);
            }
            if (cFarAuse.Equals("TR") || cFarAuse.Equals("RX") || cFarAuse.Equals("DV1") || cFarAuse.Equals("DV2"))
            {
                FtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbtx1, ptFSite);
                FtAddAnt1(ref tAnts, ref nAntkeys, nMax, pChan.call1, pChan.call2, pChan.bndcde,
                                pChan.antnumbtx2, ptFSite);
            }

            //...Log2.v("\n\nRemoteAnte.FtAddAnt(): Exit");
            return 0;
        }

        /// <summary>
        /// If an antenna number is non-zero, try to find it in the array; if it is not found, 
        /// add it to the array.
        /// </summary>
        /// <param name="tAnts"></param>
        /// <param name="nAntKeys"></param>
        /// <param name="nMax"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="anum"></param>
        /// <param name="ptSite"></param>
        /// <returns></returns>
        private static int FtAddAnt1(ref TantKey[] tAnts, ref int nAntKeys, int nMax, string call1, string call2, string bndcde, int anum, FtSiteStr ptSite)
        {
            //...Log2.v("\n\nRemoteAnte.FtAddAnt1(): Entry");

            int nInd;
            int nAnt;

            if (anum != 0)
            {
                nAnt = FtUtils.FtFindAnt(ptSite, call2, bndcde, anum);
                /*	Find it in the list. */
                for (nInd = 0; nInd < nAntKeys; nInd++)
                {
                    if (tAnts[nInd].anum == anum)
                    {
                        tAnts[nInd].aht = ptSite.stAntsPtr[nAnt].aht;
                        break;
                    }
                }

                /*	If we didn't find it, add it. */
                if (nInd >= nAntKeys)
                {
                    if (nAnt >= 0)
                    {
                        if (nAntKeys >= nMax)
                        {
                            /*	Overflowed the array */
                            GenUtil.SetError(1100, "\r\nftAddAnt1 - Too many antennas.");
                            ValErrs.AddMess("ftAddAnt1 - Too many antennas.",
                                            ValErrs.MakeKeyLine(call1, call2, bndcde, anum, ""), "E");
                            return -1;
                        }

                        tAnts[nAntKeys].call1 = call1;
                        tAnts[nAntKeys].call2 = call2;
                        tAnts[nAntKeys].bndcde = bndcde;
                        tAnts[nAntKeys].anum = anum;
                        tAnts[nAntKeys].aht = ptSite.stAntsPtr[nAnt].aht;
                        nAntKeys++;
                    }
                }
            }
            //...Log2.v("\n\nRemoteAnte.FtAddAnt1(): Exit");
            return 0;
        }






    }
}
