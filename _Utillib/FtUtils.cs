using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;
using _NewLib;
using _Configuration;
using _Utillib;

namespace _Utillib
{

    using System.Collections;
    using System.Text.RegularExpressions;
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
    using SQLULEN = UInt64;
    using SQLUSMALLINT = UInt16;

    /// <summary>
    /// Provides general purpose 'utility' methods for accessing a PDF table.
    /// </summary>
    public class FtUtils
    {
#if PINVOKE
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftNextCall(StringBuilder cCall, StringBuilder cLastCall, StringBuilder cInTableShort);

        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftGetSite(StringBuilder sb1, [In, Out] ref IntPtr intPtr, [In] int i, StringBuilder sb2);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftGetSiteWN([In] string cCall, [In, Out] ref IntPtr pSitePtr, [In] int nDepth, [In] string cInTable, [In, Out] ref IntPtr pSiteNullPtr);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftPutSiteWN([In] FtSite pFtSite, [In] IntPtr pSiteNullPtr, [In] int nDepth, [In] string cInTable);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftEnumSite([In] string cSearch, [In] string cIntable, [In, Out] StringBuilder cCallFound);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftsetchanorder([In] string cOrderStr);
        [DllImport("utillib.dll", CharSet = CharSet.Ansi)]
        private static extern int ftMakeLinks([In] FtSiteStr pSite, [In, Out] IntPtr pLinksPtr);


        public static int FtSetChanOrder_NATIVE(string cOrderStr)
        {
            return ftsetchanorder(cOrderStr);
        }

        public static int FtEnumSite_NATIVE(string cSearch, string cIntable, out string cCallFound)
        {
            StringBuilder sb = new StringBuilder(Constant.CALLSIGN_SZ);

            int nRet = ftEnumSite(cSearch, cIntable, sb);

            cCallFound = sb.ToString();

            return nRet;
        }


        //public static int FtPutSiteWN_NATIVE(FtSiteStr_OLD pSiteStr_NEW, FtSiteNull_OLD pSiteNull_NEW, int nDepth, string cInTable)
        //{
        //...Log2.v("\n\nFtUtils.FtPutSiteWN_NATIVE: Entry:  Table = " + cInTable);

        //...Log2.v("\r\nFtUtils.FtPutSiteWN_NATIVE: pSiteStr_NEW = " + pSiteStr_NEW.ToStringFtSiteOnly());

        //    for (int i = 0; i < FtSite.NUM_COLUMNS; i++)
        //    {
        //...Log2.v("\r\nFtUtils.FtPutSiteWN_NATIVE: pSiteNull_NEW.anSiteNull[" + i + "] = " + pSiteNull_NEW.anSiteNull[i]);
        //    }

        //    if (nDepth != 1)
        //    {
        //...Log2.v("\r\nFtUtils.FtPutSiteWN_NATIVE: Not implemented yet for nDepth = " + nDepth);
        //        Application.Exit();
        //    }
        //    FtSiteStr pSiteStr = new FtSiteStr(FtSiteStr.Init.ALLOCATED);
        //    FtSiteNull pSiteNull = new FtSiteNull(FtSiteNull.Init.ALLOCATED);

        //    pSiteStr.stSite = pSiteStr_NEW.stSite;
        //    pSiteStr.nNumChans = pSiteStr_NEW.nNumChans;
        //    pSiteStr.nNumAnts = pSiteStr_NEW.nNumAnts;
        //    pSiteStr.nDepth = nDepth;
        //    pSiteStr.pAntennas = pSiteStr_NEW.pAntennas;

        //...Log2.v("\r\nFtUtils.FtPutSiteWN_NATIVE: pSiteStr.nDepth = " + pSiteStr.nDepth);

        //    if (pSiteStr_NEW.stAntsPtr == null)
        //    {
        //        Log2.e("\r\nFtUtils.FtPutSiteWN_NATIVE: ERROR: pSiteStr_NEW.stAntsPtr == null");
        //    }

        //    // Not required if nDepth = 1;
        //    //Marshal.StructureToPtr(pSiteStr_NEW.stAntsPtr, pSiteStr.stAntsPtr,false);
        //    //Marshal.StructureToPtr(pSiteStr_NEW.stChanPtr, pSiteStr.stChanPtr, false);

        //    pSiteNull.anSiteNull = Marshal.AllocHGlobal(FtSite.NUM_COLUMNS * sizeof(SQLLEN));
        //    Marshal.Copy(pSiteNull_NEW.anSiteNull, 0, pSiteNull.anSiteNull, FtSite.NUM_COLUMNS);

        //...Log2.v("\r\nFtUtils.FtPutSiteWN_NATIVE: pSiteStr = " + pSiteStr.ToStringFtSiteOnly());

        //    for (int i = 0; i < FtSite.NUM_COLUMNS; i++)
        //    {
        //        SQLLEN nullInd = Marshal.ReadInt64(pSiteNull.anSiteNull, i * sizeof(SQLLEN));
        //...Log2.v("\r\nFtUtils.FtPutSiteWN_NATIVE: pSiteNull.anSiteNull[" + i + "] = " + nullInd);
        //    }

        //    // Native call:  int ftPutSiteWN(ftSiteStr   * pSite, ftSiteNull* pSiteNull, int nDepth, char* cInTable)
        //    int rc = ftPutSiteWN(pSiteStr.stSite, pSiteNull.anSiteNull, nDepth, cInTable);

        //...Log2.v("\n\nFtUtils.FtPutSiteWN_NATIVE: Exit:  returned " + rc + "\r\n");
        //    return rc;
        //}
        //public static int FtGetSiteWN_NATIVE(string cCall, out FtSiteStr_NEW pSite_NEW, int nDepth, string cInTable, out FtSiteNull_NEW pSiteNull_NEW)
        //{
        //...Log2.v("\n\nFtUtils.FtGetSiteWN_NATIVE: Entry:  call = " + cCall + ",  Table = " + cInTable);

        //    // To satisfy the 'out' conditions.
        //    pSite_NEW = new FtSiteStr_NEW(FtSiteStr_NEW.Init.ALLOCATED);
        //    pSiteNull_NEW = new FtSiteNull_NEW(FtSiteNull_NEW.Init.ALLOCATED);

        //    // Use classes that mimic the native structures used in the native call.
        //    FtSiteStr pSite = new FtSiteStr(FtSiteStr.Init.ALLOCATED);
        //    FtSiteNull pSiteNull = new FtSiteNull(FtSiteNull.Init.ALLOCATED);

        //    // Make copies in global memory with pointers assigned.
        //    IntPtr pSitePtr = Marshal.AllocHGlobal(Marshal.SizeOf(pSite));
        //    IntPtr pSiteNullPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pSiteNull));

        //    // Native call:
        //    // int ftGetSiteWN(char* cCall, ftSiteStr** pSite, int nDepth, char* cInTable, ftSiteNull ** pSiteNull);
        //    int rc = ftGetSiteWN(cCall, ref pSitePtr, nDepth, cInTable, ref pSiteNullPtr);

        //...Log2.v("\r\nFtUtils.FtGetSiteWN_NATIVE: rc = " + rc);

        //    if (rc != Constant.SUCCESS)
        //    {
        //        // ftGetSiteWN failed to find a site in the DB - or there was an error.
        //        // Let the caller deal with it ...
        //...Log2.v("\n\nFtUtils.FtGetSiteWN_NATIVE: Exit: rc = " + rc);
        //        return rc;
        //    }

        //    // Reverse marshal the native call's output parameters.
        //    Marshal.PtrToStructure(pSitePtr, pSite);
        //    Marshal.PtrToStructure(pSiteNullPtr, pSiteNull);

        //    // Test results for sanity.
        //    Boolean nDepthIsNotValid = (pSite.nDepth < 1) || (pSite.nDepth > 3);   //Therefor nDepth = 1, 2, or 3.

        //    if (nDepthIsNotValid)
        //    {
        //        Log2.e("\r\nFtUtils.FtGetSiteWN_NATIVE: ERROR: pSite.nDepth is not 1, 2, or 3");
        //        Application.Exit();
        //    }
        //    else  // nDepth is 1, 2, or 3.
        //    {
        //        // Now construct the FtSiteStr_NEW and FtSiteNull_NEW 'out' objects.
        //        pSite_NEW = new FtSiteStr_NEW(FtSiteStr_NEW.Init.ALLOCATED);
        //        pSiteNull_NEW = new FtSiteNull_NEW(FtSiteNull_NEW.Init.ALLOCATED);

        //        // Copy reliable members first.
        //        pSite_NEW.stSite = pSite.stSite;
        //        pSite_NEW.nDepth = pSite.nDepth;
        //        pSite_NEW.pAntennas = pSite.pAntennas;

        //        if (pSite.nDepth >= 1)  // Deal with the site case first.
        //        {
        //            pSite_NEW.nNumAnts = 0;
        //            pSite_NEW.nNumChans = 0;

        //            pSite_NEW.stAntsPtr = null;
        //            pSite_NEW.stChanPtr = null;

        //...Log2.v("\r\nFtUtils.FtGetSiteWN_NATIVE: pSite_NEW = " + pSite_NEW.ToStringFtSiteOnly());

        //            // Get the site nulls.
        //            //Marshal.Copy(pSiteNull.anSiteNull, pSiteNull_NEW.anSiteNull, 0, FtSite.NUM_COLUMNS);
        //            Marshal.Copy(pSiteNullPtr, pSiteNull_NEW.anSiteNull, 0, FtSite.NUM_COLUMNS);

        //            for (int i = 0; i < pSiteNull_NEW.anSiteNull.Length; i++)
        //            {
        //...Log2.v("\r\nFtUtils.FtGetSiteWN_NATIVE: site nullInd[" + i + "] = " + pSiteNull_NEW.anSiteNull[i]);
        //            }

        //        }

        //        if (pSite.nDepth >= 2)
        //        {
        //            Log2.e("\r\nFtUtils.FtGetSiteWN_NATIVE: ERROR: pSite.nDepth > 1: Not Yet Implemented");
        //            Application.Exit();
        //        }
        //    }

        //...Log2.v("\n\nFtUtils.FtGetSiteWN_NATIVE: Exit: rc = " + rc);
        //    return rc;
        //}

        //public static int FtGetSite_NATIVE(string cCall, out FtSiteStr ftSiteStr, int nDepth, string cTable)
        //{
        //...Log2.v("\n\nFtUtils.FtGetSite_NATIVE(): Entry");

        //    int rv = 0;

        //    StringBuilder sb_cCall = new StringBuilder(cCall);
        //    StringBuilder sb_cTable = new StringBuilder(cTable);

        //    //We want ftSiteStr to be OUT only (i.e. ignore any previous instantiation).
        //    ftSiteStr = new FtSiteStr(FtSiteStr.Init.ALLOCATED);

        //    //Instantiate member object stSite.
        //    ftSiteStr.stSite = new FtSite();

        //    //Instantiate member pointer ftAntePtr to an address in unmanaged memory.
        //    FtAnte ftAnte = new FtAnte();
        //    //ftAnte.tgain = (float)2.718;
        //    IntPtr ftAntePtr = Marshal.AllocHGlobal(Marshal.SizeOf(ftAnte));
        //    Marshal.StructureToPtr(ftAnte, ftAntePtr, false);
        //    ftSiteStr.stAntsPtr = ftAntePtr;

        //    //Instantiate member pointer ftChanPtr to an address in unmanaged memory.
        //    FtChan ftChan = new FtChan();
        //    //ftChan.freqtx = 9.18;
        //    IntPtr ftChanPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ftChan));
        //    Marshal.StructureToPtr(ftChan, ftChanPtr, false);
        //    ftSiteStr.stChanPtr = ftChanPtr;

        //    //Make a copy of ftSiteStr in unmanaged memory and get its start address.
        //    IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ftSiteStr));
        //    Marshal.StructureToPtr(ftSiteStr, intPtr, false);

        //    //Call the native function.
        //    rv = ftGetSite(sb_cCall, ref intPtr, nDepth, sb_cTable);

        //    cCall = sb_cCall.ToString();
        //    cTable = sb_cTable.ToString();

        //    //Reverse marshal the ftSiteStr object.
        //    ftSiteStr = (FtSiteStr)Marshal.PtrToStructure(intPtr, typeof(FtSiteStr));

        //...Log2.v("\n\nFtUtils.FtGetSite_NATIVE(): Exit: call1 = " + ftSiteStr.stSite.call1);
        //    return rv;
        //}

        public static int FtNextCall_NATIVE(out string cCall, string cLastCall, string cInTableShort)
        {
            //...Log2.v("\n\nFtUtils.FtNextCall_NATIVE(): Entry:");

            cCall = "";

            int rv = 0;

            StringBuilder sb_cCall = new StringBuilder();
            StringBuilder sb_cLastCall = new StringBuilder(cLastCall);
            StringBuilder sb_cInTableShort = new StringBuilder(cInTableShort);

            //...Log2.v("\r\nFtUtils.FtNextCall_NATIVE(): Before native call.");
            rv = ftNextCall(sb_cCall, sb_cLastCall, sb_cInTableShort);
            //...Log2.v("\r\nFtUtils.FtNextCall_NATIVE(): After native call.");

            cCall = sb_cCall.ToString();
            cLastCall = sb_cLastCall.ToString();
            cInTableShort = cInTableShort.ToString();

            //...Log2.v("\n\nFtUtils.FtNextCall()_NATIVE: Exit: cCall =         " + cCall);
            //...Log2.v("\n\nFtUtils.FtNextCall()_NATIVE: Exit: cLastCall =     " + cLastCall);
            //...Log2.v("\n\nFtUtils.FtNextCall()_NATIVE: Exit: cInTableShort = " + cInTableShort);

            //...Log2.v("\n\nFtUtils.FtNextCall()_NATIVE: Exit:");
            return rv;
        }
#endif

        //--------------------------------------------------------------------------------------------------
        /*
		Ordering of the retrieval.  In order to change the order of the antennas
		and channels as they are retrieved (and therefore stored), we call these
		routines.	GJS - 2015-06 - added for hilocheck.
        */

        private static string glbFTAntOrder = " call1, call2, bndcde, anum";
        private static string glbFTChanOrder = "call1, call2, bndcde, chid";

        public const string SELECT = "SELECT cmd,recstat,call1,call2,bndcde,anum,ause,acode,aht,azmth,elvtn,dist,offazm,tazmth,telvtn,tgain,txfdlnth,txfdlnlh,txfdlntv,txfdlnlv,rxfdlnth,rxfdlnlh,rxfdlntv,rxfdlnlv,txpadpam,rxpadlna,txcompl,rxcompl,obsloss,kvalue,atwrno,nota,apoint,sdate,licence,mdate,mtime FROM  {0} WHERE call1 = '{1}' and call2 = '{2}' and bndcde = '{3}' and anum = {4} ";

        //---------------------------------------------------------------------------------------

        /// <summary>
        /// Determines whether there is any record in a database FT SITE table that satisfies a
        /// prescribed search criteria (i.e. an SQL 'where' clause).
        /// </summary>
        /// <param name="cSearch"> - prescribes the 'where' clause parameters.</param>
        /// <param name="cInTable"> - name of the PDF.</param>
        /// <param name="cCallFound"> - on input this prescribes that any found call sign must be aphabetically greater 
        /// than cCallFound; if a record that matches all the search criteria is found, cCallFound returns its call sign.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - at least one SITE record exists that satisfies the search criteria.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed.</para>
        /// <para>- ErrorMessages.ODBC_GET_FAILED - call to ODBC.SQLGetData() threw an exception. </para>
        /// <para>- Constant.NOMORERECS - no SITE record found that matches search criteria.</para>
        public static int FtEnumSite(string cSearch, string cInTable, ref string cCallFound)
        {
            //...Log2.v("\n\nFtUtils.FtEnumSite(): Entry: cSearch = " + cSearch);

            int nRet;

            string s_SQL;
            string cWhere;
            SQLLEN vNullInd = 0;
            string cTableName;
            string cTable;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            if (String.IsNullOrWhiteSpace(cSearch))
            {
                cWhere = "";
            }
            else
            {
                cWhere = String.Format("{0} and ", cSearch);
            }

            FtGetTableName(out cTable, cInTable);

            /*  Open a new search */
            GenUtil.UtCvtName(Constant.FT_SITE, cTable, out cTableName);

            /*	Create the statement. */
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            s_SQL = String.Format("Select top 1 call1 FROM {0} WHERE ({1} call1>'{2}') ORDER BY call1 ",
                cTableName, cWhere, cCallFound);

            //...Log2.v("\r\nFtUtils.FtEnumSite(): s_SQL = " + s_SQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, s_SQL, s_SQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                /*  Error encountered */
                Ssutil.DbGetDiagStmt(hStmt, "*ERROR* - Could not enumerate sites for " + cTable + "\r\n");
                nRet = Error.ODBC_EXECDIRECT_FAILED;
            }

            /*  Now get the first call sign */
            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                nRet = Constant.NOMORERECS;
            }
            else
            {
                if (Ssutil.DbGetString(hStmt, 1, "Call1", out cCallFound, Constant.CALLSIGN_SZ, out vNullInd) == 0)
                {
                    /*  Return the value */
                    nRet = Constant.SUCCESS;
                }
                else
                {
                    Ssutil.DbGetDiagStmt(hStmt, "ftEnumSite - Could not get callsign");
                    nRet = Error.ODBC_GET_FAILED;
                }
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nFtUtils.FtEnumSite(): Exit: nRet = " + nRet);
            return nRet;
        }

