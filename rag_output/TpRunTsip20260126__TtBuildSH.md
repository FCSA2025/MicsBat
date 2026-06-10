# Documented File: TtBuildSH.cs
**Repository Path:** `TpRunTsip20260126\TtBuildSH.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TpRunTsip
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using global::TpRunTsip;
    using System.Runtime.InteropServices;
    using SQLLEN = Int64;
    using SQLHANDLE = IntPtr;
    using _Auxlib;

    /// <summary>
    /// Provides a large number of methods
    /// used to create and populate the TS SH Tables with data.
    /// </summary>
    public class TtBuildSH
    {
#if PINVOKE
        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int ttBuildSHTable([In] string tsipName, [In, Out] TpParm tpParm, [In, Out] SQLLEN[] tpParmNulls, [In, Out] ref int numCases, [In] string startDate, [In] string startTime);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int vic2DimTable([In] TpParm tpParm,
                                                [In] FtSiteStr_OLD ftSiteStr_OLD,
                                                [In] FtSiteStrNulls_OLD ftSiteStrNull_OLD,
                                                [In] IntPtr tLinks_OLD_IntPtr,
                                                [In] int nLinkNum,
                                                [In] string envName,
                                                [In] string proName,
                                                [In] int isMDBInt,
                                                [In] string selCommand,
                                                [In, Out] ref int numCases);

        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int createAnteSHTable([In, Out] TpParm tpParmStruct,
                                                    [In, Out] TtSite proSiteStruct,
                                                    [In, Out] SQLLEN[] proSiteNulls,
                                                    [In, Out] TtSite envSiteStruct,
                                                    [In, Out] SQLLEN[] envSiteNulls,
                                                    [In] string proName,
                                                    [In] string envName,
                                                    [In, Out] FtSiteStr_OLD pProSite,
                                                    [In, Out] FtSiteStrNulls_OLD pProSiteNulls,
                                                    [In, Out] TLink_OLD pProLink,
                                                    [In, Out] FtSiteStr_OLD pEnvSite,
                                                    [In, Out] FtSiteStrNulls_OLD pEnvSiteNulls,
                                                    [In, Out] TLink_OLD pEnvLink,
                                                    [In] string envCall1,
                                                    [In] string envCall2,
                                                    [In] string envBndCode,
                                                    [In, Out] ref int numProAnte,
                                                    [In, Out] ref int numEnvAnte,
                                                    [In, Out] ref int numCases);


        [DllImport("tpRunTsip.dll", CharSet = CharSet.Ansi)]
        private extern static int createChanSHTable([In, Out] TpParm tpParmStruct,
                                                    [In, Out] TtSite proSiteStruct,
                                                    [In, Out] SQLLEN[] proSiteNulls,
                                                    [In, Out] TtSite envSiteStruct,
                                                    [In, Out] SQLLEN[] envSiteNulls,
                                                    [In, Out] TtAnte proAnteStruct,
                                                    [In, Out] SQLLEN[] proAnteNulls,
                                                    [In, Out] TtAnte envAnteStruct,
                                                    [In, Out] SQLLEN[] envAnteNulls,
                                                    [In] string proName,
                                                    [In] string envName,
                                                    [In, Out] ref int numProChan,
                                                    [In, Out] ref int numEnvChan,
                                                    [In, Out] ref int numCases,
                                                    [In] double envMBnd,
                                                    [In] double proMBnd,
                                                    [In] double envPatLoss,
                                                    [In] double proPatLoss);

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int CreateChanSHTable_NATIVE(TpParm tpParmStruct,
                                            TtSite proSiteStruct,
                                            SQLLEN[] proSiteNulls,
                                            TtSite envSiteStruct,
                                            SQLLEN[] envSiteNulls,
                                            TtAnte proAnteStruct,
                                            SQLLEN[] proAnteNulls,
                                            TtAnte envAnteStruct,
                                            SQLLEN[] envAnteNulls,
                                            string proName,
                                            string envName,
                                            ref int numProChan,
                                            ref int numEnvChan,
                                            ref int numCases,
                                            double envMBnd,
                                            double proMBnd,
                                            double envPatLoss,
                                            double proPatLoss)
        {
            //...Log2.v("\nTtBuildSH.CreateChanSHTable_NATIVE(): Entry");

            // 'out' requirements.
            //numProChan = 0;
            //numEnvChan = 0;

            int rc = Constant.FAILURE;

            try
            {
                rc = createChanSHTable(tpParmStruct,
                                            proSiteStruct,
                                            proSiteNulls,
                                            envSiteStruct,
                                            envSiteNulls,
                                            proAnteStruct,
                                            proAnteNulls,
                                            envAnteStruct,
                                            envAnteNulls,
                                            proName,
                                            envName,
                                            ref numProChan,
                                            ref numEnvChan,
                                            ref numCases,
                                            envMBnd,
                                            proMBnd,
                                            envPatLoss,
                                            proPatLoss);
            }
            catch (Exception e)
            {
                //...Log2.v("\nTtBuildSH.CreateChanSHTable_NATIVE(): ERROR: then Exit: " + e.Message + "\n" + e.StackTrace);
            }

            //...Log2.v("\nTtBuildSH.CreateChanSHTable_NATIVE(): Exit");
            return rc;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int CreateAnteSHTable_NATIVE(ref TpParm tpParmStruct,
                                                    ref TtSite proSiteStruct,
                                                    ref SQLLEN[] proSiteNulls,
                                                    ref TtSite envSiteStruct,
                                                    ref SQLLEN[] envSiteNulls,
                                                    string proName,
                                                    string envName,
                                                    ref FtSiteStr pProSite,
                                                    ref FtSiteStrNulls pProSiteNulls,
                                                    ref TLink pProLink,
                                                    ref FtSiteStr pEnvSite,
                                                    ref FtSiteStrNulls pEnvSiteNulls,
                                                    ref TLink pEnvLink,
                                                    string envCall1,
                                                    string envCall2,
                                                    string envBndCode,
                                                    ref int numProAnte,
                                                    ref int numEnvAnte,
                                                    ref int numCases)
        {
            //...Log2.v("\nTtBuildSH.CreateAnteSHTable_NATIVE(): Entry.");
            int rv = Constant.FAILURE;

            try
            {
                // Need to use the 'old' version of FtSiteStr that uses pointers not arrays.
                FtSiteStr_OLD pProSite_OLD = new FtSiteStr_OLD(pProSite);
                FtSiteStr_OLD pEnvSite_OLD = new FtSiteStr_OLD(pEnvSite);

                // Need to use the 'old' version of FtSiteStrNulls that uses pointers not arrays.
                FtSiteStrNulls_OLD pProSiteNulls_OLD = new FtSiteStrNulls_OLD(pProSiteNulls);
                FtSiteStrNulls_OLD pEnvSiteNulls_OLD = new FtSiteStrNulls_OLD(pEnvSiteNulls);

                // Need to use the 'old' version of TLink that uses pointers not arrays.
                TLink_OLD pProLink_OLD = new TLink_OLD(pProLink);
                TLink_OLD pEnvLink_OLD = new TLink_OLD(pEnvLink);

                // Make the call to the native C/C++ function.
                rv = createAnteSHTable(tpParmStruct,
                                                    proSiteStruct,
                                                    proSiteNulls,
                                                    envSiteStruct,
                                                    envSiteNulls,
                                                    proName,
                                                    envName,
                                                    pProSite_OLD,
                                                    pProSiteNulls_OLD,
                                                    pProLink_OLD,
                                                    pEnvSite_OLD,
                                                    pEnvSiteNulls_OLD,
                                                    pEnvLink_OLD,
                                                    envCall1,
                                                    envCall2,
                                                    envBndCode,
                                                    ref numProAnte,
                                                    ref numEnvAnte,
                                                    ref numCases);

                // Need to convert back to 'new' structure objects.
                pProSite = new FtSiteStr(pProSite_OLD);
                pEnvSite = new FtSiteStr(pEnvSite_OLD);

                pProLink = new TLink(pProLink_OLD);
                pEnvLink = new TLink(pEnvLink_OLD);

                pProSiteNulls = new FtSiteStrNulls(pProSiteNulls_OLD, pProSite_OLD.nNumAnts, pProSite_OLD.nNumChans);
                pEnvSiteNulls = new FtSiteStrNulls(pEnvSiteNulls_OLD, pProSite_OLD.nNumAnts, pProSite_OLD.nNumChans);
            }
            catch (Exception e)
            {
                Log2.e("\nTtBuildSH.CreateAnteSHTable_NATIVE(): ERROR: exception:\n" + e.Message + "\n" + e.StackTrace);
                Application.Exit("Forced exit from: TtBuildSH.CreateAnteSHTable_NATIVE()");
            }

            //...Log2.v("\nTtBuildSH.CreateAnteSHTable_NATIVE(): Exit: successful.");
            return rv;
        }

        [System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute()]
        public static int Vic2DimTable_NATIVE(TpParm tpParm,
                                FtSiteStr ftSiteStr,   //	Full proposed site
                                FtSiteStrNulls ftSiteStrNulls,
                                TLink[] tLinks,
                                int nLinkNum,       //	This is the link we are processing.
                                string envName,
                                string proName,
                                bool isMDB,
                                string selCommand,  //	Selection from the environment.
                                ref int numCases)
        {
            //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): called.");
            int rv = Constant.FAILURE;

            try
            {

                int isMDBInt = isMDB ? 1 : 0;

                // [In] FtSiteStr.
                FtSiteStr_OLD ftSiteStr_OLD = new FtSiteStr_OLD(ftSiteStr);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): A");

                FtSiteStrNulls_OLD ftSiteStrNull_OLD = new FtSiteStrNulls_OLD(ftSiteStrNulls);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): B");

                TLink_OLD[] tLinks_OLD = TLink_OLD.ConvertArray(tLinks);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): C");
                IntPtr tLinks_OLD_IntPtr = TLink_OLD.CopyArrayToGlobalMemory(tLinks_OLD);
                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): D");

                rv = vic2DimTable(tpParm,
                                   ftSiteStr_OLD,
                                   ftSiteStrNull_OLD,
                                   tLinks_OLD_IntPtr,
                                   nLinkNum,
                                   envName,
                                   proName,
                                   isMDBInt,
                                   selCommand,
                                   ref numCases
                                   );

                //...Log2.v("\nTtBuildSH.Vic2DimTable_NATIVE(): E");
            }
            catch (Exception e)
            {
                Log2.e("\nTtBuildSH.Vic2DimTable_NATIVE(): ERROR: exception:\n" + e.Message + "\n" + e.StackTrace);
                Application.Exit("Forced exit from: TtBuildSH.Vic2DimTable_NATIVE()");
            }
            return rv;
        }

        public static int TtBuildSHTable_NATIVE(string tsipName,
                                    ref TpParm tpParm,
                                    ref SQLLEN[] tpParmNulls,
                                    ref int numCases,
                                    string startDate,
                                    string startTime)
        {
            //...Log2.v("\nTtBuildSH.TtBuildSHTable_NATIVE(): Entry");

            int rc = -666;

            // Native call.
            rc = ttBuildSHTable(tsipName, tpParm, tpParmNulls, ref numCases, startDate, startTime);

            //...Log2.v("\nTtBuildSH.TtBuildSHTable_NATIVE(): Exit, rc = " + rc);
            return rc;
        }
#endif

        private const int TEN = 10;

        /// <summary>
        /// This method provides top-level management for the creation of TS SH Tables
        /// and their population with results data. 
        /// </summary>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <param name="tpParmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="tpParmNulls"> - ODBC nullInds associated with tpParm.</param>
        /// <param name="numCases"> - accumulated count of the number of interference cases.</param>
        /// <param name="startDate"> - TSIP processing start date to be written to tpParm.</param>
        /// <param name="startTime"> - TSIP processing start date to be written to tpParm.</param>
        /// <returns></returns>
        public static int TtBuildSHTable(string tsipName,
                                            ref TpParm tpParmStruct,
                                            ref SQLLEN[] tpParmNulls,
                                            ref int numCases,
                                            string startDate,
                                            string startTime)
        {
            //...Log2.v("\nTtBuildSH.TtBuildSHTable(): Entry");

            int rc;
            string ttParmName;  /* TSIP parm table name */

            /* create empty SH TABLES */
            rc = TtCreateTsipTables(tsipName);

            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nTtBuildSH.TtBuildSHTable(): ERROR: call to TtCreateTsipTables() failed, rc = " + rc + ". Exit.");
                ErrMsg.UtPrintMessage(rc);
                return (Constant.FAILURE);
            }

            /* compose internal parameter table name */
            GenUtil.UtCvtName(Constant.TT_PARM, tsipName, out ttParmName);

            /* insert current date and time into parm rec */
            tpParmStruct.mdate = startDate;
            tpParmStruct.mtime = startTime;
            tpParmStruct.numcases = -1;
            tpParmStruct.numtecases = -1;
            tpParmNulls[TpParm.MDATE] = Constant.DB_NOT_NULL;
            tpParmNulls[TpParm.MTIME] = Constant.DB_NOT_NULL;
            tpParmNulls[TpParm.NUMCASES] = Constant.DB_NOT_NULL;
            tpParmNulls[TpParm.NUMTECASES] = Constant.DB_NOT_NULL;

            /* insert parameter record into SH TABLE */
            rc = TsipUtils.UtInsertParmRecord(ttParmName, tpParmStruct, tpParmNulls);
            if (rc != Constant.SUCCESS)
            {
                Log2.e("\nTtBuildSH.TtBuildSHTable(): ERROR: call to UtInsertParmRecord() failed. Exit.");
                ErrMsg.UtPrintMessage(rc);
                return (-10);
            }

            tpParmStruct.numcases = 0;
            tpParmStruct.numtecases = 0;

            /* perform initial rough cull to extract a set of affected sites,
             * then fine cull, and create the temporary interferer and victim
             * site pair tables.  Then from the two site pair tables, create
             * the SH TABLES and complete with interference calculations */
            if ((rc = TtCullNCreate(tpParmStruct, tsipName, ref numCases)) != Constant.SUCCESS)
            {
                if (rc != Constant.FAILURE)
                {
                    Log2.e("\nTtBuildSH.TtBuildSHTable(): ERROR: call to TtCullNCreate() failed. Exit.");
                    ErrMsg.UtPrintMessage(rc);
                }
                return (-11);
            }

            //...Log2.v("\nTtBuildSH.TtBuildSHTable(): Exit: final");
            return (Constant.SUCCESS);

        } /* ----- end ttBuildSHTable ----- */

        /// <summary>
        /// This method creates the empty SH and Temporary 
        /// tables to be populated by TSIP.   
        /// </summary>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <returns></returns>
        public static int TtCreateTsipTables(string tsipName)
        {
            int rc;     /* return code from drop or create */

            /* create TSIP SH TABLE */
            if (Ssutil.UtTableExist(Constant.TT, tsipName))
            {
                if ((rc = Ssutil.UtDropTable(Constant.TT, tsipName)) != Constant.SUCCESS)
                {
                    Log2.e("\nTtBuildSH.TtCreateTsipTables(): ERROR: call to Ssutil.UtDropTable() failed, rc = " + rc);
                    return (rc);
                }
            }

            if ((rc = Ssutil.UtCreateTable(Constant.TT, tsipName)) != Constant.SUCCESS)
            {
                Log2.e("\nTtBuildSH.TtCreateTsipTables(): ERROR: call to Ssutil.UtCreateTable() failed, rc = " + rc);
                return (rc);
            }

            return (Constant.SUCCESS);

        } /*---- end ttCreateTsipTables ----*/

        /// <summary>
        /// This method controls the selection of the sites, 
        /// antennae, and channels from the environment that satisfy the User's
        /// requirements and then populates the associated fields in the SH Tables. 
        /// </summary>
        /// <param name="tpParmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <param name="numCases"> - accumulated count of the number of interference cases.</param>
        /// <returns></returns>
        public static int TtCullNCreate(TpParm tpParmStruct,
                                        string tsipName,
                                        ref int numCases)
        {
            //...Log2.v("\nTtBuildSH.TtCullNCreate(): Entry");

            int rc;
            bool isMDB = false;
            string codesSelection;      /* selected code selectn criteria */
            string sqlCommand;          /* selected code selectn criteria */
            string ttSiteName;          /* TSIP site table name */
            string ttAnteName;          /* TSIP ante table name */
            string ttChanName;          /* TSIP chan table name */
            string envSiteName;         /* environment site table name*/
            string envAnteName;         /* environment ante table name*/
            string envChanName;         /* environment chan table name*/
            string proSiteName;         /* proposed site table name */
            string proAnteName;         /* proposed ante table name */
            string proChanName;         /* proposed chan table name */

            string cCallFound;
            FtSiteStr pSite;
            FtSiteStrNulls pSiteNulls;
            int nInd;
            TLink[] pLinks;
            int nNumLinks;

            TtTableNames(tsipName,
                tpParmStruct.envtype,
                tpParmStruct.proname,
                ref tpParmStruct.envname,
                out ttSiteName,
                out ttAnteName,
                out ttChanName,
                out envSiteName,
                out envAnteName,
                out envChanName,
                out proSiteName,
                out proAnteName,
                out proChanName,
                ref isMDB);

            codesSelection = "";

            //...Log2.v("\nTtBuildSH.TtCullNCreate(): A");

            /* create selection criteria for culling of operator codes or call signs*/
            TsipUtils.UtOpCodesCallSigns(tpParmStruct, out codesSelection);

            /* prepare SH SITE, ANTENNA, and CHANNEL tables for insert */
            if ((rc = TtDynSite.TtPrepareSite(ttSiteName)) != Constant.SUCCESS)
            {
                Log2.e("\nTtBuildSH.TtCullNCreate(): ERROR: call to TtPrepareSite() failed. Exit");
                GenUtil.SetError(-20, "Could not prepare site: " + rc);
                return Constant.FAILURE;
            }
            //...Log2.v("\nTtBuildSH.TtCullNCreate(): B");
            if ((rc = TtDynAnte.TtPrepareAnte(ttAnteName)) != Constant.SUCCESS)
            {
                Log2.e("\nTtBuildSH.TtCullNCreate(): ERROR: call to TtPrepareAnte() failed. Exit");
                GenUtil.SetError(-21, "Could not prepare antenna: " + rc);
                return Constant.FAILURE;
            }
            //...Log2.v("\nTtBuildSH.TtCullNCreate(): C");
            if ((rc = TtDynChan.TtPrepareChan(ttChanName)) != Constant.SUCCESS)
            {
                Log2.e("\nTtBuildSH.TtCullNCreate(): ERROR: call to TtPrepareChan() failed. Exit");
                GenUtil.SetError(-22, "Could not prepare channel: " + rc);
                return Constant.FAILURE;
            }

            /* loop through all sites in the Proposed file and process each
             * completely (ie. all its ante's and chan's) before doing next site */
            cCallFound = "";

            rc = FtUtils.FtEnumSite("cmd != 'D'", tpParmStruct.proname, ref cCallFound); //	Start the search.

            //...Log2.v("\nTtBuildSH.TtCullNCreate(): D");
            /* for (each site in the proposed file) */
            while (rc == 0)
            {
                //...Log2.v("\nTtBuildSH.TtCullNCreate(): E");
                //	Got the next call sign in the enumeration.  Get the site.
                rc = FtUtils.FtGetSiteWN(cCallFound, out pSite, 3, tpParmStruct.proname, out pSiteNulls);

                if (rc != 0)
                {
                    Log2.e("\nTtBuildSH.TtCullNCreate(): ERROR: call to FtGetSiteWN() failed. Exit");
                    TpRunTsip.mTW_ERR.Write("\nttBuildSH: Error getting a proposed site:-\n{0}\n", GenUtil.GetUserMess());
                    return -25;
                }

                //	Get the links for this site.
                nNumLinks = FtUtils.FtMakeLinks(pSite, out pLinks);

                //...Log2.v("\nTtBuildSH.TtCullNCreate(): F");
                if (nNumLinks < 0)
                {
                    //	error getting links
                    Log2.e("\nTtBuildSH.TtCullNCreate(): ERROR: call to FtMakeLinks() failed. Exit.");
                    String str = String.Format("ttCullNCreate:  Error getting links for {0} ({1})", pSite.stSite.call1, pSite.stSite.name);
                    GenUtil.SetErr(str);
                    return Constant.FAILURE;
                }

                //	Go through the proposed site's links.
                for (nInd = 0; nInd < nNumLinks; nInd++)
                {
                    //...Log2.v("\nTtBuildSH.TtCullNCreate(): G");
                    //	Generate the rough cull for this link by using the first antenna in the link.
                    GenRoughCull(tpParmStruct, pSite, pLinks[nInd], isMDB,
                        codesSelection, out sqlCommand);

                    //...Log2.v(pSiteNulls.ToString());

                    //...Log2.v("\nTtBuildSH.TtCullNCreate(): H");
                    //	Process this link against the environment in the rough cull...
                    //AH: avoid warning C4706: assignment within conditional expression
                    rc = Vic2DimTable(tpParmStruct,
                                                pSite,
                                                pSiteNulls,
                                                pLinks,
                                                nInd,
                                                tpParmStruct.envname,
                                                tpParmStruct.proname,
                                                isMDB,
                                                sqlCommand,
                                                ref numCases
                                                );

                    if (rc != Constant.SUCCESS)
                    {
                        rc = -1;
                        if (rc != 0)
                        {
                            //	Could not find the other end of the link.  Continue with next case
                            GenUtil.AddErr("Could not find the other end of the link, ignoring...");
                            continue;
                        }

                        Log2.e("\nTtBuildSH.TtCullNCreate(): ERROR: call to Vic2DimTable() failed. Exit");
                        return (GenUtil.AddErr("Could not create vic2Dim table: " + rc));
                    }

                    //...Log2.v("\nTtBuildSH.TtCullNCreate(): I");

                } // end of for loop

                FtUtils.FtFreeLinks(pLinks, nNumLinks);

                //...Log2.v("\nTtBuildSH.TtCullNCreate(): J");
                rc = FtUtils.FtEnumSite("cmd != 'D'", tpParmStruct.proname, ref cCallFound); //	continue the search.

            } /* end for (each site) */

            TtDynChan.TtChanClose();
            TtDynAnte.TtCloseAnte();
            TtDynSite.TtSiteClose();

            if (rc != Constant.NOMORERECS)
            {
                return (rc);
            }

            //...Log2.v("\nTtBuildSH.TtCullNCreate(): Exit: final.");
            return (Constant.SUCCESS);

        } /* ----- end ttCullNCreate ----- */

        /// <summary>
        /// This method builds the full SQL table names for the TS
        /// SH Tables and temp tables. The names are built from the tsip parameter file 
        /// name and the runname of the record within that file.   
        /// </summary>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <param name="envType"> - "MDB_TS" or "INTRA".</param>
        /// <param name="proName"> - table name of proposed sites.</param>
        /// <param name="envName"> - table name of sites in the environment.</param>
        /// <param name="ttSiteName"> - name of Tt site table.</param>
        /// <param name="ttAnteName"> - name of Tt ante table.</param>
        /// <param name="ttChanName"> - name of Tt chan table.</param>
        /// <param name="envSiteName"> - name of table for containing environment site.</param>
        /// <param name="envAnteName"> - name of table for containing environment ante.</param>
        /// <param name="envChanName"> - name of table for containing environment chan.</param>
        /// <param name="proSiteName"> - name of table for proposed site.</param>
        /// <param name="proAnteName"> - name of table for proposed ante.</param>
        /// <param name="proChanName"> - name of table for proposed chan.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        public static void TtTableNames(string tsipName,
                                        string envType,
                                        string proName,
                                        ref string envName,
                                        out string ttSiteName,
                                        out string ttAnteName,
                                        out string ttChanName,
                                        out string envSiteName,
                                        out string envAnteName,
                                        out string envChanName,
                                        out string proSiteName,
                                        out string proAnteName,
                                        out string proChanName,
                                        ref bool isMDB)
        {
            GenUtil.UtCvtName(Constant.TT_SITE, tsipName, out ttSiteName);
            GenUtil.UtCvtName(Constant.TT_ANTE, tsipName, out ttAnteName);
            GenUtil.UtCvtName(Constant.TT_CHAN, tsipName, out ttChanName);

            /* ENVIRONMENT TABLE NAMES */
            envType = envType.Trim();

            isMDB = false;
            if (envType.Equals("MDB_TS"))
            {
                isMDB = true;
                envSiteName = "mt_site";
                envAnteName = "mt_ante";
                envChanName = "mt_chan";
            }
            else if (envType.Equals("INTRA"))
            {
                envSiteName = proName;
                envAnteName = proName;
                envChanName = proName;
                //	In the INTRA case, the environment name in the parm table will be blank.
                //	Change this to the proposed file.
                envName = proName;
            }
            else
            {
                envSiteName = envName;
                envAnteName = envName;
                envChanName = envName;
            }

            /* PROPOSED TABLE NAMES */
            proSiteName = proName;
            proAnteName = proName;
            proChanName = proName;
        } /*---- end ttTableNames ----*/

        /// <summary>
        /// This method generates the text of the SQL SELECT query that culls 
        /// sites from the environment for further processing. To increase real-time performance
        /// use is made of user-defined functions that run on the SQL Server.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="ftIntSite"> - FtSiteStr object.</param>
        /// <param name="tLink"> - TLink object.</param>
        /// <param name="isMDB"> - boolean; true if the tables are in the main DB table set, otherwise false.</param>
        /// <param name="codesSelection"> - provides parameters to be used in user-defined functions on the SQL Server.</param>
        /// <param name="sqlCommand"> - text of an SQL query that performs culling using user-defined functions on the SQL Server.</param>
        public static void GenRoughCull(TpParm tpParm,
                                        FtSiteStr ftIntSite,
                                        TLink tLink,
                                        bool isMDB,
                                        //envName,
                                        string codesSelection,
                                        out string sqlCommand)
        {
            // 'out' requirement.
            sqlCommand = null;

            double coordDist = tpParm.coordist;
            string cDistString;
            string cBands;
            string cOr;
            string cOneBand;
            BandBits sBandBits;

            int nNumBands = Suutils.SuNumberOfBands();  //	Get the number of bands
            int nNumBandWds = (nNumBands + Constant.BITS_PER_BANDWORD - 1) / Constant.BITS_PER_BANDWORD;
            int nInd;
            int nInd1;

            //	Check for a non-deleted antenna.
            for (nInd = 0; nInd < tLink.nNumAnts; nInd++)
            {
                if (!ftIntSite.stAntsPtr[tLink.aAnts[nInd]].cmd.Equals("D"))
                {
                    break;
                }
            }

            //	If all the antennas are deleted, no processing.
            if (nInd < tLink.nNumAnts)
            {
                //	Not a deleted antenna
                if (tpParm.selsites.Equals("CALL SIGN"))
                {
                    /*	We don't pay attention to distance if we have call signs */
                    cDistString = "(1=1)";
                }
                else
                {

                    int nAntNo = tLink.aAnts[nInd];

                    //Use the distance and keyhole routines at the server
                    cDistString = String.Format("tsip.keyhole_hs({0}, {1}, latit, longit, {2:F6}, {3:F6}) <= 2 ",
                        ftIntSite.stSite.latit, ftIntSite.stSite.longit,
                        ftIntSite.stAntsPtr[nAntNo].azmth, coordDist);
                }

                /* Fill in the bandBit array with the adjacent band bits. */
                SiteAdjBands(ftIntSite.stSite, out sBandBits);
                //	Set the bandword search...
                cBands = "";
                cOr = "";
                for (nInd1 = 0; nInd1 < nNumBandWds; nInd1++)
                {
                    cOneBand = String.Format("{0}bandwd{1} & 0x{2:x8} != 0", cOr, nInd1 + 1, sBandBits.bitArray[nInd1]);
                    cOr = " or ";
                    cBands += cOneBand;
                }

                /* set up the sql command to select (call1, call2, bndcde) from
                 * the proposed antenna table where the call1 is the same as the
                 * current proposed site's call1, the latitude and longitude of the
                 * call1's site falls within the culling box. This select statement
                 * is completed in genKeyholeCull, where another culling box is added,
                 * the bndcde is in the list of bands adj to the proposed site, and
                 * the call1 is one of those specified by the user (if any) */
                if (isMDB == true)
                {
                    /* environment is MDB - no need to check command field*/

                    sqlCommand = String.Format("({0}) and ({1}) {2} ",
                        cDistString, cBands, codesSelection);

                }
                else
                {
                    /* environment is a filew - check command field & ignore DELETED records */

                    sqlCommand = String.Format("({0}) and ({1}) {2} and cmd != 'D' ",
                        cDistString, cBands, codesSelection);

                }

            }

        } /* ----- end genRoughCull ----- */

        /// <summary>
        /// This method checks if the interfering and victim 
        /// sites operate in adjacent bands with results output
        /// as a set of bits encoding true or false.
        /// </summary>
        /// <param name="intSiteStruct"> - FtSite object.</param>
        /// <param name="pBitArray"> - BandBits object encapsulating the set of result bits.</param>
        public static void SiteAdjBands(FtSite intSiteStruct,
    out BandBits pBitArray)
        {
            // 'out' requirement.
            pBitArray = new BandBits();

            int aBitPos;
            SdBand curBand;
            BandBits sBandAdj;
            int nBandCount = Suutils.SuNumberOfBands();

            pBitArray.Initialize(); // Zero the output array.

            // Get the bandbits from the FtSite object.
            BandBits bandBits = new BandBits(intSiteStruct);

            /* for each interferer band bit position.  */
            for (aBitPos = 1; aBitPos <= nBandCount; aBitPos++)
            {

                /* is band used */
                //if (GenUtil.UtTestBit(intSiteStruct.bandwd1, aBitPos - 1))
                if (GenUtil.UtTestBit(bandBits.bitArray, aBitPos - 1) == Enums.BIT.SET)
                {
                    /* check status of select */
                    if (TsipUtils.UtGetBandBit((short)aBitPos, out curBand) == Constant.SUCCESS)
                    {
                        //	We have the band for this bit position.  Get the adjacency 
                        Suutils.SuAdjBands(curBand.bndcde, out sBandAdj);
                        Suutils.SuBandBitsOR(ref pBitArray, sBandAdj);
                    }
                    else
                    {
                        /* band with this position doesn't exist */
                        ErrMsg.UtPrintMessage(Error.INVBNDBIT);
                    }
                }
            }
        } /*---- end siteAdjBands ----*/

        /// <summary>
        /// This method uses the SQL query produced by GenRoughCull to select the 
        /// (call1, call2, bndcode) sets from the environment and then process each of 
        /// these environment sets against each proposed set. Processing includes 
        /// additional culling as well as calculations.  
        /// </summary>
        /// <param name="tpParmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="ftIntSite"> - FtSiteStr object for proposed site.</param>
        /// <param name="ftIntSiteNulls"> - array of ODBC nullInds associated with ftIntSite.</param>
        /// <param name="pLinks"> - array of TLink objects.</param>
        /// <param name="nLinkNum"> - number of the link currently being processed.</param>
        /// <param name="envName"> - table name of sites in the environment.</param>
        /// <param name="proName"> - table name of proposed site.</param>
        /// <param name="isMDB"> - boolean; true if the tables are in the main DB table set, otherwise false.</param>
        /// <param name="selCommand"> - text of an SQL query previously output by GenRoughCull().</param>
        /// <param name="numCases"> - cummulative count of the number of interference cases.</param>
        /// <returns></returns>
        public static int Vic2DimTable(TpParm tpParmStruct,
                                        FtSiteStr ftIntSite,   //	Full proposed site
                                        FtSiteStrNulls ftIntSiteNulls,
                                        TLink[] pLinks,
                                        int nLinkNum,       //	This is the link we are processing.
                                        string envName,
                                        string proName,
                                        //int isMDB,
                                        bool isMDB,
                                        string selCommand,  //	Selection from the environment.
                                        ref int numCases)
        {
            //...Log2.v("\nTtBuild.Vic2DimTable(): Entry.");

            string intPrintMsg = "";
            string vicPrintMsg = "";
            int numProAnte = 0;
            int numEnvAnte = 0;
            int rc;
            double dist;
            double azimIV;
            double azimVI;

            TtSite proSiteStruct;
            TtSite envSiteStruct;

            SQLLEN[] proSiteNulls;
            SQLLEN[] envSiteNulls;

            FtAnte pIntAnte;

            string vicCall1;
            string vicCall2;
            string sqlCommand;
            string vicBndCode;

            int nInd;
            int nRet;
            int nLink;

            string cCallFound;
            string cRememberString;

            FtSiteStr pIntSite2;
            FtSiteStrNulls pIntSite2Nulls;
            FtSiteStr pVicSite = null;
            FtSiteStrNulls pVicSiteNulls = null;
            FtSiteStr vicSite2;
            FtSiteStrNulls vicSite2Nulls;

            TLink[] pVicLinks;
            int nNumVicLinks;

            int nAntNum = FtUtils.FtFirstNonDelAnte(ftIntSite, pLinks, nLinkNum);       //	First non deleted antenna of the link.

            //...Log2.v("\nTtBuild.Vic2DimTable(): A");

            pIntAnte = ftIntSite.stAntsPtr[nAntNum];

            sqlCommand = selCommand;

            //	Get the other end of the interfering antenna
            //AH: avoid warning C4706: assignment within conditional expression
            nRet = TpMdbPdfGet.TtFullSiteGet(pIntAnte.call2, proName, false, out pIntSite2, out pIntSite2Nulls);
            if (nRet != Constant.SUCCESS)
            {
                string str = String.Format("Could not get other end for proposed antenna: {0} to {1} band {2}", pIntAnte.call1, pIntAnte.call2, pIntAnte.bndcde);
                Log2.e("\nTtBuildSH.Vic2DimTable(): ERROR: " + str);
                GenUtil.SetErr("vic2DimTable: " + str);
                return Constant.FAILURE;
            }

            //...Log2.v("\nTtBuild.Vic2DimTable(): B");

            if (tpParmStruct.tsorbout.Equals("Y"))
            {
                //...Log2.v("\nTtBuild.Vic2DimTable(): C");

                //	Run the tsorb calcs between the site and its link.
                if (!GenUtil.Remember(RememberString(out cRememberString, pIntAnte.call1, pIntAnte.call2)) &&
                    !GenUtil.Remember(RememberString(out cRememberString, pIntAnte.call2, pIntAnte.call1)))
                {
                    //...Log2.v("\nTtBuild.Vic2DimTable(): D");
                    //	This pair hasn't been seen yet.
                    //...Log2.v("\ngamma");
                    nRet = TtCalcs.TtTsorbCalcs(tpParmStruct, pIntAnte.offazm, pIntAnte.aht, pIntAnte.call2,
                        pIntAnte.bndcde, pIntAnte.tazmth, pIntAnte.telvtn, ftIntSite.stSite);
                }
            }

            cCallFound = "";

            while (true)
            {
                //...Log2.v("\nTtBuild.Vic2DimTable(): E");

                //	Go through the victim sites as designated by the input where clause.
                nInd = TpMdbPdfGet.TtEnumSite(sqlCommand, isMDB ? null : envName, ref cCallFound);

                if (nInd != 0)
                {
                    //	That's all there are.
                    //...Log2.v("\nTtBuild.Vic2DimTable(): F");
                    break;
                }

                //...Log2.v("\nTtBuild.Vic2DimTable(): G");

                //	Get the victim full site.
                nInd = TpMdbPdfGet.TtFullSiteGet(cCallFound, envName, isMDB, out pVicSite, out pVicSiteNulls);
                if (nInd != 0)
                {
                    //...Log2.v("\nTtBuild.Vic2DimTable(): H");
                    // Call sign not there.  This is an error.
                    string str = String.Format("Victim call sign {0} not found.", cCallFound);
                    Log2.e("\nTtBuildSH.Vic2DimTable(): ERROR: " + str);
                    GenUtil.SetErr("vic2DimTable01: " + str);
                    break;
                }

                //...Log2.v("\nTtBuild.Vic2DimTable(): I: Victim = " + pVicSite.stSite.call1);

                if (IntVicSiteCull(tpParmStruct, ftIntSite.stSite, pVicSite.stSite.oper,
                    pVicSite.stSite.latit / 100.0, pVicSite.stSite.longit / 100.0,
                    pVicSite.stSite.prov, out dist, out azimIV, out azimVI) != Constant.SUCCESS)
                {
                    //...Log2.v("\nTtBuild.Vic2DimTable(): J");
                    continue;
                }

                //...Log2.v("\nTtBuild.Vic2DimTable(): K");

                //	Site passes basic geometry cull, copy in the site call sign
                vicCall1 = pVicSite.stSite.call1;

                //	Get the links in the victim site.
                nNumVicLinks = FtUtils.FtMakeLinks(pVicSite, out pVicLinks);
                if (nNumVicLinks < 0)
                {
                    //...Log2.v("\nTtBuild.Vic2DimTable(): L");
                    //	Problem making the victims links.
                    string str = String.Format("vic2DimTable: Could not make the victims links for {0} ({1})", pVicSite.stSite.call1, pVicSite.stSite.name);
                    Log2.e("\nTtBuildSH.Vic2DimTable(): ERROR: " + str);
                    GenUtil.SetErr(str);
                    continue;
                }

                //...Log2.v("\nTtBuild.Vic2DimTable(): M: nNumVicLinks = " + nNumVicLinks);

                //	Go through the links in the victim site
                for (nLink = 0; nLink < nNumVicLinks; nLink++)
                {
                    //...Log2.v("\nTtBuild.Vic2DimTable(): N");

                    //	Check that the proposed and env links are in the same band.
                    if (!Suutils.SuIsBandAdjacent(pLinks[nLinkNum].bndcde, pVicLinks[nLink].bndcde))
                    {
                        string str = String.Format("\npLinks[nLinkNum].bndcde = {0}\npVicLink[nLink].bndcde = {1}", pLinks[nLinkNum].bndcde, pVicLinks[nLink].bndcde);
                        //...Log2.v("\nTtBuild.Vic2DimTable(): O: " + str);
                        //	Bands for these antennas aren't adjacent.
                        continue;
                    }

                    //...Log2.v("\nTtBuild.Vic2DimTable(): P");

                    vicCall2 = pVicLinks[nLink].call2;
                    vicBndCode = pVicLinks[nLink].bndcde;
                    if (SitePairsCull(tpParmStruct.envtype, ftIntSite.stSite.call1,
                        ftIntSite.stAntsPtr[nAntNum].call2, vicCall1, vicCall2) != Constant.SUCCESS)
                    {
                        //...Log2.v("\nTtBuild.Vic2DimTable(): Q");
                        continue;
                    }

                    //...Log2.v("\nTtBuild.Vic2DimTable(): R");

                    /*
                     * Read environment site's remote data from the	env. file.
                     * A Constant.FAILURE from this function means only that the data was not
                     * found and the remainder of the for loop should be skipped.
                     */
                    rc = SetRemData(vicCall2, envName, isMDB, out vicSite2, out vicSite2Nulls);
                    if (rc != 0)
                    {
                        if (rc == Constant.FAILURE)
                        {
                            //...Log2.v("\nTtBuild.Vic2DimTable(): S");
                            continue;
                        }
                        else
                        {
                            //...Log2.v("\nTtBuild.Vic2DimTable(): T");
                            string str = String.Format("***Error Remote error for {0}, {1}\n", vicCall2, envName);
                            Log2.e("\nTtBuildSH.Vic2DimTable(): ERROR: " + str);
                            TpRunTsip.mTW_ERR.Write("\n" + str);
                            return (rc);
                        }
                    }

                    //...Log2.v("\nTtBuild.Vic2DimTable(): U");

                    /* 	set the values of the 2 TS Site SH records from the	pro. and env. files,  setBothSites() - calls
                    * 	ttSiteCalcs() to finish the population of the	TS Site SH records. */
                    SetBothSites(tpParmStruct.envtype, ftIntSite, ftIntSiteNulls, pLinks[nLinkNum],
                        pVicSite, pVicSiteNulls, pVicLinks[nLink], dist, azimIV, azimVI,
                        out proSiteStruct, out proSiteNulls, out envSiteStruct, out envSiteNulls, pIntSite2, vicSite2);

                    //...Log2.v("\nTtBuild.Vic2DimTable(): V");

                    /*
                     * Find distance cull: here we determine whether the site is within the coordination distance
                     * or within the Keyhole area. If it is neither, no further processing is done for this site.
                     */
                    //	Check distance with azimuth for keyhole cull.
                    if (dist > tpParmStruct.coordist)
                    {
                        if (!(TpKeyhole.WithinRange(proSiteStruct.intoffax, 5.0) ||
                            TpKeyhole.WithinRange(proSiteStruct.vicoffax, 5.0)))
                        {
                            //...Log2.v("\nTtBuild.Vic2DimTable(): W");
                            continue;
                        }
                        else
                        {
                            if (dist > 2.0 * tpParmStruct.coordist)
                            {
                                //...Log2.v("\nTtBuild.Vic2DimTable(): X");
                                //	Even in the keyhole, the distance must be less than twice coord.
                                continue;
                            }
                        }
                    }

                    //...Log2.v("\nTtBuild.Vic2DimTable(): Y, " + pVicSite.stSite.call1);

                    /* from SH SITE create SH ANTENNA */
                    //if ((rc = CreateAnteSHTable_NATIVE(ref tpParmStruct, ref proSiteStruct,
                    if ((rc = CreateAnteSHTable(ref tpParmStruct, ref proSiteStruct,
                        ref proSiteNulls, ref envSiteStruct,
                        ref envSiteNulls, proName, envName,
                        ref ftIntSite, ref ftIntSiteNulls, ref pLinks[nLinkNum],
                        ref pVicSite, ref pVicSiteNulls, ref pVicLinks[nLink],
                        vicCall1, vicCall2, vicBndCode,
                        out numProAnte, out numEnvAnte, ref numCases))
                        != Constant.SUCCESS)
                    {
                        string str = String.Format("***Error Could not create Ante Table for:-\nVic: {0} into {1} band {2}\n", vicCall1, vicCall2, vicBndCode);
                        Log2.e("\nTtBuildSH.Vic2DimTable(): ERROR: " + str);
                        TpRunTsip.mTW_ERR.Write("\n" + str);
                        return (rc);
                    }

                    //...Log2.v("\nTtBuild.Vic2DimTable(): Z");

                    //	Add the records to the tt tables 
                    if (numProAnte > 0)
                    {
                        //...Log2.v("\nTtBuild.Vic2DimTable(): AA");
                        //	Proposed site is interferer.
                        proSiteStruct.report = Constant.TRUE;

                        rc = TtDynSite.TtInsertSite(proSiteStruct, proSiteNulls);
                        if (rc != Constant.SUCCESS)
                        {
                            ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                            ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);

                            string str = String.Format("Error Inserting site returned {0}.\n", rc);
                            Log2.e("\nTtBuildSH.Vic2DimTable(): ERROR: " + str);
                            TpRunTsip.mTW_ERR.Write(str);
                            return (rc);
                        }
                    }

                    //...Log2.v("\nTtBuild.Vic2DimTable(): AB");
                    if (numEnvAnte > 0 && !tpParmStruct.envtype.Equals("INTRA"))
                    {
                        //...Log2.v("\nTtBuild.Vic2DimTable(): AC");
                        //	Environment is the interferer.
                        envSiteStruct.report = Constant.TRUE;

                        rc = TtDynSite.TtInsertSite(envSiteStruct, envSiteNulls);
                        if (rc != Constant.SUCCESS)
                        {
                            //...Log2.v("\nTtBuild.Vic2DimTable(): AD");
                            ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                            ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                            TpRunTsip.mTW_ERR.Write("Error Inserting site(1) returned {0}.\n", rc);
                            return (rc);
                        }
                    }

                } /* end for (each proposed set) */
            } /* end while (endLoop) */

            //...Log2.v("\nTtBuild.Vic2DimTable(): Exit.");
            return (Constant.SUCCESS);

        } /* ----- end vic2DimTable ----- */


        /// <summary>
        /// Construct a string from two call signs to use as input to the 
        /// method GenUtil.Remember().  
        /// </summary>
        /// <param name="cOutString"> - output string = call1 + " " + call2</param>
        /// <param name="call1"> - callsign of 1st site.</param>
        /// <param name="call2"> - callsign of 2nd site.</param>
        /// <returns></returns>
        public static string RememberString(out string cOutString, string call1, string call2)
        {
            cOutString = call1;
            cOutString += " ";
            cOutString += call2;

            return cOutString;
        }

        /// <summary>
        /// This method culls environment sites based on ALL EXCEPT SELF, 
        /// Country, and distance (fine distance cull).  
        /// </summary>
        /// <param name="parmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="intSiteStruct"> - FtSite object for interfering site.</param>
        /// <param name="vicOper"> - victim's operator code.</param>
        /// <param name="vicLatit"> - victim's latitude.</param>
        /// <param name="vicLongit"> - victim's longitude.</param>
        /// <param name="vicProv"> - victim's Province or State.</param>
        /// <param name="distIV"> - calculated distance between victim and interferer.</param>
        /// <param name="azimIV"> - calculated bearing from interferer to victim.</param>
        /// <param name="azimVI"> - calculated bearing from victim to interferer.</param>
        /// <returns></returns>
        public static int IntVicSiteCull(TpParm parmStruct,
                                            FtSite intSiteStruct,
                                            string vicOper,
                                            double vicLatit,
                                            double vicLongit,
                                            string vicProv,
                                            out double distIV,
                                            out double azimIV,
                                            out double azimVI)
        {
            // 'out' requirements.
            distIV = 0.0;
            azimIV = 0.0;
            azimVI = 0.0;

            double intLatit;
            double intLongit;
            string vicCountry;

            /* identical operator codes
             * if ((proposed oper is same as environment oper)
             * and (selsites is "ALL EXCEPT SELF")) */
            if ((intSiteStruct.oper.Equals(vicOper)) &&
                (parmStruct.selsites.Equals("ALL EXCEPT SELF")))
            {
                return (Constant.FAILURE);
            }

            /* culling distance (fine distance cull) -
             * calculate the distance between the proposed site and environment
             * site */
            intLatit = intSiteStruct.latit / 100.00;
            intLongit = intSiteStruct.longit / 100.00;

            AxSub2.AxDistan(intLatit, vicLatit, intLongit, vicLongit, out distIV, out azimIV, out azimVI);

            /* if (the distance is greater than the coordination dist) */
            if (distIV > (2 * parmStruct.coordist))
            {
                return (Constant.FAILURE);
            }

            /* within specified country -
             * if ((the user specified a country) and (country is not "ALL")) */
            if ((!parmStruct.country.Equals("")) &&
                (!parmStruct.country.Equals("ALL")))
            {
                /* check that the environment is in the specified country */
                GenUtil.UtGetCountry(vicProv, out vicCountry);
                if (!vicCountry.Equals(parmStruct.country))
                {
                    return (Constant.FAILURE);
                }
            }

            return (Constant.SUCCESS);

        } /* ----- end intVicSiteCull ----- */

        /// <summary>
        /// This method selects sites to test based on the site cull.
        /// </summary>
        /// <remarks>
        /// The site pairs cull selects records that:
        /// <list type="bullet">
        /// <item>are NOT co-located site pairs (x,y)-(A,B) where x=A & y=B; AND</item>
        /// <item>are NOT cross-located site pairs (x,y)-(A,B) where x=B & y=A.</item>
        /// </list>list> 
        /// </remarks>
        /// <param name="envType"> - "MDB_TS" or "INTRA".</param>
        /// <param name="intCall1"> - interferer, 1st callsign.</param>
        /// <param name="intCall2"> - interferer, 2nd callsign.</param>
        /// <param name="vicCall1"> - victim, 1st callsign.</param>
        /// <param name="vicCall2"> - victim, 2nd callsign.</param>
        /// <returns></returns>
        public static int SitePairsCull(string envType,
                                        string intCall1,
                                        string intCall2,
                                        string vicCall1,
                                        string vicCall2)
        {
            /* co-located site pairs (call1,call2)-(call1,call2) -
             * if ((proposed call1 is same as environment call1)
             * and (proposed call2 is same as environment call2) */
            if ((intCall1.Equals(vicCall1)) &&
                (intCall2.Equals(vicCall2)))
            {
                return (Constant.FAILURE);
            }

            /* cross-located site pairs (call1,call2)-(call2,call1) -
             * if ((proposed call1 is same as environment call2)
             * and (proposed call2 is same as environment call1) */
            if ((intCall1.Equals(vicCall2)) &&
                (intCall2.Equals(vicCall1)))
            {
                return (Constant.FAILURE);
            }

            /* for passive reflectors, PRs (their call signs start w/ %),
             * ignore cases where call1 is a PR and
             * 	 (call1,call2)-(call1,call3)
             * 	 (call1,call2)-(call3,call1) -
             * if ((proposed call1 starts with '%')
             * and ((proposed call1 is same as environment call1)
             * or (proposed call1 is same as environment call2))) */
            if (Strings.FirstCharIs(intCall1, '%') &&
                    ((intCall1.Equals(vicCall1)) || (intCall1.Equals(vicCall2))))
            {
                return (Constant.FAILURE);
            }

            /* PR - ignore cases where call2 is a PR and
             * 	 (call1,call2)-(call2,call3)
             * 	 (call1,call2)-(call3,call2) -
             * if ((proposed call2 starts with '%')
             * and ((proposed call2 is same as environment call1)
             * or (proposed call2 is same as environment call2))) */
            if (Strings.FirstCharIs(intCall2, '%') &&
                ((intCall2.Equals(vicCall1)) || (intCall2.Equals(vicCall2))))
            {
                return (Constant.FAILURE);
            }

            return (Constant.SUCCESS);

        } /* ----- end sitePairsCull ----- */

        /// <summary>
        /// This method finds and stores necessary site data about 
        /// the remote site of the input site.  
        /// </summary>
        /// <param name="callTwo"> - callsign of remote site.</param>
        /// <param name="siteName"> - name of site table to be searched.</param>
        /// <param name="isMDB"> - boolean; true if the tables are in the main DB table set, otherwise false.</param>
        /// <param name="pSite2"> - FtSiteStr object for remote site.</param>
        /// <param name="pSiteNulls"> - ODBC nullInds for pSite2.</param>
        /// <returns></returns>
        public static int SetRemData(string callTwo, string siteName, bool isMDB,
                                        out FtSiteStr pSite2, out FtSiteStrNulls pSiteNulls)
        {
            int rc = Constant.FAILURE;

            /* get interferer's remote name, lat & long */
            if ((rc = TpMdbPdfGet.TtFullSiteGet(callTwo, siteName, isMDB, out pSite2, out pSiteNulls)) != 0)
            {
                if (rc != 0)
                {
                    Log2.e("\nTtBuildSH.SetRemData(): ERROR: call to TtFullSiteGet() failed, rc = " + rc);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", callTwo);
                }
                return (rc);
            }

            return (Constant.SUCCESS);
        } /*---- end setRemData ----*/

        /// <summary>
        /// This method sets the values of the TS Site SH Table from the 
        /// proposed and environment files; it also calls the method TtSiteCalcs()
        /// to complete the population of the TS Site record.  
        /// </summary>
        /// <param name="envtype"> - "MDB_TS" or "INTRA".</param>
        /// <param name="pIntSite"> - FtSiteStr object describing the interfering site.</param>
        /// <param name="pIntSiteNulls"> - ODBC nullInds associated with pIntSite.</param>
        /// <param name="pIntLink"> - TLink object associated with the interfering site.</param>
        /// <param name="pVicSite"> - FtSiteStr object describing the victim site.</param>
        /// <param name="pVicSiteNulls"> - ODBC nullInds associated with pVicSite.</param>
        /// <param name="pVicLink"> - TLink object associated with the victim site.</param>
        /// <param name="dist"> - distance between interferer and victim sites.</param>
        /// <param name="azimIV"> - calculated bearing from interferer to victim.</param>
        /// <param name="azimVI"> - calculated bearing from victim to interferer.</param>
        /// <param name="proSite"> - TtSite object describing the proposed site.</param>
        /// <param name="proNulls"> - ODBC nullInds associated with proSite.</param>
        /// <param name="envSite"> - TtSite object describing an environment site.</param>
        /// <param name="envNulls"> - ODBC nullInds associated with envSite.</param>
        /// <param name="intRemote"> - FtSiteStr object describing the interferer's remote site.</param>
        /// <param name="vicRemote"> - FtSiteStr object describing the victim's remote site.</param>
        public static void SetBothSites(string envtype,
                                        FtSiteStr pIntSite,
                                        FtSiteStrNulls pIntSiteNulls,
                                        TLink pIntLink,
                                        FtSiteStr pVicSite,
                                        FtSiteStrNulls pVicSiteNulls,
                                        TLink pVicLink,
                                        double dist,
                                        double azimIV,
                                        double azimVI,
                                        out TtSite proSite,
                                        out SQLLEN[] proNulls,
                                        out TtSite envSite,
                                        out SQLLEN[] envNulls,
                                        FtSiteStr intRemote,
                                        FtSiteStr vicRemote)
        {
            // 'out' requirements.
            proSite = new TtSite();   /* TSIP site record structure */
            envSite = new TtSite();   /* TSIP site record structure */
            proNulls = NullHelper.CreateArrayOfNullInd(TtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            envNulls = NullHelper.CreateArrayOfNullInd(TtSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            FtSite intSite = pIntSite.stSite;
            FtSite vicSite = pVicSite.stSite;

            NullHelper.FillArray(ref proNulls, Constant.DB_NULL);

            proSite.interferer = "P";
            proSite.intcall1 = intSite.call1;
            proSite.intcall2 = pIntLink.call2;
            proSite.viccall1 = vicSite.call1;
            proSite.viccall2 = pVicLink.call2;

            proSite.intname1 = intSite.name;
            proSite.intoper = intSite.oper;
            proSite.intlatit = intSite.latit;
            proSite.intlongit = intSite.longit;
            proSite.intgrnd = (double)intSite.grnd;

            proSite.vicname1 = vicSite.name;
            proSite.vicoper = vicSite.oper;
            proSite.viclatit = vicSite.latit;
            proSite.viclongit = vicSite.longit;
            proSite.vicgrnd = (double)vicSite.grnd;
            proSite.report = Constant.FALSE;
            proSite.caseno = 0;
            proSite.subcases = 0;

            proSite.intname2 = intRemote.stSite.name;
            proSite.intoper2 = intRemote.stSite.oper;
            proSite.int1int2dist = pIntSite.stAntsPtr[pIntLink.aAnts[0]].dist; // Distance of first antenna in link.
            proSite.vicname2 = vicRemote.stSite.name;
            proSite.vicoper2 = vicRemote.stSite.oper;
            proSite.vic1vic2dist = pVicSite.stAntsPtr[pVicLink.aAnts[0]].dist;
            proSite.int1vic1dist = dist;

            /*	Store the azimuths for the geometry on the report - GJS - 2005.10.04 */
            proSite.intvicaz = azimIV;
            proSite.vicintaz = azimVI;

            proNulls[TtSite.INTERFERER] = Constant.DB_NOT_NULL;
            proNulls[TtSite.INTCALL1] = Constant.DB_NOT_NULL;
            proNulls[TtSite.INTCALL2] = Constant.DB_NOT_NULL;
            proNulls[TtSite.VICCALL1] = Constant.DB_NOT_NULL;
            proNulls[TtSite.VICCALL2] = Constant.DB_NOT_NULL;

            proNulls[TtSite.INTNAME1] = pIntSiteNulls.anSiteNull[FtSite.NAME];
            proNulls[TtSite.INTOPER] = pIntSiteNulls.anSiteNull[FtSite.OPER];
            proNulls[TtSite.INTLATIT] = pIntSiteNulls.anSiteNull[FtSite.LATIT];
            proNulls[TtSite.INTLONGIT] = pIntSiteNulls.anSiteNull[FtSite.LONGIT];
            proNulls[TtSite.INTGRND] = pIntSiteNulls.anSiteNull[FtSite.GRND];

            proNulls[TtSite.VICNAME1] = pVicSiteNulls.anSiteNull[FtSite.NAME];
            proNulls[TtSite.VICOPER] = pVicSiteNulls.anSiteNull[FtSite.OPER];
            proNulls[TtSite.VICLATIT] = pVicSiteNulls.anSiteNull[FtSite.LATIT];
            proNulls[TtSite.VICLONGIT] = pVicSiteNulls.anSiteNull[FtSite.LONGIT];
            proNulls[TtSite.VICGRND] = pVicSiteNulls.anSiteNull[FtSite.GRND];

            proNulls[TtSite.REPORT] = Constant.DB_NOT_NULL;
            proNulls[TtSite.CASENO] = Constant.DB_NOT_NULL;
            proNulls[TtSite.SUBCASES] = Constant.DB_NOT_NULL;

            proNulls[TtSite.INTNAME2] = Constant.DB_NOT_NULL;
            proNulls[TtSite.INTOPER2] = Constant.DB_NOT_NULL;
            proNulls[TtSite.INT1INT2DIST] = Constant.DB_NOT_NULL;
            proNulls[TtSite.VICNAME2] = Constant.DB_NOT_NULL;
            proNulls[TtSite.VICOPER2] = Constant.DB_NOT_NULL;
            proNulls[TtSite.VIC1VIC2DIST] = Constant.DB_NOT_NULL;
            proNulls[TtSite.INT1VIC1DIST] = Constant.DB_NOT_NULL;

            proNulls[TtSite.INTVICAZ] = Constant.DB_NOT_NULL;
            proNulls[TtSite.VICINTAZ] = Constant.DB_NOT_NULL;

            TtCalcs.TtSiteCalcs(ref proSite, ref proNulls, pIntSite.stSite.grnd, pVicSite.stSite.grnd,
                azimIV, azimVI, pIntSite.stAntsPtr[pIntLink.aAnts[0]].azmth,
                pVicSite.stAntsPtr[pVicLink.aAnts[0]].azmth);


            if (!envtype.Equals("INTRA"))
            {
                for (int i = 0; i < TtSite.NUM_COLUMNS; i++)
                {
                    envNulls[i] = proNulls[i];
                }

                envSite.interferer = "E";
                envSite.intcall1 = vicSite.call1;
                envSite.intcall2 = pVicLink.call2;
                envSite.viccall1 = intSite.call1;
                envSite.viccall2 = pIntLink.call2;

                envSite.intname1 = vicSite.name;
                envSite.intoper = vicSite.oper;
                envSite.intlatit = vicSite.latit;
                envSite.intlongit = vicSite.longit;
                envSite.intgrnd = (double)vicSite.grnd;

                envSite.vicname1 = intSite.name;
                envSite.vicoper = intSite.oper;
                envSite.viclatit = intSite.latit;
                envSite.viclongit = intSite.longit;
                envSite.vicgrnd = (double)intSite.grnd;

                /*	Store the azimuths for the geometry on the report - GJS - 2005.10.04 */
                envSite.intvicaz = azimVI;
                envSite.vicintaz = azimIV;
                envNulls[TtSite.INTVICAZ] = Constant.DB_NOT_NULL;
                envNulls[TtSite.VICINTAZ] = Constant.DB_NOT_NULL;

                envSite.report = Constant.FALSE;
                envSite.caseno = 0;
                envSite.subcases = 0;

                envSite.intname2 = vicRemote.stSite.name;
                envSite.intoper2 = vicRemote.stSite.oper;
                envSite.int1int2dist = pVicSite.stAntsPtr[pVicLink.aAnts[0]].dist;//
                envSite.vicname2 = intRemote.stSite.name;
                envSite.vicoper2 = intRemote.stSite.oper;
                envSite.vic1vic2dist = pIntSite.stAntsPtr[pIntLink.aAnts[0]].dist;
                envSite.int1vic1dist = dist;
                envNulls[TtSite.INTNAME2] = Constant.DB_NOT_NULL;
                envNulls[TtSite.INTOPER2] = Constant.DB_NOT_NULL;
                envNulls[TtSite.VICNAME2] = Constant.DB_NOT_NULL;
                envNulls[TtSite.VICOPER2] = Constant.DB_NOT_NULL;

                TtCalcs.TtSiteCalcs(ref envSite, ref envNulls, pVicSite.stSite.grnd, pIntSite.stSite.grnd,
                    azimVI, azimIV, pVicSite.stAntsPtr[pVicLink.aAnts[0]].azmth,
                    pIntSite.stAntsPtr[pIntLink.aAnts[0]].azmth);
            }

        } /*---- end setBothSites ----*/


        /// <summary>
        /// This method controls the culling and population of the TS Ante SH 
        /// Table records.  
        /// </summary>
        /// <param name="tpParmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="proSiteStruct"> - TtSite object describing the proposed site.</param>
        /// <param name="proSiteNulls"> - ODBC nullInds associated with proSiteStruct.</param>
        /// <param name="envSiteStruct"> - TtSite object describing an environment site.</param>
        /// <param name="envSiteNulls"> - ODBC nullInds associated with envSiteStruct.</param>
        /// <param name="proName"> - table name of proposed sites.</param>
        /// <param name="envName"> - table name of sites in the environment.</param>
        /// <param name="pProSite"> - FtSiteStr object describing the proposed site.</param>
        /// <param name="pProSiteNulls"> - ODBC nullInds associated with pProSite.</param>
        /// <param name="pProLink"> - TLink objected associated with proposed site.</param>
        /// <param name="pEnvSite"> - FtSiteStr object describing the environment site.</param>
        /// <param name="pEnvSiteNulls"> - ODBC nullInds associated with pEnvSite.</param>
        /// <param name="pEnvLink"> - TLink objected associated with environment site.</param>
        /// <param name="envCall1"> - callsign of environment site.</param>
        /// <param name="envCall2"> - callsign of environment remote site.</param>
        /// <param name="envBndCode"> - Band Code of environment site.</param>
        /// <param name="numProAnte"> - number of antennae processed for proposed site.</param>
        /// <param name="numEnvAnte"> - number of antennae processed for environment site.</param>
        /// <param name="numCases"> - cummulative count of number of interference cases considered.</param>
        /// <returns></returns>
        public static int CreateAnteSHTable(ref TpParm tpParmStruct,
                                            ref TtSite proSiteStruct,
                                            ref SQLLEN[] proSiteNulls,
                                            ref TtSite envSiteStruct,
                                            ref SQLLEN[] envSiteNulls,
                                            string proName,
                                            string envName,
                                            ref FtSiteStr pProSite,
                                            ref FtSiteStrNulls pProSiteNulls,
                                            ref TLink pProLink,
                                            ref FtSiteStr pEnvSite,
                                            ref FtSiteStrNulls pEnvSiteNulls,
                                            ref TLink pEnvLink,
                                            string envCall1,
                                            string envCall2,
                                            string envBndCode,
                                            out int numProAnte,
                                            out int numEnvAnte,
                                            ref int numCases)
        {
            //...Log2.v("\n\n===== TtBuildSH.CreateAnteSHTable(): Entry");

            // 'out' requirements.
            numProAnte = 0;
            numEnvAnte = 0;

            int numProChan = 0; ;
            int numEnvChan = 0; ;
            int rc;
            double proMBnd;
            double envMBnd = 0.0;
            double envPatLoss = 0.0;
            double proPatLoss;
            string intPrintMsg;
            string vicPrintMsg;
            string oldEnvBndCode;
            char[] pat360 = new char[2] { 'N', 'N' };

            TtAnte proAnteStruct = new TtAnte();   /* TSIP antenna record structure */
            SQLLEN[] proAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            TtAnte envAnteStruct = new TtAnte();   /* TSIP antenna record structure */
            SQLLEN[] envAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            string proAcode;
            string envAcode;
            string proAuse;
            string envAuse;
            float proGain;
            float envGain;

            float proht;
            float envht;

            short proAnum;
            short envAnum;

            string envOffax;
            double envTazmth;
            double envTelvtn;
            double envTgain;
            double envAz;
            double envEl;

            double envAntAz;
            double envAntEl;
            double envDiscAng;

            string proOffax;
            double proTazmth;
            double proTelvtn;
            double proTgain;
            double proAz;
            double proEl;

            double proAntAz;
            double proAntEl;
            double proDiscAng;

            double dDistIV;
            double dBearIV;
            double dBearVI;
            double dElevIV;
            double dElevVI;

            int nProInd;
            int nEnvInd;

            FtAnte pProAnte;
            SQLLEN[] pProAnteNulls;
            FtAnte pEnvAnte;
            SQLLEN[] pEnvAnteNulls;

            int nProAntNum;
            int nEnvAntNum;

            SuBand curBand;

            /* for each site permutation produced in vic2DimTable(), find all
             * possible bandCode/antennaNumber permutations of antenna information
             * (x,y,bandCode,antennaNumber)-(A,B,bandCode,antennaNumber) */

            oldEnvBndCode = "";

            proAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            envAnteNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            //	Go through proposed antennnas in link.
            for (nProInd = 0; nProInd < pProLink.nNumAnts; nProInd++)
            {
                //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): A");

                //	Assign the proposed variables...
                nProAntNum = pProLink.aAnts[nProInd];

                pProAnte = pProSite.stAntsPtr[nProAntNum]; // The link has the index of the ante.
                pProAnteNulls = pProSiteNulls.anAntsNullPtr[nProAntNum];

                proAnum = pProAnte.anum;
                proAcode = pProAnte.acode;
                proAuse = pProAnte.ause;
                proGain = pProAnte.tgain;
                proht = pProAnte.aht;
                proOffax = pProAnte.offazm;
                proTazmth = pProAnte.tazmth;
                proTelvtn = pProAnte.telvtn;
                proTgain = pProAnte.tgain;
                proAz = pProAnte.azmth;
                proEl = pProAnte.elvtn;

                intPrintMsg = String.Format("INTERFERER:  {0,10}   {1,10}   {2,5}   {3,4:D}",
                    pProAnte.call1, pProAnte.call2, pProAnte.bndcde, proAnum);

                for (nEnvInd = 0; nEnvInd < pEnvLink.nNumAnts; nEnvInd++)
                {
                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): B");

                    nEnvAntNum = pEnvLink.aAnts[nEnvInd];
                    pEnvAnte = pEnvSite.stAntsPtr[nEnvAntNum]; // The link has the index of the ante.
                    pEnvAnteNulls = pEnvSiteNulls.anAntsNullPtr[nEnvAntNum];

                    envAnum = pEnvAnte.anum;
                    envAcode = pEnvAnte.acode;
                    envAuse = pEnvAnte.ause;
                    envGain = pEnvAnte.tgain;
                    envht = pEnvAnte.aht;
                    envOffax = pEnvAnte.offazm;
                    envTazmth = pEnvAnte.tazmth;
                    envTelvtn = pEnvAnte.telvtn;
                    envTgain = pEnvAnte.tgain;
                    envAz = pEnvAnte.azmth;
                    envEl = pEnvAnte.elvtn;

                    vicPrintMsg = String.Format("VICTIM:      {0,10}   {1,10}   {2,5}   {3,4:D}",
                        pEnvAnte.call1, pEnvAnte.call2, pEnvAnte.bndcde, envAnum);

                    /*****************************************************************\
                    *
                    *		Offaxis angle processing.	GJS - 1108 - 2002.11
                    *
                    *		The true (T) azimuth and elevation represent the antenna as it
                    *		really is, if they are present.  Otherwise, the antenna
                    *		boresight is assumed to be right down the hop line.
                    *
                    *		Set the default values for the offaxis values from the input
                    *
                    \*****************************************************************/
                    string s = envOffax.Trim().ToUpper();
                    bool isYorT = s.Equals("Y") || s.Equals("T");

                    if ((pEnvAnteNulls[FtAnte.OFFAZM] != Constant.DB_NULL) && isYorT)
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): C");

                        /*	An offaxis angle was present */
                        if (pEnvAnteNulls[FtAnte.TAZMTH] != Constant.DB_NULL)
                        {
                            //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): D");

                            envAntAz = envTazmth;
                            if (pEnvAnteNulls[FtAnte.TELVTN] != Constant.DB_NULL)
                            {
                                envAntEl = envTelvtn;
                            }
                            else
                            {
                                envAntEl = 0.0; /*	Assume zero elevation if missing */
                            }
                        }
                        else
                        {
                            //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): E");

                            TpRunTsip.mTW_ERR.Write("\n{0}\n{1}\nOffaxis angle indicated for environment, but angle not found.\nHop azimuth is being used.\n",
                                    intPrintMsg, vicPrintMsg);

                            envAntAz = envAz;
                            envAntEl = envEl;
                        }
                        envOffax = "Y"; /*	Make sure it is not null. */
                    }
                    else
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): F");

                        /*	Just use the hop azimuth */
                        envAntAz = envAz;
                        envAntEl = envEl;
                        envOffax = "N";
                    }

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): G");

                    /*	Now the proposed angles */
                    s = proOffax.Trim().ToUpper();
                    isYorT = s.Equals("Y") || s.Equals("T");

                    if (pProAnteNulls[FtAnte.OFFAZM] != Constant.DB_NULL && isYorT)
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): H");

                        /*	An offaxis angle was present */
                        if (pProAnteNulls[FtAnte.TAZMTH] != Constant.DB_NULL)
                        {
                            proAntAz = proTazmth;
                            if (pProAnteNulls[FtAnte.TELVTN] != Constant.DB_NULL)
                            {
                                proAntEl = proTelvtn;
                            }
                            else
                            {
                                proAntEl = 0.0; /*	Zero if missing */
                            }
                        }
                        else
                        {
                            TpRunTsip.mTW_ERR.Write("\n{0}\n{1}\nOffaxis angle indicated for proposed, but angle not found.\nHop azimuth is being used.\n",
                                    intPrintMsg, vicPrintMsg);

                            proAntAz = proAz;
                            proAntEl = proEl;
                        }
                        proOffax = "Y";
                    }
                    else
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): I");

                        /*	Just use the hop azimuth */
                        proAntAz = proAz;
                        proAntEl = proEl;
                        proOffax = "N";
                    }

                    /*	Now we have the actual antenna azimuths and elevations for both
                    *		env and pro sites.  We need to calculate the vector between them
                    *		and use that to calculate the offaxis angles. */
                    AxSub2.AxDistan(proSiteStruct.intlatit / 100.0, proSiteStruct.viclatit / 100.0,
                        proSiteStruct.intlongit / 100.0, proSiteStruct.viclongit / 100.0,
                        out dDistIV, out dBearIV, out dBearVI);

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): J");

                    if (dDistIV > 0.001)
                    {
                        /*	Given the distance we can calculate the elevation */
                        AxSub3.AxElev((proSiteStruct.intgrnd + proht) / 1000.0,
                            (proSiteStruct.vicgrnd + envht) / 1000.0,
                            dDistIV, out dElevIV, out dElevVI);

                        /*	Now we can calculate the offaxis angles for these antennas. */
                        proDiscAng = GenUtil.IncAngle(proAntAz, proAntEl, dBearIV, dElevIV);
                        envDiscAng = GenUtil.IncAngle(envAntAz, envAntEl, dBearVI, dElevVI);
                    }
                    else
                    {
                        /*	The antennas are colocated, set the offaxis angles to 90.0 */
                        envDiscAng = 90.0;
                        proDiscAng = 90.0;
                    }

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): K");

                    /*	Set up the topology for the antennas.  This includes the off-axis
                    *		angles. First when Proposed is interferer, then environment. */
                    SetTopology(ref proAnteStruct, ref proAnteNulls,
                                proOffax, proAz, proAntAz, proDiscAng,
                                envOffax, envAz, envAntAz, envDiscAng);
                    SetTopology(ref envAnteStruct, ref envAnteNulls,
                                envOffax, envAz, envAntAz, envDiscAng,
                                proOffax, proAz, proAntAz, proDiscAng);

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): L");

                    /* set the values of the 2 TS Ante SH records from the pro. and env. files,
                     * setBothAntes() - calls ttAnteCalcs() to finish the population of the
                     * TS Ante SH records. */

                    rc = SetBothAntes(tpParmStruct,
                        pProSite, pProSiteNulls, nProAntNum,
                        pEnvSite, pEnvSiteNulls, nEnvAntNum, pProLink.bndcde, proAnum,
                        proAcode, proAuse, pProAnteNulls[FtAnte.ACODE], pProAnteNulls[FtAnte.AUSE],
                        proGain, pProAnteNulls[FtAnte.TGAIN], envBndCode, envAnum,
                        envAcode, envAuse, pEnvAnteNulls[FtAnte.ACODE], pProAnteNulls[FtAnte.AUSE],
                        envGain, pProAnteNulls[FtAnte.TGAIN], proSiteStruct,
                        proAnteStruct, proAnteNulls, envAnteStruct, envAnteNulls, intPrintMsg, vicPrintMsg,
                        ref pat360, proht, envht, proDiscAng,
                        envDiscAng, proEl, envEl);

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): M");

                    if (rc != 0)
                    {
                        return (rc);
                    }

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): N");

                    /* if (oldEnvBndCode is different from curr env. bndcode) */
                    if (!oldEnvBndCode.Equals(envBndCode))
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): O");

                        oldEnvBndCode = envBndCode;
                        /* get midband freq from SDB for environment */
                        if (Suutils.SuGetBand(envBndCode, out curBand) != Constant.SUCCESS)
                        {
                            Log2.e("\nTtBuildSH.CreateAnteSHTable(): ERROR: call to SuGetBand() failed.");
                            ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                            ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                            ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, envBndCode);
                            return (Constant.FAILURE);
                        }
                        envMBnd = curBand.bmidf;

                        /* if (env. file type is not "INTRA") */
                        if (!tpParmStruct.envtype.Equals("INTRA"))
                        {
                            /**********************************************************************\
                            *
                            *		Calculate the path distance, not the ground distance for use later
                            *		on GJS - 1179 - 2004.08.23
                            *
                            \**********************************************************************/
                            dDistIV = AxSub3.PathDist((proSiteStruct.intgrnd + proAnteStruct.intaht) / 1000.0,
                                (proSiteStruct.vicgrnd + proAnteStruct.vicaht) / 1000.0,
                                dDistIV);

                            /* calculate the env. path loss, envPatLoss */
                            GenUtil.FreeSpacePathLoss(dDistIV, envMBnd / 1000.0, out envPatLoss);
                        }
                    }

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): P");

                    /* get midband freq, proMBnd, from SDB for proposed */
                    if (Suutils.SuGetBand(pProLink.bndcde, out curBand) != Constant.SUCCESS)
                    {
                        Log2.e("\nTtBuildSH.CreateAnteSHTable(): ERROR: call to SuGetBand() failed.");
                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                        ErrMsg.UtPrintMessage(Error.INVALIDBANDCODE, pProLink.bndcde);
                        return (Constant.FAILURE);
                    }
                    proMBnd = curBand.bmidf;

                    /* calculate the pro. path loss, proPatLoss */
                    GenUtil.FreeSpacePathLoss(dDistIV, proMBnd / 1000.0, out proPatLoss);

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): Q");

                    /* from SH ANTE create SH CHANNEL */
                    if ((rc = CreateChanSHTable(tpParmStruct, proSiteStruct, proSiteNulls,
                    //if ((rc = CreateChanSHTable_NATIVE(tpParmStruct, proSiteStruct, proSiteNulls,
                    envSiteStruct, envSiteNulls, proAnteStruct,
                    proAnteNulls, envAnteStruct, envAnteNulls,
                    proName, envName, out numProChan,
                    out numEnvChan, ref numCases, envMBnd, proMBnd,
                    envPatLoss, proPatLoss))
                    != Constant.SUCCESS)
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): Q-1");
                        return (rc);
                    }

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): R");

                    /* add proAnte to SH Table */
                    if (numProChan > 0)
                    {
                        //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): R-1");
                        if (proAnteStruct.report == 1)
                        {
                            //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): R-2");
                            /* Set the case number in the antenna table */
                            proAnteStruct.caseno = proSiteStruct.caseno;
                            proAnteNulls[TtAnte.CASENO] = Constant.DB_NOT_NULL;

                            /* insert the proposed ante data into the ante SH Table */
                            rc = TtDynAnte.TtInsertAnte(proAnteStruct, proAnteNulls);
                            if (rc != Constant.SUCCESS)
                            {
                                Log2.e("\nTtBuildSH.CreateAnteSHTable(): ERROR: call to TtInsertAnte() failed.");
                                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                TpRunTsip.mTW_ERR.Write("Error: Inserting tt Antenna returned {0}.\n", rc);
                                return (rc);
                            }
                        }

                        numProAnte++;
                    }

                    //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): S");

                    if (numEnvChan > 0)
                    {
                        if (envAnteStruct.report == 1)
                        {
                            /*	Set the antenna case */
                            envAnteStruct.caseno = envSiteStruct.caseno;
                            envAnteNulls[TtAnte.CASENO] = Constant.DB_NOT_NULL;

                            /* insert the environment ante data into the ante * SH Table */
                            rc = TtDynAnte.TtInsertAnte(envAnteStruct, envAnteNulls);
                            if (rc != Constant.SUCCESS)
                            {
                                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                TpRunTsip.mTW_ERR.Write("Error: Inserting tt Antenna(2) returned {0}.\n", rc);
                                return (rc);
                            }
                        }

                        numEnvAnte++;
                    }
                }
            }

            //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): T");

            /*
             * Revision for 360 degree antenna pattern: if pat360[0] has been set to
             * 'Y' by setBothAntes, then add 360 to intoffax if it is less than 0.
             * Likewise, if pat360[1] has been set to 'Y' by setBothAntes, then add
             * 360 to vicoffax if it is less than 0.
             */

            //...Log2.v("\nSIGMA: pat360[0] = " + pat360[0]);
            //...Log2.v("\nSIGMA: intoffax = " + proSiteStruct.intoffax);
            if (pat360[0] == 'Y' && (proSiteStruct.intoffax < 0))
            {
                proSiteStruct.intoffax = 360 + proSiteStruct.intoffax;
                //...Log2.v("\nZULU: intoffax = " + proSiteStruct.intoffax);

            }

            //...Log2.v("\nSIGMA: pat360[1] = " + pat360[1]);
            //...Log2.v("\nSIGMA: vicoffax = " + proSiteStruct.vicoffax);
            if (pat360[1] == 'Y' && (proSiteStruct.vicoffax < 0))
            {
                proSiteStruct.vicoffax = 360 + proSiteStruct.vicoffax;
                //...Log2.v("\nZULU: vicoffax = " + proSiteStruct.vicoffax);

            }

            //...Log2.v("\nTtBuildSH.CreateAnteSHTable(): Exit: final");
            return (Constant.SUCCESS);

        } /* ----- end createAnteSHTable ----- */


        /// <summary>
        /// This method sets the azimuth and off-axis angles fields in a TtAnte object.  
        /// </summary>
        /// <param name="pttAnte"> - TtAnte object to be partially populated.</param>
        /// <param name="nttAnteNulls"> - ODBC nullInds associated with pttAnte.</param>
        /// <param name="cIntOffax"> - TBD.</param>
        /// <param name="dIntHopAz"> - TBD.</param>
        /// <param name="dIntAntAz"> - TBD.</param>
        /// <param name="dIntOffax"> - TBD.</param>
        /// <param name="cVicOffax"> - TBD.</param>
        /// <param name="dVicHopAz"> - TBD.</param>
        /// <param name="dVicAntAz"> - TBD.</param>
        /// <param name="dVicOffax"> - TBD.</param>
        public static void SetTopology(ref TtAnte pttAnte,
                                        ref SQLLEN[] nttAnteNulls,
                                        string cIntOffax,
                                        double dIntHopAz,
                                        double dIntAntAz,
                                        double dIntOffax,
                                        string cVicOffax,
                                        double dVicHopAz,
                                        double dVicAntAz,
                                        double dVicOffax)
        {
            pttAnte.intaoffax = cIntOffax;
            pttAnte.inthopaz = dIntHopAz;
            pttAnte.intantaz = dIntAntAz;
            pttAnte.intoffantax = dIntOffax;

            nttAnteNulls[TtAnte.INTAOFFAX] = Constant.DB_NOT_NULL;
            nttAnteNulls[TtAnte.INTHOPAZ] = Constant.DB_NOT_NULL;
            nttAnteNulls[TtAnte.INTANTAZ] = Constant.DB_NOT_NULL;
            nttAnteNulls[TtAnte.INTOFFANTAX] = Constant.DB_NOT_NULL;

            pttAnte.vicaoffax = cVicOffax;
            pttAnte.vichopaz = dVicHopAz;
            pttAnte.vicantaz = dVicAntAz;
            pttAnte.vicoffantax = dVicOffax;

            nttAnteNulls[TtAnte.VICAOFFAX] = Constant.DB_NOT_NULL;
            nttAnteNulls[TtAnte.VICHOPAZ] = Constant.DB_NOT_NULL;
            nttAnteNulls[TtAnte.VICANTAZ] = Constant.DB_NOT_NULL;
            nttAnteNulls[TtAnte.VICOFFANTAX] = Constant.DB_NOT_NULL;
        }

        /// <summary>
        /// This method sets the values of the TS Ante SH Table from the 
        /// proposed and environment files; it also calls the method TtAnteCalcs()
        /// to complete the population of the TS Site record.    
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="pProSite"> - FtSiteStr object describing the proposed site.</param>
        /// <param name="pProSiteNulls"> - ODBC nullInds associated with pProSite.</param>
        /// <param name="nProAntNum"> - proposed antennae number.</param>
        /// <param name="pEnvSite"> - FtSiteStr object describing the environment site.</param>
        /// <param name="pEnvSiteNulls"> - ODBC nullInds associated with pEnvSite.</param>
        /// <param name="nEnvAntNum"> - proposed antennae number</param>
        /// <param name="intBndcde"> - interferer's Band Code.</param>
        /// <param name="intAnum"> - interferer's antenna number.</param>
        /// <param name="intAcode"> - interferer's antenna code.</param>
        /// <param name="intAuse"> - TBD.</param>
        /// <param name="intNull"> - ODBC nullInd.</param>
        /// <param name="intNullUse"> - ODBC nullInd.</param>
        /// <param name="intGain"> - inteferer's gain.</param>
        /// <param name="intGainNull"> - ODBC nullInd associated with intGain.</param>
        /// <param name="vicBndcde"> - victim's Band Code.</param>
        /// <param name="vicAnum"> - victim's antenna number.</param>
        /// <param name="vicAcode"> - victim's antenna code.</param>
        /// <param name="vicAuse"> - TBD.</param>
        /// <param name="vicNull"> - ODBC nullInd.</param>
        /// <param name="vicNullUse"> - ODBC nullInd.</param>
        /// <param name="vicGain"> - victim's gain.</param>
        /// <param name="vicGainNull"> - ODBC nullInd associated with vicGain.</param>
        /// <param name="ttSite"> - TtSite object.</param>
        /// <param name="proAnte"> - TtAnte object describing the proposed site's antenna.</param>
        /// <param name="proNulls"> - ODBC nullInd associated with proAnte.</param>
        /// <param name="envAnte"> - TtAnte object describing the environment site's antenna.</param>
        /// <param name="envNulls"> - ODBC nullInd associated with envAnte.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <param name="pat360"> - TBD.</param>
        /// <param name="intaht"> - TBD.</param>
        /// <param name="vicaht"> - TBD.</param>
        /// <param name="proDiscAng"> - TBD.</param>
        /// <param name="envDiscAng"> - TBD.</param>
        /// <param name="proEl"> - TBD.</param>
        /// <param name="envEl"> - TBD.</param>
        /// <returns></returns>
        public static int SetBothAntes(
                                        TpParm tpParm,
                                        FtSiteStr pProSite,
                                        FtSiteStrNulls pProSiteNulls,
                                        int nProAntNum,
                                        FtSiteStr pEnvSite,
                                        FtSiteStrNulls pEnvSiteNulls,
                                        int nEnvAntNum,
                                        string intBndcde,
                                        short intAnum,
                                        string intAcode,
                                        string intAuse,
                                        SQLLEN intNull,
                                        SQLLEN intNullUse,
                                        float intGain,
                                        SQLLEN intGainNull,
                                        string vicBndcde,
                                        short vicAnum,
                                        string vicAcode,
                                        string vicAuse,
                                        SQLLEN vicNull,
                                        SQLLEN vicNullUse,
                                        float vicGain,
                                        SQLLEN vicGainNull,
                                        TtSite ttSite,
                                        TtAnte proAnte,
                                        SQLLEN[] proNulls,
                                        TtAnte envAnte,
                                        SQLLEN[] envNulls,
                                        string intPrintMsg,
                                        string vicPrintMsg,
                                        ref char[] pat360,
                                        float intaht,
                                        float vicaht,
                                        double proDiscAng,
                                        double envDiscAng,
                                        double proEl,
                                        double envEl)
        {
            int rc;
            string intaxref = "";
            string intamodel = "";
            string vicaxref = "";
            string vicamodel = "";
            float intagain = 0.0f;
            float vicagain = 0.0f;
            int axtype = 0;
            SQLLEN axtypenull = 0;
            SQLLEN intmodnull = 0;
            SQLLEN intgainnull = 0;
            SQLLEN vicaxnull = 0;
            SQLLEN vicmodnull = 0;
            SQLLEN vicgainnull = 0;
            SQLLEN intaxnull = 0;

            SuAntStr pAntStr;

            proAnte.interferer = "P";
            proAnte.intcall1 = ttSite.intcall1;
            proAnte.intcall2 = ttSite.intcall2;
            proAnte.viccall1 = ttSite.viccall1;
            proAnte.viccall2 = ttSite.viccall2;

            proAnte.intbndcde = intBndcde;
            proAnte.intanum = intAnum;
            proAnte.intacode = intAcode;
            proAnte.intause = intAuse;
            proAnte.intaht = intaht;

            proAnte.vicbndcde = vicBndcde;
            proAnte.vicanum = vicAnum;
            proAnte.vicacode = vicAcode;
            proAnte.vicause = vicAuse;
            proAnte.vicaht = vicaht;

            /*	Store the elevations from site to other end */
            proAnte.intelev = proEl;
            proAnte.vicelev = envEl;

            //	Pull in the interfering antenna 
            rc = Suutils.SuGetAnt(proAnte.intacode, out pAntStr);
            if (rc == 0)
            {
                /*  Set the same variables as above.  */
                intaxref = pAntStr.acAnt.axref;
                NullHelper.BlankNull(pAntStr.acAnt.axref, out intaxnull);

                intamodel = pAntStr.acAnt.amodel;
                NullHelper.BlankNull(pAntStr.acAnt.axref, out intmodnull);

                intagain = pAntStr.acAnt.again;
                NullHelper.ZeroNull(pAntStr.acAnt.again, out intgainnull);

                axtype = pAntStr.acAnt.axtype;
                NullHelper.ZeroNull(pAntStr.acAnt.axtype, out axtypenull);
            }
            else
            {
                TpRunTsip.mTW_ERR.Write("\n*ERROR* getting antenna code {0}\n", proAnte.intacode);
            }

            if (intaxnull != Constant.DB_NULL)
            {
                proAnte.intaxref = intaxref;
                proNulls[TtAnte.INTAXREF] = Constant.DB_NOT_NULL;
            }

            if (intmodnull != Constant.DB_NULL)
            {
                proAnte.intamodel = intamodel;
                proNulls[TtAnte.INTAMODEL] = Constant.DB_NOT_NULL;
            }

            if (intgainnull != Constant.DB_NULL)
            {
                proAnte.intgain = intagain;
                proNulls[TtAnte.INTGAIN] = Constant.DB_NOT_NULL;
            }

            if (axtypenull != Constant.DB_NULL)
            {
                if (axtype == 1 || axtype == 2)
                    pat360[0] = 'Y';
            }

            /*  Now the victim antenna.  */
            rc = Suutils.SuGetAnt(proAnte.vicacode, out pAntStr);

            if (rc == 0)
            {
                /*  Set the same variables as above.  */
                vicaxref = pAntStr.acAnt.axref;
                NullHelper.BlankNull(pAntStr.acAnt.axref, out vicaxnull);

                vicamodel = pAntStr.acAnt.amodel;
                NullHelper.BlankNull(pAntStr.acAnt.axref, out vicmodnull);

                vicagain = pAntStr.acAnt.again;
                NullHelper.ZeroNull(pAntStr.acAnt.again, out vicgainnull);

                axtype = pAntStr.acAnt.axtype;
                NullHelper.ZeroNull(pAntStr.acAnt.axtype, out axtypenull);
            }
            else
            {
                TpRunTsip.mTW_ERR.Write("\n*ERROR* getting antenna code {0}\n", proAnte.vicacode);
            }

            if (vicaxnull != Constant.DB_NULL)
            {
                proAnte.vicaxref = vicaxref;
                proNulls[TtAnte.VICAXREF] = Constant.DB_NOT_NULL;
            }

            if (vicmodnull != Constant.DB_NULL)
            {
                proAnte.vicamodel = vicamodel;
                proNulls[TtAnte.VICAMODEL] = Constant.DB_NOT_NULL;
            }

            if (vicgainnull != Constant.DB_NULL)
            {
                proAnte.vicgain = vicagain;
                proNulls[TtAnte.VICGAIN] = Constant.DB_NOT_NULL;
            }

            if (axtypenull != Constant.DB_NULL)
            {
                if (axtype == 1 || axtype == 2)
                {
                    pat360[1] = 'Y';
                }
            }


            proAnte.report = Constant.FALSE;
            proAnte.subcaseno = 0;

            proNulls[TtAnte.INTERFERER] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.INTCALL1] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.INTCALL2] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICCALL1] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICCALL2] = Constant.DB_NOT_NULL;

            proNulls[TtAnte.INTBNDCDE] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.INTANUM] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.INTACODE] = intNull;
            proNulls[TtAnte.INTAUSE] = intNullUse;

            proNulls[TtAnte.VICBNDCDE] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICANUM] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICACODE] = vicNull;
            proNulls[TtAnte.VICAUSE] = vicNullUse;
            proNulls[TtAnte.INTAHT] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICAHT] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.INTELEV] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICELEV] = Constant.DB_NOT_NULL;

            proNulls[TtAnte.REPORT] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.SUBCASENO] = Constant.DB_NOT_NULL;

            /*********************************************************\
            * 	Calculate the elevation angles between the two antennas.
            \*********************************************************/
            AxSub3.AxElev((ttSite.intgrnd + intaht) / 1000.0,
                (ttSite.vicgrnd + vicaht) / 1000.0,
                ttSite.int1vic1dist,
                out proAnte.intvicel, out proAnte.vicintel);

            proNulls[TtAnte.INTVICEL] = Constant.DB_NOT_NULL;
            proNulls[TtAnte.VICINTEL] = Constant.DB_NOT_NULL;

            rc = TtCalcs.TtAnteCalcs(tpParm, pProSite, pProSiteNulls, nProAntNum, pEnvSite,
                pEnvSiteNulls, nEnvAntNum, proAnte, ttSite, proNulls,
                intPrintMsg, vicPrintMsg, proDiscAng, envDiscAng);

            if (rc != Constant.SUCCESS)
            {
                return (rc);
            }

            if (!tpParm.envtype.Equals("INTRA"))
            {

                envAnte.interferer = "E";
                envAnte.intcall1 = ttSite.viccall1;
                envAnte.intcall2 = ttSite.viccall2;
                envAnte.viccall1 = ttSite.intcall1;
                envAnte.viccall2 = ttSite.intcall2;

                envAnte.intbndcde = vicBndcde;
                envAnte.intanum = vicAnum;
                envAnte.intacode = vicAcode;
                envAnte.intause = vicAuse;
                envAnte.intgain = vicGain;
                envAnte.intaht = vicaht;
                envAnte.intelev = envEl;
                envAnte.vicelev = proEl;

                envAnte.vicaxref = intaxref;
                envAnte.intaxref = vicaxref;
                envAnte.vicamodel = intamodel;
                envAnte.intamodel = vicamodel;

                envAnte.vicbndcde = intBndcde;
                envAnte.vicanum = intAnum;
                envAnte.vicacode = intAcode;
                envAnte.vicause = intAuse;
                envAnte.vicgain = intGain;
                envAnte.vicaht = intaht;

                /*	Calculate the elevation angles for the Environment pair */
                AxSub3.AxElev((ttSite.vicgrnd + vicaht) / 1000.0,
                    (ttSite.intgrnd + intaht) / 1000.0,
                    ttSite.int1vic1dist,
                    out envAnte.intvicel, out envAnte.vicintel);

                envNulls[TtAnte.INTVICEL] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICINTEL] = Constant.DB_NOT_NULL;

                envAnte.report = Constant.FALSE;
                envAnte.subcaseno = 0;

                envNulls[TtAnte.INTERFERER] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.INTCALL1] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.INTCALL2] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICCALL1] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICCALL2] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.INTBNDCDE] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.INTANUM] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.INTACODE] = vicNull;
                envNulls[TtAnte.INTAUSE] = vicNullUse;
                envNulls[TtAnte.INTGAIN] = vicGainNull;
                envNulls[TtAnte.VICBNDCDE] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICANUM] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICACODE] = intNull;
                envNulls[TtAnte.VICAUSE] = intNullUse;
                envNulls[TtAnte.INTAHT] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICAHT] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICGAIN] = intGainNull;
                envNulls[TtAnte.REPORT] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.SUBCASENO] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICAXREF] = intaxnull;
                envNulls[TtAnte.INTAXREF] = vicaxnull;
                envNulls[TtAnte.VICAMODEL] = intmodnull;
                envNulls[TtAnte.INTAMODEL] = vicmodnull;
                envNulls[TtAnte.INTELEV] = Constant.DB_NOT_NULL;
                envNulls[TtAnte.VICELEV] = Constant.DB_NOT_NULL;

                envAnte.adiscctxh = proAnte.adisccrxh;
                envAnte.adiscctxv = proAnte.adisccrxv;
                envAnte.adisccrxh = proAnte.adiscctxh;
                envAnte.adisccrxv = proAnte.adiscctxv;

                envAnte.adiscxtxh = proAnte.adiscxrxh;
                envAnte.adiscxtxv = proAnte.adiscxrxv;
                envAnte.adiscxrxh = proAnte.adiscxtxh;
                envAnte.adiscxrxv = proAnte.adiscxtxv;

                envNulls[TtAnte.ADISCCTXH] = proNulls[TtAnte.ADISCCRXH];
                envNulls[TtAnte.ADISCCTXV] = proNulls[TtAnte.ADISCCRXV];
                envNulls[TtAnte.ADISCCRXH] = proNulls[TtAnte.ADISCCTXH];
                envNulls[TtAnte.ADISCCRXV] = proNulls[TtAnte.ADISCCTXV];

                envNulls[TtAnte.ADISCXTXH] = proNulls[TtAnte.ADISCXRXH];
                envNulls[TtAnte.ADISCXTXV] = proNulls[TtAnte.ADISCXRXV];
                envNulls[TtAnte.ADISCXRXH] = proNulls[TtAnte.ADISCXTXH];
                envNulls[TtAnte.ADISCXRXV] = proNulls[TtAnte.ADISCXTXV];
            }
            return (Constant.SUCCESS);

        } /*---- end setBothAntes ----*/

        /// <summary>
        /// This method controls the culling and population 
        /// of the TS Chan SH Table records.  
        /// </summary>
        /// <param name="tpParmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="proSiteStruct"> - TtSite object describing the proposed site.</param>
        /// <param name="proSiteNulls"> - ODBC nullInds associated with pSiteStruct.</param>
        /// <param name="envSiteStruct"> - TtSite object describing the environment site.</param>
        /// <param name="envSiteNulls"> - ODBC nullInds associated with pSiteStruct.</param>
        /// <param name="proAnteStruct"> - TtAnte object describing the proposed site antenna.</param>
        /// <param name="proAnteNulls"> - ODBC nullInds associated with proAnteStruct.</param>
        /// <param name="envAnteStruct"> - TtAnte object describing the environment site antenna.</param></param>
        /// <param name="envAnteNulls"> - ODBC nullInds associated with envAnteStruct.</param>
        /// <param name="proName"> - table name of proposed site.</param>
        /// <param name="envName"> - table name of environment site.</param>
        /// <param name="numProChan"> - number of proposed site/antenna channels.</param>
        /// <param name="numEnvChan"> - number of environment site/antenna channels.</param>
        /// <param name="numCases"> - cummulative number of interference cases.</param>
        /// <param name="envMBnd"> - TBD.</param>
        /// <param name="proMBnd"> - TBD.</param>
        /// <param name="envPatLoss"> - TBD.</param>
        /// <param name="proPatLoss"> - TBD.</param>
        /// <returns></returns>
        public static int CreateChanSHTable(TpParm tpParmStruct,
                                            TtSite proSiteStruct,
                                            SQLLEN[] proSiteNulls,
                                            TtSite envSiteStruct,
                                            SQLLEN[] envSiteNulls,
                                            TtAnte proAnteStruct,
                                            SQLLEN[] proAnteNulls,
                                            TtAnte envAnteStruct,
                                            SQLLEN[] envAnteNulls,
                                            string proName,
                                            string envName,
                                            out int numProChan,
                                            out int numEnvChan,
                                            ref int numCases,
                                            double envMBnd,
                                            double proMBnd,
                                            double envPatLoss,
                                            double proPatLoss)
        {
            //...Log2.v(tpParmStruct.ToString());
            //...Log2.v(proSiteStruct.ToString());
            //...Log2.v(envSiteStruct.ToString());
            //...Log2.v(proAnteStruct.ToString());
            //...Log2.v("\nproName = " + proName);
            //...Log2.v("\nenvName = " + envName);
            //...Log2.v("\nnumCases = " + numCases);
            //...Log2.v("\nenvMBnd = " + envMBnd);
            //...Log2.v("\nproMBnd = " + proMBnd);
            //...Log2.v("\nenvPatLoss = " + envPatLoss);
            //...Log2.v("\nproPatLoss = " + proPatLoss);
            //...Log2.v("\n");

            //...Log2.v("\nTtBuildSH.CreateChanSHTable(): Entry");

            // 'out' requirements.
            numProChan = 0;
            numEnvChan = 0;

            int ftProChanHandle;    /* interferer pdf chan handle-dynamic */
            int ftEnvChanHandle;    /* victim pdf chan handle - dynamic */
            int rc1;
            int rc;
            int rc2;
            string proChanName = "";
            string envChanName = "";
            SQLLEN[] proChanNulls;
            SQLLEN[] envChanNulls;
            SQLLEN[] ftProChanNulls;   /* interferer pdf chan nulls*/
            SQLLEN[] ftEnvChanNulls;   /* victim pdf chan nulls */
            SQLLEN[] proPatNulls;
            SQLLEN[] envPatNulls;
            bool intPcSkip = false;
            bool vicPcSkip = false;
            double proAntDiscW = 0.0;
            double envAntDiscW = 0.0;
            string proSelection;    /* interferer selectn criteria-dynamic*/
            string envSelection; /* victim selection criteria - dynamic*/
            string intPrintMsg;
            string vicPrintMsg;
            TtChan proChanStruct;   /* TSIP chan recort structure */
            TtChan envChanStruct;   /* TSIP chan recort structure */
            FtChan ftProChanStruct;/* interferer pdf chan record struct*/
            FtChan ftEnvChanStruct;/* victim pdf chan record structure */

            // Instantiate data structure objects and nullInd arrays.
            envChanStruct = new TtChan();
            envChanNulls = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            proChanStruct = new TtChan();
            proChanNulls = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            proPatNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            envPatNulls = NullHelper.CreateArrayOfNullInd(TtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            /* calculate the worst antenna discrimination for the proposed ante,
             * this will be used in the calculations */
            rc = CalcADiscW(proAnteNulls, proAnteStruct, out proAntDiscW);

            if (rc != Constant.SUCCESS)
            {
                //...Log2.v("\ncreateChanSHTable(): Exit: A");
                string str = String.Format("createChanSHTable01: Could not calculate proposed antenna discrimination. ({0})", rc);
                Log2.e("\vTtBuildSH.CreateChanSHTable(): ERROR: call to CalcADiscW() failed: " + str);
                GenUtil.SetErr(str);
                return (Constant.FAILURE);
            }

            /* if (env. file type is not "INTRA") */
            if (!tpParmStruct.envtype.Equals("INTRA"))
            {

                /* calculate the worst antenna discrimination for the env. ante
                 * this will be used in the calculations */
                rc = CalcADiscW(envAnteNulls, envAnteStruct, out envAntDiscW);

                if (rc != Constant.SUCCESS)
                {
                    //...Log2.v("\ncreateChanSHTable(): Exit: B");
                    string str = String.Format("createChanSHTable02: Could not calculate environment antenna discrimination. ({0})", rc);
                    Log2.e("\vTtBuildSH.CreateChanSHTable(): ERROR: call to CalcADiscW() failed: " + str);
                    GenUtil.SetErr(str);
                    return (Constant.FAILURE);
                }
            }

            //...Log2.v("\ncreateChanSHTable(): 1");

            /* proposed selection */
            proSelection = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbtx1 = {3} or antnumbtx2 = {4} or antnumbrx1 = {5} or antnumbrx2 = {6} or antnumbrx3 = {7}) and cmd!='D'",
                proAnteStruct.intcall1, proAnteStruct.intcall2,
                proAnteStruct.intbndcde, proAnteStruct.intanum,
                proAnteStruct.intanum, proAnteStruct.intanum,
                proAnteStruct.intanum, proAnteStruct.intanum);

            //...Log2.v("\nTtBuildSH.CreateChanSHTable(): proSelection = " + proSelection);

            /* environment selection */
            envSelection = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbrx1 = {3} or antnumbrx2 = {4} or antnumbrx3 = {5} or antnumbtx1 = {6} or antnumbtx2 = {7})",
                proAnteStruct.viccall1, proAnteStruct.viccall2,
                proAnteStruct.vicbndcde, proAnteStruct.vicanum,
                proAnteStruct.vicanum, proAnteStruct.vicanum,
                proAnteStruct.vicanum, proAnteStruct.vicanum);

            //...Log2.v("\nTtBuildSH.CreateChanSHTable(): envSelection = " + envSelection);

            proPatNulls[TtAnte.ADISCCTXV] = proAnteNulls[TtAnte.ADISCCTXV];
            proPatNulls[TtAnte.ADISCXTXV] = proAnteNulls[TtAnte.ADISCXTXV];
            proPatNulls[TtAnte.ADISCCTXH] = proAnteNulls[TtAnte.ADISCCTXH];
            proPatNulls[TtAnte.ADISCXTXH] = proAnteNulls[TtAnte.ADISCXTXH];
            proPatNulls[TtAnte.ADISCCRXV] = proAnteNulls[TtAnte.ADISCCRXV];
            proPatNulls[TtAnte.ADISCXRXV] = proAnteNulls[TtAnte.ADISCXRXV];
            proPatNulls[TtAnte.ADISCCRXH] = proAnteNulls[TtAnte.ADISCCRXH];
            proPatNulls[TtAnte.ADISCXRXH] = proAnteNulls[TtAnte.ADISCXRXH];

            /* if (env. file type is not "INTRA") */
            if (!tpParmStruct.envtype.Equals("INTRA"))
            {
                envPatNulls[TtAnte.ADISCCTXV] = envAnteNulls[TtAnte.ADISCCTXV];
                envPatNulls[TtAnte.ADISCXTXV] = envAnteNulls[TtAnte.ADISCXTXV];
                envPatNulls[TtAnte.ADISCCTXH] = envAnteNulls[TtAnte.ADISCCTXH];
                envPatNulls[TtAnte.ADISCXTXH] = envAnteNulls[TtAnte.ADISCXTXH];
                envPatNulls[TtAnte.ADISCCRXV] = envAnteNulls[TtAnte.ADISCCRXV];
                envPatNulls[TtAnte.ADISCXRXV] = envAnteNulls[TtAnte.ADISCXRXV];
                envPatNulls[TtAnte.ADISCCRXH] = envAnteNulls[TtAnte.ADISCCRXH];
                envPatNulls[TtAnte.ADISCXRXH] = envAnteNulls[TtAnte.ADISCXRXH];
            }

            /* select from the proposed file all channels belonging to the current
             * proposed ante. */
            /*
               * TASK 497: In order to allow users to access dba PDF's, we must specify
               * that we are not opening this cursor for update, even though we have
               * no ordering cirteria. This is what "NU" means.
               */


            GenUtil.SetFullChanName(proName, tpParmStruct.protype, out proChanName);
            GenUtil.SetFullChanName(envName, tpParmStruct.envtype, out envChanName);

            //...Log2.v("\nTtBuildSH.CreateChanSHTable(): proChanName = " + proChanName);
            //...Log2.v("\nTtBuildSH.CreateChanSHTable(): envChanName = " + envChanName);

            ftProChanHandle = DynChannel.FtSelectChannel(proChanName, proSelection, "NU");

            if (ftProChanHandle < 0)
            {
                //...Log2.v("\ncreateChanSHTable(): Exit: C");
                Log2.e("\nTtBuildSH.CreateChanSHTable(): ERROR: call to FtSelectChannel() failed.");
                return (ftProChanHandle);
            }

            //...Log2.v("\ncreateChanSHTable(): 2");

            /* for (each channel selected) */
            while ((rc = DynChannel.FtFetchChannel(ftProChanHandle, out ftProChanStruct, out ftProChanNulls)) == Constant.SUCCESS)
            {
                //...Log2.v("\ncreateChanSHTable(): 3");

                /* select from the environment file all channels belonging to
                 * the current environment ante. */
                if ((ftEnvChanHandle = TpMdbPdfGet.TtTtSelectChannel(envChanName, envSelection, tpParmStruct.envtype)) < 0)
                {
                    //...Log2.v("\ncreateChanSHTable(): Exit: D");
                    Log2.e("\nTtBuildSH.CreateChanSHTable(): ERROR: call to TtTtSelectChannel() failed");
                    return (ftEnvChanHandle);
                }

                //...Log2.v("\ncreateChanSHTable(): 4");

                /* for (each channel selected) */
                while ((rc = TpMdbPdfGet.TtTtFetchChannel(ftEnvChanHandle, out ftEnvChanStruct,
                    out ftEnvChanNulls, tpParmStruct.envtype)) == Constant.SUCCESS)
                {
                    //...Log2.v("\ncreateChanSHTable(): 5");

                    /* proposed is interferer */
                    if (!intPcSkip)
                    {

                        /* if (pro. chan. antnumbtx1 is same as pro. ante. intanum
                         * or pro. chan. antnumbtx2 is same as pro. ante. intanum) */
                        if (ftProChanStruct.antnumbtx1 == proAnteStruct.intanum ||
                            ftProChanStruct.antnumbtx2 == proAnteStruct.intanum)
                        {

                            /* if (env. chan. antnumbrx1 is same as env. ante. intanum
                             * or env. chan. antnumbrx2 is same as env. ante. intanum
                             * or env. chan. antnumbrx3 is same as env. ante. intanum) */
                            if (ftEnvChanStruct.antnumbrx1 == proAnteStruct.vicanum ||
                                ftEnvChanStruct.antnumbrx2 == proAnteStruct.vicanum ||
                                ftEnvChanStruct.antnumbrx3 == proAnteStruct.vicanum)
                            {

                                intPrintMsg = String.Format("INTERFERER:  {0,10}   {1,10}   {2,5}   {3,4:D}   {4}",
                                    proAnteStruct.intcall1, proAnteStruct.intcall2,
                                    proAnteStruct.intbndcde, proAnteStruct.intanum,
                                    ftProChanStruct.chid);

                                vicPrintMsg = String.Format("VICTIM:      {0,10}   {1,10}   {2,5}   {3,4:D}   {4}",
                                    proAnteStruct.viccall1, proAnteStruct.viccall2,
                                    proAnteStruct.vicbndcde, proAnteStruct.vicanum,
                                    ftEnvChanStruct.chid);

                                /* check if the transmit status code is among the
                                 * status codes specified by the user */


                                rc2 = ChanCull(tpParmStruct, proAnteStruct.interferer, ftProChanStruct, ftEnvChanStruct);

                                if (rc2 == Constant.SUCCESS)
                                {
                                    //...Log2.v("\ncreateChanSHTable(): 6");

                                    numProChan++;

                                    /* 	Set the value of the proChanStruct to be the values of the
                                    *		proposed ft channel into the environment ft channel and runs
                                    *		the interference calculation.  The value of 'report' in the
                                    *		proChanStruct will be 1 if there was interference */
                                    rc1 = TtSetPCChan(tpParmStruct, ftProChanStruct,
                                        ftProChanNulls, ftEnvChanStruct,
                                        ftEnvChanNulls, proSiteStruct, proAnteStruct,
                                        proAnteNulls, out proChanStruct, out proChanNulls,
                                        proPatNulls, intPrintMsg, vicPrintMsg,
                                        proAntDiscW, proMBnd, envMBnd, proPatLoss);


                                    if (rc1 != Constant.SUCCESS)
                                    {
                                        if (rc1 == Constant.PC_SKIP)
                                        {
                                            intPcSkip = true;
                                        }
                                        else
                                        {
                                            TpMdbPdfGet.TtTtCloseChannel(ftEnvChanHandle, tpParmStruct.envtype);
                                            DynChannel.FtCloseChannel(ftProChanHandle);
                                            //...Log2.v("\ncreateChanSHTable(): Exit: E");
                                            return (rc1);
                                        }
                                    }

                                    //...Log2.v("\ncreateChanSHTable(): 7");

                                    /* if (analysis option is "BAND") */
                                    if (tpParmStruct.analopt.Equals("BAND"))
                                    {
                                        //...Log2.v("\ncreateChanSHTable(): 7-1");
                                        intPcSkip = true;
                                    }

                                    if (proChanStruct.report == Constant.TRUE)
                                    {
                                        //...Log2.v("\ncreateChanSHTable(): 7-2");
                                        /*	There was Proposed into Environment interference on these
                                        *		channels */
                                        if (proSiteStruct.report != Constant.TRUE)
                                        {
                                            //...Log2.v("\ncreateChanSHTable(): 7-3");
                                            /*	This site has not yet had any interference.  Now it has */
                                            proSiteStruct.report = Constant.TRUE;
                                            proSiteStruct.caseno = ++numCases;
                                            proSiteStruct.subcases = 1;

                                            proSiteNulls[TtSite.REPORT] = Constant.DB_NOT_NULL;
                                            proSiteNulls[TtSite.CASENO] = Constant.DB_NOT_NULL;
                                            proSiteNulls[TtSite.SUBCASES] = Constant.DB_NOT_NULL;
                                        }
                                        else if (proAnteStruct.report != Constant.TRUE)
                                        {
                                            //...Log2.v("\ncreateChanSHTable(): 7-4");
                                            /*	This antenna has had no interference yet */
                                            (proSiteStruct.subcases)++;
                                        }

                                        if (proAnteStruct.report != Constant.TRUE)
                                        {
                                            //...Log2.v("\ncreateChanSHTable(): 7-5");
                                            proAnteStruct.report = Constant.TRUE;
                                            proAnteStruct.caseno = proSiteStruct.caseno;
                                            proAnteStruct.subcaseno = proSiteStruct.subcases;
                                            proAnteNulls[TtAnte.REPORT] = Constant.DB_NOT_NULL;
                                            proAnteNulls[TtAnte.SUBCASENO] = Constant.DB_NOT_NULL;
                                        }

                                        //...Log2.v("\ncreateChanSHTable(): 7-6");

                                        /*	Store the case number in the channel as well. */
                                        proChanStruct.caseno = proSiteStruct.caseno;
                                        proChanNulls[TtChan.CASENO] = Constant.DB_NOT_NULL;

                                        /* insert the proposed chan data into
                                         * the chan SH Table */
                                        rc = TtDynChan.TtInsertChan(proChanStruct, proChanNulls);

                                        if (rc != Constant.SUCCESS)
                                        {
                                            Log2.e("\nTtBuildSH.CreateChanSHTable(): ERROR: call to TtInsertChan() failed.");
                                            ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                            ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                            TpRunTsip.mTW_ERR.Write("Error: Inserting tt Channel returns {0}.\n", rc);
                                            //...Log2.v("\ncreateChanSHTable(): Exit: F");
                                            return (rc);
                                        }

                                    }  //if (proChanStruct.report == Constant.TRUE)
                                }  //if (rc2 == Constant.SUCCESS)
                            }  //if (ftEnvChanStruct.antnumbrx1 == proAnteStruct.vicanum ||
                        }  //if (ftProChanStruct.antnumbtx1 == proAnteStruct.intanum ||
                    }  //if (intPcSkip)

                    //...Log2.v("\ncreateChanSHTable(): 8");

                    /* if (env. file type is not "INTRA") */
                    if (!tpParmStruct.envtype.Equals("INTRA"))
                    {
                        if (vicPcSkip == false)
                        {
                            /* if (pro. chan. antnumbrx1 is same as pro. ante. intanum
                             * or pro. chan. antnumbrx2 is same as pro. ante. intanum
                             * or pro. chan. antnumbrx3 is same as pro. ante. intanum) */
                            if (ftProChanStruct.antnumbrx1 == proAnteStruct.intanum ||
                                ftProChanStruct.antnumbrx2 == proAnteStruct.intanum ||
                                ftProChanStruct.antnumbrx3 == proAnteStruct.intanum)
                            {

                                /* if (env. chan. antnumbtx1 is same as env. ante. intanum
                                 * or env. chan. antnumbtx2 is same as env. ante. intanum) */
                                if (ftEnvChanStruct.antnumbtx1 == envAnteStruct.intanum ||
                                    ftEnvChanStruct.antnumbtx2 == envAnteStruct.intanum)
                                {
                                    /* environment is interferer */

                                    intPrintMsg = String.Format("INTERFERER:  {0,10}   {1,10}   {2,5}   {3,4:D}   {4}",
                                        envAnteStruct.intcall1,
                                        envAnteStruct.intcall2,
                                        envAnteStruct.intbndcde,
                                        envAnteStruct.intanum,
                                        ftEnvChanStruct.chid);

                                    vicPrintMsg = String.Format("VICTIM:      {0,10}   {1,10}   {2,5}   {3,4:D}   {4}",
                                        envAnteStruct.viccall1,
                                        envAnteStruct.viccall2,
                                        envAnteStruct.vicbndcde,
                                        envAnteStruct.vicanum,
                                        ftEnvChanStruct.chid);

                                    /* check if receive status code is among the
                                    * status codes specified by the user */
                                    rc2 = ChanCull(tpParmStruct, envAnteStruct.interferer,
                                        ftEnvChanStruct, ftProChanStruct);


                                    if (rc2 == Constant.SUCCESS)
                                    {
                                        //...Log2.v("\ncreateChanSHTable(): 9");

                                        numEnvChan++;

                                        /* 	set the values of the TS Chan SH record from the pro. and env.
                                        *		files, ttSetPCChan() - calls ttChanCalcs() to finish the
                                        *		population of the TS Chan SH records - if its return code is
                                        * 	PC_SKIP then vicPcSkip = true */
                                        rc1 = TtSetPCChan(tpParmStruct,
                                                            ftEnvChanStruct,
                                                            ftEnvChanNulls,
                                                            ftProChanStruct,
                                                            ftProChanNulls,
                                                            envSiteStruct,
                                                            envAnteStruct,
                                                            envAnteNulls,
                                                            out envChanStruct,
                                                            out envChanNulls,
                                                            envPatNulls,
                                                            intPrintMsg,
                                                            vicPrintMsg,
                                                            envAntDiscW,
                                                            envMBnd,
                                                            proMBnd,
                                                            envPatLoss);

                                        if (rc1 != Constant.SUCCESS)
                                        {
                                            //...Log2.v("\ncreateChanSHTable(): 10");

                                            if (rc1 == Constant.PC_SKIP)
                                            {
                                                vicPcSkip = true;
                                            }
                                            else
                                            {
                                                TpMdbPdfGet.TtTtCloseChannel(ftEnvChanHandle, tpParmStruct.envtype);
                                                DynChannel.FtCloseChannel(ftProChanHandle);

                                                //...Log2.v("\ncreateChanSHTable(): Exit: G");
                                                return (rc1);
                                            }
                                        }


                                        /* if (analysis option is "BAND") */
                                        if (tpParmStruct.analopt.Equals("BAND"))
                                        {
                                            vicPcSkip = true;
                                        }

                                        if (envChanStruct.report == Constant.TRUE)
                                        {
                                            if (envSiteStruct.report != Constant.TRUE)
                                            {
                                                envSiteStruct.report = Constant.TRUE;
                                                envSiteStruct.caseno = ++numCases;
                                                envSiteStruct.subcases = 1;
                                                envSiteNulls[TtSite.REPORT] = Constant.DB_NOT_NULL;
                                                envSiteNulls[TtSite.CASENO] = Constant.DB_NOT_NULL;
                                                envSiteNulls[TtSite.SUBCASES] = Constant.DB_NOT_NULL;
                                            }
                                            else if (envAnteStruct.report != Constant.TRUE)
                                            {
                                                (envSiteStruct.subcases)++;
                                            }
                                            if (envAnteStruct.report != Constant.TRUE)
                                            {
                                                envAnteStruct.report = Constant.TRUE;
                                                envAnteStruct.caseno = envSiteStruct.caseno;
                                                envAnteStruct.subcaseno = envSiteStruct.subcases;
                                                envAnteNulls[TtAnte.REPORT] = Constant.DB_NOT_NULL;
                                                envAnteNulls[TtAnte.SUBCASENO] = Constant.DB_NOT_NULL;
                                            }

                                            /*	Store the case number in the channel as well */
                                            envChanStruct.caseno = envSiteStruct.caseno;
                                            envChanNulls[TtChan.CASENO] = Constant.DB_NOT_NULL;

                                            /* insert the proposed chan
                                             * data into the chan SH Table*/
                                            rc = TtDynChan.TtInsertChan(envChanStruct, envChanNulls);
                                            //...Log2.v("\ncreateChanSHTable(): 11");

                                            if (rc != Constant.SUCCESS)
                                            {
                                                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                                TpRunTsip.mTW_ERR.Write("Error inserting tt Channel(3) returns {0}.\n", rc);
                                                //...Log2.v("\ncreateChanSHTable(): Exit: H");
                                                return (rc);
                                            }
                                        }  //if (envChanStruct.report == Constant.TRUE)
                                    }  //if (chanCull(tpParmStruct, envAnteStruct.interferer,
                                }  //if (ftEnvChanStruct.antnumbtx1 == envAnteStruct.intanum ||
                            }  //if (ftProChanStruct.antnumbrx1 == proAnteStruct.intanum ||
                        }  // if (vicPcSkip == false)
                    }  //if (!tpParmStruct.envtype.Equals("INTRA"))

                    if (intPcSkip && vicPcSkip)
                    {
                        break; /* end for (each channel) */
                    }
                }

                TpMdbPdfGet.TtTtCloseChannel(ftEnvChanHandle, tpParmStruct.envtype);
                if (intPcSkip && vicPcSkip)
                {
                    break; /* end for (each channel) */
                }

                //...Log2.v("\ncreateChanSHTable(): 12");

            } //while ((rc = DynChannel.FtFetchChannel    /* end for (each channel) */

            if (rc == Error.DYN_MS_SQL_SERVER_ERR)
            {
                //...Log2.v("\ncreateChanSHTable(): Exit: I");
                return (rc);
            }
            DynChannel.FtCloseChannel(ftProChanHandle);

            //...Log2.v("\nnumProChan = " + numProChan);
            //...Log2.v("\nnumEnvChan = " + numEnvChan);

            //...Log2.v("\ncreateChanSHTable(): Exit: final");
            //...Log2.v("\nTtBuildSH.CreateChanSHTable(): Exit: final");
            return (Constant.SUCCESS);

        } /* ----- end createChanSHTable ----- */

        /// <summary>
        /// This method sets the values of the TS Chan SH Table from the 
        /// proposed and environment files if the User specified Plan/Channel 
        /// calculations; it also calls the TtChanCalcs() method to complete the 
        /// population of the TS Chan record.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="intChanStruct"> - FtChan object describing the interferer's channel.</param>
        /// <param name="intNulls"> - ODBC nullInds associated with intChanStruct.</param>
        /// <param name="vicChanStruct"> - FtChan object describing the victim's channel.</param>
        /// <param name="vicNulls"> - ODBC nullInds associated with vicChanStruct.</param>
        /// <param name="ttSite"> - TtSite object.</param>
        /// <param name="ttAnte"> - TtAnte object.</param>
        /// <param name="ttAnteNulls"> - ODBC nullInds associated with ttAnte.</param>
        /// <param name="ttChan"> - TtChan object.</param>
        /// <param name="ttChanNulls"> - ODBC nullInds associated with ttChan.</param>
        /// <param name="patNulls"> - TBD.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <param name="intADiscW"> - TBD.</param>
        /// <param name="intMBnd"> - TBD.</param>
        /// <param name="vicMBnd"> - TBD.</param>
        /// <param name="intPatLoss"> - TBD.</param>
        /// <returns></returns>
        public static int TtSetPCChan(TpParm tpParm,
                                        FtChan intChanStruct,
                                        SQLLEN[] intNulls,
                                        FtChan vicChanStruct,
                                        SQLLEN[] vicNulls,
                                        TtSite ttSite,
                                        TtAnte ttAnte,
                                        SQLLEN[] ttAnteNulls,
                                        out TtChan ttChan,
                                        out SQLLEN[] ttChanNulls,
                                        SQLLEN[] patNulls,
                                        string intPrintMsg,
                                        string vicPrintMsg,
                                        double intADiscW,
                                        double intMBnd,
                                        double vicMBnd,
                                        double intPatLoss)
        {
            // 'out' requirements.
            ttChan = new TtChan();
            ttChanNulls = NullHelper.CreateArrayOfNullInd(TtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            //...Log2.v("\nTtBuildSH.TtSetPCChan(): Entry");

            double intAFSLtx = 0.0;
            SQLLEN nullIntAfslTx;

            if (ttAnte.intanum == intChanStruct.antnumbtx1)
            {
                ttChan.txant = 1;
            }
            else
            {
                ttChan.txant = 2;
            }
            ttChanNulls[TtChan.TXANT] = Constant.DB_NOT_NULL;

            ttChan.interferer = ttAnte.interferer;
            ttChan.intcall1 = ttAnte.intcall1;
            ttChan.intcall2 = ttAnte.intcall2;
            ttChan.intbndcde = ttAnte.intbndcde;
            ttChan.intanum = ttAnte.intanum;
            ttChan.viccall1 = ttAnte.viccall1;
            ttChan.viccall2 = ttAnte.viccall2;
            ttChan.vicbndcde = ttAnte.vicbndcde;
            ttChan.vicanum = ttAnte.vicanum;

            ttChan.intchid = intChanStruct.chid;
            ttChan.intpolar = intChanStruct.poltx;
            ttChan.inttraftx = intChanStruct.traftx;
            ttChan.inteqpttx = intChanStruct.eqpttx;
            ttChan.intstattx = intChanStruct.stattx;
            ttChan.intfreqtx = intChanStruct.freqtx;
            ttChan.intpwrtx = (double)intChanStruct.pwrtx;
            ttChanNulls[TtChan.INTPWRTX] = intNulls[FtChan.PWRTX];

            if (ttChan.intanum == intChanStruct.antnumbtx1)
            {
                ttChan.intafsltx = (double)intChanStruct.afsltx1;
                ttChanNulls[TtChan.INTAFSLTX] = intNulls[FtChan.AFSLTX1];
            }
            else
            {
                ttChan.intafsltx = (double)intChanStruct.afsltx2;
                ttChanNulls[TtChan.INTAFSLTX] = intNulls[FtChan.AFSLTX2];
            }

            ttChan.vicchid = vicChanStruct.chid;
            ttChan.vicpolar = vicChanStruct.polrx;
            ttChan.victrafrx = vicChanStruct.trafrx;
            ttChan.viceqptrx = vicChanStruct.eqptrx;
            ttChan.vicstatrx = vicChanStruct.statrx;
            ttChan.vicfreqrx = vicChanStruct.freqrx;

            if (ttChan.vicanum == vicChanStruct.antnumbrx1)
            {
                ttChan.vicpwrrx = (double)vicChanStruct.pwrrx1;
                ttChanNulls[TtChan.VICPWRRX] = vicNulls[FtChan.PWRRX1];
                ttChan.vicafslrx = (double)vicChanStruct.afslrx1;
                ttChanNulls[TtChan.VICAFSLRX] = vicNulls[FtChan.AFSLRX1];
                ttChan.rxant = 1;
            }
            else if (ttChan.vicanum == vicChanStruct.antnumbrx2)
            {
                ttChan.vicpwrrx = (double)vicChanStruct.pwrrx2;
                ttChanNulls[TtChan.VICPWRRX] = vicNulls[FtChan.PWRRX2];
                ttChan.vicafslrx = (double)vicChanStruct.afslrx2;
                ttChanNulls[TtChan.VICAFSLRX] = vicNulls[FtChan.AFSLRX2];
                ttChan.rxant = 2;
            }
            else
            {
                ttChan.vicpwrrx = (double)vicChanStruct.pwrrx3;
                ttChanNulls[TtChan.VICPWRRX] = vicNulls[FtChan.PWRRX3];
                ttChan.vicafslrx = (double)vicChanStruct.afslrx3;
                ttChanNulls[TtChan.VICAFSLRX] = vicNulls[FtChan.AFSLRX3];
                ttChan.rxant = 3;
            }
            ttChanNulls[TtChan.RXANT] = Constant.DB_NOT_NULL;

            ttChan.report = Constant.FALSE;
            ttChanNulls[TtChan.REPORT] = Constant.DB_NOT_NULL;

            ttChanNulls[TtChan.INTERFERER] = ttAnteNulls[TtAnte.INTERFERER];
            ttChanNulls[TtChan.INTCALL1] = ttAnteNulls[TtAnte.INTCALL1];
            ttChanNulls[TtChan.INTCALL2] = ttAnteNulls[TtAnte.INTCALL2];
            ttChanNulls[TtChan.INTBNDCDE] = ttAnteNulls[TtAnte.INTBNDCDE];
            ttChanNulls[TtChan.INTANUM] = ttAnteNulls[TtAnte.INTANUM];
            ttChanNulls[TtChan.VICCALL1] = ttAnteNulls[TtAnte.VICCALL1];
            ttChanNulls[TtChan.VICCALL2] = ttAnteNulls[TtAnte.VICCALL2];
            ttChanNulls[TtChan.VICBNDCDE] = ttAnteNulls[TtAnte.VICBNDCDE];
            ttChanNulls[TtChan.VICANUM] = ttAnteNulls[TtAnte.VICANUM];

            ttChanNulls[TtChan.INTCHID] = intNulls[FtChan.CHID];
            ttChanNulls[TtChan.INTPOLAR] = intNulls[FtChan.POLTX];
            ttChanNulls[TtChan.INTEQPTTX] = intNulls[FtChan.EQPTTX];
            ttChanNulls[TtChan.INTTRAFTX] = intNulls[FtChan.TRAFTX];
            ttChanNulls[TtChan.INTFREQTX] = intNulls[FtChan.FREQTX];
            ttChanNulls[TtChan.INTSTATTX] = intNulls[FtChan.STATTX];

            ttChanNulls[TtChan.VICCHID] = vicNulls[FtChan.CHID];
            ttChanNulls[TtChan.VICPOLAR] = vicNulls[FtChan.POLRX];
            ttChanNulls[TtChan.VICTRAFRX] = vicNulls[FtChan.TRAFRX];
            ttChanNulls[TtChan.VICEQPTRX] = vicNulls[FtChan.EQPTRX];
            ttChanNulls[TtChan.VICFREQRX] = vicNulls[FtChan.FREQRX];
            ttChanNulls[TtChan.VICSTATRX] = vicNulls[FtChan.STATRX];

            if (Strings.FirstCharIs(intChanStruct.call1, '%'))
            {
                intAFSLtx = 0.0;
                nullIntAfslTx = Constant.DB_NOT_NULL;
            }
            else
            {
                if (ttChan.txant == 1)
                {
                    intAFSLtx = intChanStruct.afsltx1;
                    nullIntAfslTx = intNulls[FtChan.AFSLTX1];
                }
                else
                {
                    intAFSLtx = intChanStruct.afsltx2;
                    nullIntAfslTx = intNulls[FtChan.AFSLTX2];
                }
            }

            //...Log2.v("\nttSetPCChan(): before call to ttChanCalcs()...");

            int rc = TtCalcs.TtChanCalcs(ttChan, ttAnte, ttAnteNulls, ttSite, tpParm, patNulls,
                                    ttChanNulls, intChanStruct.pwrtx, intAFSLtx,
                                    nullIntAfslTx, intPrintMsg, vicPrintMsg, intADiscW, intMBnd,
                                    vicMBnd, intPatLoss);

            bool test1 = ttChanNulls[TtChan.INTERFERER] == Constant.DB_NULL;
            bool test2 = ttChanNulls[TtChan.INTCALL1] == Constant.DB_NULL;
            bool test3 = ttChanNulls[TtChan.INTCALL2] == Constant.DB_NULL;

            if (test1 || test2 || test3)
            {
                //...Log2.v("\nTtBuildSH.TtSetPCChan(): THE TRAP IS SPRUNG");
                //...Log2.v("\nTtBuildSH.TtSetPCChan(): ttChan:\n" + ttChan.ToStringWN(ttChanNulls));
            }

            //...Log2.v("\nTtBuildSH.TtSetPCChan(): Exit");
            return rc;
        } /*---- end ttSetPCChan ----*/

        /// <summary>
        /// Calculates the worst Antenna Discrimination. 
        /// </summary>
        /// <remarks>
        /// The worst Antenna Discrimination is the minimum of the following values:
        /// <list type="bullet">
        /// <item>adiscctxh + adisccrxh;</item>
        /// <item>adiscctxv + adisccrxv;</item> 
        /// <item>adiscctxh + adiscxrxv;</item> 
        /// <item>adiscxtxh + adisccrxv;</item> 
        /// <item>adiscctxv + adiscxrxh;</item> 
        /// <item>adiscxtxv + adisccrxh.</item> 
        /// </list>
        /// </remarks>
        /// <param name="anteNulls"> - ODBC nullInds associated with anteStruct.</param>
        /// <param name="anteStruct"> - TtAnte object.</param>
        /// <param name="totADiscW"> - calculated worst Antenna Discrimination.</param>
        /// <returns></returns>
        public static int CalcADiscW(SQLLEN[] anteNulls, TtAnte anteStruct, out double totADiscW)
        {
            totADiscW = Constant.DFLT_ADISCW;

            if ((anteNulls[TtAnte.ADISCCTXH] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCCRXH] != Constant.DB_NULL))
            {
                totADiscW = anteStruct.adiscctxh + anteStruct.adisccrxh;
            }
            if ((anteNulls[TtAnte.ADISCCTXV] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCCRXV] != Constant.DB_NULL))
            {
                totADiscW =
                    (totADiscW < anteStruct.adiscctxv + anteStruct.adisccrxv) ?
                    totADiscW :
                    anteStruct.adiscctxv + anteStruct.adisccrxv;
            }
            if ((anteNulls[TtAnte.ADISCCTXH] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCXRXV] != Constant.DB_NULL))
            {
                totADiscW = (totADiscW < anteStruct.adiscctxh + anteStruct.adiscxrxv) ?
                    totADiscW :
                    anteStruct.adiscctxh + anteStruct.adiscxrxv;
            }
            if ((anteNulls[TtAnte.ADISCXTXH] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCCRXV] != Constant.DB_NULL))
            {
                totADiscW = (totADiscW < anteStruct.adiscxtxh + anteStruct.adisccrxv) ?
                    totADiscW :
                    anteStruct.adiscxtxh + anteStruct.adisccrxv;
            }
            if ((anteNulls[TtAnte.ADISCCTXV] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCXRXH] != Constant.DB_NULL))
            {
                totADiscW = (totADiscW < anteStruct.adiscctxv + anteStruct.adiscxrxh) ?
                    totADiscW :
                    anteStruct.adiscctxv + anteStruct.adiscxrxh;
            }
            if ((anteNulls[TtAnte.ADISCXTXV] != Constant.DB_NULL) &&
                (anteNulls[TtAnte.ADISCCRXH] != Constant.DB_NULL))
            {
                totADiscW = (totADiscW < anteStruct.adiscxtxv + anteStruct.adisccrxh) ?
                    totADiscW :
                    anteStruct.adiscxtxv + anteStruct.adisccrxh;
            }
            if (totADiscW == Constant.DFLT_ADISCW)
            {
                return (Error.WRST_ADISC);
            }

            return (Constant.SUCCESS);

        } /* ----- end calcADiscW ----- */

        /// <summary>
        /// This method selects channels to test for interference based 
        /// on the channel cull. 
        /// </summary>
        /// <remarks>
        /// The channel cull consists of selecting records i.a.w. the following criteria:
        /// <list type="bullet">
        /// <item>if the interferer is the environment, the transmit status code is among 
        /// the specified codes.</item>
        /// <item>if the victim is the environment, the receive status 
        /// code is among the specified codes.</item>
        /// </list>
        /// </remarks>
        /// <param name="parmStruct"> - TpParm object providing paramater data.</param>
        /// <param name="interferer"> - TBD.</param>
        /// <param name="intChanStruct"> - FtChan object describing the interferer's channel.</param>
        /// <param name="vicChanStruct"> - FtChan object describing the victim's channel.</param>
        /// <returns></returns>
        public static int ChanCull(TpParm parmStruct, string interferer, FtChan intChanStruct, FtChan vicChanStruct)
        {
            int rc;
            int getRc;
            string codesList;
            string aCode;

            //...Log2.v("\nchanCull: Entry");

            if (parmStruct.numchan == TEN)
            {
                return (Constant.SUCCESS);
            }

            rc = Constant.FAILURE;
            codesList = parmStruct.chancodes;

            getRc = GenUtil.UtGetInputString(codesList, out aCode, 1);

            while (rc != Constant.SUCCESS && getRc >= 0)
            {
                //...Log2.v("\nchanCull: A");

                /* if the environ is the interferer use it's transmit status */
                if (Strings.FirstCharIs(interferer, 'E'))
                {
                    //...Log2.v("\nchanCull: B");
                    if (aCode[0] == intChanStruct.stattx[0])
                    {
                        //...Log2.v("\nchanCull: C");
                        rc = Constant.SUCCESS;
                    }
                }
                else
                {
                    //...Log2.v("\nchanCull: D");
                    /* if environment is victim use receive (rx) status */
                    if (aCode[0] == vicChanStruct.statrx[0])
                    {
                        //...Log2.v("\nchanCull: E");
                        rc = Constant.SUCCESS;
                    }
                }
                getRc = GenUtil.UtGetInputString(null, out aCode, 1);
            }
            return (rc);
        } /*---- end chanCull */





    }
}

```
