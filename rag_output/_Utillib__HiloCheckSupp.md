# Documented File: HiloCheckSupp.cs
**Repository Path:** `_Utillib\HiloCheckSupp.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;

using _NewLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    /// <summary>
    /// Provides methods used by the HiLo analysis.
    /// </summary>
    public class HiloCheckSupp
    {
        //--------------------------------------------------------------------------------------------

        private static double[] aFreqTx = new double[Constant.BANDLIST_SZ];
        private static double[] aFreqRx = new double[Constant.BANDLIST_SZ];
        private static int nBandCount = 0;

        //--------------------------------------------------------------------------------------------

        /// <summary>
        /// This is the top-level method for performing a HiLo check.
        /// </summary>
        /// <param name="cTable"> - name of PDF.</param>
        /// <param name="IsVerbose"> - verbose mode switch.</param>
        /// <param name="dDistKm"> - prescribes the minimum distance for HiLo analysis.</param>
        /// <param name="tw"> - TextWriter object used to output HiLo messages.</param>
        /// <returns> - the number of HiLo violations detected.</returns>
        public static int HiloCheckFunc(string cTable, bool IsVerbose, double dDistKm, TextWriter tw)
        {
            //...Log2.v("\n\nHilochecksupp.HiloCheckFunc(): Entry");

            string cCall = "";
            int nRet = 0;
            FtSiteStr pSiteStr_NEW;
            int nViolations = 0;
            int nSites = 0;

            //	Set the ordering of the antennas and channels in the ft and mt tables.  This is 
            //	important because we want to go through all of a band in a whole site before going
            //	on to the next band.
            //
            //  AH: note that there is not an equivalent call to FtSetAntOrder() ?
            FtUtils.FtSetChanOrder("call1, bndcde, call2, chid");

            // Initialize the retieval of FtSiteStr objects from the ft_ tables.
            nRet = FtUtils.FtNextCall(out cCall, cCall, cTable);

            while (nRet == 0)
            {
                // FtSiteStr objects are retrieved in alphabetical order of call1.
                // The associated array of FtAnte objects is ordered by "call1, call2,  bndcde, anum".
                // The associated array of FtClan objects is ordered by "call1, bndcde, call2,  chid"
                nRet = FtUtils.FtGetSite(cCall, out pSiteStr_NEW, 3, cTable);

                //...Log2.v(String.Format("\n\ncall1 = {0}, bandwd1 = {1}, bandwd2 = {2}", pSiteStr_NEW.stSite.call1, pSiteStr_NEW.stSite.bandwd1, pSiteStr_NEW.stSite.bandwd1));

                if (nRet == 0)
                {
                    nSites++;
                    nViolations += HiLoSiteCheck(pSiteStr_NEW, IsVerbose, dDistKm, tw);
                }
                else
                {
                    GenUtil.SetError(9801, "hilocheckfunc: Could not get site %s, reason: %d",
                                                cCall, nRet.ToString());
                    nViolations = -1;
                    break;
                }

                nRet = FtUtils.FtNextCall(out cCall, cCall, cTable);

            }  //end: while (nRet == 0) {

            tw.WriteLine("\r\n" + nSites + " Site(s) checked.");
            //...Log2.v("\n\n" + nSites + " Site(s) checked.\n");

            //...Log2.v("\n\nHilochecksupp.HiLoCheckFunc(): Exit");
            return nViolations;
        }

        /// <summary>
        /// Performs a hilo check on a prescribed site. It checks the channels internally, 
        /// and if it passes, checks with all neighbouring sites up to a distance of dDistKm.
        /// </summary>
        /// <param name="pSite"> - prescribed FtSiteStr_NEW object.</param>
        /// <param name="IsVerbose"> - verbose mode switch</param>
        /// <param name="dDistKm"> - prescribes the minimum distance for HiLo analysis.</param>
        /// <param name="tw"> - TextWriter object used to output HiLo messages.</param>
        /// <returns> - the number of HiLo violations detected.</returns>
        public static int HiLoSiteCheck(FtSiteStr pSite, bool IsVerbose, double dDistKm, TextWriter tw)
        {
            //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): Entry: site has call1 = " + pSite.stSite.call1);

            int nInd = 0;
            int nRet = 0;
            SuBand pBand = new SuBand();
            SuBand pAdjBand;
            string cBand = "";
            string cAdjBand;
            int nCounter;
            FtChan pChan;

            string cSQL;
            string cCallSign;
            MtSiteStr pNearSite;
            string cTrimName;
            string cTrimOper;
            int nViolations = 0;
            int nMDBSites = 0;

            Enums.HiLo eTxHiLo = Enums.HiLo.NotSet;
            Enums.HiLo eRxHiLo = Enums.HiLo.NotSet;
            //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): A: eTxHilo = eRxHilo = Enums.HiLo.NotSet");

            cTrimName = pSite.stSite.name.Trim();
            cTrimOper = pSite.stSite.oper.Trim();

            string str = String.Format("\r\n\r\nChecking {0} ({1}/{2}) in proposed file ...",
                                            pSite.stSite.call1, cTrimName, cTrimOper);
            tw.Write(str);
            //...Log2.v(str);

            /*	Check this site's tx frequencies for each band. */
            //	We rely on the input being properly sorted: ORDER BY call1, bndcde, call2, chid

            /*	Go through the channels resetting at the band breaks */
            BandListClear();    //	The frequencies of this site and band are stored internally in the bandlist.
            nInd = 0;

            // This loop it iterating over the elements of the channel array pSite.stChanPtr[] 
            // for a prescribed site. It is assumed that the sort order for these channels is by bndcde. 
            while (true)
            {
                //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): ===== top of WHILE LOOP: nInd = " + nInd);
                //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): cBand = " + Strings.AddBars(cBand));
                //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): eTxHiLo = " + eTxHiLo);
                //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): eRxHiLo = " + eRxHiLo);

                bool go = false;
                pChan = null;

                if (nInd >= pSite.nNumChans)  // pSite.stChanPtr[nInd] would exceed the array bound.
                {
                    go = true;
                }
                else                          // pSite.stChanPtr[nInd] is within bounds.
                {
                    pChan = pSite.stChanPtr[nInd];        //	Get the channel.

                    //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): new pChan:" + pChan.KeysToString());

                    // cBand still has its old value from the previous loop.
                    // Later, cBand is set equal to pChan.bndcde.
                    // If the bndcde of the last chan record processed is the same as the bndcde
                    // of the current chan record then 'go' will always be FALSE.
                    // Put another way, 'go' is only TRUE when we encounter a bndcde that we have
                    // not handled before.
                    go = !pChan.bndcde.Equals(cBand);
                }

                if (go)
                {
                    //...Log2.v("\r\nHilochecksupp.HiLoSiteCheck(): go is TRUE");

                    // On the first pass throught cBand is an empty string.
                    if (!String.IsNullOrWhiteSpace(cBand))
                    {
                        //...Log2.v("\r\nHilochecksupp.HiLoSiteCheck(): cBand is NOT IsNullOrWhiteSpace)");

                        /* 	Finish off the previous band.  We do this by Running checks on
                        *		the adjacent bands, and then checking the	near sites in the mdb (if required)
                        *		as long as we don't have a violation. */

                        //------------------------------------------------------------------------
                        // Check for HiLo violations that are local to the prescribed site.
                        //------------------------------------------------------------------------
                        if (eTxHiLo != Enums.HiLo.Violation && eRxHiLo != Enums.HiLo.Violation)
                        {
                            //...Log2.v("\r\nHilochecksupp.HiLoSiteCheck(): (eTxHiLo != Enums.HiLo.Violation && eRxHiLo != Enums.HiLo.Violation) is TRUE");

                            tw.Write(String.Format("\r\n  Band {0} OK, checking adjacent bands.", cBand));
                            //...Log2.v(String.Format("\n\n  Band {0} OK, checking adjacent bands.\n", cBand));

                            //	The last band is in pBand
                            nCounter = 0;

                            //	Check through each adjacent band in the band's adjacent band list.
                            //  pBand is still the object from the end of the previous loop. 
                            while ((nRet = GenUtil.StepToken(out cAdjBand, Constant.BNDCDE_SZ, pBand.badj, ";", ref nCounter)) == 0)
                            {
                                //...Log2.v(String.Format("\r\nHilochecksupp.HiLoSiteCheck(): GenUtil.StepToken() succeeded; cAdjBand = {0}, nCounter = {1}", cAdjBand, nCounter));

                                if (!String.IsNullOrEmpty(cAdjBand) && !cAdjBand.Equals(cBand))
                                {
                                    //...Log2.v(String.Format("\r\nHilochecksupp.HiLoSiteCheck(): !String.IsNullOrEmpty(cAdjBand) && !cAdjBand.Equals(cBand) is TRUE")); 

                                    //	blank or don't do this band again.
                                    nRet = Suutils.SuGetBand(cAdjBand, out pAdjBand);

                                    //...Log2.v(String.Format("\r\nHilochecksupp.HiLoSiteCheck(): call to SuGetBand returned nRet = {0}, pAdjBand = {1}", nRet, pAdjBand.ToString()));

                                    // Checks the accumulated lists of frequencies for hilo violations with respect to a prescribed
                                    // midband frequency (pAdjBand.bmidf). 

                                    // AH: REMOVE a.s.a.p.
                                    //...Log2.v(String.Format("\n\ncall1 = {0}, cBand = {1}, cAdjBand = {2}", pSite.stSite.call1, cBand, cAdjBand));
                                    //if (pSite.stSite.call1.Equals("CGUARD1B") && pChan.bndcde.Equals("8B") && cAdjBand.Equals("8A"))
                                    //{
                                    //...Log2.v(FrequenciesToString());
                                    //}

                                    nRet = BandListCheck(pAdjBand);

                                    if (nRet != 0)
                                    {
                                        tw.Write(String.Format("\r\n  HiLo Failure in Band {0}", pAdjBand.bndcde));
                                        //...Log2.v(String.Format("\r\n  HiLo Failure in Band {0}", pAdjBand.bndcde));

                                        nViolations++;
                                    }
                                    else
                                    {
                                        tw.Write(String.Format("\r\n  Adjacent Band {0} OK.", pAdjBand.bndcde));
                                        //...Log2.v(String.Format("\r\n  Adjacent Band {0} OK.", pAdjBand.bndcde));
                                    }
                                }
                                else
                                {
                                    //...Log2.v(String.Format("\r\nHilochecksupp.HiLoSiteCheck(): !String.IsNullOrEmpty(cAdjBand) && !cAdjBand.Equals(cBand) is FALSE"));
                                }
                            } // while ((nRet = GenUtil.StepToken(out cAdjBand, Constant.BNDCDE_SZ, pBand.badj, ";", ref nCounter)) == 0)
                        }
                        else
                        {
                            //...Log2.v("\r\nHilochecksupp.HiLoSiteCheck(): (eTxHiLo != Enums.HiLo.Violation && eRxHiLo != Enums.HiLo.Violation) is FALSE");

                            tw.Write(String.Format("\r\n  Band {0} HiLo failure, not checking adjacent bands.", cBand));
                            //...Log2.v(String.Format("\r\n  Band {0} HiLo failure, not checking adjacent bands.", cBand));

                            if (Suutils.HasSubBands(cBand))
                            {
                                tw.Write(String.Format("\r\n    WARNING: the program could report erroneous HiLo violations for one-way channels in the {0} band.", cBand));
                                //...Log2.v(String.Format("\r\n    WARNING: the program could report erroneous HiLo violations for one-way channels in the {0} band.", cBand));

                            }

                        } // End of checking for local site HiLo violations.

                        //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): point A\n");

                        //------------------------------------------------------------------------
                        // If we have no 'local to site' HiLo violations then proceed to check 
                        // for violations with nearby sites.
                        //------------------------------------------------------------------------
                        if (eTxHiLo != Enums.HiLo.Violation && eRxHiLo != Enums.HiLo.Violation)
                        {
                            //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): eTxHiLo != Enums.HiLo.Violation && eRxHiLo != Enums.HiLo.Violation is TRUE");

                            //	Now go through the adjacent sites (if dDistKm >= 0).  The frequencies will have been
                            //	stored.
                            BandListClear();

                            if (dDistKm >= 0)
                            {
                                //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): dDistKm >= 0 is TRUE");

                                /// Creates a string containing the distance condition for a SQL 'WHERE' clause
                                /// for a prescribed site and minimum HiLo analysis distance.
                                HiloCheckSupp.GetCondition(out cSQL, pSite, dDistKm);

                                //...Log2.v("\r\nHilochecksupp.HiLoSiteCheck(): HiloCheckSupp.GetCondition(): SQL WHERE clause: " + cSQL);

                                // Initialize the site enumeration.
                                cCallSign = "";

                                // Successive calls to this method return the call signs of
                                // adjacent sites that are within the prescribed HiLo analysis distance.
                                // Adjacent sites are accessed in order of their call signs.
                                nRet = MtUtils.MtEnumSite(cSQL, ref cCallSign);

                                //...Log2.v(String.Format("\nHilochecksupp.HiLoSiteCheck(): MtUtils.MtEnumSite() returned nRet = {0}, cCallSign = {1}", nRet, cCallSign));

                                if (nRet == 0)
                                {
                                    //...Log2.v("\r\n  Checking database nearby for this band and adjacent bands ...");

                                    tw.Write(String.Format("\r\n  Checking database nearby for this band and adjacent bands ..."));
                                    //...Log2.v(String.Format("\r\n  Checking database nearby for this band and adjacent bands ..."));
                                }

                                //  Go through the enumerated sites in the mdb.
                                nMDBSites = 0;

                                while (nRet == 0)
                                {
                                    //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): fetching remote site, call1 = " + cCallSign);

                                    nMDBSites++;
                                    int nRetVal = MtUtils.MtGetSite(cCallSign, out pNearSite, 3);

                                    //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): call to MtUtils.MtGetSite(): nRetVal = " + nRetVal);
                                    //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): call to MtUtils.MtGetSite(): pNearSite.stSite.call1 = " + pNearSite.stSite.call1);

                                    nRet = HiLoSiteBandCheck(pNearSite, IsVerbose, cBand, eTxHiLo, eRxHiLo, tw);

                                    //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): call to HiLoSiteBandCheck(): nRet = " + nRet);

                                    if (nRet > 0)
                                    {
                                        nViolations++;

                                        //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): nViolations = " + nViolations);
                                    }

                                    nRet = MtUtils.MtEnumSite(cSQL, ref cCallSign);

                                    //...Log2.v(String.Format("\n\nHilochecksupp.HiLoSiteCheck(): call to MtUtils.MtEnumSite(): nRet = {0}, cCallSign = {1}", nRet, cCallSign));
                                }

                                tw.Write(String.Format("\r\n\r\nThere were {0} nearby sites checked in the database.", nMDBSites));
                                //...Log2.v(String.Format("\r\n\r\nThere were {0} nearby sites checked in the database.\n", nMDBSites));
                            }
                        }
                        else
                        {
                            //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): eTxHiLo != Enums.HiLo.Violation && eRxHiLo != Enums.HiLo.Violation is FALSE");

                            tw.Write(String.Format("\r\n  There were violations in the site.  Adjacent sites will not be checked."));
                            //...Log2.v(String.Format("\r\n  There were violations in the site.  Adjacent sites will not be checked."));
                        }

                    }  // if (!String.IsNullOrWhiteSpace(cBand))
                    else
                    {
                        //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): cBand is empty.\n");
                    }

                    if (nInd < pSite.nNumChans)
                    {
                        //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): (nInd < pSite.nNumChans) returned TRUE.");

                        /* If we are not finishing up, then set the new band */
                        cBand = pChan.bndcde;
                        eTxHiLo = eRxHiLo = Enums.HiLo.NotSet;
                        //...Log2.v("\n\nHilochecksupp.HiLoSiteCheck(): B: eTxHilo = eRxHilo = Enums.HiLo.NotSet");


                        //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): calling Suutils.SuGetBand() for new cBand = " + cBand);

                        nRet = Suutils.SuGetBand(cBand, out pBand);    /*	Read in the current band. */
                        if (nRet != 0)
                        {
                            tw.Write(String.Format("\r\nCould not retrieve Band {0}", cBand));
                            //...Log2.v(String.Format("\r\nCould not retrieve Band {0}", cBand));

                            nRet = -1;
                            break;
                        }
                        else
                        {
                            tw.Write(String.Format("\r\nChecking proposed file Site {0} band {1}.", pChan.call1, cBand));
                            //...Log2.v(String.Format("\r\nChecking proposed file Site {0} band {1}.", pChan.call1, cBand));
                        }
                    }

                }  // if (go)

                if (nInd >= pSite.nNumChans)
                {
                    // Jump out of the main channel loop.
                    //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): end of the channel retrieval loop.");
                    break;
                }

                //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): eTxHiLo = " + eTxHiLo);

                /*	Check one channel: Do the TX */
                if (eTxHiLo != Enums.HiLo.Violation)
                {
                    //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): (eTxHiLo != Enums.HiLo.Violation) is TRUE.");

                    if (Suutils.HasSubBands(pBand.bndcde))
                    {
                        eTxHiLo = SubBandCheck(eTxHiLo, pBand.bmidf, pChan.freqtx, pChan.freqrx);
                    }
                    else
                    {
                        eTxHiLo = BandCheck(eTxHiLo, pBand.bmidf, pChan.freqtx);
                    }

                    if (eTxHiLo == Enums.HiLo.Violation)
                    {
                        tw.Write(String.Format("\r\n{0} to {1} band {2} chid {3} is first TX HiLo violation.",
                            pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));

                        //...Log2.v(String.Format("\r\n{0} to {1} band {2} chid {3} is first TX HiLo violation.", pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));

                        nViolations++;
                    }
                }

                //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): eRxHiLo = " + eRxHiLo);

                /*	Check the Rx */
                if (eRxHiLo != Enums.HiLo.Violation)
                {
                    //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): (eRxHiLo != Enums.HiLo.Violation) is TRUE.");

                    if (Suutils.HasSubBands(pBand.bndcde))
                    {
                        eRxHiLo = SubBandCheck(eRxHiLo, pBand.bmidf, pChan.freqrx, pChan.freqtx);

                    }
                    else
                    {
                        eRxHiLo = BandCheck(eRxHiLo, pBand.bmidf, pChan.freqrx);
                    }

                    if (eRxHiLo == Enums.HiLo.Violation)
                    {
                        tw.Write(String.Format("\r\n{0} to {1} band {2} chid {3} is first RX HiLo violation.",
                            pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));

                        //...Log2.v(String.Format("\r\n{0} to {1} band {2} chid {3} is first RX HiLo violation.", pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));

                        nViolations++;
                    }
                }

                //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): nViolations = " + nViolations);

                //	Save the frequencies to run them through the adjacent bands.
                BandListAdd(pChan.freqtx, pChan.freqrx);

                //...Log2.v(String.Format("\nHilochecksupp.HiLoSiteCheck(): called BandListAdd(): freqtx = {0};  freqrx = {1}", pChan.freqtx, pChan.freqrx));

                // Prepare to process the next channel for the local site.
                // Increment the index of the pSite.stChanPtr[] array of FtChan objects.
                nInd++;

                //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): end of main channel loop.");

            } // while (true) - the end of the main channel loop.

            tw.Write(String.Format("\r\n\r\n---------------------------\r\n"));
            //...Log2.v(String.Format("\r\n\r\n---------------------------\r\n"));

            if (nRet >= 0)
            {
                nRet = nViolations;
            }

            //...Log2.v("\nHilochecksupp.HiLoSiteCheck(): Exit, nRet = " + nRet + "\n"); 
            return nRet;
        }

        /// <summary>
        /// This class maintains two lists: one for transmit frequencies and the other for receive frequences.
        /// This methods empties both of these list.
        /// </summary>
        private static void BandListClear()
        {
            //...Log2.v("\n\nHiloCheckSupp.BandListClear()");

            nBandCount = 0;
        }

        /// <summary>
        /// Checks the accumulated lists of frequencies for hilo violations with respect to a prescribed
        /// midband frequency. 
        /// </summary>
        /// <param name="pBand"> - SuBand object (pBand.bndcde provides the midband frequency).</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - <b>no</b> no HiLo violations detected.</para>
        /// <para>- Constant.FAILURE - HiLo violations were detected.</para>
        public static int BandListCheck(SuBand pBand)
        {
            //...Log2.v("\n\nHiloCheckSupp.BandListCheck(): Entry: pBand.bndcde = " + pBand.bndcde);

            double dMidFreq = pBand.bmidf;
            int nInd;

            Enums.HiLo eRx = Enums.HiLo.NotSet;
            Enums.HiLo eTx = Enums.HiLo.NotSet;
            //...Log2.v("\n\nHilochecksupp.BandListCheck(): C: eTxHilo = eRxHilo = Enums.HiLo.NotSet");

            for (nInd = 0; nInd < nBandCount; nInd++)
            {
                if (Suutils.HasSubBands(pBand.bndcde))
                {
                    eTx = SubBandCheck(eTx, dMidFreq, aFreqTx[nInd], aFreqRx[nInd]);
                    eRx = SubBandCheck(eRx, dMidFreq, aFreqRx[nInd], aFreqTx[nInd]);
                }
                else
                {
                    eTx = BandCheck(eTx, dMidFreq, aFreqTx[nInd]);
                    eRx = BandCheck(eRx, dMidFreq, aFreqRx[nInd]);
                }
            }

            if (eRx == Enums.HiLo.Violation || eTx == Enums.HiLo.Violation)
            {
                nInd = Constant.FAILURE;
            }
            else
            {
                nInd = Constant.SUCCESS;
            }

            //...Log2.v("\n\nHiloCheckSupp.BandListCheck(): Exit: nInd = " + nInd + "\n");
            return nInd;
        }

        /// <summary>
        /// Checks whether a prescribed band has any subbands, or not. The method BandCheck()
        /// only compares the frequencies with the mid-band frequency of the bands.
        /// In some of the higher bands (7B for instance in the 7GHz range) the band
        /// is divided into sub-bands above and below the midband frequency; consequently BandCheck()
        /// would not be applicable. In SubBandCheck() we compare the transmitting frequency with the corresponding
        /// receiving frequency (if present).
        /// </summary>
        /// <param name="eHiLo"> - current HiLo status.</param>
        /// <param name="bmidf"> - mid-band frequency.</param>
        /// <param name="freq1"> - frequency.</param>
        /// <param name="freq2"> - frequency.</param>
        /// <returns> - Enums.HiLo { Hi, Lo, NotSet, Violation }.</returns>
        private static Enums.HiLo SubBandCheck(Enums.HiLo eHiLo, double bmidf, double freq1, double freq2)
        {
            //...Log2.v(String.Format("\n\nHiloCheckSupp.SubBandCheck(): Entry: eHiLo = {0}, bmidf = {1}, freq1 = {2}, freq2 = {3}", eHiLo, bmidf, freq1, freq2));

            Enums.HiLo eRetHiLo = eHiLo;

            //	Only run the code if we do not already have a violation.
            if (eRetHiLo != Enums.HiLo.Violation)
            {
                if (freq1 != 0)
                {
                    //	First frequency is present, compare it to the second.
                    if (freq2 != 0)
                    {
                        //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 and freq2 are both non-zero.");

                        //	Second frequency also present, simply compare them.
                        if (freq1 > freq2)
                        {
                            //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 > freq2.");

                            //	First freq is greater than second.
                            if (eRetHiLo != Enums.HiLo.Hi)
                            {
                                //	If it is already high ignore this.  Otherwise set it if not set.
                                if (eRetHiLo == Enums.HiLo.NotSet)
                                {
                                    eRetHiLo = Enums.HiLo.Hi;
                                }
                                else
                                {
                                    eRetHiLo = Enums.HiLo.Violation;
                                }
                            }
                        }
                        else
                        {
                            //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 < freq2.");

                            //	the second frequency is greater than the first
                            if (eRetHiLo != Enums.HiLo.Lo)
                            {
                                if (eRetHiLo == Enums.HiLo.NotSet)
                                {
                                    eRetHiLo = Enums.HiLo.Lo;
                                }
                                else
                                {
                                    eRetHiLo = Enums.HiLo.Violation;
                                }
                            }
                        }
                    }
                    else
                    {
                        //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 is non-zero, freq2 = 0.");

                        //	freq1 > zero, freq2 is zero
                        if (freq1 > bmidf)
                        {
                            //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 > bmidf.");

                            //	Use bmidf to compare with.
                            if (eRetHiLo != Enums.HiLo.Hi)
                            {
                                if (eRetHiLo == Enums.HiLo.NotSet)
                                {
                                    eRetHiLo = Enums.HiLo.Hi;
                                }
                                else
                                {
                                    eRetHiLo = Enums.HiLo.Violation;
                                }
                            }
                        }
                        else
                        {
                            //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 < bmidf.");

                            //	freq1 < bmidf
                            if (eRetHiLo != Enums.HiLo.Lo)
                            {
                                if (eRetHiLo == Enums.HiLo.NotSet)
                                {
                                    eRetHiLo = Enums.HiLo.Lo;
                                }
                                else
                                {
                                    eRetHiLo = Enums.HiLo.Violation;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //	freq1 is zero, then we are not interested in assigning HiLo
                    //...Log2.v("\nHiloCheckSupp.SubBandCheck(): freq1 is zero - nothing to be done.");
                }

            }
            else
            {
                //...Log2.v("\nHiloCheckSupp.SubBandCheck(): a violation already exists - nothing to be done.");
            }

            //...Log2.v("\nHiloCheckSupp.SubBandCheck(): Exit; eRetHiLo = " + eRetHiLo + "\n");
            return eRetHiLo;
        }

        /// <summary>
        /// Checks the band against the midfreq and the current state of the HiLo
        /// switch, and return what was found.
        /// </summary>
        /// <param name="eBand"> - current HiLo status.</param>
        /// <param name="dMidf"> - mid-band frequency.</param>
        /// <param name="dFreq"> - frequency to be checked.</param>
        /// <returns> - Enums.HiLo { Hi, Lo, NotSet, Violation }.</returns>
        public static Enums.HiLo BandCheck(Enums.HiLo eBand, double dMidf, double dFreq)
        {
            //...Log2.v(String.Format("\n\nHiloCheckSupp.BandCheck(): Entry: eBand = {0}, dMidf = {1}, dFreq = {2}", eBand, dMidf, dFreq));

            if (dFreq > 0.0)
            {
                /*	Make sure there is a frequency.  */
                if (eBand != Enums.HiLo.Violation)
                {
                    //...Log2.v("\nHiloCheckSupp.BandCheck(): eBand != Enums.HiLo.Violation is TRUE.");

                    if (dFreq >= dMidf)
                    {
                        //...Log2.v("\nHiloCheckSupp.BandCheck(): dFreq >= dMidf");

                        /* High end of the band. */
                        if (eBand != Enums.HiLo.Hi)
                        {
                            if (eBand == Enums.HiLo.NotSet)
                            {
                                eBand = Enums.HiLo.Hi;
                            }
                            else
                            {
                                eBand = Enums.HiLo.Violation;
                            }
                        }
                    }
                    else
                    {
                        //...Log2.v("\nHiloCheckSupp.BandCheck(): dFreq < dMidf");

                        /*	Low end of the band. */
                        if (eBand != Enums.HiLo.Lo)
                        {
                            if (eBand == Enums.HiLo.NotSet)
                            {
                                eBand = Enums.HiLo.Lo;
                            }
                            else
                            {
                                eBand = Enums.HiLo.Violation;
                            }
                        }
                    }
                }
            }
            else
            {
                //...Log2.v("\nHiloCheckSupp.BandCheck(): dFreq <= 0.0 - nothing to be done.");
            }

            //...Log2.v("\nHiloCheckSupp.BandCheck(): Exit: eBand = " + eBand);
            return eBand;
        }


        /// <summary>
        /// This class maintains two lists: one for transmit frequencies and the other for receive frequences.
        /// This method appends a transmit frequency and a receive frequency to these lists.
        /// </summary>
        /// <param name="dTx"> - transmitted frequency.</param>
        /// <param name="dRx"> - received frequency.</param>
        private static void BandListAdd(double dTx, double dRx)
        {
            if (nBandCount >= Constant.BANDLIST_SZ)
            {
                //fprintf(stderr, "\r\n*ERROR* Hilocheck: bandlist overflow. Stopping.\r\n");
                Log2.e("\r\nHiloCheckSupp.BandListAdd(): ERROR: Hilocheck: bandlist overflow. Stopping.\r\n");
                Application.Exit("", 127);
            }
            //...Log2.v(String.Format("\nHiloCheckSupp.BandListAdd(): {0}, freqtx = {1}, freqrx = {2}", nBandCount, dTx, dRx));

            aFreqTx[nBandCount] = dTx;
            aFreqRx[nBandCount] = dRx;
            nBandCount++;
        }

        /// <summary>
        /// Performs a hilo check on one site in the database.
        /// </summary>
        /// <param name="pSite"> - prescribed site to check.</param>
        /// <param name="IsVerbose"> - verbose mode switch.</param>
        /// <param name="cInBand"> - prescribed band.</param>
        /// <param name="eTxHiLo"> - current Tx HiLo status.</param>
        /// <param name="eRxHiLo"> - current Rx HiLo status.</param>
        /// <param name="tw"> - TextWriter object used to output HiLo messages.</param>
        /// <returns></returns>
        /// <para>- non-negative integer - count of the number of HiLo violations detected.</para>
        /// <para>- Constant.FAILURE - attempt to retrieve band from DB failed.</para>
        public static int HiLoSiteBandCheck(MtSiteStr pSite, bool IsVerbose, string cInBand, Enums.HiLo eTxHiLo, Enums.HiLo eRxHiLo, TextWriter tw)
        {
            //...Log2.v(String.Format("\n\nHiloCheckSupp.HiLoSiteBandCheck(): Entry, call1 = {0}", pSite.stSite.call1));

            int nInd = 0;
            int nRet = 0;
            int nErr = 0;
            SuBand pBand;
            MtChan pChan;
            SuBand pThisBand;
            string cBand = "";
            Enums.HiLo eTmpRxHiLo;
            Enums.HiLo eTmpTxHiLo;

            string cTrimName;
            string cTrimOper;

            cTrimName = pSite.stSite.name.Trim();
            cTrimOper = pSite.stSite.oper.Trim();

            tw.Write(String.Format("\r\n\r\n\tDatabase Site: {0} ({1}/{2}) ", pSite.stSite.call1, cTrimName, cTrimOper));
            //...Log2.v(String.Format("\r\n\r\n\tDatabase Site: {0} ({1}/{2}) ", pSite.stSite.call1, cTrimName, cTrimOper));

            /* Set the band codes */
            if (Suutils.SuGetBand(cInBand, out pBand) != 0)
            {   /*	Read in the current band. */
                tw.Write(String.Format("\r\n\tCould not retrieve band {0}", cBand));
                nRet = Constant.FAILURE;
            }
            else
            {
                //	We have the band.  Now we go through all the band and adjacent bands.
                //	The band itself is in the adjacent band list, so we have to skip this one.
                int nCounter = 0;

                cBand = "";

                //AH: Recoded to avoid compiler warning C4706: assignment within conditional expression
                //while (nRet = steptoken(cBand, BNDCDE_SZ, pBand.badj, ";", &nCounter) == 0){
                nRet = GenUtil.StepToken(out cBand, Constant.BNDCDE_SZ, pBand.badj, ";", ref nCounter);

                while (nRet == 0)
                {
                    /*	Check this site's frequencies for each band. */
                    if (String.IsNullOrWhiteSpace(cBand))
                    {
                        //	Sometimes there are too many semicolons
                        //AH: Recoded to avoid compiler warning C4706: assignment within conditional expression
                        nRet = GenUtil.StepToken(out cBand, Constant.BNDCDE_SZ, pBand.badj, ";", ref nCounter);
                        continue;
                    }
                    tw.Write(String.Format("\r\n\tBand {0}", cBand));
                    //...Log2.v(String.Format("\r\n\tBand {0}", cBand));

                    Suutils.SuGetBand(cBand, out pThisBand);
                    if (IsVerbose)
                    {
                        //	Print out the midband frequency
                        tw.Write(String.Format(", midband frequency: {0:0.000}", pThisBand.bmidf));
                        //...Log2.v(String.Format(", midband frequency: {0:0.000}", pThisBand.bmidf));
                    }
                    eTmpRxHiLo = eRxHiLo;
                    eTmpTxHiLo = eTxHiLo;

                    //...Log2.v("\r\nHiloCheckSupp.HiLoSiteBandCheck(): pSite.nNumChans = " + pSite.nNumChans);
                    /*	Go through the channels looking only at the required band */
                    for (nInd = 0; nInd < pSite.nNumChans; nInd++)
                    {
                        //...Log2.v("\r\nHiloCheckSupp.HiLoSiteBandCheck(): nInd = " + nInd);
                        pChan = pSite.stChanPtr[nInd];

                        if (pChan.bndcde.Equals(cBand))
                        {
                            //...Log2.v("\r\nHiloCheckSupp.HiLoSiteBandCheck(): pChan.bndcde.Equals(cBand) = TRUE");
                            /* This is the band */
                            if (IsVerbose)
                            {
                                tw.Write(String.Format("\r\nChannel chid: {0}, Tx freq: {1:0.000}({2}), Rx freq: {3:0.000}({4})",
                                    pChan.chid, pChan.freqtx, pChan.poltx, pChan.freqrx, pChan.polrx));
                                //...Log2.v(String.Format("\r\nChannel chid: {0}, Tx freq: {1:0.000}({2}), Rx freq: {3:0.000}({4})", pChan.chid, pChan.freqtx, pChan.poltx, pChan.freqrx, pChan.polrx));
                            }
                            if (eTmpTxHiLo != Enums.HiLo.Violation)
                            {
                                if (Suutils.HasSubBands(pBand.bndcde))
                                {
                                    //	We handle the case of subbands differently - see the routine.
                                    eTmpTxHiLo = SubBandCheck(eTxHiLo, pThisBand.bmidf, pChan.freqtx, pChan.freqrx);
                                }
                                else
                                {
                                    eTmpTxHiLo = BandCheck(eTxHiLo, pThisBand.bmidf, pChan.freqtx);
                                }
                                if (eTmpTxHiLo == Enums.HiLo.Violation)
                                {
                                    tw.Write(String.Format("\r\n\t{0} to {1} band {2} chid {3} is first TX HiLo violation.",
                                        pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));
                                    //...Log2.v(String.Format("\r\n\t{0} to {1} band {2} chid {3} is first TX HiLo violation.", pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));

                                    nErr++;
                                }

                            }

                            if (eTmpRxHiLo != Enums.HiLo.Violation)
                            {
                                if (Suutils.HasSubBands(pBand.bndcde))
                                {
                                    eTmpRxHiLo = SubBandCheck(eRxHiLo, pThisBand.bmidf, pChan.freqrx, pChan.freqtx);
                                }
                                else
                                {
                                    eTmpRxHiLo = BandCheck(eRxHiLo, pThisBand.bmidf, pChan.freqrx);
                                }
                                if (eTmpRxHiLo == Enums.HiLo.Violation)
                                {
                                    tw.Write(String.Format("\r\n\t{0} to {1} band {2} chid {3} is first RX HiLo violation.",
                                        pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));
                                    //...Log2.v(String.Format("\r\n\t{0} to {1} band {2} chid {3} is first RX HiLo violation.", pChan.call1, pChan.call2, pChan.bndcde, pChan.chid));

                                    nErr++;
                                }
                            }

                            /*	Stop if both tx and rx are in violation  */
                            if (eTmpRxHiLo == Enums.HiLo.Violation && eTmpTxHiLo == Enums.HiLo.Violation)
                            {
                                tw.Write(String.Format("\r\n\t{0} has both tx and rx violations.  Stopping scanning band {1}.",
                                    pChan.call1, pChan.bndcde));
                                //...Log2.v(String.Format("\r\n\t{0} has both tx and rx violations.  Stopping scanning band {1}.", pChan.call1, pChan.bndcde));

                                break;
                            }

                        } // if (pChan.bndcde.Equals(cBand))

                    }  // end for loop

                    if (eTmpRxHiLo == Enums.HiLo.Violation || eTmpTxHiLo == Enums.HiLo.Violation)
                    {
                        tw.Write(String.Format("\r\n\tViolation for Band {0}", cBand));
                        //...Log2.v(String.Format("\r\n\tViolation for Band {0}", cBand));
                    }
                    else
                    {
                        tw.Write(String.Format("\r\n\tBand {0} OK", cBand));
                        //...Log2.v(String.Format("\r\n\tBand {0} OK", cBand));
                    }

                    //AH: Recoded to avoid compiler warning C4706: assignment within conditional expression
                    nRet = GenUtil.StepToken(out cBand, Constant.BNDCDE_SZ, pBand.badj, ";", ref nCounter);

                }  //while

                if (nErr > 0)
                {
                    tw.Write(String.Format("\r\n        Site Has HiLo violation(s)."));
                    //...Log2.v(String.Format("\r\n        Site Has HiLo violation(s)."));

                    if (Suutils.HasSubBands(pBand.bndcde))
                    {
                        tw.Write(String.Format("\r\n          WARNING: the program could report erroneous HiLo violations for one-way channels in the {0} band.", pBand.bndcde));
                        //...Log2.v(String.Format("\r\n          WARNING: the program could report erroneous HiLo violations for one-way channels in the {0} band.", pBand.bndcde));
                    }
                    nRet = 1;
                }
                else
                {
                    tw.Write(String.Format("\r\n        Site OK"));
                    //...Log2.v(String.Format("\r\n        Site OK"));

                    nRet = 0;
                }

            }

            //...Log2.v("\n\nHiloCheckSupp.HiLoSiteBandCheck(): Exit");
            return nRet;
        }

        /// <summary>
        /// Creates a string containing the distance and bndcde conditions for a SQL 'WHERE' clause
        /// for a prescribed site and prescribed HiLo analysis distance from this site;
        /// the 'WHERE' clause includes bitwise AND operations applied to the site's
        /// bandwd1, ... bandwd8 bitmap band indicators such that <b> only the bndcde's used
        /// by pSite and all of their adjacent bands are selected</b> in the WHERE clause.
        /// </summary>
        /// <param name="cSQL"> - distance condition for 'WHERE' clause.</param>
        /// <param name="pSite"> - prescribed site.</param>
        /// <param name="dDistKm"> - prescribed minimum HiLo analysis distance.</param>
        public static void GetCondition(out string cSQL, FtSiteStr pSite, double dDistKm)
        {
            //...Log2.v("\nHiloCheckSupp.GetCondition(): Entry: call1 = " + pSite.stSite.call1);

            // Satisfy 'out' condition.
            cSQL = "";

            string cBandLogic;

            if (pSite != null)
            {
                //	Get the band bits with adjacency.
                FtUtils.GenBandClause(pSite.stSite, out cBandLogic, true);

                cSQL = String.Format("tsip.distance_hs({0}, {1}, latit, longit) <= {2:#0.000} and ({3}) ",
                    pSite.stSite.latit, pSite.stSite.longit, dDistKm, cBandLogic);
            }

            //...Log2.v("\nHiloCheckSupp.GetCondition(): Exit: cSQL = " + cSQL);
            return;
        }




    }
}


```