        /// <summary>
        /// Prescribes the ordering of ANTENNA record retrieval, i.e. the order of the antennas
        /// and channels as they are retrieved (and therefore stored). The default SQL ordering is
        /// 'ORDER BY  call1, call2, bndcde, anum'.
        /// </summary>
        /// <param name="cOrderStr"> - prescribed 'ORDER BY' parameters.</param>
        public static void FtSetAntOrder(string cOrderStr)
        {
            if (String.IsNullOrWhiteSpace(cOrderStr))
            {
                glbFTAntOrder = cOrderStr;
            }
            else
            {
                //	Set to default if the input is zero length or NULL.
                glbFTAntOrder = " call1, call2, bndcde, anum";
            }
        }

        /// <summary>
        /// Returns a string containing the parameters that will be used in the
        /// SQL 'ORDER BY' clause for subsequent ANTENNA record retrieval.
        /// </summary>
        /// <returns> - SQL 'ORDER BY" parameters.</returns>
        public static string FtGetAntOrder()
        {
            return glbFTAntOrder;
        }

        /// <summary>
        /// Prescribes the ordering of CHANNEL record retrieval; the default SQL ordering is
        /// 'ORDER BY  call1, call2, bndcde, chid'.
        /// </summary>
        /// <param name="cOrderStr"> - prescribed 'ORDER BY' parameters.</param>
        public static int FtSetChanOrder(string cOrderStr)
        {
            if (String.IsNullOrWhiteSpace(cOrderStr))
            {
                //	Set to default if the input is zero length or NULL.
                glbFTChanOrder = " call1, call2, bndcde, chid";
            }
            else
            {
                glbFTChanOrder = cOrderStr;
            }
            return 0;
        }

        /// <summary>
        /// Returns a string containing the parameters that will be used in the
        /// SQL 'ORDER BY' clause for subsequent CHANNEL record retrieval.
        /// </summary>
        /// <returns> - SQL 'ORDER BY" parameters.</returns>
        public static string FtGetChanOrder()
        {
            return glbFTChanOrder;
        }

