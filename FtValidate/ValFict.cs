using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FtValidate
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
    /// Provides several utility methods relating to the validation of ficticious entities in the PDF.
    /// </summary>
    public class ValFict
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        public static extern int ftValFictChgPassive([In] string pdfName, [In] string call);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        public static extern int ftValFictRxOnly([In] string pdfName, [In] string call);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        public static extern int ftValFictTx([In] string pdfName, [In] string call);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        public static extern int ftValFictPassive([In] string pdfName, [In] string call);

        //--------------------------------------------------------------------------------

        public static int FtValFictChgPassive_NATIVE(string pdfName, string call)
        {
            return ftValFictChgPassive(pdfName, call);
        }
        public static int FtValFictRxOnly_NATIVE(string pdfName, string call)
        {
            return ftValFictRxOnly(pdfName, call);
        }
        public static int FtValFictTx_NATIVE(string pdfName, string call)
        {
            return ftValFictTx(pdfName, call);
        }
        public static int FtValFictPassive_NATIVE(string pdfName, string call)
        {
            return ftValFictPassive(pdfName, call);
        }
#endif

        /// <summary>
        /// This method determines whether a site changing from passive to non-passive 
        /// has had its antenna data updated accordingly.
        /// </summary>
        /// <param name="pdfName">- name of the PDF file.</param>
        /// <param name="call"> - call1 for a site.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtValFictChgPassive(string pdfName, string call)  /* site call1 info */
        {
            //...Log2.v("\n\nValFict.FtValFictChgPassive(): Entry");

            int ret;        /* return code */
            int rc;     /* another return code */
            int mtAnteHandle;
            int acodesize;  /* size of antenna code values */

            MtAnte mtAnte;      /* temporary structures */
            FtAnte ftAnte = new FtAnte();

            SQLLEN[] mtAnteNulls;

            string tableName;
            string siteCall;
            string selectionCriteria;

            FtSiteStr pPDFSite = new FtSiteStr(FtSiteStr.Init.ALLOCATED);
            int nInd = 0;

            ret = Constant.SUCCESS; /* set return code to SUCCESS ...code must
			 * verify that this condition does not exist */

            siteCall = call;
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            /* if site is changing from passive to non-passive, then and all
             * antennas at that site must not have acodes that end with '%' */

            /* get all MDB antenna records with this Call sign */

            /* set up the selection criteria for the MDB Ingres cursor */
            selectionCriteria = String.Format(" call1 = '{0}'", siteCall);

            /* set up the MDB Ingres cursor */
            if ((mtAnteHandle = DynMdbAntenna.MtSelectAntenna(selectionCriteria, "")) < 0)
            {
                /* error selecting MDB antenna table */
                return (mtAnteHandle);
            }

            /* fetch an MDB antenna record with this Call sign */
            while (DynMdbAntenna.MtFetchAntenna(mtAnteHandle, out mtAnte, out mtAnteNulls) == Constant.SUCCESS)
            {
                /* check to see if this antenna qualifies as passive */

                /* for each antenna, determine its size and
                 * then look at the last character in the
                 * string (len -1 ) to see if it is a '%' */

                mtAnte.acode = mtAnte.acode.Trim();
                acodesize = mtAnte.acode.Length;
                if (mtAnte.acode[acodesize - 1] == '%')
                {
                    /* check the PDF for a corresponding antenna record
                     * that will change this antenna from passive to
                     * non-passive (or else, delete it)  */

                    rc = FtUtils.FtGetSite(mtAnte.call1, out pPDFSite, 2, pdfName);
                    if (rc == 0)
                    {
                        nInd = FtUtils.FtFindAnt(pPDFSite, mtAnte.call2, mtAnte.bndcde, mtAnte.anum);
                        if (nInd >= 0)
                        {
                            rc = 0;
                        }
                    }
                    if (rc != 0)
                    {
                        if (rc == 1 /* No record found */)
                        {
                            ret = Constant.FAILURE;
                            break;
                        }
                        else
                        {
                            ValErrs.AddMess("valFict - Could not access PDF Antenna data (%s %s %s %d):-\r\n %s",
                                                        tableName, "E", mtAnte.call1, mtAnte.call2, mtAnte.bndcde,
                                                        mtAnte.anum.ToString(), GenUtil.GetUserMess());
                            ret = Error.DYN_MS_SQL_SERVER_ERR;
                            break;
                        }
                    }
                    else
                    {
                        /* if the antenna is being deleted, then
                         * this antenna is not a problem */
                        if (pPDFSite.stAntsPtr[nInd].CmdEquals('D'))
                        {
                            continue;
                        }
                        else
                        {
                            /* is the antenna code being changed
                             * to be non-passive ?  If so, then
                             * this antenna is not a problem */
                            string trimmed = pPDFSite.stAntsPtr[nInd].acode.Trim();
                            pPDFSite.stAntsPtr[nInd].acode = trimmed;
                            acodesize = pPDFSite.stAntsPtr[nInd].acode.Length;
                            if (pPDFSite.stAntsPtr[nInd].acode[acodesize - 1] != '%')
                            {
                                continue;
                            }
                            else
                            {
                                ret = Constant.FAILURE;
                                break;
                            }
                        }
                    }
                }
            }

            DynMdbAntenna.MtCloseAntenna(mtAnteHandle);

            rc = 1;
            for (nInd = 0; nInd < pPDFSite.nNumAnts; nInd++)
            {
                FtAnte pAnt = pPDFSite.stAntsPtr[nInd];
                if (pAnt.call1.Equals(mtAnte.call1) && pAnt.cmd.Equals("A") && FtUtils.IsBillBoard(pAnt.acode))
                {
                    rc = 0;
                    break;
                }
            }

            if (rc != 0)
            {
                if (rc == 1)
                {
                }
                else
                {
                    ValErrs.AddMess("valFict - Could not access PDF antenna for add:\r\n(%s %s %s %d): %s",
                                                tableName, "E", mtAnte.call1, mtAnte.call2, mtAnte.bndcde, mtAnte.anum.ToString(),
                                                GenUtil.GetUserMess());
                    ret = Error.DYN_MS_SQL_SERVER_ERR;
                }
            }
            else
            {
                /* a passive antenna code is being added.
                 * This is a problem */
                ret = Constant.FAILURE;
                ValErrs.AddMess("A passive site (%s %s %s %d) changing to non-passive must also have its antenna information (%s) updated.", tableName, "E",
                                            mtAnte.call1, mtAnte.call2, mtAnte.bndcde,
                                            mtAnte.anum.ToString(), ftAnte.acode);
            }

            //...Log2.v("\n\nValFict.FtValFictChgPassive(): Exit");
            return (ret);   /* return the status of the check.
			 * if it is a failure, there is a problem
			 * with changing this call signs to non-passive */

        }   /* ----- End of ftValFictChgPassive ----- */

        /// <summary>
        /// This method determines if a site with a '$' call1 has only Receiving channels.
        /// </summary>
        /// <param name="shortName"> - name of the PDF file.</param>
        /// <param name="call"> - call1 for a site.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtValFictRxOnly(string shortName, string call)    /* site call1 info */
        {
            //...Log2.v("\n\nValFict.FtValFictRxOnly(): Entry");

            int ret;                /* return code */
            //int MDB_recs; /* flags indicating presence of these records*/
            int PDF_recs; /* flags indicating presence of these records*/

            int chanHandle;             /* dynamic cursor handles */
            int mtChanHandle;


            FtChan ftChan;      /* temporary structures */
            MtChan mtChan;

            SQLLEN[] nullIndChan;
            SQLLEN[] mtChanNulls;

            string tableName;

            string siteCall;
            string whereClause;
            string selectionCriteria;

            ret = Constant.SUCCESS;     /* set return code to success ...code must */
                                        /* verify that this condition does not exist */
                                        //MDB_recs = Constant.FAILURE;
            PDF_recs = Constant.FAILURE;

            siteCall = call;

            GenUtil.UtCvtName(Constant.FT_CHAN, shortName, out tableName);


            /* Get all MDB channel records with this Call sign */

            /* Set up the selection criteria for the MDB Ingres cursor */
            selectionCriteria = String.Format(" call1 = '{0}'", siteCall);

            /* Set up the MDB Ingres cursor */
            if ((mtChanHandle = DynMdbChannel.MtSelectChannel(selectionCriteria, "")) < 0)
            {
                /* error selecting MDB site table */
                return (mtChanHandle);
            }

            /* Fetch an MDB channel record with this Call sign */
            while (DynMdbChannel.MtFetchChannel(mtChanHandle, out mtChan, out mtChanNulls) == Constant.SUCCESS)
            {
                /* at least one MDB channel record exists for this Call sign */
                //MDB_recs = Constant.SUCCESS;

                /* check to see if this channel is being deleted by a
                PDF record.  If so, we are done with this MDB record */

                /* Set up the selection criteria for the PDF Ingres cursor */

                whereClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                                                mtChan.call1, mtChan.call2, mtChan.bndcde, mtChan.chid);

                /* Set up the PDF Ingres cursor */
                if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }

                /* fetch a PDF channel record for this site */
                while (DynChannel.FtFetchChannel(chanHandle, out ftChan, out nullIndChan) == Constant.SUCCESS)
                {
                    // Bug fix: b150722A
                    // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                    // Continue to the next ftChan to be fetched from the DB.
                    if (!FtUtil.IsValidCmd(ftChan.cmd))
                    {
                        continue;
                    }

                    /* at least one PDF channel record exists for this
                    Call sign */
                    PDF_recs = Constant.SUCCESS;

                    /* if the channel is being deleted, we don't care
                    if it was TX */
                    if (ftChan.CmdEquals('D'))
                    {
                        continue;
                    }
                    else
                    {
                        /* is the channel being set to TX */
                        if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                        {
                            /* then failure - must be RX only */
                            ret = Constant.FAILURE;
                            break;
                        }
                    }
                }

                if (PDF_recs != Constant.SUCCESS)
                {
                    /* if no PDF records existed then check to see if
                    the MDB channel was already a TX */
                    if (mtChanNulls[MtChan.FREQTX] != Constant.DB_NULL)
                    {
                        /* then failure - must be RX only */
                        ret = Constant.FAILURE;
                        break;
                    }
                }
                /* close the PDF cursor */
                DynChannel.FtCloseChannel(chanHandle);
            }

            /* close the MDB cursor */
            DynMdbChannel.MtCloseChannel(mtChanHandle);

            /* if the site had no MDB Channel records */
            if (ret == Constant.SUCCESS)
            {
                /* check to see if a TX channel is being added by a PDF
                record.  If so, this site fails the criteria */

                /* Set up the selection criteria for the PDF Ingres cursor */
                whereClause = String.Format(" call1 = '{0}' and cmd = '{1}'", siteCall, "A");

                /* Set up the Ingres cursor */
                if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }

                /* fetch a PDF channel record for this site */
                while (DynChannel.FtFetchChannel(chanHandle, out ftChan, out nullIndChan) == Constant.SUCCESS)
                {
                    // Bug fix: b150722A
                    // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                    // Continue to the next ftChan to be fetched from the DB.
                    if (!FtUtil.IsValidCmd(ftChan.cmd))
                    {
                        continue;
                    }

                    /* is the PDF record adding a TX */
                    if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                    {
                        /* then failure - must be RX only */
                        ret = Constant.FAILURE;
                        break;
                    }
                }
                /* close the PDF cursor */
                DynChannel.FtCloseChannel(chanHandle);

            }

            //...Log2.v("\n\nValFict.FtValFictRxOnly(): Exit");
            return (ret); /* return the status of the check
				 if it is a failure, there is a problem
				 with the ficticious call signs */

        }   /* ----- End of ftValFictRxOnly ----- */

        /// <summary>
        /// This method determines whether a site with an '=' call1 has at least on transmit channel.  
        /// </summary>
        /// <param name="shortName"> - name of the PDF file.</param>
        /// <param name="call"> - call1 for a site.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtValFictTx(string shortName, string call)    /* site call1 info */
        {
            //...Log2.v("\n\nValFict.FtValFictTx(): Entry");

            int ret;                /* return code */
            int rc;                 /* another return code */

            int chanHandle;             /* dynamic cursor handles */
            int mtChanHandle;

            FtChan ftChan;      /* temporary structures */
            MtChan mtChan;

            SQLLEN[] nullIndChan;
            SQLLEN[] mtChanNulls;

            string tableName;

            string siteCall;
            string whereClause;
            string selectionCriteria;

            ret = Constant.FAILURE;     /* set return code to failure ...code must */
                                        /* verify that the condition does exist */


            siteCall = call;
            GenUtil.UtCvtName(Constant.FT_CHAN, shortName, out tableName);

            /* Get all PDF channel records with this Call sign */

            /* Set up the selection criteria for the PDF Ingres cursor */
            whereClause = String.Format(" call1 = '{0}'", siteCall);

            /* Set up the PDF Ingres cursor */
            if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            /* Fetch a PDF record with this Call sign */
            while (DynChannel.FtFetchChannel(chanHandle, out ftChan, out nullIndChan) == Constant.SUCCESS)
            {
                /* if the channel is being deleted, we keep looking */
                if (ftChan.CmdEquals('D'))
                {
                    continue;
                }
                else
                {
                    /* is the channel being set to TX */
                    if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                    {
                        /* then success - must have >= 1 TX channel */
                        ret = Constant.SUCCESS;
                        break;
                    }
                }
            }

            DynChannel.FtCloseChannel(chanHandle);

            if (ret != Constant.SUCCESS)
            {
                /* Get all MDB channel records with this Call sign */

                /* Set up the selection criteria for the MDB Ingres cursor */
                selectionCriteria = String.Format(" call1 = '{0}'", siteCall);

                /* Set up the MDB Ingres cursor */
                if ((mtChanHandle = DynMdbChannel.MtSelectChannel(selectionCriteria, "")) < 0)
                {
                    /* error selecting MDB site table */
                    return (mtChanHandle);
                }

                /* Fetch an MDB channel record with this Call sign */
                while (DynMdbChannel.MtFetchChannel(mtChanHandle, out mtChan, out mtChanNulls) == Constant.SUCCESS)
                {
                    /* check to see if this channel is being deleted by a
                    PDF record.  If so, keep looking at MDB records */

                    /* Set up the selection criteria for the PDF Ingres
                    cursor */

                    whereClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and chid='{3}'",
                                                    mtChan.call1, mtChan.call2, mtChan.bndcde, mtChan.chid);

                    /* Set up the PDF Ingres cursor */
                    if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                    {
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }

                    /* fetch the PDF channel record for this site */
                    rc = DynChannel.FtFetchChannel(chanHandle, out ftChan, out nullIndChan);
                    if (rc == Constant.SUCCESS)
                    {
                        /* if the channel is being deleted, it doesn't
                        *  qualify this site at TX */
                        if (ftChan.CmdEquals('D'))
                        {
                            continue;
                        }
                        else
                        {
                            /* is the channel being set to TX */
                            if (nullIndChan[FtChan.FREQTX] != Constant.DB_NULL)
                            {
                                /* then success - must be >= 1 TX */
                                ret = Constant.SUCCESS;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (mtChanNulls[MtChan.FREQTX] != Constant.DB_NULL)
                        {
                            /* there is an MDB rec with TX chan */
                            ret = Constant.SUCCESS;
                            break;
                        }
                    }
                }
                DynChannel.FtCloseChannel(chanHandle);
                DynMdbChannel.MtCloseChannel(mtChanHandle);
            }

            //...Log2.v("\n\nValFict.FtValFictTx(): Exit");
            return (ret); /* return the status of the check
				 if it is a failure, there is a problem
				 with the ficticious call signs */

        }   /* ----- End of ftValFictTx ----- */

        /// <summary>
        /// This method determines whether a site with a '%' call1 has only passive antennae.
        /// /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call"> - call1 for a site.</param>
        /// <returns> - Constant.SUCCESS or Constant.FAILURE.</returns>
        public static int FtValFictPassive(string pdfName, string call) /* site call1 info */
        {
            //...Log2.v("\n\nValFict.FtValFictPassive(): Entry");

            int ret;        /* return code */
            int anteHandle; /* antenna handle */
            int mtAnteHandle;
            int acodesize;  /* size of antenna code values */


            MtAnte mtAnte;      /* temporary structures */
            FtAnte ftAnte;

            SQLLEN[] nullIndAnte;
            SQLLEN[] mtAnteNulls;

            string tableName;

            string siteCall;
            string whereClause;
            string selectionCriteria;

            ret = Constant.SUCCESS;     /*	set return code to Constant.SUCCESS ...code must
						verify that this condition does not exist */


            /* if site begins with a '%' it is a passive site and all
            antennas at that site must have acodes that end with '%' */

            /* perform check on all PDF antenna records */

            siteCall = call;
            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            whereClause = String.Format("call1 = '{0}'", siteCall);

            if ((anteHandle = DynAntenna.FtSelectAntenna(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read site information", tableName, "W");
                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return Constant.FAILURE;
            }
            else
            {
                while (DynAntenna.FtFetchAntenna(anteHandle, out ftAnte, out nullIndAnte) == Constant.SUCCESS)
                {
                    // Bug fix: b150722A
                    // If the ftAnte has an invalid 'cmd' then don't apply any further validation.
                    // Continue to the next ftAnte to be fetched from the DB.
                    if (!FtUtil.IsValidCmd(ftAnte.cmd))
                    {
                        continue;
                    }

                    //AH: warning C4130: '!=': logical operation on address of string constant
                    //if (( nullIndAnte[FtAnte.ACODE] != Constant.DB_NULL )  (ftAnte.cmd != "D")) {
                    if ((nullIndAnte[FtAnte.ACODE] != Constant.DB_NULL) && (!ftAnte.CmdEquals('D')))
                    {
                        /* if acode exists and if the	antenna is not being deleted
                        perform similar check as was done above for the MDB */

                        ftAnte.acode.Trim();
                        acodesize = ftAnte.acode.Length;
                        if (ftAnte.acode[acodesize - 1] != '%')
                        {
                            /* there is no need to check further. There is an invalid antenna code
                            * associated with this ante/site combination.
                            */

                            ret = Constant.FAILURE;
                            break;
                        }
                    }
                }
            }
            DynAntenna.FtCloseAntenna(anteHandle);

            if (ret == Constant.SUCCESS)
            {
                /* get all MDB antenna records with this Call sign */

                /* set up the selection criteria for the MDB Ingres cursor */
                selectionCriteria = String.Format("call1 = '{0}'", siteCall);

                /* set up the MDB Ingres cursor */
                if ((mtAnteHandle = DynMdbAntenna.MtSelectAntenna(selectionCriteria, "")) < 0)
                {
                    /* error selecting MDB antenna table */
                    return (mtAnteHandle);
                }

                /* fetch an MDB antenna record with this Call sign */
                while (DynMdbAntenna.MtFetchAntenna(mtAnteHandle, out mtAnte, out mtAnteNulls) == Constant.SUCCESS)
                {
                    /* check to see if this antenna qualifies as passive */

                    /* for each antenna, determine its size and then look at the last character in the
                    string (len -1 ) to see if it is a '%' */

                    mtAnte.acode.Trim();
                    acodesize = mtAnte.acode.Length;
                    if (mtAnte.acode[acodesize - 1] != '%')
                    {
                        ValErrs.AddMess("MDB antenna not passive in passive link.", mtAnte.call1, "E");
                        ret = Constant.FAILURE;
                        break;
                    }
                }
                DynMdbAntenna.MtCloseAntenna(mtAnteHandle);
            }

            //...Log2.v("\n\nValFict.FtValFictPassive(): Exit");
            return (ret); /* return the status of the check
				 if it is a failure, there is a problem
				 with the ficticious call signs */

        }	/* ----- End of ftValFictPassive ----- */



    }
}
