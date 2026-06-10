# Documented File: ValChng.cs
**Repository Path:** `FtValidate\ValChng.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
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
    /// Provides several utility methods used by FtValidate.
    /// </summary>
    public class ValChng
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValChngTS(string s, ref short sh1, ref short sh2);

        //--------------------------------------------------------------------------------

        public static void FtValChngTS_NATIVE(string s, ref short sh1, ref short sh2)
        {
            ftValChngTS(s, ref sh1, ref sh2);
        }
#endif
        /// <summary>
        /// This method reads all the call sign records in the PDF, 
        /// determines if each record is being added modified or deleted, 
        /// and validates accordingly.
        /// </summary>
        /// <param name="pdfName">- name of the PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        public static void FtValChngTS(string pdfName, ref short errCount, ref short warnCount)
        {
            //...Log2.v("\n\nValChng.FtValChngTS(): Entry");

            /* Local variables */
            int sID1;
            int rc;        /* return code for integers */
            int count;
            SQLLEN[] nArrayFW;
            SQLLEN[] nArrayMDB;
            string tableName;
            string chngcall;
            string keyLine;
            bool siteExistsInMDB = false;
            bool hasMissingField = false;

            MtSite mtSite;
            FtChng ftChng;

            // Collect all the 'GK' change records in a list so that we can
            // perform newCallSign duplication checking etc.
            List<FtChng> previousFtChngList = new List<FtChng>();

            /* Initialization */
            //string titleLine = "\r\nVALIDATION AGAINST SDB";
            string whereClause = "";
            string orderBy = "";
            string siteoper = "";

            GenUtil.UtCvtName(Constant.FT_CHNG_CALL, pdfName, out tableName);

            /* select change call sign records from PDF*/
            if ((sID1 = DynChange.FtSelectChngCall(tableName, whereClause, orderBy)) < 0)
            {
                ValErrs.AddMess("Could not read change call information.  Reason: %d",
                                            tableName, "W", sID1.ToString());
                warnCount++;
                return;
            }

            /* Read all change call sign records */
            while (DynChange.FtFetchChngCall(sID1, out ftChng, out nArrayFW) == Constant.SUCCESS)
            {
                siteExistsInMDB = false;
                hasMissingField = false;

                /* print key information */
                keyLine = String.Format("Change Call Sign: :{0}: to :{1}:", ftChng.oldcall1, ftChng.newcall1);

                //...Log2.v("\r\nValChng.FtValChngTS(): A:  " + keyLine);

                // Verify that all fields are present.
                // The user DB table ft_XXX_chng has qty. 3 fields:
                //     oldcall1 (not nullable).
                //     newcall1 (not nullable).
                //     name         (nullable).
                // Prior to bug fix b191204A missing fields were not being checked for
                // by FtImport.
                // Just to make sure, we will check for missing fields in the GK record
                // before performing more detailed validation checks.
                if (String.IsNullOrWhiteSpace(ftChng.oldcall1))
                {
                    ValErrs.AddMess("Must Enter Old Call Sign. ", keyLine, "E");
                    hasMissingField = true;
                    errCount++;
                }

                if (String.IsNullOrWhiteSpace(ftChng.newcall1))
                {
                    ValErrs.AddMess("Must Enter New Call Sign.", keyLine, "E");
                    hasMissingField = true;
                    errCount++;
                }

                if ((nArrayFW[FtChng.NAME] == Constant.DB_NULL) || String.IsNullOrWhiteSpace(ftChng.name))
                {
                    ValErrs.AddMess("Must Enter Name field.", keyLine, "E");
                    hasMissingField = true;
                    errCount++;
                }

                // If one or more fields are missing from the GK record stop processing
                // this FtChng object and start processing the next.
                if (hasMissingField) continue;

                /* -- CHNG UPDATE VALIDATION -- */

                /* - Make sure site with "old" call1 does exist in MDB - */
                whereClause = String.Format(" call1 = '{0}'", ftChng.oldcall1);
                rc = ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB);

                switch (rc)
                {
                    case Constant.SUCCESS:
                        //...Log2.v("\r\nValChng.FtValChngTS(): A-1");
                        siteoper = mtSite.oper.Trim();
                        /* verify the name */
                        //if (strncmp(mtSite.name, ftChng.name, 16) != 0)
                        if (!mtSite.name.Equals(ftChng.name))
                        {
                            ValErrs.AddMess("Invalid CALL1 - NAME (%s) combination",
                                                        keyLine, "E", ftChng.name);
                            errCount++;
                            break;
                        }
                        siteExistsInMDB = true;
                        break;

                    case Constant.NOMORERECS:
                        //...Log2.v("\r\nValChng.FtValChngTS(): A-2");
                        ValErrs.AddMess("SITE does not exist.", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.RECLOCK:
                        //...Log2.v("\r\nValChng.FtValChngTS(): A-3");
                        ValErrs.AddMess("SITE record locked  in MDB", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        //...Log2.v("\r\nValChng.FtValChngTS(): A-4");
                        ValErrs.AddMess("SITE undefined error: %d", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;

                } /* End switch */

                //...Log2.v("\r\nValChng.FtValChngTS(): B-1");
                /* - Warn if site with "new" call1 does exist in MDB - */
                whereClause = String.Format(" call1 = '{0}'", ftChng.newcall1);
                rc = ValRetrieveTS.FtValRetrieveMDBSite(out mtSite, whereClause, out nArrayMDB);

                bool noMoreRecs = false;
                switch (rc)
                {
                    case Constant.SUCCESS:
                        ValErrs.AddMess("New call sign already exists in MDB.", keyLine, "E");
                        errCount++;
                        break;

                    case Constant.NOMORERECS:
                        /* This case is OK - do nothing here */
                        noMoreRecs = true;
                        break;

                    case Constant.RECLOCK:
                        ValErrs.AddMess("SITE record locked  in MDB.", keyLine, "W");
                        warnCount++;
                        break;

                    default:
                        ValErrs.AddMess("SITE undefined error: %d", keyLine, "W", rc.ToString());
                        warnCount++;
                        break;

                } /* End switch */

                // Check whether the current FtChng object duplicates the oldCallSign and/or 
                // newCallSign of previous FtChng objects.
                string errMsg;

                if (DuplicateCallSigns(ftChng, ref previousFtChngList, out errMsg, ref errCount))
                {
                    ValErrs.AddMess(errMsg, keyLine, "E");
                }

                // If the oldCallSign does not exist in the MDB then this is as far as we can go.
                // Stop processing the current FtChng object and start processing the next.
                //...Log2.v("\r\nValChng.FtValChngTS(): B-1-1: siteExistsInMDB = " + siteExistsInMDB);
                if (!siteExistsInMDB) continue;

                //...Log2.v("\r\nValChng.FtValChngTS(): B-2");
                /* if a Passive site is being changed to a non-Passive,
                 * then we must ensure that antenna data has also been
                 * changed accordingly  */
                if ((ftChng.oldcall1[0] == '%') && (ftChng.newcall1[0] != '%'))
                {
                    if (ValFict.FtValFictChgPassive(pdfName, ftChng.oldcall1) != Constant.SUCCESS)
                    {
                        ValErrs.AddMess("A passive site changing to non-passive must also have its antenna information updated.", keyLine, "E");
                        errCount++;
                    }
                }

                //...Log2.v("\r\nValChng.FtValChngTS(): B-3");
                switch (ftChng.newcall1[0])
                {
                    case '$':
                        /* ensure that a '$' ficticious call sign is valid */
                        if (ValFict.FtValFictRxOnly(pdfName, ftChng.oldcall1) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("A site beginning with '$' must have all RX frequencies.",
                                                        keyLine, "E");
                            errCount++;
                        }
                        break;

                    case '=':
                        /* ensure that a '=' ficticious call sign is valid */
                        if (ValFict.FtValFictTx(pdfName, ftChng.oldcall1) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("A site beginning with '=' must have at least one TX frequency.",
                                                        keyLine, "E");
                            errCount++;
                        }
                        break;

                    case '%':
                        /* ensure that a '%' ficticious call sign is valid */
                        if (ValFict.FtValFictPassive(pdfName, ftChng.oldcall1) != Constant.SUCCESS)
                        {
                            ValErrs.AddMess("A site beginning with '%' must have passive antennas only.",
                                                        keyLine, "E");
                            errCount++;
                        }
                        break;

                    case ';':
                        /* no special rules for ';' ficticious call sign */
                        break;

                } /* End switch */

                //...Log2.v("\r\nValChng.FtValChngTS(): B-4");
                //...Log2.v("\r\nValChng.FtValChngTS(): ftChng.newcall1[0] = " + ftChng.newcall1[0]);
                //...Log2.v("\r\nValChng.FtValChngTS(): nArrayMDB[MtSite.OPER] = " + nArrayMDB[MtSite.OPER]);

                if (((ftChng.newcall1[0] == '=') || /* new sign is ficticious */
                    (ftChng.newcall1[0] == '$') ||
                    (ftChng.newcall1[0] == '%') ||
                    (ftChng.newcall1[0] == ';')) &&
                    (noMoreRecs || (nArrayMDB[MtSite.OPER] != Constant.DB_NULL)))
                {
                    //...Log2.v("\r\nValChng.FtValChngTS(): C");
                    bool done = false;
                    count = 3;
                    //...Log2.v("\r\nValChng.FtValChngTS(): siteoper = " + Strings.AddBars(siteoper));
                    while (!done && (count <= 6))
                    {
                        // strncpy(chngcall,&ftChng.newcall1[1], count);
                        chngcall = ftChng.newcall1.Substring(1, count);
                        //...Log2.v("\r\nValChng.FtValChngTS(): chngcall = " + Strings.AddBars(chngcall));
                        if (chngcall.Equals(siteoper))
                        {
                            done = true;
                        }
                        count++;
                    }
                    if (!done)
                    {
                        //...Log2.v("\r\nValChng.FtValChngTS(): D");
                        ValErrs.AddMess("The operator code in the new call sign must be the same as the operator code for this site.",
                                                    keyLine, "E");
                        errCount++;
                    }
                } /* End if new callsign is fictitious */

            } /* End of Read all change call sign records */


            DynChange.FtCloseChngCall(sID1);

            //...Log2.v("\n\nValChng.FtValChngTS(): Exit");
            return;
        }   /* ----- End of ftValChngTS ----- */

        /// <summary>
        /// This method checks whether a prescribed FtChng object's old and new CallSigns
        /// have been used before in previous 'GK' record(s); it also checks that the 
        /// current object's old and new CallSigns are different.
        /// </summary>
        /// <remarks>
        /// Added for bug fix <b>b190708A</b> - FtValidate: Change of Call Anomalies
        /// </remarks>
        /// <param name="ftChng"></param>
        /// <param name="previousFtChngList"></param>
        /// <param name="errMsg"></param>
        /// <param name="errCount"></param>
        /// <returns></returns>
        private static bool DuplicateCallSigns(FtChng ftChng, ref List<FtChng> previousFtChngList, out string errMsg, ref short errCount)
        {
            // 'out' requirement.
            errMsg = "";

            // Check the input.
            if (ftChng == null || previousFtChngList == null)
            {
                Log2.e("\nValChng.CheckForDuplicateCallSigns(): ERROR: invalid input.");
                return false;
            }

            // Check if the oldCallSign has been mentioned before in a previous 'GK' record (FtChng object).
            // This condition is checked for by FtImport but the following code is included for completeness.
            if (!String.IsNullOrWhiteSpace(ftChng.oldcall1))
            {
                foreach (FtChng prevFtChng in previousFtChngList)
                {
                    if (!String.IsNullOrWhiteSpace(prevFtChng.oldcall1))
                    {
                        if (ftChng.oldcall1.Equals(prevFtChng.oldcall1))
                        {
                            errMsg += String.Format("{0} has previously been used as an old Call Sign and cannot be reused.", ftChng.oldcall1);

                            errCount++;

                            break;
                        }

                    }
                }
            }

            // Check if the newCallSign has been mentioned before in a previous 'GK' record (FtChng object).
            if (!String.IsNullOrWhiteSpace(ftChng.newcall1))
            {
                foreach (FtChng prevFtChng in previousFtChngList)
                {
                    if (!String.IsNullOrWhiteSpace(prevFtChng.newcall1))
                    {
                        if (ftChng.newcall1.Equals(prevFtChng.newcall1))
                        {
                            string lineBreak = "";
                            if (!String.IsNullOrWhiteSpace(errMsg))
                            {
                                lineBreak = "\r\n";
                            }

                            errMsg += String.Format("{0}{1} has previously been used as a  new Call Sign and cannot be reused.",
                                                        lineBreak, ftChng.newcall1);
                            errCount++;

                            break;
                        }

                    }
                }
            }

            // Check that the current object's oldCallSign and newCallSign are different.
            if (!String.IsNullOrWhiteSpace(ftChng.oldcall1) && !String.IsNullOrWhiteSpace(ftChng.oldcall1))
            {
                if (ftChng.oldcall1.Equals(ftChng.newcall1))
                {
                    string lineBreak = "";
                    if (!String.IsNullOrWhiteSpace(errMsg))
                    {
                        lineBreak = "\r\n";
                    }

                    errMsg += String.Format("{0}{1} cannot be used as both the old Call Sign and the new Call Sign; they must be different.",
                                                lineBreak, ftChng.oldcall1);
                    errCount++;
                }

            }

            // We must add the current FtChng object to list list of previous objects.
            previousFtChngList.Add(ftChng);

            //...Log2.v("\nValChng.CheckForDuplicateCallSigns(): errMsg = \n" + errMsg);
            return !String.IsNullOrWhiteSpace(errMsg);
        }




    }
}

```
