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
    /// This class provides methods that validate the azimuth information
    /// provided by a previously imported PDF.
    /// </summary>
    public class ValAzim
    {
        private const string titleLine = @".VALIDATION AGAINST SDB";

        /// <summary>
        /// This method validates ES azimuth records i.a.w. their prescribed
        /// MDB operation (ADD, BLANK-OUT, DELETE, NO-CHANGE, UPDATE).  
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - number of errors encountered.</param>
        /// <param name="warnCount"> - number of warnings encountered.</param>
        /// <param name="azimRecChanged"> - boolean, true if the azimuth record has changed.</param>
        public static void FeValAzimES(string pdfName,    // pdf display name 
                                 ref short errCount,    // number of errors encountered 
                                 ref short warnCount,   // number of warnings encountered 
                                 ref bool azimRecChanged)
        {
            SQLLEN[] nArrayFW;
            int aID1;
            string whereClause;
            string tableName;
            string keyLine = "";
            FeAzim feAzim;

            whereClause = "";

            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);

            if (azimRecChanged)
            { // from merge process 
                warnCount++;
                ValErrs.AddMess("Azimuth record may have to be changed.", pdfName, "W");
            }


            // read azimuth information 
            if ((aID1 = DynFeAzim.FeSelectAzim(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read azimuth information. Reason: %d", tableName, "", "W");
                warnCount++;
                return;
            }

            while (DynFeAzim.FeFetchAzim(aID1, out feAzim, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} {1} {2:F1}", feAzim.location, feAzim.call1, feAzim.azim);

                if (feAzim.deleteall.Equals("Y"))
                {
                    FeValAzimDelAll(feAzim, pdfName, ref errCount, ref warnCount, keyLine);
                }
                else
                {
                    switch (feAzim.cmd[0])
                    {
                        case 'A':
                            FeValAzimAdd(feAzim, nArrayFW, pdfName, ref errCount, ref warnCount, keyLine);
                            break;

                        case 'D':
                            FeValAzimDel(feAzim, ref errCount, ref warnCount, keyLine);
                            break;

                        case 'U':
                        case 'B':
                            FeValAzimUpdt(feAzim, nArrayFW, pdfName, ref errCount, ref warnCount,
                                                        keyLine);
                            break;

                        case 'N':
                            FeValAzimNoop(feAzim, ref errCount, ref warnCount, keyLine);
                            break;

                        default:
                            ValErrs.AddMess("Command (%s) is invalid.", keyLine, "E", feAzim.cmd);
                            errCount++;
                            break;
                    }
                }
                DynFeAzim.FeUpdateAzim(aID1, feAzim, nArrayFW);
            }
            DynFeAzim.FeCloseAzim(aID1);
        }

        /// <summary>
        /// This method deletes all azimuth records from the PDF's _azim tables that have a prescribed location and call1; 
        /// this is necessary to avoid trying to add, update, or delete an azimuth record that either does not exist in the
        /// MDB or which will be getting deleted anyway; then copy in any matching (location, call1) azimuth records in the MDB into 
        /// the PDF's _azim table tagged with the MDB operation command 'D' for delete.  
        /// </summary>
        /// <param name="feAzim"> - azim info</param>
        /// <param name="pdfName"></param>
        /// <param name="errCount"> - number of errors</param>
        /// <param name="warnCount"> - number of warnings</param>
        /// <param name="keyLine"> - key information to print</param>
        /// <summary>
        public static void FeValAzimDelAll(FeAzim feAzim,// azim info 
                                                        string pdfName, // display name 
                                                        ref short errCount, // number of errors 
                                                        ref short warnCount,    // number of warnings 
                                                        string keyLine) // key information to print 
        {
            SQLLEN[] nArrayTemp;
            string whereClause;
            string tableName;
            int ID1;
            int nRet;
            FeAzim feTmpAzim;

            whereClause = String.Format("location = '{0}' and call1 = '{1}'", feAzim.location, feAzim.call1);

            // delete all azim recs 
            /* first go and delete all azim recs which are in the PDF for this
             * loc/call1 - this is necessary to avoid trying to add, update, or
             * delete a record which either does not exist in the MDB or which
             * will be getting deleted anyway */
            GenUtil.UtCvtName(Constant.FE_AZIM, pdfName, out tableName);

            if ((ID1 = DynFeAzim.FeSelectAzim(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Could not read azimuth information. Reason: %d",
                                keyLine, "W", ID1.ToString());
                warnCount++;
            }
            else
            {
                while (DynFeAzim.FeFetchAzim(ID1, out feTmpAzim, out nArrayTemp) == Constant.SUCCESS)
                {
                    if ((nRet = DynFeAzim.FeDeleteAzim(ID1)) != Constant.SUCCESS)
                    {
                        ValErrs.AddMess("Could not delete azimuth record. Reason: %d", keyLine, "W", nRet.ToString());
                        warnCount++;
                    }
                }
                DynFeAzim.FeCloseAzim(ID1);
            }

            // Copy in any matching (location, call1) azimuth records in the MDB into 
            // the PDF's _azim table tagged with the MDB operation command 'D' for delete.
            ValImport.FeImportAzimuth(pdfName, whereClause, "D");
        }


        /// <summary>
        /// This method validates ES PDF azimuth records whose MDB operation is "ADD".  
        /// </summary>
        /// <remarks>
        /// This procedure performs the azimuth validation for an ADD record. It 
        /// verifies that the record does not already exist and then calls lower-level 
        /// methods that verify that all fields have the correct format/syntax and
        /// numerical bounds.  
        /// </remarks>
        /// <param name="feAzim"> - azimuth information, an instance of FeAzim.</param>
        /// <param name="nArrayFW"> - null flags for feAzim.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAzimAdd(FeAzim feAzim,// azim info 
                                                         SQLLEN[] nArrayFW, // null flags 
                                                         string pdfName,    // display name 
                                                         ref short errCount,    // number of errors 
                                                         ref short warnCount,   // number of warnings 
                                                         string keyLine)        // key information to print 
        {
            SQLLEN[] nArrayMDB;
            short rc;
            string whereClause;
            MeAzim meAzim;

            // make sure azimuth does not already exist 
            whereClause = String.Format("location = '{0}' and call1 = '{1}' and azim = {2}",
                            feAzim.location, feAzim.call1, feAzim.azim);

            rc = (short)ValRetrieveES.FeValRetrieveMDBAzimuth(out meAzim, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    ValErrs.AddMess("Azimuth record already exists in MDB, cannot ADD.", keyLine, "E");
                    errCount++;
                    break;

                case Constant.NOMORERECS:
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Azimuth record locked in MDB.", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Azimuth record undefined error (%d).", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }

            FeValAzimFields(feAzim, pdfName, nArrayFW, ref errCount, ref warnCount, keyLine);
        }

        /// <summary>
        /// This method validates an ES azimuth record whose MDB operation is "DELETE"; it 
        /// verifies that the record does indeed exist in the MDB.
        /// </summary>
        /// <param name="feAzim"> - azimuth information, an instance of FeAzim.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAzimDel(FeAzim feAzim,// azim info 
                                                         ref short errCount,    // number of errors 
                                                         ref short warnCount,   // number of warnings 
                                                         string keyLine)        // key information to print 
        {
            SQLLEN[] nArrayMDB;
            short rc;
            string whereClause;
            MeAzim meAzim;

            whereClause = String.Format("location = '{0}' and call1 = '{1}' and azim = {2}",
                            feAzim.location, feAzim.call1, feAzim.azim);

            if ((!String.IsNullOrWhiteSpace(feAzim.recstat)) && (feAzim.recstat[0] != 'C'))
            {
                // if user generated, make sure it exists in the MDB 
                rc = (short)ValRetrieveES.FeValRetrieveMDBAzimuth(out meAzim, whereClause, out nArrayMDB);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        break;

                    case Constant.NOMORERECS:
                        ValErrs.AddMess("Azimuth does not exist in MDB.", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("Azimuth record locked  in MDB.", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("Azimuth record undefined error (%d).", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;
                }
            }
        }

        /// <summary>
        /// This method validates ES azimuth records whose MDB operation is "UPDATE".  
        /// </summary>
        /// <remarks>
        /// This method performs the azimuth validation for an UPDATE record. It verifies 
        /// that the record does indeed already exist and calls a field validation method to 
        /// verify that all its fields are correct.  
        /// </remarks>
        /// <param name="feAzim"> - azimuth information, instance of FeAzim.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feAzim.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAzimUpdt(FeAzim feAzim,// azim info 
                      SQLLEN[] nArrayFW,    // null flags 
                      string pdfName,   // display name 
                      ref short errCount,   // number of errors 
                      ref short warnCount,  // number of warnings 
                      string keyLine)   // key information to print 
        {
            short rc;
            SQLLEN[] nArrayMDB;
            string whereClause;
            MeAzim meAzim;

            // make sure azim does exist 
            whereClause = String.Format("location = '{0}' and call1 = '{1}' and azim = {2}",
                            feAzim.location, feAzim.call1, feAzim.azim);

            rc = (short)ValRetrieveES.FeValRetrieveMDBAzimuth(out meAzim, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Azimuth does not exist in MDB.", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Azimuth record locked  in MDB.", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Azimuth record undefined error (%d).", keyLine, "W", rc.ToString());
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

            // Check modify date and time 
            if (((!feAzim.mdate.Equals(meAzim.mdate))) ||
                 ((!feAzim.mtime.Equals(meAzim.mtime))))
            {
                // Modify dates/times don't match 
                ValErrs.AddMess("This Azimuth record has been modified since you retrieved it.",
                                keyLine, "W");
                warnCount++;
            }

            FeValAzimFields(feAzim, pdfName, nArrayFW, ref errCount, ref warnCount, keyLine);
        }

        /// <summary>
        /// This method validates that an azimuth record already exists in the MDB; if it does not 
        /// exist an error message is added to the report.  
        /// </summary>
        /// <param name="feAzim"> - azimuth information, instance of FeAzim.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAzimNoop(FeAzim feAzim,// azim info 
                                                            ref short errCount,    // number of errors 
                                                            ref short warnCount,   // number of warnings 
                                                            string keyLine) // key information to print 
        {
            short rc;
            SQLLEN[] nArrayMDB;
            string whereClause;
            MeAzim meAzim;

            // make sure azim does exist 
            whereClause = String.Format("location = '{0}' and call1 = '{1}' and azim = {2}",
                            feAzim.location, feAzim.call1, feAzim.azim);

            rc = (short)ValRetrieveES.FeValRetrieveMDBAzimuth(out meAzim, whereClause, out nArrayMDB);
            switch (rc)
            {
                case Constant.SUCCESS:
                    break;

                case Constant.NOMORERECS:
                    ValErrs.AddMess("Azimuth does not exist in MDB.", keyLine, "E");
                    errCount++;
                    break;

                case Constant.RECLOCK:
                    ValErrs.AddMess("Azimuth record locked  in MDB.", keyLine, "W");
                    warnCount++;
                    break;

                default:
                    ValErrs.AddMess("Azimuth record undefined error. (%d)", keyLine, "W", rc.ToString());
                    warnCount++;
                    break;
            }
        }

        /// <summary>
        /// This method validates individual fields in an ES 
        /// azimuth record for the MDB operations of "ADD" and "UPDATE"; it ensures that 
        /// all required (mandated) information is indeed present in the azimuth record.  
        /// </summary>
        /// <param name="feAzim"> - azimuth information, instance of FeAzim.</param>
        /// <param name="nArrayFW"> - array with ODBC null flags for feAzim.</param>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative number of errors.</param>
        /// <param name="warnCount"> - cummulative number of warnings.</param>
        /// <param name="keyLine"> - key info to print.</param>
        public static void FeValAzimFields(FeAzim feAzim,// azim info 
                                                        string pdfName, // display name 
                                                        SQLLEN[] nArrayFW,  // array nulls 
                                                        ref short errCount,    // number of errors 
                                                        ref short warnCount,   // number of warnings 
                                                        string keyLine) // key information to print 
        {
            SQLLEN[] nArrayTemp;
            short rc;
            int aID1;
            string whereClause;
            string tableName;
            FeAnte feAnte;

            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            whereClause = String.Format("location = '{0}' and call1 = '{1}'", feAzim.location, feAzim.call1);

            if ((aID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot read antenna information. Reason: %d", keyLine, "W", aID1.ToString());
                warnCount++;
            }
            else
            {
                if ((rc = (short)DynFeAnte.FeFetchAnte(aID1, out feAnte, out nArrayTemp)) != Constant.SUCCESS)
                {
                    if (rc == Error.DYN_MS_SQL_SERVER_ERR)
                    {
                        ValErrs.AddMess("Could not read Antenna. Reason: %d", keyLine, "W", rc.ToString());
                        warnCount++;
                    }
                    else
                    {
                        ValErrs.AddMess("No antenna exists for this azimuth.", keyLine, "E");
                        errCount++;
                    }
                }
                DynFeAnte.FeCloseAnte(aID1);
            }

            if ((feAzim.cmd.Equals("A")) || (feAzim.cmd.Equals("B")))
            {
                if (nArrayFW[FeAzim.ELEV] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Elevation must be present.", keyLine, "E");
                    errCount++;
                }
                else
                {
                    // ensure that elevation is between 0 and 90 degrees 
                    if ((feAzim.elev < -90.00) || (feAzim.elev > 90.00))
                    {
                        ValErrs.AddMess("Elevation must be between -90.00 and 90.00", keyLine, "E");
                        errCount++;
                    }
                }

                if (nArrayFW[FeAzim.DIST] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Distance must be present", keyLine, "E");
                    errCount++;
                }
                else
                {
                    // ensure that distance falls between 0 and 999.99 m. 
                    if ((feAzim.dist < 0.00) || (feAzim.dist > 999.99))
                    {
                        ValErrs.AddMess("Distance must be between 0.00 and 999.99", keyLine, "E");
                        errCount++;
                    }
                }
            }
        }





    }
}
