# Documented File: ValMerge.cs
**Repository Path:** `FtValidate\ValMerge.cs`
**Primary Layer:** `FtValidate`
**Namespace:** `FtValidate`

## Source Code Representation
```csharp
using _Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _Utillib;

namespace FtValidate
{
    using _DataStructures;

    using _NewLib;
    using SQLCHAR = Byte;
    using SQLCHARPTR = String;  //Invented to mimic (char *)
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLHENV = IntPtr;
    using SQLHSTMT = IntPtr;
    using SQLINTEGER = Int32;
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
    /// Provides methods related to the validation of TS site, antenna and channel 
    /// information that is to be merged with existing information in the database.
    /// </summary>
    public class ValMerge
    {
#if PINVOKE
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        public static extern void ftValMergeSiteRecords(string s);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValMergeAnteRecords(string s);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftValMergeChanRecords(string s, ref short sh1, ref short sh2);

        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern int ftValMergePassiveLinks(string s, ref short sh1, ref short sh2);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void siteDel([In] string pdf, [In] string call1);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void siteMerge([In] MtSite mtSite, [In] SQLLEN[] nullsMDB, [In, Out] ref FtSite ftSite, [In, Out] ref SQLLEN[] nullsPDF, [In, Out] ref uint pulled);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void ftGetChannelsAntenna([In] string pdfName, [In] string call1, int site, [In] string cmdVal);
        [DllImport("ftValidate.dll", CharSet = CharSet.Ansi)]
        private static extern void anteMerge([In] MtAnte mtAnte, [In] SQLLEN[] nullsMDB, [In, Out] FtAnte ftAnte, [In, Out] SQLLEN[] nullsPDF, [In, Out] uint[] pulled);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern SQLHDBC getConnFromMtAnteCursor([In] int n);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern SQLHSTMT getStmtFromMtAnteCursor([In] int n);

        public static void FtGetChannelsAntenna_NATIVE(string pdfName, string call1, int site, string cmdVal)
        {
            ftGetChannelsAntenna(pdfName, call1, site, cmdVal);
        }
        public static void AnteMerge_NATIVE(MtAnte mtAnte, SQLLEN[] nullsMDB, ref FtAnte ftAnte, ref SQLLEN[] nullsPDF, ref uint[] pulled)
        {
            anteMerge(mtAnte, nullsMDB, ftAnte, nullsPDF, pulled);
        }
        public static void SiteMerge_NATIVE(MtSite mtSite, SQLLEN[] nullsMDB, ref FtSite ftSite, ref SQLLEN[] nullsPDF, ref uint pulled)
        {
            siteMerge(mtSite, nullsMDB, ref ftSite, ref nullsPDF, ref pulled);
        }

        public static int FtValMergePassiveLinks_NATIVE(string pdfName, ref short errorCount, ref short warningsCount)
        {
            //...Log2.v("\n\nFtValMergePassiveLinks_NATIVE: Entry");
            int rc = ftValMergePassiveLinks(pdfName, ref errorCount, ref warningsCount);
            //...Log2.v("\n\nFtValMergePassiveLinks_NATIVE: Exit");
            return rc;
        }

        public static void FtValMergeChanRecords_NATIVE(string pdfName, ref short errorCount, ref short warningsCount)
        {
            ftValMergeChanRecords(pdfName, ref errorCount, ref warningsCount);
        }

        public static void FtValMergeAnteRecords_NATIVE(string pdfName)
        {
            ftValMergeAnteRecords(pdfName);
        }

#endif
        //---------------------------------------------------------------------------------------------------

        private static bool glb_powerchange = false;

        //---------------------------------------------------------------------------------------------------


        /// <summary>
        /// This method merges the site information in the MDB and in the PDF.
        /// </summary>
        /// <remarks>
        /// It verifies each field to see if it is blank(NULL) and, if so, takes the value 
        /// from the MDB and puts it in the right spot depending on which fields have been modified.
        /// It also extracts the records required for TSIP calculations and remote/local record maintanence.
        /// </remarks>
        /// <param name="shortName"></param>
        public static void FtValMergeSiteRecords(string shortName)
        {
            //...Log2.v("\n\nValMerge.FtValMergeSiteRecords(): Entry");

            SQLLEN[] nArrayFW = Arrays.CreateArrayUsingDefaultElementConstructor<SQLLEN>(Constant.FT_SITE_SIZE_);

            int sID1;
            string tableName;

            FtSite ftSite = new FtSite();

            int nRet;
            MtSiteStr pSite = new MtSiteStr(MtSiteStr.Init.ALLOCATED);
            MtSiteStrNulls pSiteNull = new MtSiteStrNulls(MtSiteStrNulls.Init.ALLOCATED);

            /* Get long name of PDF */
            GenUtil.UtCvtName(Constant.FT_SITE, shortName, out tableName);

            //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): A");

            /* Read site information (from PDF). Select ALL sites. */
            if ((sID1 = DynSite.FtSelectSite(tableName, "", "")) < 0)
            {
                //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): B");
                Log2.e("\r\nValMerge.FtValMergeSiteRecords(): ERROR: FtSelectSite() returned " + sID1);

                //ValErrs.AddMess("MERGE 1 - Could not read site information. Reason %d", tableName, "E", sID1);
                ValErrs.AddMess("MERGE 1 - Could not read site information. Reason " + sID1, tableName, "E");

                ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                return;
            }

            /* Read all PDF site records */
            while (DynSite.FtFetchSite(sID1, out ftSite, out nArrayFW) == 0)
            {
                //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): C");
                //...Log2.v("\n\nValMerge.FtValMergeSiteRecords(): ftSite: " + ftSite.ToString());

                // Bug fix: b150722A
                // If the ftSite has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftSite to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftSite.cmd))
                {
                    continue;
                }

                /* Initialise the pulled array to no records (0) */
                if (ftSite.CmdEquals('A'))
                {
                    //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): D");
                    /* No need to perform merge function on 'A'dd cases */
                    continue;
                }

                nRet = MtUtils.MtGetSiteWN(ftSite.call1, out pSite, 1, out pSiteNull);
                //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): call to MtGetSiteWN() returned " + nRet);

                if (nRet != 0)
                {
                    //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): E");
                    continue;
                }

                //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): pSite: " + pSite.ToString());

                if (ftSite.CmdEquals('N'))
                {
                    //...Log2.v("\r\nValMerge.FtValMergeSiteRecords(): F");

                    /* No op. Just copy record from MDB into PDF */
                    // Copy: ftSite.field = mtSite.Field for all fields of ftSite other than 'cmd' and 'recstat'.
                    // The corresponding NullInds are also copied from mtSite to ftSite.
                    FtValCopy.FtCopySite(ref ftSite, pSite.stSite, ref nArrayFW, pSiteNull.anSiteNull); //(Tgt, Src, Tgt, Src)

                    /* Write the record back to PDF */
                    int rC;
                    if ((rC = DynSite.FtUpdateSite(sID1, ftSite, nArrayFW)) != Constant.SUCCESS)
                    {
                        Log2.e("\r\nValMerge.FtValMergeSiteRecords(): ERROR: FtUpdateSite() returned " + rC);
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }

                    //The following native calls to free local resources do not need to be
                    //coded in C# because once pSite and pSiteNull go out of scope (e.g. the
                    //method returns) the garbage collector will free these resources.
                    //            mtFreeSite(pSite);
                    //            mtFreeNulls(pSiteNull);

                    continue;	/* No further processing required */
                }

                if (ftSite.CmdEquals('D'))
                {
                    //Get all associated antennae and channels and set them for deletion.
                    SiteDel(shortName, ftSite.call1);
                    continue;
                }


                //      
                //		The following blocks of code will test each field, if
                //		it is blank it will copy the value stored in the MDB,
                //		if it is not blank it will test to see if it hasutTestBit
                //		changed.  If changed then it will set flags to extract
                //		the correct records.  After all fields have been checked
                //		we then extract the necessary records as indicated by the
                //		validation rules document.
                //		

                uint[] pulledBitMap = new uint[1];
                pulledBitMap[0] = 0;
                SiteMerge(pSite.stSite, pSiteNull.anSiteNull, ref ftSite, ref nArrayFW, ref pulledBitMap);

                int rc;
                if ((rc = DynSite.FtUpdateSite(sID1, ftSite, nArrayFW)) != Constant.SUCCESS)
                {
                    Log2.e("\r\nValMerge.FtValMergeSiteRecords(): ERROR: FtUpdateSite() returned " + rc);
                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }

                // Depending on the outcome of testing all of the
                // individual fields, we will need to extract the
                // required local and remote records for modification.
                // If cmd is Noop don't extract additional records.

                // Retrieve all local information
                string whereClause;
                ValImport.FtFormWhereClause(out whereClause, ftSite.call1, "", "", Constant.NO_ANUM, "");

                if (GenUtil.UtTestBit(pulledBitMap, Constant.LOCAL_ANTENNA) == Enums.BIT.SET)
                {
                    ValImport.FtImportAntenna(shortName, whereClause, "N");
                }

                if (GenUtil.UtTestBit(pulledBitMap, Constant.LOCAL_CHANNEL) == Enums.BIT.SET)
                {
                    if (GenUtil.UtTestBit(pulledBitMap, Constant.RXPOWER) == Enums.BIT.SET)
                    {

                        ValImport.FtImportChannel(shortName, whereClause, "U");
                    }
                    else
                    {

                        ValImport.FtImportChannel(shortName, whereClause, "N");
                    }
                }

                // retrieve all remote information */
                // Obtain the Remote info by looking where MDB.call1 is same
                // as PDF.call2

                ValImport.FtFormWhereClause(out whereClause, "", ftSite.call1, "", Constant.NO_ANUM, "");
                if (GenUtil.UtTestBit(pulledBitMap, Constant.REMOTE_CHANNEL) == Enums.BIT.SET)
                {
                    if (GenUtil.UtTestBit(pulledBitMap, Constant.RXPOWER) == Enums.BIT.SET)
                    {

                        ValImport.FtImportChannel(shortName, whereClause, "U");
                    }
                    else
                    {

                        ValImport.FtImportChannel(shortName, whereClause, "N");
                    }
                }

                // It is important that this is the last to be extracted
                // as it relies on the remote channels having been
                // extracted into the PDF

                if (GenUtil.UtTestBit(pulledBitMap, Constant.REMOTE_ANTENNA) == Enums.BIT.SET)
                {
                    FtGetChannelsAntenna(shortName, ftSite.call1, true, "N");
                }

            } //where loop


            DynSite.FtCloseSite(sID1);

            //...Log2.v("\n\nValMerge.FtValMergeSiteRecords(): Exit()");
        }   /* ----- End of ftValMergeSiteRecords ----- */


        /// <summary>
        /// Performs the merging of site record information on a field by field basis.
        /// </summary>
        /// <param name="mtSite"> - MDB site record object.</param>
        /// <param name="nullsMDB"> - array of ODBC nullInds for mtSite fields.</param>
        /// <param name="ftSite"> - PDF site record.</param>
        /// <param name="nullsPDF"> - array of ODBC nullInds for ftSite fields.</param>
        /// <param name="pulled"> - bitmap for records needed.</param>
        public static void SiteMerge(MtSite mtSite, SQLLEN[] nullsMDB, ref FtSite ftSite, ref SQLLEN[] nullsPDF, ref uint[] pulled)
        {
            if (MergeFieldString(ftSite.cmd, mtSite.call1, nullsMDB[MtSite.CALL1],
                    ref ftSite.call1, ref nullsPDF[FtSite.CALL1]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
            }

            MergeFieldString(ftSite.cmd, mtSite.name, nullsMDB[MtSite.NAME],
                   ref ftSite.name, ref nullsPDF[FtSite.NAME]);

            MergeFieldString(ftSite.cmd, mtSite.prov, nullsMDB[MtSite.PROV],
                   ref ftSite.prov, ref nullsPDF[FtSite.PROV]);

            if (MergeFieldString(ftSite.cmd, mtSite.oper, nullsMDB[MtSite.OPER],
                    ref ftSite.oper, ref nullsPDF[FtSite.OPER]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
            }

            if (MergeFieldInt(ftSite.cmd, mtSite.latit, nullsMDB[MtSite.LATIT],
                    ref ftSite.latit, ref nullsPDF[FtSite.LATIT]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldInt(ftSite.cmd, mtSite.longit, nullsMDB[MtSite.LONGIT],
                   ref ftSite.longit, ref nullsPDF[FtSite.LONGIT]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldFloat(ftSite.cmd, mtSite.grnd, nullsMDB[MtSite.GRND],
                    ref ftSite.grnd, ref nullsPDF[FtSite.GRND]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            MergeFieldString(ftSite.cmd, mtSite.stats, nullsMDB[MtSite.STATS],
                   ref ftSite.stats, ref nullsPDF[FtSite.STATS]);

            MergeFieldString(ftSite.cmd, mtSite.sdate, nullsMDB[MtSite.SDATE],
                   ref ftSite.sdate, ref nullsPDF[FtSite.SDATE]);

            MergeFieldString(ftSite.cmd, mtSite.loc, nullsMDB[MtSite.LOC],
                   ref ftSite.loc, ref nullsPDF[FtSite.LOC]);

            MergeFieldString(ftSite.cmd, mtSite.icaccount, nullsMDB[MtSite.ICACCOUNT],
                   ref ftSite.icaccount, ref nullsPDF[FtSite.ICACCOUNT]);

            MergeFieldString(ftSite.cmd, mtSite.reg, nullsMDB[MtSite.REG],
                   ref ftSite.reg, ref nullsPDF[FtSite.REG]);

            MergeFieldString(ftSite.cmd, mtSite.spoint, nullsMDB[MtSite.SPOINT],
                   ref ftSite.spoint, ref nullsPDF[FtSite.SPOINT]);

            MergeFieldString(ftSite.cmd, mtSite.nots, nullsMDB[MtSite.NOTS],
                   ref ftSite.nots, ref nullsPDF[FtSite.NOTS]);

            if (MergeFieldString(ftSite.cmd, mtSite.oprtyp, nullsMDB[MtSite.OPRTYP],
                    ref ftSite.oprtyp, ref nullsPDF[FtSite.OPRTYP]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
            }

            MergeFieldString(ftSite.cmd, mtSite.snumb, nullsMDB[MtSite.SNUMB],
                   ref ftSite.snumb, ref nullsPDF[FtSite.SNUMB]);


            MergeFieldShort(ftSite.cmd, mtSite.notwr, nullsMDB[MtSite.NOTWR],
                   ref ftSite.notwr, ref nullsPDF[FtSite.NOTWR]);

            if (nullsPDF[FtSite.BANDWD1] == Constant.DB_NULL)
            {
                /* Copy all 8 bandwords */
                ftSite.bandwd1 = mtSite.bandwd1;
                nullsPDF[FtSite.BANDWD1] = nullsMDB[MtSite.BANDWD1];
                ftSite.bandwd2 = mtSite.bandwd2;
                nullsPDF[FtSite.BANDWD2] = nullsMDB[MtSite.BANDWD2];
                ftSite.bandwd3 = mtSite.bandwd3;
                nullsPDF[FtSite.BANDWD3] = nullsMDB[MtSite.BANDWD3];
                ftSite.bandwd4 = mtSite.bandwd4;
                nullsPDF[FtSite.BANDWD4] = nullsMDB[MtSite.BANDWD4];
                ftSite.bandwd5 = mtSite.bandwd5;
                nullsPDF[FtSite.BANDWD5] = nullsMDB[MtSite.BANDWD5];
                ftSite.bandwd6 = mtSite.bandwd6;
                nullsPDF[FtSite.BANDWD6] = nullsMDB[MtSite.BANDWD6];
                ftSite.bandwd7 = mtSite.bandwd7;
                nullsPDF[FtSite.BANDWD7] = nullsMDB[MtSite.BANDWD7];
                ftSite.bandwd8 = mtSite.bandwd8;
                nullsPDF[FtSite.BANDWD8] = nullsMDB[MtSite.BANDWD8];
            }

            if (nullsPDF[FtSite.MDATE] == Constant.DB_NULL)
            {
                ftSite.mdate = mtSite.mdate;
                nullsPDF[FtSite.MDATE] = nullsMDB[MtSite.MDATE];
            }

            if (nullsPDF[FtSite.MTIME] == Constant.DB_NULL)
            {
                ftSite.mtime = mtSite.mtime;
                nullsPDF[FtSite.MTIME] = nullsMDB[MtSite.MTIME];
            }

        }   /* ----- End of siteMerge ----- */


        /// <summary>
        /// Insert into a PDF all the antennas and channels for a site in the MDB.
        /// </summary>
        /// <remarks>
        /// Insert into a PDF all the antennas and channels for a site in the MDB.
        /// with the given 'call1'; remote antennas and sites are also inserted where call2 = 'call1'.
        /// </remarks>
        /// <param name="pdf"> - name of the PDF file.</param>
        /// <param name="call1"> - call1 of site record whose associated antennae and channels are to be deleted.</param>
        public static void SiteDel(string pdf, string call1)        /* Call1 of site being used */
        {
            //...Log2.v("\n\nValMerge.SiteDel(): Entry");

            /* Local variables */
            //char whereClause[WHERE_SIZE];       /* Dynamic where clause */
            int rc = Constant.SUCCESS;

            /* Make sure that there are not any NO OP antenna
             * records in the PDF which should be 'D'eletes
             * because the site is being deleted
             * Can ignore values: A, B, U, D - will be OK.
            */
            //sprintf(whereClause, " (call1 = '%s' or call2 = '%s') and cmd = 'N' ", call1, call1);
            string whereClause = String.Format(" (call1 = '{0}' or call2 = '{1}') and cmd = 'N' ", call1, call1);

            rc = ValImport.FtSetAnteCmd(pdf, whereClause, "D");
            if (rc != Constant.SUCCESS)
            {
                //ValErrs.AddMess("MERGE 9 - Could not set antenna command values", pdf, "E");
                ValErrs.AddMess("MERGE 9 - Could not set antenna command values", pdf, "E");
            }

            /* Make sure that there are not any NO OP channel
             * records in the PDF which should be 'D'eletes
             * because the site is being deleted
             * Can ignore values: A, B, U, D - will be OK.
            */
            //sprintf(whereClause, " (call1 = '%s' or call2 = '%s') and cmd = 'N' ", call1, call1);
            whereClause = String.Format(" (call1 = '{0}' or call2 = '{1}') and cmd = 'N' ", call1, call1);

            rc = ValImport.FtSetChanCmd(pdf, whereClause, "D");
            if (rc != Constant.SUCCESS)
            {
                ValErrs.AddMess("MERGE 10 - Could not set channel command values", pdf, "E");
            }

            /// * Set up a where clause which specifies the records to be delt with */
            //sprintf(whereClause, " call1 = '%s' or call2 = '%s'", call1, call1);
            whereClause = String.Format(" call1 = '{0}' or call2 = '{1}' ", call1, call1);

            /// * Get all the antennas which correspond to the selection criteria */
            /// * Set their cmd fields to 'D'elete */
            ValImport.FtImportAntenna(pdf, whereClause, "D");

            /// * Get all the channels which correspond to the selection criteria */
            /// * Set their cmd fields to 'D'elete */
            ValImport.FtImportChannel(pdf, whereClause, "D");

            //...Log2.v("\n\nValMerge.SiteDel(): Exit");
            /* Return to caller */
            return;

        }   /* ----- End of SiteDel ----- */

        /// <summary>
        /// Performs a merge of a MDB field, and its corresponding PDF field, 
        /// where the field is type 'string'.
        /// </summary>
        /// <param name="cmd"> - command prescribed in PDF.</param>
        /// <param name="fieldMDB"> - the MDB field value.</param>
        /// <param name="nullIndMDB"> - an ODBC nullInd for fieldMDB.</param>
        /// <param name="fieldPDF"> - the PDF field value.</param>
        /// <param name="nullIndPDF"> - an ODBC nullInd for fieldPDF.</param>
        /// <returns>Enums.MDB.WILL_NOT_CHANGE or Enums.MDB.WILL_CHANGE.</returns>
        public static Enums.MDB MergeFieldString(string cmd, string fieldMDB, SQLLEN nullIndMDB, ref string fieldPDF, ref SQLLEN nullIndPDF)
        {
            //...Log2.v("\n\nValMerge.MergeFieldString(): Entry");

            Enums.MDB rc = Enums.MDB.WILL_NOT_CHANGE;  //default.

            // If both fields are tagged with nulls we have nothing to do.
            if ((nullIndPDF == Constant.DB_NULL) && (nullIndMDB == Constant.DB_NULL))
            {
                return (rc);
            }

            // UseCase: field value and/or nullInd value differ between MDB and PDF.
            if ((nullIndPDF != nullIndMDB) || !fieldMDB.Equals(fieldPDF))
            {
                // MDB != PDF.
                if ((nullIndPDF == Constant.DB_NULL) && (!cmd.Equals("B")))  // "B" for "blank out optional field value"
                {
                    /* User wants data to be copied from MDB.  */
                    fieldPDF = fieldMDB;
                    nullIndPDF = nullIndMDB;
                }
                else
                {
                    // MDB is being changed.
                    rc = Enums.MDB.WILL_CHANGE;
                }
            }

            //...Log2.v("\n\nValMerge.MergeFieldString(): Exit");
            return rc;
        }

        /// <summary>
        /// Performs a merge of a MDB field, and its corresponding PDF field, 
        /// where the field is type 'short'.
        /// </summary>
        /// <param name="cmd"> - command prescribed in PDF.</param>
        /// <param name="fieldMDB"> - the MDB field value.</param>
        /// <param name="nullIndMDB"> - an ODBC nullInd for fieldMDB.</param>
        /// <param name="fieldPDF"> - the PDF field value.</param>
        /// <param name="nullIndPDF"> - an ODBC nullInd for fieldPDF.</param>
        /// <returns>Enums.MDB.WILL_NOT_CHANGE or Enums.MDB.WILL_CHANGE.</returns>
        public static Enums.MDB MergeFieldShort(string cmd, short fieldMDB, SQLLEN nullIndMDB, ref short fieldPDF, ref SQLLEN nullIndPDF)
        {
            Enums.MDB rc = Enums.MDB.WILL_NOT_CHANGE;  //default.

            // If both fields are tagged with nulls we have nothing to do.
            if ((nullIndPDF == Constant.DB_NULL) && (nullIndMDB == Constant.DB_NULL))
            {
                return (rc);
            }

            // UseCase: field value and/or nullInd value differ between MDB and PDF.
            if ((nullIndPDF != nullIndMDB) || !fieldMDB.Equals(fieldPDF))
            {
                // MDB != PDF.
                if ((nullIndPDF == Constant.DB_NULL) && (!cmd.Equals("B")))  // "B" for "blank out optional field value"
                {
                    /* User wants data to be copied from MDB.  */
                    fieldPDF = fieldMDB;
                    nullIndPDF = nullIndMDB;
                }
                else
                {
                    // MDB is being changed.
                    rc = Enums.MDB.WILL_CHANGE;
                }
            }
            return rc;
        }

        /// <summary>
        /// Performs a merge of a MDB field, and its corresponding PDF field, 
        /// where the field is type 'int'.
        /// </summary>
        /// <param name="cmd"> - command prescribed in PDF.</param>
        /// <param name="fieldMDB"> - the MDB field value.</param>
        /// <param name="nullIndMDB"> - an ODBC nullInd for fieldMDB.</param>
        /// <param name="fieldPDF"> - the PDF field value.</param>
        /// <param name="nullIndPDF"> - an ODBC nullInd for fieldPDF.</param>
        /// <returns>Enums.MDB.WILL_NOT_CHANGE or Enums.MDB.WILL_CHANGE.</returns>
        public static Enums.MDB MergeFieldInt(string cmd, int fieldMDB, SQLLEN nullIndMDB, ref int fieldPDF, ref SQLLEN nullIndPDF)
        {
            Enums.MDB rc = Enums.MDB.WILL_NOT_CHANGE;  //default.

            // If both fields are tagged with nulls we have nothing to do.
            if ((nullIndPDF == Constant.DB_NULL) && (nullIndMDB == Constant.DB_NULL))
            {
                return (rc);
            }

            // UseCase: field value and/or nullInd value differ between MDB and PDF.
            if ((nullIndPDF != nullIndMDB) || !fieldMDB.Equals(fieldPDF))
            {
                // MDB != PDF.
                if ((nullIndPDF == Constant.DB_NULL) && (!cmd.Equals("B")))  // "B" for "blank out optional field value"
                {
                    /* User wants data to be copied from MDB.  */
                    fieldPDF = fieldMDB;
                    nullIndPDF = nullIndMDB;
                }
                else
                {
                    // MDB is being changed.
                    rc = Enums.MDB.WILL_CHANGE;
                }
            }
            return rc;
        }

        /// <summary>
        /// Performs a merge of a MDB field, and its corresponding PDF field, 
        /// where the field is type 'float'.
        /// </summary>
        /// <param name="cmd"> - command prescribed in PDF.</param>
        /// <param name="fieldMDB"> - the MDB field value.</param>
        /// <param name="nullIndMDB"> - an ODBC nullInd for fieldMDB.</param>
        /// <param name="fieldPDF"> - the PDF field value.</param>
        /// <param name="nullIndPDF"> - an ODBC nullInd for fieldPDF.</param>
        /// <returns>Enums.MDB.WILL_NOT_CHANGE or Enums.MDB.WILL_CHANGE.</returns>
        public static Enums.MDB MergeFieldFloat(string cmd, float fieldMDB, SQLLEN nullIndMDB, ref float fieldPDF, ref SQLLEN nullIndPDF)
        {
            Enums.MDB rc = Enums.MDB.WILL_NOT_CHANGE;  //default.

            // If both fields are tagged with nulls we have nothing to do.
            if ((nullIndPDF == Constant.DB_NULL) && (nullIndMDB == Constant.DB_NULL))
            {
                return (rc);
            }

            // UseCase: field value and/or nullInd value differ between MDB and PDF.
            if ((nullIndPDF != nullIndMDB) || !fieldMDB.Equals(fieldPDF))
            {
                // MDB != PDF.
                if ((nullIndPDF == Constant.DB_NULL) && (!cmd.Equals("B")))  // "B" for "blank out optional field value"
                {
                    /* User wants data to be copied from MDB.  */
                    fieldPDF = fieldMDB;
                    nullIndPDF = nullIndMDB;
                }
                else
                {
                    // MDB is being changed.
                    rc = Enums.MDB.WILL_CHANGE;
                }
            }
            return rc;
        }

        /// <summary>
        /// Performs a merge of a MDB field, and its corresponding PDF field, 
        /// where the field is type 'double'.
        /// </summary>
        /// <param name="cmd"> - command prescribed in PDF.</param>
        /// <param name="fieldMDB"> - the MDB field value.</param>
        /// <param name="nullIndMDB"> - an ODBC nullInd for fieldMDB.</param>
        /// <param name="fieldPDF"> - the PDF field value.</param>
        /// <param name="nullIndPDF"> - an ODBC nullInd for fieldPDF.</param>
        /// <returns>Enums.MDB.WILL_NOT_CHANGE or Enums.MDB.WILL_CHANGE.</returns>
        public static Enums.MDB MergeFieldDouble(string cmd, double fieldMDB, SQLLEN nullIndMDB, ref double fieldPDF, ref SQLLEN nullIndPDF)
        {
            Enums.MDB rc = Enums.MDB.WILL_NOT_CHANGE;  //default.

            // If both fields are tagged with nulls we have nothing to do.
            if ((nullIndPDF == Constant.DB_NULL) && (nullIndMDB == Constant.DB_NULL))
            {
                return (rc);
            }

            // UseCase: field value and/or nullInd value differ between MDB and PDF.
            if ((nullIndPDF != nullIndMDB) || !fieldMDB.Equals(fieldPDF))
            {
                // MDB != PDF.
                if ((nullIndPDF == Constant.DB_NULL) && (!cmd.Equals("B")))  // "B" for "blank out optional field value"
                {
                    /* User wants data to be copied from MDB.  */
                    fieldPDF = fieldMDB;
                    nullIndPDF = nullIndMDB;
                }
                else
                {
                    // MDB is being changed.
                    rc = Enums.MDB.WILL_CHANGE;
                }
            }
            return rc;
        }

        /// <summary>
        /// Include all required records into the fileW associated with a users 
        /// antenna record; these records are extracted from the MDB.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        public static void FtValMergeAnteRecords(string pdfName)
        {
            //...Log2.v("\n\nValMerge.FtValMergeAnteRecords(): Entry");

            SQLLEN[] nArrayFW;
            int aID1;
            uint[] pulled = new uint[1];
            string tableName;
            string whereClause;
            FtAnte ftAnte;
            int rc;
            int nMDBHandle;
            int nRet;
            MtAnte mtAnte;
            SQLLEN[] nArrayMDB;

            ResetPowerChange();  // Reset the global switch indicating power calcs will
                                 // be needed -- set below  1127 - GJS - 2007.02 */

            ValImport.FtFormWhereClause(out whereClause, "", "", "", Constant.NO_ANUM, "");

            GenUtil.UtCvtName(Constant.FT_ANTE, pdfName, out tableName);

            /* Read antenna information */
            if ((aID1 = DynAntenna.FtSelectAntenna(tableName, whereClause, " call1,call2,bndcde,anum ")) < 0)
            {
                ValErrs.AddMess("MERGE 2 - Could not read antenna information. Reason: %d", tableName, "E", aID1.ToString());
                return;
            }

            /* Set null array to all NULLS */
            nArrayFW = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            nArrayMDB = NullHelper.CreateArrayOfNullInd(MtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            /* Loop around, fetching the next available record from the user's ft_XXX_ante table. */
            while (DynAntenna.FtFetchAntenna(aID1, out ftAnte, out nArrayFW) == Constant.SUCCESS)
            {
                //...Log2.v("\n\nValMerge.FtValMergeAnteRecords(): APPLE: ftAnte: cmd = " + ftAnte.cmd + ";   " + ftAnte.KeysToString());

                // Bug fix: b150722A
                // If the ftAnte has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftAnte to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftAnte.cmd))
                {
                    continue;
                }

                string str = String.Format("Merging Antenna: {0}/{1}/{2}/{3}", ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);
                //...Log2.v("\r\nFtValMergeAnteRecords(): " + str);
                //...Log2.v("\r\nFtValMergeAnteRecords(): A: ftAnte.cmd = " + ftAnte.cmd);

                /* Initialise the pulled array to no records (0) */
                pulled[0] = 0;

                /* First if this is an add antenna we must ensure that
                 * the PDF has a site record for this new antenna
                 */
                if (ftAnte.cmd.Equals("A"))
                {
                    /* extract the local site */
                    //...Log2.v("\r\nFtValMergeAnteRecords(): Being added.");
                    ValImport.FtImportSite(pdfName, ftAnte.call1, "N");
                    continue;
                }

                /* If we are deleting a local antenna we must have the
                 * local channels present for this antenna
                 */
                if (ftAnte.cmd.Equals("D"))
                {
                    GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                    ValImport.FtImportSite(pdfName, ftAnte.call1, "U");
                }

                if (ftAnte.recstat.Equals("C"))
                {
                    /*  Task 1070 - We are ignoring the C recstat. GJS */
                    /*	continue; */  // No we're not.
                }

                /* Fetch the corresponding MDB record */
                ValImport.FtFormWhereClause(out whereClause, ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, "");

                nMDBHandle = DynMdbAntenna.MtSelectAntenna(whereClause, "");

                nRet = DynMdbAntenna.MtFetchAntenna(nMDBHandle, out mtAnte, out nArrayMDB);

                DynMdbAntenna.MtCloseAntenna(nMDBHandle);

                if (nRet != 0)
                {
                    /* if MDB record not found, get next from FW */
                    //...Log2.v("\r\nFtValMergeAnteRecords(): Not in MDB.");
                    continue;
                }

                if (ftAnte.CmdEquals('N'))
                {
                    bool IsNotUpdate = true;

                    /*  This is a no-op.  Check to see if it is a passive, and if the mdb and
                    *   the pdf have the same non-blank values in the offaxis fields.  If they
                    *   do not, make this an update.  */
                    if (FtUtils.IsBillBoard(ftAnte.acode))
                    {
                        /*  Check the fields for the passive normals. If they are absent or
                        *   different from the mdb, it is an update */
                        bool areDifferent = false;
                        areDifferent = areDifferent || ftAnte.offazm.Equals("");
                        areDifferent = areDifferent || !ftAnte.offazm.Equals(mtAnte.offazm);
                        areDifferent = areDifferent || !ftAnte.tazmth.Equals(mtAnte.tazmth);
                        areDifferent = areDifferent || !ftAnte.telvtn.Equals(ftAnte.telvtn);  // BUG ?!

                        if (areDifferent)
                        {
                            /*  They are different, make the command an update. mdb copy will
                            *   not override the command.  */
                            ftAnte.cmd = "U";
                            IsNotUpdate = false;
                        }
                    }

                    /* No op. Just copy record from MDB into PDF */
                    FtValCopy.FtCopyAnte(ref ftAnte, mtAnte, ref nArrayFW, nArrayMDB);

                    if (!String.IsNullOrEmpty(ftAnte.cmd))
                    {
                        ftAnte.cmd = "N";
                    }
                    nArrayFW[FtAnte.CMD] = Constant.DB_NOT_NULL;

                    /* Write the record back to PDF */
                    //...Log2.v("\r\nFtValMergeAnteRecords(): Updating cmd.");
                    if (DynAntenna.FtUpdateAntenna(aID1, ftAnte, nArrayFW) != Constant.SUCCESS)
                    {
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }

                    if (IsNotUpdate)
                    {
                        //...Log2.v("\r\nFtValMergeAnteRecords(): Not being updated.");
                        continue;   /* No further processing required */
                    }
                } // End of if (ftAnte.CmdEquals('N'))

                // At this point cmd cannot be 'A' or 'N'.
                // If any of these were true, would not be here!
                // Also - record must exist in MDB or would not be here! */

                // The following blocks of code will test each field, if
                // it is blank it will copy the value stored in the MDB,
                // if it is not blank it will test to see if it has
                // changed.  If changed then it will set flags to extract
                // the correct records in the 'pulled' variable.
                // After all fields have been checked
                // we then extract the necessary records as indicated by the
                // validation rules document.

                //...Log2.v("\n\nValMerge.FtValMergeAnteRecords(): BEAR: ftAnte: cmd = " + ftAnte.cmd + ";   " + ftAnte.KeysToString());

                AnteMerge(mtAnte, nArrayMDB, ref ftAnte, ref nArrayFW, ref pulled);

                //...Log2.v("\n\nValMerge.FtValMergeAnteRecords(): COLD: ftAnte: cmd = " + ftAnte.cmd + ";   " + ftAnte.KeysToString());


                // Write the antenna as merged with the mdb out to the ft tables.
                //...Log2.v("\r\nFtValMergeAnteRecords(): Updating merged record.");
                if (DynAntenna.FtUpdateAntenna(aID1, ftAnte, nArrayFW) != Constant.SUCCESS)
                {

                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }

                if (!ftAnte.cmd.Equals("N"))
                {
                    // Depending on the outcome of testing all of the
                    // individual fields, we will need to extract the
                    // required local and remote records for modification.
                    // remember that the local tx value is often the remote
                    // rx value in this wacky crazy world...
                    // see the TS VALIDATE RULES document for more info

                    // retrieve all local information
                    if (GenUtil.UtTestBit(pulled, Constant.LOCAL_SITE) == Enums.BIT.SET)
                    {
                        ValImport.FtImportSite(pdfName, ftAnte.call1, "N");
                    }

                    if (GenUtil.UtTestBit(pulled, Constant.LOCAL_CHANNEL) == Enums.BIT.SET)
                    {
                        whereClause = String.Format(" call1= '{0}'and call2 = '{1}'and bndcde = '{2}' and (antnumbrx1= {3} or antnumbrx2= {4} or antnumbrx3= {5} or antnumbtx1= {6} or antnumbtx2 = {7}) ",
                                                      ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum);

                        if (GenUtil.UtTestBit(pulled, Constant.RXPOWER) == Enums.BIT.SET)
                        {
                            ValImport.FtImportChannel(pdfName, whereClause, "U");

                            SetPowerChange(true);
                        }
                        else
                        {
                            ValImport.FtImportChannel(pdfName, whereClause, "N");
                        }
                    }

                    // retrieve all remote information

                    if (GenUtil.UtTestBit(pulled, Constant.REMOTE_SITE) == Enums.BIT.SET)
                    {
                        ValImport.FtImportSite(pdfName, ftAnte.call2, "N");
                    }

                    if (GenUtil.UtTestBit(pulled, Constant.REMOTE_CHANNEL) == Enums.BIT.SET)
                    {
                        whereClause = String.Format(" call1= '{0}'and call2 = '{1}'and bndcde = '{2}' and (antnumbrx1= {3} or antnumbrx2= {4} or antnumbrx3= {5} or antnumbtx1= {6} or antnumbtx2 = {7}) ",
                                                      ftAnte.call2, ftAnte.call1, ftAnte.bndcde, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum, ftAnte.anum);

                        if (GenUtil.UtTestBit(pulled, Constant.RXPOWER) == Enums.BIT.SET)
                        {
                            /*  Some change has made recalculation of the rx power necessary.
                            *   Bring in the channels from the neighbouring link with the update
                            *   flag set.  */
                            ValImport.FtImportChannel(pdfName, whereClause, "U");
                            SetPowerChange(true);  // This will cause the whole passive chain to be imported.
                        }
                        else
                        {
                            ValImport.FtImportChannel(pdfName, whereClause, "N");
                        }
                    }

                    // It is important that this is the last to be extracted as it relies on the remote channels already having been
                    // extracted into the PDF.
                    // If the local antenna height is changing, then the remote antennae should have their elevtn fields
                    // recalculated.  Therefore, remote antenna records pulled in from the MDB will have their cmd values
                    // set to 'U' instead of 'N' to force this recalculation to occur later, if the local aht
                    // is being changed.  Note that this fails if the user enteres a 'N'o op record for the remote
                    // antenna, since this would prevent that record from being imported from the MDB.  Similarly for the
                    // RXPOWER situation above.

                    if (GenUtil.UtTestBit(pulled, Constant.REMOTE_ANTENNA) == Enums.BIT.SET)
                    {
                        /* Is local Antenna height changing? */
                        if (GenUtil.UtTestBit(pulled, Constant.LOCAL_AHT) == Enums.BIT.SET)
                        {
                            //Import recs with cmd = 'U'
                            //This forces recalculation of elevtn field in remote antenna
                            FtGetChannelsAntenna(pdfName, ftAnte.call1, false, "U");
                            SetPowerChange(true);
                        }
                        else
                        {
                            /* Import recs with cmd = 'N' */
                            FtGetChannelsAntenna(pdfName, ftAnte.call1, false, "N");
                        }
                    }

                    // Make sure that there are not any NO OP antenna
                    // records in the PDF which should be 'U'pdates
                    // because the elvtn field is being changed
                    // Can ignore values: A, B, U, D - will be OK.

                    //...Log2.v("\r\nFtValMergeAnteRecords(): Set cmd after height check.");

                    whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and cmd = 'N' ", ftAnte.call2, ftAnte.call1, ftAnte.bndcde);

                    rc = ValImport.FtSetAnteCmd(pdfName, whereClause, "U");

                    if (rc != Constant.SUCCESS)
                    {
                        string keyLine = ValErrs.MakeKeyLine(ftAnte.call2, ftAnte.call1, ftAnte.bndcde, Constant.ZERO, "");
                        ValErrs.AddMess("MERGE 3 - Could not set antenna command values. ", keyLine, "W");
                    }

                    // As it turns out, when we extract the antennas local channels, we also need
                    // to extract the other local antennas that those channels use.
                    if (GenUtil.UtTestBit(pulled, Constant.LOCAL_ANTENNA) == Enums.BIT.SET)
                    {
                        // Get remote antenna and site information.
                        FtGetChannelsAntenna(pdfName, ftAnte.call2, false, "N");
                    }

                } // End of if (!ftAnte.cmd.Equals("N"))

                NullHelper.FillArray(ref nArrayFW, Constant.DB_NULL);

            } // End of while (DynAntenna.FtFetchAntenna())

            DynAntenna.FtCloseAntenna(aID1);

            //...Log2.v("\n\nValMerge.FtValMergeAnteRecords(): Exit");

        }   /* ----- End of ftValMergeAnteRecords ----- */

        /// <summary>
        ///   Returns flag value that records whether the rx power would have changed for use
        ///   by passives; passives are merged later in the validation process 
        ///   and this flag is used all down the passive chain.
        /// </summary>
        /// <returns> - true of false.</returns>
        private static bool GetPowerChange()
        {
            return glb_powerchange;
        }

        /// <summary>
        ///   Sets the flag that records whether the rx power would have changed for use
        ///   by passives; passives are merged later in the validation process 
        ///   and this flag is used all down the passive chain.
        /// </summary>
        /// <param name="nNewVal"> - new value of flag.</param>
        /// <returns> - the newly set flag value.</returns>
        private static bool SetPowerChange(bool nNewVal)
        {
            glb_powerchange = nNewVal;
            return glb_powerchange;
        }

        /// <summary>
        ///   Sets to 'false' the flag that records whether the rx power would have changed for use
        ///   by passives; passives are merged later in the validation process 
        ///   and this flag is used all down the passive chain.
        /// </summary>
        /// <returns> - the value of the flag prior to reset.</returns>
        private static bool ResetPowerChange()
        {
            bool nTemp = glb_powerchange;
            glb_powerchange = false;
            return nTemp;
        }

        /// <summary>
        /// Performs the low level merge process on a field-by-field basis for antenna records.
        /// </summary>
        /// <param name="mtAnte"> - MDB site record object.</param>
        /// <param name="nullsMDB"> - array of ODBC nullInds for mtAnte.</param>
        /// <param name="ftAnte"> - PDF site record object.</param>
        /// <param name="nullsPDF"> - array of ODBC nullInds for ftAnte.</param>
        /// <param name="pulled"> - bitmap for records needed.</param>
        public static void AnteMerge(MtAnte mtAnte, SQLLEN[] nullsMDB, ref FtAnte ftAnte, ref SQLLEN[] nullsPDF, ref uint[] pulled)
        {
            MergeFieldString(ftAnte.cmd, mtAnte.call1, nullsMDB[MtAnte.CALL1], ref ftAnte.call1, ref nullsPDF[FtAnte.CALL1]);

            MergeFieldString(ftAnte.cmd, mtAnte.call2, nullsMDB[MtAnte.CALL2], ref ftAnte.call2, ref nullsPDF[FtAnte.CALL2]);

            MergeFieldString(ftAnte.cmd, mtAnte.bndcde, nullsMDB[MtAnte.BNDCDE], ref ftAnte.bndcde, ref nullsPDF[FtAnte.BNDCDE]);

            MergeFieldShort(ftAnte.cmd, mtAnte.anum, nullsMDB[MtAnte.ANUM], ref ftAnte.anum, ref nullsPDF[FtAnte.ANUM]);

            if (MergeFieldString(ftAnte.cmd, mtAnte.ause, nullsMDB[MtAnte.AUSE],
                ref ftAnte.ause, ref nullsPDF[FtAnte.AUSE]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                /* set rxpower to ensure that the chan records
                 * extracted are tested for ause implications
                 */
                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.acode, nullsMDB[MtAnte.ACODE],
                ref ftAnte.acode, ref nullsPDF[FtAnte.ACODE]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.aht, nullsMDB[MtAnte.AHT],
                ref ftAnte.aht, ref nullsPDF[FtAnte.AHT]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_AHT);
                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.azmth, nullsMDB[MtAnte.AZMTH],
                ref ftAnte.azmth, ref nullsPDF[FtAnte.AZMTH]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.elvtn, nullsMDB[MtAnte.ELVTN],
                ref ftAnte.elvtn, ref nullsPDF[FtAnte.ELVTN]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.dist, nullsMDB[MtAnte.DIST],
                ref ftAnte.dist, ref nullsPDF[FtAnte.DIST]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.offazm, nullsMDB[MtAnte.OFFAZM],
                ref ftAnte.offazm, ref nullsPDF[FtAnte.OFFAZM]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.tazmth, nullsMDB[MtAnte.TAZMTH],
                ref ftAnte.tazmth, ref nullsPDF[FtAnte.TAZMTH]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.telvtn, nullsMDB[MtAnte.TELVTN],
                ref ftAnte.telvtn, ref nullsPDF[FtAnte.TELVTN]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.tgain, nullsMDB[MtAnte.TGAIN],
                ref ftAnte.tgain, ref nullsPDF[FtAnte.TGAIN]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.txfdlnth, nullsMDB[MtAnte.TXFDLNTH],
                ref ftAnte.txfdlnth, ref nullsPDF[FtAnte.TXFDLNTH]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.txfdlnlh, nullsMDB[MtAnte.TXFDLNLH],
                ref ftAnte.txfdlnlh, ref nullsPDF[FtAnte.TXFDLNLH]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.txfdlntv, nullsMDB[MtAnte.TXFDLNTV],
                ref ftAnte.txfdlntv, ref nullsPDF[FtAnte.TXFDLNTV]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.txfdlnlv, nullsMDB[MtAnte.TXFDLNLV],
                ref ftAnte.txfdlnlv, ref nullsPDF[FtAnte.TXFDLNLV]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            MergeFieldString(ftAnte.cmd, mtAnte.rxfdlnth, nullsMDB[MtAnte.RXFDLNTH], ref ftAnte.rxfdlnth, ref nullsPDF[FtAnte.RXFDLNTH]);

            MergeFieldFloat(ftAnte.cmd, mtAnte.rxfdlnlh, nullsMDB[MtAnte.RXFDLNLH], ref ftAnte.rxfdlnlh, ref nullsPDF[FtAnte.RXFDLNLH]);

            MergeFieldString(ftAnte.cmd, mtAnte.rxfdlntv, nullsMDB[MtAnte.RXFDLNTV], ref ftAnte.rxfdlntv, ref nullsPDF[FtAnte.RXFDLNTV]);

            MergeFieldFloat(ftAnte.cmd, mtAnte.rxfdlnlv, nullsMDB[MtAnte.RXFDLNLV], ref ftAnte.rxfdlnlv, ref nullsPDF[FtAnte.RXFDLNLV]);

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.txpadpam, nullsMDB[MtAnte.TXPADPAM],
                ref ftAnte.txpadpam, ref nullsPDF[FtAnte.TXPADPAM]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            MergeFieldFloat(ftAnte.cmd, mtAnte.rxpadlna, nullsMDB[MtAnte.RXPADLNA], ref ftAnte.rxpadlna, ref nullsPDF[FtAnte.RXPADLNA]);


            if (MergeFieldFloat(ftAnte.cmd, mtAnte.txcompl, nullsMDB[MtAnte.TXCOMPL],
                ref ftAnte.txcompl, ref nullsPDF[FtAnte.TXCOMPL]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            MergeFieldFloat(ftAnte.cmd, mtAnte.rxcompl, nullsMDB[MtAnte.RXCOMPL], ref ftAnte.rxcompl, ref nullsPDF[FtAnte.RXCOMPL]);

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.obsloss, nullsMDB[MtAnte.OBSLOSS],
                ref ftAnte.obsloss, ref nullsPDF[FtAnte.OBSLOSS]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldFloat(ftAnte.cmd, mtAnte.kvalue, nullsMDB[MtAnte.KVALUE],
                ref ftAnte.kvalue, ref nullsPDF[FtAnte.KVALUE]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldShort(ftAnte.cmd, mtAnte.atwrno, nullsMDB[MtAnte.ATWRNO],
                ref ftAnte.atwrno, ref nullsPDF[FtAnte.ATWRNO]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.nota, nullsMDB[MtAnte.NOTA],
                ref ftAnte.nota, ref nullsPDF[FtAnte.NOTA]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.apoint, nullsMDB[MtAnte.APOINT],
                ref ftAnte.apoint, ref nullsPDF[FtAnte.APOINT]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            if (MergeFieldString(ftAnte.cmd, mtAnte.licence, nullsMDB[MtAnte.LICENCE],
                ref ftAnte.licence, ref nullsPDF[FtAnte.LICENCE]) == Enums.MDB.WILL_CHANGE)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);
            }

            MergeFieldString(ftAnte.cmd, mtAnte.sdate, nullsMDB[MtAnte.SDATE], ref ftAnte.sdate, ref nullsPDF[FtAnte.SDATE]);

            MergeFieldString(ftAnte.cmd, mtAnte.mdate, nullsMDB[MtAnte.MDATE], ref ftAnte.mdate, ref nullsPDF[FtAnte.MDATE]);

            MergeFieldString(ftAnte.cmd, mtAnte.mtime, nullsMDB[MtAnte.MTIME], ref ftAnte.mtime, ref nullsPDF[FtAnte.MTIME]);
        }

        /// <summary>
        /// Gets remote antenna and site information.
        /// </summary>
        /// <remarks>
        /// This procedure will take the table name and the call1 value
        /// and from that extract the remote channel records.  The
        /// remote channel records will then be used to find the remote
        /// antennas.  We must find all remote antennas for those
        /// channels therefore rx1, 2, & 3 and tx 1 & 2.
        /// 
        /// As a later addition it was found that we need to also be
        /// able to extract the remote site if we are modifing the
        /// local site.  We indicate this with the passed variable
        /// "site".  If "site" is TRUE then also fetch the site
        /// associated with the remote channels.
        /// 
        /// Later it was found that the function should be capable
        /// of extracting the records with different cmd field values.
        /// Thus, the parameter cmdVal was added so that the caller
        /// could specify the cmd value to be placed onto extracted
        /// records.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call1"> - call1 for a site.</param>
        /// <param name="site"> - if true then also fetch the site associated with 
        /// the remote channels.</param>
        /// <param name="cmdVal"> - desired cmd value.</param>
        public static void FtGetChannelsAntenna(string pdfName, string call1, bool site, string cmdVal)
        {
            //...Log2.v("\n\nValMerge.FtGetChannelsAntenna(): Entry");

            /* Local variables */
            int cID1;
            int[] ants = new int[5];
            int curAntenna;
            FtChan ftChan;
            SQLLEN[] nArrayFW = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            /* Formulate the select clause to fetch all channels where the
             * 'call2' field is equal to the local site ('call1' parameter).
             */
            string whereClause;
            ValImport.FtFormWhereClause(out whereClause, "", call1, "", Constant.NO_ANUM, "");

            string tableName;
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);


            /* Select all remote channels from the PDF. */
            if ((cID1 = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("MERGE 7 - Could not read channel information.  Reason %d", tableName, "E", cID1.ToString());
                return;
            }

            /* for each remote channel, get all its antennae. */
            while (DynChannel.FtFetchChannel(cID1, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                if (ftChan.call2.Equals(call1))
                {
                    /* initialise the storage for anums */
                    ants[0] = -1;
                    ants[1] = -1;
                    ants[2] = -1;
                    ants[3] = -1;
                    ants[4] = -1;
                    curAntenna = 0;

                    /* where clause can use no blank anums therefore a
                     * bit of a song & dance here
                     */
                    whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum in (",
                        ftChan.call1, ftChan.call2, ftChan.bndcde);

                    if (nArrayFW[FtChan.ANTNUMBTX1] != Constant.DB_NULL)
                    {
                        ants[curAntenna++] = ftChan.antnumbtx1;
                    }
                    if (nArrayFW[FtChan.ANTNUMBTX2] != Constant.DB_NULL)
                    {
                        ants[curAntenna++] = ftChan.antnumbtx2;
                    }
                    if (nArrayFW[FtChan.ANTNUMBRX1] != Constant.DB_NULL)
                    {
                        ants[curAntenna++] = ftChan.antnumbrx1;
                    }
                    if (nArrayFW[FtChan.ANTNUMBRX2] != Constant.DB_NULL)
                    {
                        ants[curAntenna++] = ftChan.antnumbrx2;
                    }
                    if (nArrayFW[FtChan.ANTNUMBRX3] != Constant.DB_NULL)
                    {
                        ants[curAntenna++] = ftChan.antnumbrx3;
                    }

                    /* Now we can construct the in clause for the
                     * whereClause.  must be real numbers between the ", "
                     */
                    char comma = ' ';
                    while (--curAntenna >= 0)
                    {
                        string tstring = String.Format("{0}{1}", comma, ants[curAntenna]);
                        comma = ',';

                        whereClause += tstring;
                    }

                    whereClause += ")";

                    /* Import the missing antenna records */
                    ValImport.FtImportAntenna(pdfName, whereClause, cmdVal);

                    /* If called from local site record, find remote sites */
                    if (site)
                    {
                        /* now import the remote site */
                        ValImport.FtImportSite(pdfName, ftChan.call1, cmdVal);
                    }
                }
            }

            DynChannel.FtCloseChannel(cID1);

            //...Log2.v("\n\nValMerge.FtGetChannelsAntenna(): Exit: whereClause = " + whereClause);
        }   /* ----- End of ftGetChannelsAntenna ----- */



        /// <summary>
        /// This method merges the channel information in the MDB and in the PDF.
        /// </summary>
        /// <remarks>
        /// It verifies each field to see if it is blank(NULL) and, if so, takes the value 
        /// from the MDB and puts it in the right spot depending on which fields have been modified.
        /// </remarks>
        /// <param name="pdfName"> - name of the prescribed PDF file.</param>
        /// <param name="errCount"> - cummulative error count.</param>
        /// <param name="warnCount"> - cummulative warning count.</param>
        public static void FtValMergeChanRecords(string pdfName, ref short errCount, ref short warnCount)
        {
            //...Log2.v("\n\nValMerge.FtValMergeChanRecords(): Entry");

            /* Local variables */
            SQLLEN[] nArrayFW = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            short[] remotes = new short[20];
            uint[] pulled = new uint[1];
            int rc;
            int cID1;
            FtChan ftChan;

            MtChan mtChan;
            SQLLEN[] nArrayMDB;

            int nRet;
            int nCurHandle;

            /* Clear out and set up channel section clause */
            string whereClause;
            ValImport.FtFormWhereClause(out whereClause, "", "", "", Constant.NO_ANUM, "");

            string tableName;
            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* Read Channel information */
            if ((cID1 = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                ValErrs.AddMess("MERGE 4 - Could not read channel information.  Reason %d", tableName, "E", cID1.ToString());
                return;
            }

            while (DynChannel.FtFetchChannel(cID1, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                /* Initialise the pulled array to no records (0) */
                pulled[0] = 0;

                /* If this is a passive reflector, then we will later
                ** have to calculate passive rx powers.  See the file
                ** valPassive.sc for more on this thrilling topic.
                ** In the mean time, we have to ensure that if the
                ** channel in question is on a passive reflector,
                ** then the 'reflected' channel must be in the pdf,
                ** since we will be setting its tx power, based on the
                ** calculated rx power.  Recall that passives use '%'
                ** as the first character of the callsign.
                */
                if (ftChan.call1.StartsWith("%") && (!ftChan.cmd.Equals("D")) && (!ftChan.cmd.Equals("A")))
                {

                    GenUtil.UtSetBit(ref pulled, Constant.PASSIVE_REFLECTOR);
                }

                /* If we are deleting the local tx channel we must
                 * pull in the remote channel to delete its rx side
                 */
                if (ftChan.cmd.Equals("D"))
                {
                    GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                }

                /* First if this is an add channel we must ensure that
                 * the PDF has a site record for this new channel
                 */
                if (ftChan.cmd.Equals("A"))
                {
                    /* Extract the local site & antenna */
                    ValImport.FtImportSite(pdfName, ftChan.call1, "N");

                    /* And now the antenna */
                    whereClause = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum in ({3}, {4}, {5}, {6}, {7})",
                        ftChan.call1, ftChan.call2, ftChan.bndcde,
                        ftChan.antnumbrx1, ftChan.antnumbrx2, ftChan.antnumbrx3,
                        ftChan.antnumbtx1, ftChan.antnumbtx2);

                    ValImport.FtImportAntenna(pdfName, whereClause, "N");

                    /* 	the passive power calculation requires that all frequency fields be
                    *		filled out.  For adds, this does not get done until later on in the
                    *		processing.  We need thsi information before the passives power
                    *		calculations start.  Therefore we will fill out the receive field
                    *		with the remote transmit field. */

                    if ((ftChan.call1.StartsWith("%")) || (ftChan.call2.StartsWith("%")))
                    {
                        int nRet_;
                        int nChan;
                        FtSiteStr pftOtherEnd;
                        FtSiteStrNulls pftOtherNulls;

                        //	Get the channel at the other end.
                        nRet_ = FtUtils.FtGetSiteWN(ftChan.call2, out pftOtherEnd, 3, pdfName, out pftOtherNulls);
                        if (nRet_ == 0)
                        {
                            //	Get the channel from the other end
                            nChan = FtUtils.FtFindChanChid(pftOtherEnd, ftChan.call1, ftChan.bndcde, ftChan.chid);
                            if (nChan >= 0)
                            {
                                ftChan.freqrx = pftOtherEnd.stChanPtr[nChan].freqtx;
                                nArrayFW[FtChan.FREQRX] = pftOtherNulls.anChanNullPtr[nChan][FtChan.FREQTX];
                                if (DynChannel.FtUpdateChannel(cID1, ftChan, nArrayFW) != Constant.SUCCESS)
                                {
                                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                                }
                            }
                        }
                    }

                    /* -- Continue to next record in PDF -- */
                    continue;
                }

                /*
                the add channels have been taken care of,
                now we must fill out the delete and update channels
                the "N" channels can be ignored
                */

                /* If this is a computer generated record */
                if (ftChan.recstat.Equals("C"))
                {
                    if (ftChan.cmd.Equals("U"))
                    {
                        GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
                    }
                    else
                    {
                        /*  Task 1070 -- We are allowing users to enter changed records
                        *   with the recstat = C, since this happens on occasion.  GJS */
                        /*	continue; */
                    }
                }

                /* Fetch the corresponding MDB record */
                ValImport.FtFormWhereClause(out whereClause, ftChan.call1, ftChan.call2, ftChan.bndcde, Constant.NO_ANUM, ftChan.chid);
                nCurHandle = DynMdbChannel.MtSelectChannel(whereClause, "");

                nRet = DynMdbChannel.MtFetchChannel(nCurHandle, out mtChan, out nArrayMDB);

                DynMdbChannel.MtCloseChannel(nCurHandle);

                if (nRet != 0)
                {
                    /* -- If MDB record not found, get next from FW -- */
                    continue;
                }

                /* NOTE:  If user messes with antnumb?x? fields, then they
                 *	may get an error reported as cannot delete antenna
                 *	since there are channels on it - even if that
                 *	channel record is marked for delete in the PDF.
                 *	If the following condition is modified to include
                 *	records	marked for delete, then the fields will get
                 *	overlapped with the values from the MDB, and the
                 *	problem will not be seen. */
                if (ftChan.CmdEquals('N'))
                {
                    /* Just copy record from MDB into PDF */
                    FtValCopy.FtCopyChan(ref ftChan, mtChan, ref nArrayFW, nArrayMDB);

                    /* Write the record back to PDF */
                    if (DynChannel.FtUpdateChannel(cID1, ftChan, nArrayFW) != Constant.SUCCESS)
                    {

                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                    continue;	/* No further processing required */
                }

                /* --- NOTE: ---
                **	At this point, the channel record in question
                **	cannot have either 'A'dd nor 'N'o-op as a
                **	command on it.  It cannnot
                **	have a status of 'C'omputer generated. (but see task 1070 above - GJS)
                **	Also, it must exist in the MDB.
                **	If any of theses conditions weren't met,
                **	we wouldn't be here!
                */

                /*
                The following blocks of code will test each field, if
                it is blank it will copy the value stored in the MDB,
                if it is not blank it will test to see if it has
                changed.  If changed then it will set flags to extract
                the correct records.  After all fields have been checked
                we then extract the necessary records as indicated by the
                validation rules document.
                */

                ChanMerge(mtChan, nArrayMDB, ref ftChan, ref nArrayFW, ref pulled);

                if (DynChannel.FtUpdateChannel(cID1, ftChan, nArrayFW) != Constant.SUCCESS)
                {

                    ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                }

                /*
                Depending on the outcome of testing all of the
                individual fields, we will need to extract the
                required local and remote records for modification.
                remember that the local tx value is often the remote
                rx value in this wacky crazy world...
                see the TS VALIDATE RULES document for more info
                */

                /* the testbits for channel have been commented out as a result of
                conflicting requirements from claudia & al.  If the channel is going
                to have the rxpower recalculated each time it is updated then it needs
                the local & remote channel & antenna records in the file w.  This is the
                effect of removing the uttestbits
                */

                if (GenUtil.UtTestBit(pulled, Constant.RXPOWER) == Enums.BIT.SET)
                {
                    /*  The channel is changing tx powers.  If there are any passive in the
                    *   pdf, then we need all of them to be update.  */
                    SetPowerChange(true);
                }

                /* retrieve all local information */
                if (GenUtil.UtTestBit(pulled, Constant.LOCAL_SITE) == Enums.BIT.SET)
                {
                    ValImport.FtImportSite(pdfName, ftChan.call1, "N");
                }

                whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum in ({3}, {4}, {5}, {6}, {7})",
                    ftChan.call1,
                    ftChan.call2,
                    ftChan.bndcde,
                    ftChan.antnumbrx1,
                    ftChan.antnumbrx2,
                    ftChan.antnumbrx3,
                    ftChan.antnumbtx1,
                    ftChan.antnumbtx2);

                ValImport.FtImportAntenna(pdfName, whereClause, "N");

                /* 	Retrieve all remote information.  We need the site if any of site,
                *		antenna or channel are to be pulled.  */
                if (GenUtil.UtTestBit(pulled, Constant.REMOTE_SITE) == Enums.BIT.SET ||

                            GenUtil.UtTestBit(pulled, Constant.REMOTE_ANTENNA) == Enums.BIT.SET ||

                                GenUtil.UtTestBit(pulled, Constant.REMOTE_CHANNEL) == Enums.BIT.SET)
                {
                    ValImport.FtImportSite(pdfName, ftChan.call2, "N");
                }


                ValImport.FtFormWhereClause(out whereClause, ftChan.call2, ftChan.call1, ftChan.bndcde,
                    Constant.NO_ANUM, ftChan.chid);

                bool remoteRxUpdate = GenUtil.UtTestBit(pulled, Constant.REMOTE_CHANNEL_UP) == Enums.BIT.SET;

                ValImport.FtImportUpdateChannel(pdfName, whereClause, ftChan, nArrayFW, remoteRxUpdate, ref errCount, ref warnCount);

                /* Make sure that there are not any NO OP remote channel
                 * records in the PDF which should be 'D'eletes
                 * because the local channel is being deleted
                * Can ignore values: A, B, U, D - will be OK.
                * The whereClause from above is modified to isolate
                * those records that meet the above criteria and are
                * marked as NO OPs
                */
                if (ftChan.CmdEquals('D'))
                {

                    whereClause += "and cmd = 'N' ";
                    rc = ValImport.FtSetChanCmd(pdfName, whereClause, "D");
                    if (rc != Constant.SUCCESS)
                    {

                        ValErrs.AddMess("MERGE 5 - Could not set channel command values", pdfName, "W");
                    }
                }

                /* We now must check to see if we need to import the
                ** 'reflected' side of a passive channel.
                */
                if (GenUtil.UtTestBit(pulled, Constant.PASSIVE_REFLECTOR) == Enums.BIT.SET)
                {

                    whereClause = String.Format(" call1='{0}' and call2!='{1}' and bndcde='{2}' and chid = '{3}'",
                                                    ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

                    SetPowerChange(true);

                    ValImport.FtImportChannel(pdfName, whereClause, "U");
                }

                /*
                It is important that this is the last to be extracted as it relies on
                the remote channels having been	extracted into the PDF
                */
                //memset(remotes, -1, sizeof(remotes));
                for (int i = 0; i < remotes.Length; i++) { remotes[i] = -1; }

                if (GetRemoteAntennae(pdfName, ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid, ref remotes) >= 0)
                {
                    /* The following is a weird and ugly, albeit efficient
                     * way of constructing a query whereClause which makes
                     * use (abuse?) of an SQL 'in' clause to find all
                     * antennae with anum values 'in' a given set
                    */
                    whereClause = String.Format(" call1='{0}' and call2='{1}' and bndcde='{2}' and anum in (",
                                                    ftChan.call2, ftChan.call1, ftChan.bndcde);

                    /* Construct the set of anum values */
                    int i = 0;
                    char comma = ' ';
                    while (remotes[i] > 0)
                    {
                        string temp = String.Format(" {0}{1}", comma, remotes[i]);

                        whereClause += temp;
                        comma = ',';
                        i++;
                    }


                    whereClause += ")";

                    string cmd;
                    /*  If the powers on a passive are changed, change the other end ant. */
                    if (GetPowerChange())
                    {
                        cmd = "U";
                    }
                    else
                    {
                        cmd = "N";
                    }

                    /* Insert all remote antennae */
                    ValImport.FtImportAntenna(pdfName, whereClause, cmd);

                    /*	We need to ensure that if an antenna is imported, then the site is
                    *		as well.  */
                    ValImport.FtImportSite(pdfName, ftChan.call2, "N");

                }
                else
                {/* Error retrieving remote antennae anum's */

                    string keyLine = ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid);
                    ValErrs.AddMess("MERGE 6 - Could not retrieve remote antenna numbers.", keyLine, "E");
                    errCount++;
                }

                NullHelper.FillArray(ref nArrayFW, Constant.DB_NULL);

            }   /* End while more channels to fetch */


            DynChannel.FtCloseChannel(cID1);

            //...Log2.v("\n\nValMerge.FtValMergeChanRecords(): Exit");
        }   /* ----- End of ftValMergeChanRecords ----- */

        /// <summary>
        /// This method merges the MDB channel data with the PDF channel data, if needed. 
        /// </summary>
        /// <remarks>
        /// Recall that if the PDF field is NULL then the value of the PDF field is set to the
        /// value of the MDB field. The exception to this rule is for those records
        /// where the cmd field = 'B', in this case the user is attempting to set the
        /// MDB value to NULL and no merge is required.
        /// 
        /// If the value of the MDB is being changed then the bitmask "pulled" identifies
        /// additional records which must be added to the PDF.
        /// </remarks>
        /// <param name="mtChan"> - MDB channel record object.</param>
        /// <param name="nullsMDB"> - array of ODBC nullInds for mtChan.</param>
        /// <param name="ftChan"> - PDF channel record object.</param>
        /// <param name="nullsPDF"> - array of ODBC nullInds for ftChan.</param>
        /// <param name="pulled"> - bitmap for records needed.</param>
        public static void ChanMerge(MtChan mtChan, SQLLEN[] nullsMDB, ref FtChan ftChan, ref SQLLEN[] nullsPDF, ref uint[] pulled)
        {
            //...Log2.v("\n\nValMerge.ChanMerge(): Entry");

            /* Merge the call1 field if necessary */
            if (MergeFieldString(ftChan.cmd, mtChan.call1, nullsMDB[MtChan.CALL1],
                ref ftChan.call1, ref nullsPDF[FtChan.CALL1]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.call2, nullsMDB[MtChan.CALL2],
                ref ftChan.call2, ref nullsPDF[FtChan.CALL2]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.bndcde, nullsMDB[MtChan.BNDCDE],
                ref ftChan.bndcde, ref nullsPDF[FtChan.BNDCDE]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.splan, nullsMDB[MtChan.SPLAN],
                ref ftChan.splan, ref nullsPDF[FtChan.SPLAN]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.hl, nullsMDB[MtChan.HL],
                ref ftChan.hl, ref nullsPDF[FtChan.HL]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.vh, nullsMDB[MtChan.VH],
                ref ftChan.vh, ref nullsPDF[FtChan.VH]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }


            MergeFieldString(ftChan.cmd, mtChan.chid, nullsMDB[MtChan.CHID],
                ref ftChan.chid, ref nullsPDF[FtChan.CHID]);

            if (MergeFieldDouble(ftChan.cmd, mtChan.freqtx, nullsMDB[MtChan.FREQTX],
                ref ftChan.freqtx, ref nullsPDF[FtChan.FREQTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL_UP);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }


            if (MergeFieldString(ftChan.cmd, mtChan.poltx,
                nullsMDB[MtChan.POLTX], ref ftChan.poltx,
                ref nullsPDF[FtChan.POLTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL_UP);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.antnumbtx1,
                nullsMDB[MtChan.ANTNUMBTX1], ref ftChan.antnumbtx1,
                ref nullsPDF[FtChan.ANTNUMBTX1]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.antnumbtx2,
                nullsMDB[MtChan.ANTNUMBTX2], ref ftChan.antnumbtx2,
                ref nullsPDF[FtChan.ANTNUMBTX2]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.eqpttx,
                nullsMDB[MtChan.EQPTTX], ref ftChan.eqpttx,
                ref nullsPDF[FtChan.EQPTTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.pwrtx,
                nullsMDB[MtChan.PWRTX], ref ftChan.pwrtx,
                ref nullsPDF[FtChan.PWRTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.atpccde,
                nullsMDB[MtChan.ATPCCDE], ref ftChan.atpccde,
                ref nullsPDF[FtChan.ATPCCDE]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.afsltx1,
                nullsMDB[MtChan.AFSLTX1], ref ftChan.afsltx1,
                ref nullsPDF[FtChan.AFSLTX1]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.afsltx2,
                nullsMDB[MtChan.AFSLTX2], ref ftChan.afsltx2,
                ref nullsPDF[FtChan.AFSLTX2]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.traftx,
                nullsMDB[MtChan.TRAFTX], ref ftChan.traftx,
                ref nullsPDF[FtChan.TRAFTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL_UP);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.srvctx,
                nullsMDB[MtChan.SRVCTX], ref ftChan.srvctx,
                ref nullsPDF[FtChan.SRVCTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL_UP);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.stattx,
                nullsMDB[MtChan.STATTX], ref ftChan.stattx,
                ref nullsPDF[FtChan.STATTX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL_UP);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }


            MergeFieldDouble(ftChan.cmd, mtChan.freqrx, nullsMDB[MtChan.FREQRX],
                ref ftChan.freqrx, ref nullsPDF[FtChan.FREQRX]);

            if (MergeFieldString(ftChan.cmd, mtChan.polrx,
                nullsMDB[MtChan.POLRX], ref ftChan.polrx,
                ref nullsPDF[FtChan.POLRX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.antnumbrx1,
                nullsMDB[MtChan.ANTNUMBRX1], ref ftChan.antnumbrx1,
                ref nullsPDF[FtChan.ANTNUMBRX1]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.RXPOWER);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.antnumbrx2,
                nullsMDB[MtChan.ANTNUMBRX2], ref ftChan.antnumbrx2,
                ref nullsPDF[FtChan.ANTNUMBRX2]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.antnumbrx3,
                nullsMDB[MtChan.ANTNUMBRX3], ref ftChan.antnumbrx3,
                ref nullsPDF[FtChan.ANTNUMBRX3]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.eqptrx,
                nullsMDB[MtChan.EQPTRX], ref ftChan.eqptrx,
                ref nullsPDF[FtChan.EQPTRX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.eqpturx,
                nullsMDB[MtChan.EQPTURX], ref ftChan.eqpturx,
                ref nullsPDF[FtChan.EQPTURX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.afslrx1,
                nullsMDB[MtChan.AFSLRX1], ref ftChan.afslrx1,
                ref nullsPDF[FtChan.AFSLRX1]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.afslrx2,
                nullsMDB[MtChan.AFSLRX2], ref ftChan.afslrx2,
                ref nullsPDF[FtChan.AFSLRX2]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
            }

            if (MergeFieldFloat(ftChan.cmd, mtChan.afslrx3,
                nullsMDB[MtChan.AFSLRX3], ref ftChan.afslrx3,
                ref nullsPDF[FtChan.AFSLRX3]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_ANTENNA);

                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_SITE);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_ANTENNA);
            }


            MergeFieldFloat(ftChan.cmd, mtChan.pwrrx1, nullsMDB[MtChan.PWRRX1],
                ref ftChan.pwrrx1, ref nullsPDF[FtChan.PWRRX1]);


            MergeFieldFloat(ftChan.cmd, mtChan.pwrrx2, nullsMDB[MtChan.PWRRX2],
                ref ftChan.pwrrx2, ref nullsPDF[FtChan.PWRRX2]);


            MergeFieldFloat(ftChan.cmd, mtChan.pwrrx3, nullsMDB[MtChan.PWRRX3],
                ref ftChan.pwrrx3, ref nullsPDF[FtChan.PWRRX3]);


            MergeFieldString(ftChan.cmd, mtChan.trafrx, nullsMDB[MtChan.TRAFRX],
                ref ftChan.trafrx, ref nullsPDF[FtChan.TRAFRX]);


            MergeFieldFloat(ftChan.cmd, mtChan.esint, nullsMDB[MtChan.ESINT],
                ref ftChan.esint, ref nullsPDF[FtChan.ESINT]);


            MergeFieldFloat(ftChan.cmd, mtChan.tsint, nullsMDB[MtChan.TSINT],
                ref ftChan.tsint, ref nullsPDF[FtChan.TSINT]);


            MergeFieldString(ftChan.cmd, mtChan.srvcrx, nullsMDB[MtChan.SRVCRX],
                ref ftChan.srvcrx, ref nullsPDF[FtChan.SRVCRX]);


            MergeFieldString(ftChan.cmd, mtChan.statrx, nullsMDB[MtChan.STATRX],
                ref ftChan.statrx, ref nullsPDF[FtChan.STATRX]);

            if (MergeFieldString(ftChan.cmd, mtChan.routnumb,
                nullsMDB[MtChan.ROUTNUMB], ref ftChan.routnumb,
                ref nullsPDF[FtChan.ROUTNUMB]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.stnnumb,
                nullsMDB[MtChan.STNNUMB], ref ftChan.stnnumb,
                ref nullsPDF[FtChan.STNNUMB]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldShort(ftChan.cmd, mtChan.hopnumb,
                nullsMDB[MtChan.HOPNUMB], ref ftChan.hopnumb,
                ref nullsPDF[FtChan.HOPNUMB]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }

            if (MergeFieldString(ftChan.cmd, mtChan.sdate,
                nullsMDB[MtChan.SDATE], ref ftChan.sdate,
                ref nullsPDF[FtChan.SDATE]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);
            }


            MergeFieldString(ftChan.cmd, mtChan.notetx, nullsMDB[MtChan.NOTETX],
                ref ftChan.notetx, ref nullsPDF[FtChan.NOTETX]);


            MergeFieldString(ftChan.cmd, mtChan.noterx, nullsMDB[MtChan.NOTERX],
                ref ftChan.noterx, ref nullsPDF[FtChan.NOTERX]);


            MergeFieldString(ftChan.cmd, mtChan.notegnl, nullsMDB[MtChan.NOTEGNL],
                ref ftChan.notegnl, ref nullsPDF[FtChan.NOTEGNL]);


            MergeFieldString(ftChan.cmd, mtChan.cpoint, nullsMDB[MtChan.CPOINT],
                ref ftChan.cpoint, ref nullsPDF[FtChan.CPOINT]);

            if (MergeFieldString(ftChan.cmd, mtChan.feetx, nullsMDB[MtChan.FEETX],
                ref ftChan.feetx, ref nullsPDF[FtChan.FEETX]) > 0)
            {
                /* Attempting to change MDB Value */
                GenUtil.UtSetBit(ref pulled, Constant.LOCAL_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL);

                GenUtil.UtSetBit(ref pulled, Constant.REMOTE_CHANNEL_UP);
            }


            MergeFieldString(ftChan.cmd, mtChan.feerx, nullsMDB[MtChan.FEERX],
                ref ftChan.feerx, ref nullsPDF[FtChan.FEERX]);


            MergeFieldString(ftChan.cmd, mtChan.mdate, nullsMDB[MtChan.MDATE],
                ref ftChan.mdate, ref nullsPDF[FtChan.MDATE]);


            MergeFieldString(ftChan.cmd, mtChan.mtime, nullsMDB[MtChan.MTIME],
                ref ftChan.mtime, ref nullsPDF[FtChan.MTIME]);

            //...Log2.v("\n\nValMerge.ChanMerge(): Exit");
        }   /* ----- End of chanMerge ----- */

        /// <summary>
        /// For a given channel record determine its remote antennae.
        /// </summary>
        /// <remarks>
        /// CAUTION: 
        /// Caller must initialize the array 'remoteAnums' by filling with -1.
        /// </remarks>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="call1"> - local call sign.</param>
        /// <param name="call2"> - remote call sign.</param>
        /// <param name="bndcde"> - band code of antenna.</param>
        /// <param name="chid"> - channel ID.</param>
        /// <param name="remoteAnums"> - remote antennae.</param>
        /// <returns></returns>
        public static int GetRemoteAntennae(string pdfName, string call1, string call2, string bndcde, string chid, ref short[] remoteAnums)
        {
            //...Log2.v("\n\nValMerge.GetRemoteAntennae(): Entry");

            /* Local variables */
            FtChan ftChan;          /* current channel record */
            SQLLEN[] nullIndChan;   /* null ind. for chan fields */
            int chanHandle;             /* channel handle for dynamic */
            string whereClause;    /* selection criteria */
            string tableName;      /* chan. table name- long form*/
            int i = 0;              /* loop control */
            int rc = 0;

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* Construct selection criteria for all channels belonging to the
             * local antenna.
             */
            whereClause = String.Format("call1='{0}' and call2='{1}' and bndcde='{2}' and chid = '{3}'",
                                            call2, call1, bndcde, chid);

            /* Setup to select channels from PDF. */
            if ((chanHandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {
                return (chanHandle);
            }

            if (DynChannel.FtFetchChannel(chanHandle, out ftChan, out nullIndChan) == Constant.SUCCESS)
            {
                /* Get the remote antenna number */
                if ((nullIndChan[FtChan.ANTNUMBTX1] != Constant.DB_NULL) && (ftChan.antnumbtx1 > 0))
                {
                    /* tx1 antenna is present. Add to list. */
                    if (!AntInList(remoteAnums, ftChan.antnumbtx1))
                    {
                        remoteAnums[i++] = ftChan.antnumbtx1;
                    }
                }

                if (nullIndChan[FtChan.ANTNUMBTX2] != Constant.DB_NULL && ftChan.antnumbtx2 > 0)
                {
                    /* rx1 antenna is present. Add to list. */
                    if (!AntInList(remoteAnums, ftChan.antnumbtx2))
                    {
                        remoteAnums[i++] = ftChan.antnumbtx2;
                    }
                }

                if (nullIndChan[FtChan.ANTNUMBRX1] != Constant.DB_NULL && ftChan.antnumbrx1 > 0)
                {
                    /* rx1 antenna is present. Add to list. */
                    if (!AntInList(remoteAnums, ftChan.antnumbrx1))
                    {
                        remoteAnums[i++] = ftChan.antnumbrx1;
                    }
                }

                if (nullIndChan[FtChan.ANTNUMBRX2] != Constant.DB_NULL && ftChan.antnumbrx2 > 0)
                {
                    /* rx1 antenna is present. Add to list. */
                    if (!AntInList(remoteAnums, ftChan.antnumbrx2))
                    {
                        remoteAnums[i++] = ftChan.antnumbrx2;
                    }
                }

                if (nullIndChan[FtChan.ANTNUMBRX3] != Constant.DB_NULL && ftChan.antnumbrx3 > 0)
                {
                    /* rx1 antenna is present. Add to list. */
                    if (!AntInList(remoteAnums, ftChan.antnumbrx3))
                    {
                        remoteAnums[i++] = ftChan.antnumbrx3;
                    }
                }
            }
            else
            {
                rc = -1;
            }


            DynChannel.FtCloseChannel(chanHandle);

            //...Log2.v("\n\nValMerge.GetRemoteAntennae(): Exit");
            return (rc);
        }   /* ----- End of getRemoteAntennae ----- */

        /// <summary>
        /// This method determines whether the given antenna number is
        /// already in the list provided by the caller.
        /// </summary>
        /// <param name="list"> - array of antenna numbers.</param>
        /// <param name="antNum"> - antenna number to search for.</param>
        /// <returns>true or false.</returns>
        public static bool AntInList(short[] list, short antNum)
        {
            bool retVal = false;

            for (int i = 0; i < list.Length; i++)
            {
                if (!(list[i] > 0))
                {
                    break;
                }
                if (list[i] == antNum)
                {
                    /* antenna is in list */
                    retVal = true;
                    break;
                }
            }

            return retVal;
        }   /* ----- End of antInList ----- */

        /// <summary>
        /// This method builds the entire passive link based on limited passive information provided 
        /// in the PDF; if a passive is being added without a complete link, this erroneous situation is
        /// reported.
        /// </summary>
        /// <param name="pdfName"> - name of the PDF file.</param>
        /// <param name="errorCount"> - cummulative error count.</param>
        /// <param name="warningCount"> - cummulative warning count.</param>
        /// <returns></returns>
        public static int FtValMergePassiveLinks(string pdfName, ref short errorCount, ref short warningCount)
        {
            //...Log2.v("\n\nFtValMergePassiveLinks: Entry");

            FtChan ftChan;
            FtChan tmpChan = null;
            SQLLEN[] nArrayFW;
            SQLLEN[] tmpNulls = null;
            MtChan mtChan;
            SQLLEN[] mtChanNulls;

            int chandle;
            int tmphandle;
            bool freqChanged;
            int rc;
            int ret = Constant.SUCCESS;
            string whereClause;
            string tableName;
            string cCmd;
            bool powerchange = GetPowerChange();

            string currcall;   /* current call1 */
            string nextcall = "";   /* next call1 in the passive link */
            string oldcall;    /* previous call1 in the passive lnk */
            string bndcde;
            string chid;
            string eqpttemp = "";
            string eqptmdb = "";
            string traftemp = "";
            string trafmdb = "";
            string poltxmdb = "";
            double freqtxmdb = 0.0;
            short tx1, tx2, rx1, rx2, rx3; /* antennas */
            SQLLEN tx1null, tx2null, rx1null, rx2null, rx3null;
            SQLLEN traftempnull, eqpttempnull, trafmdbnull, eqptmdbnull;
            SQLLEN poltxnull, freqtxnull = 0;

            int nHandle;

            GenUtil.UtCvtName(Constant.FT_CHAN, pdfName, out tableName);

            /* Go through all channels in the PDF */
            string searchCriteria = " LEFT(call1,1)='%' or LEFT(call2,1)='%' ";
            if ((chandle = DynChannel.FtSelectChannel(tableName, searchCriteria, "")) < 0)
            {

                ValErrs.AddMess("MERGE 11 - Could not read channel information. Reason: %d",
                                            tableName, "E", chandle.ToString());
                errorCount++;
                ret = Constant.FAILURE;
            }


            //memset(nArrayFW, DB_NULL, sizeof(SQLLEN) * FT_CHAN_SIZE_);
            nArrayFW = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            // Iterate through every channel in the pdf.  
            // This is the merge with the mdb section.
            while (DynChannel.FtFetchChannel(chandle, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                /* select those channels that are part of a passive link */
                if ((ftChan.call1[0] == '%') || (ftChan.call2[0] == '%'))
                {
                    /* record the passive link info for this site */
                    currcall = ftChan.call1;     // currcall is the current site

                    oldcall = ftChan.call2;      // oldcall is the other end of this.

                    bndcde = ftChan.bndcde;

                    chid = ftChan.chid;

                    /* processing begins.  we are going to follow the link to the 'left'.
                       currcall is the current ftChan.call1  */

                    // Bug fix: b150722A
                    // It is possible that currcall is null or empty so test for it prior to accessing currcall[0]
                    while (!String.IsNullOrWhiteSpace(currcall) && currcall[0] == '%' && ret == Constant.SUCCESS)
                    {
                        MtChan mtChan_;
                        SQLLEN[] aChanNulls;

                        /* 	If the current call1 is a passive, get the next call1 in the passive
                        *		link.  The callsign of this one will be in nextcall */
                        //sprintf_s(whereClause, sizeof(whereClause),
                        //    "call1 != '%s' and call2 = '%s' and bndcde = '%s' and chid = '%s'",
                        //    oldcall, currcall, bndcde, chid);
                        whereClause = String.Format(" call1 != '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                        oldcall, currcall, bndcde, chid);

                        nHandle = DynMdbChannel.MtSelectChannel(whereClause, "");

                        rc = DynMdbChannel.MtFetchChannel(nHandle, out mtChan_, out aChanNulls);

                        DynMdbChannel.MtCloseChannel(nHandle);

                        /* 	Using the information from the above query Import the channel at
                        *		the other end of the link, first trying the mdb, then the
                *	 	pdf.  Then get the appropriate antennas and sites. */
                        if (rc == 0)
                        {
                            nextcall = mtChan_.call1;
                            tx1 = mtChan_.antnumbtx1;
                            tx1null = aChanNulls[MtChan.ANTNUMBTX1];
                            tx2 = mtChan_.antnumbtx2;
                            tx2null = aChanNulls[MtChan.ANTNUMBTX2];

                            whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                                nextcall, currcall, bndcde, chid);
                            /*	The import functions check the mdb for all channels, antennas, or
                            *		sites and if they are in the mdb and not in the pdf, they copy
                            *		them over to the pdf. */
                            /*  If this is a change of power calculation then the command must be
                            *   at least a U. */
                            //...Log2.v("\nValMerge.FtValMergePassiveLinks(): before call to GenUtil.CmdMax(): ALPHA");
                            GenUtil.CmdMax(out cCmd, powerchange ? "U" : ftChan.cmd, ftChan.cmd);

                            ValImport.FtImportChannel(pdfName, whereClause, cCmd);

                            whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum in ({3}, {4})",
                                                            nextcall, currcall, bndcde, tx1, tx2);

                            // Bug Fix: b070419a
                            // =================
                            // AH: 28-Aug-2020.
                            // The problem with line "X" is that, if the (previous) channel was
                            // marked for deletion, then so is the antenna it is associated with.
                            // However, a channel deletion should *never* force an antenna deletion.
                            // If the user wants an antenna deleted it must have its command set 
                            // to 'D' in the PDF.

                            if (cCmd == "D") cCmd = "N";  // This line is the bug fix for b070419a.

                            ValImport.FtImportAntenna(pdfName, whereClause, cCmd);  // Line "X".

                            ValImport.FtImportSite(pdfName, nextcall, "N");

                            ValImport.FtImportSite(pdfName, currcall, "N");
                        }
                        else
                        {
                            if (rc == Constant.NOMORERECS)
                            {
                                /*	Could not find this channel in the mdb, check the pdf */
                                whereClause = String.Format(" call1 != '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                                 oldcall, currcall, bndcde, chid);

                                if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                                {

                                    ValErrs.AddMess("MERGE 12 - Could not read channel information. Reason %d", tableName, "E", tmphandle.ToString());
                                    errorCount++;
                                    ret = Constant.FAILURE;
                                }
                                else
                                {
                                    /*	Read the passive from the pdf into tmpChan, tmpNulls */
                                    if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                                    {

                                        nextcall = tmpChan.call1;

                                        //if (strcmp(tmpChan.cmd, "A") == 0)
                                        if (tmpChan.cmd.Equals("A"))
                                        {
                                            //	This is okay.  Found in pdf, but not in mdb.
                                        }
                                        else
                                        {
                                            //	This is not okay.  Not found in mdb, and not being added in pdf.
                                            ValErrs.AddMess("MERGE 40: You have a passive in the input file that is not in the mdb, and it is not an add.",
                                                ValErrs.MakeKeyLine(tmpChan.call1, tmpChan.call2, tmpChan.bndcde, 0, tmpChan.chid), "E");
                                            errorCount++;
                                        }
                                    }
                                    else
                                    {

                                        ValErrs.AddMess("MERGE 13 - An incomplete passive link is being validated. You may be adding a new channel with command 'U'.",
                                                                    tableName, "E");
                                        errorCount++;
                                        ret = Error.BADPASSIVEDATA;
                                    }
                                }

                                DynChannel.FtCloseChannel(tmphandle);
                            }
                            else
                            {

                                ValErrs.AddMess("MERGE 14 - Unable to access PDF channel records. Reason %d",
                                                            ValErrs.MakeKeyLine(oldcall, currcall, bndcde, 0, chid), "E", rc.ToString());
                                errorCount++;
                                ret = Constant.FAILURE;
                            }
                        }

                        /*** currcall is the current call1, */
                        oldcall = currcall;

                        currcall = nextcall;
                    }


                    oldcall = ftChan.call1;

                    currcall = ftChan.call2;

                    /* Now we do the same processing going to the 'right'
                       this time */
                    ret = Constant.SUCCESS;
                    while (currcall[0] == '%' && ret == Constant.SUCCESS)
                    {
                        MtChan mtChan_;
                        SQLLEN[] aChanNulls;


                        whereClause = String.Format(" call2 != '{0}' and call1 = '{1}' and bndcde = '{2}' and chid = '{3}' ",
                                                            oldcall, currcall, bndcde, chid);

                        nHandle = DynMdbChannel.MtSelectChannel(whereClause, "");

                        rc = DynMdbChannel.MtFetchChannel(nHandle, out mtChan_, out aChanNulls);

                        DynMdbChannel.MtCloseChannel(nHandle);

                        if (rc == Constant.SUCCESS)
                        {
                            nextcall = mtChan_.call2;
                            rx1 = mtChan_.antnumbrx1;
                            rx1null = aChanNulls[MtChan.ANTNUMBRX1];
                            rx2 = mtChan_.antnumbrx2;
                            rx2null = aChanNulls[MtChan.ANTNUMBRX2];
                            rx3 = mtChan_.antnumbrx2;
                            rx3null = aChanNulls[MtChan.ANTNUMBRX2];

                            /*	Found channel in the mdb */
                            whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                                currcall, nextcall, bndcde, chid);

                            //...Log2.v("\nValMerge.FtValMergePassiveLinks(): before call to GenUtil.CmdMax(): BRAVO");
                            GenUtil.CmdMax(out cCmd, powerchange ? "U" : ftChan.cmd, ftChan.cmd);

                            ValImport.FtImportChannel(pdfName, whereClause, cCmd);

                            whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and anum in ({3}, {4}, {5})",
                                                            currcall, nextcall, bndcde, rx1, rx2, rx3);

                            // Bug Fix: b070419a
                            // =================
                            // AH: 28-Aug-2020.
                            // The problem with line "X" is that, if the (previous) channel was
                            // marked for deletion, then so is the antenna it is associated with.
                            // However, a channel deletion should *never* force an antenna deletion.
                            // If the user wants an antenna deleted it must have its command set 
                            // to 'D' in the PDF.

                            if (cCmd == "D") cCmd = "N"; // This line is the bug fix for b070419a.

                            ValImport.FtImportAntenna(pdfName, whereClause, cCmd);  // <-- Line "X".

                            ValImport.FtImportSite(pdfName, nextcall, "N");

                            ValImport.FtImportSite(pdfName, currcall, "N");
                        }
                        else
                        {
                            if (rc == Constant.NOMORERECS)
                            {

                                whereClause = String.Format(" call1 = '{0}' and call2 != '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                                    currcall, oldcall, bndcde, chid);

                                if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                                {

                                    ValErrs.AddMess("MERGE 15 - Could not read channel information. Reason %d.", tableName, "E", tmphandle.ToString());
                                    errorCount++;
                                    ret = Constant.FAILURE;
                                }

                                if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                                {

                                    nextcall = tmpChan.call2;
                                    if (tmpChan.cmd.Equals("A"))
                                    {
                                        //	This is okay.  Found in pdf, but not in mdb.
                                    }
                                    else
                                    {
                                        //	This is not okay.  Not found in mdb, and not being added in pdf.
                                        ValErrs.AddMess("MERGE 41: You have a passive in the input file that is not in the mdb, and it is not an add.\r\nPassive calculations cannot continue.",
                                            ValErrs.MakeKeyLine(tmpChan.call1, tmpChan.call2, tmpChan.bndcde, 0, tmpChan.chid), "E");
                                        errorCount++;
                                    }
                                }
                                else
                                {

                                    ValErrs.AddMess("MERGE 16 - An incomplete passive link is being validated.",
                                                                ValErrs.MakeKeyLine(currcall, nextcall, bndcde, 0, chid), "E");
                                    errorCount++;
                                    ret = Error.BADPASSIVEDATA;
                                }

                                DynChannel.FtCloseChannel(tmphandle);
                            }
                            else
                            {

                                ValErrs.AddMess("MERGE 17 - Unable to access PDF channel records. Reason %d",
                                                        pdfName, "E", rc.ToString());
                                errorCount++;
                                ret = Constant.FAILURE;
                            }
                        }

                        oldcall = currcall;

                        currcall = nextcall;
                    }
                }
            }

            DynChannel.FtCloseChannel(chandle);

            /* The passive links have now been imported.  However, if a
             * user has made a modification to the frequency, polarization,
             * or traffic code of the Active to passive record (at the
             * beginning of the link) then we must apply the changes through
             * the entire passive link.  Al has confirmed this logic.
             * Glen H. Dec 2, 1994
             */

            /* We make sure that the user is not modifying either the
             * traffic or equipment recieve fields from the recieve end.
             * To change these fields, the user must modify the fields
             * from the transmit side.  If these fields (at the recieve
             * end) do not equal a) the MDB record or b) the Transmit
             * field in the PDF, the user is changing these fields without
             * modifying the transmit.  This is an error
             */

            whereClause = "((left(call1, 1) = '%') or ((left(call2, 1) = '%' and left(call1, 1) != '%')))";

            if ((chandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {

                ValErrs.AddMess("MERGE 18 - Could not read passive channel.  Reason %d",
                                            tableName, "E", chandle.ToString());
                errorCount++;
                ret = Constant.FAILURE;
            }

            while (DynChannel.FtFetchChannel(chandle, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                oldcall = ftChan.call2;
                currcall = ftChan.call1;
                bndcde = ftChan.bndcde;
                chid = ftChan.chid;

                /* initialize these fields in case we don't
                 * have to process this record */
                tmpChan = new FtChan();

                tmpChan.freqtx = ftChan.freqtx;
                tmpChan.poltx = ftChan.poltx;
                tmpChan.eqpttx = ftChan.eqpttx;
                tmpChan.traftx = ftChan.traftx;

                tmpNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                tmpNulls[FtChan.FREQTX] = nArrayFW[FtChan.FREQTX];
                tmpNulls[FtChan.POLTX] = nArrayFW[FtChan.POLTX];
                tmpNulls[FtChan.EQPTTX] = nArrayFW[FtChan.EQPTTX];
                tmpNulls[FtChan.TRAFTX] = nArrayFW[FtChan.TRAFTX];

                /* we must find the end station where the transmit fields
                 * for this record are controlled by the originator */

                while (currcall[0] == '%')
                {

                    whereClause = String.Format("call1 != '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                        oldcall, currcall, bndcde, chid);

                    if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                    {

                        ValErrs.AddMess("MERGE 19 - Could not read passive channel.  Reason %d",
                                                    tableName, "E", tmphandle.ToString());
                        errorCount++;
                        ret = Constant.FAILURE;
                    }

                    rc = DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls);
                    if (rc == Constant.SUCCESS)
                    {

                        oldcall = tmpChan.call2;

                        currcall = tmpChan.call1;
                    }
                    else
                    {
                        /*	If we get here there is a serious problem with the way the PDF is
                         *	constructed.  We wern't able to merge the passive links and we can't
                         *	run passive power calculations
                         */
                        DynChannel.FtCloseChannel(tmphandle);
                        FtValidate.runpassives = false;

                        ValErrs.AddMess("MERGE 20 - Error fetching passive channel for:-\r\n\t%s",
                                                    tableName, "W", whereClause);
                        return (Constant.SUCCESS);
                    }

                    DynChannel.FtCloseChannel(tmphandle);
                }

                /*  On exit from the above loop, tmpChan will have the active end pointed
                *   to by ftChan. */

                /* 	before we do a check to ensure that these fields are
                * 	OK or not OK, we need to get the MDB values of these
                * 	four fields since it is OK for these fields to be different
                * 	from thier transmit equivalents as long as the are equal
                * 	to the original MDB values.  This would be the same as
                * 	these fields being modified from the originators side
                * 	but the local passive fields still have the original data
                * 	from the MDB.  This is a non-error situation.
                */

                /* 	If the link is being added, however, we do not need
                * 	to consider the MDB records (there are none) so we
                * 	set the originating values to be those of the current
                * 	record
                */

                if (!ftChan.CmdEquals('A'))
                {
                    whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                        ftChan.call1, ftChan.call2, bndcde, chid);

                    nHandle = DynMdbChannel.MtSelectChannel(whereClause, "");
                    rc = DynMdbChannel.MtFetchChannel(nHandle, out mtChan, out mtChanNulls);

                    if (rc != Constant.SUCCESS)
                    {
                        ret = Constant.FAILURE;

                        ValErrs.AddMess("MERGE 21 - No mdb channel.",
                            ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, bndcde, 0, chid), "E");
                        errorCount++;
                    }
                    else
                    {
                        freqtxmdb = mtChan.freqtx;
                        freqtxnull = mtChanNulls[MtChan.FREQTX];

                        poltxmdb = mtChan.poltx;
                        poltxnull = mtChanNulls[MtChan.POLTX];

                        trafmdb = mtChan.trafrx;
                        trafmdbnull = mtChanNulls[MtChan.TRAFRX];

                        eqptmdb = mtChan.eqptrx;
                        eqptmdbnull = mtChanNulls[MtChan.EQPTRX];
                    }

                    DynMdbChannel.MtCloseChannel(nHandle);
                }
                else
                {
                    freqtxmdb = ftChan.freqtx;
                    freqtxnull = nArrayFW[FtChan.FREQTX];

                    poltxmdb = ftChan.poltx;
                    poltxnull = nArrayFW[FtChan.POLTX];

                    trafmdb = ftChan.traftx;
                    trafmdbnull = nArrayFW[FtChan.TRAFRX];

                    eqptmdb = ftChan.eqpttx;
                    eqptmdbnull = nArrayFW[FtChan.EQPTRX];
                }

                /*
                 * 	TASK 497: To fix a validate error for this task, we do multiple
                 * 	checks to make certain that the frequencies exist and should
                 * 	be compared. Otherwise, we get a floating point error and the
                 * 	program crashes.
                 */
                freqChanged = false;
                if (nArrayFW[FtChan.FREQTX] == Constant.DB_NULL)
                {
                    if (tmpNulls[FtChan.FREQTX] != Constant.DB_NULL && freqtxnull != Constant.DB_NULL)
                    {
                        freqChanged = true;
                    }
                }
                else
                {
                    if ((tmpNulls[FtChan.FREQTX] == Constant.DB_NULL ||
                        ftChan.freqtx != tmpChan.freqtx) &&
                        (freqtxnull == Constant.DB_NULL || ftChan.freqtx != freqtxmdb))
                    {
                        freqChanged = true;
                    }
                }

                //if (freqChanged && (*ftChan.cmd != 'D')) {
                if (freqChanged && !ftChan.CmdEquals('D'))
                {
                    ValErrs.AddMess("MERGE 22 - Must change transmit frequency field at the TX end.\r\nPower calculations will not be done.",
                                                ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid), "E");
                    FtValidate.runpassives = false;
                    errorCount++;
                }

                //if (((strcmp(ftChan.poltx, poltxmdb) != 0) &&
                //    (strcmp(ftChan.poltx, tmpChan.poltx) != 0)) &&
                //    (*ftChan.cmd != 'D'))
                if ((!ftChan.poltx.Equals(poltxmdb) && !ftChan.poltx.Equals(tmpChan.poltx)) && !ftChan.CmdEquals('D'))
                {
                    ValErrs.AddMess("MERGE 23 - Must change polarization field at the originator.\r\nValidate will use the originating transmit field for this value (%s).",
                        ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid), "W", ftChan.poltx);
                    warningCount++;
                }


                //if (((strcmp(ftChan.eqpttx, eqptmdb) != 0) &&
                //(strcmp(ftChan.eqpttx, tmpChan.eqpttx) != 0)) &&
                //(*ftChan.cmd != 'D'))
                if ((!ftChan.eqpttx.Equals(eqptmdb) && !ftChan.eqpttx.Equals(tmpChan.eqpttx)) && !ftChan.CmdEquals('D'))
                {
                    ValErrs.AddMess("MERGE 24 - Must change equipment transmit field at originator.\tValidate will use the originating transmit field for this value (%s).",
                                                ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid), "W", ftChan.eqpttx);
                    warningCount++;
                }

                //if (((strcmp(ftChan.traftx, trafmdb) != 0) &&
                //    (strcmp(ftChan.traftx, tmpChan.traftx) != 0)) &&
                //    (*ftChan.cmd == 'D'))
                if ((!ftChan.traftx.Equals(trafmdb) && !ftChan.traftx.Equals(tmpChan.traftx)) && ftChan.CmdEquals('D'))
                {
                    ValErrs.AddMess("MERGE 25 - Must change traffic transmit field at originator.\r\n\tValidate will use the originating transmit field for this value (%s).",
                        ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid), "W", ftChan.trafrx);
                    warningCount++;
                }

                /* we must find the end station where the recieve fields
                 * for this record are controlled by the transmit fields */
                oldcall = ftChan.call1;

                currcall = ftChan.call2;

                // Bug fix: b150722A
                // It is possible that currcall is null or empty so test for it prior to accessing currcall[0]. 
                while (!String.IsNullOrWhiteSpace(currcall) && currcall[0] == '%')
                {

                    whereClause = String.Format(" call1 = '{0}' and call2 != '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                        currcall, oldcall, bndcde, chid);

                    if ((tmphandle =
                        DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                    {

                        ValErrs.AddMess("MERGE 26 - Could not read passive channel. Reason %d",
                            ValErrs.MakeKeyLine(currcall, oldcall, bndcde, 0, chid), "E", tmphandle.ToString());
                        errorCount++;
                        ret = Constant.FAILURE;
                    }

                    if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                    {
                        oldcall = tmpChan.call1;

                        currcall = tmpChan.call2;
                    }
                    else
                    {

                        ValErrs.AddMess("MERGE 27 - Active site not found.", currcall, "E");

                        currcall = "";
                        errorCount++;
                        ret = Constant.FAILURE;
                    }


                    DynChannel.FtCloseChannel(tmphandle);
                } // End of while (currcall[0] == '%')

                /* Here we have found the end of the link.  We need to get
                 * the remote record because we have previously been dealing
                 * with the receive fields.  We need to get the transmit
                 * information to make sure that the recieve info is
                 * not different from the transmit info
                 */

                whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                    currcall, oldcall, bndcde, chid);

                if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                {

                    ValErrs.AddMess("MERGE 28 - Could not read passive channel.  Reason %d",
                        tableName, "E", tmphandle.ToString());
                    errorCount++;
                    ret = Constant.FAILURE;
                }

                if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                {

                    eqpttemp = tmpChan.eqpttx;
                    traftemp = tmpChan.traftx;
                    eqpttempnull = tmpNulls[FtChan.EQPTTX];
                    traftempnull = tmpNulls[FtChan.TRAFTX];
                }


                DynChannel.FtCloseChannel(tmphandle);

                /* before we do a check to ensure that these fields are
                 * OK or not OK, we need to get the MDB values of these
                 * two fields since it is OK for these fields to be different
                 * from thier transmit equivalents as long as the are equal
                 * to the original MDB values.  This would be the same as
                 * these fields being modified from the transmit side
                 * but the recieve fields still have the original data
                 * from the MDB.  This is a non-error situation.
                 */

                //sprintf(whereClause, "select trafrx, eqptrx "
                //                      "from mt_chan "
                //                     "where call1 = '%s' and call2 = '%s' and "
                //                            "bndcde = '%s' and chid = '%s'",
                //                     ftChan.call1, ftChan.call2, bndcde, chid);

                //exec sql execute immediate :whereClause
                //into :trafmdb:trafmdbnull, :eqptmdb:eqptmdbnull;

                whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                ftChan.call1, ftChan.call2, bndcde, chid);

                nHandle = DynMdbChannel.MtSelectChannel(whereClause, "");

                rc = DynMdbChannel.MtFetchChannel(nHandle, out mtChan, out mtChanNulls);

                if (rc == ODBC.SQL_NO_DATA)
                {
                    trafmdb = "";
                    eqptmdb = "";
                }
                else if (rc == Constant.SUCCESS)
                {
                    trafmdb = mtChan.trafrx;
                    eqptmdb = mtChan.eqptrx;
                }

                if (!ftChan.eqptrx.Equals(eqptmdb) && !ftChan.eqptrx.Equals(eqpttemp))
                {
                    ValErrs.AddMess("MERGE 29 - Must change RX equipment field from remote active TX side.  Validate will use the value in the remote active TX equipment field (%s).",
                                            ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid),
                                            "W", ftChan.eqptrx);
                    warningCount++;
                }

                if (!ftChan.trafrx.Equals(trafmdb) && !ftChan.trafrx.Equals(traftemp))
                {
                    ValErrs.AddMess("MERGE 30 - Must change RX traffic field from remote active TX side. Validate will use the value in the remote active TX traffic field (%s).",
                        ValErrs.MakeKeyLine(ftChan.call1, ftChan.call2, ftChan.bndcde, 0, ftChan.chid),
                        "W", ftChan.trafrx);
                    warningCount++;
                }

                DynMdbChannel.MtCloseChannel(nHandle);
            }

            DynChannel.FtCloseChannel(chandle);


            /* Now we must fill out the appropriate records given a change
             * in frequency, polarization, equipment code or traffic code.
             * (eg. for a change in freqtx, we must change freqrx of remote,
             * freqtx of reflected, frqerx of reflected remote and so on down
             * the link until we hit the end active site
             */

            whereClause = " left(call1, 1) != '%' and left(call2, 1) = '%' and (cmd = 'U' or cmd = 'B')";     /* and recstat = 'U'"); *//* 1070 */

            if ((chandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
            {

                ValErrs.AddMess("MERGE 31 - Could not read channel information. Reason %d",
                    tableName, "E", chandle.ToString());
                errorCount++;
                ret = Constant.FAILURE;
            }

            while (DynChannel.FtFetchChannel(chandle, out ftChan, out nArrayFW) == Constant.SUCCESS)
            {
                // Bug fix: b150722A
                // If the ftChan has an invalid 'cmd' then don't apply any further validation.
                // Continue to the next ftChan to be fetched from the DB.
                if (!FtUtil.IsValidCmd(ftChan.cmd))
                {
                    continue;
                }

                oldcall = ftChan.call1;

                currcall = ftChan.call2;

                bndcde = ftChan.bndcde;

                chid = ftChan.chid;
                /* we must first modify the receive fields for the remote record */

                whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                ftChan.call2, ftChan.call1, bndcde, chid);

                if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                {

                    ValErrs.AddMess("MERGE 32 - Could not read passive channel.  Reason %d",
                        tableName, "E", tmphandle.ToString());
                    errorCount++;
                    ret = Constant.FAILURE;
                }

                if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                {
                    if (nArrayFW[FtChan.FREQTX] != Constant.DB_NULL)
                    {
                        tmpChan.freqrx = ftChan.freqtx;
                        tmpChan.polrx = ftChan.poltx;
                        tmpChan.trafrx = ftChan.traftx;
                        tmpChan.eqptrx = ftChan.eqpttx;

                        tmpNulls[FtChan.FREQRX] = nArrayFW[FtChan.FREQTX];
                        tmpNulls[FtChan.POLRX] = nArrayFW[FtChan.POLTX];
                        tmpNulls[FtChan.TRAFRX] = nArrayFW[FtChan.TRAFTX];
                        tmpNulls[FtChan.EQPTRX] = nArrayFW[FtChan.EQPTTX];
                    }

                    if (DynChannel.FtUpdateChannel(tmphandle, tmpChan, tmpNulls) != Constant.SUCCESS)
                    {
                        ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                    }
                }


                DynChannel.FtCloseChannel(tmphandle);

                /* we loop through the link until we find a record which
                 * corresponds to the 'far' active site.
                 */

                while (currcall[0] == '%')
                {
                    /* we first find the next record in the passive link
                     * making sure that we don't select the remote
                     * record for the previous channel
                     */

                    whereClause = String.Format(" call1 = '{0}' and call2 != '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                    currcall, oldcall, bndcde, chid);

                    if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                    {

                        ValErrs.AddMess("MERGE 33 - Could not read passive channel. Reason %d",
                                                    tableName, "E", tmphandle.ToString());
                        errorCount++;
                        ret = Constant.FAILURE;
                    }

                    if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                    {
                        /* We then update the next record in the link
                         * using the freq, pol code, and traf from
                         * the initiating active record
                         */

                        if (nArrayFW[FtChan.FREQTX] != Constant.DB_NULL)
                        {
                            tmpChan.freqtx = ftChan.freqtx;
                            tmpChan.poltx = ftChan.poltx;
                            tmpChan.traftx = ftChan.traftx;
                            tmpChan.eqpttx = ftChan.eqpttx;

                            tmpNulls[FtChan.FREQTX] = nArrayFW[FtChan.FREQTX];
                            tmpNulls[FtChan.POLTX] = nArrayFW[FtChan.POLTX];
                            tmpNulls[FtChan.TRAFTX] = nArrayFW[FtChan.TRAFTX];
                            tmpNulls[FtChan.EQPTTX] = nArrayFW[FtChan.EQPTTX];
                        }

                        if (DynChannel.FtUpdateChannel(tmphandle, tmpChan, tmpNulls) != Constant.SUCCESS)
                        {

                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                        }
                    }


                    DynChannel.FtCloseChannel(tmphandle);

                    /* we modify the old and current variables to indicate
                     * that this is now the current record */

                    oldcall = tmpChan.call1;
                    currcall = tmpChan.call2;

                    /* we must also modify the receive fields for the remote record  */

                    whereClause = String.Format(" call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and chid = '{3}'",
                                                    tmpChan.call2, tmpChan.call1, bndcde, chid);

                    if ((tmphandle = DynChannel.FtSelectChannel(tableName, whereClause, "")) < 0)
                    {
                        ValErrs.AddMess("MERGE 34 - Could not read passive channel. Reason %d",
                                                    tableName, "E", tmphandle.ToString());
                        errorCount++;
                        ret = Constant.FAILURE;
                    }

                    if (DynChannel.FtFetchChannel(tmphandle, out tmpChan, out tmpNulls) == Constant.SUCCESS)
                    {
                        if (nArrayFW[FtChan.FREQTX] != Constant.DB_NULL)
                        {
                            tmpChan.freqrx = ftChan.freqtx;
                            tmpChan.polrx = ftChan.poltx;
                            tmpChan.trafrx = ftChan.traftx;
                            tmpChan.eqptrx = ftChan.eqpttx;

                            tmpNulls[FtChan.FREQRX] = nArrayFW[FtChan.FREQTX];
                            tmpNulls[FtChan.POLRX] = nArrayFW[FtChan.POLTX];
                            tmpNulls[FtChan.TRAFRX] = nArrayFW[FtChan.TRAFTX];
                            tmpNulls[FtChan.EQPTRX] = nArrayFW[FtChan.EQPTTX];
                        }

                        if (DynChannel.FtUpdateChannel(tmphandle, tmpChan, tmpNulls) != Constant.SUCCESS)
                        {
                            ErrMsg.UtPrintMessage(Error.DYN_MS_SQL_SERVER_ERR);
                        }
                    }


                    DynChannel.FtCloseChannel(tmphandle);
                }
            }

            DynChannel.FtCloseChannel(chandle);

            //...Log2.v("\n\nFtValMergePassiveLinks: Exit");
            return (ret);
        }




    }
}

```
