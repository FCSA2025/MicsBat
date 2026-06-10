# Documented File: AnteDel.cs
**Repository Path:** `FtValidate\AnteDel.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _Configuration;
using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Utillib;

namespace FtValidate
{
    using SQLLEN = Int64;

    /// <summary>
    /// The static methods in this class are used to validate an Antenna Delete.
    /// It enforces the rule that an antenna can be delete only if there are no
    /// channels which refer to it.
    /// </summary>
    public class AnteDel
    {

        private static string searchParams = new string(new char[1024]);
        private static ChanSubData[] channels = Arrays.CreateArrayUsingDefaultElementConstructor<ChanSubData>(DefineConstants.MAX_CHANNELS + 1); // the stack
        private static int nextFreeElement; // top of stack

        /// <summary>
        /// Ensurer that the antenna which is about to be delete is not being used by 
        /// any existing channels.
        /// </summary>
        /// <remarks>
        /// This code handles the case where channels exist in the MDB but the user
        /// is deleting or updating these channel in the SAME PDF so that there are
        /// no more references to the antenna by channels.
        /// 
        /// The logic is as follows:
        /// 
        /// - get all the channels which reference tha antenna from the PDF;
        /// - place these channels in a data structure of some kind (modified stack);
        /// - get all the channels which reference the antenna from the MDB;
        /// - if the channel is not already in the stack then add the channel;
        ///   (obtained from the MDB) to the stack;
        /// - search the stack for references to the Antenna (disregarding any DELETE operations.
        /// </remarks>
        /// <param name="pdfName">short form of PDF name</param>
        /// <param name="call1">antenna's call1</param>
        /// <param name="call2">antenna's call2</param>
        /// <param name="bndcde">antenna's band code</param>
        /// <param name="anum">antenna number</param>
        /// <returns>Number of channels which are referencing tha antenna.</returns>
        public static int FtValidateAnteDelete(ref string pdfName, ref string call1, ref string call2, ref string bndcde, short anum)
        {
            //...Log2.v("\nAnteDel.FtValidateAnteDelete(): Entry: " + String.Format("{0}, {1}, {2}, {3}, {4}",pdfName, call1, call2, bndcde, anum));

            /* Local variables */
            string errCall1 = new string(new char[DefineConstants.CALL_SZ]); // call1 of channel in error
            string errCall2 = new string(new char[DefineConstants.CALL_SZ]); // call2 of channel in error
            string chid = new string(new char[Constant.CHID_SZ]); // channel id of channel in error
            int numErrors = 0; // number of bad channels
            bool anteKeyPrinted = false; // flag for "antenna key printed?"
            string keyLine = new string(new char[64]);

            /* Local function declaration */
            ChanListReset(); // reset in core list of channels


            /* Get all channels which refer to antenna, from the PDF. Place on the
             * stack.
             */
            if (GetAnteChannelsPDF(ref pdfName, ref call1, ref call2, ref bndcde, anum) == DefineConstants.STACK_OVERFLOW)
            {
                ValErrs.AddMess("Too many PDF channels per antenna", pdfName, "E");
                //...Log2.v("\nAnteDel.FtValidateAnteDelete(): Return: A");
                return (1);
            }

            /* Get all channels which refer to antenna, from the MDB. Place on the
             * stack.
             */
            if (GetAnteChannelsMDB(ref pdfName, ref call1, ref call2, ref bndcde, anum) == DefineConstants.STACK_OVERFLOW)
            {
                ValErrs.AddMess("Too many MDB channels per antenna", pdfName, "E");
                //...Log2.v("\nAnteDel.FtValidateAnteDelete(): Return: B");
                return (1);
            }

            /* get all channels (from the stack) which are referenced by the
             * antenna in question.
             */
            while (PopToRefAntenna(anum, ref errCall1, ref errCall2, ref chid) > 0)
            {
                /*  report error; */
                if (!anteKeyPrinted)
                {
                    keyLine = ValErrs.MakeKeyLine(call1, call2, bndcde, anum, null);

                    ValErrs.AddMess("Cannot delete Antenna with channels. The channels are:", keyLine, "E");
                    anteKeyPrinted = true;
                    numErrors++; //    Only count this as one error.

                    //...Log2.v("\n\nAnteDel.FtValidateAnteDelete(): Cannot delete Antenna with channels. The channels are:\n" + Environment.StackTrace);
                }

                ValErrs.AddMess("Channel: %s -> %s %s %d %s:", keyLine, "E", errCall1, errCall2, bndcde, anum.ToString(), chid);
            }

            //...Log2.v("\nAnteDel.FtValidateAnteDelete(): Exit: " + String.Format("{0}", numErrors));
            return (numErrors);

        } // ----- End of ftValidateAnteDelete -----

        /// <summary>
        /// Searches the PDF's database FT_CHAN table to find all channels 
        /// that refer to a prescribed antenna, retrieves theme as FtChan objects and
        /// pushes (adds) derived channel information on to a stack.
        /// </summary>
        /// <param name="pdf"> - name of PDF.</param>
        /// <param name="call1"> - call1.</param>
        /// <param name="call2"> - call2.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        private static int GetAnteChannelsPDF(ref string pdf, ref string call1, ref string call2, ref string bndcde, short anum)
        {
            //...Log2.v("\nAnteDel.GetAnteChannelsPDF(): Entry");

            /* Local variables */
            FtChan ftChan = new FtChan(); // current channel record
            SQLLEN[] nullIndChan; // null ind. for chan fields
            int chanHandle; // channel handle for dynamic
            string pdfName = new string(new char[Constant.TABLE_NM_SZ]); // chan. table name- long form
            string whereClause = new string(new char[DefineConstants.SEL_CRITERIA_SZ]); // space for where clause
            int rc = Constant.SUCCESS; // return code
                                       //int checkremote = Constant.SUCCESS;


            /* Construct the SQL select statement to select all channels which
              * reference the antenna in question.
              */
            whereClause = string.Format("call1='{0}' and call2='{1}' and bndcde='{2}' and " + "(antnumbtx1={3:D} or antnumbtx2={4:D} or antnumbrx1 = {5:D} or " + "antnumbrx2={6:D} or antnumbrx3={7:D})", call1, call2, bndcde, anum, anum, anum, anum, anum);

            /* Open a cursor on the PDF */
            GenUtil.UtCvtName(Constant.FT_CHAN, pdf, out pdfName);

            /* Setup to select channels from PDF. */
            if ((chanHandle = DynChannel.FtSelectChannel(pdfName, whereClause, "")) < 0)
            {
                return (chanHandle);
            }

            /* Read channel information */
            while (DynChannel.FtFetchChannel(chanHandle, out ftChan, out nullIndChan) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                /* Come here with channel that refers to antenna */
                if (nullIndChan[FtChan.ANTNUMBTX1] == Constant.DB_NULL)
                {
                    ftChan.antnumbtx1 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBTX2] == Constant.DB_NULL)
                {
                    ftChan.antnumbtx2 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBRX1] == Constant.DB_NULL)
                {
                    ftChan.antnumbrx1 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBRX2] == Constant.DB_NULL)
                {
                    ftChan.antnumbrx2 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBRX3] == Constant.DB_NULL)
                {
                    ftChan.antnumbrx3 = DefineConstants.NO_ANTENNA;
                }

                /* add channel to list of channels */
                rc = PushChan(ftChan.cmd, ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid, ftChan.antnumbtx1, ftChan.antnumbtx2, ftChan.antnumbrx1, ftChan.antnumbrx2, ftChan.antnumbrx3);

                if (rc != 0)
                {
                    break;
                }

            } // End while

            DynChannel.FtCloseChannel(chanHandle);

            //...Log2.v("\nAnteDel.GetAnteChannelsPDF(): Exit");
            return (rc);

        } // ----- End of getAnteChannelsPDF -----


        /// <summary>
        /// Searches the master database <b>main.mt_chan</b> to find all channels 
        /// that refer to a prescribed antenna, retrieves theme as MtChan objects and
        /// pushes (adds) derived channel information on to a stack.
        /// </summary>
        /// <param name="pdfName"> - name of PDF.</param>
        /// <param name="call1"> - call1.</param>
        /// <param name="call2"> - call2.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        private static int GetAnteChannelsMDB(ref string pdfName, ref string call1, ref string call2, ref string bndcde, short anum)
        {
            //...Log2.v("\nAnteDel.GetAnteChannelsMDB(): Entry");

            MtChan mtChan = new MtChan(); // local channel record
            SQLLEN[] nullInd = NullHelper.CreateArrayOfNullInd(Constant.MT_CHAN_SIZE_, NullHelper.ColumnStatus.NULL); // null indicators for rx ant.
            MtChan mtChanRem;
            SQLLEN[] nullIndRem;
            string chid = new string(new char[Constant.CHID_SZ]);
            int rc = 0; // return code
            int ret = Constant.SUCCESS;
            int nHandle = 0;
            int existsinPDF = 1; /* return code from isMDBRecInPDF
																 TRUE if MDB record is in PDF
						                     FALSE if MDB record is not in PDF */


            /* Construct selection criteria for all channels using the antenna.
             */
            searchParams = string.Format("call1='{0}' and call2='{1}' and bndcde='{2}' and (antnumbtx1={3:D} or antnumbtx2={4:D} or antnumbrx1 = {5:D} or antnumbrx2={6:D} or antnumbrx3={7:D})", call1, call2, bndcde, anum, anum, anum, anum, anum);

            /* get all channels using this antenna */
            // exec sql open cursorMdbChans for readonly;
            nHandle = DynMdbChannel.MtSelectChannel(searchParams, null);
            if (nHandle < 0)
            {
                Log2.e("\r\nAnteDel.getAnteChannelsMDB(): ERROR: handle = " + nHandle);
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            rc = DynMdbChannel.MtFetchChannel(nHandle, out mtChan, out nullInd);
            // exec sql fetch cursorMdbChans into :mtChan:nullInd;

            while (rc == Constant.SUCCESS)
            {

                if (nullInd[MtChan.ANTNUMBTX1] == Constant.DB_NULL)
                {
                    mtChan.antnumbtx1 = DefineConstants.NO_ANTENNA;
                }

                if (nullInd[MtChan.ANTNUMBTX2] == Constant.DB_NULL)
                {
                    mtChan.antnumbtx2 = DefineConstants.NO_ANTENNA;
                }

                if (nullInd[MtChan.ANTNUMBRX1] == Constant.DB_NULL)
                {
                    mtChan.antnumbrx1 = DefineConstants.NO_ANTENNA;
                }

                if (nullInd[MtChan.ANTNUMBRX2] == Constant.DB_NULL)
                {
                    mtChan.antnumbrx2 = DefineConstants.NO_ANTENNA;
                }

                if (nullInd[MtChan.ANTNUMBRX3] == Constant.DB_NULL)
                {
                    mtChan.antnumbrx3 = DefineConstants.NO_ANTENNA;
                }


                /* isMDBRecInPDF checks to see if each selected record from the MDB is
                *  in the PDF.  If the record is in the PDF we want to be sure that
                *  if it is marked as 'U'pdate its antenna number is being changed
                *  in this way the channel will no longer be part of the antenna that
                *  is being deleted */

                chid = mtChan.chid;
                existsinPDF = IsMDBRecInPDF(ref pdfName, ref call1, ref call2, ref bndcde, ref chid, anum);

                if (existsinPDF == 0)
                {
                    /* add channel to list of channels */
                    string str = " ";
                    rc = PushChan(str, mtChan.call1, mtChan.call2, mtChan.bndcde, mtChan.chid, mtChan.antnumbtx1, mtChan.antnumbtx2, mtChan.antnumbrx1, mtChan.antnumbrx2, mtChan.antnumbrx3);
                }

                /* execute the same instructions but for the remote side of the channel */

                ret = GetRemoteMdbChannel(mtChan, out mtChanRem, out nullIndRem);

                if (ret != Constant.SUCCESS)
                {
                    /* utPrintMessage(GENERROR,
                        "Could not find remote chan"); */
                    ValErrs.AddMess("Remote channel not found", pdfName, "E");

                    /* This is a crummy way to handle this error, but we
                    ** must return something to the caller so that he
                    ** knows to increment the error count. */
                    ret = DefineConstants.STACK_OVERFLOW;
                    break;
                }

                // exec sql fetch cursorMdbChans into :mtChan:nullInd;
                rc = DynMdbChannel.MtFetchChannel(nHandle, out mtChan, out nullInd);

            }  // while
            DynMdbChannel.MtCloseChannel(nHandle);

            //...Log2.v("\nAnteDel.GetAnteChannelsMDB(): Exit");
            return (ret);

        } // ----- End of getAnteChannelsMDB -----

        /// <summary>
        /// Pushes (add) channel information onto a stack if it does not not already exist there.
        /// </summary>
        /// <remarks>
        /// The channel-information stack keep track of all channels
        /// associated with an antenna.
        /// 
        /// The code implements the stack using an array. An element is pushed onto the
        /// stack if it is not already in the stack.
        /// 
        /// This piece of code is designed to be use in the following way:
        /// <list type="bullet">
        /// <item>Add all the channels contained in the PDF associated with
        /// a specific antenna.</item>
        /// <item>Next add all the channels from the MDB associated with the
        /// antenna. Remember that our modified stack will not add an
        /// element if it already exists in the stack therefore we end up
        /// with the channels the way they would appear after the updates
        /// specified in the PDF are performed. (Basically we are doing a
        /// look ahead).</item>
        /// <item>After this is done the caller uses 'popToRefAntenna' to pop off
        /// everything up to an including the first channel record which
        /// references the antenna in which we are interested. The caller
        /// keeps doing this until 0 is returned.</item>
        /// </list>
        /// </remarks>
        /// <param name="cmd"></param>
        /// <param name="call1"></param>
        /// <param name="call2"></param>
        /// <param name="bndcde"></param>
        /// <param name="chid"></param>
        /// <param name="tx1"></param>
        /// <param name="tx2"></param>
        /// <param name="rx1"></param>
        /// <param name="rx2"></param>
        /// <param name="rx3"></param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        private static int PushChan(string cmd, string call1, string call2, string bndcde, string chid, short tx1, short tx2, short rx1, short rx2, short rx3)
        {
            //...Log2.v("\nAnteDel.PushChan(): Entry");

            /* Local variables */
            ChanSubData chan; // current chan. in stack


            /* check for stack overflow */
            if (nextFreeElement >= DefineConstants.MAX_CHANNELS)
            {
                /* Error. Too many channels */
                Log2.e("\n\nAnteDel.PushChan(): ERROR: no remaining free slots in the PushChan() stack.");
                return (DefineConstants.STACK_OVERFLOW);
            }

            chan = channels[nextFreeElement]; // top of stack

            /* Add element to stack */
            chan.cmd = cmd;
            chan.call1 = call1;
            chan.call2 = call2;
            chan.bndcde = bndcde;
            chan.chid = chid;
            chan.tx1 = tx1;
            chan.tx2 = tx2;
            chan.rx1 = rx1;
            chan.rx2 = rx2;
            chan.rx3 = rx3;

            //...Log2.v("\nAnteDel.PushChan(): chan object:\n\n" + chan.ToStringSingleLine() + "\n\n");

            /* Perform "Quick Search".
             * At this point we have added the channel to the top of the stack but
             * we have not yet moved the "top of Stack" pointer.
                 * We must now determine if this channel is already in the stack. To do
              * this we perform an "endless" loop examining each element in the stack
             * looking for a match on the channel keys. WE WILL ALWAYS FIND A MATCH
             * since the channel for which we are searching is always at the top of
             * the stack. When we MATCH we terminate the "endless loop".
             *
             * If the match was on the top of the stack then we know the channel
             * was not previously in the stack and we simply bump the top of stack
             * pointer.
             */

            //AH: 
            //chan = channels[0]; // bottom of stack
            int index = 0;
            while (true)
            {
                chan = channels[index];
                if ((string.Compare(call1, chan.call1) == 0) && (string.Compare(call2, chan.call2) == 0) && (string.Compare(chid, chan.chid) == 0))
                {
                    /* Come here if keys match */
                    break;
                }

                index++; // next elem in stack
            }

            if (chan == channels[nextFreeElement])
            {
                /* matched on last element therefore not already in list */
                //...Log2.v("\nAnteDel.PushChan(): item placed on top of the stack; pointer incremented");
                nextFreeElement++; // bump top of stack
            }
            else
            {
                //...Log2.v("\nAnteDel.PushChan(): item already on the stack; pointer NOT incremented");
            }

            //...Log2.v("\nAnteDel.PushChan(): Exit");
            return (0);

        } // ----- End of pushChan -----


        /// <summary>
        /// Resets (clears, empties) the stack of channel information.
        /// </summary>
        private static void ChanListReset()
        {
            //...Log2.v("\nAnteDel(): Entry");

            nextFreeElement = 0; // make stack empty

            //...Log2.v("\nAnteDel(): Exit");
        }

        /// <summary>
        /// Pops (retrieves) all channel information from the top of the stack up 
        /// to and including the first channel that references a prescribed antenna.
        /// The method then outputs the key information (call1, call2 and chid)) of 
        /// the channel that references the antenna so that the caller can identify 
        /// that channel there is a need to report an error.
        /// </summary>
        /// <param name="anum"> - antenna number.</param>
        /// <param name="call1"> - call1.</param>
        /// <param name="call2"> - call2.</param>
        /// <param name="chid"> - channel ID.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        public static int PopToRefAntenna(short anum, ref string call1, ref string call2, ref string chid)
        {
            //...Log2.v("\nAnteDel.PopToRefAntenna(): Entry");

            ChanSubData chan;   /* current channel in stack */

            chan = channels[nextFreeElement];  /* point to top of stack */

            while (nextFreeElement-- > 0)
            {
                /* Come here if stack not empty */
                chan = channels[nextFreeElement];

                if ((chan.cmd != null && chan.cmd.Length > 0 && chan.cmd[0] != 'D') &&
                    (chan.tx1 == anum ||
                        chan.tx2 == anum ||
                        chan.rx1 == anum ||
                        chan.rx2 == anum ||
                        chan.rx3 == anum))
                {
                    /* Antenna is being referenced. Return key to caller. */
                    call1 = chan.call1;

                    call2 = chan.call2;

                    chid = chan.chid;

                    //...Log2.v("\nAnteDel.PopToRefAntenna(): Return: A");
                    return (1);
                }
            }

            /* Come here if antenna is not referenced */
            ChanListReset();        /* clear stack */

            //...Log2.v("\nAnteDel.PopToRefAntenna(): Exit");
            return (0);
        }

        /// <summary>
        /// Identifies those channel records in the PDF that were on
        /// an antenna that is being deleted but have now been changed to another
        /// antenna at the same site. For each record that matches the selection
        /// criteria in GetAnteChannelsMDB(), an equivalent record is seached for in
        /// the PDF.  If the same record is found in the PDF, a test is performed
        /// to see if the PDF record is being updated to a new antenna.  If this
        /// is the case, there is no need to bring in the old MDB record (which
        /// would cause a 'antenna cannot be deleted' error since the old record
        /// is still attached to the antenna.
        /// </summary>
        /// <param name="pdf"> - name of the PDF.</param>
        /// <param name="call1"> - call1.</param>
        /// <param name="call2"> - call2.</param>
        /// <param name="bndcde"> - band code.</param>
        /// <param name="chid"> - channel ID.</param>
        /// <param name="anum"> - antenna number.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        private static int IsMDBRecInPDF(ref string pdf, ref string call1, ref string call2, ref string bndcde, ref string chid, short anum)
        {
            int rc = 0; // return code

            string pdfName = new string(new char[Constant.TABLE_NM_SZ]);
            FtChan ftChan = new FtChan();
            SQLLEN[] nullIndChan;
            string selCriteria = new string(new char[1024]); // selection criteria
            int nHandle;
            int nRet = 0;

            GenUtil.UtCvtName(Constant.FT_CHAN, pdf, out pdfName);

            /* Construct selection criteria for all channels using the antenna.  */

            selCriteria = string.Format("call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'", call1, call2, bndcde, chid);

            //sprintf(selStmt,
            //				"select * from %s where %s",
            //				pdfName, selCriteria);

            //exec sql execute immediate :selStmt into :ftChan:nullIndChan;
            //	Select and read the (only) channel.
            nHandle = DynChannel.FtSelectChannel(pdfName, selCriteria, null);
            nRet = DynChannel.FtFetchChannel(nHandle, out ftChan, out nullIndChan);
            if (nRet != Constant.SUCCESS)
            {
                if (nRet == Constant.NOMORERECS)
                {
                    rc = 0;
                }
                else
                {
                    rc = Error.DYN_MS_SQL_SERVER_ERR;
                    Log2.e("\r\nAnteDel.isMDBRecInPDF(): ERROR: FtFetchChannel() FAIL");
                    ValErrs.AddMess("isMDBRecInPDF: Could not read: %s", pdfName, "E");
                }
            }
            else
            {
                DynChannel.FtCloseChannel(nHandle);
                /* Come here with channel that refers to antenna */
                if (nullIndChan[FtChan.ANTNUMBTX1] == Constant.DB_NULL)
                {
                    ftChan.antnumbtx1 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBTX2] == Constant.DB_NULL)
                {
                    ftChan.antnumbtx2 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBRX1] == Constant.DB_NULL)
                {
                    ftChan.antnumbrx1 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBRX2] == Constant.DB_NULL)
                {
                    ftChan.antnumbrx2 = DefineConstants.NO_ANTENNA;
                }

                if (nullIndChan[FtChan.ANTNUMBRX3] == Constant.DB_NULL)
                {
                    ftChan.antnumbrx3 = DefineConstants.NO_ANTENNA;
                }

                /* add channel to list of channels */
                if (((ftChan.CmdEquals('U') || (ftChan.CmdEquals('B')) && ((ftChan.antnumbtx1 != anum) && (ftChan.antnumbtx2 != anum) && (ftChan.antnumbrx1 != anum) && (ftChan.antnumbrx2 != anum) && (ftChan.antnumbrx3 != anum)))))
                {
                    rc = 1;
                }
                else
                {
                    rc = 0;
                }
            }

            return (rc);

        } // ----- End of isMDBRecInPDF -----

        /// <summary>
        /// For a given channel record determine its remote searching the master table <b>main.mt_chan</b>.
        /// </summary>
        /// <param name="locChan"> - prescribed local MtChan object.</param>
        /// <param name="remChan"> - found remote MtChan object.</param>
        /// <param name="nullChan"> - array of ODBC nullInds for remChan.</param>
        /// <returns></returns>
        /// <para> - Constant.SUCCESS - the method succeeded.</para>
        /// <para> - Any other value - the attempt failed.</para>
        private static int GetRemoteMdbChannel(MtChan locChan, out MtChan remChan, out SQLLEN[] nullChan)
        {
            //...Log2.v("\nAnteDel.GetRemoteMdbChannel(): Entry");

            int ret = Constant.SUCCESS;
            string whereClause = new string(new char[DefineConstants.SEL_CRITERIA_SZ]);
            int nHandle;

            /* Construct selection criteria for remote channel
              * local antenna.
             */

            whereClause = string.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid ='{3}'", locChan.call2, locChan.call1, locChan.bndcde, locChan.chid);

            /* Execute the statement */
            //exec sql execute immediate :whereClause into :mtChan:nullRemChan;
            nHandle = DynMdbChannel.MtSelectChannel(whereClause, null);

            ret = DynMdbChannel.MtFetchChannel(nHandle, out remChan, out nullChan);
            if ((ret != Constant.SUCCESS) && (ret != Constant.NOMORERECS))
            {
                ret = Constant.FAILURE;
            }

            DynMdbChannel.MtCloseChannel(nHandle);

            //...Log2.v("\nAnteDel.GetRemoteMdbChannel(): Exit");
            return (ret);

        } // ----- End of getRemoteMdbChannel -----

        /// <summary>
        /// This private class only has scope within the FtValidate assembly and is used as a 
        /// convenient way to group a set of constant values together.
        /// </summary>
        private static class DefineConstants
        {
            public const int TX1 = 0; // null ind. offset for antnumbtx1
            public const int TX2 = 1; // null ind. offset for antnumbtx2
            public const int RX1 = 2; // null ind. offset for antnumbrx1
            public const int RX2 = 3; // null ind. offset for antnumbrx2
            public const int RX3 = 4; // null ind. offset for antnumbrx3
            public const int MAX_CHANNELS = 250;
            public const int MAX_ANTENNA_CHAN = 5; // max num of antennae per channel
            public const int CMD_SZ = 2;
            public const int CALL_SZ = 10;
            public const int SEL_CRITERIA_SZ = 2000; // selection criteria max size
            public const int NO_ANTENNA = -1;
            public const int STACK_OVERFLOW = -1; // too many channels for antenna
        }

    }
}

```
