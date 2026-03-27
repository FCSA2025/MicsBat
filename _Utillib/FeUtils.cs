using _DataStructures;
using _NewLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
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
    /// Provides general purpose 'utility' methods for accessing an ES PDF table.
    /// </summary>
    public class FeUtils
    {
        /// <summary>
        /// Creates a fully-populated FeTitl object that mirror the column values 
        /// of the prescribed pdf table.
        /// </summary>
        /// <param name="pTitle"> - FeTitl object</param>
        /// <param name="cInTable"> - pdf table name.</param>
        /// <returns></returns>
        /// <para>- ErrorMessages.UTCVTNAME_FAILURE - attempt to convert table name failed.</para>
        /// <para>- ErrorMessages.ODBC_SQLALLOCHANDLE_FAILURE - called to ODBC.SQLAllocHandle() failed.</para>
        public static int FeGetTitle(out FeTitl pTitle, string cInTable)
        {
            //...Log2.v("\n\nFeUtils.FeGetTitle(): Entry");
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


            FeGetTableName(out cTable, cInTable);

            //	if (utCvtName(FeGetTitle_TITL, cTable, cTab) != 0) 
            if (GenUtil.UtCvtName(Constant.FE_TITL, cTable, out cTab) != 0)
            {
                Ssutil.DisConn(hConnection);
                nRet = Error.UTCVTNAME_FAILED;
                return (nRet);
            }

            //	Allocate the handle.
            //    sqlRet = SQLAllocHandle(SQL_HANDLE_STMT, hConnection, &hStmt);
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConnection, out hStmt);
            //	if (sqlRet != SQL_SUCCESS && sqlRet != SQL_SUCCESS_WITH_INFO) 
            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "FeGetTitle: Could not allocate handle");

                Ssutil.DisConn(hConnection);
                return (Error.ODBC_SQLALLOCHANDLE_FAILED);
            }


            //    sprintf_s(cSQL, sizeof(cSQL),
            //		"SELECT "
            //		"validated,"
            //		"namef,"
            //		"source,"
            //		"descr,"
            //		"mdate,"
            //		"mtime "
            //		"FROM %s", cTab);
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
            //	sqlRet = SQLExecDirect(hStmt, (SQLCHAR*)cSQL, SQL_NTS);

            //...Log2.v("\r\nFeUtils.FeGetTitle(): SQLExecDirect():\r\n" + cSQL);
            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, ODBC.SQL_NTS);
            //	if (sqlRet != SQL_SUCCESS && sqlRet != SQL_SUCCESS_WITH_INFO) 
            if (!ODBC.IsOK(sqlRet))
            {
                pTitle = null;
                nRet = -1;
            }
            else
            {
                sqlRet = ODBC.SQLFetch(hStmt);

                if (sqlRet == ODBC.SQL_NO_DATA)
                {
                    // *
                    pTitle = null;

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                    Ssutil.DisConn(hConnection);
                    return 1;   //	No record.
                }

                //		if (sqlRet != SQL_SUCCESS && sqlRet != SQL_SUCCESS_WITH_INFO) 
                if (!ODBC.IsOK(sqlRet))
                {
                    Ssutil.DbGetDiagStmt(hStmt, "FeGetTitle: Could not retrieve title record.");

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);

                    Ssutil.DisConn(hConnection);
                    return -32;
                }

                //	Allocate the title in the calling program.  It will need to free it.
                //        * pTitle = (struct FeGetTitleTitl_ *) malloc(sizeof(struct FeGetTitleTitl_));
                pTitle = new FeTitl();

                //        memset(*pTitle, 0, sizeof(struct FeGetTitleTitl_));

                string cName = "";
                SQLLEN nNull = new SQLLEN();
                try
                {
                    //			sqlRet = (SQLRETURN)dbGetString(hStmt, 1, "validated", (*pTitle)->validated,
                    //				sizeof((* pTitle)->validated), NULL);
                    cName = "validated";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 1, cName, out pTitle.validated, Constant.FETITLE_VALIDATED_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    //			sqlRet = (SQLRETURN)dbGetString(hStmt, 2, "namef", (*pTitle)->namef,
                    //				sizeof((* pTitle)->namef), NULL);
                    cName = "namef";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 2, cName, out pTitle.namef, Constant.FETITLE_NAMEF_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    //			sqlRet = (SQLRETURN)dbGetString(hStmt, 3, "source", (*pTitle)->source,
                    //				sizeof((* pTitle)->source), NULL);
                    cName = "source";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 3, cName, out pTitle.source, Constant.FETITLE_SOURCE_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    //			sqlRet = (SQLRETURN)dbGetString(hStmt, 4, "descr", (*pTitle)->descr,
                    //				sizeof((* pTitle)->descr), NULL);
                    cName = "descr";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 4, cName, out pTitle.descr, Constant.FETITLE_DESCR_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    //			sqlRet = (SQLRETURN)dbGetString(hStmt, 5, "mdate", (*pTitle)->mdate,
                    //				sizeof((* pTitle)->mdate), NULL);
                    cName = "mdate";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 5, cName, out pTitle.mdate, Constant.DATE_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.

                    //			sqlRet = (SQLRETURN)dbGetString(hStmt, 6, "mtime", (*pTitle)->mtime,
                    //				sizeof((* pTitle)->mtime), NULL);
                    cName = "mtime";
                    sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 6, cName, out pTitle.mtime, Constant.TIME_SZ, out nNull);
                    if (!ODBC.IsOK(sqlRet)) throw new Exception();  //AH: added.
                }
                catch (Exception)
                {

                    //            dbGetDiagStmt(hStmt, "FeGetTitle: Could not retrieve title record on field: %s", cName);
                    Ssutil.DbGetDiagStmt(hStmt, "FeGetTitle: Could not retrieve title record on field: " + cName);
                    //            SQLFreeHandle(SQL_HANDLE_STMT, hStmt);
                    //...Log2.v("\r\nFeUtils.FeGetTitle(): Ssutil.DbGetString() FAILED to retrieve FeGetTitleTitle field: " + cName);
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    //            DisConn(hConnection);
                    Ssutil.DisConn(hConnection);
                    return (-31);
                }
                nRet = 0;
            }

            //Release resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConnection);

            //...Log2.v("\n\nFeUtils.FeGetTitle(): Exit");
            return nRet;
        }

        /// <summary>
        /// Checks whether a table name is already a full table name (i.e. starts 
        /// with fe_) and, if so, the method extracts and returns the base (short) table name.
        /// </summary>
        /// <param name="cTable"> - extracted base table name.</param>
        /// <param name="cInTable"> - prescribed (possibly full) table name.</param>
        public static void FeGetTableName(out string cTable, string cInTable)
        {
            //char* cDot;

            //if ((cDot = strchr(cInTable, '.')) != NULL)
            if (cInTable.Contains('.'))
            {
                //	We have an incorrect input, correct it.  It will be in form:-
                //	<schema>.FeGetTitle_<table>_xxxx, what we want is the <table>
                int cDot = cInTable.IndexOf('.');
                int cLast = cInTable.LastIndexOf('_');
                cTable = cInTable.Substring(cDot + 3, cLast - 1);
            }
            else
            {
                //safecopy(cTable, cInTable, nOutLen);
                cTable = cInTable;

            }
        }

        /// <summary>
        /// Get the next sequential site in a data file.  
        /// </summary>
        /// <param name="cLastLoc"></param>
        /// <param name="cNextLoc"></param>
        /// <param name="feSiteStr"></param>
        /// <param name="nDepth"></param>
        /// <param name="cShortName"></param>
        /// <returns></returns>
        public static int FeNextSite(string cLastLoc, out string cNextLoc, out FeSiteStr feSiteStr, int nDepth, string cShortName)
        {
            //...Log2.v("\nTpRunTsip.FeNextSite(): Entry");

            // out.
            cNextLoc = null;
            feSiteStr = null;

            int nRet = FeNextLoc(out cNextLoc, cLastLoc, cShortName);

            if (nRet == 0)
            {
                nRet = FeGetSite(cNextLoc, out feSiteStr, nDepth, cShortName);
            }

            //...Log2.v("\nTpRunTsip.FeNextSite(): Exit: nRet = " + nRet);
            return nRet;
        }

        /// <summary>
        /// Get the next call sign in the fe table. The last call sign retrieved is 
        /// given and the next one after it is returned. Start the process through the 
        /// table by making the last call sign blank. Table is the short name.  
        /// </summary>
        /// <param name="cLoc"></param>
        /// <param name="cLastLoc"></param>
        /// <param name="cTable"></param>
        /// <returns></returns>
        public static int FeNextLoc(out string cLoc, string cLastLoc, string cTable)
        {
            //...Log2.v("\nTpRunTsip.FeNextLoc(): Entry: " + cLastLoc + "   " + cTable);

            cLoc = null;
            string cSQL;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLLEN IsNull;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            cSQL = String.Format("select top (1) location from {0}.fe_{1}_site where location > '{2}' order by location",
                                    Info.GlobalSchema, cTable, cLastLoc);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);

            sqlRet = ODBC.SQLFetch(hStmt);

            if (ODBC.IsOK(sqlRet))
            {
                sqlRet = (SQLRETURN)Ssutil.DbGetString(hStmt, 1, "Location", out cLoc, Constant.LOCATION_SZ, out IsNull);
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nTpRunTsip.FeNextLoc(): Exit: sqlRet = " + sqlRet);
            return sqlRet;
        }

        /// <summary>
        /// Retrieve an ES site from the PDF tables. The depth of the retrieval is 
        /// dependent on the retrieve switch. Do not return null information.  
        /// </summary>
        /// <param name="cLoc"></param>
        /// <param name="feSiteStr"></param>
        /// <param name="nDepth"></param>
        /// <param name="cTable"></param>
        /// <returns></returns>
        public static int FeGetSite(string cLoc, out FeSiteStr feSiteStr, int nDepth, string cTable)
        {
            //...Log2.v("\nTpRunTsip.FeGetSite(): Entry: " + cLoc + "   " + nDepth + "   " + cTable);

            feSiteStr = null;
            FeSiteStrNulls anNulls;      /*  Pointer to the array of nulls */
            int nRet;

            nRet = FeGetSiteWN(cLoc, out feSiteStr, nDepth, cTable, out anNulls);

            //...Log2.v("\nTpRunTsip.FeGetSite(): Exit: nRet = " + nRet);
            return nRet;
        }

        /// <summary>
        /// Retrieves a TS SITE record from a DB FE_SITE table together with its associated nullInds. 
        /// The caller prescribes the 'depth' of retrieval, i.e. 'site only', 'site, antennae and azimuths',  
        /// or 'site, antennae, azimuths and channels'.
        /// </summary>
        /// <remarks>
        /// If nDepth = 1 only the site will be returned, 
        /// If nDepth = 2 the site, antennae and azimuths will be returned,
        /// If nDepth = 3 the site, antennae, azimuths and channels will be returned.
        /// </remarks>
        /// <param name="location"> - call sign of the SITE to be retrieved.</param>
        /// <param name="feSiteStr"> - a FeSiteStr object populated with data.</param>
        /// <param name="nDepth"> - 1, 2, 3 or 4 (see remarks above).</param>
        /// <param name="cInTable"> - PDF table name.</param>
        /// <param name="feSiteNull"> - a FeSiteNull object populated with data.</param>
        /// <returns></returns>
        /// <para>- Constant.SUCCESS - call succeeded in retrieving data from the DB..</para>
        /// <para>- Constant.FAILURE - invalid number of sites, antennae or channels.</para>
        /// <para>- Error.INVALID_DATA - either cCall or nDepth is invalid.</para>
        /// <para>- Error.UTCVTNAME_FAILED - call to GenUtil.UtCvtName() failed.</para>
        /// <para>- Error.ODBC_SELECT_FAILED - selection of rows failed.</para>
        public static int FeGetSiteWN(string location, out FeSiteStr feSiteStr, int nDepth, string cInTable, out FeSiteStrNulls feSiteNull)
        {
            //...Log2.v("\n\nFeUtils.FeGetSiteWN(): Entry: " + location + "   " + nDepth + "   " + cInTable);

            int nRet = -666;
            int nInd = 0;
            string whereClause = " location='" + location + "' ";
            string orderByClause;

            // out.
            feSiteStr = null;
            feSiteNull = null;

            // First, sanity check the input data and return if there is a problem..
            Boolean nDepthIsNotValid = (nDepth < 1) || (nDepth > 3);   //Therefor nDepth = 1, 2, or 3.
            if (String.IsNullOrWhiteSpace(location))
            {
                Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: location string is null or empty");
                nRet = Error.INVALID_DATA;
                return nRet;
            }
            if ((nDepth < 1) || (nDepth > 3))
            {
                Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Invalid nDepth : " + nDepth);
                nRet = Error.INVALID_DATA;
                return nRet;
            }

            // Instantiate the structured data objects to be output.
            // These constructors set appropriate initial values.
            feSiteStr = new FeSiteStr();
            feSiteNull = new FeSiteStrNulls();

            // Set the depth parameter.
            feSiteStr.nDepth = nDepth;

            // Get the full site table name.
            string cTable;
            FeGetTableName(out cTable, cInTable);
            string cFullTableName;  // The full table name.
            if (GenUtil.UtCvtName(Constant.FE_SITE, cTable, out cFullTableName) != 0)
            { /*  Get the site table name in cTab */
                GenUtil.SetErr("*ERROR* feGetSiteWN: Converting the table name.");
                Log2.e("\n\nFeGetSiteWN(): ERROR: GenUtil.UtCvtName() failed: Exit: -10");
                nRet = Error.UTCVTNAME_FAILED;
                return (nRet);
            }

            // ============================================
            // Select and fetch the FeSite data from the DB.
            // ============================================

            // We are assuming that only one FeSite in the DB has a call1 column that matches the location
            // provided by the caller. We should test for this!
            int nNumSites = Ssutil.DbCountRows(cFullTableName, whereClause);

            //...Log2.v("\r\nFeUtils.FeGetSiteWN(): nNumSites = " + nNumSites);

            // Check for failure of DbCountRows() to return valid number of sites.
            if (nNumSites < 0)
            {
                Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumSites);
                string str = String.Format("ftGetSiteWN03: Ingres error {0} getting sites.", nNumSites);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of no sites to read from the DB.
            else if (nNumSites == 0)
            {
                //...Log2.v("\r\nFeUtils.FeGetSiteWN(): Marker G: site not found in DB; location = " + location);
                return (Constant.FAILURE);
            }
            // Handle the pathological case of too many sites to read from the DB.
            else if (nNumSites >= 2)
            {
                Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker G-1: multiple sites in DB with location = " + location);
                return (Error.INVALID_DATA);
            }
            else // #1, nNumSite = 1
            {
                // Select the FeSite.
                int nDynFeSiteCursor = DynFeSite.FeSelectSite(cFullTableName, whereClause, null);
                if (nDynFeSiteCursor < 0)
                {
                    Log2.e("\nFeUtils.FeGetSiteWN(): ERROR: DynFeSite.FeSelectSite() failed.");
                    return (Error.ODBC_SELECT_FAILED);
                }

                // We have now found the site; next get its column data. 
                FeSite feSite;
                SQLLEN[] nullInd;

                if (DynFeSite.FeFetchSite(nDynFeSiteCursor, out feSite, out nullInd) == Constant.SUCCESS)
                {
                    // Save the site and associated column-nulls that we just fetched from the DB.
                    feSiteStr.stSite = feSite;
                    feSiteNull.anSiteNull = nullInd;
                }
                else
                {
                    Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker G-2: failed to fetch an extant site from the DB; location = " + location);
                    DynFeSite.FeCloseSite(nDynFeSiteCursor);
                    return (Constant.FAILURE);
                }

                // If we reach here, the site fetch was successful.
                // Free the DynFeSite Cursor (release ODBC resources etc).
                DynFeSite.FeCloseSite(nDynFeSiteCursor);
            }

            // At this point we have the site read in from the DB and stored in the site structure.  
            // If nDepth is 1, then this is all that was required and we can exit.  
            if (nDepth == 1)
            {
                //...Log2.v("\n\nFeUtils.FeGetSiteWN(): Exit (nDepth == 1)");
                return Constant.SUCCESS;
            }
            // End of fetching the site data from the DB.

            // ============================================
            // Find and fetch the FeAnte data from the DB.
            // ============================================

            // At this point, nDepth is 2 or 3 so we need to fetch and store (at least) the antennae and azimuth data from the DB.

            // Get antennae info from the DB.

            // Get the full antenna table name.
            GenUtil.UtCvtName(Constant.FE_ANTE, cTable, out cFullTableName);

            FeAnte[] pAnts = null;
            SQLLEN[][] pAntNull = null;  // Usage: [antenna, column]
            int nNumAnts;

            // First find out how many antennae there are in order to allocate the array.
            nNumAnts = Ssutil.DbCountRows(cFullTableName, whereClause);

            //...Log2.v("\r\nFeUtils.FeGetSiteWN(): nNumAnts = " + nNumAnts);

            // Check for failure of DbCountRows() to return valid number of antennae.
            if (nNumAnts < 0)
            {
                Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker F: DbCountRows() returned an invalid value: " + nNumAnts);
                return (nNumAnts);
            }

            // Handle the case of no antennae to read from the DB.
            if (nNumAnts == 0)
            {
                Log2.w("\r\nFeUtils.FeGetSiteWN(): WARNING: Marker G: number of antennae to read from DB is zero.");
            }
            else // #1, nNumAnts > 0
            {
                // Select the antennae.
                orderByClause = "location, call1";
                int nAnteCursor = DynFeAnte.FeSelectAnte(cFullTableName, whereClause, orderByClause);
                if (nAnteCursor < 0)
                {
                    Log2.e("\nFeUtils.FeGetSiteWN(): ERROR: DynFeSite.FeSelectAnte() failed.");
                    return (Error.ODBC_SELECT_FAILED);
                }

                // Instantiate the objects required store the antennae data.
                // There may be multiple fetches from the DB.
                pAnts = Arrays.CreateArrayUsingDefaultElementConstructor<FeAnte>(nNumAnts);
                pAntNull = new SQLLEN[nNumAnts][];

                // Now fetch the selected antennae data from the DB using a loop.
                nInd = 0;  //Counts the number of records fetched.
                FeAnte feAnte;
                SQLLEN[] nullInd;

                while (DynFeAnte.FeFetchAnte(nAnteCursor, out feAnte, out nullInd) == Constant.SUCCESS)
                {
                    // Sanity-check the number of antennae read from the DB so far.
                    if (nInd > nNumAnts)
                    {
                        Log2.e("\nFeUtils.FeGetSiteWN(): ERROR: Marker J: SQLFetch(): antennae count exceeds that of DbCountRows()");
                        DynFeAnte.FeCloseAnte(nAnteCursor);
                        GenUtil.SetError(1107, "*ERROR* feGetSiteWN: Antenna counts wrong.");
                        return (-10);
                    }

                    // Accumulate the data over successive fetch-loops.
                    pAnts[nInd] = feAnte;
                    pAntNull[nInd] = nullInd;

                    // Increment loop counter.
                    nInd++;

                } //end of while-loop

                // We have now accumulated all the antennae data - 'attach' it to feSiteStr.
                feSiteStr.stAnts = pAnts;
                feSiteStr.nNumAnts = nInd;

                // We have now accumulated all the antennae nullInd data - 'attach' it to feSiteNull.
                feSiteNull.anAntsNull = pAntNull;

                // Close the cursor to free ODBC resources etc.
                DynFeAnte.FeCloseAnte(nAnteCursor);

            } // else #1, nNumAnts > 0

            // Get azimuth info from the DB.

            // Get the full azimuth table name.
            GenUtil.UtCvtName(Constant.FE_AZIM, cTable, out cFullTableName);

            FeAzim[] pAzim = null;
            SQLLEN[][] pAzimNull = null;  // Usage: [antenna, column]
            int nNumAzim;

            // First find out how many azimuths there are in order to allocate the array.
            nNumAzim = Ssutil.DbCountRows(cFullTableName, whereClause);

            //...Log2.v("\r\nFeUtils.FeGetSiteWN(): nNumAzim = " + nNumAzim);

            // Check for failure of DbCountRows() to return valid number of azimuths.
            if (nNumAzim < 0)
            {
                Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker F1: DbCountRows() returned an invalid value: " + nNumAzim);
                return (nNumAzim);
            }

            // Handle the case of no azimuths to read from the DB.
            if (nNumAzim == 0)
            {
                Log2.w("\r\nFeUtils.FeGetSiteWN(): WARNING: Marker G1: number of azimuths to read from DB is zero.");
            }
            else // #1, nNumAzim > 0
            {
                // Select the azimuth rows.
                orderByClause = "location, call1, azim";
                int nAzimCursor = DynFeAzim.FeSelectAzim(cFullTableName, whereClause, orderByClause);
                if (nAzimCursor < 0)
                {
                    Log2.e("\nFeUtils.FeGetSiteWN(): ERROR: DynFeSite.FeSelectAzim() failed.");
                    return (Error.ODBC_SELECT_FAILED);
                }

                // Instantiate the objects required store the azimuths data (multiple fetches from the DB).
                pAzim = Arrays.CreateArrayUsingDefaultElementConstructor<FeAzim>(nNumAzim);
                pAzimNull = new SQLLEN[nNumAzim][];

                // Now fetch the selected azimuths data from the DB using a loop.
                nInd = 0;  //Counts the number of records fetched.
                FeAzim feAzim;
                SQLLEN[] nullInd;

                while (DynFeAzim.FeFetchAzim(nAzimCursor, out feAzim, out nullInd) == Constant.SUCCESS)
                {
                    // Sanity-check the number of azimuths read from the DB so far.
                    if (nInd > nNumAzim)
                    {
                        Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker J1: SQLFetch(): azimuths count exceeds that of DbCountRows()");
                        DynFeAzim.FeCloseAzim(nAzimCursor);
                        GenUtil.SetError(1107, "*ERROR* feGetSiteWN: Azimuth counts wrong.");
                        return (-10);
                    }

                    // Accumulate the data over successive fetch-loops.
                    pAzim[nInd] = feAzim;
                    pAzimNull[nInd] = nullInd;

                    // Increment the loop counter.
                    nInd++;

                } //end of while-loop

                // We have now accumulated all the azimuths data - 'attach' it to feSiteStr.
                feSiteStr.stAzim = pAzim;
                feSiteStr.nNumAzim = nInd;
                // We have now accumulated all the azimuths nullInd data - 'attach' it to feSiteNull.
                feSiteNull.anAzimNull = pAzimNull;

                // Close the azim cursor to release ODBC resources etc.
                DynFeAzim.FeCloseAzim(nAzimCursor);

            } // else #1, nNumAzim > 0

            // At this point we have the site, antennae and azimuths read in from the DB.  
            // If nDepth is 2, then this is all that was required and we can exit.  
            if (nDepth == 2)
            {
                //...Log2.v("\n\nFeUtils.FeGetSiteWN(): Exit (nDepth == 2)");
                return Constant.SUCCESS;
            }
            // End of fetching the antennae and azimuth data from the DB. -------------------------------------------

            // ============================================
            // Find and fetch the FeChan data from the DB.
            // ============================================

            // We have now loaded the site and antennae data (if any).  
            // If nDepth = 3, we need to fetch the channel data from the DB.

            if (nDepth == 3) // #3
            {
                // Get the full channel table name.
                GenUtil.UtCvtName(Constant.FE_CHAN, cTable, out cFullTableName);

                // Use these two objects to accumulate the channel data fetched from the DB.
                FeChan[] pChan = null;
                SQLLEN[][] pChanNull = null;

                // Determine how many channels there are in order to allocate the array.
                int nNumChan = Ssutil.DbCountRows(cFullTableName, whereClause);

                //...Log2.v("\r\nFeUtils.FeGetSiteWN(): nNumChan = " + nNumChan);

                if (nNumChan < 0)
                {
                    // An error occurred - deal with it.
                    Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker M: DbCountRows() returned nNumChan = " + nNumChan);
                    return -24;
                }

                // Handle the case of no channels to read (i.e. nNumChan == 0).
                else if (nNumChan == 0)
                {
                    Log2.w("\r\nFeUtils.FeGetSiteWN(): WARNING: Marker N: number of channels to read from DB is zero.");
                }

                // Handle the 'regular' case of nNumChan > 0.
                else  // #2, nNumChan > 0
                {
                    // Accumulate the channel and nullInd data in these two objects.
                    pChan = Arrays.CreateArrayUsingDefaultElementConstructor<FeChan>(nNumChan);
                    pChanNull = new SQLLEN[nNumChan][];

                    // Select the channel rows.
                    orderByClause = "location, call1, chid";
                    int nChanCursor = DynFeChan.FeSelectChan(cFullTableName, whereClause, orderByClause);
                    if (nChanCursor < 0)
                    {
                        Log2.e("\nFeUtils.FeGetSiteWN(): ERROR: DynFeSite.FeSelectChan() failed.");
                        return (Error.ODBC_SELECT_FAILED);
                    }

                    // Start a loop to fetch all the selected channel data from the DB.
                    nInd = 0;  //This counts the number of successful channel fetches so far.
                    FeChan ftChan;
                    SQLLEN[] nullInd;

                    while (DynFeChan.FeFetchChan(nChanCursor, out ftChan, out nullInd) == Constant.SUCCESS)
                    {
                        // Sanity-check the number of channel records read from the DB so far.
                        if (nInd > nNumChan)
                        {
                            Log2.e("\r\nFeUtils.FeGetSiteWN(): ERROR: Marker R: SQLFetch(): channel count exceeds that of DbCountRows()");
                            DynFeChan.FeCloseChan(nChanCursor);
                            return (-27);
                        }

                        //Accumulate the channel data over successive fetch-loops.
                        pChan[nInd] = ftChan;
                        pChanNull[nInd] = nullInd;

                        // Increment loop counter.
                        nInd++;

                    } // end of while fetch-loop.

                    // We have now accumulated all the channel data - 'attach' it to feSiteStr.
                    feSiteStr.stChan = pChan;
                    feSiteStr.nNumChans = nInd;
                    // We have now accumulated all the channel nullInd data - 'attach' it to feSiteNull.
                    feSiteNull.anChanNull = pChanNull;

                    // Tidy up.
                    DynFeChan.FeCloseChan(nChanCursor);

                } // #2, if nNumChan > 0


            } // #3, if (nDepth == 3)

            //...Log2.v("\n\nFeUtils.FeGetSiteWN(): Exit (nDepth == 3)");
            return Constant.SUCCESS;
        }  //end of method








    }
}
