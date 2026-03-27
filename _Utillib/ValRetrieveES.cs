using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _DataStructures;
    using _NewLib;
    using _Utillib;

    using SQLLEN = Int64;

    /// <summary>
    /// Provides methods to retrieve the first site, antenna or channel record from the 
    /// database tables <b>main.me_site, </b><b>main.me_ante</b>, <b>main.me_azim</b> or 
    /// <b>main.me_chan</b> that matches prescribed SQL 'WHERE' search criteria and 'ORDER BY' clauses.
    /// The corresponding column nullInd information is also retrieved.
    /// </summary>
    public class ValRetrieveES
    {
        /// <summary>
        /// Retrieves the first site record from the database table <b>main.me_site</b> that
        /// matches the prescribed SQL 'WHERE' search clause. The 
        /// corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="meSite"> - populated MeSite object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FeValRetrieveMDBSite(out MeSite meSite,            /* site information */
                                               string whereClause,    /* search clause for the MDB */
                                               out SQLLEN[] nArrayMDB)     /* array with null flags */
        {
            // 'out' requirement.
            meSite = null;
            nArrayMDB = null;

            int nRet;
            int nHandle;

            nHandle = DynMeSite.MeSelectSite(whereClause, null);

            // Check if the select query worked.
            if (nHandle < 0)
            {
                return nHandle;
            }

            nRet = DynMeSite.MeFetchSite(nHandle, out meSite, out nArrayMDB);

            DynMeSite.MeCloseSite(nHandle);

            return nRet;
        }

        /// <summary>
        /// Retrieves the first antenna record from the database table <b>main.me_ante</b> that
        /// matches the prescribed SQL 'WHERE' search clause.
        ///  The corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="meAnte"> - populated MeAnte object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FeValRetrieveMDBAntenna(out MeAnte meAnte, /* antenna info */
                string whereClause, /* search clause for the MDB */
                out SQLLEN[] nArrayMDB) /* array with null flags */
        {
            // 'out' requirement.
            meAnte = null;
            nArrayMDB = null;

            int nHandle;
            int nRet;

            nHandle = DynMeAnte.MeSelectAnte(whereClause, null);

            // Check if the select query worked.
            if (nHandle < 0)
            {
                return nHandle;
            }

            nRet = DynMeAnte.MeFetchAnte(nHandle, out meAnte, out nArrayMDB);

            DynMeAnte.MeCloseAnte(nHandle);

            return nRet;
        }

        /// <summary>
        /// Retrieves the first channel record from the database table <b>main.me_chan</b> that
        /// matches the prescribed SQL 'WHERE' search criteria and 'ORDER BY' clauses.
        /// The corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="meChan"> - populated MeChan object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FeValRetrieveMDBChannel(out MeChan meChan, /* channel record */
                                                                string whereClause,         /* search clause for the MDB */
                                                                out SQLLEN[] nArrayMDB)     /* array with null flags */
        {
            // 'out' requirement.
            meChan = null;
            nArrayMDB = null;

            int nHandle;
            int nRet;

            nHandle = DynMeChan.MeSelectChan(whereClause, null);

            // Check if the select query worked.
            if (nHandle < 0)
            {
                return nHandle;
            }

            nRet = DynMeChan.MeFetchChan(nHandle, out meChan, out nArrayMDB);

            DynMeChan.MeCloseChan(nHandle);

            return nRet;
        }

        /// <summary>
        /// Retrieves the first channel record from the database table <b>main.me_azim</b> that
        /// matches the prescribed SQL 'WHERE' search criteria and 'ORDER BY' clauses.
        /// The corresponding column nullInd information is also retrieved.
        /// </summary>
        /// <param name="meAzim"> - populated MeAzim object.</param>
        /// <param name="whereClause"> - parameters to follow the SQL 'WHERE' token.</param>
        /// <param name="nArrayMDB"> - populated array of ODBC nullInds.</param>
        /// <returns></returns>
        /// <para>-   Constant.SUCCESS                - fetch attempt was successful.</para>
        /// <para>-   Constant.FAILURE                - fetch attempt failed.</para>
        public static int FeValRetrieveMDBAzimuth(out MeAzim meAzim, /* azimuth record */
                                                                string whereClause, /* search clause for the MDB */
                                                                out SQLLEN[] nArrayMDB)     /* array with null flags */
        {
            // 'out' requirement.
            meAzim = null;
            nArrayMDB = null;

            int nHandle;
            int nRet;

            nHandle = DynMeAzim.MeSelectAzim(whereClause, null);

            // Check if the select query worked.
            if (nHandle < 0)
            {
                return nHandle;
            }

            nRet = DynMeAzim.MeFetchAzim(nHandle, out meAzim, out nArrayMDB);

            DynMeAzim.MeCloseAzim(nHandle);

            return nRet;
        }






    }
}
