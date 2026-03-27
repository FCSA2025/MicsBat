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
    /// This class provides methods that validate the 'change of location' information
    /// provided by a previously imported PDF.
    /// </summary>
    public class ValCLoc
    {


        /// <summary> 
        /// This method validates ES 'change of location' records.    
        /// </summary>
        /// <remarks>
        /// The validation for change of location simply verifies that:
        /// <list type="bullet">
        /// <item>the old location does exist;</item>
        /// <item>the new location does not already exist;</item>
        /// <item>the name old location combination is correct; and</item>
        /// <item>the old location is not referenced in the PDF.</item>
        /// </list>
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="errCount"> - cummulative count of errors.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        public static void FeValCLocES(string pdfName,    // pdf display name 
                                 ref short errCount,    // number of errors encountered 
                                 ref short warnCount)   // number of warnings encountered 
        {
            SQLLEN[] nArrayFW;
            SQLLEN[] nArrayMDB;
            int rc;
            int sID1;
            string tableName;
            string whereClause = "";
            string keyLine = "";
            MeSite meSite;
            FeCLoc feCLoc;

            GenUtil.UtCvtName(Constant.FE_CLOC, pdfName, out tableName);

            // Select location information 
            if ((sID1 = DynFeCLoc.FeSelectCLoc(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("could not read change location information. Reason: %d",
                                pdfName, "W", sID1.ToString());
                warnCount++;
                return;
            }

            // Read each change of location record 
            while (DynFeCLoc.FeFetchCLoc(sID1, out feCLoc, out nArrayFW) == Constant.SUCCESS)
            {
                keyLine = String.Format("KEY: {0} to {1}.", feCLoc.oldlocation, feCLoc.newlocation);

                // verify that all fields are there 

                if (nArrayFW[FeCLoc.OLDLOCATION] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Must Enter Old Location field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeCLoc.NEWLOCATION] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Must Enter New Location field.", keyLine, "E");
                    errCount++;
                }

                if (nArrayFW[FeCLoc.NAME] == Constant.DB_NULL)
                {
                    ValErrs.AddMess("Must Enter Name field.", keyLine, "E");
                    errCount++;
                }


                // -- Make sure old site location does exist in MDB -- 

                whereClause = String.Format("location = '{0}'", feCLoc.oldlocation);
                rc = ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        // verify the name 
                        if (Strings.StrnCmp(meSite.name, feCLoc.name, 16) != 0)
                        {
                            ValErrs.AddMess("Invalid LOCATION - NAME combination.", keyLine, "E");
                            errCount++;
                            break;
                        }

                        /* check to see that no other records contain
                        ** the new location from the change of location record.
                        */
                        if (FeCheckForLocationPropagate(pdfName,
                                                        feCLoc.newlocation,
                                                        ref warnCount,
                                                        keyLine) == true)
                        {
                            ValErrs.AddMess("Illegal reference to New Location in the pdf.", keyLine, "E");
                            errCount++;
                            break;
                        }
                        break;

                    case Constant.NOMORERECS:
                        ValErrs.AddMess("SITE does not exist.", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("SITE record locked in MDB.", keyLine, "E");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("SITE undefined error.", keyLine, "W");
                        warnCount++;
                        break;

                }   // End switch 


                // -- Warn if new site location already exists in MDB -- 

                whereClause = String.Format("location = '{0}'", feCLoc.newlocation);

                rc = ValRetrieveES.FeValRetrieveMDBSite(out meSite, whereClause, out nArrayMDB);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        ValErrs.AddMess("New Location exists in MDB. PDF may fail MDB update.",
                                        keyLine, "W");
                        warnCount++;
                        break;


                    case Constant.NOMORERECS:
                        // This case is OK - leave alone 
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("SITE record locked in MDB.", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("SITE undefined error.", keyLine, "W");
                        warnCount++;
                        break;

                }   // End switch 

            }   // End while 

            DynFeCLoc.FeCloseCLoc(sID1);

        }   // ----- End feValCLocES ----- 

        /// <summary>
        /// This method searches for PDF records that reference a prescribed
        /// site location; the search order is antennae, channels then sites. 
        /// </summary>
        /// <param name="pdfName"> - name of the PDF.</param>
        /// <param name="newlocation"> - location to search for.</param>
        /// <param name="warnCount"> - cummulative count of warnings.</param>
        /// <param name="keyLine"> - key message to print.</param>
        /// <returns>true if at least one record is found that references the location; otherwise false.</returns>
        public static bool FeCheckForLocationPropagate(string pdfName,                   // pdf display name 
                                                                string newlocation,     // old location 
                                                                ref short warnCount,    // number of warnings 
                                                                string keyLine)         // key info to print 
        {
            int ID1;                // cursor ID 
            SQLLEN[] nArrayFW;      // Null Array for ODBC 
            string tableName;       // Ingres internal table name 
            string whereClause;  // selection clause for SQL 
            FeChan feChan;          // Channel information 
            FeAnte feAnte;          // Antenna information 
            FeSite feSite;          // Channel information 

            // Initialization 
            GenUtil.UtCvtName(Constant.FE_ANTE, pdfName, out tableName);

            whereClause = String.Format("location = '{0}'", newlocation);

            // -- First, check the Antenna records -- 

            // Declare cursor for Ante. recs in PDF 
            if ((ID1 = DynFeAnte.FeSelectAnte(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot access Antenna information in PDF.  Reason: %d",
                                pdfName, "W", ID1.ToString());
                warnCount++;
                return (false);
            }

            // Try to read an Antenna record 
            if (DynFeAnte.FeFetchAnte(ID1, out feAnte, out nArrayFW) == Constant.SUCCESS)
            {
                // found a rec with changing location 
                DynFeAnte.FeCloseAnte(ID1);
                return (true);
            }
            DynFeAnte.FeCloseAnte(ID1);


            // -- Check the Channel records -- 

            GenUtil.UtCvtName(Constant.FE_CHAN, pdfName, out tableName);
            whereClause = String.Format("location = '{0}'", newlocation);

            // Declare cursor for Chan. recs in PDF 
            if ((ID1 = DynFeChan.FeSelectChan(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot access Channel information in PDF. Reason: %d",
                                pdfName, "W", ID1.ToString());
                warnCount++;
                return (false);
            }

            // Try to read a Channel record 
            if (DynFeChan.FeFetchChan(ID1, out feChan, out nArrayFW) == Constant.SUCCESS)
            {
                // found a rec with changing location 
                DynFeChan.FeCloseChan(ID1);
                return (true);
            }
            DynFeChan.FeCloseChan(ID1);

            // -- Check the Site records -- 
            GenUtil.UtCvtName(Constant.FE_SITE, pdfName, out tableName);
            whereClause = String.Format("location = '{0}' ", newlocation);

            // Declare cursor for Site recs in PDF 
            if ((ID1 = DynFeSite.FeSelectSite(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("Cannot access Site information in PDF. Reason: %d",
                                pdfName, "W", ID1.ToString());
                warnCount++;
                return (false);
            }

            // Try to read a Site record 
            if (DynFeSite.FeFetchSite(ID1, out feSite, out nArrayFW) == Constant.SUCCESS)
            {
                // found a rec with changing location 
                DynFeSite.FeCloseSite(ID1);
                return (true);
            }
            DynFeSite.FeCloseSite(ID1);


            /* If got this far, then found no 'propagated location values', so
            ** return false value to caller.
            */
            return (false);

        }	// --- End of feCheckForLocationPropagate ----- 







    }
}
