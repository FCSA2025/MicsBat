# Documented File: TeBuildSH.cs
**Repository Path:** `TpRunTsip20260126\TeBuildSH.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _DataStructures;

namespace TpRunTsip
{
    using _DataStructures;
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
    using _Auxlib;
    using System.Diagnostics;
    /// <summary>
    /// Provides a large number of methods
    /// used to create and populate the ES SH Tables with data.
    /// </summary>
    public class TeBuildSH
    {

        //----------------------------------------------------------------------

        private static string[] temp1Table;

        // Note: the length of this array can go up to 20,000 !
        private static TtTemp2[] temp2Table;

        //-----------------------------------------------------------------------

        /// <summary>
        /// This method provides top-level management for the creation of ES SH Tables
        /// and their population with results data.
        /// </summary>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <param name="paramName"> - name of paramater file.</param>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="tpParmNulls"> - ODBC nullInds associated with tpParm.</param>
        /// <param name="numEtCases"> - accumulated count of the number of Et cases.</param>
        /// <param name="numTeCases"> - accumulated count of the number of Te cases.</param>
        /// <param name="startDate"> - TSIP processing start date to be written to tpParm.</param>
        /// <param name="startTime"> - TSIP processing start date to be written to tpParm.</param>
        /// <returns></returns>
        public static int TeBuildSHTable(string tsipName,
                                            string paramName,
                                            ref TpParm tpParm,
                                            ref SQLLEN[] tpParmNulls,
                                            ref int numEtCases,
                                            ref int numTeCases,
                                            string startDate,
                                            string startTime)
        {
            //...Log2.v("\nTeBuildSH.TeBuildSHTable(): Entry");

            int rc;
            string teParmTableName;

            /* create empty SH TABLES */
            if ((rc = TeCreateTsipTables(paramName, tsipName)) != Constant.SUCCESS)
            {
                Log2.e("\n\nTeBuildSH.TeBuildSHTable(): ERROR: call to TeCreateTsipTables() failed, rc = " + rc);
                TpRunTsip.mTW_ERR.Write("\r\n*** Could not create TSIP Tables ({0})\r\n", rc);
                return (Constant.FAILURE);
            }

            /* store date and time */
            tpParm.mdate = startDate;
            tpParm.mtime = startTime;
            tpParm.numcases = -1;
            tpParm.numtecases = -1;
            tpParmNulls[TpParm.MDATE] = Constant.DB_NOT_NULL;
            tpParmNulls[TpParm.MTIME] = Constant.DB_NOT_NULL;
            tpParmNulls[TpParm.NUMCASES] = Constant.DB_NOT_NULL;
            tpParmNulls[TpParm.NUMTECASES] = Constant.DB_NOT_NULL;

            /* compose internal parameter table name */
            GenUtil.UtCvtName(Constant.TE_PARM, tsipName, out teParmTableName);

            /* insert parameter record into SH TABLE */
            if ((rc = TsipUtils.UtInsertParmRecord(teParmTableName, tpParm, tpParmNulls)) != Constant.SUCCESS)
            {
                TpRunTsip.mTW_ERR.Write("\r\n*** Could not insert Parameter Record into SH table ({0}).", rc);
                return (Constant.FAILURE);
            }
            tpParm.numcases = 0;
            tpParm.numtecases = 0;

            /* perform initial rough cull to extract a set of affected sites,
             * then fine cull, and create the temporary ts and es site pair tables.
             * Then from the two site pair tables, create the SH TABLES and
             * complete with interference calculations */

            if ((rc = TeCullNCreate(ref tpParm, paramName, tsipName, ref numEtCases, ref numTeCases)) != Constant.SUCCESS)
            {
                if (rc != Constant.FAILURE)
                {
                    TpRunTsip.mTW_ERR.Write("\r\n*** Error culling the table ({0}).\r\n", rc);
                    ErrMsg.UtPrintMessage(rc, "", "", "", "", "");
                }

                // Drop the temporary DB tables.
                RemoveTempTables(paramName);
                return (Constant.FAILURE);
            }

            // Drop the temporary DB tables.
            RemoveTempTables(paramName);

            //...Log2.v("\nTeBuildSH.TeBuildSHTable(): Exit");
            return (Constant.SUCCESS);

        } /*---- end teBuildSHTable ----*/

        /// <summary>
        /// This method creates the empty SH and Temporary 
        /// tables to be populated by TSIP.  
        /// </summary>
        /// <param name="paramName"> - name of paramater file.</param>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <returns></returns>
        public static int TeCreateTsipTables(string paramName, string tsipName)
        {
            //...Log2.v("\nTeBuildSH.TeCreateTsipTables(): Entry");

            int rc;

            if (Ssutil.UtTableExist(Constant.TE, tsipName))
            {
                if ((rc = Ssutil.UtDropTable(Constant.TE, tsipName)) != Constant.SUCCESS)
                {
                    return (-666);
                }
            }

            if ((rc = Ssutil.UtCreateTable(Constant.TE, tsipName)) != Constant.SUCCESS)
            {
                return (-667);
            }

            /* create temporary tables */
            /* if the (TS) temporary table exists drop it */
            if (Ssutil.UtTableExist(Constant.TT_TEMP1, paramName))
            {
                if ((rc = Ssutil.UtDropTable(Constant.TT_TEMP1, paramName)) != Constant.SUCCESS)
                {
                    return (-668);
                }
            }
            /* create a one dimensional temporary table */
            if ((rc = Ssutil.UtCreateTable(Constant.TT_TEMP1, paramName)) != Constant.SUCCESS)
            {
                return (-669);
            }

            /* if the temporary (TS) table exists drop it */
            if (Ssutil.UtTableExist(Constant.TT_TEMP2, paramName))
            {
                if ((rc = Ssutil.UtDropTable(Constant.TT_TEMP2, paramName)) != Constant.SUCCESS)
                {
                    return (-670);
                }
            }
            /* create a two dimensional temporary environment (TS) table */
            if ((rc = Ssutil.UtCreateTable(Constant.TT_TEMP2, paramName)) != Constant.SUCCESS)
            {
                return (-671);
            }

            /* if the (ES) temporary table exists drop it */
            if (Ssutil.UtTableExist(Constant.TE_TEMP1, paramName))
            {
                if ((rc = Ssutil.UtDropTable(Constant.TE_TEMP1, paramName)) != Constant.SUCCESS)
                {
                    return (-672);
                }
            }
            /* create an (ES) temporary table */
            if ((rc = Ssutil.UtCreateTable(Constant.TE_TEMP1, paramName)) != Constant.SUCCESS)
            {
                return (-673);
            }

            //...Log2.v("\nTeBuildSH.TeCreateTsipTables(): Exit");

            return (Constant.SUCCESS);

        } /*---- end teCreateTsipTables ----*/

        /// <summary>
        /// This method deletes the temporary tables when they are no longer needed.  
        /// </summary>
        /// <param name="paramName"> - name of paramater file.</param>
        public static void RemoveTempTables(string paramName)
        {
            //...Log2.v("\nTeBuildSH.RemoveTempTables(): Entry");

            Ssutil.UtDropTable(Constant.TT_TEMP1, paramName);
            Ssutil.UtDropTable(Constant.TT_TEMP2, paramName);
            Ssutil.UtDropTable(Constant.TE_TEMP1, paramName);

            //...Log2.v("\nTeBuildSH.RemoveTempTables(): Exit");

        } /*---- end removeTempTables ----*/

        /// <summary>
        /// This method controls the selection of the sites, 
        /// antennae, and channels from the environment that satisfy the User's
        /// requirements and then populates the associated fields in the SH Tables.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="paramName"> - name of paramater file.</param>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <param name="numEtCases"> - accumulated count of the number of Et cases.</param>
        /// <param name="numTeCases"> - accumulated count of the number of Te cases.</param>
        /// <returns></returns>
        public static int TeCullNCreate(ref TpParm tpParm, string paramName, string tsipName, ref int numEtCases, ref int numTeCases)
        {
            //...Log2.v("\nTeBuildSH.TeCullNCreate(): Entry");

            string ttTemp1Name = null;
            string ttTemp2Name = null;
            string teTemp1Name = null;
            string teSiteTableName = null;
            string teAnteTableName = null;
            string teChanTableName = null;
            string tsSiteTableName = null;
            string tsAnteTableName = null;
            string tsChanTableName = null;
            string esSiteTableName = null;
            string esAnteTableName = null;
            string esChanTableName = null;
            string esAzimTableName = null;
            string codesSelection = null;
            bool isMDB;
            int rc;

            TeBuildShTableNames(
                                    tpParm,
                                    paramName,
                                    tsipName,
                                    out ttTemp1Name,
                                    out ttTemp2Name,
                                    out teTemp1Name,
                                    out teSiteTableName,
                                    out teAnteTableName,
                                    out teChanTableName,
                                    out tsSiteTableName,
                                    out tsAnteTableName,
                                    out tsChanTableName,
                                    out esSiteTableName,
                                    out esAnteTableName,
                                    out esChanTableName,
                                    out esAzimTableName,
                                    out isMDB);

            //...Log2.v("\nTeBuildSH.TeCullNCreate(): A");

            /* create selection criteria for culling of oper. codes or call signs */
            TsipUtils.UtOpCodesCallSigns(tpParm, out codesSelection);

            //...Log2.v("\nTeBuildSH.TeCullNCreate(): B");

            if (tpParm.protype == "E")
            {
                //...Log2.v("\nTeBuildSH.TeCullNCreate(): C");

                /* if (proposed file type is "ES") */

                /* perform initial rough cull to extract TS sites which are
                 * roughly within the coordination distance, and create the
                 * earth location table. Also, create the terrestrial call1
                 * table of the sites in the proposed file */
                if ((rc = TeRoughCull(tpParm, esSiteTableName, tsSiteTableName,
                    esAnteTableName, esAzimTableName, ttTemp1Name,
                    teTemp1Name, codesSelection)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }
            else
            {
                //...Log2.v("\nTeBuildSH.TeCullNCreate(): D");

                /* (proposed file type is "TS") */

                /* perform initial rough cull to extract affected ES sites,
                 * and create the terrestrial call1 table.  Also, create the
                 * earth location table of the sites in the proposed file */
                if ((rc = TtRoughCull(tpParm, isMDB, tsSiteTableName, tsAnteTableName,
                    esSiteTableName, esAnteTableName, ttTemp1Name,
                    teTemp1Name, codesSelection)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }

            /* create the terrestrial site pair table from the terrestrial call1
             * table created in teRoughCull() or ttRoughCull() */
            if ((rc = Terr2DimTable(tpParm, tsSiteTableName, tsAnteTableName,
                ttTemp1Name, ttTemp2Name)) != Constant.SUCCESS)
            {
                return (rc);
            }

            //...Log2.v("\nTeBuildSH.TeCullNCreate(): E");

            /* from the terrestrial site pair table and earth location table,
             * create the SH TABLES */
            // AH: HERE WIP
            if ((rc = CreateSHTables(tpParm, isMDB, teSiteTableName, esSiteTableName,
                tsSiteTableName, teAnteTableName, esAnteTableName,
                tsAnteTableName, teChanTableName, esChanTableName,
                esAzimTableName, tsChanTableName, teTemp1Name,
                ttTemp2Name, ref numEtCases, ref numTeCases)) != Constant.SUCCESS)
            {
                return (rc);
            }

            //...Log2.v("\nTeBuildSH.TeCullNCreate(): Exit");
            return (Constant.SUCCESS);
        } /*---- end teCullNCreate ----*/

        /// <summary>
        /// This method builds the full SQL table names for the 
        /// SH Tables and temp tables. The names are built from the tsip parameter file 
        /// name and the runname of the record within that file.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="paramName"> - name of paramater file.</param>
        /// <param name="tsipName"> - name of PDF file.</param>
        /// <param name="ttTemp1Name"> - temporary unique filename to be used.</param>
        /// <param name="ttTemp2Name"> - temporary unique filename to be used.</param>
        /// <param name="teTemp1Name"> - temporary unique filename to be used.</param>
        /// <param name="teSiteTableName"> - name of Te site table.</param>
        /// <param name="teAnteTableName"> - name of Te ante table.</param>
        /// <param name="teChanTableName"> - name of Te chan table.</param>
        /// <param name="tsSiteTableName"> - name of Ts site table.</param>
        /// <param name="tsAnteTableName"> - name of Ts ante table.</param>
        /// <param name="tsChanTableName"> - name of Ts chan table.</param>
        /// <param name="esSiteTableName"> - name of Es site table.</param>
        /// <param name="esAnteTableName"> - name of Es ante table.</param>
        /// <param name="esChanTableName"> - name of Es chan table.</param>
        /// <param name="esAzimTableName"> - </param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        public static void TeBuildShTableNames(TpParm tpParm,
                                                string paramName,
                                                string tsipName,
                                                out string ttTemp1Name,
                                                out string ttTemp2Name,
                                                out string teTemp1Name,
                                                out string teSiteTableName,
                                                out string teAnteTableName,
                                                out string teChanTableName,
                                                out string tsSiteTableName,
                                                out string tsAnteTableName,
                                                out string tsChanTableName,
                                                out string esSiteTableName,
                                                out string esAnteTableName,
                                                out string esChanTableName,
                                                out string esAzimTableName,
                                                out bool isMDB)
        {
            //...Log2.v("\nTeBuildSH.TeBuildShTableNames(): Entry");

            /* ENVIRONMENT & PROPOSED TABLE NAMES */
            GenUtil.UtCvtName(Constant.TT_TEMP1, paramName, out ttTemp1Name);
            GenUtil.UtCvtName(Constant.TT_TEMP2, paramName, out ttTemp2Name);
            GenUtil.UtCvtName(Constant.TE_TEMP1, paramName, out teTemp1Name);

            //...Log2.v("\nTeBuildSH.TeBuildShTableNames(): ttTemp1Name = " + ttTemp1Name);
            //...Log2.v("\nTeBuildSH.TeBuildShTableNames(): ttTemp2Name = " + ttTemp2Name);
            //...Log2.v("\nTeBuildSH.TeBuildShTableNames(): teTemp1Name = " + teTemp1Name);

            GenUtil.UtCvtName(Constant.TE_SITE, tsipName, out teSiteTableName);
            GenUtil.UtCvtName(Constant.TE_ANTE, tsipName, out teAnteTableName);
            GenUtil.UtCvtName(Constant.TE_CHAN, tsipName, out teChanTableName);

            tpParm.envtype.Trim();
            isMDB = false;
            if (tpParm.protype.Equals("E"))
            {
                if (tpParm.envtype.Equals("MDB_TS"))
                {
                    isMDB = true;
                    tsSiteTableName = "main.mt_site";
                    tsAnteTableName = "main.mt_ante";
                    tsChanTableName = "main.mt_chan";
                }
                else
                {
                    GenUtil.UtCvtName(Constant.FT_SITE, tpParm.envname, out tsSiteTableName);
                    GenUtil.UtCvtName(Constant.FT_ANTE, tpParm.envname, out tsAnteTableName);
                    GenUtil.UtCvtName(Constant.FT_CHAN, tpParm.envname, out tsChanTableName);
                }
                GenUtil.UtCvtName(Constant.FE_SITE, tpParm.proname, out esSiteTableName);
                GenUtil.UtCvtName(Constant.FE_ANTE, tpParm.proname, out esAnteTableName);
                GenUtil.UtCvtName(Constant.FE_CHAN, tpParm.proname, out esChanTableName);
                GenUtil.UtCvtName(Constant.FE_AZIM, tpParm.proname, out esAzimTableName);
            }
            else
            {
                if (tpParm.envtype.Equals("MDB_ES"))
                {
                    isMDB = true;
                    esSiteTableName = "main.me_site";
                    esAnteTableName = "main.me_ante";
                    esChanTableName = "main.me_chan";
                    esAzimTableName = "main.me_azim";
                }
                else
                {
                    GenUtil.UtCvtName(Constant.FE_SITE, tpParm.envname, out esSiteTableName);
                    GenUtil.UtCvtName(Constant.FE_ANTE, tpParm.envname, out esAnteTableName);
                    GenUtil.UtCvtName(Constant.FE_CHAN, tpParm.envname, out esChanTableName);
                    GenUtil.UtCvtName(Constant.FE_AZIM, tpParm.envname, out esAzimTableName);
                }
                GenUtil.UtCvtName(Constant.FT_SITE, tpParm.proname, out tsSiteTableName);
                GenUtil.UtCvtName(Constant.FT_ANTE, tpParm.proname, out tsAnteTableName);
                GenUtil.UtCvtName(Constant.FT_CHAN, tpParm.proname, out tsChanTableName);
            }

            //...Log2.v("\nTeBuildSH.TeBuildShTableNames(): Exit");
        } /*---- end teBuildShTableNames */

        /// <summary>
        /// This method controls the culling of the environment 
        /// when the environment is either the ES MDB or a ES PDF.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="esSiteTableName"> - name of Es site table.</param>
        /// <param name="tsSiteTableName"> - name of Ts site table.</param>
        /// <param name="esAnteTableName"> - name of Es ante table.</param>
        /// <param name="esAzimTableName"> - name of Es azim table</param>
        /// <param name="ttTemp1Name"> - temporary unique filename to be used.</param>
        /// <param name="teTemp1Name"> - temporary unique filename to be used.</param>
        /// <param name="codesSelection"> - the operator code or call sign selection criteria from the specified codes to be used in the SQL query.</param>
        /// <returns></returns>
        public static int TeRoughCull(TpParm tpParm,
                                        string esSiteTableName,
                                        string tsSiteTableName,
                                        string esAnteTableName,
                                        string esAzimTableName,
                                        string ttTemp1Name,
                                        string teTemp1Name,
                                        string codesSelection)
        {
            //...Log2.v("\nTeBuildSH.TeRoughCull(): Entry");

            string sqlCommand;
            SQLLEN[] feSiteNulls;
            SQLLEN[] feAnteNulls;
            int feSiteHandle;
            int feAnteHandle;
            int rc;
            int rc2;
            int longMinBorder;
            int longMaxBorder;
            int deltaLong;
            int deltaLat;
            double maxCoordDistTX;
            double maxCoordDistRX;
            double maxCoordDist;
            string selectionCriteria;
            FeSite feSite;
            FeAnte feAnte;
            string cDistSel;

            // Use this list to accumulate call1 strings.
            List<string> call1List = new List<SQLCHARPTR>();

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* ONE DIMENSIONAL TABLE - ROUGH CULL
             * Walk through each record in earth table selecting records from the
             * environment that fall within desired lat/long range. Selected
             * records are inserted into the TE_TEMP1 temporary table
             */

            //	Clean table
            Ssutil.DbDeleteRows(teTemp1Name, null);

            if ((feSiteHandle = DynFeSite.FeSelectSite(esSiteTableName, "cmd != 'D'", "")) < 0)
            {
                return (feSiteHandle);
            }

            // Loop over each Earth Station location in the user's fe_XXX_site table.
            while ((rc = DynFeSite.FeFetchSite(feSiteHandle, out feSite, out feSiteNulls)) == Constant.SUCCESS)
            {

                /* check each site's azimuth records for loss value */
                rc2 = CheckPDFAzimuthLoss(esAzimTableName, feSite.location);

                /* add the current location from earth table to the
                 * temporary earth location table */
                sqlCommand = String.Format("insert into {0} (location) values ('{1}')",
                    teTemp1Name, feSite.location);
                sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);
                if (!ODBC.IsOK(sqlRet))
                {
                    Ssutil.DbGetDiagStmt(hStmt,
                        "teRoughCull02: Could not insert location (" + feSite.location + ") into temp table.");
                    return -2;
                }


                selectionCriteria = String.Format("location = '{0}'", feSite.location);
                if ((feAnteHandle = DynFeAnte.FeSelectAnte(esAnteTableName, selectionCriteria, "")) < 0)
                {
                    return (feAnteHandle);
                }

                // Loop over all antennae in the user's fe_XXX_ante table that are associated with
                // the current Earth Station location.
                while ((rc = DynFeAnte.FeFetchAnte(feAnteHandle, out feAnte, out feAnteNulls)) == Constant.SUCCESS)
                {
                    if (tpParm.selsites.Equals("CALL SIGN"))
                    {
                        /*	We do not worry about the lat/long box or coordination distance if
                        *		we are selecting by call sign. GJS - 2002.05.20 */
                        cDistSel = "(1=1)";
                    }
                    else
                    {
                        /* calculate the delta latitude and longitude from max
                             of feAnte.txtro and feAnte.txpre ranges*/

                        maxCoordDistTX = 0.0;
                        maxCoordDistRX = 0.0;

                        if (feAnteNulls[FeAnte.TXBAND] != Constant.DB_NULL)
                        {
                            /* use the max of txtro and txpre for the
                             * coordination distance */
                            maxCoordDistTX = (feAnte.txtro > feAnte.txpre) ?
                                feAnte.txtro : feAnte.txpre;
                        }

                        if (feAnteNulls[FeAnte.RXBAND] != Constant.DB_NULL)
                        {
                            maxCoordDistRX = (feAnte.rxtro > feAnte.rxpre) ?
                                feAnte.rxtro : feAnte.rxpre;
                        }

                        maxCoordDist = (maxCoordDistTX > maxCoordDistRX) ?
                            maxCoordDistTX : maxCoordDistRX;

                        /* using this coordination distance and location of the
                         * current proposed ES site, calculate the latitudes
                         * and longitudes for a rough culling box (adjustments
                         * are also made when the longitude is close to 180 deg
                         * (E/W)), utDeltaLatLong() and addl calcs */

                        GenUtil.UtDeltaLatLong(feSite.latit, maxCoordDist, out deltaLat, out deltaLong);
                        longMinBorder = feSite.longit - deltaLong;
                        longMaxBorder = feSite.longit + deltaLong;

                        /* adjustement when the longitude is close to
                         * 180 degrees (E/W) */
                        if (longMinBorder < -Constant.LONG_MAXIMUM)
                        {
                            longMinBorder += (2 * Constant.LONG_MAXIMUM);
                        }
                        if (longMaxBorder > Constant.LONG_MAXIMUM)
                        {
                            longMaxBorder -= (2 * Constant.LONG_MAXIMUM);
                        }

                        cDistSel = String.Format("(a.latit >= {0} and a.latit <= {1} and a.longit >= {2} and a.longit <= {3})",
                            feSite.latit - deltaLat,
                            feSite.latit + deltaLat,
                            longMinBorder,
                            longMaxBorder);
                    }

                    /*  This is the non-keyhole cull version. */
                    sqlCommand = String.Format("insert into {0} (call1) select call1 from {1} a  where {2} {3}",
                        ttTemp1Name, tsSiteTableName,
                        cDistSel, codesSelection);

                    /* if environment is not MDB, exclude deleted records */
                    if (!tpParm.envtype.Equals("MDB_TS"))
                    {
                        sqlCommand += " and a.cmd != 'D'";
                    }

                    //...Log2.v("\nTeBuildSH.TeRoughCull(): sqlCommand = " + sqlCommand);

                    sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);
                    if (!(ODBC.IsOK(sqlRet) || sqlRet == ODBC.SQL_NO_DATA))
                    {
                        Ssutil.DbGetDiagStmt(hStmt,
                            "teRoughCull03: Could not insert call signs into temp table.\n\tSQL is:\n" + sqlCommand);
                        return -3;
                    }

                } // End of loop over all antennae in the user's fe_XXX_ante table that are associated with
                  // the current Earth Station location.

                if (rc != Constant.NOMORERECS)
                {
                    return (rc);
                }

                DynFeAnte.FeCloseAnte(feAnteHandle);

            } /* End of loop over each Earth Station location in the user's fe_XXX_site table. */

            if (rc != Constant.NOMORERECS)
            {
                return (rc);
            }

            DynFeSite.FeCloseSite(feSiteHandle);

            // AH:
            // Date: 22-Aug-2019.
            // Bug Fix b190626A : tsip ests duplicate cases.
            // We need to delete the duplicate rows in table ttTemp1Name that have the same call1.
            string SQL = String.Format("WITH TableBWithRowID AS ( SELECT ROW_NUMBER() OVER (ORDER BY call1) AS RowID, call1  FROM {0} )  DELETE o FROM TableBWithRowID o WHERE RowID < (SELECT MAX(rowID) FROM TableBWithRowID i WHERE i.call1=o.call1 GROUP BY call1)",
                                        ttTemp1Name);
            Ssutil.DbExecute(SQL);

            //...Log2.v("\nTeBuildSH.TeRoughCull(): ttTemp1Name = " + ttTemp1Name);

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nTeBuildSH.TeRoughCull(): Exit");

            return (Constant.SUCCESS);
        } /*---- end teRoughCull ----*/

        /// <summary>
        /// This method tests if an azimuth loss value is non-zero and 
        /// writes out a warning message to the .ERR report.
        /// </summary>
        /// <param name="azimname"> - name of the azim table in the DB.</param>
        /// <param name="location"> - name of location to be used in SQL select clause.</param>
        /// <returns></returns>
        public static int CheckPDFAzimuthLoss(string azimname, string location)
        {
            string select;
            FeAzim feAzim;
            SQLLEN[] azimNulls;
            int handle, rc;

            select = String.Format("location = '{0}'", location.Trim());

            if ((handle = (short)DynFeAzim.FeSelectAzim(azimname, select, "")) < 0)
            {
                return (handle);
            }

            /* for (each record in the earth proposed file) */
            rc = DynFeAzim.FeFetchAzim(handle, out feAzim, out azimNulls);
            while (rc == Constant.SUCCESS)
            {
                if ((azimNulls[FeAzim.LOSS] != Constant.DB_NULL) && (feAzim.loss != 0.0))
                {
                    TpRunTsip.mTW_ERR.Write("WARNING - Azimuth Record: {0} {1} {2,5:F2} has a loss value of {3,5:F2}\r\n",
                            feAzim.location.Trim(), feAzim.call1.Trim(), feAzim.azim, feAzim.loss);
                }
                rc = DynFeAzim.FeFetchAzim(handle, out feAzim, out azimNulls);
            }

            DynFeAzim.FeCloseAzim(handle);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method controls the culling of the environment when the environment 
        /// is either the TS MDB or a TS PDF. A one dimensional TS table an a one 
        /// dimensional ES table are created. To increase real-time performance
        /// use is made of user-defined functions that run on the SQL Server.
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="tsSiteTableName"> - name of Ts site table.</param>
        /// <param name="tsAnteTableName"> - name of Ts ante table.</param>
        /// <param name="esSiteTableName"> - name of Es site table.</param>
        /// <param name="esAnteTableName"> - name of Es ante table.</param>
        /// <param name="ttTemp1Name"> - temporary unique filename.</param>
        /// <param name="teTemp1Name"> - temporary unique filename.</param>
        /// <param name="codesSelection">- the operator code or call sign selection criteria from the specified codes to be used in the SQL query.</param>
        /// <returns></returns>
        public static int TtRoughCull(TpParm tpParm,
            bool isMDB,
            string tsSiteTableName,
            string tsAnteTableName,
            string esSiteTableName,
            string esAnteTableName,
            string ttTemp1Name,
            string teTemp1Name,
            string codesSelection)
        {
            //...Log2.v("\nTeBuildSH.TtRoughCull(): Entry");

            string sqlCommand;
            long lAvLat;
            long lAvLng;
            SQLLEN nNull;
            float fRadius;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* ONE DIMENSIONAL TABLE - ROUGH CULL */

            //	Clean out the temporary table.
            Ssutil.DbDeleteRows(ttTemp1Name, null);
            Ssutil.DbDeleteRows(teTemp1Name, null);

            /****************************************************************************\
            *
            *		Calculate the centroid of the ts table and the radius.
            *
            \****************************************************************************/
            sqlCommand = String.Format("Select AVG(CAST(s.latit AS bigint)), AVG(CAST(s.longit AS bigint))   From {0} s  Where cmd != 'D' ",
                tsSiteTableName);

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\n\nTeBuildSH.TtRoughCull(): ERROR: SQLExecDirect() failed for sqlCommand = {0}", sqlCommand);
            }

            sqlRet = ODBC.SQLFetch(hStmt);

            sqlRet = (SQLRETURN)Ssutil.DbGetLong(hStmt, 1, "AvLat", out lAvLat, out nNull);
            sqlRet = (SQLRETURN)Ssutil.DbGetLong(hStmt, 2, "AvLng", out lAvLng, out nNull);

            // To reuse the statement handle we must first close the open cursor.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE);

            //	This gives us the centroid, now find the maximum radius from this centroid in the
            //	ts file.
            sqlCommand = String.Format("SELECT Max([tsip].[distance_hs]({0}, {1}, s.latit, s.longit))   FROM {2} s  WHERE cmd != 'D' ",
                lAvLat, lAvLng, tsSiteTableName);

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);

            if (!ODBC.IsOK(sqlRet))
            {
                Ssutil.DbGetDiagStmt(hStmt, "ttRoughCull10: Could not get radius");
                return -12;
            }
            sqlRet = ODBC.SQLFetch(hStmt);

            sqlRet = (SQLRETURN)Ssutil.DbGetFloat(hStmt, 1, "MaxRadius", out fRadius, out nNull);

            // To reuse the statement handle we must first close the open cursor.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE);

            //	We now have the centroid and the maximum distance from the centroid of all the
            //	sites in the proposed ts file.  Now we go through the environment and select 
            //	all those sites that are within the distance specified on their antenna records
            //	to the centroid plus the radius plus 2km (because we are using great circle math
            //	here on an ellipsoidal earth).
            sqlCommand = String.Format("INSERT INTO {0} SELECT DISTINCT s.location, 0, 0, ''   FROM {1} s JOIN {2} a ON s.location = a.location  WHERE [tsip].[distance_hs]({3}, {4}, s.latit, s.longit) <= [tsip].[maxnum]({5},([tsip].[maxnum]([tsip].[maxnum](a.txtro, a.txpre), [tsip].[maxnum](a.rxtro, a.rxpre)) + {6})) + 2.0 ",
                teTemp1Name, esSiteTableName, esAnteTableName,
                lAvLat, lAvLng, tpParm.coordist, fRadius);

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);

            // To reuse the statement handle we must first close the open cursor.
            ODBC.SQLFreeStmt(hStmt, ODBC.SQL_CLOSE);

            //	Now add the ts call signs to the tt_tmp1 table.
            sqlCommand = String.Format("INSERT INTO {0} SELECT s.call1, 0, 0, ''   FROM {1} s  WHERE s.cmd != 'D' ",
                ttTemp1Name, tsSiteTableName);
            sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);

            // Release ODBC resources.
            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nTeBuildSH.TtRoughCull(): Exit");
            return (Constant.SUCCESS);
        } /*---- end ttRoughCull ----*/

        /// <summary>
        /// This method populates the temporary table 
        /// containing all pairs of communicating TS sites withing the culling range 
        /// or proposed PDF.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="tsSiteTableName"> - name of Ts site table.</param>
        /// <param name="tsAnteTableName"> - name of Ts ante table.</param>
        /// <param name="ttTemp1Name"> - temporary unique filename.</param>
        /// <param name="ttTemp2Name"> - temporary unique filename.</param>
        /// <returns></returns>
        public static int Terr2DimTable(TpParm tpParm, string tsSiteTableName, string tsAnteTableName, string ttTemp1Name, string ttTemp2Name)
        {
            //...Log2.v("\nTeBuildSH.Terr2DimTable(): Entry");

            int counter = 0;
            string sqlCommand;

            SQLLEN[] tsAnteNulls;
            SQLLEN[] ftSiteNulls;
            int tsAnteHandle, ttTemp1Handle, rc;
            string oldCallTwo;
            FtSite ftSite;
            FtAnte ftAnte;
            int nCount = 0;
            string call1;
            SQLLEN call1NullInd;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /* TWO DIMENSIONAL VICTIM TABLE
             * for each site falling into the culling range, identify all it's
             * communicating sites and insert the site pair into the terrestrial
             * temporary table */
            //	Clean table
            Ssutil.DbDeleteRows(ttTemp2Name, null);

            if ((ttTemp1Handle = TtDynTemp1.TtSelectTemp1(ttTemp1Name, "", "")) < 0)
            {
                Log2.e("\nTeBuildSH.Terr2DimTable(): ODBC 'Select' failed, return code = " + ttTemp1Handle);
                return (ttTemp1Handle);
            }

            /* for (each site in the temporary TS table) */
            while ((rc = TtDynTemp1.TtFetchTemp1(ttTemp1Handle, out call1, out call1NullInd)) == Constant.SUCCESS)
            {
                oldCallTwo = "";

                /* find all antennas from the TS table whose call1 is the
                 * same as the current call1 from the temp. TS table
                 * (ordered by call2) */

                sqlCommand = String.Format("call1 = '{0}'", call1);

                if ((tsAnteHandle = TpMdbPdfGet.SelectTerrAnte(tsAnteTableName, sqlCommand, tpParm.envtype)) < 0)
                {
                    return (tsAnteHandle);
                }

                /* for (each temporary TS site) */
                while ((rc = TpMdbPdfGet.FetchTerrAnte(tsAnteHandle, out ftAnte, out tsAnteNulls, tpParm.envtype)) == Constant.SUCCESS)
                {
                    nCount++;
                    if (!oldCallTwo.Equals(ftAnte.call2))
                    {
                        /* if (oldCall2 is different from curr call2) */

                        /* insert the (call1, call2) pair into the temporary TS pairs table */
                        sqlCommand = String.Format("insert into {0} (call1, call2) values ('{1}', '{2}')",
                            ttTemp2Name, ftAnte.call1, ftAnte.call2);

                        sqlRet = ODBC.SQLExecDirect(hStmt, sqlCommand, sqlCommand.Length);
                        if (!ODBC.IsOK(sqlRet))
                        {
                            Log2.e("\nTeBuildSH.Terr2DimTable(): ERROR: SQLExecDirect() failed for:\n" + sqlCommand);
                            string str = String.Format("terr2DimTable02: Could not insert link ({0}-{1}) into temp table.", ftAnte.call1, ftAnte.call2);
                            Ssutil.DbGetDiagStmt(hStmt, str);
                            return Error.ODBC_EXECDIRECT_FAILED;
                        }

                        /* Now do Orbit calcs if the user requested them.
                         * Orbit calcs are done once for each pair combination
                         * of TS sites */
                        if ((Strings.FirstCharIs(tpParm.protype, 'T')) && (Strings.FirstCharIs(tpParm.tsorbout, 'Y')))
                        {
                            sqlCommand = String.Format("call1 = '{0}' and call2 = '{1}'",
                                ftAnte.call2, ftAnte.call1);

                            counter = Ssutil.DbCountRows(ttTemp2Name, sqlCommand);

                            if ((counter == 0) || (Strings.FirstCharIs(ftAnte.offazm, 'Y')))
                            {
                                /* get local site Info */
                                if ((rc = TpMdbPdfGet.TtSiteGetCall(ftAnte.call1, tsSiteTableName, false, out ftSite, out ftSiteNulls)) != Constant.SUCCESS)
                                {
                                    return (rc);
                                }

                                /* perform Orbit calcs */
                                if ((rc = TtCalcs.TtTsorbCalcs(tpParm, ftAnte.offazm, ftAnte.aht,
                                    ftAnte.call2, ftAnte.bndcde, ftAnte.tazmth,
                                    ftAnte.telvtn, ftSite)) != Constant.SUCCESS)
                                {
                                    return (rc);
                                }
                            }
                        }

                        oldCallTwo = ftAnte.call2;

                    }
                    else if (Strings.FirstCharIs(ftAnte.offazm, 'Y'))
                    {
                        /* if (true azim/elev values are set in the ES ante rec) */

                        /* orbit calcs are also done for all antennas with
                         * true azim/elev values */
                        if ((Strings.FirstCharIs(tpParm.protype, 'T')) && (Strings.FirstCharIs(tpParm.tsorbout, 'Y')))
                        {
                            /* get local site Info */
                            if ((rc = TpMdbPdfGet.TtSiteGetCall(ftAnte.call1, tsSiteTableName, false, out ftSite,
                                out ftSiteNulls)) != Constant.SUCCESS)
                            {
                                return (rc);
                            }

                            if ((rc = TtCalcs.TtTsorbCalcs(tpParm, ftAnte.offazm, ftAnte.aht,
                                ftAnte.call2, ftAnte.bndcde, ftAnte.tazmth,
                                ftAnte.telvtn, ftSite)) != Constant.SUCCESS)
                            {
                                return (rc);
                            }
                        }
                    }
                } /* end for (each temporary TS site) */

                if (rc != Constant.NOMORERECS)
                {
                    return (rc);
                }

                TpMdbPdfGet.CloseTerrAnte(tsAnteHandle, tpParm.envtype);
            }

            TtDynTemp1.TtCloseTemp1(ttTemp1Handle);

            if (rc != Constant.NOMORERECS)
            {
                return (rc);
            }

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nTeBuildSH.Terr2DimTable(): Exit");
            return (Constant.SUCCESS);
        } /*---- end terr2DimTable ----*/

        /// <summary>
        /// This method manages the population of the SH 
        /// Tables with data once the rough cull has been completed.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="isMDB"> - indicates whether the tables are in the main DB table set, or not.</param>
        /// <param name="teSiteTableName"> - name of Te site table.</param>
        /// <param name="esSiteTableName"> - name of Es site table.</param>
        /// <param name="tsSiteTableName"> - name of Ts site table.</param>
        /// <param name="teAnteTableName"> - name of Te ante table.</param>
        /// <param name="esAnteTableName"> - name of Es ante table.</param>
        /// <param name="tsAnteTableName"> - name of Ts ante table.</param>
        /// <param name="teChanTableName"> - name of Te chan table.</param>
        /// <param name="esChanTableName"> - name of Es chan table.</param>
        /// <param name="esAzimTableName"> - name of Es azim table.</param>
        /// <param name="tsChanTableName"> - name of Ts azim table.</param>
        /// <param name="teTemp1Name"> - temporary unique filename</param>
        /// <param name="ttTemp2Name"> - temporary unique filename</param>
        /// <param name="numEtCases"> - accumulated count of the number of Et cases.</param>
        /// <param name="numTeCases"> - accumulated count of the number of Te cases.</param>
        /// <returns></returns>
        public static int CreateSHTables(TpParm tpParm,
                                            bool isMDB,
                                            string teSiteTableName,
                                            string esSiteTableName,
                                            string tsSiteTableName,
                                            string teAnteTableName,
                                            string esAnteTableName,
                                            string tsAnteTableName,
                                            string teChanTableName,
                                            string esChanTableName,
                                            string esAzimTableName,
                                            string tsChanTableName,
                                            string teTemp1Name,
                                            string ttTemp2Name,
                                            ref int numEtCases,
                                            ref int numTeCases)
        {
            //...Log2.v("\nTeBuildSH.CreateSHTables(): Entry");

            SQLLEN[] esSiteNulls;  //[ME_SITE_SIZE_],
            SQLLEN[] tsSiteNulls = null;  //[FtSite.SIZE_],
            SQLLEN[] teSiteNulls;  //[TE_SITE_SIZE_],
            SQLLEN teTemp1Nulls;  //[TE_TEMP1_SIZE_];

            int teTemp1Handle;
            int ttTemp2Count;
            bool distFail = false;
            int numAnte;
            int i;
            int rc;
            string oldTtCallOne;
            string select = "";
            string intPrintMsg;
            string vicPrintMsg;

            FtSite tsSite = null;
            FeSite esSite;
            TeSite teSite;

            string teTemp1;

            int nTemp1Count = 0;
            int nTemp1Counter;

            /* read all records from the temporary TS pairs table into memory */
            if ((rc = InitTemp2(out temp2Table, out ttTemp2Count, ttTemp2Name)) != Constant.SUCCESS)
            {
                return (rc);
            }

            //...Log2.v("\nTeBuildSH.CreateSHTables(): ttTemp2Count = " + ttTemp2Count);
            // AH: REMOVE!
            //ttTemp2Count = 1;

            /* prepare SH SITE, ANTENNA, and CHANNEL tables for insert */

            if ((rc = TeDynSite.TePrepareSite(teSiteTableName)) != Constant.SUCCESS)
            {
                return (rc);
            }

            if ((rc = TeDynAnte.TePrepareAnte(teAnteTableName)) != Constant.SUCCESS)
            {
                return (rc);
            }

            if ((rc = TeDynChan.TePrepareChan(teChanTableName)) != Constant.SUCCESS)
            {
                return (rc);
            }

            /* read the EARTH records */
            /*  GJS 07/12/99 In order to commit at the end of each item in the
            *   temp1 table, we cannot keep a cursor open over the whole table,
            *   since a commit closes all cursors.  Keeping this outer loop cursor
            *   open til the end results in an unacceptably large transaction file.
            *
            *   What we must do then is read the Temp1 table into memory and use it
            *   as the outer loop.
            *
            *   Start by getting its size. */
            nTemp1Count = Ssutil.DbCountRows(teTemp1Name, null);

            //...Log2.v("\nTeBuildSH.CreateSHTables(): teTemp1Name = " + teTemp1Name);
            //...Log2.v("\nTeBuildSH.CreateSHTables(): nTemp1Count = " + nTemp1Count);

            /*  Now load up the teTemp1 array with the table */
            if ((teTemp1Handle = TeDynTemp1.TeSelectTemp1(teTemp1Name, "", "location")) < 0)
            {
                return (teTemp1Handle);
            }

            List<string> teTemp1List = new List<string>();

            nTemp1Counter = 0;
            while (TeDynTemp1.TeFetchTemp1(teTemp1Handle, out teTemp1, out teTemp1Nulls) == Constant.SUCCESS)
            {
                teTemp1List.Add(teTemp1);
            }

            temp1Table = teTemp1List.ToArray();

            nTemp1Counter = temp1Table.Length;

            TeDynTemp1.TeCloseTemp1(teTemp1Handle);

            /*  Do a sanity check. */
            if (nTemp1Counter != nTemp1Count)
            {
                Log2.e("\nTeBuildSH.CreateSHTables(): ERROR: temp1Table inconsistency.");
                TpRunTsip.mTW_ERR.Write("/n*ERROR* Temp1 table inconsistent. /n");
                return (Constant.FAILURE);
            }

            /*  Now go through the internal array of temp1 elements
            *   for (each record in the earth location table) */
            nTemp1Counter = 0;
            while (nTemp1Counter < nTemp1Count)
            {
                teTemp1 = teTemp1List[nTemp1Counter];

                nTemp1Counter++;

                intPrintMsg = String.Format("INTERFERER:   {0,11}", teTemp1);

                /* retrieve the earth record for the current location from
                 * the PDF or MDB */
                if (Strings.FirstCharIs(tpParm.protype, 'E'))
                {
                    rc = TpMdbPdfGet.TeSiteGetLoc(teTemp1, esSiteTableName, false, out esSite, out esSiteNulls);
                }
                else
                {
                    rc = TpMdbPdfGet.TeSiteGetLoc(teTemp1, esSiteTableName, isMDB, out esSite, out esSiteNulls);
                }


                if (rc != Constant.SUCCESS)
                {
                    Log2.e("\nTeBuildSH.CreateSHTables(): ERROR: TeSiteGetLoc failed, rc = " + rc);
                    ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                    ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", select);
                    return (rc);
                }

                //...Log2.v("\nTeBuildSH.CreateSHTables(): esSite = " + esSite.KeysToString());

                oldTtCallOne = "";
                TtTemp2 ttTemp2;

                /* for (each record in the temporary TS pairs table) */
                for (i = 0; i < ttTemp2Count; i++)
                {
                    ttTemp2 = temp2Table[i];

                    //...Log2.v("\nTeBuildSH.CreateSHTables(): i = " + i);
                    //...Log2.v("\nTeBuildSH.CreateSHTables(): ttTemp2 = \n" + ttTemp2.ToString());

                    vicPrintMsg = String.Format("VICTIM:   {0,10}     {0,10}", ttTemp2.call1, ttTemp2.call2);

                    ///*	check and set the debugging case switch */
                    //if (glbStopCase != 0)
                    //{
                    //    glbStopCase = teDbgCheckCase("", "", ttTemp2.temp2.call1, ttTemp2.temp2.call2);
                    //}
                    //if (glbStopCase > 0)
                    //{
                    //    //int nRet = 1;
                    //}

                    /* if (oldTtCallOne is different from curr. temp TS call1) */
                    if (!ttTemp2.call1.Equals(oldTtCallOne))
                    {
                        /* retrieve the terrestrial record for the
                         * current site from the PDF or MDB */
                        if (Strings.FirstCharIs(tpParm.protype, 'T'))
                        {
                            rc = TpMdbPdfGet.TtSiteGetCall(ttTemp2.call1, tsSiteTableName, false, out tsSite, out tsSiteNulls);
                        }
                        else
                        {
                            rc = TpMdbPdfGet.TtSiteGetCall(ttTemp2.call1, tsSiteTableName, isMDB, out tsSite, out tsSiteNulls);
                        }

                        if (rc != Constant.SUCCESS)
                        {
                            Log2.e("\nTeBuildSH.CreateSHTables(): ERROR: TtSiteGetCall failed, rc = " + rc);
                            ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                            ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                            ErrMsg.UtPrintMessage(Error.FETCH_FAIL, "SITE", select);
                            return (rc);
                        }
                        oldTtCallOne = ttTemp2.call1;
                    }
                    else if (distFail == true)
                    {
                        /* same call1 as last and distCull failed
                         * so go on to next temp. TS pairs record */
                        continue;
                    }

                    /* cull the environment information based on operator
                     * codes (if the user specified any) and country */
                    if (SiteCull(tpParm, tsSite, esSite) == Constant.SUCCESS)
                    {
                        distFail = false;

                        /* set the values for an ES Site SH record
                         * from the pro. and env. files, setTeSite() -
                         * calls teSiteCalcs() to finish the
                         * population of the ES Site SH record. */

                        rc = SetTeSite(tpParm, tsSite, tsSiteNulls, esSite, esSiteNulls,
                            out teSite, out teSiteNulls, ttTemp2.call2, intPrintMsg,
                            vicPrintMsg);

                        if ((rc != Constant.SUCCESS) && (rc != Constant.CONT_PROCESSING))
                        {
                            return (rc);
                        }

                        /* CONT_PROCESSING indicates that TS remote site data could
                         * not be located - simply skip the record */
                        numAnte = 0;

                        if (rc == Constant.SUCCESS)
                        {
                            /* from SH Site create SH ANTENNA */
                            // AH: HERE 20180209
                            if ((rc = CreateAnteTable(tpParm, ref teSite, ref teSiteNulls, esAnteTableName,
                                tsAnteTableName, esChanTableName, tsChanTableName,
                                ref numAnte, ref numEtCases, ref numTeCases)) != Constant.SUCCESS)
                            {
                                return (rc);
                            }
                        }

                        if (numAnte > 0)
                        {
                            //...Log2.v("\n\nTeBuildSH.CreateSHTables(): inserting TeSite: " + teSite.ToStringA());

                            /* add record to the ES SH site table */
                            if ((rc = TeDynSite.TeInsertSite(teSite, teSiteNulls)) != Constant.SUCCESS)
                            {
                                Log2.e("\nTeBuildSH.CreateSHTables(): ERROR: TeInsertSite() failed, rc = " + rc);
                                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                TpRunTsip.mTW_ERR.Write("Error inserting te Site: {0}.\r\n", rc);
                                return (rc);
                            }
                        }

                    }
                    else if (rc == Constant.SITE_FAIL)
                    {
                        distFail = true;
                    }

                } /* end for (each temp. TS pairs record) */

            } // while (nTemp1Counter < nTemp1Count) : for each earth location record.


            TeDynChan.TeCloseChan();

            TeDynAnte.TeCloseAnte();

            TeDynSite.CloseTESite();

            if (rc != 0 && rc != Constant.NOMORERECS)
            {
                return (rc);
            }

            rc = CheckMDBAzimuthLoss(esAzimTableName, teAnteTableName);

            //...Log2.v("\nTeBuildSH.CreateSHTables(): Exit");

            return (Constant.SUCCESS);

        } /*---- end createSHTables ----*/


        /// <summary>
        /// This method reads all records from the temporary TS pairs DB table into application memory.  
        /// </summary>
        /// <param name="currTemp2"> - a TtTemp2 object to be populated with data.</param>
        /// <param name="temp2TableCount"> - the number of tables read from the DB.</param>
        /// <param name="temp2Name"> - name of temporary TS pairs DB table.</param>
        /// <returns></returns>
        private static int InitTemp2(out TtTemp2[] currTemp2, out int temp2TableCount, string temp2Name)
        {
            //...Log2.v("\nTeBuildSH.InitTemp2(): Entry");

            // 'out' requirement
            currTemp2 = null;
            temp2TableCount = -666;

            SQLLEN[] temp2Nulls;
            int rc;
            string tmpMaxTemp2;
            int temp2Handle = 0;

            /* move all parameter records to the parameter table */
            if ((temp2Handle = TtDynTemp2.TtSelectTemp2(temp2Name, "", "call1")) < 0)
            {
                return (temp2Handle);
            }

            List<TtTemp2> ttTemp2List = new List<TtTemp2>();
            TtTemp2 ttTemp2;
            temp2TableCount = 0;

            while ((rc = TtDynTemp2.TtFetchTemp2(temp2Handle, out ttTemp2, out temp2Nulls)) == Constant.SUCCESS)
            {
                ttTemp2List.Add(ttTemp2);

                temp2TableCount++;

                if (temp2TableCount == Constant.MAXTMP2REC)
                {
                    Log2.e("\nTeBuildSH.InitTemp2(): ERROR: TtTemp2[] exceeds maximum array size.");
                    tmpMaxTemp2 = String.Format("{0}", Constant.MAXTMP2REC);
                    ErrMsg.UtPrintMessage(Error.MAXTMP2S, tmpMaxTemp2);
                    break;
                }

            }

            // If it crashed out of the while-loop with an error then just return.
            if (rc == Error.DYN_MS_SQL_SERVER_ERR)
            {
                return (rc);
            }

            // If we get here all is well; we can set the 'out' parameters.
            currTemp2 = ttTemp2List.ToArray();
            temp2TableCount = currTemp2.Length;

            TtDynTemp2.TtCloseTemp2(temp2Handle);

            //...Log2.v("\nTeBuildSH.InitTemp2(): Exit");
            return (Constant.SUCCESS);

        } /*---- end initTemp2 ----*/

        /// <summary>
        /// This method determines if an ES ante in the SH table list has a 
        /// non-zero loss value.  
        /// </summary>
        /// <param name="azimTable"> - name of _azim table in DB.</param>
        /// <param name="anteTable"> - name of _ante table in DB.</param>
        /// <returns></returns>
        public static int CheckMDBAzimuthLoss(string azimTable, string anteTable)
        {
            string sel1;
            string location;  //[LOCATION_SZ];
            string call1;  //[CALLSIGN_SZ];
            float azimuth;
            float loss;
            SQLLEN locanull;
            SQLLEN call1null;
            SQLLEN azimnull;
            SQLLEN lossnull;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;

            SQLHDBC hConn = Ssutil.NewConn();

            if (azimTable.StartsWith("me")) /* environment is MDB */
            {
                sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

                sel1 = String.Format("select distinct {0}.earthlocation, {1}.earthcall1, {2}.azim, {3}.loss from {4}, {5} where {6}.earthlocation = {7}.location and {8}.earthcall1 = {9}.call1 and {10}.loss != 0.0",
                    anteTable, anteTable, azimTable, azimTable,
                    anteTable, azimTable, anteTable, azimTable,
                    anteTable, azimTable, azimTable);

                sqlRet = ODBC.SQLExecDirect(hStmt, sel1, sel1.Length);

                if (!ODBC.IsOK(sqlRet))
                {
                    Log2.e("\nTeBuildSH.CheckMDBAzimuthLoss(): ERROR: SQLExecDirect failed.");
                    return Error.ODBC_EXECDIRECT_FAILED;
                }

                while (true)
                {
                    sqlRet = ODBC.SQLFetch(hStmt);

                    if (!ODBC.IsOK(sqlRet))
                    {
                        if (sqlRet == ODBC.SQL_NO_DATA)
                        {
                            // It is EOF, break out of the while-loop.
                            break;
                        }
                        else
                        {
                            // Something is wrong.
                            Log2.e("\nTeBuildSH.CheckMDBAzimuthLoss(): ERROR: SQLFetch failed, return value = " + sqlRet);
                            return Error.ODBC_FETCH_FAILED;
                        }
                    }

                    try
                    {
                        // Initialize auto column indexing.
                        Ssutil.DbStartGets();

                        // Get the DB column values from the fetched table.
                        Ssutil.DbGetString(hStmt, 0, "earthlocation", out location, Constant.LOCATION_SZ, out locanull);

                        Ssutil.DbGetString(hStmt, 0, "earthcall1", out call1, Constant.CALLSIGN_SZ, out call1null);

                        Ssutil.DbGetFloat(hStmt, 0, "azim", out azimuth, out azimnull);

                        Ssutil.DbGetFloat(hStmt, 0, "loss", out loss, out lossnull);

                        TpRunTsip.mTW_ERR.Write("WARNING - Azimuth Record:{0} {1} {0,5:F2} has a loss value of {0,5:F2}\r\n",
                                location.Trim(), call1.Trim(), azimuth, loss);
                    }
                    catch (Exception e)
                    {
                        Log2.e("\nTeBuildSH.CheckMDBAzimuthLoss(): ERROR: ODBC 'Get' failed: " + e.Message);

                        return Error.ODBC_GET_FAILED;
                    }

                }

                sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            }

            Ssutil.DisConn(hConn);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method culls the sites from the environment based on 
        /// the User's inputs via the MICS GUI, ie. Country and ALL EXCEPT SELF.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="tsSite"> - FtSite object.</param>
        /// <param name="esSite"> - FeSite object.</param>
        /// <returns></returns>
        public static int SiteCull(TpParm tpParm, FtSite tsSite, FeSite esSite)
        {
            //...Log2.v("\nTeBuildSH.SiteCull(): Entry");

            string province;
            string country;  //[4];

            /* cull OPERATOR CODES */

            /* if ((ts. oper is same as es. oper) and (user specified "ALL EXCEPT SELF")) */
            if ((tsSite.oper.Equals(esSite.oper)) &&
                (tpParm.selsites.Equals("ALL EXCEPT SELF")))
            {
                Log2.w("\nTeBuildSH.SiteCull(): WARNING: returned Constant.FAILURE");

                return (Constant.FAILURE);
            }

            /* cull for COUNTRY */

            /* if (proposed file type) is "ES") */
            if (Strings.FirstCharIs(tpParm.protype, 'E'))
            {
                /* ES - TS */
                province = tsSite.prov;
            }
            else
            {
                /* TS - ES */
                province = esSite.prov;
            }

            /* if ((the user specified a country) and (country is not "ALL")) */
            if ((!tpParm.country.Equals("")) && (!tpParm.country.Equals("ALL")))
            {
                /* check that the environment is in the specified country */
                GenUtil.UtGetCountry(province, out country);
                if (!country.Equals(tpParm.country))
                {
                    return (Constant.FAILURE);
                }
            }

            //...Log2.v("\nTeBuildSH.SiteCull(): Entry");

            return (Constant.SUCCESS);

        } /*---- end siteCull ----*/

        /// <summary>
        /// This method fills in the values of the SH Site record 
        /// using source data from the prescribed PDF, the environment file (MDB or PDF) as well as results data calculated by TSIP.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="tsSite"> - FtSite object.</param>
        /// <param name="tsNulls"> - ODBC nullInds for tsSite.</param>
        /// <param name="esSite"> - FeSite object.</param>
        /// <param name="esNulls"> - ODBC nullInds for esSite.</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="teNulls"> - ODBC nullInds for teSite.</param>
        /// <param name="call2"> - call sign for 2nd site.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int SetTeSite(TpParm tpParm,
                                    FtSite tsSite,
                                    SQLLEN[] tsNulls,
                                    FeSite esSite,
                                    SQLLEN[] esNulls,
                                    out TeSite teSite,
                                    out SQLLEN[] teNulls,
                                    string call2,
                                    string intPrintMsg,
                                    string vicPrintMsg)
        {
            //...Log2.v("\nTeBuildSH.SetTeSite(): Entry");

            teSite = new TeSite();
            teNulls = NullHelper.CreateArrayOfNullInd(TeSite.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            teSite.terrcall1 = tsSite.call1;
            teSite.terrcall2 = call2;
            teSite.terrname1 = tsSite.name;
            teSite.terroper = tsSite.oper;
            teSite.terrlatit = tsSite.latit;
            teSite.terrlongit = tsSite.longit;
            teSite.terrgrnd = tsSite.grnd;
            teSite.earthlocation = esSite.location;
            teSite.earthname = esSite.name;
            teSite.earthoper = esSite.oper;
            teSite.earthlatit = esSite.latit;
            teSite.earthlongit = esSite.longit;
            teSite.earthgrnd = esSite.grnd;
            teSite.radiozone = esSite.radio;
            teSite.rainzone = esSite.rain;
            teSite.processed = 0;  // false;
            teSite.etreport = 0;   // false;
            teSite.tereport = 0;   // false;
            teSite.etcaseno = 0;
            teSite.tecaseno = 0;
            teSite.etsubcases = 0;
            teSite.tesubcases = 0;

            teNulls[TeSite.TERRCALL1] = tsNulls[FtSite.CALL1];
            teNulls[TeSite.TERRCALL2] = Constant.DB_NOT_NULL;
            teNulls[TeSite.TERRNAME1] = tsNulls[FtSite.NAME];
            teNulls[TeSite.TERROPER] = tsNulls[FtSite.OPER];
            teNulls[TeSite.TERRLATIT] = tsNulls[FtSite.LATIT];
            teNulls[TeSite.TERRLONGIT] = tsNulls[FtSite.LONGIT];
            teNulls[TeSite.TERRGRND] = tsNulls[FtSite.GRND];
            teNulls[TeSite.EARTHLOCATION] = esNulls[FeSite.LOCATION];
            teNulls[TeSite.EARTHNAME] = esNulls[FeSite.NAME];
            teNulls[TeSite.EARTHOPER] = esNulls[FeSite.OPER];
            teNulls[TeSite.EARTHLATIT] = esNulls[FeSite.LATIT];
            teNulls[TeSite.EARTHLONGIT] = esNulls[FeSite.LONGIT];
            teNulls[TeSite.EARTHGRND] = esNulls[FeSite.GRND];
            teNulls[TeSite.RADIOZONE] = esNulls[FeSite.RADIO];
            teNulls[TeSite.RAINZONE] = esNulls[FeSite.RAIN];
            teNulls[TeSite.PROCESSED] = Constant.DB_NOT_NULL;
            teNulls[TeSite.ETREPORT] = Constant.DB_NOT_NULL;
            teNulls[TeSite.TEREPORT] = Constant.DB_NOT_NULL;
            teNulls[TeSite.ETCASENO] = Constant.DB_NOT_NULL;
            teNulls[TeSite.TECASENO] = Constant.DB_NOT_NULL;
            teNulls[TeSite.ETSUBCASES] = Constant.DB_NOT_NULL;
            teNulls[TeSite.TESUBCASES] = Constant.DB_NOT_NULL;

            int rc = TeCalcs.TeSiteCalcs(tpParm, ref teSite, ref teNulls, intPrintMsg, vicPrintMsg);

            //...Log2.v("\nTeBuildSH.SetTeSite(): Exit");

            return rc;

        } /*---- end setTeSite ----*/

        /// <summary>
        /// This method controls the culling of antennae and the population of 
        /// the Antenna SH Table with data.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="teSiteNulls"> - ODBC nullInds for teSite.</param>
        /// <param name="esAnteTableName"> - name of Es ante table.</param>
        /// <param name="tsAnteTableName"> - name of Ts ante table.</param>
        /// <param name="esChanTableName"> - name of Es chan table.</param>
        /// <param name="tsChanTableName"> - name of Ts ante table.</param>
        /// <param name="numAnte"> - number of ante records read from the DB.</param>
        /// <param name="numEtCases"> - accumulated count of the number of Et cases.</param>
        /// <param name="numTeCases"> - accumulated count of the number of Te cases.</param>
        /// <returns></returns>
        public static int CreateAnteTable(TpParm tpParm,
                                            ref TeSite teSite,
                                            ref SQLLEN[] teSiteNulls,
                                            string esAnteTableName,
                                            string tsAnteTableName,
                                            string esChanTableName,
                                            string tsChanTableName,
                                            ref int numAnte,
                                            ref int numEtCases,
                                            ref int numTeCases)
        {
            //...Log2.v("\nTeBuildSH.CreateAnteTable(): Entry");

            bool mode1;
            bool mode2;

            // AH : HERE
            SQLLEN[] teAnteNulls = NullHelper.CreateArrayOfNullInd(TeAnte.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            SQLLEN[] esAnteNulls;  //[FE_ANTE_SIZE_];

            int esAnteHandle;
            int anteCount;
            int chanCount;
            int i;
            int rc;

            string esCriteria;
            string tsCriteria;
            string intPrintMsg;
            string vicPrintMsg;

            TeAnte teAnte = new TeAnte();
            FeAnte esAnte;

            //AnteTable tsAnte;
            FtAnteWN[] ftAnteWNarray;

            //tsAnte = anteTable;
            //*numAnte = 0;

            tsCriteria = String.Format("call1 = '{0}' and call2 = '{1}'",
                teSite.terrcall1,
                teSite.terrcall2);

            /* read all antenna records belonging to the current TS (call1, call2)
             * pair from TS table into memory */
            if ((rc = InitTsAnte(tsCriteria, out ftAnteWNarray, out anteCount, tsAnteTableName, tpParm.envtype)) != Constant.SUCCESS)
            {
                return (rc);
            }

            esCriteria = String.Format("location = '{0}'", teSite.earthlocation);

            if ((esAnteHandle = TpMdbPdfGet.SelectEarthAnte(esAnteTableName, esCriteria, tpParm.envtype)) < 0)
            {
                return (esAnteHandle);
            }

            FtAnteWN tsAnteWN;

            /* for (each antenna record belonging to the current ES location) */
            while ((rc = TpMdbPdfGet.FetchEarthAnte(esAnteHandle, out esAnte, out esAnteNulls, tpParm.envtype)) == Constant.SUCCESS)
            {
                /* for (each TS antenna record) */
                for (i = 0; i < anteCount; i++)
                {
                    tsAnteWN = ftAnteWNarray[i];

                    /* If (the current es antenna transmits)
                     * do the first fine Cull for double distance and identify
                     * the calculation modes. The NULL indicates that teAnte
                     * information is not yet available.
                     */

                    if ((esAnteNulls[FeAnte.ACODETX] != Constant.DB_NULL) &&
                        (DistanceCull(teSite, esAnte, null, "E", out mode1, out mode2) == Constant.SUCCESS))
                    {

                        intPrintMsg = String.Format("INTERFERER:   {0,11}     {0,10}    {0,5}",
                            esAnte.location, esAnte.call1, esAnte.txband);
                        vicPrintMsg = String.Format("VICTIM:       {0,10}     {0,10}    {0,5}    {0,4:D}",
                            tsAnteWN.ante.call1, tsAnteWN.ante.call2,
                            tsAnteWN.ante.bndcde, tsAnteWN.ante.anum);

                        /* cull ante info for adjancent band and call sign. */
                        if (AnteCull(tpParm, esAnte, tsAnteWN.ante, "E") == Constant.SUCCESS)
                        {

                            /* set the values for an ES Ante SH record from the pro. and env. files,
                             * ES is interferer, setTeAnte() - calls teAnteCalcs() to finish the
                             * population of the ES Ante SH record*/

                            rc = SetTeAnte(tpParm, true, tsAnteWN.ante, tsAnteWN.anteNulls,
                                esAnte, esAnteNulls, ref teSite, ref teAnte, ref teAnteNulls,
                                mode1, mode2, intPrintMsg, vicPrintMsg);

                            //AH: REMOVE
                            bool filter = teAnte.terrcall1.Equals("CHX576");
                            filter &= teAnte.terrcall2.Equals("VEL885");
                            if (filter)
                            {
                                string str = String.Format("\nFILTER-A: {0}  {1}  {2}  {3}", teAnte.terrcall1, teAnte.terrcall2, teAnte.tvdistes, teAnteNulls[TeAnte.TVDISTES]);
                                //...Log2.v(str);
                            }

                            if ((rc != Constant.SUCCESS) && (rc != Constant.CONT_PROCESSING))
                            {
                                Log2.e("\nTeBuildSH.CreateAnteTable(): ERROR: A: call to SetTeAnte() failed: TS remote antenna or site data could not be located.");
                                return (rc);
                            }
                            /* CONT_PROCESSING indicates that TS remote antenna or site data could
                             * not be located - simply skip the record */

                            chanCount = 0;

                            /*
                             * Do a second distance cull with the azimuths stored in teAnte--this cull includes only
                             * areas within the coordination distance and the Keyhole area.
                             *	---- OEL 970709 ----- fix the distance cull so that
                             *                        interferer is ES.
                             */
                            if ((rc == Constant.SUCCESS) &&
                                (DistanceCull(teSite, esAnte, teAnte, "E", out mode1, out mode2) == Constant.SUCCESS))
                            {
                                //AH: REMOVE
                                filter = teAnte.terrcall1.Equals("CHX576");
                                filter &= teAnte.terrcall2.Equals("VEL885");
                                if (filter)
                                {
                                    string str = String.Format("\nFILTER-B: {0}  {1}  {2}  {3}", teAnte.terrcall1, teAnte.terrcall2, teAnte.tvdistes, teAnteNulls[TeAnte.TVDISTES]);
                                    //...Log2.v(str);
                                }

                                /* from SH Ante create SH	CHANNEL */
                                // AH: HERE 20180219
                                if ((rc = CreateChanTable(tpParm, ref teSite, ref teSiteNulls, ref teAnte, ref teAnteNulls, esChanTableName, tsChanTableName, ref chanCount, ref numEtCases, ref numTeCases)) != Constant.SUCCESS)
                                {
                                    return (rc);
                                }

                                if (chanCount > 0)
                                {
                                    /* add record to the ES SH ante table where ES is the
                                    *	interferer */

                                    //AH: REMOVE
                                    filter = teAnte.terrcall1.Equals("CHX576");
                                    filter &= teAnte.terrcall2.Equals("VEL885");
                                    if (filter)
                                    {
                                        string str = String.Format("\nFILTER-Z: {0}  {1}  {2}  {3}", teAnte.terrcall1, teAnte.terrcall2, teAnte.tvdistes, teAnteNulls[TeAnte.TVDISTES]);
                                        //...Log2.v(str);
                                    }

                                    if ((rc = TeDynAnte.TeInsertAnte(teAnte, teAnteNulls)) != Constant.SUCCESS)
                                    {
                                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                        TpRunTsip.mTW_ERR.Write("Attempt to insert Antenna record returned {0}\n", rc);
                                        return (rc);
                                    }
                                    numAnte++;
                                }
                            }   /* end if (distanceCull...	*/
                        }       /*	end if (anteCull...	*/
                    }           /* end if ((esAnteNulls...	*/

                    /* if (the current es antenna receives) */
                    if ((esAnteNulls[FeAnte.ACODERX] != Constant.DB_NULL)
                        /* do the fine Cull for double distance and identify
                         * the calculation modes */
                        && (DistanceCull(teSite, esAnte, null, "T", out mode1, out mode2) == Constant.SUCCESS))
                    {
                        vicPrintMsg = String.Format("VICTIM:       {0,11}     {0,10}    {0,5}",
                            esAnte.location, esAnte.call1, esAnte.txband);
                        intPrintMsg = String.Format("INTERFERER:   {0,10}     {0,10}    {0,5}    {0,4:D}",
                            tsAnteWN.ante.call1, tsAnteWN.ante.call2,
                            tsAnteWN.ante.bndcde, tsAnteWN.ante.anum);

                        /* cull ante info */
                        if (AnteCull(tpParm, esAnte, tsAnteWN.ante, "T") == Constant.SUCCESS)
                        {

                            /* 	set the values for an ES Ante SH record from the pro. and env.
                            *		files, ES is victim, setTeAnte() - calls teAnteCalcs() to finish
                            *		the population of the ES Ante SH record*/
                            rc = SetTeAnte(tpParm, false, tsAnteWN.ante, tsAnteWN.anteNulls,
                                esAnte, esAnteNulls, ref teSite, ref teAnte, ref teAnteNulls,
                                mode1, mode2, intPrintMsg, vicPrintMsg);
                            if ((rc != Constant.SUCCESS) && (rc != Constant.CONT_PROCESSING))
                            {
                                Log2.e("\nTeBuildSH.CreateAnteTable(): ERROR: B: call to SetTeAnte() failed: TS remote antenna or site data could not be located.");
                                return (rc);
                            }
                            /* CONT_PROCESSING indicates that TS remote antenna or site data could
                             * not be located - simply skip the record */

                            chanCount = 0;
                            /*
                             * 	Do a second distance cull with the azimuths stored in teAnte
                             *	-- this cull includes only areas within the coordination distance
                             *	and the Keyhole area.
                             */
                            if ((rc == Constant.SUCCESS) &&
                                (DistanceCull(teSite, esAnte, teAnte, "T", out mode1, out mode2) == Constant.SUCCESS))
                            {
                                /* from SH Ante create SH	CHANNEL */
                                if ((rc = CreateChanTable(tpParm, ref teSite, ref teSiteNulls, ref teAnte,
                                    ref teAnteNulls, esChanTableName,
                                    tsChanTableName, ref chanCount, ref numEtCases,
                                    ref numTeCases)) != Constant.SUCCESS)
                                {
                                    return (rc);
                                }

                                if (chanCount > 0)
                                {
                                    /* add record to the ES SH ante table where ES is the victim */
                                    if ((rc = TeDynAnte.TeInsertAnte(teAnte, teAnteNulls)) != Constant.SUCCESS)
                                    {
                                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                        TpRunTsip.mTW_ERR.Write("Inserting Antenna returned {0}\n", rc);
                                        return (rc);
                                    }
                                    numAnte++;
                                }

                            }       /* end if (distanceCull... */

                        }           /* end if (anteCull...  	*/

                    }               /* end if ((esAnteNulls... */


                } /* end for (each TS antenna record) */

            } /* end for (each ES antenna record) */

            if (rc != Constant.NOMORERECS)
            {
                return (rc);
            }

            TpMdbPdfGet.CloseEarthAnte(esAnteHandle, tpParm.envtype);

            //...Log2.v("\nTeBuildSH.CreateAnteTable(): Exit");

            return (Constant.SUCCESS);

        } /*---- end createAnteTable ----*/

        /// <summary>
        /// This method populates the fields of the SH Site record with source data
        /// from the PDF, the environment file (MDB or PDF) as well as results data
        /// calculated by TSIP.  
        /// </summary>
        /// <param name="select"> - provides the text of an SQL 'where' clause used to selectively read Ts ante records from the DB.</param>
        /// <param name="ftAnteWNarray"> - FtAnteWN object combining fields for FtAnte and its associated ODBC nulInds.</param>
        /// <param name="anteTableCount"> - the number of FtAnte records read from the DB.</param>
        /// <param name="tsTableName"> - name of the DB table containing the FtAnte records.</param>
        /// <param name="envtype"></param>
        /// <returns></returns>
        public static int InitTsAnte(string select,
                                        out FtAnteWN[] ftAnteWNarray,
                                        out int anteTableCount,
                                        string tsTableName,
                                        string envtype)
        {
            //...Log2.v("\nTeBuildSH.InitTsAnte(): Entry");

            // 'out' requirement.
            ftAnteWNarray = null;
            anteTableCount = -666;

            int anteHandle;
            int rc;
            int count = 0;

            /* move all parameter records to the parameter table */
            if ((anteHandle = TpMdbPdfGet.SelectTerrAnte(tsTableName, select, envtype)) < 0)
            {
                return (anteHandle);
            }

            FtAnte ftAnte;
            SQLLEN[] ftAnteNullInds;
            List<FtAnteWN> ftAnteWNList = new List<FtAnteWN>();

            while ((rc = TpMdbPdfGet.FetchTerrAnte(anteHandle, out ftAnte, out ftAnteNullInds,
                envtype)) == Constant.SUCCESS)
            {
                //currAnte++;
                FtAnteWN ftAnteWN = new FtAnteWN();
                ftAnteWN.ante = ftAnte;
                ftAnteWN.anteNulls = ftAnteNullInds;

                ftAnteWNList.Add(ftAnteWN);

                count++;

                if (count >= Constant.MAXANTEREC)
                {
                    ErrMsg.UtPrintMessage(Error.MAXANTES, Constant.MAXANTEREC.ToString());
                    break;
                }
            }

            if (rc != Constant.NOMORERECS)
            {
                Log2.e("\nTeBuildSH.InitTsAnte():  ERROR: Too many FtAnteWN array objects.");
                return (rc);
            }

            TpMdbPdfGet.CloseTerrAnte(anteHandle, envtype);

            // Finally, create and return the 'out' FtAnteWN[].
            ftAnteWNarray = ftAnteWNList.ToArray();
            anteTableCount = ftAnteWNarray.Length;

            //...Log2.v("\nTeBuildSH.InitTsAnte(): Exit");

            return (Constant.SUCCESS);

        } /*---- end initTsAnte */

        /// <summary>
        /// This method culls the sites from the environment based on the 
        /// User's specifications ie. coordination distance. 
        /// </summary>
        /// <remarks>
        /// This method is used to do two such culls. The first is for double
        /// the coordination distance, and is done when teAnte is NULL. The 
        /// second is a fine cull which removes everything beyond the 
        /// coordination distance except for what lies within the Keyhole 
        /// Coodination Zone. This second cull requires values from teAnte, 
        /// thus teAnte is always required for this sort of cull. 
        /// </remarks>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="esAnte"> - FeAnte object.</param>
        /// <param name="teAnte"> - TeAnte object.</param>
        /// <param name="interferer"> - name of interferer.</param>
        /// <param name="mode1"> - see the code for a full definition.</param>
        /// <param name="mode2"> - see the code for a full definition.</param>
        /// <returns></returns>
        public static int DistanceCull(TeSite teSite,
                                        FeAnte esAnte,
                                        TeAnte teAnte,
                                        string interferer,
                                        out bool mode1,
                                        out bool mode2)
        {
            //...Log2.v("\nTeBuildSH.DistanceCull(): Entry");

            // 'out' requirement;
            mode1 = false;
            mode2 = false;

            int rc;
            double esLatit;
            double esLongit;
            double tsLatit;
            double tsLongit;
            double distKm;
            double bearingMid;
            double bearingDiff;

            esLatit = teSite.earthlatit / 100.00;
            esLongit = teSite.earthlongit / 100.00;
            tsLatit = teSite.terrlatit / 100.00;
            tsLongit = teSite.terrlongit / 100.00;

            /* calculate the distance between ES and TS stations */
            AxSub2.AxDistan(esLatit, tsLatit, esLongit, tsLongit, out distKm, out bearingMid, out bearingDiff);

            /* MODE 1 --> ES-TS txtro ; TS-ES rxtro */
            rc = Constant.FAILURE;

            /* allow double distance cull for first distance cull, indicated
                by a null value for teAnte */

            if (teAnte == null)
                distKm = .5 * distKm;
            /*
             * In the first distance cull, all sites more than twice the maximum
             * distance are disallowed. In the second cull, we have the discrimination
             * angles, and we are able to remove the sites which are not within
             * the maximum distance or not within the keyhole area.
             */

            /* if (((interferer is "ES") and ((distKm <= es. txtro)
               *		or (2nd distance cull and abs(teAnte.ediscang < 5))))
               * or ((interferer is "TS") and ((distKm <= es. rxtro)
               *		or (2nd distance cull and abs(teAnte.tdiscang < 5))))
               */
            if ((Strings.FirstCharIs(interferer, 'E') && ((distKm <= esAnte.txtro) || ((teAnte != null) && TpKeyhole.WithinRange(teAnte.ediscang, 5.0))))
                ||
                (Strings.FirstCharIs(interferer, 'T') && ((distKm <= esAnte.rxtro) || ((teAnte != null) && TpKeyhole.WithinRange(teAnte.tdiscang, 5.0))))
            )
            {
                mode1 = true;
                rc = Constant.SUCCESS;
            }
            else
            {
                mode1 = false;
            }

            /* MODE 2 --> ES-TS txpre ; TS-ES rxpre */

            /* if ((interferer is "ES" and distKm <= es. txpre)
             *		or (2nd distance cull and abs(teAnte.ediscang < 5))))
             * or (interferer is "TS" and distKm <= es. rxpre))
             *		or (2nd distance cull and abs(teAnte.tdiscang < 5))))
             */
            if ((Strings.FirstCharIs(interferer, 'E') && ((distKm <= esAnte.txpre) || ((teAnte != null) && TpKeyhole.WithinRange(teAnte.ediscang, 5.0))))
                ||
                (Strings.FirstCharIs(interferer, 'T') && ((distKm <= esAnte.rxpre) || ((teAnte != null) && TpKeyhole.WithinRange(teAnte.tdiscang, 5.0)))))
            {
                mode2 = true;
                rc = Constant.SUCCESS;
            }
            else
            {
                mode2 = false;
            }

            //...Log2.v("\nTeBuildSH.DistanceCull(): Exit");

            return (rc);

        } /*---- end distanceCull ----*/

        /// <summary>
        /// This method culls the antennae from the environment based on 
        /// adjacent bands.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="esAnte"> - FeAnte object.</param>
        /// <param name="tsAnte"> - FtAnte object.</param>
        /// <param name="interferer"> - name of inteferer.</param>
        /// <returns></returns>
        public static int AnteCull(TpParm tpParm, FeAnte esAnte, FtAnte tsAnte, string interferer)
        {
            //...Log2.v("\nTeBuildSH.AnteCull(): Entry");

            int rc;

            /* if (the curr. ES and TS bands are not adjacent */
            if (Strings.FirstCharIs(interferer, 'E'))
            {
                /* ES-TS */
                if ((rc = TsipUtils.UtAnteAdjBands(esAnte.txband, tsAnte.bndcde)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }
            else
            {
                /* TS-ES */
                if ((rc = TsipUtils.UtAnteAdjBands(tsAnte.bndcde, esAnte.rxband)) != Constant.SUCCESS)
                {
                    return (rc);
                }
            }

            /* if (proposed file type is "TS") */
            if (Strings.FirstCharIs((tpParm.protype), 'T'))
            {
                /* if (user specified "CALL SIGN" culling) */
                if (tpParm.selsites.Equals("CALL SIGN"))
                {
                    /* check if es. call1 is in the list supplied by
                     * the user to call signs */
                    return (CallInList(esAnte.call1, tpParm.codes));
                }
            }

            //...Log2.v("\nTeBuildSH.AnteCull(): Exit");

            return (Constant.SUCCESS);

        } /*---- end anteCull ----*/

        /// <summary>
        /// This method determines whether the given call sign is in the 
        /// list of call signs specified by the User for culling.  
        /// </summary>
        /// <param name="callSign"> - call sign of site to be tested.</param>
        /// <param name="list"> - string containing a list of User' prescribed list of call signs to be culled.</param>
        /// <returns></returns>
        public static int CallInList(string callSign, string list)
        {
            int rc;
            string callSignList; // [150]
            string aCode;  // [9];

            callSignList = list;

            rc = GenUtil.UtGetInputString(callSignList, out aCode, 9);
            while (rc >= 0)
            {
                if (callSign.Trim().Equals(aCode.Trim()))
                {
                    return (Constant.SUCCESS);
                }
                rc = GenUtil.UtGetInputString(null, out aCode, 9);
            }
            return (Constant.FAILURE);

        } /*---- end callInList ----*/

        /// <summary>
        /// This method populates the fields of the SH Ante record with source data
        /// from the PDF, the environment file (MDB or PDF) and results data 
        /// calculated by TSIP.  
        /// </summary>
        /// <param name="tpParm"> - name of paramater file.</param>
        /// <param name="isTX"> - see code for definition.</param>
        /// <param name="tsAnte"> - FtAnte object.</param>
        /// <param name="tsAnteNulls"> - ODBC nullInds associated with tsAnte.</param>
        /// <param name="esAnte"> - FeAnte object.</param>
        /// <param name="esAnteNulls"> - ODBC nullInds associated with esAnte.</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="teAnte"> - TeAnte object.</param>
        /// <param name="teAnteNulls"> - ODBC nullInds associated with teAnte.</param>
        /// <param name="mode1"> - see code for definition.</param>
        /// <param name="mode2"> - see code for definition.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int SetTeAnte(TpParm tpParm,
                                        bool isTX,
                                        FtAnte tsAnte,
                                        SQLLEN[] tsAnteNulls,
                                        FeAnte esAnte,
                                        SQLLEN[] esAnteNulls,
                                        ref TeSite teSite,
                                        ref TeAnte teAnte,
                                        ref SQLLEN[] teAnteNulls,
                                        bool mode1,
                                        bool mode2,
                                        string intPrintMsg,
                                        string vicPrintMsg)
        {
            //...Log2.v("\nTeBuildSH.SetTeAnte(): Entry");
#if false
            //...Log2.v("\nTeBuildSH.SetTeAnte(): tsAnte: " + tsAnte.KeysToString());
            //...Log2.v("\nTeBuildSH.SetTeAnte(): esAnte: " + esAnte.KeysToString());
#endif

            /* insert record into TSIP ANTE table */

            NullHelper.FillArray(ref teAnteNulls, Constant.DB_NULL);

            teAnte.terrcall1 = tsAnte.call1;
            teAnte.terrcall2 = tsAnte.call2;
            teAnte.terrbndcde = tsAnte.bndcde;
            teAnte.terranum = tsAnte.anum;
            teAnte.terracode = tsAnte.acode;
            teAnte.intause = tsAnte.ause;

            teAnte.earthlocation = esAnte.location;
            teAnte.earthcall1 = esAnte.call1;
            teAnte.satname = esAnte.satname;
            teAnte.satoper = esAnte.op2;
            teAnte.satlongit = esAnte.satlongit;
            teAnte.txpre = esAnte.txpre;
            teAnte.txtro = esAnte.txtro;
            teAnte.rxpre = esAnte.rxpre;
            teAnte.rxtro = esAnte.rxtro;
            teAnte.sarc1 = esAnte.sarc1;
            teAnte.sarc2 = esAnte.sarc2;
            teAnte.tvazim = 0.0; /* need to initialize this value */
            teAnte.processed = 0;
            teAnte.mode1 = (short)(mode1 ? 1 : 0);
            teAnte.mode2 = (short)(mode2 ? 1 : 0);
            teAnte.etreport = 0;
            teAnte.tereport = 0;
            teAnte.etsubcaseno = 0;
            teAnte.tesubcaseno = 0;

            /*	Copy in the offaxis angle fields from the ts. GJS - 1108 - 2002.12 */
            if (tsAnteNulls[FtAnte.OFFAZM] == Constant.DB_NULL ||
                !tsAnte.offazm.Equals("Y"))
            {
                teAnte.tsoffaxis = "N";
                /*	Set all the offaxis angle fields to null.  They are not used. */
                teAnte.tstrueaz = tsAnte.azmth;
                teAnte.tstrueel = tsAnte.elvtn;
                teAnte.angleuta = -1.0;
                teAnte.angleeta = -1.0;
                teAnte.angleatv = -1.0;
                teAnte.adisc_atv = 0.0;
                teAnteNulls[TeAnte.ANGLEUTA] = Constant.DB_NULL;
                teAnteNulls[TeAnte.ANGLEETA] = Constant.DB_NULL;
                teAnteNulls[TeAnte.ANGLEATV] = Constant.DB_NULL;
                teAnteNulls[TeAnte.ADISC_ATV] = Constant.DB_NULL;
            }
            else
            {
                teAnte.tsoffaxis = "Y"; /* 'Y' if offaxis */
                teAnte.tstrueaz = tsAnte.tazmth;
                teAnte.tstrueel = tsAnte.telvtn;
                /*	Others will be filled in after the geometry has been calculated. */
            }
            teAnteNulls[TeAnte.TSTRUEAZ] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.TSTRUEEL] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.TSOFFAXIS] = Constant.DB_NOT_NULL;

            teAnteNulls[TeAnte.TERRCALL1] = tsAnteNulls[FtAnte.CALL1];
            teAnteNulls[TeAnte.TERRCALL2] = tsAnteNulls[FtAnte.CALL2];
            teAnteNulls[TeAnte.TERRBNDCDE] = tsAnteNulls[FtAnte.BNDCDE];
            teAnteNulls[TeAnte.TERRANUM] = tsAnteNulls[FtAnte.ANUM];
            teAnteNulls[TeAnte.TERRACODE] = tsAnteNulls[FtAnte.ACODE];
            teAnteNulls[TeAnte.INTAUSE] = tsAnteNulls[FtAnte.AUSE];
            teAnteNulls[TeAnte.EARTHLOCATION] = esAnteNulls[FeAnte.LOCATION];
            teAnteNulls[TeAnte.EARTHCALL1] = esAnteNulls[FeAnte.CALL1];
            teAnteNulls[TeAnte.SATNAME] = esAnteNulls[FeAnte.SATNAME];
            teAnteNulls[TeAnte.SATOPER] = esAnteNulls[FeAnte.OP2];
            teAnteNulls[TeAnte.SATLONGIT] = esAnteNulls[FeAnte.SATLONGIT];
            teAnteNulls[TeAnte.TXPRE] = esAnteNulls[FeAnte.TXPRE];
            teAnteNulls[TeAnte.TXTRO] = esAnteNulls[FeAnte.TXTRO];
            teAnteNulls[TeAnte.RXPRE] = esAnteNulls[FeAnte.RXPRE];
            teAnteNulls[TeAnte.RXTRO] = esAnteNulls[FeAnte.RXTRO];
            teAnteNulls[TeAnte.SARC1] = esAnteNulls[FeAnte.SARC1];
            teAnteNulls[TeAnte.SARC2] = esAnteNulls[FeAnte.SARC2];
            teAnteNulls[TeAnte.PROCESSED] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.MODE1] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.MODE2] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.INTERFERER] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.ETREPORT] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.TEREPORT] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.ETSUBCASENO] = Constant.DB_NOT_NULL;
            teAnteNulls[TeAnte.TESUBCASENO] = Constant.DB_NOT_NULL;

            if (isTX)
            {
                teAnte.interferer = "E";
                teAnte.earthband = esAnte.txband;
                teAnte.earthacode = esAnte.acodetx;
                teAnteNulls[TeAnte.EARTHBAND] = esAnteNulls[FeAnte.TXBAND];
                teAnteNulls[TeAnte.EARTHACODE] = esAnteNulls[FeAnte.ACODETX];
            }
            else
            {
                teAnte.interferer = "T";
                teAnte.earthband = esAnte.rxband;
                teAnte.earthacode = esAnte.acoderx;
                teAnteNulls[TeAnte.EARTHBAND] = esAnteNulls[FeAnte.RXBAND];
                teAnteNulls[TeAnte.EARTHACODE] = esAnteNulls[FeAnte.ACODERX];
            }

            int rc = TeCalcs.TeAnteCalcs(tpParm, ref teAnte, ref teSite, ref teAnteNulls, intPrintMsg,
                vicPrintMsg);

            //...Log2.v("\nTeBuildSH.SetTeAnte(): Exit");

            return rc;

        } /*---- end setTeAnte ----*/

        /// <summary>
        /// This method controls the channel culling and population of 
        /// the Channel SH Table with data.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="teSiteNulls"> - ODBC nullInds for teSite.</param>
        /// <param name="teAnte"> - TeAnte object.</param>
        /// <param name="teAnteNulls"> - ODBC nullInds for teAnte.</param>
        /// <param name="esChanTableName"> - name of Es chan table.</param>
        /// <param name="tsChanTableName"> - name of Ts chan table.</param>
        /// <param name="chanCount"> - channel count.</param>
        /// <param name="numEtCases"> - accumulated count of the number of Et cases.</param>
        /// <param name="numTeCases"> - accumulated count of the number of Te cases.</param>
        /// <returns></returns>
        public static int CreateChanTable(TpParm tpParm,
                                            ref TeSite teSite,
                                            ref SQLLEN[] teSiteNulls,  // Check all these 'ref'
                                            ref TeAnte teAnte,
                                            ref SQLLEN[] teAnteNulls,
                                            string esChanTableName,
                                            string tsChanTableName,
                                            ref int chanCount,
                                            ref int numEtCases,
                                            ref int numTeCases)
        {
            //...Log2.v("\nTeBuildSH.CreateChanTable(): Entry");

            SQLLEN[] esChanNulls;  //[FE_CHAN_SIZE_],
            SQLLEN[] tsChanNulls;  //[FtChan.SIZE_];
            int esChanHandle, tsChanHandle, rc, rc1;
            string intPrintMsg = "";
            string vicPrintMsg = "";
            TeChan teChan;
            FeChan esChan;
            FtChan tsChan;

            teChan = new TeChan();
            SQLLEN[] teChanNulls = NullHelper.CreateArrayOfNullInd(TeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            string esCriteria;
            string tsCriteria;
            float tmppwr = 0.0f;
            SQLLEN pwrnull = Constant.DB_NULL;

            SQLRETURN sqlRet;
            SQLHANDLE hStmt;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            chanCount = 0;

            //setSqlNulls(teChanNulls, TE_CHAN_SIZE_, Constant.DB_NULL);
            //setSqlNulls(esChanNulls, FE_CHAN_SIZE_, Constant.DB_NULL);
            //setSqlNulls(tsChanNulls, FtChan.SIZE_, Constant.DB_NULL);

            teChanNulls = NullHelper.CreateArrayOfNullInd(TeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            esChanNulls = NullHelper.CreateArrayOfNullInd(FeChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);
            tsChanNulls = NullHelper.CreateArrayOfNullInd(FtChan.NUM_COLUMNS, NullHelper.ColumnStatus.NULL);

            /* if (analysis option is "BAND") */
            if (tpParm.analopt.Equals("BAND"))
            {

                /* BAND - add a single CHANNEL record FOR ES INTERFERER */
                if (teAnte.interferer.Equals("E"))
                {
                    esCriteria = String.Format("select max(pwrtx) from {0} where location = '{1}' and call1 = '{2}'", esChanTableName,
                        teAnte.earthlocation, teAnte.earthcall1);

                    sqlRet = ODBC.SQLExecDirect(hStmt, esCriteria, esCriteria.Length);
                    if (ODBC.IsOK(sqlRet))
                    {
                        try
                        {
                            Ssutil.DbGetFloat(hStmt, 1, "max(pwrtx)", out tmppwr, out pwrnull);

                            if (pwrnull == Constant.DB_NULL)
                            {
                                // Ensure that tmppwr is set to zero regardless of 
                                // value of Ssutil.dbGetMode (Enums.DbGetForNullMode).
                                tmppwr = 0.0f;
                            }
                        }
                        catch (Exception e)
                        {
                            Log2.w("\nTeBuildSH.CreateChanTable(): WARNING: A: DbGetFloat() threw an exception: " + e.Message);
                            // If there are no pwrtxs at all, then we will still return
                            // ODBC.IsOK, but the result will be null, and DbGetFloat will
                            // throw an exception.
                            // Note: determine if this is actually ever needed?
                            tmppwr = 0.0f;
                            pwrnull = Constant.DB_NULL;
                        }
                    }
                    else
                    {
                        tmppwr = 0.0f;
                        pwrnull = Constant.DB_NULL;
                    }

                    teChan.inttxpwr = tmppwr;
                    teChanNulls[TeChan.INTTXPWR] = pwrnull;

                    /* set the values for an ES Chan SH record from the
                     * pro. and env. files, ES is interferer, setBandChan()
                     * calls teChanCalcs() to finish the population of the
                     * ES Chan SH record */
                    rc = SetBandChan("E", tpParm, ref teSite, ref teAnte, ref teAnteNulls, ref teChan,
                        ref teChanNulls, out chanCount, out intPrintMsg, out vicPrintMsg);

                    if ((rc != Constant.SUCCESS) && (rc != Constant.PC_SKIP) && (rc != Constant.CONT_PROCESSING))
                    {
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return (rc);
                    }

                    TeDump(teChan, ref teAnte, ref teAnteNulls, ref teSite, ref teSiteNulls, ref numEtCases, ref numTeCases);

                    /* add record to the ES SH Chan table where ES is the
                    * interferer */
                    if ((rc = TeDynChan.TeInsertChan(teChan, teChanNulls)) != Constant.SUCCESS)
                    {
                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                        TpRunTsip.mTW_ERR.Write("Inserting Channel returned: {0}.\n", rc);
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return (rc);
                    }
                }

                /* FOR TS AS THE INTERFERER */

                /* set the values for an ES Chan SH record from the pro. and
                 * env. files, ES is victim, setBandChan() - calls
                 * teChanCalcs() to finish the population of the ES Chan
                 * SH record */

                if (teAnte.interferer.Equals("T"))
                {
                    tsCriteria = String.Format("select max(pwrtx) from {0} where call1 = '{1}' and call2 = '{2}' and bndcde = '{3}' and (antnumbtx1 = {4} or antnumbtx2 = {5})",
                        tsChanTableName, teAnte.terrcall1, teAnte.terrcall2,
                        teAnte.terrbndcde, teAnte.terranum, teAnte.terranum);

                    sqlRet = ODBC.SQLExecDirect(hStmt, tsCriteria, tsCriteria.Length);
                    if (ODBC.IsOK(sqlRet))
                    {
                        try
                        {
                            Ssutil.DbGetFloat(hStmt, 1, "max(pwrtx)", out tmppwr, out pwrnull);
                        }
                        catch (Exception e)
                        {
                            Log2.w("\nTeBuildSH.CreateChanTable(): WARNING: B: DbGetFloat() threw an exception: " + e.Message);
                            // If there are no pwrtxs at all, then we will still return
                            // ODBC.IsOK, but the result will be null, and DbGetFloat will
                            // throw an exception.
                            // Note: determine if this is actually ever needed?
                            tmppwr = 0.0f;
                            pwrnull = Constant.DB_NULL;
                        }
                    }
                    else
                    {
                        tmppwr = 0.0f;
                        pwrnull = Constant.DB_NULL;
                    }

                    teChan.inttxpwr = tmppwr;
                    teChanNulls[TeChan.INTTXPWR] = pwrnull;

                    rc = SetBandChan("T", tpParm, ref teSite, ref teAnte, ref teAnteNulls, ref teChan,
                        ref teChanNulls, out chanCount, out intPrintMsg, out vicPrintMsg);

                    if ((rc != Constant.SUCCESS) && (rc != Constant.PC_SKIP) && (rc != Constant.CONT_PROCESSING))
                    {
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return (rc);
                    }

                    TeDump(teChan, ref teAnte, ref teAnteNulls, ref teSite, ref teSiteNulls, ref numEtCases, ref numTeCases);

                    /* add record to the ES SH Chan table where ES is the
                    * victim */
                    if ((rc = TeDynChan.TeInsertChan(teChan, teChanNulls)) != Constant.SUCCESS)
                    {
                        ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                        ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                        TpRunTsip.mTW_ERR.Write("Inserting channel returned {0}\n", rc);
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return (rc);
                    }
                }
            }
            else
            {
                /* P/C-add a record for each chan permutation */
                esCriteria = String.Format("location = '{0}' and call1 = '{1}'",
                    teAnte.earthlocation, teAnte.earthcall1);

                //&&Console.Error.Write("\nteBuildSH.createChanTable(): rhino");
                /* read earth channel info */
                if ((esChanHandle = TpMdbPdfGet.SelectEarthChan(esChanTableName, esCriteria, tpParm.envtype)) < 0)
                {
                    Log2.e("\nTeBuildSH.CreateChanTable(): ERROR: SelectEarthChan() failed");
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return (esChanHandle);
                }

                /* for (each channel from the ES table which belongs to
                 * the curr. ES Antenna) */
                //&&Console.Error.Write("\nteBuildSH.createChanTable(): whale");
                while ((rc = TpMdbPdfGet.FetchEarthChan(esChanHandle, out esChan, out esChanNulls, tpParm.envtype)) == Constant.SUCCESS)
                {
                    if (Strings.FirstCharIs((teAnte.interferer), 'E'))
                    {
                        /*	Earth site is the interferer, check if this channel is tx, i.e. has
                        *		a non-zero freqtx.  -- GJS - 1267 - 2008.05 */
                        if (esChan.freqtx == 0.0 || esChanNulls[FeChan.FREQTX] == Constant.DB_NULL)
                        {
                            continue;
                        }

                        /*	Prepare the error message strings */
                        intPrintMsg = String.Format("INTERFERER:   {0,11}     {1,10}    {2,5}",
                            esChan.location,
                            esChan.call1,
                            esChan.chid);
                        /* ES-TS */
                        tsCriteria = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbrx1 = {3} or antnumbrx2 = {4} or antnumbrx3 = {5})",
                            teAnte.terrcall1, teAnte.terrcall2,
                            teAnte.terrbndcde, teAnte.terranum,
                            teAnte.terranum, teAnte.terranum);
                    }
                    else
                    {
                        /*	Earth site is the victim, check if this channel is rx, i.e. has
                        *		a non-zero freqrx.  -- GJS - 1267 - 2008.05 */
                        if (esChan.freqrx == 0.0 || esChanNulls[FeChan.FREQRX] == Constant.DB_NULL)
                        {
                            continue;
                        }

                        vicPrintMsg = String.Format("VICTIM:       {0,11}     {1,10}    {2,5}",
                            esChan.location, esChan.call1,
                            esChan.chid);
                        /* TS-ES */
                        tsCriteria = String.Format("call1 = '{0}' and call2 = '{1}' and bndcde = '{2}' and (antnumbtx1 = {3} or antnumbtx2 = {4})",
                            teAnte.terrcall1, teAnte.terrcall2,
                            teAnte.terrbndcde, teAnte.terranum,
                            teAnte.terranum);
                    }

                    /* read terrestrial channel info */
                    if ((tsChanHandle = TpMdbPdfGet.SelectTerrChan(tsChanTableName, tsCriteria, tpParm.envtype)) < 0)
                    {
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return (tsChanHandle);
                    }

                    /* for (each channel from the TS table which belongs to
                     * the curr. TS Antenna) */
                    while ((rc = TpMdbPdfGet.FetchTerrChan(tsChanHandle, out tsChan, out tsChanNulls, tpParm.envtype)) == Constant.SUCCESS)
                    {
                        if (Strings.FirstCharIs((teAnte.interferer), 'E'))
                        {
                            vicPrintMsg = String.Format("VICTIM:       {0,10}     {1,10}    {2,5}    {3}",
                                tsChan.call1, tsChan.call2, tsChan.bndcde, tsChan.chid);
                        }
                        else
                        {
                            intPrintMsg = String.Format("INTERFERER:   {0,10}     {1,10}    {2,5}    {3}",
                                tsChan.call1, tsChan.call2, tsChan.bndcde, tsChan.chid);
                        }

                        /* cull chan info */
                        if (CullChan(tpParm, teAnte.interferer, esChan, tsChan) == Constant.SUCCESS)
                        {

                            chanCount++;

                            /* 	set the values for an ES Chan SH record from the pro. and env.
                            *		files, teSetPCChan() - calls teChanCalcs() to finish the population
                            *		of the ES Chan SH record */
                            rc1 = TeSetPCChan(tpParm, teSite, teAnte, teAnteNulls, ref teChan, ref teChanNulls, tsChan,
                                tsChanNulls, esChan, esChanNulls, intPrintMsg, vicPrintMsg);

                            if ((rc1 != Constant.SUCCESS) && (rc1 != Constant.PC_SKIP) && (rc1 != Constant.CONT_PROCESSING))
                            {
                                GenUtil.SetErr("Error %d on \n%s into\n%s\n", rc1.ToString(), intPrintMsg, vicPrintMsg);
                                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                                Ssutil.DisConn(hConn);
                                return (rc1);
                            }

                            TeDump(teChan, ref teAnte, ref teAnteNulls, ref teSite, ref teSiteNulls, ref numEtCases, ref numTeCases);

                            /* add record to the ES SH Chan table */
                            if ((rc = TeDynChan.TeInsertChan(teChan, teChanNulls)) != Constant.SUCCESS)
                            {
                                ErrMsg.UtPrintMessage(Error.GENERROR, intPrintMsg);
                                ErrMsg.UtPrintMessage(Error.GENERROR, vicPrintMsg);
                                TpRunTsip.mTW_ERR.Write("Inserting channel returned {0}\n", rc);
                                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                                Ssutil.DisConn(hConn);
                                return (rc);
                            }
                        }
                    } /* end for (each TS channel) */

                    if (rc != Constant.NOMORERECS)
                    {
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return (rc);
                    }

                    TpMdbPdfGet.CloseTerrChan(tsChanHandle, tpParm.envtype);

                } /* end for (each ES channel) */

                if (rc != Constant.NOMORERECS)
                {
                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return (rc);
                }

                TpMdbPdfGet.CloseEarthChan(esChanHandle, tpParm.envtype);
            }

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nTeBuildSH.CreateChanTable(): Exit");

            return (Constant.SUCCESS);

        } /*---- end createChanTable ----*/

        /// <summary>
        /// This method sets the values for an ES Chan SH record from the PDF and 
        /// environment files; ES is the interferer. This method calls TeChanCalcs()
        /// to complete the population of the ES Chan SH record with data.  
        /// </summary>
        /// <param name="interf"> - name of the interferer.</param>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="teAnte"> - TeAnte object.</param>
        /// <param name="teAnteNulls"> - ODBC nullInds for teAnte.</param>
        /// <param name="teChan"> - TeChan object.</param>
        /// <param name="teChanNulls"> - ODBC nullInds for teChan.</param>
        /// <param name="chanCount"> - channel count.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int SetBandChan(string interf,
                                        TpParm tpParm,
                                        ref TeSite teSite,  // Check ref?
                                        ref TeAnte teAnte,  // Check ref?
                                        ref SQLLEN[] teAnteNulls,
                                        ref TeChan teChan,
                                        ref SQLLEN[] teChanNulls,
                                        out int chanCount,
                                        out string intPrintMsg,
                                        out string vicPrintMsg)
        {
            //...Log2.v("\nTeBuildSH.SetBandChan(): Entry");

            chanCount = 1;

            teChan.interferer = interf;
            teChan.terrcall1 = teAnte.terrcall1;
            teChan.terrcall2 = teAnte.terrcall2;
            teChan.terrbndcde = teAnte.terrbndcde;
            teChan.terranum = teAnte.terranum;
            teChan.earthlocation = teAnte.earthlocation;
            teChan.earthcall1 = teAnte.earthcall1;
            teChan.processed = 1;

            teChanNulls[TeChan.INTERFERER] = Constant.DB_NOT_NULL;
            teChanNulls[TeChan.TERRCALL1] = teAnteNulls[TeAnte.TERRCALL1];
            teChanNulls[TeChan.TERRCALL2] = teAnteNulls[TeAnte.TERRCALL2];
            teChanNulls[TeChan.TERRBNDCDE] = teAnteNulls[TeAnte.TERRBNDCDE];
            teChanNulls[TeChan.TERRANUM] = teAnteNulls[TeAnte.TERRANUM];
            teChanNulls[TeChan.EARTHLOCATION] = teAnteNulls[TeAnte.EARTHLOCATION];
            teChanNulls[TeChan.EARTHCALL1] = teAnteNulls[TeAnte.EARTHCALL1];
            teChanNulls[TeChan.PROCESSED] = Constant.DB_NOT_NULL;

            if (Strings.FirstCharIs(interf, 'E'))
            {
                intPrintMsg = String.Format("INTERFERER:   {0,11}     {1,10}",
                    teAnte.earthlocation, teAnte.earthcall1);
                vicPrintMsg = String.Format("VICTIM:       {0,10}     {1,10}    {2,5}    {3,4:D}",
                    teAnte.terrcall1, teAnte.terrcall2,
                    teAnte.terrbndcde, teAnte.terranum);
            }
            else
            {
                vicPrintMsg = String.Format("VICTIM:       {0,11}     {1,10}",
                    teAnte.earthlocation, teAnte.earthcall1);
                intPrintMsg = String.Format("INTEFERER:    {0,10}     {1,10}    {2,5}    {3,4:D}",
                    teAnte.terrcall1, teAnte.terrcall2,
                    teAnte.terrbndcde, teAnte.terranum);
            }

            int rv = TeCalcs.TeChanCalcs(ref teChan, ref teAnte, ref teSite, tpParm, ref teChanNulls, intPrintMsg, vicPrintMsg);

            //...Log2.v("\nTeBuildSH.SetBandChan(): Exit");

            return (rv);

        } /*---- end setBandChan ----*/

        /// <summary>
        /// This method sets various report flags and case/subcase numbers in 
        /// the site, ante and chan records.  
        /// </summary>
        /// <param name="teChan"> - TeChan object.</param>
        /// <param name="teAnte"> - TeAnte object.</param>
        /// <param name="anteNulls"> - ODBC nullInds for teAnte</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="siteNulls"> - ODBC nullInds for teSite</param>
        /// <param name="numETSitesRptd"> - number of Et sites to be reported.</param>
        /// <param name="numTESitesRptd"> - number of Te sites to be reported.</param>
        public static void TeDump(TeChan teChan,
                                    ref TeAnte teAnte,
                                    ref SQLLEN[] anteNulls,
                                    ref TeSite teSite,
                                    ref SQLLEN[] siteNulls,
                                    ref int numETSitesRptd,
                                    ref int numTESitesRptd)
        {
            //...Log2.v("\nTeBuildSH.TeDump(): Entry");

            if (teChan.etreport == Constant.TRUE)
            {
                if (teSite.etreport != Constant.TRUE)
                {
                    teSite.etreport = Constant.TRUE;
                    teSite.etcaseno = ++numETSitesRptd;
                    teSite.etsubcases = 1;
                    siteNulls[TeSite.ETREPORT] = Constant.DB_NOT_NULL;
                    siteNulls[TeSite.ETCASENO] = Constant.DB_NOT_NULL;
                    siteNulls[TeSite.ETSUBCASES] = Constant.DB_NOT_NULL;

                }
                else if (teAnte.etreport != Constant.TRUE)
                {
                    teSite.etsubcases++;
                }
                if (teAnte.etreport != Constant.TRUE)
                {
                    teAnte.etreport = Constant.TRUE;
                    teAnte.etsubcaseno = teSite.etsubcases;
                    anteNulls[TeAnte.ETREPORT] = Constant.DB_NOT_NULL;
                    anteNulls[TeAnte.ETSUBCASENO] = Constant.DB_NOT_NULL;
                }
            }
            if (teChan.tereport == Constant.TRUE)
            {
                if (teSite.tereport != Constant.TRUE)
                {
                    teSite.tereport = Constant.TRUE;
                    teSite.tecaseno = ++numTESitesRptd;
                    teSite.tesubcases = 1;
                    siteNulls[TeSite.TEREPORT] = Constant.DB_NOT_NULL;
                    siteNulls[TeSite.TECASENO] = Constant.DB_NOT_NULL;
                    siteNulls[TeSite.TESUBCASES] = Constant.DB_NOT_NULL;
                }
                else if (teAnte.tereport != Constant.TRUE)
                {
                    teSite.tesubcases++;
                }
                if (teAnte.tereport != Constant.TRUE)
                {
                    teAnte.tereport = Constant.TRUE;
                    teAnte.tesubcaseno = teSite.tesubcases;
                    anteNulls[TeAnte.TEREPORT] = Constant.DB_NOT_NULL;
                    anteNulls[TeAnte.TESUBCASENO] = Constant.DB_NOT_NULL;
                }
            }

            //...Log2.v("\nTeBuildSH.TeDump(): Exit");
        } /*---- end teDump ----*/

        /// <summary>
        /// This method culls the channels from the environment based 
        /// on channel status precribed by the User.  
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</param>
        /// <param name="interferer"> - name of interferer.</param>
        /// <param name="esChan"> - FeChan object.</param>
        /// <param name="tsChan"> - FtChan object.</param>
        /// <returns></returns>
        public static int CullChan(TpParm tpParm, string interferer, FeChan esChan, FtChan tsChan)
        {
            //...Log2.v("\nTeBuildSH.CullChan(): Entry");

            int rc;
            int getRc;
            string codesList;
            string aCode;

            rc = Constant.FAILURE;
            codesList = tpParm.chancodes;

            getRc = GenUtil.UtGetInputString(codesList, out aCode, 1);

            while (rc != Constant.SUCCESS && getRc >= 0)
            {

                /* if (proposed file type is "ES") */
                if (Strings.FirstCharIs((tpParm.protype), 'E'))
                {
                    /* ES (proposed) - TS (environment) */

                    /* if the environment(TS) is the interferer use it's
                     * transmit status */
                    if ((Strings.FirstCharIs(interferer, 'T'))
                        /* and (stattx of curr TS channel is in the list of
                         * channel codes to cull) */
                        && (aCode.Equals(tsChan.stattx)))
                    {
                        rc = Constant.SUCCESS;
                    }

                    /* if the environment(TS) is the victim use it's
                     * recieve status */
                    if ((Strings.FirstCharIs(interferer, 'E'))
                        /* and (statrx of curr TS channel is in the list of
                         * channel codes to cull) */
                        && (aCode.Equals(tsChan.statrx)))
                    {
                        rc = Constant.SUCCESS;
                    }
                }
                else
                {
                    /* TS (proposed) - ES (environment) */

                    /* if the environment(ES) is the interferer use it's
                     * transmit status */
                    if ((Strings.FirstCharIs(interferer, 'E'))
                        /* and (stattx of curr ES channel is in the list of
                         * channel codes to cull) */
                        && (aCode.Equals(esChan.stattx)))
                    {
                        rc = Constant.SUCCESS;
                    }

                    /* if the environment(ES) is the victim use it's
                     * recieve status */
                    if ((Strings.FirstCharIs(interferer, 'T'))
                        /* and (statrx of curr ES channel is in the list of
                         * channel codes to cull) */
                        && (aCode.Equals(esChan.statrx)))
                    {
                        rc = Constant.SUCCESS;
                    }
                }

                getRc = GenUtil.UtGetInputString(null, out aCode, 1);
            }

            //...Log2.v("\nTeBuildSH.CullChan(): Exit");

            return (rc);

        } /*---- end cullChan ----*/

        /// <summary>
        /// This method sets the values for an ES Chan SH record using source data from the PDF and environment files; it calls TeChanCalcs() to finish the
        /// population of the ES Chan SH record with data.
        /// </summary>
        /// <param name="tpParm"> - TpParm object providing paramater data.</</param>
        /// <param name="teSite"> - TeSite object.</param>
        /// <param name="teAnte"> - TeAnte object.</param>
        /// <param name="teAnteNulls"> - ODBC nullInds for teAnte.</param>
        /// <param name="teChan"> - TeChan object.</param>
        /// <param name="teChanNulls"> - ODBC nullInds for teChan.</param>
        /// <param name="tsChan"> - FtChan object.</param>
        /// <param name="tsChanNulls"> - ODBC nullInds for tsChan.</param>
        /// <param name="esChan"> - FeChan object.</param>
        /// <param name="esChanNulls"> - ODBC nullInds for esChan.</param>
        /// <param name="intPrintMsg"> - interferer: explanatory message to be written if an error occurs.</param>
        /// <param name="vicPrintMsg"> - victim: explanatory message to be written if an error occurs.</param>
        /// <returns></returns>
        public static int TeSetPCChan(TpParm tpParm,
                                        TeSite teSite,
                                        TeAnte teAnte,
                                        SQLLEN[] teAnteNulls,
                                        ref TeChan teChan,
                                        ref SQLLEN[] teChanNulls,
                                        FtChan tsChan,
                                        SQLLEN[] tsChanNulls,
                                        FeChan esChan,
                                        SQLLEN[] esChanNulls,
                                        string intPrintMsg,
                                        string vicPrintMsg)
        {
            //...Log2.v("\nTeBuildSH.TeSetPCChan(): Entry");

            teChan.interferer = teAnte.interferer;
            teChan.terrcall1 = teAnte.terrcall1;
            teChan.terrcall2 = teAnte.terrcall2;
            teChan.terrbndcde = teAnte.terrbndcde;
            teChan.terranum = teAnte.terranum;
            teChan.terrchid = tsChan.chid;
            teChan.earthlocation = teAnte.earthlocation;
            teChan.earthcall1 = teAnte.earthcall1;
            teChan.earthchid = esChan.chid;
            teChan.processed = Constant.TRUE;

            teChanNulls[TeChan.INTERFERER] = Constant.DB_NOT_NULL;
            teChanNulls[TeChan.TERRCALL1] = teAnteNulls[TeAnte.TERRCALL1];
            teChanNulls[TeChan.TERRCALL2] = teAnteNulls[TeAnte.TERRCALL2];
            teChanNulls[TeChan.TERRBNDCDE] = teAnteNulls[TeAnte.TERRBNDCDE];
            teChanNulls[TeChan.TERRANUM] = teAnteNulls[TeAnte.TERRANUM];
            teChanNulls[TeChan.TERRCHID] = tsChanNulls[FtChan.CHID];
            teChanNulls[TeChan.EARTHLOCATION] = teAnteNulls[TeAnte.EARTHLOCATION];
            teChanNulls[TeChan.EARTHCALL1] = teAnteNulls[TeAnte.EARTHCALL1];
            teChanNulls[TeChan.EARTHCHID] = esChanNulls[FeChan.CHID];
            teChanNulls[TeChan.PROCESSED] = Constant.DB_NOT_NULL;

            if (Strings.FirstCharIs((teAnte.interferer), 'E'))
            {
                teChan.inttraftx = esChan.traftx;
                teChan.victrafrx = tsChan.trafrx;
                teChan.inteqpttx = esChan.eqpttx;
                teChan.viceqptrx = tsChan.eqptrx;
                teChan.stattx = esChan.stattx;
                teChan.statrx = tsChan.statrx;
                teChan.intfreqtx = esChan.freqtx;
                teChan.inttxpwr = esChan.pwrtx;
                teChan.energy = esChan.p4khz;
                teChan.vicfreqrx = tsChan.freqrx;

                teChanNulls[TeChan.INTTRAFTX] = esChanNulls[FeChan.TRAFTX];
                teChanNulls[TeChan.VICTRAFRX] = tsChanNulls[FtChan.TRAFRX];
                teChanNulls[TeChan.INTEQPTTX] = esChanNulls[FeChan.EQPTTX];
                teChanNulls[TeChan.VICEQPTRX] = tsChanNulls[FtChan.EQPTRX];
                teChanNulls[TeChan.INTFREQTX] = esChanNulls[FeChan.FREQTX];
                teChanNulls[TeChan.INTTXPWR] = esChanNulls[FeChan.PWRTX];
                teChanNulls[TeChan.VICFREQRX] = tsChanNulls[FtChan.FREQRX];
                teChanNulls[TeChan.STATTX] = esChanNulls[FeChan.STATTX];
                teChanNulls[TeChan.ENERGY] = esChanNulls[FeChan.P4KHZ];
                teChanNulls[TeChan.STATRX] = tsChanNulls[FtChan.STATRX];

                if (teChan.terranum == tsChan.antnumbrx1)
                {
                    teChan.vicpwrrx = (double)tsChan.pwrrx1;
                    teChan.terrant = 1;
                    teChan.vicrxafls = tsChan.afslrx1;
                    teChanNulls[TeChan.VICRXAFLS] = Constant.DB_NOT_NULL;
                    teChanNulls[TeChan.VICPWRRX] = tsChanNulls[FtChan.PWRRX1];
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NOT_NULL;
                }
                else if (teChan.terranum == tsChan.antnumbrx2)
                {
                    teChan.vicpwrrx = (double)tsChan.pwrrx2;
                    teChan.terrant = 2;
                    teChan.vicrxafls = tsChan.afslrx2;
                    teChanNulls[TeChan.VICRXAFLS] = Constant.DB_NOT_NULL;
                    teChanNulls[TeChan.VICPWRRX] = tsChanNulls[FtChan.PWRRX2];
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NOT_NULL;
                }
                else if (teChan.terranum == tsChan.antnumbrx3)
                {
                    teChan.vicpwrrx = (double)tsChan.pwrrx3;
                    teChan.terrant = 3;
                    teChan.vicrxafls = tsChan.afslrx3;
                    teChanNulls[TeChan.VICRXAFLS] = Constant.DB_NOT_NULL;
                    teChanNulls[TeChan.VICPWRRX] = tsChanNulls[FtChan.PWRRX3];
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NOT_NULL;
                }
                else
                {
                    teChanNulls[TeChan.VICRXAFLS] = Constant.DB_NULL;
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NULL;
                }
            }
            else
            {
                teChan.inttraftx = tsChan.traftx;
                teChan.victrafrx = esChan.trafrx;
                teChan.inteqpttx = tsChan.eqpttx;
                teChan.viceqptrx = esChan.eqptrx;
                teChan.stattx = tsChan.stattx;
                teChan.statrx = esChan.statrx;
                teChan.intfreqtx = tsChan.freqtx;
                teChan.inttxpwr = tsChan.pwrtx;
                teChan.vicfreqrx = esChan.freqrx;
                teChan.energy = esChan.p4khz;
                teChan.vicpwrrx = esChan.pwrrx;

                teChanNulls[TeChan.INTTRAFTX] = tsChanNulls[FtChan.TRAFTX];
                teChanNulls[TeChan.VICTRAFRX] = esChanNulls[FeChan.TRAFRX];
                teChanNulls[TeChan.INTEQPTTX] = tsChanNulls[FtChan.EQPTTX];
                teChanNulls[TeChan.VICEQPTRX] = esChanNulls[FeChan.EQPTRX];
                teChanNulls[TeChan.INTFREQTX] = tsChanNulls[FtChan.FREQTX];
                teChanNulls[TeChan.INTTXPWR] = tsChanNulls[FtChan.PWRTX];
                teChanNulls[TeChan.VICFREQRX] = esChanNulls[FeChan.FREQRX];
                teChanNulls[TeChan.VICPWRRX] = esChanNulls[FeChan.PWRRX];
                teChanNulls[TeChan.STATTX] = tsChanNulls[FtChan.STATTX];
                teChanNulls[TeChan.ENERGY] = esChanNulls[FeChan.P4KHZ];
                teChanNulls[TeChan.STATRX] = esChanNulls[FeChan.STATRX];

                if (teChan.terranum == tsChan.antnumbtx1)
                {
                    teChan.terrant = 1;
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NOT_NULL;
                    teChan.inttxafls = tsChan.afsltx1;
                    teChanNulls[TeChan.INTTXAFLS] = Constant.DB_NOT_NULL;
                }
                else if (teChan.terranum == tsChan.antnumbtx2)
                {
                    teChan.terrant = 2;
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NOT_NULL;
                    teChan.inttxafls = tsChan.afsltx2;
                    teChanNulls[TeChan.INTTXAFLS] = Constant.DB_NOT_NULL;
                }
                else
                {
                    teChanNulls[TeChan.TERRANT] = Constant.DB_NULL;
                    teChanNulls[TeChan.INTTXAFLS] = Constant.DB_NULL;
                }
            }

            int rv = TeCalcs.TeChanCalcs(ref teChan, ref teAnte, ref teSite, tpParm, ref teChanNulls, intPrintMsg, vicPrintMsg);

            //...Log2.v("\nTeBuildSH.TeSetPCChan(): Exit");

            return (rv);

        } /*---- end teSetPCChan ----*/










    }
}

```
