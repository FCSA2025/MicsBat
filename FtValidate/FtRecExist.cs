using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// Provides methhods that check whether particular types of records exist within the database.
    /// </summary>
    public class FtRecExist
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int checkFictCall([In] string pdfName, [In] string call1);

        //-------------------------------------------------------------------------------
        public static int CheckFictCall_NATIVE(string pdfName, string call1)
        {
            return checkFictCall(pdfName, call1);
        }
#endif

        /// <summary>
        /// This method ensures that a '=' call sign has a transmitting antenna.
        /// </summary>
        /// <remarks>
        /// This method will determine if a site with a '=' call1 has a    
        /// transmitting antenna.  If it doesn't there is a problem since a '='  
        /// indicates a transmitting or transmitting/receiving antenna.
        /// </remarks>
        /// <param name="pdfName">short form of PDF name.</param>
        /// <param name="call">call1 for a site.</param>
        /// <returns>Constant.SUCCESS or Constant.FAILURE</returns>
        public static int CheckFictCall(string pdfName, string call)
        {
            //...Log2.v("\n\nFtRecExist.CheckFictCall(): Entry");

            int ret;        /* return code */
            int rc;
            int pdfcheck;   /* check within PDFs to see if channel has changed */
            int addcheck;   /* check to see if channel record exists in PDF */
            int count = 0;  /* counter to see if channel record exists in MDB */

            int chanHandle; /* dynamic cursor handles */
            int mtChanHandle;
            int sID1;

            SQLLEN[] nullIndChan;
            SQLLEN[] nArrayFW;
            SQLLEN[] mtChanNulls;

            string tableName;

            FtChan tempChan;        /* temporary structures */
            MtChan mtChan;
            FtChng ftChng;

            string siteCall;
            string whereClause;
            string selectionCriteria;

            ret = Constant.FAILURE;     /* set return code to failure ...code must */
            pdfcheck = Constant.FAILURE;    /* verify that this condition does not exist */
            addcheck = Constant.FAILURE;

            siteCall = call;
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* check to see if a TX frequency is being added 
               this would mean that a '=' call sign is allowed */

            whereClause = String.Format("call1 = '{0}' and (cmd = '{1}' or cmd = '{2}')",
                                            siteCall, "A", "B");

            if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            while (DynChannel.FtFetchChannel(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
            {
                addcheck = Constant.SUCCESS;
                if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                {
                    ret = Constant.SUCCESS; /* no error will be reported */
                }
            }
            DynChannel.FtCloseChannel(chanHandle);


            /* if no channel is being added or blanked, check MDB for channels
               that exist already that have a TX frequency */

            selectionCriteria = String.Format("call1 = '{0}'", siteCall);

            if (ret != Constant.SUCCESS)
            {

                if ((mtChanHandle = DynMdbChannel.MtSelectChannel(selectionCriteria, "")) < 0)
                {
                    /* error selecting mdb site table */
                    return (mtChanHandle);
                }

                while ((rc = DynMdbChannel.MtFetchChannel(mtChanHandle, out mtChan, out mtChanNulls)) == Constant.SUCCESS)
                {
                    count = 1;
                    /* this counter identifies that there is at
                       least one channel record */

                    pdfcheck = Constant.FAILURE;

                    if (mtChanNulls[MtChan.FREQTX] != Constant.DB_NULL)
                    {
                        /* found a TX frequency - now make sure that 
                           it will stay a TX frequency after the mdb 
                           is updated (ie. is there a delete or blank 
                           record in the PDF) */

                        pdfcheck = Constant.SUCCESS;
                        /* PDF does not change MDB record */

                        whereClause = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}' and cmd = '{4}'",
                                        mtChan.call1, mtChan.call2, mtChan.bndcde,
                                        mtChan.chid, "D");
                        /* check for record deletions in the PDF */

                        if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynChannel.FtFetchChannel(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            pdfcheck = Constant.FAILURE;
                            /* PDF changes MDB record */
                        }

                        DynChannel.FtCloseChannel(chanHandle);


                        whereClause = String.Format("call1 = '{0}' and call2 ='{1}' and bndcde = '{2}' and chid = '{3}' and cmd = '{4}'",
                                        mtChan.call1, mtChan.call2, mtChan.bndcde,
                                        mtChan.chid, "B");
                        /* check for record blanks in the MDB */

                        if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynChannel.FtFetchChannel(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            if (nullIndChan[FtChan.FREQTX] == Constant.DB_NULL)
                            {
                                pdfcheck = Constant.FAILURE;
                                /* PDF changes MDB */
                            }
                        }

                        DynChannel.FtCloseChannel(chanHandle);
                    }
                    else
                    {
                        /* did not find TX frequency - now make sure 
                        that it stays a non-TX record after the 
                        mdb is updated (ie. is there an update or blank 
                           record in the PDF) */

                        whereClause = String.Format("call1 = '{0}' and call2 ='{1}' and bndcde = '{2}' and chid = '{3}' and cmd = '{4}'",
                                        mtChan.call1, mtChan.call2, mtChan.bndcde,
                                        mtChan.chid, "U");

                        if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynChannel.FtFetchChannel(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                            {
                                pdfcheck = Constant.SUCCESS;
                            }
                        }

                        DynChannel.FtCloseChannel(chanHandle);


                        whereClause = String.Format("call1 = '{0}' and call2 ='{1}' and bndcde = '{2}' and chid = '{3}' and cmd = '{4}'",
                                        mtChan.call1, mtChan.call2, mtChan.bndcde,
                                        mtChan.chid, "B");

                        if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                        {
                            return (Error.DYN_MS_SQL_SERVER_ERR);
                        }

                        while (DynChannel.FtFetchChannel(chanHandle, out tempChan, out nullIndChan) == Constant.SUCCESS)
                        {
                            if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                            {
                                pdfcheck = Constant.SUCCESS;
                            }
                        }

                        DynChannel.FtCloseChannel(chanHandle);
                    }
                    if (pdfcheck == Constant.SUCCESS)
                    {
                        /* the MDB stays a TX record or becomes
                           a TX record after the update is 
                           performed */
                        ret = Constant.SUCCESS;
                    }
                }
                DynMdbChannel.MtCloseChannel(mtChanHandle);

                if ((count == 0) && (addcheck != Constant.SUCCESS))
                {
                    ret = Constant.SUCCESS; /* no channels being added 
					  its just a site and/or antenna */
                }
            }

            if (ret != Constant.SUCCESS)
            {
                /* check to see if the call sign is being changed to a
                   non-TX value.  If it is, then consider this fictitious
                   call sign as being O.K. */

                /* set up the Ingres cursor */
                GenUtil.UtCvtName(Constant.FT_CHNG_CALL, pdfName, out tableName);

                whereClause = String.Format("oldcall1='{0}'", siteCall);

                /* select change call sign records from PDF*/
                if ((sID1 = DynChange.FtSelectChngCall(tableName, whereClause, "")) >= 0)
                {
                    /* Read all change call sign records for the site */
                    while (DynChange.FtFetchChngCall(sID1, out ftChng, out nArrayFW) == Constant.SUCCESS)
                    {
                        /* if the call sign is being changed to 
                           anything other than a '=', then it is 
                           O.K. */
                        if (ftChng.newcall1[0] == '=')
                        {
                            continue;
                        }
                        else
                        {
                            ret = Constant.SUCCESS;
                        }
                    }
                    DynChange.FtCloseChngCall(sID1);
                }
            }

            //...Log2.v("\n\nFtRecExist.CheckFictCall(): Exit");
            return (ret); /* return the status of the check if it is a failure, there is a problem
			            with the ficticious call signs */
        }	/* ----- End of checkFictCall ----- */

    }
}
