using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtImport
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;

    /// <summary>
    /// This class provides method that text for the existence of ft_ tables 
    /// in the DB that have prescribed key field.
    /// </summary>
    public class FtRecExist
    {
        // Legacy C code uses FOUND, NOTFOUND and ODBC returned call values.
        private static int mLastReturnCode = -666;

        public static int LastReturnCode
        {
            get { return mLastReturnCode; }
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _site database table contains
        /// a record that has a prescribed key field call1.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="siteStruct"> - an FtSite object that prescribes the call1 to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FtSiteExist(string tableName, FtSite siteStruct)
        {
            bool result = false;
            int cursor;
            FtSite tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("call1 = '{0}'", siteStruct.call1);

            cursor = DynSite.FtSelectSite(tableName, searchCriteria, "call1");

            if (cursor >= 0)
            {
                rc = DynSite.FtFetchSite(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFtRecExist.FtSiteExist(): ERROR: call to FtFetchSite() failed, returned: " + rc);
                    }
                }
                else  // Fetch succeeded.
                {
                    mLastReturnCode = Constant.FOUND;
                    result = true;
                }
            }
            else
            {
                //...Log2.e("\nFtRecExist.FtSiteExist(): ERROR: call to FtSelectSite() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynSite.FtCloseSite(cursor);
            return result;
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _ante database table contains
        /// a record that has prescribed keys call1, call2, bndcde and anum.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="ftAnte"> - an FtAnte object that prescribes the location and call sign to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FtAnteExist(string tableName, FtAnte ftAnte)
        {
            bool result = false;
            int cursor;
            FtAnte tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("call1 = '{0}' and call2 = {1} and bndcde = {2} and anum = {3}",
                                            ftAnte.call1, ftAnte.call2, ftAnte.bndcde, ftAnte.anum);

            cursor = DynAntenna.FtSelectAntenna(tableName, searchCriteria, "call1, call2, bndcde, anum");

            if (cursor >= 0)
            {
                rc = DynAntenna.FtFetchAntenna(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFtRecExist.FtAnteExist(): ERROR: call to FtFetchAntenna() failed, returned: " + rc);
                    }
                }
                else  // Fetch succeeded.
                {
                    mLastReturnCode = Constant.FOUND;
                    result = true;
                }
            }
            else
            {
                //...Log2.e("\nFtRecExist.FtAnteExist(): ERROR: call to FtSelectAntenna() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynAntenna.FtCloseAntenna(cursor);
            return result;
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _chan database table contains
        /// a record that has a prescribed keys call1, call2, bndcde and chid.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="ftChan"> - an FtChan object that prescribes the location, call sign and channel ID to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FtChanExist(string tableName, FtChan ftChan)
        {
            bool result = false;
            int cursor;
            FtChan tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("call1 = '{0}' and call2 = {1} and bndcde = {2} and chid = '{3}'",
                                            ftChan.call1, ftChan.call2, ftChan.bndcde, ftChan.chid);

            cursor = DynChannel.FtSelectChannel(tableName, searchCriteria, "location,call1,chid");

            if (cursor >= 0)
            {
                rc = DynChannel.FtFetchChannel(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFtRecExist.FtChanExist(): ERROR: call to FtFetchChan() failed, returned: " + rc);
                    }
                }
                else  // Fetch succeeded.
                {
                    mLastReturnCode = Constant.FOUND;
                    result = true;
                }
            }
            else
            {
                //...Log2.e("\nFtRecExist.FtChanExist(): ERROR: call to FtSelectChan() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynChannel.FtCloseChannel(cursor);
            return result;
        }


        /// <summary>
        /// This method determines whether, or not, a prescribed _chng database table contains
        /// a record that has a prescribed 'old' call sign.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="ftChng"> - an FtChng object that prescribes the 'old' call sign to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FtChngExist(string tableName, FtChng ftChng)
        {
            bool result = false;
            int cursor;
            FtChng tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("oldcall1= '{0}'", ftChng.oldcall1);

            cursor = DynChange.FtSelectChngCall(tableName, searchCriteria, "oldcall1");

            if (cursor >= 0)
            {
                rc = DynChange.FtFetchChngCall(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        Log2.e("\nFtRecExist.FtChngExist(): ERROR: call to FtFetchChngCall() failed, returned: " + rc);
                    }
                }
                else  // Fetch succeeded.
                {
                    mLastReturnCode = Constant.FOUND;
                    result = true;
                }
            }
            else
            {
                Log2.e("\nFtRecExist.FtChngExist(): ERROR: call to FtSelectChngCall() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynChange.FtCloseChngCall(cursor);
            return result;
        }

    }
}
