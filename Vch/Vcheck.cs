using _Auxlib;
using _Configuration;
using _DataStructures;
using _NewLib;
using _Utillib;
using static _NewLib.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vch
{
    /// <summary>
    /// This class provides methods that perform the detailed checking on a set 
    /// of ft_ DB tables previously created by the MICS program FtImport; these 
    /// checks are additional to, and independent of, the checks performed by 
    /// FtValidate.
    /// </summary>
    public class Vcheck
    {

        private class Tstats
        {
            public int nSiteCt;
            public int nAnteCt;
            public int nChanCt;

            public int nErrors;
            public int nWarnings;
        }

        /// <summary>
        /// This method is the top-level call that performs the additional post-validation
        /// checking of a set of ft_ tables for a prescribed TS PDF; it assembles information
        /// required for a call to the method CheckSite().
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="detailRequested"></param>
        /// <returns></returns>
        public static int Go(string pdfName, bool detailRequested)
        {
            int retVal = 0;
            string[] cCalls;
            int nCount;
            int nInd;
            FtSiteStr ftSiteStr;
            Tstats tStats;

            Console.Write("\r\nChecking file: {0}", pdfName);

            if (detailRequested)
            {
                Console.Write("\r\nDetailed Output Requested.");
            }

            // Instantiate a statistics-keeper object.
            tStats = new Tstats();

            // Create a string[] containing all the call signs that appear in the
            // user's ft_XXX_site table; a call sign is unique to the site and the
            // array of call signs will be used to enumerate over sites. 
            if ((nCount = FtUtils.FtEnumCallSigns(pdfName, out cCalls)) <= 0)
            {
                //	Print an error message 
                Log2.e("\nVcheck.Go(): ERROR: call to FtEnumCallSigns() returned " + nCount);
                Console.Write("\r\n{0}", GenUtil.GetUserMess());
                return Constant.FAILURE;
            }

            // For each call sign (i.e. site) create and populate a FtSiteStr object that
            // amalgamates all the site, ante, and chan, information for that call sign;
            // then call the method CheckSite() that performs several different checks 
            // for the validity and coherence of the FtSiteStr data.
            for (nInd = 0; nInd < nCount; nInd++)
            {
                Console.Write("\r\n-------------------------------------------------------------");
                Console.Write("\r\n{0}) Site {1}:-\r\n", nInd + 1, cCalls[nInd]);

                // Create and populate a FtSiteStr object for this call sign (site) by 
                // fetching information from all the user tables: ft_XXX_ante , ft_XXX_chan and ft_XXX_site.
                retVal = FtUtils.FtGetSite(cCalls[nInd], out ftSiteStr, 3, pdfName);
                if (retVal < 0)
                {
                    //	Error in retrieval. Print and end	
                    Log2.e("\nVcheck.Go(): ERROR: call to FtGetSite() returned " + retVal);
                    Console.Write("Retrieval Error: {0}\n", GenUtil.GetUserMess());
                    return -2;
                }

                //	Keep track of the statistics  
                tStats.nSiteCt++;
                tStats.nAnteCt += ftSiteStr.nNumAnts;
                tStats.nChanCt += ftSiteStr.nNumChans;

                // The following method call performs the Vch checks on the FtSiteStr object. 
                CheckSite(ftSiteStr, pdfName, ref tStats, detailRequested);
            }

            // Write the statistics to Console.Out.
            Console.Write("\r\n---------------------------------------------------------------");
            Console.Write("\r\nSites Read...: {0,6:D}", tStats.nSiteCt);
            Console.Write("\r\nAntennas Read: {0,6:D}", tStats.nAnteCt);
            Console.Write("\r\nChannels Read: {0,6:D}", tStats.nChanCt);
            Console.Write("\r\n\r\n{0} Errors were found", tStats.nErrors);
            Console.Write("\r\n{0} Warnings were issued.", tStats.nWarnings);
            Console.Write("\r\n\r\n");

            return (tStats.nErrors);
        }

        /// <summary>
        /// This is the principal 'worker' method that performs the detailed checking of
        /// a prescribed FtSiteStr object.
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <param name="pdfName"></param>
        /// <param name="tStats"></param>
        /// <param name="detailRequested"></param>
        private static void CheckSite(FtSiteStr ftSiteStr, string pdfName, ref Tstats tStats, bool detailRequested)
        {
            string cLong;
            string cEW;
            string cLat;
            string cNS;
            int nOutLat;
            int nOutLong;
            int nInd;
            string cCall;
            string cName;

            List<Link> linkList = new List<Link>();

            GenUtil.UtLongConvStr(ftSiteStr.stSite.longit, out cLong, out cEW);
            GenUtil.UtLatConvStr(ftSiteStr.stSite.latit, out cLat, out cNS);

            cCall = ftSiteStr.stSite.call1;
            cName = ftSiteStr.stSite.name;

            Console.Write("\r\n{0} Oper: {1} \r\nL/L: {2}{3}/{4}{5} Grnd: {6:F1}m",
                             cName, ftSiteStr.stSite.oper,
                             cLat, cNS, cLong, cEW, ftSiteStr.stSite.grnd);

            nOutLat = ftSiteStr.stSite.latit;
            nOutLong = ftSiteStr.stSite.longit;

            // Check: a link's local site must have one or more antennae. (ERROR)
            if (ftSiteStr.nNumAnts <= 0)
            {
                Console.Write("\r\n*** There are no antennas for this site.");
                tStats.nErrors++;
            }

            // Check: a link's local site must have one or more channels.
            if (ftSiteStr.nNumChans <= 0)
            {
                Console.Write("\r\n*** There are no channels for this site.");
                tStats.nErrors++;
            }

            // Create the list of "links" in the site, as opposed to the raw antennas and channels.
            // Each of these links will then be processed further.
            CreateLinks(ftSiteStr, out linkList);

            if (detailRequested)
            {
                Console.Write("\r\nLinks:-");
                Prinlinks(linkList, ftSiteStr);
            }

            //	Now go through the links	
            for (nInd = 0; nInd < linkList.Count; nInd++)
            {
                Link link = linkList[nInd];

                CheckLink(ref ftSiteStr, pdfName, ref link, nInd, ref tStats, detailRequested);
            }

            Console.Write("\r\n");
        }

        /// <summary>
        /// This method inputs a list of T_Links and writes the data to Console.Out.
        /// </summary>
        /// <param name="linkList"></param>
        /// <param name="tSite"></param>
        private static void Prinlinks(List<Link> linkList, FtSiteStr tSite)
        {
            int nInd;
            int nInd1;
            int antInd;
            int chanInd;

            int nNumLinks = linkList.Count;

            Console.Write("\r\nSite {0} name {1} has {2} Link(s)...",
                             tSite.stSite.call1, tSite.stSite.name, nNumLinks);

            for (nInd = 0; nInd < nNumLinks; nInd++)
            {
                Console.Write("\r\n  To: {0} band {1} with {2} antenna(s) and {3} channel(s):",
                                 linkList[nInd].call2, linkList[nInd].bndcde, linkList[nInd].nNumAnts,
                                 linkList[nInd].nNumChans);

                //	Now print the antennas 
                Console.Write("\r\n      Antennas:-");
                for (nInd1 = 0; nInd1 < linkList[nInd].nNumAnts; nInd1++)
                {
                    antInd = linkList[nInd].aAnts[nInd1];
                    Console.Write("\r\n      {0} Ant index {1} anum {2} acode {3}",
                                     nInd1, antInd, tSite.stAntsPtr[antInd].anum,
                                     tSite.stAntsPtr[nInd].acode);
                }

                //	Now the channels  
                Console.Write("\r\n      Channels:-");
                for (nInd1 = 0; nInd1 < linkList[nInd].nNumChans; nInd1++)
                {
                    chanInd = linkList[nInd].aChans[nInd1];
                    Console.Write("\r\n      {0} Chan index {1} chid {2}",
                                     nInd1, chanInd, tSite.stChanPtr[chanInd].chid);
                }
            }
        }

        /// <summary>
        /// This method outputs a list of fully-populated T_Link objects for a prescribed TS site.
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <param name="linkList"></param>
        private static void CreateLinks(FtSiteStr ftSiteStr, out List<Link> linkList)
        {
            int nInd;

            linkList = new List<Link>();

            //	First add all the antennas to the links.  This creates the link array. 
            for (nInd = 0; nInd < ftSiteStr.nNumAnts; nInd++)
            {
                AddAnt(ref linkList, ftSiteStr, nInd);
            }

            /*	Now add the channels to these links.  A channel can only belong to
            *		one link	*/
            for (nInd = 0; nInd < ftSiteStr.nNumChans; nInd++)
            {
                AddChan(ref linkList, ftSiteStr, ref nInd);
            }

            return;
        }

        /// <summary>
        /// This method inputs a partially-populated list of T_Link objects; if a link does not
        /// already exist for the prescribed site then a new link is added to the list; the 
        /// antenna index is added to the link's list of antenna indices; finally, the index of 
        /// the highest TX antenna is determined and saved in the T_Link object; similarly for 
        /// the highest RX antenna.
        /// </summary>
        /// <param name="linkList"></param>
        /// <param name="ftSiteStr"></param>
        /// <param name="nAnt"></param>
        /// <returns></returns>
        private static int AddAnt(ref List<Link> linkList, FtSiteStr ftSiteStr, int nAnt)
        {
            int nInd;
            int nFound = -1;
            Link link;

            // First try to find the link in the link array.  
            for (nInd = 0; nInd < linkList.Count; nInd++)
            {
                link = linkList[nInd];   //	Just to reduce complexity  

                bool sameCall2 = link.call2.Equals(ftSiteStr.stAntsPtr[nAnt].call2);
                bool sameBndcde = link.bndcde.Equals(ftSiteStr.stAntsPtr[nAnt].bndcde);

                if (sameCall2 && sameBndcde)
                {
                    // We have found a link this antenna belongs to. 
                    nFound = nInd;
                    break;
                }
            }

            if (nFound == -1)
            {
                //	We did not find the link.  Add it. 
                nFound = linkList.Count;

                Link newT_Link = new Link();

                //	Initialize the link just added  
                newT_Link.call2 = ftSiteStr.stAntsPtr[nAnt].call2;
                newT_Link.bndcde = ftSiteStr.stAntsPtr[nAnt].bndcde;
                newT_Link.nNumAnts = 0;
                newT_Link.aAnts = new List<int>();
                newT_Link.nNumChans = 0;
                newT_Link.aChans = new List<int>();
                newT_Link.nHighestRXAnt = -1;
                newT_Link.nHighestTXAnt = -1;

                linkList.Add(newT_Link);
            }

            //	Add the Antenna index	
            linkList[nFound].aAnts.Add(nAnt);

            //	Increment the nNumAnts counter.  
            nInd = linkList[nFound].nNumAnts++;

            /*	Store the index of the highest TX and RX antennas for the elevation
            *		calculations */
            if (UsageIsTX(ftSiteStr.stAntsPtr[nAnt].ause))
            {
                if (linkList[nFound].nHighestTXAnt < ftSiteStr.stAntsPtr[nAnt].aht)
                {
                    linkList[nFound].nHighestTXAnt = nAnt;
                }
            }

            if (UsageIsRX(ftSiteStr.stAntsPtr[nAnt].ause))
            {
                if (linkList[nFound].nHighestRXAnt < ftSiteStr.stAntsPtr[nAnt].aht)
                {
                    linkList[nFound].nHighestRXAnt = nAnt;
                }
            }

            return 0;
        }



        /// <summary>
        /// This method returns true if the link's direction is transmit-only or
        /// transmit/receive.
        /// </summary>
        /// <param name="actualUseOfAntenna"></param>
        /// <returns></returns>
        private static bool UsageIsTX(string actualUseOfAntenna)
        {
            bool result = false;

            AnteUse eDirX = GenUtil.GetDirType(actualUseOfAntenna);

            if ((eDirX == AnteUse.eTX) || (eDirX == AnteUse.eTR))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This method returns true if the link's direction is receive-only or
        /// transmit/receive.
        /// </summary>
        /// <param name="actualUseOfAntenna"></param>
        /// <returns></returns>
        private static bool UsageIsRX(string actualUseOfAntenna)
        {
            bool result = false;

            AnteUse eDirX = GenUtil.GetDirType(actualUseOfAntenna);

            if ((eDirX == AnteUse.eRX) || (eDirX == AnteUse.eTR))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// This method inputs a partially-populated lists of T_Link objects as output by
        /// a previous call to AddAnt(); the list is searched to find the link that matches
        /// the prescribed site; the channel index is then added to that T_Link's list of 
        /// channel indices. 
        /// </summary>
        /// <param name="linkList"></param>
        /// <param name="ftSiteStr"></param>
        /// <param name="nChan"></param>
        /// <returns></returns>
        private static int AddChan(ref List<Link> linkList, FtSiteStr ftSiteStr, ref int nChan)
        {
            int nInd;
            int nFound = -1;
            Link link;

            //	First find the link this channel belongs to.  
            for (nInd = 0; nInd < linkList.Count; nInd++)
            {
                link = linkList[nInd];   //	Just to reduce complexity  

                bool sameCall2 = link.call2.Equals(ftSiteStr.stChanPtr[nChan].call2);
                bool sameBndcde = link.bndcde.Equals(ftSiteStr.stChanPtr[nChan].bndcde);

                if (sameCall2 && sameBndcde)
                {
                    //	We have found the link this channel belongs to. 
                    nFound = nInd;
                    break;
                }
            }

            if (nFound == -1)
            {
                //	Link not found for this channel.  This is an error 
                Console.Write("\r\n*ERROR* There is no link for channel {0}:-\r\n        {1} to {2} band {3} chid {4}.",
                                 nChan,
                                 ftSiteStr.stSite.call1, ftSiteStr.stChanPtr[nChan].call2,
                                 ftSiteStr.stChanPtr[nChan].bndcde, ftSiteStr.stChanPtr[nChan].chid);
                return (-1);
            }
            else
            {
                //	Add the channel to the link in nFound.  
                nInd = linkList[nFound].nNumChans++;
                linkList[nFound].aChans.Add(nChan);        //	Add the Channel index	
            }

            return 0;
        }

        /// <summary>
        /// Given the local end of a link, this method will return the site at the remote end
        /// of the prescribed link and also a 'reverse' list of links from the remote site's
        /// viewpoint.
        /// </summary>
        /// <param name="localLink"></param>
        /// <param name="remoteLinkList"></param>
        /// <param name="remoteFtSiteStr"></param>
        /// <param name="pdfName"></param>
        /// <returns></returns>
        private static int GetRemoteEnd(Link localLink,
                                        out List<Link> remoteLinkList,
                                        out FtSiteStr remoteFtSiteStr,
                                        string pdfName)
        {
            // 'out' requirements.
            remoteLinkList = new List<Link>();
            remoteFtSiteStr = null;

            int nRet = 0;
            string str;

            nRet = FtUtils.FtGetSite(localLink.call2, out remoteFtSiteStr, 3, pdfName);

            if (nRet > 0)
            {
                //  Site not found  
                str = String.Format("getOtherEnd: Site {0} not found in file ({1}).", localLink.call2, nRet);
                GenUtil.SetError(5100, str);
            }
            else if (nRet < 0)
            {
                str = String.Format("getOtherEnd: Error {0} retrieving site: {1}.", nRet, localLink.call2);
                GenUtil.SetError(5101, str);
            }
            else
            {
                // Create the list of links from the remote end's point of view. 
                CreateLinks(remoteFtSiteStr, out remoteLinkList);
            }

            return nRet;
        }

        /// <summary>
        /// This method checks a link for validity; a link is defined as a connection between a local 
        /// and a remote site on a prescribed frequency band; the links have previously been parsed out of the 
        /// site structure and input into this method as a list of T_Link objects that are
        /// populated with all the required antenna and channel numbers required to perform 
        /// the validity checking.
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <param name="pdfName"></param>
        /// <param name="link"></param>
        /// <param name="nInd"></param>
        /// <param name="tStats"></param>
        /// <param name="detailRequested"></param>
        /// <returns></returns>
        private static int CheckLink(ref FtSiteStr ftSiteStr,
                            string pdfName,
                            ref Link link,
                            int nInd,
                            ref Tstats tStats,
                            bool detailRequested)
        {
            int nRet = 0;

            try
            {

                List<Link> remoteEndLinkList;
                Link remoteEndLink;
                FtSiteStr remoteFtSiteStr;
                int nFound = 0;
                double distanceKm;
                double bearing12;
                double bearing21;
                double dActualDist = 0.0;

                Console.Write("\r\n\r\n  Link {0} to {1}, band {2}:-",
                                 nInd, link.call2, link.bndcde);

                if (ftSiteStr.stSite.call1[0] == 'A')
                {
                    //	This is an area site.  No recalculations will be done.  
                    Console.Write("\r\n  Area Site -- No checks.");
                    return (1);
                }

                if (ftSiteStr.stSite.cmd.Equals("D"))
                {
                    //	Site deletion.  No checks will be done.  
                    Console.Write("\r\n  Site being deleted -- No checks.");
                    return (2);
                }

                // Check: a link must have at least one local antenna.
                // AH: why is the error count not incremented?!
                if (link.nNumAnts <= 0)
                {
                    Console.Write("\r\n    *ERROR* No antennas at this end of this link.");
                    return (-2);
                }

                // Check: each link must have at least one local channel.
                // AH: why is the error count not incremented?!
                if (link.nNumChans <= 0)
                {
                    Console.Write("\r\n    *ERROR* No channels at this end of this link.");
                    return (-3);
                }

                // Get the FtSiteStr object corresponding to the site at the remote end of this link. 
                // Also, get a list of links for the prescribed bndcde from the viewpoint of the 
                // remote end of the current link.
                nRet = GetRemoteEnd(link, out remoteEndLinkList, out remoteFtSiteStr, pdfName);
                if (nRet == 0)
                {
                    //	We have both ends of the link; do the comparisons.
                    if (detailRequested)
                    {
                        Console.Write("\r\n--Other End...");
                        Prinlinks(remoteEndLinkList, remoteFtSiteStr);
                    }

                    // Get the remote end of this link  
                    nFound = -1;
                    for (nInd = 0; nInd < remoteEndLinkList.Count; nInd++)
                    {
                        remoteEndLink = remoteEndLinkList[nInd];

                        if (remoteEndLink.call2.Equals(ftSiteStr.stSite.call1) &&
                              remoteEndLink.bndcde.Equals(link.bndcde))
                        {
                            //	This is the one.  Show as found.  
                            nFound = nInd;
                            break;
                        }
                    }

                    // If we can't find the remote end of the link, then we have an error
                    // Check: a link must have a matching link from the remote end's POV. (WARNING)  
                    if (nFound < 0)
                    {
                        Console.Write("\r\n*WARNING* Could not get the other end of link to {0} band {1}.",
                                         link.call2, link.bndcde);
                        tStats.nWarnings++;
                        return -1;
                    }

                    // Now calculate the projected Ground distance and azimuth. 
                    AxSub2.AxDistan((ftSiteStr.stSite.latit / 100.0),  // latitude for Stn 1, in secs 
                                     (remoteFtSiteStr.stSite.latit / 100.0), // latitude for Stn 2 
                                     (ftSiteStr.stSite.longit / 100.0), // longitude for Stn 1 
                                     (remoteFtSiteStr.stSite.longit / 100.0),    // longitude for Stn 2 
                                     out distanceKm,   // output - distance between stations 
                                     out bearing12,    // output - bearing from Stn 1 to 2 
                                     out bearing21);   // output - bearing from Stn 2 to 1 

                    Console.Write("\r\n    Ground distance: {0:F4}, azimuth: {1:F4}, back-azimuth: {2:F4}",
                                     distanceKm, bearing12, bearing21);

                    //	We go through all the antennas at this end of the link.  
                    Console.Write("\r\nAnt  Anum  Height   Azimuth  Elevation   Distance");
                    Console.Write("\r\n                                 Angle");

                    for (nInd = 0; nInd < link.nNumAnts; nInd++)
                    {
                        FtAnte pThisAnte = ftSiteStr.stAntsPtr[link.aAnts[nInd]];

                        double dElev12;
                        double dElev21;
                        double dGrnd1Km = ftSiteStr.stSite.grnd / 1000.0;
                        double dGrnd2Km = remoteFtSiteStr.stSite.grnd / 1000.0;
                        double dOEaht = -1.0;

                        /// This method returns the antenna index of the highest antenna at the site at 
                        /// the other end (OE) of a link that is used by this antenna; if an error occurs
                        /// the value -1 is returned.
                        string msg;
                        dOEaht = FtUtils.HeightTallestRemoteAnte(ftSiteStr,            // local site
                                                                remoteFtSiteStr,      // remote Site 
                                                                link.aAnts[nInd],     // list of links
                                                                out msg);             // error message

                        // Check the returned value for an error condition.
                        if (dOEaht < 0)
                        {
                            if (pThisAnte.cmd == "D")
                            {
                                // Ignore errors for an antenna that is marked for deletion. 
                                msg = "   <= Antenna being deleted so is not analysed.";
                            }
                            else
                            {
                                msg = "\n    *ERROR* " + msg;
                                tStats.nErrors++;
                            }

                            Log2.e("\n");
                            Log2.e("\n\nVcheck.CheckLink(): ERROR: G: call to FtUtils.HeightTallestRemoteAnte() failed, returned:  " + dOEaht);
                            Log2.e("\n\nVcheck.CheckLink(): ERROR: G: message = " + msg);
                            Log2.e("\n");

                            Console.Write("\r\n{0,3:D} {1,4:D}  {2,7:F3}   *******  *********   ********{3}",
                                            nInd, pThisAnte.anum, pThisAnte.aht, msg);

                            // Process the next local antenna.
                            continue;
                        }

                        //	Calculate the elevation angles  
                        AxSub3.AxElev(dGrnd1Km + (pThisAnte.aht / 1000.0), // ht of stn1 antenna km	
                                 dGrnd2Km + (dOEaht / 1000.0),          // ht of stn2 antenna km	
                                     distanceKm,                                    //  distance between stations 			
                                 out dElev12,                                      // elevation angle from stn1 to 2 	
                                 out dElev21);                                     // elevation angle from stn2 to 1 	

                        // Now calculate the distance between the two antennae.
                        dActualDist = AxSub3.PathDist(dGrnd1Km + (pThisAnte.aht / 1000.0),// Aht 1-km 
                                                                     dGrnd2Km + (dOEaht / 1000.0),// Aht 2-km amsl
                                                                     distanceKm);                 // Distance on surface - km 


                        //	Print out the results  
                        Console.Write("\r\n{0,3:D} {1,4:D}  {2,7:F3} {3,9:F4} {4,10:F4} {5,10:F4}",
                                         nInd, pThisAnte.anum, pThisAnte.aht, bearing12, dElev12, dActualDist);

                        // Check: the antenna height should be between 0 and 300m. (WARNING)
                        if (pThisAnte.aht < 0.0 || pThisAnte.aht >= 300.0)
                        {
                            Console.Write("\r\n    *WARNING* It is recommended that antenna height be between 0 and 300m.");
                            tStats.nWarnings++;
                        }

                        // Check: The Vch calculated distance from the local antenna to the remote antenna
                        //        must be within 1 m of the value calculated by FtValidate and stored in the 'dist' column of table
                        //        ft_XXX_ante. (ERROR)
                        // Notes: 
                        //        a. FtValidate populates the 'dist' column of table ft_XXX_ante by calculating the
                        //           distance between the bases (ground level) between the two sites in a link.
                        //        b. the distance calculation in Vch is between the two antennae in a link; this is
                        //           different to [a].
                        if (Math.Abs(pThisAnte.dist - dActualDist) > 1.0)
                        {
                            Console.Write("\r\n    *ERROR* Calculated distance differs by more than 1m from stored: {0,9:F2}.",
                                             pThisAnte.dist);
                            tStats.nErrors++;
                        }

                        // Check: the distance between the two antennae in a link must be between 0 and 90 Km. (WARNING)
                        if (pThisAnte.dist < 0 || pThisAnte.dist > 90.0)
                        {
                            Console.Write("\r\n    *WARNING* It is recommended that the distance be between 0 and 90km.");
                            tStats.nWarnings++;
                        }

                        // Check: The Vch calculated elevation angle from the local antenna to the remote antenna
                        //        must be within 0.01 degrees of the value calculated by FtValidate and stored in the 'elvtn' 
                        //        column of table ft_XXX_ante. (ERROR)
                        if (Math.Abs(pThisAnte.elvtn - dElev12) > 0.01)
                        {
                            Console.Write("\r\n    *ERROR* Calculated elevation angle differs by more than 0.01 degree from stored: {0:F4}",
                                             pThisAnte.elvtn);
                            tStats.nErrors++;
                        }

                        // Check: The Vch calculated azimuth angle from the local antenna to the remote antenna
                        //        must be within 0.01 degrees of the value calculated by FtValidate and stored in the 'azmth' 
                        //        column of table ft_XXX_ante. (ERROR)
                        if (Math.Abs(pThisAnte.azmth - bearing12) > 0.01)
                        {
                            Console.Write("\r\n    *ERROR* Calculated Azimuth differs by more than 0.01 degrees from stored: {0:F4}",
                                             pThisAnte.azmth);
                            tStats.nErrors++;
                        }

                    } // end of loop thru antennae.

                    //	Now check the channels.  
                    Console.Write("\r\n  Chan Chid Tx Frequency  Rx Frequency  Tx Power  Rx Power  Tx Fsl  Rx Fsl");

                    // Enumerate thru the channels associated with the link.
                    for (nInd = 0; nInd < link.nNumChans; nInd++)
                    {
                        int nChan = link.aChans[nInd];
                        FtChan localFtChan = ftSiteStr.stChanPtr[nChan];
                        FtChan remoteFtChan;

                        Console.Write("\r\n{0,2:D}({1,2:D}) {2,4} ", nInd, nChan, localFtChan.chid);

                        if (localFtChan.cmd.Equals("D"))
                        {
                            //	Ignore deleted channels  
                            Console.Write(" ** Deleted, will be ignored.");
                            continue;
                        }

                        //	Transmit Frequency  
                        if (localFtChan.freqtx == 0.0)
                        {
                            Console.Write("              ");
                        }
                        else
                        {
                            Console.Write("{0,12:F3}  ", localFtChan.freqtx);
                        }

                        //	Receive Frequency  
                        if (localFtChan.freqrx == 0.0)
                        {
                            Console.Write("            ");
                        }
                        else
                        {
                            Console.Write("{0,12:F3}", localFtChan.freqrx);
                        }

                        // Transmit Power  
                        if (localFtChan.pwrtx == 0.0)
                        {
                            Console.Write("          ");
                        }
                        else
                        {
                            Console.Write("{0,10:F3} ", localFtChan.pwrtx);
                        }

                        // Receive Power  
                        if (localFtChan.pwrrx1 == 0.0)
                        {
                            Console.Write("         ");
                        }
                        else
                        {
                            Console.Write("{0,9:F3} ", localFtChan.pwrrx1);
                        }

                        // Transmit Feed System Loss  
                        if (localFtChan.afsltx1 == 0.0)
                        {
                            Console.Write("         ");
                        }
                        else
                        {
                            Console.Write("{0,7:F3} ", localFtChan.afsltx1);
                        }

                        // Receive Feed System Loss  
                        if (localFtChan.afslrx1 == 0.0)
                        {
                            Console.Write("       ");
                        }
                        else
                        {
                            Console.Write("{0,7:F3} ", localFtChan.afslrx1);
                        }

                        // Now go through these values and check for errors.

                        // Receive Frequency  
                        if (localFtChan.freqrx == 0.0)
                        {
                        }
                        else
                        {
                            // Check: a channel cannot have the same Tx and Rx Frequencies. (WARNING)
                            if (localFtChan.freqtx == localFtChan.freqrx)
                            {
                                Console.Write("\r\n    *WARNING* Transmit and Receive Frequencies are the same.");
                                tStats.nWarnings++;
                            }
                        }
                        //	Transmit Power  
                        if (localFtChan.pwrtx != 0.0)
                        {
                            if (localFtChan.freqtx == 0.0)
                            {
                                // Check: a channel cannot have a non-zero Tx power if its Tx frequency is zero.
                                Console.Write("\r\n    *ERROR* Transmit power, but no transmit frequency.");
                                tStats.nErrors++;
                            }

                            // Check: a channel's Tx power should be within the range 0 - 50 dBm. (WARNING) 
                            if (localFtChan.pwrtx < 0.0 ||
                                 localFtChan.pwrtx > 50.0)
                            {
                                Console.Write("\r\n    *WARNING* Transmit Power is out of recommended range (0 - 50dbm).");
                                tStats.nWarnings++;
                            }
                        }
                        else
                        {
                            // Check: a channel's Tx power must be non-zero. (WARNING)
                            if (localFtChan.freqtx != 0.0)
                            {
                                Console.Write("\r\n    *WARNING* Transmit Power is zero.");
                                tStats.nWarnings++;
                            }
                        }

                        //	Receive Power  
                        if (localFtChan.pwrrx1 == 0.0)
                        {
                            // Check: a channel cannot have a zero Rx power if its Rx frequency is non-zero. (WARNING)
                            if (localFtChan.freqrx != 0.0)
                            {
                                Console.Write("\r\n    *WARNING* Zero rx power on RX channel.");
                                tStats.nWarnings++;
                            }
                        }
                        else
                        {
                            // Check: a channel cannot have non-zero Rx power if its Rx frequency is zero. (ERROR)
                            if (localFtChan.freqrx == 0.0)
                            {
                                Console.Write("\r\n    *ERROR* Receive power but no rx frequency.");
                                tStats.nErrors++;
                            }
                        }

                        //	Transmit Feed System Loss  
                        if (localFtChan.afsltx1 == 0.0)
                        {
                            // Check: a channel cannot have a zero feed system loss *to* its antenna if
                            //        its Tx frequency is non-zero. (WARNING)
                            if (localFtChan.freqtx != 0.0)
                            {
                                Console.Write("\r\n    *WARNING* TX Feed System Loss is zero.");
                                tStats.nWarnings++;
                            }
                        }
                        else
                        {
                            // Check: a channel cannot have a non-zero feed system loss *to* its antenna if
                            //        its Tx frequency is zero. (WARNING)
                            if (localFtChan.freqtx == 0.0)
                            {
                                Console.Write("\r\n    *WARNING* TX Feed System Loss, but no TX frequency.");
                                tStats.nWarnings++;
                            }
                        }

                        //	Receive Feed System Loss  
                        if (localFtChan.afslrx1 == 0.0)
                        {
                            if (localFtChan.freqrx != 0.0)
                            {
                                Console.Write("\r\n    *WARNING* RX Feed System Loss is zero.");
                                tStats.nWarnings++;
                            }
                        }
                        else
                        {
                            // Check: a channel cannot have a non-zero feed system loss *from* its antenna if
                            //        its Rx frequency is zero. (WARNING)
                            if (localFtChan.freqrx == 0.0)
                            {
                                Console.Write("\r\n    *WARNING* RX Feed System Loss, but no RX Frequency.");
                                tStats.nWarnings++;
                            }
                        }
                        // Now relate this to the primary Rx antenna and get the main Tx channel at the other end.
                        if (localFtChan.freqrx != 0.0)
                        {
                            if (localFtChan.antnumbrx1 != 0)
                            {
                                // Get the primary Rx antenna array index for this channel.
                                // This index is the 'n' in  ftSiteStr.stAntsPtr[n]. 
                                int nAntInd = FtUtils.FtAnteIndex(ftSiteStr.stAntsPtr, (short)ftSiteStr.nNumAnts,
                                                                                    localFtChan.call2, localFtChan.bndcde,
                                                                                    localFtChan.antnumbrx1);
                                FtAnte pChanAnte;
                                FtAnte pTxAnte;
                                int remoteChanInd;

                                double dRxPwr;
                                double dGainRem = 0.0;
                                double dGainLoc = 0.0;
                                SuAntStr suAntStr;
                                int nRet_;
                                bool IsGoodToGo = true;

                                if (nAntInd < 0)
                                {
                                    // Check: a Rx channel must have a designated primary Rx antenna. (ERROR)
                                    // AH: is the following write statement correct - shouldn't it be 'main Rx antenna'?
                                    Console.Write("\r\n    *ERROR* Could not find the corresponding main TX antenna for this channel.");
                                    tStats.nErrors++;
                                    continue;
                                }
                                else
                                {
                                    // Found the primary Rx antenna, make sure its usage is Rx and redo the calculations.
                                    // Check: the usage (ause) of the primary Rx antenna must be either 'RX' or 'TR'. (WARNING)
                                    pChanAnte = ftSiteStr.stAntsPtr[nAntInd];
                                    if (!pChanAnte.ause.StartsWith("RX") && !pChanAnte.ause.StartsWith("TR"))
                                    {
                                        Console.Write("\r\n    *WARNING* Main rx antenna is not RX or TR.");
                                        tStats.nWarnings++;
                                    }

                                    // Find the Tx channel array index at the remote end of the link.
                                    // This index is the 'n' in  remoteEndSite.stChanPtr[n].  
                                    remoteChanInd = FtUtils.FindFtChid(remoteFtSiteStr, localFtChan.call1,
                                                                                     localFtChan.bndcde, localFtChan.chid);

                                    if (remoteChanInd < 0)
                                    {
                                        Console.Write("\r\n    *ERROR* Could not find remote channel.");
                                        tStats.nErrors++;
                                        continue;
                                    }

                                    // AH: shouldn't there be a guard against remoteChanInd being negative,
                                    //     i.e. what if FtUtils.FindFtChid() fails?
                                    remoteFtChan = remoteFtSiteStr.stChanPtr[remoteChanInd];

                                    // Find the array index of the primary Tx antenna at the other end,
                                    // i.e. the 'n' in remoteFtSiteStr.stAntsPtr[n] 
                                    nAntInd = FtUtils.FtAnteIndex(remoteFtSiteStr.stAntsPtr,
                                                                                (short)remoteFtSiteStr.nNumAnts,
                                                                                localFtChan.call1,
                                                                                localFtChan.bndcde,
                                                                                remoteFtChan.antnumbtx1);

                                    if (nAntInd < 0)
                                    {
                                        Console.Write("\r\n    *ERROR* Could not find remote antenna.");
                                        tStats.nErrors++;
                                        continue;
                                    }

                                    pTxAnte = remoteFtSiteStr.stAntsPtr[nAntInd];

                                    // Check: a link's local channel Rx frequency and the remote Tx frequency must be the same. (ERROR)
                                    if (localFtChan.freqrx != remoteFtChan.freqtx)
                                    {
                                        Console.Write("\r\n    *ERROR* Different TX and RX frequencies at each end of channel!");
                                        Console.Write("\r\n            Rx this end: {0:F3}, Tx other end: {1:F3}",
                                                         localFtChan.freqrx, remoteFtChan.freqtx);
                                        tStats.nErrors++;
                                    }

                                    // Check: the remote end Tx antenna's 'acode' must already exist in the SDB table main.sd_ante. (ERROR)
                                    nRet_ = Suutils.SuGetAnt(pTxAnte.acode, out suAntStr);
                                    if (nRet_ != 0)
                                    {
                                        Console.Write("\r\n    *ERROR* Could not find antenna code in database:- '{0}' ({1}).", pTxAnte.acode, nRet_);
                                        IsGoodToGo = false;
                                        tStats.nErrors++;
                                    }
                                    else
                                    {
                                        // We found the remote end's SuAntStr object; get the antenna gain.
                                        dGainRem = suAntStr.acAnt.again;
                                    }

                                    // Now get the antennas and gains at the local end.  

                                    // Check: a link's local Rx antenna 'acode' must already exist in the SDB table main.sd_ante. (ERROR)
                                    nRet_ = Suutils.SuGetAnt(pChanAnte.acode, out suAntStr);
                                    if (nRet_ != 0)
                                    {
                                        Console.Write("\r\n    *ERROR* Could not find local antenna '{0}' in database ({1}).",
                                                         pChanAnte.acode, nRet_);
                                        IsGoodToGo = false;
                                        tStats.nErrors++;
                                    }
                                    else
                                    {
                                        // We found this end's SuAntStr object; get the antenna gain.
                                        dGainLoc = suAntStr.acAnt.again;
                                    }

                                    if (IsGoodToGo)
                                    {
                                        dRxPwr = remoteFtChan.pwrtx + dGainRem - remoteFtChan.afsltx1 -
                                                         Constant.POWER_CONST -
                                                         20.0 * Math.Log10(remoteFtChan.freqtx / 1000.0) -
                                                         20.0 * Math.Log10(dActualDist) -
                                                         GenUtil.AtmosphericAtten(remoteFtChan.freqtx, dActualDist) +  // Added for atmospheric atten. 2012.12
                                                         dGainLoc - localFtChan.afslrx1;

                                        // Check: Vch calculated Rx power must agree with the value stored in the 'pwrrx1'
                                        //        column of table ft_XXX_chan within a tolerance of 0.01 dBm. (ERROR)
                                        if (Math.Abs(dRxPwr - localFtChan.pwrrx1) > 0.01)
                                        {
                                            Console.Write("\r\n    *ERROR* Rx Power 1 does not agree with calcs,");
                                            Console.Write(" ({0,8:F4}) vs ({1,8:F4}).", localFtChan.pwrrx1, dRxPwr);
                                            tStats.nErrors++;
                                        }
                                    }
                                    //	Check for first diversity and get its antenna   
                                    if (localFtChan.antnumbrx2 != 0)
                                    {
                                        int nAntInd_ = FtUtils.FtAnteIndex(ftSiteStr.stAntsPtr, (short)ftSiteStr.nNumAnts,
                                                                                            localFtChan.call2, localFtChan.bndcde,
                                                                                            localFtChan.antnumbrx2);
                                        FtAnte pThisDiv;

                                        // Check: if a channel has antnumbrx2 != 0 then diversity antenna 1 (DV1) must exist in the
                                        //        ft_XXX_ante table. (ERROR)  
                                        if (nAntInd_ <= 0)
                                        {
                                            Console.Write("\r\n    *ERROR* Could not retrieve diversity antenna 1 for this channel.");
                                            tStats.nErrors++;
                                        }
                                        else
                                        {
                                            // We found diversity antenna 1 (DV1).
                                            pThisDiv = ftSiteStr.stAntsPtr[nAntInd_];

                                            // Check: the antenna code (acode) of DV1 must exist in the SDB main.sd_ante. (ERROR)
                                            nRet_ = Suutils.SuGetAnt(pThisDiv.acode, out suAntStr);
                                            if (nRet_ != 0)
                                            {
                                                Console.Write("\r\n    *ERROR* Could not find local antenna {0} in database.",
                                                                 pChanAnte.acode);
                                                IsGoodToGo = false;
                                                tStats.nErrors++;
                                            }
                                            else
                                            {
                                                // Get the local antenna's gain.
                                                dGainLoc = suAntStr.acAnt.again;
                                            }

                                            if (IsGoodToGo)
                                            {
                                                dRxPwr = remoteFtChan.pwrtx + dGainRem - remoteFtChan.afsltx1 -
                                                                 Constant.POWER_CONST -
                                                                 20.0 * Math.Log10(remoteFtChan.freqtx / 1000.0) -
                                                                 20.0 * Math.Log10(dActualDist) -
                                                             GenUtil.AtmosphericAtten(remoteFtChan.freqtx, dActualDist) +
                                                                 dGainLoc - localFtChan.afslrx2;

                                                // Check: the DV1 antenna's Rx power must be within 0.01 dBm of that calculated by
                                                //        FtValidate and stored in column 'pwrrx2' of ft_XXX_chan.
                                                if (Math.Abs(dRxPwr - localFtChan.pwrrx2) > 0.01)
                                                {
                                                    Console.Write("\r\n    *ERROR* Rx Power 2 does not agree with calculations,");
                                                    Console.Write(" ({0,8:F4}) vs ({1,8:F4}).", localFtChan.pwrrx2, dRxPwr);
                                                    tStats.nErrors++;
                                                }
                                            }
                                        }

                                    }
                                    // Check for second diversity and get its antenna.              
                                    if (localFtChan.antnumbrx3 != 0)
                                    {
                                        int nAntInd_ = FtUtils.FtAnteIndex(ftSiteStr.stAntsPtr, (short)ftSiteStr.nNumAnts,
                                                                                            localFtChan.call2, localFtChan.bndcde,
                                                                                            localFtChan.antnumbrx3);
                                        FtAnte pThisDiv;

                                        // Check: if a channel has antnumbrx3 != 0 then diversity antenna 2 (DV2) must exist in the
                                        //        ft_XXX_ante table. (ERROR)  
                                        if (nAntInd_ <= 0)
                                        {
                                            Console.Write("\r\n    *ERROR* Could not retrieve diversity antenna 2 for this channel.");
                                            tStats.nErrors++;
                                        }
                                        else
                                        {
                                            // We found diversity antenna 2 (DV2).
                                            pThisDiv = ftSiteStr.stAntsPtr[nAntInd_];

                                            // Check: the antenna code (acode) of DV2 must exist in the SDB main.sd_ante. (ERROR)
                                            nRet_ = Suutils.SuGetAnt(pThisDiv.acode, out suAntStr);
                                            if (nRet_ != 0)
                                            {
                                                Console.Write("\r\n    *ERROR* Could not find local antenna {0} in database.",
                                                                 pChanAnte.acode);
                                                IsGoodToGo = false;
                                                tStats.nErrors++;
                                            }
                                            else
                                            {
                                                dGainLoc = suAntStr.acAnt.again;
                                            }

                                            if (IsGoodToGo)
                                            {
                                                dRxPwr = remoteFtChan.pwrtx + dGainRem -
                                                                 remoteFtChan.afsltx1 -
                                                                 Constant.POWER_CONST -
                                                                 20.0 * Math.Log10(remoteFtChan.freqtx / 1000.0) -
                                                                 20.0 * Math.Log10(dActualDist) -
                                                             GenUtil.AtmosphericAtten(remoteFtChan.freqtx, dActualDist) +
                                                                 dGainLoc - localFtChan.afslrx3;

                                                // Check: the DV2 antenna's Rx power must be within 0.01 dBm of that calculated by
                                                //        FtValidate and stored in column 'pwrrx3' of ft_XXX_chan.
                                                if (Math.Abs(dRxPwr - localFtChan.pwrrx3) > 0.01)
                                                {
                                                    Console.Write("\r\n    *ERROR* Rx Power 3 does not agree with calcs,");
                                                    Console.Write(" ({0,8:F4}) vs ({1,8:F4}).", localFtChan.pwrrx3, dRxPwr);
                                                    tStats.nErrors++;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Check: a channel whose Rx frequency is non-zero must have a primary Rx antenna in table ft_XXX_ante. (ERROR)
                                Console.Write("\r\n    *ERROR* Receive Frequency without main receive antenna.");
                                tStats.nErrors++;
                            }
                        }

                    } // end of loop thru channels.
                }
                else
                {
                    // Check: the remote end of a link must exist. (WARNING)
                    Console.Write("\r\n*WARNING* Other End not present.  No checks will be done.");
                    tStats.nWarnings++;
                }

            }
            catch (Exception e)
            {
                Log2.e("\nVcheck.CheckLink(): ERROR: Exception: " + e.Message);
                Log2.e("\nVcheck.CheckLink(): ERROR: Exception: " + e.StackTrace);
                Application.ExitQuietly(666);
            }

            return nRet;
        }




    }
}
