# Documented File: FeRecExist.cs
**Repository Path:** `FeImport\FeRecExist.cs`
**Primary Layer:** `FeImport`
**Namespace:** `FeImport`

## Source Code Representation
```csharp
﻿using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeImport
{
    using _Configuration;
    using _NewLib;
    using _Utillib;
    using SQLLEN = Int64;
    public class FeRecExist
    {
        // Legacy C code uses FOUND, NOTFOUND and ODBC returned call values.
        private static int mLastReturnCode = -666;

        public static int LastReturnCode
        {
            get { return mLastReturnCode; }
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _site database table contains
        /// a record that has a prescribed location.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="siteStruct"> - an FeSite object that prescribes the location to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FeSiteExist(string tableName, FeSite siteStruct)
        {
            bool result = false;
            int cursor;
            FeSite tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("location = '{0}'", siteStruct.location);

            cursor = DynFeSite.FeSelectSite(tableName, searchCriteria, "location");

            if (cursor >= 0)
            {
                rc = DynFeSite.FeFetchSite(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFeRecExist.FeSiteExist(): ERROR: call to FeFetchSite() failed, returned: " + rc);
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
                //...Log2.e("\nFeRecExist.FeSiteExist(): ERROR: call to FeSelectSite() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynFeSite.FeCloseSite(cursor);
            return result;
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _ante database table contains
        /// a record that has a prescribed location and call sign.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="feAnte"> - an FeAnte object that prescribes the location and call sign to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FeAnteExist(string tableName, FeAnte feAnte)
        {
            bool result = false;
            int cursor;
            FeAnte tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("location = '{0}' and call1 = '{1}'",
                                            feAnte.location, feAnte.call1);

            cursor = DynFeAnte.FeSelectAnte(tableName, searchCriteria, "location,call1");

            if (cursor >= 0)
            {
                rc = DynFeAnte.FeFetchAnte(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFeRecExist.FeAnteExist(): ERROR: call to FeFetchAnte() failed, returned: " + rc);
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
                //...Log2.e("\nFeRecExist.FeAnteExist(): ERROR: call to FeSelectAnte() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynFeAnte.FeCloseAnte(cursor);
            return result;
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _chan database table contains
        /// a record that has a prescribed location, call sign and channel ID.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="feChan"> - an FeChan object that prescribes the location, call sign and channel ID to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FeChanExist(string tableName, FeChan feChan)
        {
            bool result = false;
            int cursor;
            FeChan tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("location = '{0}' and call1 = '{1}' and chid = '{2}'",
                                            feChan.location, feChan.call1, feChan.chid);

            cursor = DynFeChan.FeSelectChan(tableName, searchCriteria, "location,call1,chid");

            if (cursor >= 0)
            {
                rc = DynFeChan.FeFetchChan(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFeRecExist.FeChanExist(): ERROR: call to FeFetchChan() failed, returned: " + rc);
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
                //...Log2.e("\nFeRecExist.FeChanExist(): ERROR: call to FeSelectChan() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynFeChan.FeCloseChan(cursor);
            return result;
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _cloc database table contains
        /// a record that has a prescribed 'old' location.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="feCLoc"> - an FeCLoc object that prescribes the 'old' location to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FeChngLocExist(string tableName, FeCLoc feCLoc)
        {
            bool result = false;
            int cursor;
            FeCLoc tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("oldlocation = '{0}'", feCLoc.oldlocation);

            cursor = DynFeCLoc.FeSelectCLoc(tableName, searchCriteria, "oldlocation");

            if (cursor >= 0)
            {
                rc = DynFeCLoc.FeFetchCLoc(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFeRecExist.FeCLocExist(): ERROR: call to FeFetchCLoc() failed, returned: " + rc);
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
                //...Log2.e("\nFeRecExist.FeCLocExist(): ERROR: call to FeSelectCLoc() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynFeCLoc.FeCloseCLoc(cursor);
            return result;
        }

        /// <summary>
        /// This method determines whether, or not, a prescribed _ccal database table contains
        /// a record that has a prescribed 'old' call sign.
        /// </summary>
        /// <param name="tableName"> - name of the database table to be searched.</param>
        /// <param name="feCCal"> - an FeCCal object that prescribes the 'old' call sign to be searched for.</param>
        /// <returns>True or false.</returns>
        public static bool FeChngCallExist(string tableName, FeCCal feCCal)
        {
            bool result = false;
            int cursor;
            FeCCal tempStruct;
            SQLLEN[] tempNulls;
            int rc;
            string searchCriteria;

            searchCriteria = String.Format("oldcallsign = '{0}'", feCCal.oldcallsign);

            cursor = DynFeCCal.FeSelectCCal(tableName, searchCriteria, "oldcallsign");

            if (cursor >= 0)
            {
                rc = DynFeCCal.FeFetchCCal(cursor, out tempStruct, out tempNulls);

                if (rc != Constant.SUCCESS)
                {
                    if (rc == ODBC.SQL_NO_DATA)
                    {
                        mLastReturnCode = Constant.NOT_FOUND;
                    }
                    else
                    {
                        mLastReturnCode = Error.ODBC_FETCH_FAILED;
                        //...Log2.e("\nFeRecExist.FeCCalExist(): ERROR: call to FeFetchCCal() failed, returned: " + rc);
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
                //...Log2.e("\nFeRecExist.FeCCalExist(): ERROR: call to FeSelectCCal() failed, returned: " + cursor);
                mLastReturnCode = Error.ODBC_SELECT_FAILED;
            }

            DynFeCCal.FeCloseCCal(cursor);
            return result;
        }

    }
}

```
