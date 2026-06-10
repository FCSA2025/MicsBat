# Documented File: ValPassive.cs
**Repository Path:** `FtValidate\ValPassive.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    using _Auxlib;
    using _Configuration;

    using _NewLib;
    using System.IO;
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
    /// Provides methods related to the validation of passive TS entities.
    /// </summary>
    class ValPassive
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftSetPassiveNormals(string s, ref short sh1, ref short sh2, ref short sh3);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftValSetPassiveTx(string s, ref short sh1, ref short sh2);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int performCalcs([In] string pdfName, [In, Out] FtChan[] chanArray, [In, Out] SQLLEN[][] nullArray, [In] int leftEnd, [In] int rightEnd);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftCalcPassivePwrs([In] string pdfName, [In, Out] ref FtChan curChan, [In, Out] ref SQLLEN[] ftChanNulls);

        public static int FtCalcPassivePwrs_NATIVE(string pdfName, ref FtChan curChan, ref SQLLEN[] ftChanNulls)
        {
            return ftCalcPassivePwrs(pdfName, ref curChan, ref ftChanNulls);
        }
        public static int PerformCalcs_NATIVE(string pdfName, FtChan[] chanArray, SQLLEN[][] nullArray, int leftEnd, int rightEnd)
        {
            int rc = -666;

            rc = performCalcs(pdfName, chanArray, nullArray, leftEnd, rightEnd);

            return rc;
        }
        public static void FtSetPassiveNormals_NATIVE(string s, ref short sh1, ref short sh2, ref short sh3)
        {
            ftSetPassiveNormals(s, ref sh1, ref sh2, ref sh3);
        }

        public static int FtValSetPassiveTx_NATIVE(string s, ref short sh1, ref short sh2)
        {
            return ftValSetPassiveTx(s, ref sh1, ref sh2);
        }

#endif
        //-------------------------------------------------------------------------------------

        private static int GetPaDatInfoCounter = 0;

        //--------------------------------------------------------------------------------------

        /// <summary>
        /// Calculate powers for links with passive antennae. 
        /// </summary>
        /// <remarks>
        /// Calculates the values for the related power
        /// fields in a TS Channel structure in the case where passive
        /// antennae are involved.  Links with passive reflectors are
        /// a special case, and require different calculations than
        /// those which only employ active antennae.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="curChan"> - channel object with power fields populated with calculated values.</param>
        /// <param name="ftChanNulls"> - array of ODBC nullInds for curChan.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtCalcPassivePwrs(string pdfName, ref FtChan curChan, ref SQLLEN[] ftChanNulls)
        {
            //...Log2.v("\n\nValPassive.FtCalcPassivePwrs(): Entry");

            /* Local variables */
            FtChan[] chanArray = new FtChan[Constant.CHANARRAYSIZE];                /* Channels in passive link */
            SQLLEN[][] nullArray = new SQLLEN[Constant.CHANARRAYSIZE][];
            int nInd;
            int nRet;
            int retCode = Constant.SUCCESS;          /* Function return code */
            int leftEnd = Constant.CHANARRAYMID;       /*	Ptr to 'left' chan rec of
						                    *		passive link in array */
            int rightEnd = Constant.CHANARRAYMID;        /*	Ptr to 'right' chan rec of
						                    * 	passive link in array */
            int rc = Constant.SUCCESS;                    /* Function call return code */

            /* Make sure we are dealing with a passive! */
            if ((!curChan.call1.StartsWith("%") && !curChan.call2.StartsWith("%")))
            {
                /* No passives involved - this is an error */
                GenUtil.SetErr("ftCalcPassivePwrs - channel is not a passive channel.");
                //...Log2.v("\n\nValPassive.FtCalcPassivePwrs(): Exit A: channel is not a passive channel");
                return 2;
            }

            // Copy channel record which was passed in for calculations
            // into middle element of array.  This way we will always
            // know where it is !
            chanArray[Constant.CHANARRAYMID] = curChan;
            nullArray[Constant.CHANARRAYMID] = ftChanNulls;

            /* Build the array of channels involved in this link */
            rc = BuildChanArray(pdfName, chanArray, nullArray, Constant.CHANARRAYMID, out leftEnd, out rightEnd);
            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\n\nValPassive.FtCalcPassivePwrs(): Exit B");
                return 3;
            }

            /* If all is well - attempt power calculations */
            if (retCode == Constant.SUCCESS)
            {
                /* Perform power calculations */
                rc = PerformCalcs(pdfName, chanArray, nullArray, leftEnd, rightEnd);
                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\n\nValPassive.FtCalcPassivePwrs(): Exit C");
                    retCode = rc;
                }
            }

            //...Log2.v("\n\nValPassive.FtCalcPassivePwrs(): TRIAD: retCode = " + retCode);

            if (retCode == Constant.SUCCESS)
            {
                /* Calculations are all done OK - copy the desired record
                 * information to return to caller */

                curChan = chanArray[Constant.CHANARRAYMID];

                ftChanNulls = nullArray[Constant.CHANARRAYMID];

                /**********************************************************************\
                *	If all went well we must write out the full passive chain.  This is
                *	the only time we have a chance with the full chain in memory.
                *	We also write out the actives.
                *
                *	1159 -- GJS -- 2005.12.12
                \**********************************************************************/
                for (nInd = leftEnd; nInd <= rightEnd; nInd++)
                {
                    /*  We must go through the channels, updating them  */
                    /*  We also may need to update the command.         */
                    if (chanArray[nInd].cmd.Equals("N"))
                    {
                        chanArray[nInd].cmd = "U";
                    }
                    //...Log2.v("\n\nTEMP: nInd = " + nInd + "\r\n" + chanArray[nInd].KeysToString());
                    nRet = FtReplaceChan(pdfName, chanArray[nInd], nullArray[nInd]);
                }
            }

            //...Log2.v("\n\nValPassive.FtCalcPassivePwrs(): Exit: retCode = " + retCode);
            return (retCode);
        }   /* ----- End of ftCalcPassivePwrs ----- */

        /// <summary>
        /// This method accepts space for an array of channel structures,
        /// which represent a passive link.
        /// </summary>
        /// <remarks>
        /// The element pointed to by
        /// the curChan variable is the 'centre' or starting point for
        /// this routine to scope out the link.  The 'ends' of the link
        /// as found in the array are pointed to by the leftEnd and
        /// and rightEnd variables.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="chanArray"> - array of channel object to be populated with calculated values.</param>
        /// <param name="nullArray"> - 2D array of ODBC nullInds for chanArray.</param>
        /// <param name="curChan"> - index of current (middle) element.</param>
        /// <param name="leftEnd"> - index of left-most defined array element.</param>
        /// <param name="rightEnd"> - index of right-most defined array element.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int BuildChanArray(string pdfName, FtChan[] chanArray, SQLLEN[][] nullArray, int curChan, out int leftEnd, out int rightEnd)
        {
            //...Log2.v("\n\nValPassive.BuildChanArray(): Entry");

            // SQLLEN[] nullArray[CHANARRAYSIZE][FT_CHAN_SIZE_]

            /* Local variables */
            int rc = Constant.SUCCESS;          /* Fctn call ret. code */
            int retCode = Constant.SUCCESS;         /* Fctn return code */


            /*	Start with left and right ends adjacent to curChan */
            leftEnd = Constant.CHANARRAYMID;
            rightEnd = Constant.CHANARRAYMID + 1;
            /* 	Find the next station(s) to the 'right' of current one.  This is going
            *		to be the transmitter, so look for this sites rx freq. */
            rc = FindNextStation(pdfName, ref chanArray, ref nullArray, Constant.CHANARRAYMID,
                                    rightEnd, ref rightEnd,
                                                        chanArray[Constant.CHANARRAYMID].freqrx);
            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\n\nValPassive.BuildChanArray(): Exit A");
                retCode = Constant.FAILURE;
            }
            else if (chanArray[Constant.CHANARRAYMID].call1[0] == '%')
            {
                /*	If this is not an end station itself,
                 *	Find station(s) to the 'left' of current one */
                leftEnd = Constant.CHANARRAYMID - 1;
                rc = FindNextStation(pdfName, ref chanArray, ref nullArray, Constant.CHANARRAYMID, leftEnd, ref leftEnd,
                                        chanArray[Constant.CHANARRAYMID].freqrx);
                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\n\nValPassive.BuildChanArray(): Exit B");
                    retCode = Constant.FAILURE;
                }   /* End if success finding stn's to left */
            }   /* End if success in finding stations to right */

            //...Log2.v("\n\nValPassive.BuildChanArray(): Exit");
            return (retCode);
        }   /* ----- End of buildChanArray ----- */

        /// <summary>
        /// This method finds the corresponding channel record
        /// to that passed into it, at the adjacent station of a
        /// passive link.
        /// </summary>
        /// <remarks>
        /// If the nextChan parameter is greater than
        /// the curChan parameter, the routine assumes you are going
        /// 'to the right' in the array provided as the first parameter.
        /// In this situation, the adjacent station will be the one
        /// defined by the 'call2' field of the record in the chanArray
        /// pointed to by the curChan value.  If the nextChan parameter
        /// is less than the curChan parameter, then the situation is
        /// reversed.  The adjacent channel record(s) will be placed
        /// into the left side of the current channel, in the array.
        /// Note that this routine is designed to be used in a RECURSIVE manner.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="chanArray"> - an array of channel objects.</param>
        /// <param name="chanNulls"> - 2D array of ODBC nullInds for chanArray.</param>
        /// <param name="curChan"> - the index of the current channel in chanArray.</param>
        /// <param name="nextChan"> - indicates where the channel object for the next station is to be placed in chanArray.</param>
        /// <param name="lastChan"> - Indicates where in chanArray that last channel record was 
        /// placed; this is necessary in case we find base condition and don't actually put in
        /// a new record.</param>
        /// <param name="targetFreq"> - the target frequency under consideration.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FindNextStation(string pdfName, ref FtChan[] chanArray, ref SQLLEN[][] chanNulls, int curChan, int nextChan, ref int lastChan, double targetFreq)
        {
            //...Log2.v("\n\nValPassive.FindNextStation(): Entry");

            // SQLLEN[] chanNulls[CHANARRAYSIZE][FtChan.SIZE_]

            /* Local Variables */
            int retCode = Constant.SUCCESS;         /* Function return code */
            int rc = Constant.FAILURE;           /* Function call return code */
            FtChan newChan;         /* Space for found chan rec */
            SQLLEN[] newChanNulls;    //newChanNulls[FtChan.SIZE_];    /* Ingres nulls for chan rec */


            /* Determine if we are at the base condition - ie. is this an active
            ** station?  Passive's have a '%' as their first character.
            ** If going to 'right', then looking for rec with freqtx
            ** same as target frequency.  This is the active xmitter.
            ** If going to 'left', then looking for rec with freqrx
            ** same as target frequency.  This is the active receiver.
            */
            if (!chanArray[curChan].call1.StartsWith("%") && curChan != Constant.CHANARRAYMID)
            {
                /*	Not a passive and not start site, this must be an end base station */
                if (((curChan < nextChan) && (chanArray[curChan].freqtx == targetFreq)) ||
                      ((curChan > nextChan) && (chanArray[curChan].freqrx == targetFreq)))
                {
                    /* Set last channel to current channel - no more added */
                    lastChan = curChan;
                    retCode = Constant.SUCCESS;
                }
                else
                {
                    /*	Base station, but no channel. */
                    // AH: REMOVE!
                    // Previously "E" for "ERROR" now "W" for "Warning".
                    // Unauthorized bug fix on 20210427.
                    ValErrs.AddMess("Passive channel %11.3f missing at base station. For %s into %s, band %s, chid %s.\r\nOccurred at curChan: %d with nextchan: %d Ftx: %11.3f Frx: %11.3f", "", "W",
                                    targetFreq.ToString(), chanArray[curChan].call1,
                                    chanArray[curChan].call2, chanArray[curChan].bndcde,
                                    chanArray[curChan].chid, curChan.ToString(), nextChan.ToString(),
                                    chanArray[curChan].freqtx.ToString(), chanArray[curChan].freqrx.ToString());
                    retCode = -10;
                }
            }
            else
            {
                /* Else not at base condition */
                /* See which way we are going in the array */
                if (curChan < nextChan)
                {
                    /* Going to 'right' */
                    /* Get rec for next station to right */
                    rc = FindRightStation(pdfName, chanArray[curChan], chanNulls[curChan], out newChan, out newChanNulls, targetFreq);
                    if (rc == Constant.SUCCESS)
                    {
                        /* Copy new channel record into the array */
                        chanArray[nextChan] = newChan;

                        chanNulls[nextChan] = newChanNulls;

                        /* Recursively find the next station */
                        rc = FindNextStation(pdfName, ref chanArray, ref chanNulls, nextChan,
                                              nextChan + 1, ref lastChan, targetFreq);
                        if (rc == Constant.SUCCESS)
                        {
                            retCode = Constant.SUCCESS;
                        }
                        else
                        {
                            retCode = rc;
                        }
                    }
                    else
                    {
                        retCode = rc;
                    }
                }
                else
                {
                    /* Going to left in the array */
                    /* Get channel record with call2 = call1 of current
                    ** record.
                    */
                    /* Get rec for next station to left */
                    rc = FindLeftStation(pdfName, chanArray[curChan], chanNulls[curChan], out newChan, out newChanNulls, targetFreq);
                    if (rc == Constant.SUCCESS)
                    {
                        /* Copy new channel record into the array */
                        chanArray[nextChan] = newChan;

                        chanNulls[nextChan] = newChanNulls;

                        /* Recursively find the next station */
                        rc = FindNextStation(pdfName, ref chanArray, ref chanNulls, nextChan,
                                             nextChan - 1, ref lastChan, targetFreq);
                        if (rc == Constant.SUCCESS)
                        {
                            retCode = Constant.SUCCESS;
                        }
                        else
                        {
                            retCode = rc;
                        }
                    }
                    else
                    {
                        retCode = rc;
                    }
                }   /* End else going left in array - toward active rcvr */
            }   /* End else not base condition */

            //...Log2.v("\n\nValPassive.FindNextStation(): Exit: retCode = " + retCode);
            return (retCode);

        }   /* ----- End of findNextStation ----- */

        /// <summary>
        /// Find channel record for next station 'to left'	in a passive link.
        /// </summary>
        /// <remarks>
        /// This method will attempt to find the next station 'to the
        /// left' in a passive link, 'to the left' being defined
        /// elsewhere in this file, but basicly meaning 'toward the
        /// active receiver' in the passive link.  Since there are
        /// some intricacies involved depending upon whether the next
        /// station is another passive, or the active site, the
        /// work has been broken out into this sub function.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="curChan"> - the current channel object.</param>
        /// <param name="curChanNulls"> - array of ODBC nullInds for curChan.</param>
        /// <param name="nextChan"> - the 'next' channel object in the link.</param>
        /// <param name="nextChanNulls"> - array of ODBC nullInds for nextChan.</param>
        /// <param name="targetFreq"> - the target frequency under consideration.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FindLeftStation(string pdfName, FtChan curChan, SQLLEN[] curChanNulls, out FtChan nextChan, out SQLLEN[] nextChanNulls,
                                            double targetFreq)
        {
            //...Log2.v("\n\nValPassive.FindLeftStation(): Entry");

            // SQLLEN[] curChanNulls[FT_CHAN_SIZE_]
            // SQLLEN[] nextChanNulls[FT_CHAN_SIZE_]

            /* Local variables */
            int retCode = Constant.SUCCESS;          /* Fctn ret. code */
            int rc = Constant.SUCCESS;           /* Fctn call ret. code */
            string selCriteria;    /* Rec. selection criteria */

            /* We are looking for 'who listens to what I retransmit'
            ** realizing that I retransmit what I hear from the previous
            ** station in the link.  Since we are always looking for the
            ** RX side of the channel (we are going to the 'left' in this
            ** function - toward the active receiver) we do not have to
            ** deal with the case of the active site in a special way.
            ** Here, we just 'find the receive channel'.
            */
            /* Get channel record with call1 = call2 of current
            ** record */
            selCriteria = String.Format(" call1!='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                                            curChan.call2, curChan.call1, curChan.bndcde, curChan.chid);

            rc = ValGetRecs.GetChanByClause(pdfName, selCriteria, out nextChan, out nextChanNulls);
            if (rc != Constant.SUCCESS)
            {
                retCode = Constant.FAILURE;
            }

            //...Log2.v("\n\nValPassive.FindLeftStation(): Exit: retCode = " + retCode);
            return (retCode);
        }   /* ----- End of findLeftStation ----- */

        /// <summary>
        /// Find channel record for next station 'to right' in a passive link.
        /// </summary>
        /// <remarks>
        /// This method will attempt to find the next station 'to the
        /// right' in a passive link, 'to the right' being defined
        /// elsewhere in this file, but basicly meaning 'toward the
        /// active transmitter' in the passive link.  Since there are
        /// some intricacies involved depending upon whether the next
        /// station is another passive, or the active site, the
        /// work has been broken out into this sub function.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="curChan"> - the current channel object.</param>
        /// <param name="curChanNulls"> - array of ODBC nullInds for curChan.</param>
        /// <param name="nextChan"> - the 'next' channel object in the link.</param>
        /// <param name="nextChanNulls"> - array of ODBC nullInds for nextChan.</param>
        /// <param name="targetFreq"> - the target frequency under consideration.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FindRightStation(string pdfName, FtChan curChan, SQLLEN[] curChanNulls,
                                            out FtChan nextChan, out SQLLEN[] nextChanNulls, double targetFreq)
        {
            //...Log2.v("\n\nValPassive.FindRightStation(): Entry");

            /* Local variables */
            int retCode = Constant.SUCCESS;         /* Fctn ret. code */
            int rc = Constant.SUCCESS;          /* Fctn call ret. code */
            string selCriteria; /* Rec. selection criteria */

            /* If the remote station is a passive, then we want to skip
            ** the remote 'TX' side of the channel, and go straight to the
            ** remote 'RX' side of the channel.  ie)  Where the remote
            ** receives the signal, not where it retransmits it
            */
            if (curChan.call2.StartsWith("%"))
            {
                /* Get channel record with call1 = call2 of current record */
                selCriteria = String.Format(" call1='{0}' and call2!='{1}' and bndcde='{2}' and chid='{3}'",
                                                curChan.call2, curChan.call1, curChan.bndcde, curChan.chid);

                rc = ValGetRecs.GetChanByClause(pdfName, selCriteria, out nextChan, out nextChanNulls);
                if (rc != Constant.SUCCESS)
                {
                    retCode = Constant.FAILURE;
                }

            }
            else
            {   /* We are at the end of the passive link and are dealing with an active station - just get the TX side of it. */
                /* Get channel record with call1 = call2 of current record */
                rc = ValGetRecs.GetChan(pdfName, curChan.call2, curChan.call1, curChan.bndcde,
                               curChan.chid, out nextChan, out nextChanNulls);
                if (rc != Constant.SUCCESS)
                {
                    retCode = Constant.FAILURE;
                }
            }   /* End else */

            //...Log2.v("\n\nValPassive.FindRightStation(): Exit: retCode = " + retCode);
            return (retCode);

        }   /* ----- End of findRightStation ----- */


        /// <summary>
        /// Replaces the channel in the specified pdf; the channel must already be present in the pdf.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="pChan"> - the channel object.</param>
        /// <param name="pChanNulls"> - array of ODBC nullInds for pChan.</param>
        /// <returns> - zero if successful.</returns>
        public static int FtReplaceChan(string pdfName, FtChan pChan, SQLLEN[] pChanNulls)
        {
            //...Log2.v("\n\nValPassive.FtReplaceChan(): Entry");

            int nRet = 0;
            int nHandle;
            string cWhere;
            string cFullName;
            FtChan ftCurrentChan;
            SQLLEN[] aCurrNulls;

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out cFullName);


            cWhere = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}' ",
                                        pChan.call1, pChan.call2, pChan.bndcde, pChan.chid);
            nHandle = DynChannel.FtSelectChannel(cFullName, cWhere, "");
            nRet = DynChannel.FtFetchChannel(nHandle, out ftCurrentChan, out aCurrNulls);
            if (nRet == 0)
            {
                nRet = DynChannel.FtUpdateChannel(nHandle, pChan, pChanNulls);// Update it with the one fed in.
            }

            DynChannel.FtCloseChannel(nHandle);

            //...Log2.v("\n\nValPassive.FtReplaceChan(): Exit");
            return (nRet);
        }

        /// <summary>
        /// This method inputs an array of channel objects, which
        /// represent a passive link; the information is used to calculate
        /// passive antenna rx powers by calling the relevante auxiliary library functions. 
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="chanArray"> - array of channel objects.</param>
        /// <param name="nullArray"> - array of ODBC nullInds for chanArray.</param>
        /// <param name="leftEnd"> - index of the left-most defined array element.</param>
        /// <param name="rightEnd"> - index of the right-most defined array element.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int PerformCalcs(string pdfName, FtChan[] chanArray, SQLLEN[][] nullArray, int leftEnd, int rightEnd)
        {
            //...Log2.v("\n\nValPassive.PerformCalcs(): Entry");

            /* Local variables */
            int numPassives;            /* Number of passives */
            int index = 0;          /* Loop index */
            int retCode = Constant.SUCCESS;     /* Fctn return code */
            int rc = Constant.SUCCESS;          /* Fctn call ret. code */
            double freqMhz;         /* Target freq. in Mhz */
            double[] gain = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] fgain = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] eirp = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] beirp = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] rsl = new double[Constant.MAX_STATIONS];       /* Values from axPasive */
            double[] patlos = new double[Constant.MAX_STATIONS];        /* Values from axPasive */
            double[] vfar = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] dbl = new double[Constant.MAX_STATIONS];       /* Values from axPasive */
            double[,] paDat;                                        /* Passive data table */
            TextWriter nullFile;
            AxStation[] pa = Arrays.CreateArrayUsingDefaultElementConstructor<AxStation>(Constant.MAX_STATIONS);

            /* Fill in paDat structure for later call to axPasive.  This
            ** structure contains antenna height/width information which is
            ** obtained from the antenna codes of the antenna related to each
            ** channel.
            */
            rc = GetPaDatInfo(pdfName, chanArray, nullArray, leftEnd, rightEnd, out paDat);
            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\n\nValPassive.PerformCalcs(): Exit : A : retCode = " + (10 + rc));
                return (10 + rc);
            }
            /* Fill in pa structure with active/passive data.  This
            ** is an array which contains detailed information about
            ** the active and passive sites for use by axPasive.
            */
            rc = GetPaStationData(pdfName, chanArray, nullArray, leftEnd, rightEnd, ref pa);

            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\n\nValPassive.PerformCalcs(): Exit : B : retCode = " + (30 + rc));
                return (30 + rc);
            }

            /* If all is well so far, then call the passive calculation routine
            ** from the auxiliary program suite.
            */
            numPassives = rightEnd - leftEnd - 1;

            /* Have to open /dev/null to shunt output from axPasive off
            ** to a harmless destination
            */
            nullFile = null;

            {
                /* Some initialization first */
                for (index = 0; index <= 5; index++)
                {
                    gain[index] = 0.0;
                    fgain[index] = 0.0;
                    eirp[index] = 0.0;
                    beirp[index] = 0.0;
                    rsl[index] = 0.0;
                    patlos[index] = 0.0;
                    vfar[index] = 0.0;
                    dbl[index] = 0.0;
                }

                // The original native code checked for freqrx and freqtx being zero 
                // (the C code declaration default value) rather than inspecting the
                // associated nullInds. Terrible!
                //...Log2.v("\r\nValPassive.PerformCalcs(): chanArray[Constant.CHANARRAYMID].freqrx = " + chanArray[Constant.CHANARRAYMID].freqrx);
                //freqMhz = (double)(chanArray[Constant.CHANARRAYMID].freqrx / 1000.0);
                //if (freqMhz == 0.0)
                //{
                //    freqMhz = (double)(chanArray[Constant.CHANARRAYMID].freqtx / 1000.0);
                //    if (freqMhz == 0.0)
                //    {
                //        /*	No frequency, no go */
                //        //fclose(nullFile);
                //        return (8);
                //    }
                //}
                if (nullArray[Constant.CHANARRAYMID][FtChan.FREQRX] != Constant.DB_NULL)
                {
                    freqMhz = (double)(chanArray[Constant.CHANARRAYMID].freqrx / 1000.0);
                }
                else if (nullArray[Constant.CHANARRAYMID][FtChan.FREQTX] != Constant.DB_NULL)
                {
                    freqMhz = (double)(chanArray[Constant.CHANARRAYMID].freqtx / 1000.0);
                }
                else
                {
                    /*	No frequency, no go */
                    return (8);
                }

                /* - Call to auxiliary passive calculation routine - */
                //...Log2.v("\r\nValPassive.PerformCalcs(): freqMhz = " + freqMhz);
                rc = Ax14.AxPassive(nullFile, numPassives, freqMhz, paDat, pa, false, gain,
                              fgain, eirp, beirp, rsl, patlos, vfar, dbl);
                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\n\nValPassive.PerformCalcs(): Exit : C : retCode = " + rc);
                    retCode = rc;
                }

            }   /* End else file opened OK */

            /* If all is well, peform power calculations */
            rc = PerformPowerCalcs(chanArray, nullArray, leftEnd, rightEnd, eirp,
                                   beirp, gain, fgain, rsl, patlos, vfar);
            if (rc != Constant.SUCCESS)
            {
                retCode = 100 * retCode + rc;
            }

            /* If the active receiver site is the one we are concerned with
            ** (ie. the one we will pass back to validate with all the calculations
            ** performed), and an rx2 and/or rx3 antenna is in use, then we
            ** must rerun our calculations for these cases.  Remember that the
            ** active reciever in on the left end of the array.
            */

            /* If the 'left' end of the array is the middle element, then we
            ** are doing the calculations for the active receiver.
            */
            if (leftEnd == Constant.CHANARRAYMID)
            {
                /* Is there an antenna #2 ? */
                if (nullArray[Constant.CHANARRAYMID][FtChan.ANTNUMBRX2] != Constant.DB_NULL)
                {
                    /* Call 'package' routine to handle this anomaly */
                    rc = CalcRx2Pwr(pdfName, leftEnd, rightEnd, chanArray, nullArray, paDat, pa);
                    if (rc != Constant.SUCCESS)
                    {
                        retCode = 100 * retCode + 6;
                    }
                }

                /* Is there an antenna #3 ? */
                if (nullArray[Constant.CHANARRAYMID][FtChan.ANTNUMBRX3] != Constant.DB_NULL)
                {
                    /* Call 'package' routine to handle this anomaly */
                    rc = CalcRx3Pwr(pdfName, leftEnd, rightEnd, chanArray, nullArray, paDat, pa);
                    if (rc != Constant.SUCCESS)
                    {
                        retCode = 100 * retCode + 7;
                    }
                }
            }   /* - End if this is an active receiver - */

            //...Log2.v("\n\nValPassive.PerformCalcs(): Exit: retCode = " + retCode);
            return retCode;
        }

        /// <summary>
        /// Performs passive power calculations
        /// </summary>
        /// <remarks>
        /// This method inputs an array of channel objects, that
        /// represent a passive link along with additional information
        /// which is used to calculate the rx and tx power values for
        /// channels on passive sites.
        /// </remarks>
        /// <param name="chanArray"> - array of channel objects.</param>
        /// <param name="nullArray"> - 2D array of ODBC nullInds for chanArray.</param>
        /// <param name="leftEnd"> - index of the left most defined channel object; RX end of channel array.</param>
        /// <param name="rightEnd"> - index of the left most defined channel object; TX end of channel array.</param>
        /// <param name="eirp"> - value from axPasive.</param>
        /// <param name="beirp"> - value from axPasive.</param>
        /// <param name="gain"> - value from axPasive.</param>
        /// <param name="fgain"> - value from axPasive.</param>
        /// <param name="rsl"> - value from axPasive.</param>
        /// <param name="patlos"> - value from axPasive.</param>
        /// <param name="vfar"> - value from axPasive.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int PerformPowerCalcs(FtChan[] chanArray,     /* Channel array */
                                                SQLLEN[][] nullArray,   /* MS SQL Server nulls for channels [CHANARRAYSIZE][FT_CHAN_SIZE_]*/
                                                int leftEnd,			/* RX end of channel array */
                                                int rightEnd,			/* TX end of channel array */
                                                double[] eirp,			/* Values from axPasive */
                                                double[] beirp,			/* Values from axPasive */
                                                double[] gain,			/* Values from axPasive */
                                                double[] fgain,			/* Values from axPasive */
                                                double[] rsl,			/* Values from axPasive */
                                                double[] patlos,		/* Values from axPasive */
                                                double[] vfar           /* Values from axPasive */
                                                        )
        {
            //...Log2.v("\n\nValPassive.PerformPowerCalcs(): Entry");


            /* Local Variables */
            int numPassives;                     /* Number of passives */
            int index;                           /* Loop Index */
            int chanIndex;                       /* Index for channel array */
            int retCode = Constant.SUCCESS;      /* Fctn return code */
            double tempPwr = 0.0;                /* Temp space for pwr calcs */


            /* Calculate number of passives involved */
            numPassives = rightEnd - leftEnd - 1;

            /* We don't calculate anything for the TX end of the link,
            ** only the RX sides of things, since TX power is a given at
            ** the TX end of the link.  The TX end is always on the right,
            ** and correspondingly, the RX end is always on the left of the
            ** channel array.  'Right' is higher array index numbers.
            */

            /* Calculation for active receiver */
            if (chanArray[leftEnd].freqrx > 0)
            {   /*	Only do this if this is an rx chan*/
                chanArray[leftEnd].pwrrx1 = (float)rsl[0];
                nullArray[leftEnd][FtChan.PWRRX1] = Constant.DB_NOT_NULL;
            }

            /* Point channel index to last passive before active RX */
            chanIndex = leftEnd + 1;

            /* Loop through passives to be calculated */
            for (index = 1; index <= numPassives; index++)
            {
                /* Calculate receive power for this passive */
                /* Rem: fgain array is offset by -1 places */
                tempPwr = beirp[index] - (fgain[index - 1] / 2.0);

                /* Set receive */
                if (chanArray[chanIndex].freqrx > 0)
                {
                    chanArray[chanIndex].pwrrx1 = (float)tempPwr;
                    nullArray[chanIndex][FtChan.PWRRX1] = Constant.DB_NOT_NULL;
                }

                /* Increment to next channel record */
                chanIndex++;

            }   /* End for */

            //...Log2.v("\n\nValPassive.PerformPowerCalcs(): Exit");
            return (retCode);

        }   /* ----- End of performPowerCalcs ----- */

        /// <summary>
        /// Performs passive power calculations for rx2 antenna.
        /// </summary>
        /// <remarks>
        /// This method inputs an array of channel objects, which
        /// represent a passive link along with additional information
        /// which is used to calculate the rx2 power value for
        /// channels on passive sites.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="leftEnd"> - index of left most defined array element</param>
        /// <param name="rightEnd"> - index of right most defined array element</param>
        /// <param name="chanArray"> - array of channel objects.</param>
        /// <param name="nullArray"> - 2D array of ODBC nullInds for chanArray.</param>
        /// <param name="paDat"> - 2D paDat array.</param>
        /// <param name="pa"> - array of AxStation objects.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int CalcRx2Pwr(string pdfName,                 /* Name of pdf */
                                        int leftEnd,                    /* Ptr to active RX */
                                        int rightEnd,                   /* Ptr to active TX */
                                        FtChan[] chanArray,             /* Ptr to channel array */
                                        SQLLEN[][] nullArray,           /* Nulls for channel array [CHANARRAYSIZE][FtChan.SIZE_]*/
                                        double[,] paDat,               /* paDat array [MAX_STATIONS][PADATCOLS]*/
                                        AxStation[] pa             	    /* pa structure */
                                    )
        {
            //...Log2.v("\n\nValPassive.CalcRx2Pwr(): Entry");

            /* Local variables */
            int numPassives;                                        /* Number of passives */
            int index = 0;                                          /* Loop index */
            int paDatRow;                                           /* Row ptr for paDat array */
            int paDatCol;                                           /* Col ptr for paDat array */
            int rc = Constant.SUCCESS;                              /* Fctn call ret. code */
            int retCode = Constant.SUCCESS;                         /* Fctn return code */
            double freqMhz;                                         /* Target freq. in Mhz */
            double[] gain = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] fgain = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] eirp = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] beirp = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] rsl = new double[Constant.MAX_STATIONS];       /* Values from axPasive */
            double[] patlos = new double[Constant.MAX_STATIONS];    /* Values from axPasive */
            double[] vfar = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] dbl = new double[Constant.MAX_STATIONS];       /* Values from axPasive */
            //FILE * nullFile;                                      /* Pointer to /dev/null */
            FtAnte ante;                                            /* TS Antenna (PDF or MDB) */
            SQLLEN[] anteNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL); /* TS Antenna nulls */
            double tempFloat = 0.0;                                 /* Temp storage */


            /* Must change some of the values in the paDat array to deal with the
            ** active receiver's rx2 values */

            /* Set to row of active receiver */
            paDatRow = 0;

            /* Gain value for active receiver */
            paDatCol = 1;

            rc = ValGetRecs.GetAnte(pdfName, chanArray[leftEnd].call1, chanArray[leftEnd].call2,
                         chanArray[leftEnd].bndcde, chanArray[leftEnd].antnumbrx2,
                                     out ante, out anteNulls);
            if (rc == Constant.SUCCESS)
            {
                rc = Suutils.GetLocAnteGain(ante.acode, out tempFloat);
            }
            if (rc == Constant.SUCCESS)
            {
                paDat[paDatRow, paDatCol] = tempFloat;
            }
            else
            {
                retCode = Constant.FAILURE;
            }

            /* Loss value for active receiver */
            paDatCol = 2;
            paDat[paDatRow, paDatCol] = chanArray[leftEnd].afslrx2;


            /* If all is well so far, then call the passive calculation routine
            ** from the auxiliary program suite.
            */
            if (retCode == Constant.SUCCESS)
            {
                ///* Have to open /dev/null to shunt output from axPasive off
                //** to a harmless destination */
                //nullFile = fopen("NUL:", "w");
                //if (nullFile == NULL)
                //{
                //    retCode = Constant.FAILURE;
                //}
                //else
                {
                    /* Some initialization first */
                    for (index = 0; index <= 5; index++)
                    {
                        gain[index] = 0.0;
                        fgain[index] = 0.0;
                        eirp[index] = 0.0;
                        beirp[index] = 0.0;
                    }

                    numPassives = rightEnd - leftEnd - 1;
                    freqMhz = (double)((chanArray[Constant.CHANARRAYMID].freqrx) / 1000.0);

                    //...Log2.v("\r\nValPassive.CalcRx2Pwr(): freqMhz = " + freqMhz);
                    rc = Ax14.AxPassive(null, numPassives, freqMhz, paDat, pa, false, gain,
                                  fgain, eirp, beirp, rsl, patlos, vfar, dbl);
                    if (rc != Constant.SUCCESS)
                    {
                        retCode = Constant.FAILURE;
                    }
                }

                //fclose(nullFile);

            }   /* End if */


            /* If all has gone well, then calculate the rx pwr */
            if (retCode == Constant.SUCCESS)
            {
                /* Calculation for active receiver */
                /*chanArray[leftEnd].pwrrx2 = beirp[0];*/
                chanArray[leftEnd].pwrrx2 = (float)rsl[0];
                nullArray[leftEnd][FtChan.PWRRX2] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\n\nValPassive.CalcRx2Pwr(): Exit");
            return (retCode);

        }	/* ----- End of calcRx2Pwr ----- */

        /// <summary>
        /// Performs passive power calculations for rx3 antenna.
        /// </summary>
        /// <param name="pdfName"></param>
        /// <param name="leftEnd"></param>
        /// <param name="rightEnd"></param>
        /// <param name="chanArray"></param>
        /// <param name="nullArray"></param>
        /// <param name="paDat"></param>
        /// <param name="pa"></param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int CalcRx3Pwr(
                                        string pdfName,         /* Name of pdf */
                                        int leftEnd,            /* Ptr to active RX */
                                        int rightEnd,		    /* Ptr to active TX */
                                        FtChan[] chanArray,       /* Ptr to channel array */
                                        SQLLEN[][] nullArray,   /* Nulls for channel array [CHANARRAYSIZE][FT_CHAN_SIZE_] */
                                        double[,] paDat,	    /* paDat array [MAX_STATIONS][PADATCOLS]*/
                                        AxStation[] pa	        /* pa structure [MAX_STATIONS]*/
                                    )
        {
            //...Log2.v("\n\nValPassive.CalcRx3Pwr(): Entry");

            /* Local variables */
            int numPassives;            /* Number of passives */
            int index;          /* Loop index */
            int paDatRow;           /* Row ptr for paDat array */
            int paDatCol;           /* Col ptr for paDat array */
            int rc = Constant.SUCCESS;           /* Fctn call ret. code */
            int retCode = Constant.SUCCESS;      /* Fctn return code */

            double freqMhz;            /* Target freq. in Mhz */
            double[] gain = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] fgain = new double[Constant.MAX_STATIONS];        /* Values from axPasive */
            double[] eirp = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] beirp = new double[Constant.MAX_STATIONS];        /* Values from axPasive */
            double[] rsl = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            double[] patlos = new double[Constant.MAX_STATIONS];       /* Values from axPasive */
            double[] vfar = new double[Constant.MAX_STATIONS];     /* Values from axPasive */
            double[] dbl = new double[Constant.MAX_STATIONS];      /* Values from axPasive */
            //TextWriter nullFile;         /* Pointer to /dev/null */
            FtAnte ante;                /* TS Antenna (PDF or MDB) */
            SQLLEN[] anteNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);   /* TS Antenna nulls */
            double tempFloat;          /* Temp storage */


            /* Must change some of the values in the paDat array to deal with the
            ** active receiver's rx3 values */

            /* Set to row of active receiver */
            paDatRow = 0;

            /* Gain value for active receiver */
            paDatCol = 1;

            rc = ValGetRecs.GetAnte(pdfName, chanArray[leftEnd].call1, chanArray[leftEnd].call2,
                         chanArray[leftEnd].bndcde, chanArray[leftEnd].antnumbrx3,
                                     out ante, out anteNulls);

            if (rc == Constant.SUCCESS)
            {
                rc = Suutils.GetLocAnteGain(ante.acode, out tempFloat);
                paDat[paDatRow, paDatCol] = tempFloat;
            }
            else
            {
                retCode = Constant.FAILURE;
            }

            /* Loss value for active receiver */
            paDatCol = 2;
            paDat[paDatRow, paDatCol] = chanArray[leftEnd].afslrx3;


            /* If all is well so far, then call the passive calculation routine
            ** from the auxiliary program suite.
            */
            if (retCode == Constant.SUCCESS)
            {
                /* Have to open /dev/null to shunt output from axPasive off
                ** to a harmless destination */
                //nullFile = fopen("NUL:", "w");
                //if (nullFile == NULL)
                //{
                //    retCode = Constant.FAILURE;
                //}
                //else
                {
                    /* Some initialization first */
                    for (index = 0; index <= 5; index++)
                    {
                        gain[index] = 0.0;
                        fgain[index] = 0.0;
                        eirp[index] = 0.0;
                        beirp[index] = 0.0;
                    }

                    numPassives = rightEnd - leftEnd - 1;
                    freqMhz = (double)((chanArray[Constant.CHANARRAYMID].freqrx) / 1000.0);

                    //...Log2.v("\r\nValPassive.CalcRx3Pwr(): freqMhz = " + freqMhz);
                    rc = Ax14.AxPassive(null, numPassives, freqMhz, paDat, pa, false, gain,
                                  fgain, eirp, beirp, rsl, patlos, vfar, dbl);
                    if (rc != Constant.SUCCESS)
                    {
                        retCode = Constant.FAILURE;
                    }
                }

                //fclose(nullFile);

            }   /* End if */


            /* If all has gone well, then calculate the rx pwr */
            if (retCode == Constant.SUCCESS)
            {
                /* Calculation for active receiver */
                /*chanArray[leftEnd].pwrrx3 = beirp[0];*/
                chanArray[leftEnd].pwrrx3 = (float)rsl[0];
                nullArray[leftEnd][FtChan.PWRRX3] = Constant.DB_NOT_NULL;
            }

            //...Log2.v("\n\nValPassive.CalcRx3Pwr(): Exit");
            return (retCode);

        }	/* ----- End of calcRx3Pwr ----- */

        /// <summary>
        /// This method returns a 2D array of paDat calculated values for the prescribed array of channel objects.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="chanArray"> - array of channel objects.</param>
        /// <param name="nullArray"> - array of ODBC nullInds for chanArray.</param>
        /// <param name="leftEnd"> - index of the active receiver.</param>
        /// <param name="rightEnd"> - index of the active transmitter.</param>
        /// <param name="paDat"> - 2D array of calculated paDat values for chanArray.</param>
        /// <returns> - Constant.SUCCESS if calculation was successful.</returns>
        public static int GetPaDatInfo(
                                        string pdfName,             /* Name of pdf */
                                        FtChan[] chanArray,     /* Array of channel recs */
                                        SQLLEN[][] nullArray,   /* Nulls for channel array [CHANARRAYSIZE][FT_CHAN_SIZE_]*/
                                        int leftEnd,                /* Ptr - active receiver */
                                        int rightEnd,               /* Ptr - active xmitter */
                                        out double[,] paDat			/* paDat structure [MAX_STATIONS][PADATCOLS]*/
)
        {
            //...Log2.v("\n\nValPassive.GetPaDatInfo(): Entry: " + GetPaDatInfoCounter);

            paDat = new double[Constant.MAX_STATIONS, Constant.PADATCOLS];

            GetPaDatInfoCounter++;

            /* Local variables */
            int rc = 0;                     /* Fctn call ret. code */
            int curChan = 0;                /* Offset into chanArray */
            int loopVar = 0;                /* Loop variable */
            int numPassives = 0;            /* Number of passive's */
            int paDatRow = 0;               /* Row in paDat array */
            int paDatCol = 0;               /* Row in paCol array */

            FtAnte ante;                    /* TS Antenna (PDF or MDB) */
            SQLLEN[] anteNulls;

            double pasHeight;               /* Height - passive ante */
            double pasWidth;                /* Width - passive ante */
            double tempFloat;               /* Temp storage */

            short nAnum;    /*  Antenna number */


            /* Calculate the number of passive stations involved.  The
            ** two stations on the ends of the array are actives.  Must
            ** count only stations between the actives in the array.
            */
            numPassives = rightEnd - leftEnd - 1;

            /* Initialize the paDat array */
            for (paDatRow = 0; paDatRow < Constant.MAX_STATIONS; paDatRow++)
            {
                for (paDatCol = 0; paDatCol < Constant.PADATCOLS; paDatCol++)
                {
                    paDat[paDatRow, paDatCol] = 0.0;
                }
            }


            /* Handle the two active stations on the ends of the array first */
            /* Start with the active receive end.  Info for this will be in
            ** the left end of the channel array. */

            /* -- Get info for the receive (RX) end -- */
            paDatRow = 0;

            /* TX Power value for active receiver */
            paDatCol = 0;
            paDat[paDatRow, paDatCol] = 0.0;

            /* Gain value for active receiver */
            paDatCol = 1;
            //AH: corrected to test nullInd rather than rely on native code default value.
            //if ((nAnum = chanArray[leftEnd].antnumbrx1) == 0)
            nAnum = chanArray[leftEnd].antnumbrx1;
            SQLLEN nullInd = nullArray[leftEnd][FtChan.ANTNUMBRX1];
            if (nullArray[leftEnd][FtChan.ANTNUMBRX1] == Constant.DB_NULL)
            {
                /*  This channel does not have a receive part */
                //AH: corrected to test nullInd rather than rely on native code default value.
                //if ((nAnum = chanArray[leftEnd].antnumbtx1) == 0)
                nAnum = chanArray[leftEnd].antnumbtx1;
                nullInd = nullArray[leftEnd][FtChan.ANTNUMBTX1];
                if (nullArray[leftEnd][FtChan.ANTNUMBTX1] == Constant.DB_NULL)
                {
                    //...Log2.v("\n\nValPassive.GetPaDatInfo(): Exit A: Channel does not have a tx end");
                    return 1;   /*	Channel does not have a tx end */
                }
            }

            // To reach here, either:
            //                        - chanArray[leftEnd][FtChan.ANTNUMBRX1] is not NULL
            //                    OR  - chanArray[leftEnd][FtChan.ANTNUMBTX1] is not null          

            /*	Get the "left-end" antenna record from the ft file and get the gain
            *		for the antenna. */
            //...Log2.v("\r\nC: ANUM = " + nAnum + "   " + nullInd);
            rc = ValGetRecs.GetAnte(pdfName, chanArray[leftEnd].call1, chanArray[leftEnd].call2,
                         chanArray[leftEnd].bndcde, nAnum, out ante, out anteNulls);
            if (rc == 0)
            {
                rc = Suutils.GetLocAnteGain(ante.acode, out tempFloat);
            }
            else
            {
                return 2;
            }

            if (rc == 0)
            {
                paDat[paDatRow, paDatCol] = tempFloat;
                //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = tempFloat; " + paDat[paDatRow, paDatCol]);
            }
            else
            {
                //...Log2.v("\nFtValidate.GetPaDatInfo(): AARDVARK: return 3");
                return 3;
            }

            /* Loss value for active receiver */
            paDatCol = 2;
            //paDat[paDatRow, paDatCol] = chanArray[leftEnd].afslrx1;
            paDat[paDatRow, paDatCol] = NullHelper.ReturnZeroIfNullElseValue(chanArray[leftEnd].afslrx1, nullArray[leftEnd][FtChan.AFSLRX1]);
            //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = chanArray[leftEnd].afslrx1; " + paDat[paDatRow, paDatCol]);

            /* -- Get info for the transmit (TX) end -- */
            paDatRow = numPassives + 1;

            /* Power value for active transmitter */
            paDatCol = 0;
            //paDat[paDatRow, paDatCol] = chanArray[rightEnd].pwrtx;
            paDat[paDatRow, paDatCol] = NullHelper.ReturnZeroIfNullElseValue(chanArray[rightEnd].pwrtx, nullArray[rightEnd][FtChan.PWRTX]);
            //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = chanArray[rightEnd].pwrtx; " + paDat[paDatRow, paDatCol]);

            /* Gain value for active transmitter */
            paDatCol = 1;
            //AH: corrected to test nullInd rather than rely on native code default value.
            //if ((nAnum = chanArray[rightEnd].antnumbtx1) == 0)
            nAnum = chanArray[rightEnd].antnumbtx1;
            if (nullArray[rightEnd][FtChan.ANTNUMBTX1] == Constant.DB_NULL)
            {
                /*  This channel does not have a transmit part */
                //AH: corrected to test nullInd rather than rely on native code default value.
                //if ((nAnum = chanArray[rightEnd].antnumbrx1) == 0)
                nAnum = chanArray[rightEnd].antnumbrx1;
                if (nullArray[rightEnd][FtChan.ANTNUMBRX1] == Constant.DB_NULL)
                {
                    //...Log2.v("\n\nValPassive.GetPaDatInfo(): Exit B");
                    return 4;
                }
            }

            //...Log2.v("\r\nD: ANUM = " + nAnum);
            rc = ValGetRecs.GetAnte(pdfName, chanArray[rightEnd].call1,
                           chanArray[rightEnd].call2, chanArray[rightEnd].bndcde,
                           nAnum, out ante, out anteNulls);
            if (rc == Constant.SUCCESS)
            {
                rc = Suutils.GetLocAnteGain(ante.acode, out tempFloat);
            }
            else
            {
                return 5;
            }
            if (rc == Constant.SUCCESS)
            {
                paDat[paDatRow, paDatCol] = (double)tempFloat;
                //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = (double)tempFloat; " + paDat[paDatRow, paDatCol]);
            }
            else
            {
                //...Log2.v("\nFtValidate.GetPaDatInfo(): BISON: return 6");
                return 6;
            }

            /* Loss value for active transmitter */
            paDatCol = 2;
            //paDat[paDatRow, paDatCol] = chanArray[rightEnd].afsltx1;
            paDat[paDatRow, paDatCol] = NullHelper.ReturnZeroIfNullElseValue(chanArray[rightEnd].afsltx1, nullArray[rightEnd][FtChan.AFSLTX1]);
            //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = chanArray[rightEnd].afsltx1; " + paDat[paDatRow, paDatCol]);

            /* -- Fill in information for passives -- */

            /* For each 'passive channel' record in the array, find its relevant
            ** passive antenna, get the acode, then get the height and width
            ** for that antenna to put into the paDat structure.
            */

            /* Set to paDat row for first passive next to active RX end */
            paDatRow = 1;

            /* Set chanArray pointer to passive next to active RX end */
            curChan = leftEnd + 1;

            for (loopVar = 0; loopVar < numPassives; loopVar++)
            {
                nAnum = chanArray[curChan].antnumbrx1;
                if (nullArray[curChan][FtChan.ANTNUMBRX1] == Constant.DB_NULL)
                {
                    nAnum = chanArray[curChan].antnumbtx1;
                    if (nullArray[curChan][FtChan.ANTNUMBTX1] == Constant.DB_NULL)
                    {
                        //...Log2.v("\n\nValPassive.GetPaDatInfo(): Exit C");
                        return 9;
                    }
                }

                //...Log2.v("\r\nE: ANUM = " + nAnum);
                rc = ValGetRecs.GetAnte(pdfName, chanArray[curChan].call1,
                               chanArray[curChan].call2, chanArray[curChan].bndcde,
                               nAnum, out ante, out anteNulls);
                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\n\nValPassive.GetPaDatInfo(): Exit D");
                    return 7;
                }
                else
                {
                    //...Log2.v("\r\nAlpha");
                    GenUtil.TtPassiveAcode(ante.acode, out pasHeight, out pasWidth);
                    paDatCol = 0;
                    paDat[paDatRow, paDatCol] = pasHeight;
                    //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = pasHeight; " + paDat[paDatRow, paDatCol]);
                    paDatCol = 1;
                    paDat[paDatRow, paDatCol] = pasWidth;
                    //...Log2.v("\r\nValPassive.GetPaDatInfo(): paDat[paDatRow, paDatCol] = pasWidth; " + paDat[paDatRow, paDatCol]);
                }
                /* Point to next paDat row and channel in array */
                paDatRow++;
                curChan++;
            }   /* End for */

            //AH: for testing only.
            //...Log2.v(Arrays.doubleArray2DtoString("\r\nValPassive.GetPaDatInfo(): paDat:", paDat));
            //...Log2.v("\n\nValPassive.GetPaDatInfo(): Exit");
            return 0;

        }   /* ----- End of getPaDatInfo ----- */

        /// <summary>
        /// This method inputs an array of channel objects that
        /// represent a passive link;  it populates the pa array
        /// with values that are used by the AxPassive() method.
        /// <remarks>
        /// Not all of the pa values are filled in as only a few are required
        /// for validation purposes, namely  latSeconds, longSeconds, elevM, antHtM.
        /// </remarks>
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="chanArray"> - array of channel objects.</param>
        /// <param name="nullArray"> - 2D array of ODBC nullInds for chanArray.</param>
        /// <param name="leftEnd"> - index of the left most defined array element.</param>
        /// <param name="rightEnd"> - index of the right most defined array element.</param>
        /// <param name="pa"> - array of AxStation objects.</param>
        /// <returns> - Constant.SUCCESS if no problems encountered.</returns>
        public static int GetPaStationData(string pdfName,              /* Name of pdf */
                                            FtChan[] chanArray,     /* Array of channels */
                                            SQLLEN[][] nullArray,/* Nulls for channels [CHANARRAYSIZE][FT_CHAN_SIZE_]*/
                                            int leftEnd,                /* Ptr to active RX */
                                            int rightEnd,               /* Ptr to active TX */
                                            ref AxStation[] pa  /* Ptr to pa array */
                           )
        {
            //...Log2.v("\n\nValPassive.GetPaStationData(): Entry");

            /* Local variables */
            int numPassives;            /* Number of passives */
            int paIndex = 0;            /* Index into 'pa' array */
            int index = 0;              /* Loop variable */
            int rc = 0;                 /* Fctn call ret. code */

            /* Calculate number of passives involved */
            numPassives = rightEnd - leftEnd - 1;

            /* Start with active stations, RX end first.  RX end is always
            ** on left end of array.  TX is on right end. */

            /* Get RX station data */
            rc = GetStationData(pdfName, chanArray[leftEnd], nullArray[leftEnd], ref pa[0]);
            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\n\nValPassive.GetPaStationData(): Exit A");
                return 1;
            }

            /* Get TX station data */
            rc = GetStationData(pdfName, chanArray[rightEnd], nullArray[rightEnd], ref pa[numPassives + 1]);
            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\n\nValPassive.GetPaStationData(): Exit B");
                return 2;
            }


            /* -- Now handle the passive stations -- */

            /* Set pa index to start of passive data area in array */
            paIndex = 1;

            /* For each passive station in the array (the actives are on the
            ** ends of the array, remember).
            */
            for (index = leftEnd + 1; index < rightEnd; index++)
            {
                rc = GetStationData(pdfName, chanArray[index], nullArray[index], ref pa[paIndex]);
                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\n\nValPassive.GetPaStationData(): Exit C");
                    return 3;
                }
                paIndex++;
            }

            //...Log2.v("\n\nValPassive.GetPaStationData(): Exit");
            return 0;

        }   /* ----- End of getPaStationData ----- */

        /// <summary>
        /// Populates an AxStation with latitude, longitude, height and elevation data for a local site.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="chan"> - channel object</param>
        /// <param name="nullArray"> - array of ODBC nullInds for channel object.</param>
        /// <param name="pa"> - an AxStation object.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int GetStationData(string pdfName,                /* Name of pdf */
                                            FtChan chan,            /* Array of channels */
                                            SQLLEN[] nullArray,     /* Nulls for channels */
                                            ref AxStation pa            /* Ptr to pa structure */
        )
        {
            //...Log2.v("\n\nValPassive.GetStationData(): Entry");

            /* Local variables */
            int
                retCode = Constant.SUCCESS,      /* Fctn return code */
                rc = Constant.SUCCESS;           /* Fctn call ret code */

            SQLLEN[] anteNulls;
            SQLLEN[] siteNulls;
            FtAnte locAnte;         /* Local Antenna record */
            FtSite locSite;         /* Local Site record */

            short nAnum;  /*  Antenna number to retrieve */

            /* Get local antenna and site records */
            //AH: corrected to test nullInd rather than relying on native code default value.
            //if (chan.antnumbtx1 != 0)
            if (nullArray[FtChan.ANTNUMBTX1] != Constant.DB_NULL)
            {
                /*  If a tx antenna exists, we return this --- GJS 10/22/98 */
                nAnum = chan.antnumbtx1;
            }
            else
            {
                //AH: corrected to test nullInd rather than relying on native code default value.
                //if (chan.antnumbrx1 == 0)
                if (nullArray[FtChan.ANTNUMBRX1] == Constant.DB_NULL)
                {
                    //...Log2.v("\n\nValPassive.GetStationData(): Exit A");
                    return (Constant.FAILURE);
                }
                else
                {
                    nAnum = chan.antnumbrx1;
                }
            }
            //...Log2.v("\r\nF: ANUM = " + nAnum);
            rc = ValGetRecs.GetAnte(pdfName, chan.call1, chan.call2, chan.bndcde,
                            nAnum, out locAnte, out anteNulls);

            if (rc == Constant.SUCCESS)
            {
                rc = ValGetRecs.GetSite(pdfName, chan.call1, out locSite, out siteNulls);
                /* Copy required information to pa record */
                pa.LL.latSeconds = (double)(locSite.latit / 100.0);
                pa.LL.longSeconds = (double)(locSite.longit / 100.0);
                pa.elevM = (double)locSite.grnd;
                pa.antHtM = (double)locAnte.aht;

                //...Log2.v("\r\nVapPassive.GetStationData(): pa.LL.latSeconds  = " + pa.LL.latSeconds);
                //...Log2.v("\r\nVapPassive.GetStationData(): pa.LL.longSeconds = " + pa.LL.longSeconds);
                //...Log2.v("\r\nVapPassive.GetStationData(): pa.elevM          = " + pa.elevM);
                //...Log2.v("\r\nVapPassive.GetStationData(): pa.antHtM         = " + pa.antHtM);
            }
            else
            {
                //...Log2.v("\n\nValPassive.GetStationData(): Exit B");
                retCode = Constant.FAILURE;
            }

            //...Log2.v("\n\nValPassive.GetStationData(): Exit");
            return (retCode);

        }   /* ----- End of getStationData ----- */

        /// <summary>
        /// This method sets the normals in billboard passives. The offaxis
        /// fields are used. This is a separate method, called before the channels
        /// are passed, because it requires the antenna calculations.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        public static void FtSetPassiveNormals(string pdfName, ref short errCount, ref short warnCount)
        {
            int nNumSites;
            int nInd;
            int nIndAnt;
            int nRetCode;
            string[] cCallFound = new string[10];
            FtSiteStr tSite;
            FtSiteStrNulls tSiteNulls;

            nNumSites = FtUtils.FtEnumCallSigns(pdfName, out cCallFound);

            for (nInd = 0; nInd < nNumSites; nInd++)
            {
                /*	A passive site has a call sign that starts with %.  These are the
                *		only ones we examine.  */
                if (cCallFound[nInd][0] == '%')
                {
                    /*	it's a passive. Retrieve it.  Note that we do not need a full depth
                    *		of 3, we are only going into the antennas.  We need to bring in the
                    *		Nulls because this is validate and in validate we need to distinguish
                    *		between zero and nulls entered in the input record.  */
                    nRetCode = FtUtils.FtGetSiteWN(cCallFound[nInd], out tSite, 2, pdfName, out tSiteNulls);

                    /*	Check for a billboard passive.  This will be a site with two
                    *		antennas, each one with be one link from the billboard, and the
                    *		value we wish to calculate is halfway between them.  */
                    if (nRetCode == 0)
                    {
                        bool IsChanged = false;

                        /*	We got the site.  Now go through the antennas, for each antenna
                        *		get the 'other' one that represents the other half of the
                        *		bounced path, and calculate the normal as the value between the
                        *		two.  */
                        for (nIndAnt = 0; nIndAnt < tSite.nNumAnts; nIndAnt++)
                        {
                            FtAnte tThisAnt = tSite.stAntsPtr[nIndAnt];
                            FtAnte tThatAnt;
                            int nOtherAnt;

                            /*	Get the band structure.  Because we are doing this for the
                            *		antennas, and we need the frequencies for the passive calcs,
                            *		we use the band to determine the frequency.  */
                            SuBand pBand = null;

                            Suutils.SuGetBand(tThisAnt.bndcde, out pBand);

                            /*	first this must be a billboard passive. */
                            if (FtUtils.IsBillBoard(tThisAnt.acode))
                            {
                                bool IsFound = false;

                                for (nOtherAnt = 0; nOtherAnt < tSite.nNumAnts; nOtherAnt++)
                                {
                                    if (nOtherAnt != nIndAnt)
                                    {
                                        FtSiteStr tOtherEnd;
                                        FtSiteStrNulls tOtherNulls;
                                        int nRet;

                                        /*	Skip itself.  Check if the other one is a billboard
                                        *		(should be the same acode) and same bndcde  */
                                        tThatAnt = tSite.stAntsPtr[nOtherAnt];
                                        if (tThatAnt.acode.Equals(tThisAnt.acode) &&
                                             tThatAnt.bndcde.Equals(tThisAnt.bndcde))
                                        {
                                            /*	We have the other antenna in the passive link pair.
                                            *		Do the calculations.  */
                                            SuAntStr pAntenna;
                                            double dGain = 0.0;
                                            double dHeight = 0.0;
                                            double dWidth = 0.0;
                                            double dPgain = 0.0;
                                            double dIncAngle = 0.0;
                                            double dTrueAz;
                                            double dTrueEl;
                                            float dOldTrueAz;
                                            float dOldTrueEl;

                                            IsFound = true;

                                            tThisAnt.offazm = "P";
                                            dOldTrueAz = tThisAnt.tazmth;   //	Save these to detect if anything changed.
                                            dOldTrueEl = tThisAnt.telvtn;
                                            nRet = Ax14.CalcPassiveNormal(tThisAnt.azmth, tThisAnt.elvtn, tThatAnt.azmth, tThatAnt.elvtn, out dTrueAz, out dTrueEl);
                                            tThisAnt.tazmth = (float)dTrueAz;       /*	Double to float */
                                            tThisAnt.telvtn = (float)dTrueEl;
                                            if (Math.Abs(dOldTrueAz - tThisAnt.tazmth) > 0.005 ||
                                                    Math.Abs(dOldTrueEl - tThisAnt.telvtn) > 0.005)
                                            {
                                                //	There has been a change in the passive geometry.
                                                //...Log2.v("\nValPassive.FtSetPassiveNormals(): before call to GenUtil.CmdMax(): ECHO");
                                                GenUtil.CmdMax(out tThisAnt.cmd, tThisAnt.cmd, "U");
                                                IsChanged = true;
                                            }

                                            if (nRet < 0)
                                            {
                                                /*  The beams are close to or equal to 180 degrees. */
                                                ValErrs.AddMess("SetPassiveNormals - Beams are too close to 180 degrees apart to perform calculations on passive link: %s",
                                                                ValErrs.MakeKeyLine(tThisAnt.call1, tThisAnt.call2, tThisAnt.bndcde, 0, null),
                                                                "E", tThisAnt.call2);
                                                errCount++;
                                            }
                                            else
                                            {
                                                tSiteNulls.anAntsNullPtr[nIndAnt][FtAnte.OFFAZM] = Constant.DB_NOT_NULL;
                                                tSiteNulls.anAntsNullPtr[nIndAnt][FtAnte.TAZMTH] = Constant.DB_NOT_NULL;
                                                tSiteNulls.anAntsNullPtr[nIndAnt][FtAnte.TELVTN] = Constant.DB_NOT_NULL;

                                                /*	To get the antenna gain, we need the gain of the antenna
                                                *		at the other end.  First get the site at the other end */
                                                nRet = FtUtils.FtGetSiteWN(tThisAnt.call2, out tOtherEnd, 2, pdfName, out tOtherNulls);
                                                if (nRet != 0)
                                                {
                                                    ValErrs.AddMess("SetPassiveNormals - Cannot get Other end of passive link, call: %s",
                                                                    ValErrs.MakeKeyLine(tThisAnt.call1, tThisAnt.call2, tThisAnt.bndcde, 0, ""),
                                                                    "E", tThisAnt.call2);
                                                    errCount++;
                                                }
                                                else
                                                {
                                                    /*	We have retrieved the other end of this link.  Now find
                                                    *		the antenna that points back. */
                                                    int nBack;
                                                    FtAnte tBackAnt = null;
                                                    bool IsFound1 = false;

                                                    for (nBack = 0; nBack < tOtherEnd.nNumAnts; nBack++)
                                                    {
                                                        tBackAnt = tOtherEnd.stAntsPtr[nBack];
                                                        if (tBackAnt.call2.Equals(tThisAnt.call1) &&
                                                             tBackAnt.bndcde.Equals(tThisAnt.bndcde))
                                                        {
                                                            IsFound1 = true;
                                                            break;
                                                        }
                                                    }

                                                    if (!IsFound1)
                                                    {
                                                        ValErrs.AddMess("SetPassiveNormals - Cannot get the antenna at the other end of link.",
                                                                        ValErrs.MakeKeyLine(tThisAnt.call1, tThisAnt.call2, tThisAnt.bndcde, 0, ""),
                                                                        "E");
                                                        errCount++;
                                                    }
                                                    else
                                                    {
                                                        /*	We have the other end antenna */
                                                        if (Suutils.SuGetAnt(tBackAnt.acode, out pAntenna) != 0)
                                                        {
                                                            ValErrs.AddMess("SetPassiveNormals - Could not retrieve other end's antenna: code '%s'",
                                                                            ValErrs.MakeKeyLine(tThisAnt.call1, tThisAnt.call2, tThisAnt.bndcde, 0, ""),
                                                                            "E", tBackAnt.acode);
                                                            errCount++;
                                                        }
                                                        else
                                                        {
                                                            dIncAngle = Ax14.IncludedAngle(tThisAnt.tazmth, tThisAnt.telvtn, tThisAnt.azmth, tThisAnt.elvtn);
                                                            //...Log2.v("\r\nBeta");
                                                            GenUtil.TtPassiveAcode(tThisAnt.acode, out dHeight, out dWidth);

                                                            /*	Here we have a problem.  If we are dealing with a
                                                            *		double passive, then the antenna gain at the far
                                                            *		end is not stored in the antenna subsidiary file
                                                            *		and must actually be recalculated.  This
                                                            *		calculation requires the area and included angle
                                                            *		at the far end of the link.  So if the far end is
                                                            *		a billboard, then we must get its included angle*/
                                                            if (FtUtils.IsBillBoard(tBackAnt.acode))
                                                            {
                                                                /*	We have the far site, go through and get the
                                                                *		other half of the passive link.  */
                                                                FtAnte tOtherOther;
                                                                int nOther;
                                                                double dNormalAz;
                                                                double dNormalEl;
                                                                double dOtherIncAngle;
                                                                double dBackHeight;
                                                                double dBackWidth;
                                                                double dBackArea;
                                                                bool IsOtherEndFound = false;

                                                                for (nOther = 0;
                                                                        nOther < tOtherEnd.nNumAnts;
                                                                        nOther++)
                                                                {
                                                                    if (nOther != nBack)
                                                                    {
                                                                        tOtherOther = tOtherEnd.stAntsPtr[nOther];
                                                                        if (tOtherOther.acode.Equals(tBackAnt.acode) &&
                                                                             tOtherOther.bndcde.Equals(tBackAnt.bndcde))
                                                                        {
                                                                            IsOtherEndFound = true;
                                                                            /*	We have found it.  Now calculated the
                                                                            *		included angle.  First the normal at the
                                                                            *		other end of this link  */
                                                                            nRet = Ax14.CalcPassiveNormal(tBackAnt.azmth,
                                                                                                                             tBackAnt.elvtn,
                                                                                                                             tOtherOther.azmth,
                                                                                                                             tOtherOther.elvtn,
                                                                                                                             out dNormalAz,
                                                                                                                             out dNormalEl);
                                                                            dOtherIncAngle = Ax14.IncludedAngle(dNormalAz,
                                                                                                                                        dNormalEl,
                                                                                                                                        tBackAnt.azmth,
                                                                                                                                        tBackAnt.elvtn);
                                                                            /*	Get height, width, and effective area.  */
                                                                            //...Log2.v("\r\nGamma");
                                                                            GenUtil.TtPassiveAcode(tBackAnt.acode,
                                                                                                         out dBackHeight,
                                                                                                        out dBackWidth);
                                                                            dBackArea = Ax14.EffectiveArea(dBackWidth,
                                                                                                                                dBackHeight,
                                                                                                                                dOtherIncAngle);
                                                                            if (Ax14.PassPassGain(dWidth, dHeight, dIncAngle,
                                                                                                             pBand.bmidf / 1000.0,
                                                                                                             tThisAnt.dist, dBackArea,
                                                                                                             out dPgain) != 0)
                                                                            {
                                                                                warnCount++;
                                                                                ValErrs.AddMess("Calculation incorrect on double passive gain when setting passive normals:-\r\n%s",
                                                                                                ValErrs.MakeKeyLine(tThisAnt.call1, tThisAnt.call2,
                                                                                                                        tThisAnt.bndcde, 0, null),
                                                                                                "W",
                                                                                                GenUtil.GetUserMess());
                                                                            }
                                                                        }
                                                                    }
                                                                }

                                                                if (!IsOtherEndFound)
                                                                {
                                                                    errCount++;
                                                                    ValErrs.AddMess("SetPassiveNormals - Other end of passive link not found.",
                                                                                    ValErrs.MakeKeyLine(tBackAnt.call1, tBackAnt.call2,
                                                                                                            tBackAnt.bndcde, 0, null),
                                                                                    "E");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                /*	Normal (non-billboard) remote  */
                                                                dGain = pAntenna.acAnt.again;
                                                                if (Ax14.PassiveGain(dWidth, dHeight, dIncAngle, pBand.bmidf / 1000.0,
                                                                                                tThisAnt.dist, dGain, out dPgain) != 0)
                                                                {
                                                                    warnCount++;
                                                                    ValErrs.AddMess("Calculation incorrect on passive gain when setting passive normals:-\r\n%s",
                                                                                    ValErrs.MakeKeyLine(tBackAnt.call1, tBackAnt.call2,
                                                                                                            tBackAnt.bndcde, 0, ""),
                                                                                    "W",
                                                                                    GenUtil.GetUserMess());
                                                                }
                                                            }
                                                            tThisAnt.tgain = (float)dPgain;
                                                            //...Log2.v("\nValPassive.FtSetPassiveNormals(): before call to GenUtil.CmdMax(): FOXTROT");
                                                            GenUtil.CmdMax(out tThisAnt.cmd, tThisAnt.cmd, "U");

                                                            tSiteNulls.anAntsNullPtr[nIndAnt][FtAnte.TGAIN] = Constant.DB_NOT_NULL;
                                                            tSiteNulls.anAntsNullPtr[nIndAnt][FtAnte.CMD] = Constant.DB_NOT_NULL;
                                                            IsChanged = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }

                                if (!IsFound)
                                {
                                    errCount++;
                                    ValErrs.AddMess("SetPassiveNormals - Could not get other end of Passive link.",
                                                    ValErrs.MakeKeyLine(tThisAnt.call1, tThisAnt.call2, tThisAnt.bndcde, 0, ""),
                                                    "E");
                                }
                            }
                        }

                        if (IsChanged)
                        {
                            /*	If it was changed write it out.  */
                            int nRet = FtUtils.FtPutSiteWN(tSite, tSiteNulls, 2, pdfName);
                            if (nRet != 0)
                            {
                                ValErrs.AddMess("Could not write the normals to the site: %s(%s). Reason: %d",
                                                "", "E", tSite.stSite.name, tSite.stSite.call1, nRet.ToString());
                                errCount++;
                            }
                        }

                    } // if (nRetCode == 0)
                } // if (cCallFound[nInd][0] == '%')
            } // for (nInd = 0; nInd < nNumSites; nInd++)
        } // end of FtSetPassiveNormals()

        /// <summary>
        /// Sets transmitted power levels for channels on passive antennae.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtValSetPassiveTx(string pdfName, ref short errCount, ref short warnCount)
        {
            /* Local variables */

            int cID1;                           /* Channel record handle */
            int retCode = Constant.SUCCESS;     /* Function return code */
            bool notDone;                /* Loop ctrl variable */
            int rc = Constant.SUCCESS;          /* Function call return code */
            string tableName;                   /* Long name - pdf table */
            string keyLine = "";                   /* Line for errors, etc */
            SQLLEN[] nArrayFW;                  /* Nulls for channel rec */
            FtChan ftChan;                      /* Channel record */

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* Get access to channel information */
            cID1 = DynChannel.FtSelectChannel(tableName, "", "");
            if (cID1 < 0)
            {
                ValErrs.AddMess("Could not read channel information, reason %d",
                                            tableName, "W", cID1.ToString());
                warnCount++;
                retCode = Constant.FAILURE;
            }
            else
            {
                notDone = true;
                while (notDone)
                {
                    rc = DynChannel.FtFetchChannel(cID1, out ftChan, out nArrayFW);
                    if (rc == Constant.SUCCESS)
                    {
                        keyLine = String.Format("KEY {0} {1} {2} {3}\r\n",
                                                    ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                        /* Is this channel on a passive reflector? */
                        if ((ftChan.call1.StartsWith("%")))
                        {
                            /* Set reflected TX power based upon RX
                            ** calculated power of this record. */
                            rc = SetReflectedPwr(pdfName, ftChan);
                            if (rc != Constant.SUCCESS)
                            {
                                retCode = Constant.FAILURE;
                                errCount++;
                                ValErrs.AddMess("Could not set passive tx power. Please ensure that the entire passive link has been entered in the PDF",
                                                            keyLine, "W");
                            }
                        }
                    }
                    else    /* Fetch not successful */
                    {
                        /* If not just end of records, then error */
                        if (rc != Constant.NOMORERECS)
                        {
                            retCode = Constant.FAILURE;
                            errCount++;
                            ValErrs.AddMess("Could not fetch channel record. Reason %d.", keyLine, "E", rc.ToString());
                        }
                        notDone = false;
                    }

                }   /* End while */

                /* Close the cursor - done with it */
                DynChannel.FtCloseChannel(cID1);
            }

            return (retCode);

        }   /* ----- End of ftValSetPassiveTx ----- */

        /// <summary>
        /// This method inputs the name of a pdf that requires
        /// the tx power to be set on the tx reflection of the rx
        /// channel on a passive antenna.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="ftChan"> - the channel object involved.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int SetReflectedPwr(string pdfName, FtChan ftChan)
        {
            int retCode = Constant.SUCCESS;     /* Fctn return code */
            string chanTable;       /* Name of chan table (pdf) */
            string whereClause; /* SQL where clause buffer */

            int nRet;
            int nHandle;
            FtChan ftChandb;
            SQLLEN[] ftChanNulls;

            /* Get full name of this channel table */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out chanTable);

            /* Set up where clause to find correct record to update */
            whereClause = String.Format("call1='{0}' and call2!='{1}' and bndcde='{2}' and chid='{3}'",
                                            ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

            nHandle = DynChannel.FtSelectChannel(chanTable, whereClause, "");

            nRet = DynChannel.FtFetchChannel(nHandle, out ftChandb, out ftChanNulls);

            if (nRet == 0)
            {
                ftChandb.pwrtx = ftChan.pwrrx1;  // Set the outgoing tx power to incoming rx. 
                ftChanNulls[FtChan.PWRTX] = Constant.DB_NOT_NULL;
                retCode = DynChannel.FtUpdateChannel(nHandle, ftChandb, ftChanNulls);
            }
            DynChannel.FtCloseChannel(nHandle);

            return (retCode);

        }	/* ----- End of setReflectedPwr ----- */


    }
}

```
