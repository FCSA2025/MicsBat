using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeValidate
{
    using _Auxlib;
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;

    using SQLLEN = Int64;

    /// <summary>
    /// This class provides methods that validate the 'change of callsign'
    /// information provided by a previously imported PDF.
    /// </summary>
    public class ValCCal
    {
        private const string titleLine = @".VALIDATION AGAINST MDB";

        /// <summary> 
        /// This method validates ES 'change of call sign' records.    
        /// </summary>
        /// <remarks>
        /// The validation for change of call sign simply verifies that:
        /// <list type="bullet">
        /// <item>the old location does exist;</item>
        /// <item>the new location does not already exist; and</item>
        /// <item>the old location is not referenced in the PDF.</item>
        /// </list>
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        public static void FeValCCalES(string pdfName,    // pdf display name 
                                         ref short errCount,    // number of errors encountered 
                                         ref short warnCount    // number of warnings encountered 
                                         )
        {
            // Local variables 
            int rc;
            int sID1;
            bool done;
            int count;
            SQLLEN[] nArrayFW;          // Ingres Null array 
            string tableName;           // Ingres table name 
            string whereClause = "";    // Where clause buffer 
            string keyLine = "";        // Output buffer 
            string siteoper = "";
            string chngcall;
            MeChan meChan;              // ES channel structure 
            MeAnte meAnte;              // ES antenna structure 
            FeCCal feCCal;              // ES PDF change call structure 

            MeSite meSite;
            SQLLEN[] nArrayMDB;

            GenUtil.UtCvtName(Constant.FE_CCAL, pdfName, out tableName);


            // Read callsign information 
            if ((sID1 = DynFeCCal.FeSelectCCal(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read change callsign information. Reason: %d",
                                tableName, "W", sID1.ToString());
                warnCount++;
                return;
            }

            // Read all change callsign records from PDF 
            while (DynFeCCal.FeFetchCCal(sID1, out feCCal, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} to {1}.", feCCal.oldcallsign, feCCal.newcallsign);

                // verify that all fields are there 
                if (nArrayFW[FeCCal.OLDCALLSIGN] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Must Enter Old Call Sign field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeCCal.NEWCALLSIGN] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Must Enter New Call Sign field.", keyLine, "E");
                    errCount++;
                }


                // -- Make sure old callsign does exist in the MDB -- 

                whereClause = String.Format("call1 = '{0}'", feCCal.oldcallsign);

                // Declare cursor for Ante. recs in PDF 
                rc = ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out nArrayMDB);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        whereClause = String.Format("location = '{0}'", meAnte.location);

                        if ((rc = ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB)) == Constant.SUCCESS)
                        {
                            siteoper = meSite.oper = meSite.oper.Trim();
                        }
                        else
                        {
                            ValErrs.AddMess("Unable to retrieve site operator information from MDB.", keyLine, "E");
                            errCount++;
                        }

                        // The legacy C/C++ code had an implicit switch-case fall-through
                        // but in C# this has to be explicit.
                        goto case Constant.SEVERALRECS;

                    case Constant.SEVERALRECS:
                        // check to see that no other records in the PDF
                        // contain the new callsign from the change of
                        // callsign record.
                        if (FeCheckForCallSignPropagate(pdfName,
                                                        feCCal.newcallsign,
                                                        ref warnCount,
                                                        keyLine) == true)
                        {
                            ValErrs.AddMess("Illegal reference to New Call Sign in the pdf.", keyLine, "E");
                            errCount++;
                        }
                        break;

                    case Constant.NOMORERECS:
                        rc = ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out nArrayMDB);

                        switch (rc)
                        {
                            case Constant.SUCCESS:
                                /* check to see that no other records contain
                                   the new callsign from the change of
                                   callsign record.
                                */
                                if (FeCheckForCallSignPropagate(pdfName,
                                                                feCCal.newcallsign,
                                                                ref warnCount,
                                                                keyLine) == true)
                                {
                                    ValErrs.AddMess("Illegal reference to New Call Sign in the pdf.",
                                                    keyLine, "E");
                                    errCount++;
                                }
                                break;

                            case Constant.NOMORERECS:
                                ValErrs.AddMess("Old CALL SIGN does not exist in the MDB.", keyLine, "E");
                                errCount++;
                                break;

                            case Constant.RECLOCK:

                                ValErrs.AddMess("Change Call Sign record locked in MDB.", keyLine, "W");
                                warnCount++;
                                break;

                            default:

                                ValErrs.AddMess("CALL SIGN undefined error :%d:.", keyLine, "W", rc.ToString());
                                warnCount++;
                                break;

                        } // End switch 

                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("ANTENNA record callsign locked in MDB.", keyLine, "W");
                        warnCount++;
                        break;

                    default:

                        ValErrs.AddMess("CALL SIGN undefined error :%d:.", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;

                }   // End switch 

                /* if the old call sign doesn't exist, don't bother trying
                 * to edit the new call sign.
                 */
                if (rc == Constant.NOMORERECS)
                {
                    break;
                }

                // -- Warn if new callsign already exists in the MDB -- 

                VerifyNewCallsign(feCCal.newcallsign, ref warnCount, keyLine);

                // some changes are allowed to fictitious call signs 

                switch (feCCal.newcallsign[0])
                {
                    case '$':

                        // ensure that a '$' ficticious call sign is valid 
                        if (FeValFictRxOnly(pdfName, feCCal.oldcallsign) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("A site beginning with '$' (%s) must have all RX frequencies.",
                                            keyLine, "E", feCCal.newcallsign);
                            errCount++;
                        }
                        break;

                    case '=':

                        // ensure that a '=' ficticious call sign is valid 
                        if (FeValFictTx(pdfName, feCCal.oldcallsign) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("A site beginning with '=' (%s) must have at least one TX frequency.",
                                            keyLine, "E", feCCal.oldcallsign);
                            errCount++;
                        }
                        break;

                } // End switch 


                /* - Rule:  Cannot switch some 'types' of fictitious
                 *          callsigns - */

                // Check if old and new callsigns are both fictitious 
                if (((feCCal.oldcallsign[0] == '=') || // old sign is fict 
                     (feCCal.oldcallsign[0] == '$') ||
                     (feCCal.oldcallsign[0] == '%') ||
                     (feCCal.oldcallsign[0] == ';')) &&     // and 
                    ((feCCal.newcallsign[0] == '=') ||
                     (feCCal.newcallsign[0] == '$') ||
                     (feCCal.newcallsign[0] == '%') ||
                     (feCCal.newcallsign[0] == ';'))) // new sign is fict 
                {
                    /* Check if first char is different - ie. changing
                     * fictitious callsign type to something other
                     * than '$' or '='
                     */
                    if (((feCCal.oldcallsign[0] != feCCal.newcallsign[0])) &&
                            ((feCCal.newcallsign[0] != '$') &&
                          (feCCal.newcallsign[0] != '=')))
                    {
                        ValErrs.AddMess("Cannot change Call Sign to this new Ficticious Code:%s.",
                                        keyLine, "E", feCCal.newcallsign);
                        errCount++;
                        break;
                    }
                    else    // fictitious callsigns are not changing but 
                    {
                        // check other characters for valid changes 
                        done = false;
                        count = 3;
                        while (!done && (count <= 6))
                        {
                            Strings.StrnCpy(out chngcall, Strings.DropFirstChar(feCCal.newcallsign), count);

                            if (chngcall.Equals(siteoper))
                            {
                                done = true;
                            }
                            count++;
                        }

                        if (!done)
                        {
                            ValErrs.AddMess("The operator code in the new call sign must be the same as the operator code for this site.",
                                            keyLine, "E");
                            errCount++;
                            break;

                        }   // End if 
                    }   // End else 

                }   // End if both callsigns fictitious 

            }   // End while 

            DynFeCCal.FeCloseCCal(sID1);

        }	// ----- End of feValCCalES ----- 

        /// <summary>
        /// This method searches for PDF records that reference a prescribed
        /// call sign; the search order is antennae, channels then azimuths. 
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="newcallsign"> - call sign to search for.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="keyLine"> - key message to print.</param>
        /// <returns>true if at least one record is found that references the call sign; otherwise false.</returns>
        public static bool FeCheckForCallSignPropagate(string pdfName,           // pdf display name 
                                                        string newcallsign,     // old callsign 
                                                        ref short warnCount,    // number of warnings 
                                                        string keyLine		    // key info to print 
)
        {
            // Local variables 
            int ID1;            // cursor ID 
            SQLLEN[] nArrayFW;  // Null Array for Ingres 
            string whereClause; // selection clause for SQL 
            string tableName;   // Ingres internal table name 
            FeChan feChan;      // Channel structure 
            FeAnte feAnte;      // Antenna structure 
            FeAzim feAzim;      // Antenna structure 


            // -- First, check the Antenna records -- 

            // Initialization 
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);
            whereClause = String.Format("call1 = '{0}'", newcallsign);

            // Declare cursor for Ante. recs in PDF 
            if ((ID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot access Antenna information in PDF. Reason: %d",
                                tableName, "W", ID1.ToString());
                warnCount++;
                return (false);
            }

            // Try to read an Antenna record 
            if (DynFeAnte.FeFetchAnte(ID1, out feAnte, out nArrayFW) == Constant.SUCCESS)
            {
                // found a rec with changing callsign 
                DynFeAnte.FeCloseAnte(ID1);
                return (true);
            }
            DynFeAnte.FeCloseAnte(ID1);


            // -- Check the Channel records -- 

            // Initialization 
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);
            whereClause = String.Format("call1 = '{0}'", newcallsign);

            // Declare cursor for Chan. recs in PDF 
            if ((ID1 = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot access Channel information in PDF. Reason: %d",
                                tableName, "W", ID1.ToString());
                warnCount++;
                return (false);
            }

            // Try to read a Channel record 
            if (DynFeChan.FeFetchChan(ID1, out feChan, out nArrayFW) == Constant.SUCCESS)
            {
                // found a rec with changing callsign 
                DynFeChan.FeCloseChan(ID1);
                return (true);
            }
            DynFeChan.FeCloseChan(ID1);


            // -- Check the Azimuth records -- 

            // Initialization 
            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);
            whereClause = String.Format("call1 = '{0}'", newcallsign);

            // Declare cursor for Azim. recs in PDF 
            if ((ID1 = DynFeAzim.FeSelectAzim(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot access Azimuth information in PDF. Reason: %d",
                                keyLine, "W", ID1.ToString());
                warnCount++;
                return (false);
            }

            // Try to read an Azimuth record 
            if (DynFeAzim.FeFetchAzim(ID1, out feAzim, out nArrayFW) == Constant.SUCCESS)
            {
                // found a rec with changing callsign 
                DynFeAzim.FeCloseAzim(ID1);
                return (true);
            }
            DynFeAzim.FeCloseAzim(ID1);


            return (false);


        }   // ----- End of feCheckForCallSignPropagate ----- 

        /// <summary>
        /// This method checks the MDB to ensure that a new callsign is not currently 
        /// being used in an antenna, channel or azimuth record; this situation is not 
        /// necessarily an error, but can cause problems during UPDATE if not handled 
        /// properly; the user is warned if this situation exists.  
        /// </summary>
        /// <param name="newCallsign"> - new callsign.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="keyLine"> - key information to print.</param>
        /// <returns></returns>
        public static int VerifyNewCallsign(string newCallsign,     // old callsign 
                                            ref short warnCount,    // number of warnings 
                                            string keyLine)         // key info to print 
        {
            // Local variables 
            int rc1;            // Fctn call return code 
            int rc2;            // Fctn call return code 
            int rc3;            // Fctn call return code 
            SQLLEN[] nArrayMDB; // Null Array for Ingres 
            string whereClause; // selection clause for SQL 
            MeChan meChan;      // Channel structure 
            MeAnte meAnte;      // Antenna structure 
            MeAzim meAzim;      // Antenna structure 

            // Initialization 
            whereClause = String.Format("call1 = '{0}'", newCallsign);

            // Try to retrieve records 
            rc1 = ValRetrieveES.FeValRetrieveMDBChannel(out meChan, whereClause, out nArrayMDB);
            rc2 = ValRetrieveES.FeValRetrieveMDBAntenna(out meAnte, whereClause, out nArrayMDB);
            rc3 = ValRetrieveES.FeValRetrieveMDBAzimuth(out meAzim, whereClause, out nArrayMDB);

            if (((rc1 == Constant.SUCCESS) || (rc1 == Constant.SEVERALRECS)) ||
                    ((rc2 == Constant.SUCCESS) || (rc2 == Constant.SEVERALRECS)) ||
                    ((rc3 == Constant.SUCCESS) || (rc3 == Constant.SEVERALRECS)))
            {
                ValErrs.AddMess("New call sign exists in MDB. PDF may fail MDB update.",
                                keyLine, "W");
                warnCount++;
            }
            else    // No success in finding desired records 
            {
                // - Check for possible errors 
                // Constant.NOMORERECS condition is good - anything else is bad 
                if ((rc1 != Constant.NOMORERECS) || (rc2 != Constant.NOMORERECS) || (rc3 != Constant.NOMORERECS))
                {
                    if ((rc1 == Constant.RECLOCK) || (rc2 == Constant.RECLOCK) || (rc3 == Constant.RECLOCK))
                    {
                        ValErrs.AddMess("Records locked in MDB.", keyLine, "W");
                        warnCount++;
                    }
                    else    // More serious, bizarre error 
                    {
                        ValErrs.AddMess("CALL SIGN undefined error :%d:%d:%d:.",
                                        keyLine, "W", rc1.ToString(), rc2.ToString(), rc3.ToString());
                        warnCount++;

                    }   // End else 

                }   // End if 

            }   // End else 
            return 0;
        }   // ----- End of VerifyNewCallsign ----- 

        /// <summary>
        /// This method searches the MDB and PDF tables to verify that a site whose callsign has a '$' prefix has only Receiving 
        /// channels.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="call"> - site call sign.</param>
        /// <returns></returns>
        public static int FeValFictRxOnly(string pdfName, string call)    // site call1 info 
        {
            int ret;                // return code 
            int PDF_recs;           // flags indicating presence of these records

            int chanHandle;         // dynamic cursor handles 
            int meChanHandle;

            FeChan feChan;          // temporary structures 
            MeChan meChan;

            SQLLEN[] nullIndChan;
            SQLLEN[] meChanNulls;

            string tableName;

            string siteCall;
            string whereClause;
            string selectionCriteria;

            ret = Constant.SUCCESS;     // set return code to success ...code must 
                                        // verify that this condition does not exist 

            PDF_recs = Constant.FAILURE;

            siteCall = call;
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);


            // Get all MDB channel records with this Call sign 

            // Set up the selection criteria for the MDB Ingres cursor 
            selectionCriteria = String.Format("call1 = '{0}'", siteCall);

            // Set up the MDB Ingres cursor 
            if ((meChanHandle = DynMeChan.MeSelectChan(selectionCriteria, "")) < 0)
            {
                // error selecting MDB site table 
                return (meChanHandle);
            }

            // Fetch an MDB channel record with this Call sign 
            while (DynMeChan.MeFetchChan(meChanHandle, out meChan, out meChanNulls) == Constant.SUCCESS)
            {
                /* check to see if this channel is being deleted by a
                   PDF record.  If so, we are done with this MDB record */

                // Set up the selection criteria for the PDF Ingres cursor 

                whereClause = String.Format("location='{0}' and call1='{1}' and chid='{2}'",
                                meChan.location, meChan.call1, meChan.chid);

                // Set up the PDF Ingres cursor 
                if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }

                // fetch a PDF channel record for this site 
                while (DynFeChan.FeFetchChan(chanHandle, out feChan, out nullIndChan) == Constant.SUCCESS)
                {
                    /* at least one PDF channel record exists for this
                       Call sign */
                    PDF_recs = Constant.SUCCESS;

                    /* if the channel is being deleted, we don't care
                       if it was TX */
                    if (feChan.cmd[0] == 'D')
                    {
                        continue;
                    }
                    else
                    {
                        // is the channel being set to TX 
                        if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                        {
                            // then failure - must be RX only 
                            ret = Constant.FAILURE;
                            break;
                        }
                    }
                }

                if (PDF_recs != Constant.SUCCESS)
                {
                    /* if no PDF records existed then check to see if
                       the MDB channel was already a TX */
                    if (meChanNulls[MeChan.FREQTX] != Constant.DB_NULL)
                    {
                        // then failure - must be RX only 
                        ret = Constant.FAILURE;
                        break;
                    }
                }
                // close the PDF cursor 
                DynFeChan.FeCloseChan(chanHandle);
            }

            // close the MDB cursor 
            DynMeChan.MeCloseChan(meChanHandle);

            // if the site had no MDB Channel records 
            if (ret == Constant.SUCCESS)
            {
                /* check to see if a TX channel is being added by a PDF
                   record.  If so, this site fails the criteria */

                // Set up the selection criteria for the PDF Ingres cursor 
                whereClause = String.Format("call1 = '{0}' and cmd = '{1}'", siteCall, "A");

                // Set up the Ingres cursor 
                if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                {
                    return (Error.DYN_MS_SQL_SERVER_ERR);
                }

                // fetch a PDF channel record for this site 
                while (DynFeChan.FeFetchChan(chanHandle, out feChan, out nullIndChan) == Constant.SUCCESS)
                {
                    // is the PDF record adding a TX 
                    if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                    {
                        // then failure - must be RX only 
                        ret = Constant.FAILURE;
                        break;
                    }
                }
                // close the PDF cursor 
                DynFeChan.FeCloseChan(chanHandle);

            }
            return (ret); /* return the status of the check
			if it is a failure, there is a problem
			with the ficticious call signs */

        }   // ----- End of feValFictRxOnly ----- 

        /// <summary>
        /// This method searches the MDB and PDF tables to verify that a site whose
        /// callsign has a '=' prefix has at least one transmit-only or transmit-receive channel.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="call"> - site call sign.</param>
        /// <returns></returns>
        public static int FeValFictTx(string pdfName, string call)    // site call1 info 
        {
            int ret;                // return code 
            int rc;                 // another return code 

            int chanHandle;             // dynamic cursor handles 
            int meChanHandle;

            FeChan feChan;      // temporary structures 
            MeChan meChan;

            SQLLEN[] nullIndChan;
            SQLLEN[] meChanNulls;

            string tableName;

            string siteCall;
            string whereClause;
            string selectionCriteria;

            ret = Constant.FAILURE;     // set return code to failure ...code must 
                                        // verify that the condition does exist 

            siteCall = call;
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);

            // Get all PDF channel records with this Call sign 

            // Set up the selection criteria for the PDF Ingres cursor 
            whereClause = String.Format("call1 = '{0}'", siteCall);

            // Set up the PDF Ingres cursor 
            if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                return (Error.DYN_MS_SQL_SERVER_ERR);
            }

            // Fetch a PDF record with this Call sign 
            while (DynFeChan.FeFetchChan(chanHandle, out feChan, out nullIndChan) == Constant.SUCCESS)
            {
                // if the channel is being deleted, we keep looking 
                if (feChan.cmd[0] == 'D')
                {
                    continue;
                }
                else
                {
                    // is the channel being set to TX 
                    if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                    {
                        // then success - must have >= 1 TX channel 
                        ret = Constant.SUCCESS;
                        break;
                    }
                }
            }

            DynFeChan.FeCloseChan(chanHandle);

            if (ret != Constant.SUCCESS)
            {
                // Get all MDB channel records with this Call sign 

                // Set up the selection criteria for the MDB Ingres cursor 
                selectionCriteria = String.Format("call1 = '{0}'", siteCall);

                // Set up the MDB Ingres cursor 
                if ((meChanHandle = DynMeChan.MeSelectChan(selectionCriteria, "")) < 0)
                {
                    // error selecting MDB site table 
                    return (meChanHandle);
                }

                // Fetch an MDB channel record with this Call sign 
                while (DynMeChan.MeFetchChan(meChanHandle, out meChan, out meChanNulls) == Constant.SUCCESS)
                {
                    /* check to see if this channel is being deleted by a
                       PDF record.  If so, keep looking at MDB records */

                    /* Set up the selection criteria for the PDF Ingres
                       cursor */

                    whereClause = String.Format("location='{0}' and call1='{1}' and chid='{2}'",
                                    meChan.location, meChan.call1, meChan.chid);

                    // Set up the PDF Ingres cursor 
                    if ((chanHandle = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
                    {
                        return (Error.DYN_MS_SQL_SERVER_ERR);
                    }

                    // fetch a PDF channel record for this site 
                    rc = DynFeChan.FeFetchChan(chanHandle, out feChan, out nullIndChan);
                    if (rc == Constant.SUCCESS)
                    {
                        /* if the channel is being deleted, it doesn't
                           qualify this site at TX */
                        if (feChan.cmd[0] == 'D')
                        {
                            continue;
                        }
                        else
                        {
                            // is the channel being set to TX 
                            if (nullIndChan[FeChan.FREQTX] != Constant.DB_NULL)
                            {
                                // then success - must be >= 1 TX 
                                ret = Constant.SUCCESS;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (meChanNulls[MeChan.FREQTX] != Constant.DB_NULL)
                        {
                            // there is an MDB rec with TX chan 
                            ret = Constant.SUCCESS;
                            break;
                        }
                    }
                    DynFeChan.FeCloseChan(chanHandle);
                }
                DynMeChan.MeCloseChan(meChanHandle);
            }

            return (ret); /* return the status of the check
			if it is a failure, there is a problem
			with the ficticious call signs */

        }	// ----- End of feValFictTx ----- 










    }
}