        /// <summary>
        /// Gets the next call sign, in a alphabetical sort order, in the ft_{0}_site table that immediately follows the last call sign retrieved;
        /// to get the first call sign, call this method with cLastCall set to null or "".
        /// </summary>
        /// <param name="cCall"> - the next call sign after cLastCall.</param>
        /// <param name="cLastCall"> - the value of cCall returned by the previous call to this method.</param>
        /// <param name="cInTableShort"> - PDF table name.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - next call sign was successfully retrieved.</para>
		/// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
		/// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtNextCall(out string cCall, string cLastCall, string cInTableShort)
        {
            //...Log2.v("\n\nFtUtils.FtNextCall(): Entry:");

            // Satisfy 'out' requirment.
            cCall = "";

            // Handle null or blank input.
            if (String.IsNullOrWhiteSpace(cLastCall))
            {
                cLastCall = "";
            }

            string cSQL;
            string cTable;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            FtGetTableName(out cTable, cInTableShort);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select top (1) call1 from {0}.ft_{1}_site where call1 > '{2}' order by call1",
                Info.GlobalSchema, cTable, cLastCall);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nFtUtils.FtNextCall(): SQLExecDirect() failed for cSQL = {0}", cSQL);
                Ssutil.DbGetDiagStmt(hStmt, "ftNextCall - Error in Exec Direct:-\r\n" + cSQL);
                sqlRet = Error.ODBC_EXECDIRECT_FAILED;
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    // No more data to fetch.
                }
                else if (!ODBC.IsOK(sqlRet))
                {
                    // Error
                    Ssutil.DbGetDiagStmt(hStmt, "ftNextCall: Error getting call sign.");
                    sqlRet = Error.ODBC_FETCH_FAILED;
                }
                else
                {
                    SQLLEN nullInd;
                    Ssutil.DbGetString(hStmt, 1, "call1", out cCall, 10, out nullInd);
                    sqlRet = Constant.SUCCESS;
                }
            }
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nFtUtils.FtNextCall(): Exit: cCall =         " + cCall);
            //...Log2.v("\n\nFtUtils.FtNextCall(): Exit: cLastCall =     " + cLastCall);
            //...Log2.v("\n\nFtUtils.FtNextCall(): Exit: cInTableShort = " + cInTableShort);
            //...Log2.v("\n\nFtUtils.FtNextCall(): Exit:");

            return sqlRet;
        }

        /// <summary>
        /// This method returns an array of call signs, ordered by call1, corresponding to all of the DB _site
        /// tables for a prescribed PDF root name.
        /// </summary>
        /// <param name="cInTableShort"> - PDF table root name.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - next call sign was successfully retrieved.</para>
		/// <para>-   ODBC.SQL_NO_DATA                - fetch attempt failed because there is no more data.</para>
		/// <para>-   ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static string[] FtGetCallSigns(string cInTableShort)
        {
            List<string> callSigns = new List<string>();
            string cSQL;
            string cTable;

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            FtGetTableName(out cTable, cInTableShort);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select call1 from {0}.ft_{1}_site order by call1", Info.GlobalSchema, cTable);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nFtUtils.FtGetCallSigns(): ERROR: SQLExecDirect() returned {0} for query = {1}", sqlRet, cSQL);
                Ssutil.DbGetDiagStmt(hStmt, "FtGetCallSigns() - Error in Exec Direct:-\r\n" + cSQL);
                sqlRet = Error.ODBC_EXECDIRECT_FAILED;
            }
            else
            {
                bool areMoreRecords = true;
                while (areMoreRecords)
                {
                    sqlRet = ODBC.SQLFetch(hStmt);

                    if (sqlRet == ODBC.SQL_NO_DATA)
                    {
                        // No more data to fetch.
                        areMoreRecords = false;
                    }
                    else if (!ODBC.IsOK(sqlRet))
                    {
                        // Error
                        Ssutil.DbGetDiagStmt(hStmt, "FtGetCallSigns(): Error getting call sign.");
                        break;
                    }
                    else
                    {
                        string call1;
                        SQLLEN nullInd;
                        Ssutil.DbGetString(hStmt, 1, "call1", out call1, 10, out nullInd);

                        callSigns.Add(call1);
                    }
                }

            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return callSigns.ToArray();
        }

        /// <summary>
        /// Creates an FtTitle object and populates it with values read from the 
        /// DB table <b>&lt;userID&gt;.ft_&lt;pdfName&gt;_titl</b>.
        /// </summary>
        /// <param name="pTitle"> - FtTitle object.</param>
        /// <param name="cInTable"> - either a short or full DB table name.</param>
        /// <returns></returns>
        public static int FtGetTitle(out FtTitl pTitle, string cInTable)      //struct ftTitl_      ** pTitle,   /*  Title returned         */
        {
            //...Log2.v("\n\nFtUtils.FtGetTitle(): Entry");
            pTitle = null;
            //    char cSQL[4000];

            //        char cTab[TABLE_NM_SZ + 1];
            string cTab;
            //        char cTable[TABLE_NM_SZ];
            string cTable;
            int nRet = 0;

            SQLHDBC hConnection = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;


            FtGetTableName(out cTable, cInTable);

            //	if (utCvtName(FT_TITL, cTable, cTab) != 0) { /*  Get the site table name
            if (GenUtil.UtCvtName(Constant.FT_TITL, cTable, out cTab) != 0)
            {
                Ssutil.DisConn(hConnection);
                nRet = -10;
                return (nRet);
            }

            //	Allocate the handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "ftGetTitle: Could not allocate handle");

                Ssutil.DisConn(hConnection);
                return (-30);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append("validated,");
            sb.Append("namef,");
            sb.Append("source,");
            sb.Append("descr,");
            sb.Append("mdate,");
            sb.Append("mtime ");
            sb.Append("FROM ");
            sb.Append(cTab);
            string cSQL = sb.ToString();

            //...Log2.v("\r\nFtUtils.FtGetTitle(): SQLExecDirect():\r\n" + cSQL);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, ODBC.SQL_NTS);

            if (!ODBC.IsOK(sqlRet))
            {
                // The title table may not exist?
                Log2.e("\n\nFtUtils.FtGetTitle(): ERROR: SQLExecDirect() failed for query: " + cSQL);
                nRet = -1;
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                    Ssutil.DisConn(hConnection);
                    return 1;   //	No record.
                }

                if (!ODBC.IsOK(sqlRet))
                {
                    Ssutil.DbGetDiagStmt(hStmt, "ftGetTitle: Could not retrieve title record.");

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                    Ssutil.DisConn(hConnection);
                    return -32;
                }

                //	Allocate the title in the calling program.  It will need to free it.
                pTitle = new FtTitl();

                string cName = "";
                SQLLEN nNull = new SQLLEN();
                try
                {
                    cName = "validated";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 1, cName, out pTitle.validated, Constant.FTTITLE_VALIDATED_SZ, out nNull);

                    Log2.v("\r\nFtUtils.FtGetTitle(): Ssutil.DbGetString(): returned " + sqlRet + " for FtTitle field: " + cName);
                    Log2.v("\r\nFtUtils.FtGetTitle(): returned validate value " + pTitle.validated);
                    /*
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();

                    cName = "namef";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 2, cName, out pTitle.namef, Constant.FTTITLE_NAMEF_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    cName = "source";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 3, cName, out pTitle.source, Constant.FTTITLE_SOURCE_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    cName = "descr";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 4, cName, out pTitle.descr, Constant.FTTITLE_DESCR_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    cName = "mdate";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 5, cName, out pTitle.mdate, Constant.DATE_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    cName = "mtime";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 6, cName, out pTitle.mtime, Constant.TIME_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.
                    */
                }
                catch (Exception e)
                {
                    Log2.e("\nFtUtils.FtGetTitle(): ERROR: ODBC 'get' request failed: " + e.Message);
                    
                    Ssutil.DbGetDiagStmt(hStmt, "ftGetTitle: Could not retrieve title record on field: " + cName);
                    
                    Log2.e("\r\nFtUtils.FtGetTitle(): Ssutil.DbGetString() FAILED to retrieve FtTitle field: " + cName + ":" + sqlRet);
                    Log2.e("\r\nFtUtils.FtGetTitle(): Ssutil.DbGetString() Exception message = \r\n" + e.Message);

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConnection);

                    return (-31);
                }
                nRet = 0;
            }

            //Release resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConnection);

            //...Log2.v("\n\nFtUtils.FtGetTitle(): Exit");
            return nRet;
        }

        /// <summary>
        /// Checks whether a table name is already a full table name that start with ft_ and, if so, 
        /// extracts the base (short) table name from it.
        /// </summary>
        /// <param name="cTable"> - base (short) table name.</param>
        /// <param name="cInTable"> - either a short or full table name.</param>
        public static void FtGetTableName(out string cTable, string cInTable)
        {
            // 'out' requirement.
            cTable = null;

            //if ((cDot = strchr(cInTable, '.')) != NULL)
            if (cInTable.Contains('.'))
            {
                //...Log2.v("\nFtUtils.FtGetTableName(): cInTable = " + cInTable);
                //	We have an incorrect input, correct it.  It will be in form:-
                //	<schema>.ft_<table>_xxxx, what we want is the <table>

                //  NOTE: the <table> could itself conatin one or more '_' characters.

                string pattern = @"^\w+\.ft_(\w+)_[a-z]{4}$";

                Match match = Regex.Match(cInTable, pattern);
                if (match.Success)
                {
                    cTable = match.Groups[1].Value;
                    //...Log2.v("\nFtUtils.FtGetTableName(): cTable = " + cTable);
                }
                else
                {
                    Log2.e("\nFtUtils.FtGetTableName(): ERROR: no match to pattern.");
                }

            }
            else
            {
                //safecopy(cTable, cInTable, nOutLen);
                cTable = cInTable;

            }
        }

        /// <summary>
        /// Determine whether an antenna code represents a billboard passive antenna, or not; 
        /// billboard passive antennae have code that end with the letter '%'.
        /// </summary>
        /// <param name="cAcode"> - the antenna code to be parsed.</param>
        /// <returns>true if code ends with '%'; otherwise false.</returns>
        public static bool IsBillBoard(string cAcode)
        {
            //int nLen = (int)strlen(cAcode) - 1;
            bool nRet = false;

            /*	Skip any blanks at the end. */
            //while (nLen > -1 && cAcode[nLen] == ' ')
            //{
            //    nLen--;
            //}

            if (!String.IsNullOrWhiteSpace(cAcode))
            {
                string str = cAcode.Trim();

                if (str.EndsWith("%"))
                {
                    /*	Close enough.  Must not be zero length anyway, and must end in % */
                    nRet = true;
                }
            }

            return nRet;
        }

        /**************************************************************************\
        *
        *   Get a channel by call2, band, and chid
        *
        \**************************************************************************/

        /// <summary>
        /// Retrieves a TS SITE record from a DB FT_SITE table together with its associated nullInds. 
        /// The caller prescribes the 'depth' of retrieval, i.e. 'site only', site and antennae' 
        /// or 'site, antennae and channels'.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// </remarks>
        /// <param name="cCall"> - call sign of the SITE to be retrieved.</param>
        /// <param name="ftSiteStr"> - a FtSiteStr_NEW object populated with data.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks above).</param>
        /// <param name="cInTable"> - PDF table name.</param>
        /// <param name="ftSiteNull"> - a FtSiteNull_NEW object populated with data.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB..</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- ErrorMessages.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- ErrorMessages.UTCVTNAME_FAILED - call to GenUtil.UtCvtName() failed.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILED - ODBC SQLAllocHandle() failed.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtGetSiteWN(string cCall, out FtSiteStr ftSiteStr, int nDepth, string cInTable, out FtSiteStrNulls ftSiteNull)
        {
            //...Log2.v("\n\nFtUtils.FtGetSiteWN(): A");

            string cSQLBuff;
            SQLRETURN sqlRet;
            SQLHSTMT hStmt;
            SQLHDBC hConnection;
            int nRet = -666;
            int nInd = 0;

            // First, create and populate the structured data objects to be output.
            // These constructors set appropriate initial values.
            ftSiteStr = new FtSiteStr(FtSiteStr.Init.UNALLOCATED);
            ftSiteNull = new FtSiteStrNulls(FtSiteStrNulls.Init.UNALLOCATED);

            // Set the depth parameter.
            ftSiteStr.nDepth = nDepth;

            // First, sanity check the input data and return if there is a problem..
            Boolean cCallHasNoContent = String.IsNullOrWhiteSpace(cCall);
            Boolean nDepthIsNotValid = (nDepth < 1) || (nDepth > 3);   //Therefor nDepth = 1, 2, or 3.
            if (cCallHasNoContent || nDepthIsNotValid)
            {
                Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Invalid cCall or nDepth : " + cCall + ", " + nDepth);
                nRet = Error.INVALID_DATA;
                return nRet;
            }

            // Establish an ODBC connection.
            hConnection = Ssutil.NewConn();

            // Get the full site table name.
            string cTable;
            FtGetTableName(out cTable, cInTable);
            string cFullTableName;  // The full table name.
            if (GenUtil.UtCvtName(Constant.FT_SITE, cTable, out cFullTableName) != 0)
            { /*  Get the site table name in cTab */
                GenUtil.SetError(1002, "*ERROR* ftGetSiteWN: Converting the table name.");
                //...Log2.v("\n\nftGetSiteWN(): Exit: -10");
                nRet = Error.UTCVTNAME_FAILED;
                return (nRet);
            }

            // ============================================
            // Find and fetch the FtSite data from the DB.
            // ============================================

            // Get a handle to a SQL statement to use to fetch FtSite data from the DB.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker A: SQLAllocHandle() returned " + sqlRet);
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                return (Error.ODBC_SQLALLOCHANDLE_FAILED);  // was -14
            }

            // Create the SQL 'select' statement.
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            sb.Append(FtSite.AllColumnsForSqlSelect);
            sb.Append(" FROM ");
            sb.Append(cFullTableName);
            sb.Append(" WHERE call1='");
            sb.Append(cCall);
            sb.Append("'");
            cSQLBuff = sb.ToString();

            // Do the SQL 'ExecDirect' query to perform selection of records.
            //...Log2.v("\nFtUtils.FtGetSiteWN(): cSQLBuff = {0}", cSQLBuff);
            sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                Log2.e("\n\n" + cSQLBuff);
                Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker B: SQLExecDirect() returned " + sqlRet);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                return (Error.ODBC_EXECDIRECT_FAILED); // was -15
            }

            // We are assuming that only one FtSite in the DB has a call1 column that matches the cCall
            // provided by the caller. We should test for this!
            cSQLBuff = " call1='" + cCall + "' ";
            int nNumSites = Ssutil.DbCountRows(cFullTableName, cSQLBuff);

            //...Log2.v("\r\nFtUtils.FtGetSiteWN(): nNumSites = " + nNumSites);

            // Check for failure of DbCountRows() to return valid number of sites.
            if (nNumSites < 0)
            {
                Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumSites);
                string str = String.Format("ftGetSiteWN03: Ingres error {0} getting sites.", nNumSites);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of no sites to read from the DB.
            else if (nNumSites == 0)
            {
                //...Log2.v("\r\nFtUtils.FtGetSiteWN(): Marker G: site not found in DB; cCall = " + cCall);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of too many sites to read from the DB.
            else if (nNumSites >= 2)
            {
                Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker G-1: multiple sites in DB with cCall = " + cCall);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConnection);
                return (Error.INVALID_DATA);
            }
            else // #1, nNumSite = 1
            {
                // Get a DynSite cursor.
                int nDynSiteCursor = DynSite.GetNextFreeCursor();
                DynSite.cursors[nDynSiteCursor].hStmt = hStmt;

                // We have now found the site; next get the column data. 
                FtSite ftSite;
                SQLLEN[] nullInd;

                if (DynSite.FtFetchSite(nDynSiteCursor, out ftSite, out nullInd) == Constant.SUCCESS)
                {
                    // Save the site and associated column-nulls that we just fetched from the DB.
                    ftSiteStr.stSite = ftSite;
                    ftSiteNull.anSiteNull = nullInd;
                }
                else
                {
                    Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker G-2: failed to fetch an extant site from the DB; cCall = " + cCall);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConnection);
                    //AH: Free the DynSite Cursor.
                    DynSite.cursors[nDynSiteCursor].hStmt = IntPtr.Zero;
                    DynSite.cursors[nDynSiteCursor].cursorOpen = false;
                    return (Constant.FAILURE);
                }

                // If we reach here, the fetch was successful.
                // Free the statement handle.
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                //AH: Free the DynSite Cursor.
                DynSite.cursors[nDynSiteCursor].hStmt = IntPtr.Zero;
                DynSite.cursors[nDynSiteCursor].cursorOpen = false;
            }

            // At this point we have the site read in from the DB and stored in the site structure.  
            // If nDepth is 1, then this is all that was required and we can exit.  
            if (nDepth == 1)
            {
                Ssutil.DisConn(hConnection);
                //...Log2.v("\n\nFtUtils.FtGetSiteWN(): Exit (nDepth == 1)");
                return Constant.SUCCESS;
            }
            // End of fetching the site data from the DB.

            // ============================================
            // Find and fetch the FtAnte data from the DB.
            // ============================================

            // At this point, nDepth is 2 or 3, so we need to fetch and store (at least) the antennae data from the DB.

            // Get the full antenna table name.
            GenUtil.UtCvtName(Constant.FT_ANTE, cTable, out cFullTableName);

            FtAnte[] pAnts = null;
            SQLLEN[][] pAntNull = null;  // Usage: [antenna, column]
            int nNumAnts;

            // First find out how many antennae there are in order to allocate the array.
            //sprintf_s(cSQLBuff, sizeof(cSQLBuff), "call1 = '%s'", cCall);
            cSQLBuff = " call1='" + cCall + "' ";
            nNumAnts = Ssutil.DbCountRows(cFullTableName, cSQLBuff);

            //...Log2.v("\r\nFtUtils.FtGetSiteWN(): nNumAnts = " + nNumAnts);

            // Check for failure of DbCountRows() to return valid number of antennae.
            if (nNumAnts < 0)
            {
                Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumAnts);
                Ssutil.DisConn(hConnection);
                return (nNumAnts);
            }

            // Handle the case of no antennae to read from the DB.
            if (nNumAnts == 0)
            {
                Log2.w("\r\nFtUtils.FtGetSiteWN(): WARNING: Marker G: number of antennae to read from DB is zero.");
            }
            else // #1, nNumAnts > 0
            {
                // Create a statement handle to perform the subsequent SQL queries.
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                    //...Log2.v("\n\nftGetSiteWN(): Exit: -14");
                    return (-21);
                }
                //AH: Temporary.
                int nAnteCursor = DynAntenna.GetNextFreeCursor();
                DynAntenna.cursors[nAnteCursor].hStmt = hStmt;
                DynAntenna.cursors[nAnteCursor].cursorOpen = true;

                // Create the SQL query required to read in the FtAnte column data from the DB.
                sb = new StringBuilder();
                sb.Append("SELECT ");
                sb.Append(FtAnte.AllColumnsForSqlSelect);
                sb.Append("FROM  ");
                sb.Append(cFullTableName);
                sb.Append(" WHERE call1='");
                sb.Append(cCall);
                sb.Append("'  ORDER BY ");
                sb.Append(FtGetAntOrder());  // " call1, call2, bndcde, anum"
                cSQLBuff = sb.ToString();

                // Perform the SQL SELECT query.
                //...Log2.v("\r\nFtUtils.FtGetSiteWN(): SQLExecDirect():\r\n" + cSQLBuff);
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\n\n" + cSQLBuff);
                    Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker H: SQLExecDirect() returned " + sqlRet);
                    Ssutil.DbGetDiagStmt(hStmt, "GetMTSite04 - Error retrieving antenna.");
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConnection);
                    return -22;
                }

                // Instantiate the objects required store the antennae data (multiple fetches from the DB).
                pAnts = Arrays.CreateArrayUsingDefaultElementConstructor<FtAnte>(nNumAnts);
                pAntNull = new SQLLEN[nNumAnts][];

                // Now fetch the selected antennae data from the DB using a loop.
                nInd = 0;  //Counts the number of records fetched.
                FtAnte ftAnte;
                SQLLEN[] nullInd;

                while (DynAntenna.FtFetchAntenna(nAnteCursor, out ftAnte, out nullInd) == Constant.SUCCESS)
                {
                    nInd++;
                    // Sanity-check the number of antennae read from the DB so far.
                    if (nInd > nNumAnts)
                    {
                        Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker J: SQLFetch(): antennae count exceeds that of DbCountRows()");
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConnection);
                        GenUtil.SetError(1007, "*ERROR* ftGetSiteWN: Antenna counts wrong.");
                        return (-10);
                    }

                    // Accumulate the data over successive fetch-loops.
                    pAnts[nInd - 1] = ftAnte;
                    pAntNull[nInd - 1] = nullInd;

                } //end of while-loop

                // We have now accumulated all the antennae data - 'attach' it to ftSiteStr.
                ftSiteStr.stAntsPtr = pAnts;
                ftSiteStr.nNumAnts = nInd;
                // We have now accumulated all the antennae nullInd data - 'attach' it to ftSiteNull.
                ftSiteNull.anAntsNullPtr = pAntNull;


                // Free the SQL statement handle.
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                // Free-up cursor.
                DynAntenna.cursors[nAnteCursor].hStmt = IntPtr.Zero;
                DynAntenna.cursors[nAnteCursor].cursorOpen = false;

            } // else #1, nNumAnts > 0

            // At this point we have the site and antennae read in from the DB.  
            // If nDepth is 2, then this is all that was required and we can exit.  
            if (nDepth == 2)
            {
                Ssutil.DisConn(hConnection);
                //...Log2.v("\n\nFtUtils.FtGetSiteWN(): Exit (nDepth == 2)");
                return Constant.SUCCESS;
            }
            // End of fetching the antennae data from the DB. -------------------------------------------



            // ============================================
            // Find and fetch the FtChan data from the DB.
            // ============================================

            // We have now loaded the site and antennae data (if any).  
            // If nDepth = 3, we need to fetch the channel data from the DB.

            if (nDepth == 3) // #3
            {
                // Get the full channel table name.
                GenUtil.UtCvtName(Constant.FT_CHAN, cTable, out cFullTableName);

                // Use these two objects to accumulate the channel data fetched from the DB.
                FtChan[] pChan = null;
                SQLLEN[][] pChanNull = null;

                // Determine how many channels there are in order to allocate the array.
                cSQLBuff = "call1='" + cCall + "'";
                int nNumChan = Ssutil.DbCountRows(cFullTableName, cSQLBuff);

                //...Log2.v("\r\nFtUtils.FtGetSiteWN(): nNumChan = " + nNumChan);

                if (nNumChan < 0)
                {
                    // An error occurred - deal with it.
                    Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker M: DbCountRows() returned nNumChan = " + nNumChan);
                    Ssutil.DbGetDiagStmt(hStmt, "ftGetSite24 - Error getting channel count.");
                    Ssutil.DisConn(hConnection);
                    return -24;
                }

                // Handle the case of no channels to read (i.e. nNumChan == 0).
                else if (nNumChan == 0)
                {
                    Log2.w("\r\nFtUtils.FtGetSiteWN(): WARNING: Marker N: number of channels to read from DB is zero.");
                }

                // Handle the 'regular' case of nNumChan > 0.
                else  // #2, nNumChan > 0
                {
                    // Accumulate the channel and nullInd data in these two objects.
                    pChan = Arrays.CreateArrayUsingDefaultElementConstructor<FtChan>(nNumChan);
                    pChanNull = new SQLLEN[nNumChan][];

                    // Get an SQL handle to fetch the channel data.
                    sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker P: Could not allocate handle for channel fetches");
                        Ssutil.DbGetDiagStmt(hStmt, "mtGetSite14 - Could not allocate handle for channel");
                        Ssutil.DisConn(hConnection);
                        return (-14);
                    }

                    //AH: temporary.
                    int nChanCursor = DynChannel.GetNextFreeCursor();
                    DynChannel.cursors[nChanCursor].hStmt = hStmt;
                    DynChannel.cursors[nChanCursor].cursorOpen = true;

                    // Create the SQL statement required to fetch the channel data from the DB.
                    sb = new StringBuilder();
                    sb.Append("SELECT ");
                    sb.Append(FtChan.AllColumnsForSqlSelect);
                    sb.Append("FROM  ");
                    sb.Append(cFullTableName);
                    sb.Append(" WHERE call1 = '");
                    sb.Append(cCall);
                    sb.Append("' ORDER BY ");
                    sb.Append(FtGetChanOrder());   // "call1, bndcde, call2, chid"
                    cSQLBuff = sb.ToString();

                    // Perform the SQL 'SELECT' query.
                    //...Log2.v("\r\nFtUtils.FtGetSiteWN(): SQLExecDirect():\r\n" + cSQLBuff);
                    sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);
                    if (!ODBC.IsOK(sqlRet))
                    {
                        //...Log2.v("\n\n" + cSQLBuff);
                        //...Log2.v("\r\nFtUtils.FtGetSiteWN(): SQLExecDirect() returned sqlRet = " + sqlRet);
                        Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConnection);
                        return -25;
                    }

                    // Start a loop to fetch all the selected channel data from the DB.
                    nInd = 0;  //This counts the number of successful channel fetches so far.
                    FtChan ftChan;
                    SQLLEN[] nullInd;

                    while (DynChannel.FtFetchChannel(nChanCursor, out ftChan, out nullInd) == Constant.SUCCESS)
                    {
                        nInd++;
                        // Sanity-check the number of channel records read from the DB so far.
                        if (nInd > nNumChan)
                        {
                            Log2.e("\r\nFtUtils.FtGetSiteWN(): ERROR: Marker R: SQLFetch(): channel count exceeds that of DbCountRows()");
                            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                            Ssutil.DisConn(hConnection);
                            DynChannel.cursors[nChanCursor].hStmt = IntPtr.Zero;
                            DynChannel.cursors[nChanCursor].cursorOpen = false;
                            return (-27);
                        }

                        //Accumulate the channel data over successive fetch-loops.
                        pChan[nInd - 1] = ftChan;
                        pChanNull[nInd - 1] = nullInd;

                    } // end of while fetch-loop.

                    // We have now accumulated all the channel data - 'attach' it to ftSiteStr.
                    ftSiteStr.stChanPtr = pChan;
                    ftSiteStr.nNumChans = nInd;
                    // We have now accumulated all the channel nullInd data - 'attach' it to ftSiteNull.
                    ftSiteNull.anChanNullPtr = pChanNull;

                    // Tidy up.
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    DynChannel.cursors[nChanCursor].hStmt = IntPtr.Zero;
                    DynChannel.cursors[nChanCursor].cursorOpen = false;

                } // #2, if nNumChan > 0


            } // #3, if (nDepth == 3)

            // Disconnect from ODBC / SQL DB.
            Ssutil.DisConn(hConnection);

            //...Log2.v("\n\nFtUtils.FtGetSiteWN(): Exit, nDepth == 3");
            return Constant.SUCCESS;
        }  //end of method

        /// <summary>
        /// Write a FtSiteStr_NEW object out to the indicated ft tables. 
        /// If the same site already exists in the table then it is completely
        /// replaced. The caller must also provide the associated ODBC nullInd
        /// data.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// </remarks>
        /// <param name="pSiteStr_NEW"> - FtSiteStr_NEW object to be wriiten to the DB.</param>
        /// <param name="pSiteNull_NEW"> - FtSiteNull_NEW object providing ODBC nullInd data for pSiteStr_NEW.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks)</param>
        /// <param name="cInTable"> - name of the PDF table to be written to.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS - successful write to DB.</para>
        /// <para>- any other return value - call failed.</para>
        public static int FtPutSiteWN(FtSiteStr pSiteStr_NEW, FtSiteStrNulls pSiteNull_NEW, int nDepth, string cInTable)
        {
            //...Log2.v("\n\nFtUtils.FtPutSiteWN: Entry:  Table = " + cInTable);

            string cSQLBuff;       // Buffer for SQL statement
            string cTable;
            string cTab;           // The full table name

            int nRet = 0;          // Return code
            short nInd;            //General loop Index

            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;
            SQLHDBC hConn;

            //...Log2.v("\n\nFtUtils.FtPutSiteWN(): Entry");

            hConn = Ssutil.NewConn();

            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            // Perform basic error-checking to ensure call validity.
            if (pSiteStr_NEW == null)
            {
                Application.Exit("FtUtils.FtPutSiteWN(): pSiteStr_NEW == null");
            }
            if (nDepth < 1 || nDepth > 3)
            {
                Application.Exit("FtUtils.FtPutSiteWN(): nDepth < 1 || nDepth > 3");
            }
            if (cInTable == null || cInTable.Length == 0)
            {
                Application.Exit("FtUtils.FtPutSiteWN(): cInTable == null || cInTable.Length == 0");
            }

            try
            {
                if (String.IsNullOrWhiteSpace(pSiteStr_NEW.stSite.call1))
                {
                    /*  No call sign -- abort */
                    //throw -3;          
                    throw new Exception("-3");

                }
                /*	if the depth requested is greater than the depth in the structure,
                *		use the smaller one.  */
                if (nDepth > pSiteStr_NEW.nDepth)
                {
                    nDepth = pSiteStr_NEW.nDepth;
                }

                //...Log2.v("\r\nFtUtils.FtPutSiteWN: putting site into DB");

                FtGetTableName(out cTable, cInTable);

                // Get the site table name in cTab.
                if (GenUtil.UtCvtName(Constant.FT_SITE, cTable, out cTab) == Constant.FAILURE)
                {
                    throw new Exception("-10");
                }

                /*  First delete the site. */
                cSQLBuff = String.Format(" call1='{0}'", pSiteStr_NEW.stSite.call1);

                Ssutil.DbDeleteRows(cTab, cSQLBuff);

                /* Build the SQL insert query*/
                cSQLBuff = FtSite.BuildSqlInsertString(cTab);

                //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
                //to contain the values to bind to. This necessitates copying the values of the nullInd
                //array elements into global memory with an SQLLENPTR pointer assigned to each one.
                SQLLENPTR[] nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(pSiteNull_NEW.anSiteNull);

                //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
                //to contain the values to bind to. This necessitates copying the 'column' values of pSite
                //into global memory with an SQLPOINTER pointer assigned to each one. FtSite provides
                //a convenience method that does exactly this.
                SQLPOINTER[] parameterValuePtr = pSiteStr_NEW.stSite.CopyToArrayOfSQLPOINTERinGlobalMemory();

                try
                {
                    //This will be used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                    int colNum;

                    // FtSite member #00  string  cmd
                    colNum = 1;
                    Ssutil.DbBindStringInput(hStmt, colNum, "cmd", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.cmd.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #01  string  recstat
                    colNum = 2;
                    Ssutil.DbBindStringInput(hStmt, colNum, "recstat", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.recstat.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #02  string  call1
                    colNum = 3;
                    Ssutil.DbBindStringInput(hStmt, colNum, "call1", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.call1.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #03  string  name
                    colNum = 4;
                    Ssutil.DbBindStringInput(hStmt, colNum, "name", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.name.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #04  string  prov
                    colNum = 5;
                    Ssutil.DbBindStringInput(hStmt, colNum, "prov", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.prov.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #05  string  oper
                    colNum = 6;
                    Ssutil.DbBindStringInput(hStmt, colNum, "oper", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.oper.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #06  int  latit
                    colNum = 7;
                    Ssutil.DbBindIntInput(hStmt, colNum, "latit", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #07  int  longit
                    colNum = 8;
                    Ssutil.DbBindIntInput(hStmt, colNum, "longit", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #08  float  grnd
                    colNum = 9;
                    Ssutil.DbBindFloatInput(hStmt, colNum, "grnd", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #09  string  stats
                    colNum = 10;
                    Ssutil.DbBindStringInput(hStmt, colNum, "stats", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.stats.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #10  string  sdate
                    colNum = 11;
                    Ssutil.DbBindStringInput(hStmt, colNum, "sdate", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.sdate.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #11  string  loc
                    colNum = 12;
                    Ssutil.DbBindStringInput(hStmt, colNum, "loc", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.loc.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #12  string  icaccount
                    colNum = 13;
                    Ssutil.DbBindStringInput(hStmt, colNum, "icaccount", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.icaccount.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #13  string  reg
                    colNum = 14;
                    Ssutil.DbBindStringInput(hStmt, colNum, "reg", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.reg.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #14  string  spoint
                    colNum = 15;
                    Ssutil.DbBindStringInput(hStmt, colNum, "spoint", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.spoint.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #15  string  nots
                    colNum = 16;
                    Ssutil.DbBindStringInput(hStmt, colNum, "nots", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.nots.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #16  string  oprtyp
                    colNum = 17;
                    Ssutil.DbBindStringInput(hStmt, colNum, "oprtyp", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.oprtyp.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #17  string  snumb
                    colNum = 18;
                    Ssutil.DbBindStringInput(hStmt, colNum, "snumb", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.snumb.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #18  short  notwr
                    colNum = 19;
                    Ssutil.DbBindShortInput(hStmt, colNum, "notwr", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #19  int  bandwd1
                    colNum = 20;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #20  int  bandwd2
                    colNum = 21;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #21  int  bandwd3
                    colNum = 22;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #22  int  bandwd4
                    colNum = 23;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd4", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #23  int  bandwd5
                    colNum = 24;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd5", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #24  int  bandwd6
                    colNum = 25;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd6", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #25  int  bandwd7
                    colNum = 26;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd7", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #26  int  bandwd8
                    colNum = 27;
                    Ssutil.DbBindIntInput(hStmt, colNum, "bandwd8", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                    // FtSite member #27  string  mdate
                    colNum = 28;
                    Ssutil.DbBindStringInput(hStmt, colNum, "mdate", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.mdate.Length, nullIndPtr[colNum - 1]);

                    // FtSite member #28  string  mtime
                    colNum = 29;
                    Ssutil.DbBindStringInput(hStmt, colNum, "mtime", parameterValuePtr[colNum - 1], (SQLULEN)pSiteStr_NEW.stSite.mtime.Length, nullIndPtr[colNum - 1]);

                }
                catch (Exception e)
                {
                    Log2.e("\r\nFtUtils.FtPutSiteWN(): ERROR: Exception caught");
                    Ssutil.DbGetDiagStmt(hStmt, "Error binding parameters for site on field: " + e.Message);
                    Ssutil.DisConnStmt(hConn, hStmt);
                    return -12;
                }

                // Execute the SQL insert query.
                sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Ssutil.DbGetDiagStmt(hStmt, "Error writing site: " + pSiteStr_NEW.stSite.call1);
                    //...Log2.v("\r\nFtUtils.FtPutSiteWN(): throws -11");
                    throw new Exception("-11");
                }

                // This concludes the update of the site information.

                // Next, update the antennas and the channels, as prescribed by the value of nDepth.
                if (nDepth > 1)
                {
                    //...Log2.v("\r\nFtUtils.FtPutSiteWN: putting antennae into DB");

                    // if nDepth > 1, the caller has asked for antennas and maybe channels.

                    // Update the antennae in the DB.

                    // Get the ante table name in cTab.
                    if (GenUtil.UtCvtName(Constant.FT_ANTE, cTable, out cTab) == Constant.FAILURE)
                    {
                        throw new Exception("-20");
                    }

                    // Delete all the existing antennae with he prescribed call sign.
                    cSQLBuff = String.Format(" call1='{0}'", pSiteStr_NEW.stSite.call1);
                    Ssutil.DbDeleteRows(cTab, cSQLBuff);

                    // Build the SQL insert query for the antennae updates.
                    cSQLBuff = FtAnte.BuildSqlInsertString(cTab);

                    //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
                    //to contain the values to bind to. This neccessitates copying the values of the nullInd
                    //array elements into global memory with an SQLLENPTR pointer assigned to each one.
                    //For efficiency, we will only do the parameter binding once for each field. We
                    //must set the nullInd values being pointed to prior to each SQL insert query.
                    SQLLEN[] nullInd = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL); ;
                    nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

                    //The ODBC library operates in 'native code', so we must use global (unmanaged) memory
                    //to contain the values to bind to. This necessitates copying the 'column' values of pAnte
                    //into global memory with an SQLPOINTER pointer assigned to each one. FtAnte provides
                    //a convenience method that does exactly this.
                    //For efficiency, we will only do the parameter binding once for each field. We
                    //must set the FtAnte field values being pointed to prior to each SQL insert query.
                    parameterValuePtr = FtAnte.AllocateArrayOfSQLPOINTERsWithMaxStringBuffers();

                    //It is more efficient to reuse statements than to drop them and allocate new ones. 
                    //When reusing statements, it is considered good-practice to release any bindings 
                    //previously applied to the hStmt; however the following code functions perfectly 
                    //well without it as 'numbered' bindings are just overwritten.
                    ODBC.SQLFreeStmt(hStmt, ODBC.SQL_RESET_PARAMS);

                    // Bind the parameters.
                    try
                    {
                        //This will be used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                        int N;

                        //   dbBindString(hStmt, 1, "cmd", pAnte->cmd, sizeof(pAnte->cmd), nullInd + FT_ANTE_CMD);
                        N = 1;
                        Ssutil.DbBindStringInput(hStmt, N, "cmd", parameterValuePtr[N - 1], Constant.CMD_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 2, "recstat", pAnte->recstat, sizeof(pAnte->recstat), nullInd + FT_ANTE_RECSTAT);
                        N = 2;
                        Ssutil.DbBindStringInput(hStmt, N, "recstat", parameterValuePtr[N - 1], Constant.RECSTAT_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 3, "call1", pAnte->call1, sizeof(pAnte->call1), nullInd + FT_ANTE_CALL1);
                        N = 3;
                        Ssutil.DbBindStringInput(hStmt, N, "call1", parameterValuePtr[N - 1], Constant.CALL_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 4, "call2", pAnte->call2, sizeof(pAnte->call2), nullInd + FT_ANTE_CALL2);
                        N = 4;
                        Ssutil.DbBindStringInput(hStmt, N, "call2", parameterValuePtr[N - 1], Constant.CALL_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 5, "bndcde", pAnte->bndcde, sizeof(pAnte->bndcde), nullInd + FT_ANTE_BNDCDE);
                        N = 5;
                        Ssutil.DbBindStringInput(hStmt, N, "bndcde", parameterValuePtr[N - 1], Constant.BNDCDE_SZ, nullIndPtr[N - 1]);

                        //    dbBindSShort(hStmt, 6, "anum", &pAnte->anum, nullInd + FT_ANTE_ANUM);
                        N = 6;
                        Ssutil.DbBindShortInput(hStmt, N, "anum", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 7, "ause", pAnte->ause, sizeof(pAnte->ause), nullInd + FT_ANTE_AUSE);
                        N = 7;
                        Ssutil.DbBindStringInput(hStmt, N, "ause", parameterValuePtr[N - 1], Constant.AUSE_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 8, "acode", pAnte->acode, sizeof(pAnte->acode), nullInd + FT_ANTE_ACODE);
                        N = 8;
                        Ssutil.DbBindStringInput(hStmt, N, "acode", parameterValuePtr[N - 1], Constant.FT_ANTE_ACODE_SZ, nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 9, "aht", &pAnte->aht, nullInd + FT_ANTE_AHT);
                        N = 9;
                        Ssutil.DbBindFloatInput(hStmt, N, "aht", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 10, "azmth", &pAnte->azmth, nullInd + FT_ANTE_AZMTH);
                        N = 10;
                        Ssutil.DbBindFloatInput(hStmt, N, "azmth", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 11, "elvtn", &pAnte->elvtn, nullInd + FT_ANTE_ELVTN);
                        N = 11;
                        Ssutil.DbBindFloatInput(hStmt, N, "elvtn", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 12, "dist", &pAnte->dist, nullInd + FT_ANTE_DIST);
                        N = 12;
                        Ssutil.DbBindFloatInput(hStmt, N, "dist", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 13, "offazm", pAnte->offazm, sizeof(pAnte->offazm), nullInd + FT_ANTE_OFFAZM);
                        N = 13;
                        Ssutil.DbBindStringInput(hStmt, N, "offazm", parameterValuePtr[N - 1], Constant.OFFAZM_SZ, nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 14, "tazmth", &pAnte->tazmth, nullInd + FT_ANTE_TAZMTH);
                        N = 14;
                        Ssutil.DbBindFloatInput(hStmt, N, "tazmth", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 15, "telvtn", &pAnte->telvtn, nullInd + FT_ANTE_TELVTN);
                        N = 15;
                        Ssutil.DbBindFloatInput(hStmt, N, "telvtn", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 16, "tgain", &pAnte->tgain, nullInd + FT_ANTE_TGAIN);
                        N = 16;
                        Ssutil.DbBindFloatInput(hStmt, N, "tgain", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 17, "txfdlnth", pAnte->txfdlnth, sizeof(pAnte->txfdlnth), nullInd + FT_ANTE_TXFDLNTH);
                        N = 17;
                        Ssutil.DbBindStringInput(hStmt, N, "txfdlnth", parameterValuePtr[N - 1], Constant.XFDLN_SZ, nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 18, "txfdlnlh", &pAnte->txfdlnlh, nullInd + FT_ANTE_TXFDLNLH);
                        N = 18;
                        Ssutil.DbBindFloatInput(hStmt, N, "txfdlnlh", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 19, "txfdlntv", pAnte->txfdlntv, sizeof(pAnte->txfdlntv), nullInd + FT_ANTE_TXFDLNTV);
                        N = 19;
                        Ssutil.DbBindStringInput(hStmt, N, "txfdlntv", parameterValuePtr[N - 1], Constant.XFDLN_SZ, nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 20, "txfdlnlv", &pAnte->txfdlnlv, nullInd + FT_ANTE_TXFDLNLV);
                        N = 20;
                        Ssutil.DbBindFloatInput(hStmt, N, "txfdlnlv", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 21, "rxfdlnth", pAnte->rxfdlnth, sizeof(pAnte->rxfdlnth), nullInd + FT_ANTE_RXFDLNTH);
                        N = 21;
                        Ssutil.DbBindStringInput(hStmt, N, "rxfdlnth", parameterValuePtr[N - 1], Constant.XFDLN_SZ, nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 22, "rxfdlnlh", &pAnte->rxfdlnlh, nullInd + FT_ANTE_RXFDLNLH);
                        N = 22;
                        Ssutil.DbBindFloatInput(hStmt, N, "rxfdlnlh", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 23, "rxfdlntv", pAnte->rxfdlntv, sizeof(pAnte->rxfdlntv), nullInd + FT_ANTE_RXFDLNTV);
                        N = 23;
                        Ssutil.DbBindStringInput(hStmt, N, "rxfdlntv", parameterValuePtr[N - 1], Constant.XFDLN_SZ, nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 24, "rxfdlnlv", &pAnte->rxfdlnlv, nullInd + FT_ANTE_RXFDLNLV);
                        N = 24;
                        Ssutil.DbBindFloatInput(hStmt, N, "rxfdlnlv", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 25, "txpadpam", &pAnte->txpadpam, nullInd + FT_ANTE_TXPADPAM);
                        N = 25;
                        Ssutil.DbBindFloatInput(hStmt, N, "txpadpam", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 26, "rxpadlna", &pAnte->rxpadlna, nullInd + FT_ANTE_RXPADLNA);
                        N = 26;
                        Ssutil.DbBindFloatInput(hStmt, N, "rxpadlna", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 27, "txcompl", &pAnte->txcompl, nullInd + FT_ANTE_TXCOMPL);
                        N = 27;
                        Ssutil.DbBindFloatInput(hStmt, N, "txcompl", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 28, "rxcompl", &pAnte->rxcompl, nullInd + FT_ANTE_RXCOMPL);
                        N = 28;
                        Ssutil.DbBindFloatInput(hStmt, N, "rxcompl", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 29, "obsloss", &pAnte->obsloss, nullInd + FT_ANTE_OBSLOSS);
                        N = 29;
                        Ssutil.DbBindFloatInput(hStmt, N, "obsloss", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindFloat(hStmt, 30, "kvalue", &pAnte->kvalue, nullInd + FT_ANTE_KVALUE);
                        N = 30;
                        Ssutil.DbBindFloatInput(hStmt, N, "kvalue", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindSShort(hStmt, 31, "atwrno", &pAnte->atwrno, nullInd + FT_ANTE_ATWRNO);
                        N = 31;
                        Ssutil.DbBindShortInput(hStmt, N, "atwrno", parameterValuePtr[N - 1], nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 32, "nota", pAnte->nota, sizeof(pAnte->nota), nullInd + FT_ANTE_NOTA);
                        N = 32;
                        Ssutil.DbBindStringInput(hStmt, N, "nota", parameterValuePtr[N - 1], Constant.NOTA_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 33, "apoint", pAnte->apoint, sizeof(pAnte->apoint), nullInd + FT_ANTE_APOINT);
                        N = 33;
                        Ssutil.DbBindStringInput(hStmt, N, "apoint", parameterValuePtr[N - 1], Constant.APOINT_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 34, "sdate", pAnte->sdate, sizeof(pAnte->sdate), nullInd + FT_ANTE_SDATE);
                        N = 34;
                        Ssutil.DbBindStringInput(hStmt, N, "sdate", parameterValuePtr[N - 1], Constant.DATE_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 35, "licence", pAnte->licence, sizeof(pAnte->licence), nullInd + FT_ANTE_LICENCE);
                        N = 35;
                        Ssutil.DbBindStringInput(hStmt, N, "licence", parameterValuePtr[N - 1], Constant.LICENCE_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 36, "mdate", pAnte->mdate, sizeof(pAnte->mdate), nullInd + FT_ANTE_MDATE);
                        N = 36;
                        Ssutil.DbBindStringInput(hStmt, N, "mdate", parameterValuePtr[N - 1], Constant.DATE_SZ, nullIndPtr[N - 1]);

                        //    dbBindString(hStmt, 37, "mtime", pAnte->mtime, sizeof(pAnte->mtime), nullInd + FT_ANTE_MTIME);
                        N = 37;
                        Ssutil.DbBindStringInput(hStmt, N, "mtime", parameterValuePtr[N - 1], Constant.TIME_SZ, nullIndPtr[N - 1]);
                    }
                    catch (Exception e)
                    {
                        Ssutil.DbGetDiagStmt(hStmt, "ftPutSiteWN: Error binding antenna field: " + e.Message + ".");
                        throw new Exception("-22");
                    }

                    // Now for the clever part ... iterate over all antennae to be updated by
                    // re-using the (already) bound parameterValuePtr[] and nullIndPtr[] arrays elements.
                    for (nInd = 0; nInd < pSiteStr_NEW.nNumAnts; nInd++)
                    {
                        //...Log2.v("\r\nFtUtils.FtPutSiteWN(): putting antenna nInd = " + nInd + " of {" + pSiteStr_NEW.nNumAnts + "}");

                        // Copy FtAnte values into the global memory used for their SQL bindings.
                        FtAnte ftAnte = pSiteStr_NEW.stAntsPtr[nInd];
                        ftAnte.CopyToExistingArrayOfSQLPOINTERs(parameterValuePtr);

                        // Copy corresponding nullInd values into the global memory used for their SQL bindings.
                        SQLLEN[] nullInds = pSiteNull_NEW.anAntsNullPtr[nInd];
                        NullHelper.CopyNullIndsToSQLPOINTERArray(nullInds, nullIndPtr);

                        sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

                        if (!ODBC.IsOK(sqlRet))
                        {
                            /*  Error on insert */
                            Ssutil.DbGetDiagStmt(hStmt, "ftPutSiteWN: Error executing antenna insert.");
                            throw new Exception("-23");
                        }
                    }

                    // We have now completed the update of antenae information.

                    // Update the channel information if prescribed by the value of nDepth.
                    if (nDepth >= 3)
                    {
                        //...Log2.v("\r\nFtUtils.FtPutSiteWN: putting channels into DB");

                        // Get the channel table name in cTab.
                        if (GenUtil.UtCvtName(Constant.FT_CHAN, cTable, out cTab) == Constant.FAILURE)
                        {
                            throw new Exception("-30");
                        }

                        // Delete any existing channels with the prescribed call sign.
                        cSQLBuff = String.Format(" call1='{0}'", pSiteStr_NEW.stSite.call1);
                        Ssutil.DbDeleteRows(cTab, cSQLBuff);

                        // Build the string for the channel insert query.
                        cSQLBuff = FtChan.BuildSqlInsertString(cTab);

                        // For efficiency, we will only do the parameter binding once for each field.
                        // We must set the nullInd values being pointed to prior to the SQL insert query.
                        nullInd = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NOT_NULL); ;
                        nullIndPtr = NullHelper.CreateArrayOfSQLLENPTRinGlobalMemory(nullInd);

                        // For efficiency, we will only do the parameter binding once for each field.
                        // We must set the FtChan fields being pointed to prior to the SQL insert query.
                        parameterValuePtr = FtChan.AllocateArrayOfSQLPOINTERsToBuffers();

                        //It is more efficient to reuse statements than to drop them and allocate new ones. 
                        //When reusing statements, it is considered good-practice to release any bindings 
                        //previously applied to the hStmt; however the following code functions perfectly 
                        //well without it as 'numbered' bindings are just overwritten.

                        // Bind the parameters.
                        try
                        {
                            //This integer is used to enumerate the binding sequence; ODBC definition is that first binding is N = 1;
                            int colNum;

                            // FtChan member #0  string  cmd
                            colNum = 1;
                            Ssutil.DbBindStringInput(hStmt, colNum, "cmd", parameterValuePtr[colNum - 1], Constant.CMD_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #1  string  recstat
                            colNum = 2;
                            Ssutil.DbBindStringInput(hStmt, colNum, "recstat", parameterValuePtr[colNum - 1], Constant.RECSTAT_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #2  string  call1
                            colNum = 3;
                            Ssutil.DbBindStringInput(hStmt, colNum, "call1", parameterValuePtr[colNum - 1], Constant.CALL_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #3  string  call2
                            colNum = 4;
                            Ssutil.DbBindStringInput(hStmt, colNum, "call2", parameterValuePtr[colNum - 1], Constant.CALL_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #4  string  bndcde
                            colNum = 5;
                            Ssutil.DbBindStringInput(hStmt, colNum, "bndcde", parameterValuePtr[colNum - 1], Constant.BNDCDE_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #5  string  splan
                            colNum = 6;
                            Ssutil.DbBindStringInput(hStmt, colNum, "splan", parameterValuePtr[colNum - 1], Constant.PLAN_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #6  short  hl
                            colNum = 7;
                            Ssutil.DbBindShortInput(hStmt, colNum, "hl", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #7  short  vh
                            colNum = 8;
                            Ssutil.DbBindShortInput(hStmt, colNum, "vh", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #8  string  chid
                            colNum = 9;
                            Ssutil.DbBindStringInput(hStmt, colNum, "chid", parameterValuePtr[colNum - 1], Constant.CHID_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #9  double  freqtx
                            colNum = 10;
                            Ssutil.DbBindDoubleInput(hStmt, colNum, "freqtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #10  string  poltx
                            colNum = 11;
                            Ssutil.DbBindStringInput(hStmt, colNum, "poltx", parameterValuePtr[colNum - 1], Constant.POLTX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #11  short  antnumbtx1
                            colNum = 12;
                            Ssutil.DbBindShortInput(hStmt, colNum, "antnumbtx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #12  short  antnumbtx2
                            colNum = 13;
                            Ssutil.DbBindShortInput(hStmt, colNum, "antnumbtx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #13  string  eqpttx
                            colNum = 14;
                            Ssutil.DbBindStringInput(hStmt, colNum, "eqpttx", parameterValuePtr[colNum - 1], Constant.EQPTTX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #14  string  eqptutx
                            colNum = 15;
                            Ssutil.DbBindStringInput(hStmt, colNum, "eqptutx", parameterValuePtr[colNum - 1], Constant.EQPTUTX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #15  float  pwrtx
                            colNum = 16;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "pwrtx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #16  float  atpccde
                            colNum = 17;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "atpccde", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #17  float  afsltx1
                            colNum = 18;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "afsltx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #18  float  afsltx2
                            colNum = 19;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "afsltx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #19  string  traftx
                            colNum = 20;
                            Ssutil.DbBindStringInput(hStmt, colNum, "traftx", parameterValuePtr[colNum - 1], Constant.TRAFTX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #20  string  srvctx
                            colNum = 21;
                            Ssutil.DbBindStringInput(hStmt, colNum, "srvctx", parameterValuePtr[colNum - 1], Constant.SRVCTX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #21  string  stattx
                            colNum = 22;
                            Ssutil.DbBindStringInput(hStmt, colNum, "stattx", parameterValuePtr[colNum - 1], Constant.STATTX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #22  double  freqrx
                            colNum = 23;
                            Ssutil.DbBindDoubleInput(hStmt, colNum, "freqrx", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #23  string  polrx
                            colNum = 24;
                            Ssutil.DbBindStringInput(hStmt, colNum, "polrx", parameterValuePtr[colNum - 1], Constant.POLRX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #24  short  antnumbrx1
                            colNum = 25;
                            Ssutil.DbBindShortInput(hStmt, colNum, "antnumbrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #25  short  antnumbrx2
                            colNum = 26;
                            Ssutil.DbBindShortInput(hStmt, colNum, "antnumbrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #26  short  antnumbrx3
                            colNum = 27;
                            Ssutil.DbBindShortInput(hStmt, colNum, "antnumbrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #27  string  eqptrx
                            colNum = 28;
                            Ssutil.DbBindStringInput(hStmt, colNum, "eqptrx", parameterValuePtr[colNum - 1], Constant.EQPTRX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #28  string  eqpturx
                            colNum = 29;
                            Ssutil.DbBindStringInput(hStmt, colNum, "eqpturx", parameterValuePtr[colNum - 1], Constant.EQPTURX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #29  float  afslrx1
                            colNum = 30;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "afslrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #30  float  afslrx2
                            colNum = 31;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "afslrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #31  float  afslrx3
                            colNum = 32;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "afslrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #32  float  pwrrx1
                            colNum = 33;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "pwrrx1", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #33  float  pwrrx2
                            colNum = 34;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "pwrrx2", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #34  float  pwrrx3
                            colNum = 35;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "pwrrx3", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #35  string  trafrx
                            colNum = 36;
                            Ssutil.DbBindStringInput(hStmt, colNum, "trafrx", parameterValuePtr[colNum - 1], Constant.TRAFCODE_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #36  float  esint
                            colNum = 37;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "esint", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #37  float  tsint
                            colNum = 38;
                            Ssutil.DbBindFloatInput(hStmt, colNum, "tsint", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #38  string  srvcrx
                            colNum = 39;
                            Ssutil.DbBindStringInput(hStmt, colNum, "srvcrx", parameterValuePtr[colNum - 1], Constant.TRAFCODE_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #39  string  statrx
                            colNum = 40;
                            Ssutil.DbBindStringInput(hStmt, colNum, "statrx", parameterValuePtr[colNum - 1], Constant.STATRX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #40  string  routnumb
                            colNum = 41;
                            Ssutil.DbBindStringInput(hStmt, colNum, "routnumb", parameterValuePtr[colNum - 1], Constant.ROUTE_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #41  short  stnnumb
                            colNum = 42;
                            Ssutil.DbBindShortInput(hStmt, colNum, "stnnumb", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #42  short  hopnumb
                            colNum = 43;
                            Ssutil.DbBindShortInput(hStmt, colNum, "hopnumb", parameterValuePtr[colNum - 1], nullIndPtr[colNum - 1]);

                            // FtChan member #43  string  sdate
                            colNum = 44;
                            Ssutil.DbBindStringInput(hStmt, colNum, "sdate", parameterValuePtr[colNum - 1], Constant.DATE_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #44  string  notetx
                            colNum = 45;
                            Ssutil.DbBindStringInput(hStmt, colNum, "notetx", parameterValuePtr[colNum - 1], Constant.NOTA_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #45  string  noterx
                            colNum = 46;
                            Ssutil.DbBindStringInput(hStmt, colNum, "noterx", parameterValuePtr[colNum - 1], Constant.NOTA_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #46  string  notegnl
                            colNum = 47;
                            Ssutil.DbBindStringInput(hStmt, colNum, "notegnl", parameterValuePtr[colNum - 1], Constant.NOTA_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #47  string  cpoint
                            colNum = 48;
                            Ssutil.DbBindStringInput(hStmt, colNum, "cpoint", parameterValuePtr[colNum - 1], Constant.CPOINT_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #48  string  feetx
                            colNum = 49;
                            Ssutil.DbBindStringInput(hStmt, colNum, "feetx", parameterValuePtr[colNum - 1], Constant.FEETX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #49  string  feerx
                            colNum = 50;
                            Ssutil.DbBindStringInput(hStmt, colNum, "feerx", parameterValuePtr[colNum - 1], Constant.FEERX_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #50  string  mdate
                            colNum = 51;
                            Ssutil.DbBindStringInput(hStmt, colNum, "mdate", parameterValuePtr[colNum - 1], Constant.DATE_SZ, nullIndPtr[colNum - 1]);

                            // FtChan member #51  string  mtime
                            colNum = 52;
                            Ssutil.DbBindStringInput(hStmt, colNum, "mtime", parameterValuePtr[colNum - 1], Constant.TIME_SZ, nullIndPtr[colNum - 1]);
                        }
                        catch (Exception e)
                        {
                            Ssutil.DbGetDiagStmt(hStmt, "ftPutSiteWN: Could not bind channels at column: " + e.Message);
                            throw new Exception("-23");
                        }

                        // Now for the clever part ... iterate over all channels to be updated by
                        // re-using the (already) bound parameterValuePtr[] and nullIndPtr[] arrays elements.
                        for (nInd = 0; nInd < pSiteStr_NEW.nNumChans; nInd++)
                        {
                            // Copy FtChan values into the global memory used for their SQL bindings.
                            FtChan ftChan = pSiteStr_NEW.stChanPtr[nInd];
                            ftChan.CopyToExistingArrayOfSQLPOINTERs(parameterValuePtr);

                            // Copy corresponding nullInd values into the global memory used for their SQL bindings.
                            SQLLEN[] nullInds = pSiteNull_NEW.anChanNullPtr[nInd];
                            NullHelper.CopyNullIndsToSQLPOINTERArray(nullInds, nullIndPtr);

                            sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

                            if (!ODBC.IsOK(sqlRet))
                            {
                                /*  Error on insert */
                                string str = String.Format("ftPutSiteWN: Error writing Channel: {0} {1} {2} {3}",
                                                                ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);
                                Ssutil.DbGetDiagStmt(hStmt, str);
                                throw new Exception("-25");
                            }
                        } //nInd
                    } //if (nDepth >= 3)
                } //if (nDepth > 1)
            }
            catch (Exception e)
            {
                nRet = Int32.Parse(e.Message);
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            //...Log2.v("\n\nFtUtils.FtPutSiteWN: Exit:  returned " + nRet + "\r\n");
            return (nRet);
        }

        /// <summary>
        /// Provides an array of all the call signs found in a prescribed DB FT_SITE table.
        /// </summary>
        /// <param name="cInTable"> - name of PDF.</param>
        /// <param name="cCallFound"> - array of call signs found.</param>
        /// <returns></returns>
        /// <para>- 0 - no call signs found.</para>
        /// <para>- > 0 - the number of call signs found.</para>
        /// <para>- < 0 - call failed.</para>
        public static int FtEnumCallSigns(string cInTable, out string[] cCallFound)
        {
            cCallFound = null;
            int nCount = 0;
            string cCall;
            string cTableName;
            string cTable;
            string cSQL;
            SQLLEN nullInd;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn;

            FtGetTableName(out cTable, cInTable);

            GenUtil.UtCvtName(Constant.FT_SITE, cTable, out cTableName);

            nCount = Ssutil.DbCountRows(cTableName, "");
            if (nCount < 0)
            {
                //	There was an error counting rows.
                return -2;
            }

            cCallFound = new string[nCount];

            cSQL = String.Format(" select call1 from {0} ", cTableName);

            hConn = Ssutil.NewConn();
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            for (int nInd = 0; nInd < nCount; nInd++)
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (!ODBC.IsOK(sqlRet))
                {
                    // This should not happen if DbCountRows() returned the correct value.
                    return -666;
                }
                else
                {
                    Ssutil.DbGetString(hStmt, 1, "call1", out cCall, Constant.CALLSIGN_SZ, out nullInd);

                    cCallFound[nInd] = cCall;
                }

            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

            Ssutil.DisConn(hConn);

            return nCount;
        }

        /// <summary>
        /// Retrieves a TS SITE record from a PDF FT_SITE table in the DB.
        /// The caller prescribes the 'depth' of retrieval, i.e. 'site only', site and antennae' 
        /// or 'site, antennae and channels'.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site and antennas will be returned,
        /// If nDepth = 3 the site, antennas and channels will be returned.
        /// </remarks>
        /// <param name="cCall"> - call sign of the SITE to be retrieved.</param>
        /// <param name="pSite"> - a FtSiteStr_NEW object populated with data.</param>
        /// <param name="nDepth"> - 1, 2 or 3 (see remarks above).</param>
        /// <param name="cTable"> - PDF table name.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB..</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- ErrorMessages.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- ErrorMessages.UTCVTNAME_FAILED - call to GenUtil.UtCvtName() failed.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILED - ODBC SQLAllocHandle() failed.</para>
        /// <para>- ErrorMessages.ODBC_EXECDIRECT_FAILED    - call to ODBC.SQLExecDirect() failed. </para>
        public static int FtGetSite(string cCall, out FtSiteStr pSite, int nDepth, string cTable)
        {
            //...Log2.v("\n\nFtUtils.FtGetSite(): Entry");

            FtSiteStrNulls anNulls;      /*  Pointer to the array of nulls */
            int nRet;

            nRet = FtGetSiteWN(cCall, out pSite, nDepth, cTable, out anNulls);

            //...Log2.v("\n\nFtUtils.FtGetSite(): Exit");
            return nRet;
        }

        /// <summary>
        /// Check whether a prescribed FtSiteStr object contains an antenna with
        /// prescribed call2, band code, and anum.
        /// </summary>
        /// <param name="pSite"> - FtSiteStr_NEW object to be searched.</param>
        /// <param name="cCall2"> - prescribed call sign.</param>
        /// <param name="bndcde"> - prescribed band code.</param>
        /// <param name="nAnum"> - prescribed antenna number.</param>
        /// <returns></returns>
        /// <para> non-negative - the index of the FtAnte array element that is the first match found.</para>
        /// <para> negative - no match found.</para>
        public static int FtFindAnt(FtSiteStr pSite, string cCall2, string bndcde, int nAnum)
        {
            int nInd;
            int nRet = Constant.FAILURE;

            FtAnte pAnt;

            for (nInd = 0; nInd < pSite.nNumAnts; nInd++)
            {
                pAnt = pSite.stAntsPtr[nInd];
                if (pAnt.call2.Equals(cCall2) && pAnt.bndcde.Equals(bndcde) && pAnt.anum == nAnum)
                {
                    nRet = nInd;
                    break;
                }
            }

            return nRet;
        }

        /// <summary>
        /// This method returns the FtAnte object contained within a prescribed FtSiteStr object
        /// that has prescribed call2, band code, and anum member values.
        /// </summary>
        /// <param name="ftSiteStr"> - FtSiteStr_NEW object to be searched.</param>
        /// <param name="cCall1"> - prescribed call sign.</param>
        /// <param name="bndcde"> - prescribed band code.</param>
        /// <param name="nAnum"> - prescribed antenna number.</param>
        /// <returns></returns>
        /// <para> non-null - the found FtAnte object.</para>
        /// <para> negative - no matching FtAnte object found.</para>
        public static FtAnte GetFtAnte(FtSiteStr ftSiteStr, string cCall1, string bndcde, int nAnum)
        {
            FtAnte ftAnte = null;
            int nInd;
            FtAnte pAnt;

            for (nInd = 0; nInd < ftSiteStr.nNumAnts; nInd++)
            {
                pAnt = ftSiteStr.stAntsPtr[nInd];

                if (pAnt.call2.Equals(cCall1) && pAnt.bndcde.Equals(bndcde) && pAnt.anum == nAnum)
                {
                    ftAnte = pAnt;
                    break;
                }
            }

            return ftAnte;
        }

        /// <summary>
        /// Returns the index of the next element in an array of channels that has
        /// prescribed call2, band, and anum. The first call should be made with 
        /// nLastOne set to -1.
        /// </summary>
        /// <param name="pChan"> - an array of FtChan objects.</param>
        /// <param name="nChanCount"> - number of channels.</param>
        /// <param name="cCall2"> - prescribed call2.</param>
        /// <param name="cBand"> - precribed band code.</param>
        /// <param name="nAnum"> - prescribed antenna number.</param>
        /// <param name="nLastOne"> - index returned by previous call.</param>
        /// <returns></returns>
        /// <para>- non-negative - index of next channel objec that satisfies prescribed criteria.</para>
        /// <para>- Constant.FAILURE - no match found.</para>
        public static int FtNextChan(FtChan[] pChan, int nChanCount, string cCall2, string cBand, short nAnum, int nLastOne)
        {
            int nRet = Constant.FAILURE; // was -2

            for (++nLastOne; nLastOne < nChanCount; nLastOne++)
            {
                if (pChan[nLastOne].call2.Equals(cCall2) && pChan[nLastOne].bndcde.Equals(cBand) &&
                        (pChan[nLastOne].antnumbtx1 == nAnum ||
                         pChan[nLastOne].antnumbtx2 == nAnum ||
                         pChan[nLastOne].antnumbrx1 == nAnum ||
                         pChan[nLastOne].antnumbrx2 == nAnum ||
                         pChan[nLastOne].antnumbrx3 == nAnum))
                {
                    /*  We got one. */
                    nRet = nLastOne;
                    break;
                }
            }
            return nRet;
        }

        /// <summary>
        /// Searches a FtSiteStr object for a channel with prescribed call2, band, and chid.
        /// </summary>
        /// <param name="pSite"> - FtSiteStr_NEW object to search.</param>
        /// <param name="cCall2"> - prescribed call2.</param>
        /// <param name="bndcde"> - prescribed band code.</param>
        /// <param name="cChid"> - prescribed channel ID.</param>
        /// <returns></returns>
        /// <para>- non-negative - index of the first element of the channel array that satisfies the precribed criteria.</para>
        /// <para>- Constant.FAILURE - no match found.</para>
        public static int FtFindChanChid(FtSiteStr pSite, string cCall2, string bndcde, string cChid)
        {
            //...Log2.v("\n\nFtUtils.FtFindChanChid(): Entry");

            int nInd;
            int nRet = Constant.FAILURE;

            for (nInd = 0; nInd < pSite.nNumChans; nInd++)
            {
                //...Log2.v("\r\nFtUtils.FtFindChanChid(): nInd = " + nInd);

                /*  Channels are unique over callsigns, bandcodes and chids,
                        all the Channels will have this same call1 though */
                FtChan pSiteChan = pSite.stChanPtr[nInd];

                //...Log2.v("\r\nFtUtils.FtFindChanChid(): pSiteChan.call2 =  |" + pSiteChan.call2 + "|");
                //...Log2.v("\r\nFtUtils.FtFindChanChid(): pSiteChan.bndcde = |" + pSiteChan.bndcde + "|");
                //...Log2.v("\r\nFtUtils.FtFindChanChid(): pSiteChan.chid =   |" + pSiteChan.chid + "|");

                if (pSiteChan.call2.Equals(cCall2) && pSiteChan.bndcde.Equals(bndcde) && pSiteChan.chid.Equals(cChid))
                {
                    //...Log2.v("\r\nFtUtils.FtFindChanChid(): A");
                    /*  Found it.  Replace this one */
                    nRet = nInd;
                    break;
                }
            }
            //...Log2.v("\n\nFtUtils.FtFindChanChid(): Exit, returns " + nRet);
            return nRet;
        }

        /// <summary>
        /// Provides a string giving the SQL 'WHERE' parameters that 
        /// can be used to check for band adjacency.
        /// </summary>
        /// <param name="ftSite"> - the FtSite object being analyzed for band adjacency.</param>
        /// <param name="cClause"> - 'WHERE' parameters to use.</param>
        /// <param name="withAdjacent"> - set to 'true' analyze the full site adjacency array.</param>
        public static void GenBandClause(FtSite ftSite, out string cClause, bool withAdjacent)
        {
            //...Log2.v(String.Format("\nFtUtils.GenBandClause(): Entry: FtSite = {0}, withAdjacent = {1}", ftSite.call1, withAdjacent));
            //...Log2.v(String.Format("\n\nFtUtils.GenBandClause(): ftSite BandBits:\n{0}", ftSite.GetBandBits().First64BitsAsBinaryWithRuler()));
            //...Log2.v("\n" + Suutils.BandBitsToBndcdeCSV(ftSite.GetBandBits()));
            
            // Satisfy 'out' requirement.
            cClause = "";

            BandBits sBandBits;
            string cOr;
            int nInd;
            string cOneBand;

            if (withAdjacent)
            {
                //	Get the full site adjacency array.
                FtSiteAdjBands(ftSite.GetBandWdArray(), out sBandBits);

                //...Log2.v(String.Format("\nFtUtils.GenBandClause(): withAdjacent is  TRUE: sBandBits = \n{0}\n{1}", BandBits.Ruler64Bits(), sBandBits.First64BitsToString()));
            }
            else
            {
                //	Just move in the original without any adjacency bits.
                //  memmove(sBandBits.bitarray, &(stSite.bandwd1), sizeof(sBandBits.bitarray));
                sBandBits = new BandBits();
                sBandBits.bitArray = ftSite.GetBandWdArray();

                //...Log2.v(String.Format("\nFtUtils.GenBandClause(): withAdjacent is FALSE: sBandBits = \n{0}\n{1}", BandBits.Ruler64Bits(), sBandBits.First64BitsToString()));

            }

            //	Set the bandword search...
            cOr = "";
            for (nInd = 0; nInd < Constant.FT_BANDWD_CT; nInd++)
            {
                if (sBandBits.bitArray[nInd] != 0)
                {
                    cOneBand = String.Format("{0}bandwd{1} & 0x{2:x8} != 0",
                                cOr, nInd + 1, sBandBits.bitArray[nInd]);
                    cOr = " or ";
                    cClause += cOneBand;
                }
            }

            // Ongoing bug:  b200911A.
            // It is possible that, at this point, cClause is an empty string that then gets
            // used to build a SQL query WHERE clause element of the form '()' which is invalid
            // SQL syntax.
            //
            // To remedy this, if cClause is an empty string we will reset it to "1!=1" which
            // SQL always decodes as 'false'.
            if (String.IsNullOrWhiteSpace(cClause))
            {
Log2.Write("\n\nFtUtils.GenBandClause(): withAdjacent = {0}; IsNullOrWhiteSpace(cClause) returned true for site: {1}; ", withAdjacent, ftSite.call1);
                //AH: REMOVE!
                //cClause = "1!=1";    <== Not authorized to implement this bug fix yet.
            }

            //...Log2.v(String.Format("\nFtUtils.GenBandClause(): sBandBits:\n{0}", sBandBits.First64BitsAsBinaryWithRuler()));
            //...Log2.v("\n" + Suutils.BandBitsToBndcdeCSV(sBandBits));
            //...Log2.v(String.Format("\nFtUtils.GenBandClause(): Exit: call1 = {0}, cClause = |{1}|\n", ftSite.call1, cClause));

            return;
        }

        /// <summary>
        /// Given an array of bandwds (it may be the bandwds in the site
        /// structure so they must be ordered and together) this method creates a 
        /// BandBits object populated using the input array and all adjacent bands.
        /// </summary>
        /// <param name="aBandWds"> - array of bandwds.</param>
        /// <param name="pBitArray"> - populated BandBits object.</param>
        public static void FtSiteAdjBands(uint[] aBandWds, out BandBits pBitArray)
        {
            //...Log2.v("\nFtUtils.FtSiteAdjBands(): Entry: aBandWds:");
            //...Log2.v(String.Format("\n{0}\n{1}", BandBits.Ruler64Bits(), new BandBits(aBandWds).First64BitsToString()));

            int aBitPos;
            SdBand curBand;
            BandBits sBandAdj;
            int nBandCount = Suutils.SuNumberOfBands();

            // Create and zero the output array.
            pBitArray = new BandBits();

            /* for each interferer band bit position.  */
            for (aBitPos = 1; aBitPos <= nBandCount; aBitPos++)
            {
                /* is band used in these bandwords? */
                if (GenUtil.UtTestBit(aBandWds, aBitPos - 1) == Enums.BIT.SET)
                {
                    //...Log2.v("\nFtUtils.FtSiteAdjBands(): aBitPos = " + aBitPos);

                    /* Yes, Get band and its adjacency bits. */
                    if (Suutils.SdGetBandfromBit(aBitPos, out curBand) == Constant.SUCCESS)
                    {
                        //	We have the band for this bit position.  Get the adjacency 
                        Suutils.SuAdjBands(curBand.bndcde, out sBandAdj);
                        Suutils.SuBandBitsOR(ref pBitArray, sBandAdj);
                    }
                }
            }

            //...Log2.v("\nFtUtils.FtSiteAdjBands(): Exit");
        } /*---- end siteAdjBands ----*/

        /// <summary>
        /// Read a single antenna. This is out of band as well.  
        /// </summary>
        /// <param name="cCall1"></param>
        /// <param name="cCall2"></param>
        /// <param name="cBand"></param>
        /// <param name="anum"></param>
        /// <param name="cTableName"></param>
        /// <param name="ftAnte"></param>
        /// <param name="ftAnteNulls"></param>
        /// <returns></returns>
        public static int FtReadAnte(string cCall1, string cCall2, string cBand, int anum,
    string cTableName, out FtAnte ftAnte, out SQLLEN[] ftAnteNulls)
        {
            // 'out' requirement.
            ftAnte = null;
            ftAnteNulls = null;

            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLRETURN sqlRet;

            string cSQLBuff;
            int nRet = 0;
            string cDBTableName;

            //GenUtil.UtCvtName(Constant.FT_ANTE, cTableName, out cDBTableName);  /*  Get the ante table name
            cDBTableName = cTableName;

            /*	Allocate space and read in the antennas.  We use the ingres
                    begin/end sequence for this.  This is proprietary, but the
                    preparation and execution of cursors that would be needed
                    (because the tablename has to be changed each time) was not
                    felt to be worth it, and the actual work is very localized,
                    meaning that it could be changed easily at any time.
            */
            ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQLBuff = String.Format(SELECT, cDBTableName, cCall1, cCall2, cBand, anum);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                //	Something went wrong on the exec.
                Log2.e("\nFtUtils.FtReadAnte(): ERROR: SQLExecDirect() failed.");
                Ssutil.DbGetDiagStmt(hStmt, "ftReadAnte04 - Error retrieving antenna.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            // Now fetch the antenna
            sqlRet = ODBC.SQLFetch(hStmt);
            if (sqlRet == ODBC.SQL_NO_DATA)
            {
                nRet = Constant.NOMORERECS;
            }
            else if (!ODBC.IsOK(sqlRet))
            {
                /*  Something has gone terribly wrong! */
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                nRet = Error.ODBC_FETCH_FAILED;
            }
            else
            {
                /*  Got the antenna.  move it into the array.  Zero the array first */
                ftAnte = new FtAnte();
                ftAnteNulls = NullHelper.CreateArrayOfNullInd(FtAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                try
                {
                    // Initialize auto-indexing.
                    Ssutil.DbStartGets();

                    Ssutil.DbGetString(hStmt, 0, "cmd", out ftAnte.cmd, FtAnte.CMD_SZ, out ftAnteNulls[FtAnte.CMD]);
                    Ssutil.DbGetString(hStmt, 0, "recstat", out ftAnte.recstat, FtAnte.RECSTAT_SZ, out ftAnteNulls[FtAnte.RECSTAT]);
                    Ssutil.DbGetString(hStmt, 0, "call1", out ftAnte.call1, FtAnte.CALL1_SZ, out ftAnteNulls[FtAnte.CALL1]);
                    Ssutil.DbGetString(hStmt, 0, "call2", out ftAnte.call2, FtAnte.CALL2_SZ, out ftAnteNulls[FtAnte.CALL2]);
                    Ssutil.DbGetString(hStmt, 0, "bndcde", out ftAnte.bndcde, FtAnte.BNDCDE_SZ, out ftAnteNulls[FtAnte.BNDCDE]);
                    Ssutil.DbGetShort(hStmt, 0, "anum", out ftAnte.anum, out ftAnteNulls[FtAnte.ANUM]);
                    Ssutil.DbGetString(hStmt, 0, "ause", out ftAnte.ause, FtAnte.AUSE_SZ, out ftAnteNulls[FtAnte.AUSE]);
                    Ssutil.DbGetString(hStmt, 0, "acode", out ftAnte.acode, FtAnte.ACODE_SZ, out ftAnteNulls[FtAnte.ACODE]);
                    Ssutil.DbGetFloat(hStmt, 0, "aht", out ftAnte.aht, out ftAnteNulls[FtAnte.AHT]);
                    Ssutil.DbGetFloat(hStmt, 0, "azmth", out ftAnte.azmth, out ftAnteNulls[FtAnte.AZMTH]);
                    Ssutil.DbGetFloat(hStmt, 0, "elvtn", out ftAnte.elvtn, out ftAnteNulls[FtAnte.ELVTN]);
                    Ssutil.DbGetFloat(hStmt, 0, "dist", out ftAnte.dist, out ftAnteNulls[FtAnte.DIST]);
                    Ssutil.DbGetString(hStmt, 0, "offazm", out ftAnte.offazm, FtAnte.OFFAZM_SZ, out ftAnteNulls[FtAnte.OFFAZM]);
                    Ssutil.DbGetFloat(hStmt, 0, "tazmth", out ftAnte.tazmth, out ftAnteNulls[FtAnte.TAZMTH]);
                    Ssutil.DbGetFloat(hStmt, 0, "telvtn", out ftAnte.telvtn, out ftAnteNulls[FtAnte.TELVTN]);
                    Ssutil.DbGetFloat(hStmt, 0, "tgain", out ftAnte.tgain, out ftAnteNulls[FtAnte.TGAIN]);
                    Ssutil.DbGetString(hStmt, 0, "txfdlnth", out ftAnte.txfdlnth, FtAnte.TXFDLNTH_SZ, out ftAnteNulls[FtAnte.TXFDLNTH]);
                    Ssutil.DbGetFloat(hStmt, 0, "txfdlnlh", out ftAnte.txfdlnlh, out ftAnteNulls[FtAnte.TXFDLNLH]);
                    Ssutil.DbGetString(hStmt, 0, "txfdlntv", out ftAnte.txfdlntv, FtAnte.TXFDLNTV_SZ, out ftAnteNulls[FtAnte.TXFDLNTV]);
                    Ssutil.DbGetFloat(hStmt, 0, "txfdlnlv", out ftAnte.txfdlnlv, out ftAnteNulls[FtAnte.TXFDLNLV]);
                    Ssutil.DbGetString(hStmt, 0, "rxfdlnth", out ftAnte.rxfdlnth, FtAnte.RXFDLNTH_SZ, out ftAnteNulls[FtAnte.RXFDLNTH]);
                    Ssutil.DbGetFloat(hStmt, 0, "rxfdlnlh", out ftAnte.rxfdlnlh, out ftAnteNulls[FtAnte.RXFDLNLH]);
                    Ssutil.DbGetString(hStmt, 0, "rxfdlntv", out ftAnte.rxfdlntv, FtAnte.RXFDLNTV_SZ, out ftAnteNulls[FtAnte.RXFDLNTV]);
                    Ssutil.DbGetFloat(hStmt, 0, "rxfdlnlv", out ftAnte.rxfdlnlv, out ftAnteNulls[FtAnte.RXFDLNLV]);
                    Ssutil.DbGetFloat(hStmt, 0, "txpadpam", out ftAnte.txpadpam, out ftAnteNulls[FtAnte.TXPADPAM]);
                    Ssutil.DbGetFloat(hStmt, 0, "rxpadlna", out ftAnte.rxpadlna, out ftAnteNulls[FtAnte.RXPADLNA]);
                    Ssutil.DbGetFloat(hStmt, 0, "txcompl", out ftAnte.txcompl, out ftAnteNulls[FtAnte.TXCOMPL]);
                    Ssutil.DbGetFloat(hStmt, 0, "rxcompl", out ftAnte.rxcompl, out ftAnteNulls[FtAnte.RXCOMPL]);
                    Ssutil.DbGetFloat(hStmt, 0, "obsloss", out ftAnte.obsloss, out ftAnteNulls[FtAnte.OBSLOSS]);
                    Ssutil.DbGetFloat(hStmt, 0, "kvalue", out ftAnte.kvalue, out ftAnteNulls[FtAnte.KVALUE]);
                    Ssutil.DbGetShort(hStmt, 0, "atwrno", out ftAnte.atwrno, out ftAnteNulls[FtAnte.ATWRNO]);
                    Ssutil.DbGetString(hStmt, 0, "nota", out ftAnte.nota, FtAnte.NOTA_SZ, out ftAnteNulls[FtAnte.NOTA]);
                    Ssutil.DbGetString(hStmt, 0, "apoint", out ftAnte.apoint, FtAnte.APOINT_SZ, out ftAnteNulls[FtAnte.APOINT]);
                    Ssutil.DbGetString(hStmt, 0, "sdate", out ftAnte.sdate, FtAnte.SDATE_SZ, out ftAnteNulls[FtAnte.SDATE]);
                    Ssutil.DbGetString(hStmt, 0, "licence", out ftAnte.licence, FtAnte.LICENCE_SZ, out ftAnteNulls[FtAnte.LICENCE]);
                    Ssutil.DbGetString(hStmt, 0, "mdate", out ftAnte.mdate, FtAnte.MDATE_SZ, out ftAnteNulls[FtAnte.MDATE]);
                    Ssutil.DbGetString(hStmt, 0, "mtime", out ftAnte.mtime, FtAnte.MTIME_SZ, out ftAnteNulls[FtAnte.MTIME]);
                }
                catch (Exception e)
                {
                    Log2.e("\nFtUtils.FtReadAnte(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    GenUtil.SetError(1011, "Could not get antenna, because of field " + e.Message);
                    nRet = Error.ODBC_GET_FAILED;
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return nRet;
        }

        /// <summary>
        /// Test if a callsign is a callsign from a passive site.  
        /// </summary>
        /// <param name="cCall"></param>
        /// <returns></returns>
        public static bool IsCallPassive(string cCall)
        {
            return Strings.FirstCharIs(cCall, '%');
        }

        /// <summary>
        /// Get the next antenna index. The optional call2 and optional band code are 
        /// given. The first call is with an index of -1, the rest the previous 
        /// returned value.  
        /// </summary>
        /// <param name="pSite"></param>
        /// <param name="nLast"></param>
        /// <param name="cCall2"></param>
        /// <param name="cBand"></param>
        /// <returns></returns>
        public static int FtNextAnte(FtSiteStr pSite, int nLast, string cCall2, string cBand)
        {
            int nInd;
            string cSiteBand;

            for (nInd = ++nLast; nInd < pSite.nNumAnts; nInd++)
            {
                if (!String.IsNullOrWhiteSpace(cCall2))
                {
                    //	We are searching for a call2 ...
                    if (pSite.stAntsPtr[nInd].call2.Equals(cCall2))
                    {
                        //	We have found one that matches.  Carry on.
                    }
                    else
                    {
                        //	call2s don't match, try another.
                        continue;
                    }
                }

                cSiteBand = pSite.stAntsPtr[nInd].bndcde.Trim();
                if (!String.IsNullOrWhiteSpace(cBand))
                {
                    if (cSiteBand.Equals(cBand))
                    {
                        //	bands are the same.  Carry on.
                    }
                    else
                    {
                        //	Not the same bands
                        continue;
                    }
                }

                //	If we get here, we have found one.
                break;
            }

            if (nInd >= pSite.nNumAnts)
            {
                //	None found.  Return -2;
                nInd = -2;
            }

            return nInd;
        }

        /// <summary>
        /// Get the main antenna pointing to the given call2 on the given band. 
        /// Negative if there is none.  
        /// </summary>
        /// <param name="pSite"></param>
        /// <param name="cCall2"></param>
        /// <param name="cBand"></param>
        /// <returns></returns>
        public static int FtGetMainAnte(FtSiteStr pSite, string cCall2, string cBand)
        {
            int nInd;

            nInd = FtNextAnte(pSite, -1, cCall2, cBand);

            while (nInd >= 0)
            {
                if (pSite.stAntsPtr[nInd].ause.Equals("TR") ||
                    pSite.stAntsPtr[nInd].ause.Equals("TX") ||
                    pSite.stAntsPtr[nInd].ause.Equals("RX"))
                {
                    break;
                }
                nInd = FtNextAnte(pSite, nInd, cCall2, cBand);
            }

            return nInd;
        }

        /// <summary>
        /// Read a channel from the channel table. This is out of band for these 
        /// routines (you really should get the channels from a site read). It is being 
        /// put in for the tsip conversion because I really don't have the time to fix 
        /// the logic in tsip right now. GJS - 1283 - 2010. 01. 13.  
        /// </summary>
        /// <param name="cCall1"></param>
        /// <param name="cCall2"></param>
        /// <param name="bndcde"></param>
        /// <param name="chid"></param>
        /// <param name="cTableName"></param>
        /// <param name="pChan"></param>
        /// <param name="pChanNull"></param>
        /// <returns></returns>
        public static int FtReadChan(string cCall1,
                                        string cCall2,
                                        string bndcde,
                                        string chid,
                                        string cTableName,
                                        out FtChan pChan,
                                        out SQLLEN[] pChanNull)
        {
            // 'out' requirements.
            pChan = null;
            pChanNull = null;

            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            SQLHDBC hConn = Ssutil.NewConn();

            string cDBTableName;
            string cSQLBuff;
            int nRet = 0;

            cDBTableName = cTableName;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nFtUtils.FtReadChan(): ERROR: call to SQLAllocHandle() failed.");
                string str = String.Format("ftReadChan01 - Could not allocate handle for channel {0}->{1} band {2} chid {3}",
                    cCall1, cCall2, bndcde, chid);
                Ssutil.DbGetDiagStmt(hStmt, str);
                Ssutil.DisConn(hConn);
                return Error.ODBC_SQLALLOCHANDLE_FAILED;
            }

            /*  Allocated array of channels, now read them in using the
                    channel structure. */
            cSQLBuff = String.Format("SELECT cmd,recstat,call1,call2,bndcde,splan,hl,vh,chid,freqtx,poltx,antnumbtx1,antnumbtx2,eqpttx,eqptutx,pwrtx,atpccde,afsltx1,afsltx2,traftx,srvctx,stattx,freqrx,polrx,antnumbrx1,antnumbrx2,antnumbrx3,eqptrx,eqpturx,afslrx1,afslrx2,afslrx3,pwrrx1,pwrrx2,pwrrx3,trafrx,esint,tsint,srvcrx,statrx,routnumb,stnnumb,hopnumb,sdate,notetx,noterx,notegnl,cpoint,feetx,feerx,mdate,mtime FROM  {0} WHERE call1='{1}' and call2='{2}' and bndcde='{3}' and chid='{4}' ",
                cDBTableName, cCall1, cCall2, bndcde, chid);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQLBuff, cSQLBuff.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nFtUtils.FtReadChan(): ERROR: call to SQLExecDirect() failed.");
                Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                nRet = Error.ODBC_EXECDIRECT_FAILED;
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (ODBC.IsNoData(sqlRet))
                {
                    //	End of data.
                    nRet = Constant.NOMORERECS;
                }
                else if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nFtUtils.FtReadChan(): ERROR: call to SQLFetch() failed.");
                    Ssutil.DbGetDiag(ODBC.SQL_HANDLE_STMT, hStmt);
                    nRet = Error.ODBC_FETCH_FAILED;
                }
                else
                {
                    /*  Got the channel.  move it into the array */
                    /* Retrieve the data for the channel */
                    pChan = new FtChan();
                    pChanNull = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

                    try
                    {
                        Ssutil.DbGetString(hStmt, 1, "cmd", out pChan.cmd, FtChan.CMD_SZ, out pChanNull[FtChan.CMD]);
                        Ssutil.DbGetString(hStmt, 2, "recstat", out pChan.recstat, FtChan.RECSTAT_SZ, out pChanNull[FtChan.RECSTAT]);
                        Ssutil.DbGetString(hStmt, 3, "call1", out pChan.call1, FtChan.CALL1_SZ, out pChanNull[FtChan.CALL1]);
                        Ssutil.DbGetString(hStmt, 4, "call2", out pChan.call2, FtChan.CALL2_SZ, out pChanNull[FtChan.CALL2]);
                        Ssutil.DbGetString(hStmt, 5, "bndcde", out pChan.bndcde, FtChan.BNDCDE_SZ, out pChanNull[FtChan.BNDCDE]);
                        Ssutil.DbGetString(hStmt, 6, "splan", out pChan.splan, FtChan.SPLAN_SZ, out pChanNull[FtChan.SPLAN]);
                        Ssutil.DbGetShort(hStmt, 7, "hl", out pChan.hl, out pChanNull[FtChan.HL]);
                        Ssutil.DbGetShort(hStmt, 8, "vh", out pChan.vh, out pChanNull[FtChan.VH]);
                        Ssutil.DbGetString(hStmt, 9, "chid", out pChan.chid, FtChan.CHID_SZ, out pChanNull[FtChan.CHID]);
                        Ssutil.DbGetDouble(hStmt, 10, "freqtx", out pChan.freqtx, out pChanNull[FtChan.FREQTX]);
                        Ssutil.DbGetString(hStmt, 11, "poltx", out pChan.poltx, FtChan.POLTX_SZ, out pChanNull[FtChan.POLTX]);
                        Ssutil.DbGetShort(hStmt, 12, "antnumbtx1", out pChan.antnumbtx1, out pChanNull[FtChan.ANTNUMBTX1]);
                        Ssutil.DbGetShort(hStmt, 13, "antnumbtx2", out pChan.antnumbtx2, out pChanNull[FtChan.ANTNUMBTX2]);
                        Ssutil.DbGetString(hStmt, 14, "eqpttx", out pChan.eqpttx, FtChan.EQPTTX_SZ, out pChanNull[FtChan.EQPTTX]);
                        Ssutil.DbGetString(hStmt, 15, "eqptutx", out pChan.eqptutx, FtChan.EQPTUTX_SZ, out pChanNull[FtChan.EQPTUTX]);
                        Ssutil.DbGetFloat(hStmt, 16, "pwrtx", out pChan.pwrtx, out pChanNull[FtChan.PWRTX]);
                        Ssutil.DbGetFloat(hStmt, 17, "atpccde", out pChan.atpccde, out pChanNull[FtChan.ATPCCDE]);
                        Ssutil.DbGetFloat(hStmt, 18, "afsltx1", out pChan.afsltx1, out pChanNull[FtChan.AFSLTX1]);
                        Ssutil.DbGetFloat(hStmt, 19, "afsltx2", out pChan.afsltx2, out pChanNull[FtChan.AFSLTX2]);
                        Ssutil.DbGetString(hStmt, 20, "traftx", out pChan.traftx, FtChan.TRAFTX_SZ, out pChanNull[FtChan.TRAFTX]);
                        Ssutil.DbGetString(hStmt, 21, "srvctx", out pChan.srvctx, FtChan.SRVCTX_SZ, out pChanNull[FtChan.SRVCTX]);
                        Ssutil.DbGetString(hStmt, 22, "stattx", out pChan.stattx, FtChan.STATTX_SZ, out pChanNull[FtChan.STATTX]);
                        Ssutil.DbGetDouble(hStmt, 23, "freqrx", out pChan.freqrx, out pChanNull[FtChan.FREQRX]);
                        Ssutil.DbGetString(hStmt, 24, "polrx", out pChan.polrx, FtChan.POLRX_SZ, out pChanNull[FtChan.POLRX]);
                        Ssutil.DbGetShort(hStmt, 25, "antnumbrx1", out pChan.antnumbrx1, out pChanNull[FtChan.ANTNUMBRX1]);
                        Ssutil.DbGetShort(hStmt, 26, "antnumbrx2", out pChan.antnumbrx2, out pChanNull[FtChan.ANTNUMBRX2]);
                        Ssutil.DbGetShort(hStmt, 27, "antnumbrx3", out pChan.antnumbrx3, out pChanNull[FtChan.ANTNUMBRX3]);
                        Ssutil.DbGetString(hStmt, 28, "eqptrx", out pChan.eqptrx, FtChan.EQPTRX_SZ, out pChanNull[FtChan.EQPTRX]);
                        Ssutil.DbGetString(hStmt, 29, "eqpturx", out pChan.eqpturx, FtChan.EQPTURX_SZ, out pChanNull[FtChan.EQPTURX]);
                        Ssutil.DbGetFloat(hStmt, 30, "afslrx1", out pChan.afslrx1, out pChanNull[FtChan.AFSLRX1]);
                        Ssutil.DbGetFloat(hStmt, 31, "afslrx2", out pChan.afslrx2, out pChanNull[FtChan.AFSLRX2]);
                        Ssutil.DbGetFloat(hStmt, 32, "afslrx3", out pChan.afslrx3, out pChanNull[FtChan.AFSLRX3]);
                        Ssutil.DbGetFloat(hStmt, 33, "pwrrx1", out pChan.pwrrx1, out pChanNull[FtChan.PWRRX1]);
                        Ssutil.DbGetFloat(hStmt, 34, "pwrrx2", out pChan.pwrrx2, out pChanNull[FtChan.PWRRX2]);
                        Ssutil.DbGetFloat(hStmt, 35, "pwrrx3", out pChan.pwrrx3, out pChanNull[FtChan.PWRRX3]);
                        Ssutil.DbGetString(hStmt, 36, "trafrx", out pChan.trafrx, FtChan.TRAFRX_SZ, out pChanNull[FtChan.TRAFRX]);
                        Ssutil.DbGetFloat(hStmt, 37, "esint", out pChan.esint, out pChanNull[FtChan.ESINT]);
                        Ssutil.DbGetFloat(hStmt, 38, "tsint", out pChan.tsint, out pChanNull[FtChan.TSINT]);
                        Ssutil.DbGetString(hStmt, 39, "srvcrx", out pChan.srvcrx, FtChan.SRVCRX_SZ, out pChanNull[FtChan.SRVCRX]);
                        Ssutil.DbGetString(hStmt, 40, "statrx", out pChan.statrx, FtChan.STATRX_SZ, out pChanNull[FtChan.STATRX]);
                        Ssutil.DbGetString(hStmt, 41, "routnumb", out pChan.routnumb, FtChan.ROUTNUMB_SZ, out pChanNull[FtChan.ROUTNUMB]);
                        Ssutil.DbGetShort(hStmt, 42, "stnnumb", out pChan.stnnumb, out pChanNull[FtChan.STNNUMB]);
                        Ssutil.DbGetShort(hStmt, 43, "hopnumb", out pChan.hopnumb, out pChanNull[FtChan.HOPNUMB]);
                        Ssutil.DbGetString(hStmt, 44, "sdate", out pChan.sdate, FtChan.SDATE_SZ, out pChanNull[FtChan.SDATE]);
                        Ssutil.DbGetString(hStmt, 45, "notetx", out pChan.notetx, FtChan.NOTETX_SZ, out pChanNull[FtChan.NOTETX]);
                        Ssutil.DbGetString(hStmt, 46, "noterx", out pChan.noterx, FtChan.NOTERX_SZ, out pChanNull[FtChan.NOTERX]);
                        Ssutil.DbGetString(hStmt, 47, "notegnl", out pChan.notegnl, FtChan.NOTEGNL_SZ, out pChanNull[FtChan.NOTEGNL]);
                        Ssutil.DbGetString(hStmt, 48, "cpoint", out pChan.cpoint, FtChan.CPOINT_SZ, out pChanNull[FtChan.CPOINT]);
                        Ssutil.DbGetString(hStmt, 49, "feetx", out pChan.feetx, FtChan.FEETX_SZ, out pChanNull[FtChan.FEETX]);
                        Ssutil.DbGetString(hStmt, 50, "feerx", out pChan.feerx, FtChan.FEERX_SZ, out pChanNull[FtChan.FEERX]);
                        Ssutil.DbGetString(hStmt, 51, "mdate", out pChan.mdate, FtChan.MDATE_SZ, out pChanNull[FtChan.MDATE]);
                        Ssutil.DbGetString(hStmt, 52, "mtime", out pChan.mtime, FtChan.MTIME_SZ, out pChanNull[FtChan.MTIME]);

                    }
                    catch (Exception e)
                    {
                        Log2.e("\nFtUtils.FtReadChan(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                        GenUtil.SetError(1012, "Could not retrieve channel field: " + e.Message);
                        nRet = Error.ODBC_GET_FAILED;
                    }
                }
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return nRet;
        }

        /// <summary>
        /// Create an array of link structures from a site structure.  
        /// </summary>
        /// <param name="ftSiteStr"></param>
        /// <param name="pLinks"></param>
        /// <returns></returns>
        public static int FtMakeLinks(FtSiteStr ftSiteStr, out TLink[] pLinks)
        {
            //...Log2.v("\nFtUtils.FtMakeLinks(): called");

            // 'out' requirement.
            pLinks = null;

            int nInd;

            // Use a list to accumulate the TLinks and then convert this to TLink[].
            //	Start with 0 links.
            List<TLink> tLinksList = new List<TLink>();

            // First add all the antennas to the links.  
            // This creates the link array.
            for (nInd = 0; nInd < ftSiteStr.nNumAnts; nInd++)
            {
                FtAddAnt(ref tLinksList, ftSiteStr, nInd);
            }

            /*	Now add the channels to these links.  A channel can only belong to
            *		one link	*/
            for (nInd = 0; nInd < ftSiteStr.nNumChans; nInd++)
            {
                FtAddChan(ref tLinksList, ftSiteStr, nInd);
            }

            pLinks = tLinksList.ToArray();

            for (int i = 0; i < pLinks.Length; i++)
            {
                //...Log2.v("\nFtUtils.FtMakeLinks(): tLink[" + i + "] = " + pLinks[i]);
            }

            return pLinks.Length;
        }

        /// <summary>
        /// Given a channel structure from the pdf, add all the antennas referred to by 
        /// the channel, making sure they are compatible with the antenna use at the 
        /// other end.  
        /// </summary>
        /// <param name="tLinkList"></param>
        /// <param name="ftSiteStr"></param>
        /// <param name="nAnt"></param>
        /// <returns></returns>
        public static int FtAddAnt(ref List<TLink> tLinkList, FtSiteStr ftSiteStr, int nAnt)
        {
            int nInd;
            bool foundExisting = false;
            TLink tLink = null;

            if (ftSiteStr.stAntsPtr[nAnt].cmd.Equals("D"))
            {
                //	This antenna is being deleted.  Don't add the link.
                return 1;
            }

            /*	First find the link in the link array  */
            for (nInd = 0; nInd < tLinkList.Count; nInd++)
            {
                tLink = tLinkList[nInd];   /*	Just to reduce complexity  */

                if (tLink.call2.Equals(ftSiteStr.stAntsPtr[nAnt].call2) &&
                    tLink.bndcde.Equals(ftSiteStr.stAntsPtr[nAnt].bndcde))
                {
                    /*	We have found a link this antenna belongs to. */
                    foundExisting = true;
                    break;
                }
            }

            if (!foundExisting)
            {
                /*	We did not find the link.  Add it. */
                tLink = new TLink();

                // Populate the new TLink with its data.
                tLink.eWhich = Enums.DB.FT;
                tLink.call2 = ftSiteStr.stAntsPtr[nAnt].call2;
                tLink.bndcde = ftSiteStr.stAntsPtr[nAnt].bndcde;
                tLink.nNumAnts = 0;
                tLink.aAnts = new int[0];
                tLink.nNumChan = 0;
                tLink.aChan = new int[0];

                tLinkList.Add(tLink);
            }

            /*	Add the antenna to the link. */

            // Arrays in C# are immutable so this is going to be messy.
            // There is surely a better way to accumulate this data?
            tLink.nNumAnts++;

            List<int> aAntsList = tLink.aAnts.ToList();
            aAntsList.Add(nAnt);
            tLink.aAnts = aAntsList.ToArray();

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Add a channel to the link structure. Adding the antenna to the links has 
        /// already created the link structure, now we go through the channels and add 
        /// each one to the links. A channel can only belong to one link.  
        /// </summary>
        /// <param name="tLinkList"></param>
        /// <param name="ftSiteStr"></param>
        /// <param name="nChan"></param>
        /// <returns></returns>
        public static int FtAddChan(ref List<TLink> tLinkList, FtSiteStr ftSiteStr, int nChan)
        {
            int nInd;
            bool foundExisting = false;
            TLink tLink = null;

            if (ftSiteStr.stChanPtr[nChan].cmd.Equals("D"))
            {
                //	This channel is being deleted.  Don't add to link.
                return 1;
            }

            /*	First find the link this channel belongs to.  */

            // BUG FIX PENDING: 
            // In certain circumstances the 'for' loop increments an index
            // beyond the current List.Count.
            // The bug-fixed line is provided below.
            //for (nInd = 0; nInd < tLinkList.Count; nInd++)
            for (nInd = 0; nInd <= tLinkList.Count; nInd++)
            {
                tLink = tLinkList[nInd];   /*	Just to reduce complexity  */

                if (tLink.call2.Equals(ftSiteStr.stChanPtr[nChan].call2) &&
                    tLink.bndcde.Equals(ftSiteStr.stChanPtr[nChan].bndcde))
                {
                    /*	We have found the link this channel belongs to. */
                    foundExisting = true;
                    break;
                }
            }

            if (!foundExisting)
            {
                /*	Link not found for this channel.  This is an error */
                string str = String.Format("There is no link for channel {0}, {1} to {2} band {3} chid {4}.",
                        nChan,
                        ftSiteStr.stSite.call1, ftSiteStr.stChanPtr[nChan].call2,
                        ftSiteStr.stChanPtr[nChan].bndcde, ftSiteStr.stChanPtr[nChan].chid);
                Log2.e("\nFtUtils.FtAddChan(): ERROR: " + str);
                GenUtil.SetErr("addchan: " + str);
                return Constant.FAILURE;
            }

            /*	Add the channel to the link in nFound.  */

            // Arrays in C# are immutable so this is going to be messy.
            // There is surely a better way to accumulate this data?
            tLink.nNumChan++;

            List<int> aChanList = tLink.aChan.ToList();
            aChanList.Add(nChan);
            tLink.aChan = aChanList.ToArray();

            return Constant.SUCCESS;
        }

        /// <summary>
        /// Free up the link array.  
        /// </summary>
        /// <param name="pLinks"></param>
        /// <param name="nNumLinks"></param>
        public static void FtFreeLinks(TLink[] pLinks, int nNumLinks)
        {
            return;
        }

        /// <summary>
        /// Return the index of the first non-deleted antenna of a link, -1 if there 
        /// are none.  
        /// </summary>
        /// <param name="pSite"></param>
        /// <param name="pLink"></param>
        /// <param name="nLinkNum"></param>
        /// <returns></returns>
        public static int FtFirstNonDelAnte(FtSiteStr pSite, TLink[] pLink, int nLinkNum)
        {
            int nInd;
            int nRet = -1;
            int nAntInd;

            for (nInd = 0; nInd < pLink[nLinkNum].nNumAnts; nInd++)
            {
                nAntInd = pLink[nLinkNum].aAnts[nInd];
                if (!pSite.stAntsPtr[nAntInd].cmd.Equals("D"))
                {
                    nRet = nAntInd;
                    break;
                }
            }

            return nRet;
        }

        /// <summary>
        /// This method returns an array of FtChng objects corresponding to all the
        /// 'change of call sign' (i.e. 'GK' CSV record type) for a prescribed PDF table root name.
        /// </summary>
        /// <param name="ftChngs"> - returned array of FtChng objects.</param>
        /// <param name="cInTable"> - prescribed PDF table root name.</param>
        /// <returns></returns>
        public static int FtGetChgCalls(out FtChng[] ftChngs, string cInTable)   /*  The pdf table name     */
        {
            // 'out' requirement.
            ftChngs = new FtChng[0];

            List<FtChng> ftChngList = new List<FtChng>();
            string cSQL;

            string cTab;
            string cTableName;
            int nRet = 0;

            SQLHDBC hConnection = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            int nCount = 0;
            int nInd;

            FtGetTableName(out cTableName, cInTable);

            /*  Get the change of call sign table name in cTab */
            if (GenUtil.UtCvtName(Constant.FT_CHNG_CALL, cTableName, out cTab) != 0)
            {
                return -10;
            }

            //	Allocate the handle.
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "ftGetChgCalls: Could not allocate handle");
                return -30;
            }

            nCount = Ssutil.DbCountRows(cTab, "");  //	Get the number of change of call signs

            if (nCount > 0)
            {
                cSQL = String.Format("SELECT newcall1, oldcall1, name FROM {0} ORDER BY newcall1 ", cTab);

                sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Ssutil.DbGetDiagStmt(hStmt, "Error getting change of call sign.");
                    nRet = -1;
                }
                else
                {
                    for (nInd = 0; nInd < nCount; nInd++)
                    {
                        sqlRet = ODBC.SQLFetch(hStmt);

                        if (sqlRet == ODBC.SQL_NO_DATA)
                        {
                            return 1;   //	No record.
                        }

                        if (!ODBC.IsOK(sqlRet))
                        {
                            Ssutil.DbGetDiagStmt(hStmt, "ftGetChgCall: Could not retrieve record.");
                            return -32;
                        }

                        FtChng ftChng = new FtChng();
                        SQLLEN nullInd;

                        try
                        {
                            Ssutil.DbGetString(hStmt, 1, "newcall1", out ftChng.newcall1, FtChng.NEWCALL1_SZ, out nullInd);
                            Ssutil.DbGetString(hStmt, 2, "oldcall1", out ftChng.oldcall1, FtChng.OLDCALL1_SZ, out nullInd);
                            Ssutil.DbGetString(hStmt, 3, "name", out ftChng.name, FtChng.NAME_SZ, out nullInd);
                        }
                        catch (Exception e)
                        {
                            Log2.e("\nFtUtils.FtGetChgCalls(): ERROR: call to Ssutil.DbGetString() failed for: " + e.Message);
                            Ssutil.DbGetDiagStmt(hStmt, "ftGetChgCall: Could not retrieve title record on field: " + e.Message);
                            return -31;
                        }

                        // Accumulate the newly populated FtChng object.
                        ftChngList.Add(ftChng);
                    }

                    // Convert the list to an array to be returned to the caller.
                    ftChngs = ftChngList.ToArray();

                    // Set the return value.
                    nRet = nCount;
                }
            }

            // Release ODBC resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConnection);

            return nRet;
        }

        /// <summary>
        /// This method returns the index of the next FtChan object in the FtChan[] array 
        /// that has a prescribed call2 and bndcde fields (it returns -2 if not found); 
        /// <b>the first call to this method must have nLastIndex set to -1</b>.
        /// </summary>
        /// <param name="ftChans"> - prescribed array of FtChan objects.</param>
        /// <param name="cCall2"> - call2 to be matched.</param>
        /// <param name="cBand"> - bndcde to be matched.</param>
        /// <param name="nLastIndex"> - this should be set equal to the returned value from the previous call.</param>
        /// <returns> - the index of the next FtChan object in the FtChan[] array 
        /// that has a prescribed call2 and bndcde fields.</returns>
        public static int FtNextChanLink(FtChan[] ftChans,      /*	Pointer to array of Chans */
                                            string cCall2,
                                            string cBand,
                                            int nLastIndex)
        {
            int nRet = -2;

            for (++nLastIndex; nLastIndex < ftChans.Length; nLastIndex++)
            {
                if (ftChans[nLastIndex].call2.Equals(cCall2) &&
                    ftChans[nLastIndex].bndcde.Equals(cBand))
                {
                    /*  We got one. */
                    nRet = nLastIndex;
                    break;
                }
            }
            return nRet;
        }

        /// <summary>
        /// This method returns a string that can be used in an SQL 'WHERE' clause to
        /// select only adjacent bands; these adjacent bands are encoded as a 'bit-array'
        /// encapsulated by a prescribed BandBits object.
        /// </summary>
        /// <param name="tBands"></param>
        /// <param name="cSearch"></param>
        public static void FtSQLSearchString(BandBits tBands, out string cSearch)
        {
            // 'out' requirement.
            cSearch = "";

            string cOr;
            int nInd;
            int nNumBands = Suutils.SuNumberOfBands();  //	Get the number of bands
            int nNumBandWds = (nNumBands + Constant.BITS_PER_BANDWORD - 1) / Constant.BITS_PER_BANDWORD;
            string cOneBand;

            cSearch = "";
            cOr = "";

            for (nInd = 0; nInd < nNumBandWds; nInd++)
            {
                cOneBand = String.Format("{0}bandwd{1} & 0x{2:x8} != 0",
                                            cOr, nInd + 1, tBands.bitArray[nInd]);

                cOr = " or ";
                cSearch += cOneBand;
            }

            return;
        }

        private static SQLLEN glb_sqlRowCount = -2;		//	Row count from the execute

        /// <summary>
        /// This method sends a single SQL query to be executed on the SQL Server via ODBC;
        /// the method returns true if the SQL query succeeded and false if it failed.
        /// </summary>
        /// <param name="cStatement"></param>
        /// <returns></returns>
        public static bool DbExecute(string cStatement)
        {
            SQLHANDLE hStmt;
            SQLRETURN sqlRet = ODBC.SQL_ERROR;
            SQLHDBC hConn = Ssutil.NewConn();

            glb_sqlRowCount = -1;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            if (ODBC.IsOK(sqlRet))
            {
                sqlRet = ODBC.SQLExecDirect(hStmt, cStatement, cStatement.Length);

                if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
                {
                    //	If it is not a success or success with info, or not end of data then it's an error.
                    Log2.e("\n\nFtUtils.DbExecute(): ERROR: call to SQLExecDirect() returned sqlRet = " + sqlRet);
                    Ssutil.DbGetDiagStmt(hStmt, "dbExecute Error in Exec Direct.");
                }
                else
                {
                    IntPtr intPtr = Marshal.AllocHGlobal(sizeof(SQLLEN));

                    ODBC.SQLRowCount(hStmt, intPtr);

                    glb_sqlRowCount = Marshal.ReadInt64(intPtr);
                }
            }
            else
            {
                Log2.e("\n\nFtUtils.DbExecute(): ERROR: call to SQLAllocHandle() returned sqlRet = " + sqlRet);
                Ssutil.DbGetDiagStmt(hStmt, "dbExecute Error allocating handle.");
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (sqlRet == ODBC.SQL_SUCCESS || sqlRet == ODBC.SQL_SUCCESS_WITH_INFO || sqlRet == ODBC.SQL_NO_DATA);
        }

        /// <summary>
        /// This method returns the index of an antenna in an antenna array given the 
        /// call2, band, and anum; returns -1 if not found.
        /// </summary>
        /// <param name="pAnte"></param>
        /// <param name="nAnteCount"></param>
        /// <param name="cCall2"></param>
        /// <param name="cBand"></param>
        /// <param name="nAnum"></param>
        /// <returns></returns>
        public static int FtAnteIndex(FtAnte[] pAnte,           /*  Array of antenna structs */
                                        short nAnteCount,       /*  Number of antenna        */
                                        string cCall2,          /*  call2                    */
                                        string cBand,           /*  Band code                */
                                        short nAnum)            /*  Antenna number           */
        {
            short nInd;
            int nRet = -1;

            for (nInd = 0; nInd < nAnteCount; nInd++)
            {
                if (pAnte[nInd].call2.Equals(cCall2) &&
                    pAnte[nInd].bndcde.Equals(cBand) &&
                    pAnte[nInd].anum == nAnum)
                {
                    /*  Found it */
                    nRet = nInd;
                    break;
                }
            }
            return nRet;
        }

        /// <summary>
        /// This method returns the channel index associated with a prescribed
        /// call2, bndcde and chid for a prescribed FtSiteStr object; if an error
        /// occurs then the value -1 is returned.
        /// </summary>
        /// <param name="pSite"></param>
        /// <param name="cCall"></param>
        /// <param name="cBand"></param>
        /// <param name="cChid"></param>
        /// <returns></returns>
        public static int FindFtChid(FtSiteStr pSite,
                                        string cCall,
                                        string cBand,
                                        string cChid)
        {
            short nInd;
            int nRet = -1;
            FtChan pChan;

            if (pSite == null)
            {
                /*  If there is no site, then don't find the channel. */
                nRet = -1;
            }
            else
            {
                for (nInd = 0; nInd < pSite.nNumChans; nInd++)
                {
                    pChan = pSite.stChanPtr[nInd];

                    if (pChan.call2.Equals(cCall) &&
                        pChan.bndcde.Equals(cBand) &&
                        pChan.chid.Equals(cChid))
                    {
                        /*  Got it return its index */
                        nRet = nInd;
                        break;
                    }
                }
            }
            return nRet;
        }

        /// <summary>
        /// This method returns the height of the highest antenna at the remote site at 
        /// that is used by a prescribed antenna at a local site; if an error occurs
        /// then a negative value is returned and an error message provides an explanation.
        /// </summary>
        /// <remarks>
        /// This is used for the elevation angle calculations.  It is done by
        /// looking at the antenna at this end that we are interested in, getting all
        /// the channels that use it, and finding all those channels at the other
        /// end, and then returning the index of the highest antenna at the other
        /// end that these channels use.  This is unavoidable with the database
        /// structure the way it is.
        /// </remarks>
        /// <param name="localFtSiteStr"> - prescribed local site FtSiteStr object.</param>
        /// <param name="remoteFtSiteStr"> - prescribed remote site FtSiteStr object.</param>
        /// <param name="nAntInd"> - index of prescribed antenna that is an element of localFtSiteStr's array of FtAnte.</param>
        /// <param name="errMsg"> - output message that describes an error condition (if any).</param>
        /// <returns></returns>
        public static double HeightTallestRemoteAnte(FtSiteStr localFtSiteStr,
                                                        FtSiteStr remoteFtSiteStr,
                                                        int nAntInd,
                                                        out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            const double ERROR_VALUE = -666.6;
            double dHeight = ERROR_VALUE;

            try
            {
                int nChanInd;

                FtAnte localFtAnte = localFtSiteStr.stAntsPtr[nAntInd];
                int localAnum = localFtAnte.anum;

                int[] anum5Pack = new int[5];

                List<FtChan> localFtChans = new List<FtChan>();
                List<FtChan> remoteFtChans = new List<FtChan>();
                List<int> remoteAnums = new List<int>();
                List<FtAnte> remoteFtAntes = new List<FtAnte>();

                // Create a list of all the channels at 'this' end that use the prescribed antenna's
                // anum and bndcde and that are linked to the prescribed remote site.
                for (nChanInd = 0; nChanInd < localFtSiteStr.nNumChans; nChanInd++)
                {
                    FtChan ftChan = localFtSiteStr.stChanPtr[nChanInd];

                    // Test if the channel at this end uses the prescribed antenna.
                    bool chanUsesAntenna = ftChan.antnumbrx1 == localAnum ||
                                           ftChan.antnumbrx2 == localAnum ||
                                           ftChan.antnumbrx3 == localAnum ||
                                           ftChan.antnumbtx1 == localAnum ||
                                           ftChan.antnumbtx2 == localAnum;

                    if (!chanUsesAntenna)
                    {
                        continue; // skip this channel.
                    }

                    // Test that the channel at this end has the same bndcde as the prescribed antenna.
                    bool chanHasSameBndcdeAsAntenna = ftChan.bndcde.Equals(localFtAnte.bndcde);

                    if (!chanHasSameBndcdeAsAntenna)
                    {
                        continue; // skip this channel.
                    }

                    // Test if the channel at this end is linked to the remote site.
                    bool chanLinkedToRemoteSite = ftChan.call2.Equals(remoteFtSiteStr.stSite.call1);

                    if (!ftChan.call2.Equals(remoteFtSiteStr.stSite.call1))
                    {
                        continue; // skip this channel.
                    }

                    // Add the channel to the list.
                    localFtChans.Add(ftChan);

                } // End of for-loop: nInd

                // If the filtered list of channels at this end is empty then we have
                // a problem. (This should have been caught by FtValidate).
                if (localFtChans.Count == 0)
                {
                    errMsg = String.Format("This antenna has no channels assigned to it.");
                    return ERROR_VALUE;
                }

                // Create a list of matching channels at the remote end of these links.
                foreach (FtChan thisEndFtChan in localFtChans)
                {
                    // Find the index of the FtChan at the remote end of the link.
                    int remoteChanIndex = FtFindChanChid(remoteFtSiteStr, localFtSiteStr.stSite.call1,
                                                localFtAnte.bndcde,
                                                thisEndFtChan.chid);

                    if (remoteChanIndex < 0)
                    {
                        errMsg = "Could not find channel at remote end.";

                        return ERROR_VALUE;
                    }

                    // Add the matching remote channels to a list.
                    remoteFtChans.Add(remoteFtSiteStr.stChanPtr[remoteChanIndex]);
                }

                // If the list of matching channels at the remote end is empty then we have
                // a problem. (This should have been caught by FtValidate).
                if (remoteFtChans.Count == 0)
                {
                    errMsg = "Cannot find any linked channels at the remote site.";

                    return ERROR_VALUE;
                }

                // Create a list of all the anums of the antennae referenced by the matching
                // channels at the remote site.
                foreach (FtChan remoteFtChan in remoteFtChans)
                {
                    FtGetAnumArray(out anum5Pack, remoteFtChan);

                    // Only add non-zero anums to the list. (Zero anum corresponds to a null).
                    // Also, ensure that there are no duplicates.
                    foreach (int aNum in anum5Pack)
                    {
                        if ((aNum != 0) && !remoteAnums.Contains(aNum))
                        {
                            remoteAnums.Add(aNum);
                        }
                    }
                }

                // If the list of anums at the remote end is empty then we have
                // a problem. (This should have been caught by FtValidate).
                if (remoteAnums.Count == 0)
                {
                    errMsg = "Cannot find any linked antenna at the remote site.";

                    return ERROR_VALUE;
                }

                // Create a list of antennae at the remote site that are used by the matching channels 
                // and are linked to an antenna at the local site.
                foreach (int remoteAnum in remoteAnums)
                {

                    FtAnte remoteFtAnte = GetFtAnte(remoteFtSiteStr, localFtSiteStr.stSite.call1,
                                            localFtAnte.bndcde,
                                            remoteAnum);

                    if (remoteFtAnte == null)
                    {
                        errMsg = String.Format("Cannot find antenna at remote site with anum = {0}.", remoteAnum);

                        return ERROR_VALUE;
                    }
                    else
                    {
                        // Verify that the local and remote antenna can communicate by checking 
                        // that their directionalities (ause) are compatible.
                        Enums.AnteUse remoteAnteUse = GenUtil.GetDirType(localFtAnte.ause);

                        if (GenUtil.IsOEndType(remoteAnteUse, remoteFtAnte.ause))
                        {
                            remoteFtAntes.Add(remoteFtAnte);
                        }

                    }
                }

                // If the list of matching antenae at the remote end is empty then we have
                // a problem. (This should have been caught by FtValidate).
                if (remoteFtAntes.Count == 0)
                {
                    errMsg = "Cannot find any linked antenna at the remote site.";

                    return ERROR_VALUE;
                }

                // If we get here the all is well; iterate through the list of matching antennae
                // at the remote site and determine the index of the highest one.
                foreach (FtAnte ftAnte in remoteFtAntes)
                {
                    if (ftAnte.aht > dHeight)
                    {
                        dHeight = ftAnte.aht;
                    }
                }

            }
            catch (Exception e)
            {
                Log2.e("\nVcheck.HeightTallestRemoteAnte(): ERROR: Exception: " + e.Message);
                Log2.e("\nVcheck.HeightTallestRemoteAnte(): ERROR: Exception: " + e.StackTrace);
                Application.ExitQuietly(666);
            }

            return dHeight;
        }

        /// <summary>
        /// This routine returns an array of integers corresponding to the the antnumbrx1 ... 
        /// antnumbrx5 field values from the prescribed FtChan object.
        /// </summary>
        /// <param name="outAnums"></param>
        /// <param name="pChan"></param>
        public static void FtGetAnumArray(out int[] outAnums, FtChan pChan)
        {
            // 'out' requirement.
            outAnums = new int[5];

            outAnums[0] = pChan.antnumbrx1;
            outAnums[1] = pChan.antnumbrx2;
            outAnums[2] = pChan.antnumbrx3;
            outAnums[3] = pChan.antnumbtx1;
            outAnums[4] = pChan.antnumbtx2;

            return;
        }





    }
}
