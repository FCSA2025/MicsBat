using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeValidate
{
    using _Configuration;
    using _DataStructures;
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
    /// This class provides methods that validate the site information
    /// provided by a previously imported PDF.
    /// </summary>
    public class ValSite
    {
        /// <summary>
        /// This method validates ES site records i.a.w. their prescribed
        /// MDB operation (ADD, BLANK-OUT, DELETE, NO-CHANGE, UPDATE).  
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - number of errors encountered.</param>
        /// <param name="warnCount"> - number of warnings encountered.</param>
        public static void FeValSiteES(string pdfName,          // pdf display name 
                                        ref short errCount,     // number of errors encountered 
                                        ref short warnCount)    // number of warnings encountered 
        {
            SQLLEN[] nArrayFW; //[FeSite.SIZE_];
            int sID1;
            string tableName;
            string whereClause = "";
            string keyLine = "";
            FeSite feSite;

            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);

            // read site information 
            if ((sID1 = DynFeSite.FeSelectSite(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read site information. Reason %d", tableName, "W", sID1.ToString());
                warnCount++;
                return;
            }

            while (DynFeSite.FeFetchSite(sID1, out feSite, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = String.Format("Location: {0}", feSite.location);

                switch (feSite.cmd[0])
                {
                    case 'A':
                        FeValSiteAdd(feSite, nArrayFW, ref errCount, ref warnCount, keyLine, pdfName);
                        break;

                    case 'D':
                        FeValSiteDel(feSite, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'U':
                    case 'B':
                        FeValSiteUpdt(feSite, nArrayFW, pdfName, ref errCount, ref warnCount, keyLine);
                        break;

                    case 'N':
                        FeValSiteNoop(feSite, ref errCount, ref warnCount, keyLine);
                        break;

                    default:
                        ValErrs.AddMess("Command (%s) is invalid - must be A, B, D, N or U",
                               keyLine, "E", feSite.cmd);
                        errCount++;
                        break;
                }

                // TASK 1015: New code. 
                if ((feSite.cmd.Equals("A")) ||
                        (feSite.cmd.Equals("B")) ||
                    (feSite.cmd.Equals("U")))
                {
                    if (feSite.rain < 1 || feSite.rain > 9)
                    {
                        ValErrs.AddMess("SITE Rain (%d) out of range (1-9), TSIP will use 9.",
                                        keyLine, "W", feSite.rain.ToString());
                        warnCount++;
                    }
                }
                // TASK 1015: End new code. 

                DynFeSite.FeUpdateSite(sID1, feSite, nArrayFW);
            }
            DynFeSite.FeCloseSite(sID1);
        }


        /// <summary>
        /// This method validates ES site records whose MDB operation is "ADD".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the site validation for an ADD record. It verifies 
        /// that the record does not exist and calls the field validation method to 
        /// verify that all fields are correct.  
        /// </remarks>
        /// <param name="feSite"> - site information, instance of FeSite.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feSite.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        public static void FeValSiteAdd(FeSite feSite,          // site info 
                                        SQLLEN[] nArrayFW,      // array with null flags 
                                        ref short errCount,     // number of errors 
                                        ref short warnCount,    // number of warnings 
                                        string keyLine,         // key info to print 
                                        string pdfName)
        {
            MeSite meSite;
            SQLLEN[] nArrayMDB; //[MeSite.SIZE_];
            short rc;
            string whereClause;

            // make sure location does not already exist 
            ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");

            rc = (short)ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    ValErrs.AddMess("SITE already exists in MDB, cannot ADD", keyLine, "E");
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("SITE locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("SITE undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            FeValSiteFields(feSite, nArrayFW, pdfName, ref errCount, keyLine);
            FeCalcSiteFields(feSite, nArrayFW);
        }


        /// <summary>
        /// This method validates ES site records whose MDB operation is "UPDATE".  
        /// </summary>
        /// <remarks>
        /// This method performs the site validation for an UPDATE record. It verifies 
        /// that the record does indeed already exist and calls the field validation method to 
        /// verify that all fields are correct.  
        /// </remarks>
        /// <param name="feSite"> - site information, instance of FeSite.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feSite.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValSiteUpdt(FeSite feSite,         // site info 
                                         SQLLEN[] nArrayFW,     // array with null flags 
                                         string pdfName,        // pdf display name 
                                         ref short errCount,    // number of errors 
                                         ref short warnCount,   // number of warnings 
                                         string keyLine)        // key info to print 
        {
            FeAnte feAnte;
            MeSite meSite;
            string whereClause;
            string tableName;
            SQLLEN[] nArrayMDB; //[MeSite.SIZE_],
            SQLLEN[] nArrayTemp; //[FeAnte.SIZE_];
            int rc;
            int aID1;

            // make sure site does exist 
            ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");

            rc = ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Site does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("SITE record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("SITE undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            if (rc != Constant.SUCCESS)
            {
                /* if we couldn't properly retrieve this record
                         * we don't want to continue validating
                     */
                return;
            }

            // if lat, long or ground are changing, modify Antenna records 
            if ((feSite.latit != meSite.latit) ||
                    (feSite.longit != meSite.longit) ||
                    (feSite.grnd != meSite.grnd))
            {
                ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");

                // read antenna information 
                GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

                if ((aID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
                {
                    ValErrs.AddMess("Could not read antenna information. Reason: %d",
                                    keyLine, "W", aID1.ToString());
                    warnCount++;
                }
                else
                {
                    while (DynFeAnte.FeFetchAnte(aID1, out feAnte, out nArrayTemp) == Constant.SUCCESS)
                    {
                        if (feAnte.cmd.Equals("N"))
                        {
                            feAnte.cmd = "U";
                        }
                        DynFeAnte.FeUpdateAnte(aID1, feAnte, nArrayTemp);
                    }
                    DynFeAnte.FeCloseAnte(aID1);
                }
            }

            // Check modify date and time 
            if (((!feSite.mdate.Equals(meSite.mdate))) ||
                 ((!feSite.mtime.Equals(meSite.mtime))))
            {
                // Modify dates/times don't match 
                ValErrs.AddMess("This Site record has been modified since you retrieved it.",
                                keyLine, "W");
                warnCount++;
            }

            FeValSiteFields(feSite, nArrayFW, pdfName, ref errCount, keyLine);
            FeCalcSiteFields(feSite, nArrayFW);
        }


        /// <summary>
        /// This method validates ES site records whose MDB operation is "DELETE".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the site validation for a DELETE record. It 
        /// verifies that the record does indeed exist, and retrieves all information 
        /// below the site record (i.e. antennae, azimuths, channels) to delete as well as all information pointing to the 
        /// site record.  
        /// </remarks>
        /// <param name="feSite"> - site information, instance of FeSite.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValSiteDel(FeSite feSite,          // site info 
                                        string pdfName,         // pdf display name 
                                        ref short errCount,     // number of errors 
                                        ref short warnCount,    // number of warnings 
                                        string keyLine)         // key info to print 
        {
            SQLLEN[] nArrayTemp;
            SQLLEN[] nArrayMDB; //[MeSite.SIZE_];
            short rc;
            int ID1;
            string whereClause;
            string tableName;
            MeSite meSite;
            FeAzim feAzim;
            FeAnte feAnte;
            FeChan feChan;

            // make sure site does exist 
            ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");

            if ((!String.IsNullOrWhiteSpace(feSite.recstat)) && (feSite.recstat[0] != 'C'))
            {
                rc = (short)ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB);
                switch (rc)
                {
                    case Constant.SUCCESS:
                        break;

                    case Constant.NOMORERECS:
                        ValErrs.AddMess("Site does not exist in MDB", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("SITE record locked in MDB", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("SITE undefined error: %d", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;
                }
            }

            // delete all antennas,azimuths and channels below 
            // modify antenna information 
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);
            if ((ID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read antenna information. Reason: %d", keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {
                while (DynFeAnte.FeFetchAnte(ID1, out feAnte, out nArrayTemp) == Constant.SUCCESS)
                {
                    if ((feAnte.cmd[0] != 'N') && (feAnte.cmd[0] != 'D'))
                    {
                        ValErrs.AddMess("Cannot delete if antenna being Added or Updated",
                               keyLine, "E");
                        errCount++;
                    }
                    else
                    {
                        feAnte.cmd = "D";
                        DynFeAnte.FeUpdateAnte(ID1, feAnte, nArrayTemp);
                    }
                }
                DynFeAnte.FeCloseAnte(ID1);
            }

            // modify azimuth information 
            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);
            if ((ID1 = DynFeAzim.FeSelectAzim(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read azimuth information.  Reason: %d",
                                keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {
                while (DynFeAzim.FeFetchAzim(ID1, out feAzim, out nArrayTemp) == Constant.SUCCESS)
                {
                    if (nArrayTemp[FeAzim.CMD] != Constant.DB_NULL)
                    {
                        if ((feAzim.cmd[0] != 'N') && (feAzim.cmd[0] != 'D'))
                        {
                            ValErrs.AddMess("Cannot delete if azimuth being Added or Updated",
                                         keyLine, "E");
                            errCount++;
                        }
                        else
                        {
                            feAzim.cmd = "D";
                            DynFeAzim.FeUpdateAzim(ID1, feAzim, nArrayTemp);
                        }
                    }
                }
                DynFeAzim.FeCloseAzim(ID1);
            }


            // modify channel information 
            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);
            if ((ID1 = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read channel information. Reason: %d",
                                keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {

                while (DynFeChan.FeFetchChan(ID1, out feChan, out nArrayTemp) == Constant.SUCCESS)
                {
                    if ((feChan.cmd[0] != 'N') && (feChan.cmd[0] != 'D'))
                    {
                        ValErrs.AddMess("Cannot delete if channel being Added or Updated",
                                        keyLine, "E");
                        errCount++;
                    }
                    else
                    {
                        feChan.cmd = "D";
                        DynFeChan.FeUpdateChan(ID1, feChan, nArrayTemp);
                    }
                }
                DynFeChan.FeCloseChan(ID1);
            }
        }



        /// <summary>
        /// This method validates that a site record exists in the MDB; if it does not 
        /// exist an error message is added to the report.  
        /// </summary>
        /// <param name="feSite"> - site information, instance of FeSite.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValSiteNoop(FeSite feSite,         // site info 
                                         ref short errCount,    // number of errors 
                                         ref short warnCount,   // number of warnings 
                                         string keyLine)        // key info to print 
        {
            MeSite meSite;
            string whereClause;
            SQLLEN[] nArrayMDB;
            int rc;

            // make sure site does exist 
            ValImport.FeFormWhereClause(out whereClause, feSite.location, "", "");

            rc = ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    keyLine = String.Format("Location: {0}", feSite.location);
                    ValErrs.AddMess("Site does not exist in MDB", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("SITE record locked in MDB", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("SITE undefined error: %d", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

        }

        /// <summary>
        /// This method validates individual fields in a 
        /// site record for the MDB operations of "ADD" and "UPDATE"; it ensures that 
        /// all required (mandated) information is indeed present in the site record.  
        /// </summary>
        /// <param name="feSite"> - site information, instance of FeSite.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feSite.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValSiteFields(FeSite feSite,     // site info 
                                   SQLLEN[] nArrayFW,   // null flags 
                                   string pdfName, // pdf name 
                                   ref short errCount,    // num errors 
                                   string keyLine)     // key info to print 
        {
            int recExist;
            SuNote suNote;
            SuOper suOper;


            // validate nots against snote table (nonum) 
            /*strncpy(siteOper, feSite.oper, 7);
                strncpy(siteNots, feSite.nots, 5);*/

            if (nArrayFW[FeSite.NOTS] != Constant.DB_NULL && feSite.nots.Length > 0)
            {
                recExist = Suutils.SuGetNote(feSite.oper, feSite.nots, out suNote);
                if (recExist != 0)
                {
                    ValErrs.AddMess("Nots (%s) not present in Note table for that Operator :%s:",
                                    keyLine, "E", feSite.nots, feSite.oper);
                    errCount++;
                }
            }

            if (nArrayFW[FeSite.OPER] != Constant.DB_NULL && feSite.oper.Length > 0)
            {
                recExist = Suutils.SuGetOper(feSite.oper, out suOper);
                if (recExist != 0)
                {
                    ValErrs.AddMess("Operator Code (%s) not present in operator table.",
                                    keyLine, "E", feSite.oper);
                    errCount++;
                }
                else if ((suOper.opnote[1] != 'C') && (suOper.opnote[1] != 'F'))
                {
                    ValErrs.AddMess("Operator (%s) is not a coordinating member",
                           keyLine, "E", feSite.oper);
                    errCount++;
                }

            }

            // verify that all required fields are in fact present 
            if ((feSite.cmd.Equals("A")) || (feSite.cmd.Equals("B")))
            {
                if (nArrayFW[FeSite.NAME] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Name must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.PROV] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Province must be present\r\n", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.OPER] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Operator must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.LATIT] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Latitude must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.LONGIT] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Longitude must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.GRND] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Ground Height must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.RADIO] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Radio must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.RAIN] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Rain must be present", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeSite.STATS] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Site Status must be present", keyLine, "E");
                    errCount++;
                }
            }
        }

        /// <summary>
        /// This method populates the operator-type field of an
        /// ES site record and sets its record-status field to "U".
        /// </summary>
        /// <param name="feSite"> - site information, instance of FeSite.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feSite.</param>
        public static void FeCalcSiteFields(FeSite feSite,       // site info 
                                            SQLLEN[] nArrayFW)    // null flags 
        {
            string oprTyp;
            SuOper suOper;
            int nRet;

            // get operator type 
            nRet = Suutils.SuGetOper(feSite.oper, out suOper);

            if (nRet == 0)
            {
                oprTyp = suOper.opnote.Substring(0, 1);
                oprTyp += "E";
                feSite.oprtyp = oprTyp;
                nArrayFW[FeSite.OPRTYP] = Constant.DB_NOT_NULL;
            }

            if (nArrayFW[FeSite.RECSTAT] == Constant.DB_NULL)
            {
                feSite.recstat = "U";
                nArrayFW[FeSite.RECSTAT] = Constant.DB_NOT_NULL;
            }
        }



    }
}
