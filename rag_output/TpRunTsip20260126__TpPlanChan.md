# Documented File: TpPlanChan.cs
**Repository Path:** `TpRunTsip20260126\TpPlanChan.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TpRunTsip
{
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
    /// Provides methods to generate the channels for a plan.
    /// </summary>
    public class TpPlanChan
    {
        private static string[] mPolarity = new string[4] { "VVHH", "HHVV", "VHHV", "HVVH" };

        private static string mCall1;
        private static string mCall2;
        private static string mBndcde;
        private static float[] mFreq = new float[50];

        //-----------------------------------------------------------------------

        /// <summary>
        /// This is the principal method for the generation of channels; its
        /// function is to oversee the generation of channel records for PLAN mode
        /// analysis.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>The first step is to copy the PDF over to a temporary PDF.</item>
        /// <item>For convenience of testing, we also ensure that a title record is
        /// included when the temporary PDF is created.</item>
        /// <item>Two channel cursors are opened, one for the old PDF and one for the new.</item> 
        /// <item>For each channel record in the old PDF, we check if we are dealing with a new
        /// call1-call2 pair, and reset the chid (which becomes the seed for the
        /// new channel ids, which are always preceded by 'pl').</item> 
        /// <item>After checking that hl is valid, we intialise the structure which serves as the
        /// basis for new channels, and then add those channels (at least, those
        /// which are valid) to the temporary PDF.</item> 
        /// <item>If any of this fails, the method returns an error condition.</item>
        /// </list>
        /// </remarks>
        /// <param name="tpParm"> - prescribed TpParm object.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS:	method exited normally.</para>
        /// <para> - Constant.FAILURE:	invalid fields?</para>
        public static int GenChan(TpParm tpParm)
        {
            /* Local variables */
            NewChan[] newChan;  /* [Constant.MAXPLANPTS] 99 plans max 			*/
            int row;                                            /* counter for # channels 							*/
            string call1, call2;/* present call1 and call2*/
            string bndcde;              /* band code 														*/
            string fwName;          /* source PDF name											*/
            string tempName;   /* temporary PDF name										*/
            string intTableNm; /* full table names for channel tables	*/
            FtChan ftChan;                /* space for Channel record 						*/
            SQLLEN[] chanNullInd; /* [FtChan.NUM_COLUMNS] null indicators for channel 	*/
            int chanHandle;                             /* Handle for source channel table			*/
            int newChanHandle;                      /* Handle for temporary channel table		*/
            int chid = 0;                                           /* integer portion of plxx chids				*/
            int rc;

            fwName = tpParm.proname.Trim();
            tempName = String.Format("pl_{0,13}", tpParm.proname);
            tpParm.proname = tempName;


            Ssutil.UtDropTable(Constant.FT, tempName);

            /* COPY THE PDF TO THE NEW PDF HERE */
            if ((rc = Ssutil.UtCopyTable(Constant.FT, fwName, tempName, false)) != Constant.SUCCESS)
            {
                ErrMsg.UtPrintMessage(rc);
                return rc;
            }


            Ssutil.CreateNewTitleRec(Constant.FT_TITL, tempName);

            /*
             * Now we open two cursors, one for the new PDF, one for the old. The
             * order in which these are opened is important, since the value of
             * intTableNm is used later in the call to addChannels, and must
             * contain the name of the old PDF.
             */
            /* Convert temp PDF name to internal Channel table name */
            GenUtil.UtCvtName(Constant.FT_CHAN, tempName, out intTableNm);

            newChanHandle = DynChannel.FtSelectChannel(intTableNm, null, "call1, call2, bndcde, chid");
            if (newChanHandle < 0)
            {
                ErrMsg.UtPrintMessage(newChanHandle);
                return (newChanHandle);
            }

            /* Convert PDF name to internal Channel table name */
            GenUtil.UtCvtName(Constant.FT_CHAN, fwName, out intTableNm);
            chanHandle = DynChannel.FtSelectChannel(intTableNm, null, "call1, call2, bndcde, chid");
            if (chanHandle < 0)
            {
                ErrMsg.UtPrintMessage(chanHandle);
                return (chanHandle);
            }

            /* Get the channel record from the screen */

            /*
             * Initialise the static variables in validateFreq, and the variables
             * used to determine if a new call1/call2/bandcode set is being used.
             */
            ValidateFreq(null, null, 0, 0);
            call1 = "";
            call2 = "";
            bndcde = "";

            /* START CHANNEL CURSOR LOOP HERE  */
            while (DynChannel.FtFetchChannel(chanHandle, out ftChan, out chanNullInd) == Constant.SUCCESS)
            {
                if (!call1.Equals(ftChan.call1) || !call2.Equals(ftChan.call2) || !bndcde.Equals(ftChan.bndcde))
                {
                    call1 = ftChan.call1;
                    call2 = ftChan.call2;
                    bndcde = ftChan.bndcde;
                    chid = 1;
                }

                if ((ftChan.hl <= 0) || (ftChan.hl > 6))
                {
                    /* Invalid or unknown Hl code */
                    ErrMsg.UtPrintMessage(Error.INVHILO);
                    DynChannel.FtCloseChannel(chanHandle);
                    DynChannel.FtCloseChannel(newChanHandle);
                    chanHandle = Constant.DB_NULL;
                    return (Constant.FAILURE);
                }


                /*
                 * Create an array of details for generated channels, which will be
                 * combined with the details from the existing channels in addChannels
                 * to create full channel records in the temporary PDF
                 */
                if (InitialiseNewChan(ftChan.bndcde, ftChan.splan, ftChan.hl, ftChan.vh,
                                                out row, out newChan) != Constant.SUCCESS)
                {
                    ErrMsg.UtPrintMessage(Error.INVBAND);
                    DynChannel.FtCloseChannel(chanHandle);
                    DynChannel.FtCloseChannel(newChanHandle);
                    chanHandle = Constant.DB_NULL;
                    return (Constant.FAILURE);
                }

                if (AddChannels(intTableNm, newChanHandle, ftChan, chanNullInd,
                                 newChan, row, ref chid) == Constant.FAILURE)
                {
                    DynChannel.FtCloseChannel(chanHandle);
                    DynChannel.FtCloseChannel(newChanHandle);
                    return Constant.FAILURE;
                }
            }

            DynChannel.FtCloseChannel(chanHandle);
            DynChannel.FtCloseChannel(newChanHandle);

            //...Log2.v("\nTpPlanChan.GetChan(): succeeded");
            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method uses the current bndcde & plan to calculate the channel 
        /// records with this plan. These values can then be entered into an internal table 
        /// that will be the basis for the new channels.  
        /// </summary>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="pplan"> - plan name.</param>
        /// <param name="hl"> - hilo case indicator.</param>
        /// <param name="vh"> - polarization case indicator.</param>
        /// <param name="row"> - row indicator for newChan struct</param>
        /// <param name="newChan"> - initialized NewChan object.</param>
        /// <returns></returns>
        public static int InitialiseNewChan(string bndcde,      /* band code */
                                            string pplan,       /* plan name */
                                            short hl,           /* hilo */
                                            short vh,
                                            out int row,        /* row indicator for newChan struct */
                                            out NewChan[] newChan)    /* newChan Struct */
        {
            // out
            row = 0;
            newChan = Arrays.CreateArrayUsingDefaultElementConstructor<NewChan>(Constant.MAXPLANPTS);

            /* Local variables */
            int i, j;              /* loop counters */
            SdPlan sdPlan;  /* Space for BAND/PLAN row */
            SdPlnd[] sdPlndArray;
            int nNumData;

            int nRet;

            /* First initialise the internal structure */
            // ps = NewChan;


            for (i = 0; i < newChan.Length; i++)
            {
                newChan[i].rxInd = Constant.DB_NULL;
                newChan[i].txInd = Constant.DB_NULL;
                newChan[i].chidInd = Constant.DB_NULL;
            }

            nRet = Suutils.SdGetPlan(pplan, bndcde, out sdPlan, out sdPlndArray, out nNumData);

            // Now initialise the row variable
            row = 0;

            // The following loop will initialise the internal newChan structure
            // with all of the automatically generated channels. The channels
            // rx & tx frequencies come from the plan.  After each set of
            // frequencies is extracted bump the newChan struct to the next entry.
            for (i = 0, j = 0; i < nNumData; i++)
            {
                SdPlnd plan = sdPlndArray[i];

                switch (hl)
                {
                    case 1:
                        newChan[j].txfreq = (double)plan.set1;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 1, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set2;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 2, out newChan[j].polrx);

                        break;

                    case 2:
                        newChan[j].txfreq = (double)plan.set2;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 2, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set1;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 1, out newChan[j].polrx);
                        break;

                    case 3:
                        newChan[j].txfreq = (double)plan.set3;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 3, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set4;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 4, out newChan[j].polrx);
                        break;

                    case 4:
                        newChan[j].txfreq = (double)plan.set4;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 4, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set3;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 3, out newChan[j].polrx);
                        break;

                    case 5:
                        newChan[j].txfreq = (double)plan.set1;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 1, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set2;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 2, out newChan[j].polrx);

                        j++;
                        row++;

                        newChan[j].txfreq = (double)plan.set3;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 3, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set4;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 4, out newChan[j].polrx);
                        break;

                    case 6:
                        newChan[j].txfreq = (double)plan.set2;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 2, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set1;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 1, out newChan[j].polrx);

                        j++;
                        row++;

                        newChan[j].txfreq = (double)plan.set4;
                        newChan[j].txInd = Constant.DB_NOT_NULL;
                        newChan[j].txstatus = "1";
                        /* Set the TX polarization */
                        GetPol(vh, 4, out newChan[j].poltx);

                        newChan[j].rxfreq = (double)plan.set3;
                        newChan[j].rxInd = Constant.DB_NOT_NULL;
                        newChan[j].rxstatus = "1";
                        /* Set the RX polarization */
                        GetPol(vh, 3, out newChan[j].polrx);
                        break;

                    default:
                        break;
                }

                j++;
                row++;

            }

            return 0;
        }

        /// <summary>
        /// This method determines the channel's polarization for the prescribed vh and SET indicators.
        /// (See page B-72 of TSIP reference Guide.) 
        /// </summary>
        /// <param name="vh"> - polarization code indicator.</param>
        /// <param name="set"> - selects the character position within a 4-char polarization code.</param>
        /// <param name="pol"></param>
        public static void GetPol(short vh, int set, out string pol)
        {
            // vh  = 1, 2, 3, or 4
            // set = 1, 2, 3, or 4.
            if (vh <= 0 || vh > 4 || set <= 0 || set > 4)
            {
                /* Invalid combination. Should be impossible to get here! */
                pol = " ";
            }
            else
            {
                // mPolarity[4] = { "VVHH", "HHVV", "VHHV", "HVVH" }
                pol = mPolarity[vh - 1].Substring(set - 1, 1);
            }
        }

        /// <summary>
        /// This method checks whether a given set of frequencies have already 
        /// been used as TX or RX in a channel record for this {call1, call2, band code} 
        /// in this PDF or in the original channels of the old PDF.  
        /// </summary>
        /// <param name="fwName"> - name of PDF.</param>
        /// <param name="chan"> - FtChan object.</param>
        /// <param name="freqtx"> - 2 band plan transmit frequency.</param>
        /// <param name="freqrx"> - 2 band plan receive frequency.</param>
        /// <returns></returns>
        public static int ValidateFreq(string fwName,    /* PDF name */
                          FtChan chan,     /* chan structure */
                          double freqtx,    /* 2 band plan frequencies */
                          double freqrx)
        {
            string searchClause;

            int i;
            int ret = Constant.SUCCESS;

            if (String.IsNullOrWhiteSpace(fwName))
            {
                mCall1 = "";
                mCall2 = "";
                mBndcde = "";

                for (i = 0; i < 50; i++)
                {
                    mFreq[i] = 0.0f;
                }

                return Constant.SUCCESS;
            }

            if (!mCall1.Equals(chan.call1) || !mCall2.Equals(chan.call2) || !mBndcde.Equals(chan.bndcde))
            {
                mCall1 = chan.call1;
                mCall2 = chan.call2;
                mBndcde = chan.bndcde;
                i = 0;
            }
            else
            {
                for (i = 0; i < 50 && mFreq[i] > 0.0; i++)
                {
                    if (freqtx == mFreq[i] || freqrx == mFreq[i])
                    {
                        return Error.FREQUENCYUSED;
                    }
                }
            }

            if (freqtx > 0.0) mFreq[i++] = (float)freqtx;
            if (freqrx > 0.0) mFreq[i++] = (float)freqrx;
            mFreq[i++] = 0.0f;

            /* set up search clause to search PDF */

            searchClause = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (freqtx = {3} or freqtx = {4} or freqrx = {5} or freqrx = {6})",
                                            chan.call1, chan.call2, chan.bndcde, freqtx, freqrx, freqtx, freqrx);

            ret = Ssutil.DbCountRows(fwName, searchClause);

            return (ret);
        }

        /// <summary>
        /// This method adds the channels to the temporary PDF. 
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>In order for TX 
        /// channels to be added, the txpow field must be defined.</item> 
        /// <item>For RX channels, the 
        /// antnumbrx1 must exist.</item> 
        /// <item>Finally, validateFreq checks that the frequency has 
        /// not been used already, and that is does not already exist in the original 
        /// channels. </item>
        /// <item>If all of these tests are passed, the channel information from 
        /// the generated channel info is combined with the original channel data to 
        /// create new records which are added to the channel table of the temporary 
        /// PDF.</item>
        /// </list>
        /// </remarks>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="chanHandle"> - handle (index) of a cursor previously returned by a call to DynChannel.FtSelectChannel().</param>
        /// <param name="chan"> - FtChan object.</param>
        /// <param name="nullsChan"> - ODBC nullInds associated with chan.</param>
        /// <param name="newChans"> - array of NewChan objects.</param>
        /// <param name="rows"> - number of array elements in newChans.</param>
        /// <param name="chid"> - channel ID.</param>
        /// <returns>Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int AddChannels(string pdfName,
                                        int chanHandle,
                                        FtChan chan,                    /* channel structure */
                                        SQLLEN[] nullsChan,
                                        NewChan[] newChans,  /* newChan structure*/
                                        int rows,				        /* rows indicator */
                                    ref int chid)
        {
            int recCount;
            int i, rc;

            recCount = 0;

            for (i = 0; i <= rows; i++)
            {
                NewChan newChanStruct = newChans[i];
                //AH: CHECK THIS HERE!
                if (((newChanStruct.rxInd != Constant.DB_NULL && nullsChan[FtChan.ANTNUMBRX1] != Constant.DB_NULL) ||
                     (newChanStruct.txInd != Constant.DB_NULL && nullsChan[FtChan.PWRTX] != Constant.DB_NULL)) &&
                          (ValidateFreq(pdfName, chan, newChanStruct.txfreq, newChanStruct.rxfreq) == 0))
                {
                    /* extract the information from the structure */

                    chan.chid = String.Format("pl{0:00}", chid); // "%02d" format = zero fill, minimum of 2 digits, integer
                    chid++;
                    newChanStruct.chidInd = Constant.DB_NOT_NULL;
                    if (newChanStruct.txInd != Constant.DB_NULL &&
                          nullsChan[FtChan.PWRTX] != Constant.DB_NULL)
                    {
                        /* Tx part of channel is defined */
                        chan.freqtx = newChanStruct.txfreq;
                        nullsChan[FtChan.FREQTX] = Constant.DB_NOT_NULL;
                        chan.stattx = newChanStruct.txstatus;
                        nullsChan[FtChan.STATTX] = Constant.DB_NOT_NULL;
                        chan.poltx = newChanStruct.poltx;
                        nullsChan[FtChan.POLTX] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        /* Come here if RX only channel */
                        nullsChan[FtChan.FREQTX] = Constant.DB_NULL;
                        nullsChan[FtChan.STATTX] = Constant.DB_NULL;
                        nullsChan[FtChan.POLTX] = Constant.DB_NULL;
                    }

                    if (newChanStruct.rxInd != Constant.DB_NULL &&
                            nullsChan[FtChan.ANTNUMBRX1] != Constant.DB_NULL)
                    {
                        /* Rx part of channel is defined */
                        chan.freqrx = newChanStruct.rxfreq;
                        nullsChan[FtChan.FREQRX] = Constant.DB_NOT_NULL;
                        chan.statrx = newChanStruct.rxstatus;
                        nullsChan[FtChan.STATRX] = Constant.DB_NOT_NULL;
                        chan.polrx = newChanStruct.polrx;
                        nullsChan[FtChan.POLRX] = Constant.DB_NOT_NULL;
                    }
                    else
                    {
                        /* Come here if TX only channel */
                        nullsChan[FtChan.FREQRX] = Constant.DB_NULL;
                        nullsChan[FtChan.STATRX] = Constant.DB_NULL;
                        nullsChan[FtChan.POLRX] = Constant.DB_NULL;
                    }

                    rc = DynChannel.FtInsertChannel(chanHandle, chan, nullsChan);
                    if (rc == Constant.SUCCESS)
                    {
                        /* Insert worked ok */
                        recCount++;     /* bump records added */
                    }
                    else
                    {
                        /* Insert failed */
                        ErrMsg.UtPrintMessage(rc);
                        return (Constant.FAILURE);
                    }
                }

            } // for (i = 0; i <= rows; i++)

            return (recCount);
        }







    }
}

```
