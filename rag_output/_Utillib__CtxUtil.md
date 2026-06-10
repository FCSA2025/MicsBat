# Documented File: CtxUtil.cs
**Repository Path:** `_Utillib\CtxUtil.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
    using _Configuration;
    using _NewLib;
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
    /// Provides methods that support the Caching of 
    /// TcTxAnalog and TcTxDigital objects to avoid unnecessary
    /// fetches from DB tables.
    /// </summary>
    public class CtxUtil
    {
        /// <summary>
        /// This class provide cache functionality for TcTxAnalog objects
        /// by extending the generic Cache class.
        /// </summary>
        private class AnalogCache : Cache<TcTxAnalog>
        {

            /// <summary>
            /// This method overrides the constructor in the base class and
            /// prescribes the maximum number of elements that can be stored
            /// in the cache.
            /// </summary>
            /// <param name="maxSize"> - eponym.</param>
            public AnalogCache(int maxSize) : base(maxSize) { }

            /// <summary>
            /// This method overrides the base class method and returns true if
            /// we have a cache 'hit' for the keys {ecode, trafcode}.
            /// </summary>
            /// <param name="t1"> - TcTxAnalog object</param>
            /// <param name="t2"></param>
            /// <returns></returns>
            public override bool Hit(TcTxAnalog t1, TcTxAnalog t2)
            {
                return t1.ecode.Equals(t2.ecode) &&
                    t1.trafcode.Equals(t2.trafcode);
            }

            /// <summary>
            /// This method returns a 'safe' copy of a cache element
            /// that has been 'hit'. In this instance, it is safe to
            /// return access to the cached element itself as this is never
            /// modified by the caller.
            /// </summary>
            /// <param name="t"> - TcTxAnalog object.</param>
            /// <returns></returns>
            public override TcTxAnalog SafeCopy(TcTxAnalog t)
            {
                return t;
            }
        }
        private static AnalogCache mAnalogCache = new AnalogCache(Constant.ANALOGCACHE_SIZE_);

        /// <summary>
        /// This class provide cache functionality for TcTxDigital objects
        /// by extending the generic Cache class.
        /// </summary>
        private class DigitalCache : Cache<TcTxDigital>
        {
            /// <summary>
            /// This method overrides the constructor in the base class and
            /// prescribes the maximum number of elements that can be stored
            /// in the cache.
            /// </summary>
            /// <param name="maxSize"> - eponym.</param>
            public DigitalCache(int maxSize) : base(maxSize) { }

            /// <summary>
            /// This method overrides the base class method and returns true if
            /// we have a cache 'hit' for the keys {ecode, trafcode}.
            /// </summary>
            /// <param name="t1"> - TxTxDigital object.</param>
            /// <param name="t2"> - TxTxDigital object.</param>
            /// <returns></returns>
            public override bool Hit(TcTxDigital t1, TcTxDigital t2)
            {
                return t1.ecode.Equals(t2.ecode) &&
                    t1.trafcode.Equals(t2.trafcode);
            }

            /// <summary>
            /// This method returns a 'safe' copy of a cache element
            /// that has been 'hit'. In this instance, it is safe to
            /// return access to the cached element itself as this is never
            /// modified by the caller.
            /// </summary>	
            /// <param name="t"> - TcTxDigital object.</param>
            /// <returns></returns>
            public override TcTxDigital SafeCopy(TcTxDigital t)
            {
                return t;
            }
        }
        private static DigitalCache mDigitalCache = new DigitalCache(Constant.ANALOGCACHE_SIZE_);

        /// <summary>
        /// This method returns a TcTxAnalog analog equipment object, even if we have to 
        /// go through the cross reference chain.  
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="ptAnalog"> - TcTxAnalog object.</param>
        /// <returns></returns>
        public static int GetAeqpt(string cEqpt, string cTraf, out TcTxAnalog ptAnalog)
        {
            int nRet;
            SuTraf tTraf;

            if ((nRet = GetAnalog(cEqpt, cTraf, out ptAnalog)) != 0)
            {
                /*	Not found.  Get the traffic cross reference */
                if ((nRet = Suutils.SuGetTraf(cTraf, cEqpt, out tTraf)) == 0)
                {
                    /*	Got the traffic record, now try to find the cross reference */
                    nRet = GetAnalog(tTraf.xrefeqcde, tTraf.xreftrcde, out ptAnalog);
                }
                /*	If we could not either get a cross-reference or retrieve the cross
                *		reference equipment, then get the defaults */
                if (nRet != 0)
                {
                    /*	Not found.  Use default worst case  */
                    nRet = GetAnalog("UNKNOWN", "A1200", out ptAnalog);
                }
            }

            return (nRet);
        }

        /// <summary>
        /// This method returns a TcTxDigital digital equipment object, even if we have to 
        /// go through the cross reference chain.  
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="ptDigital"> - TcTxDigital object.</param>
        /// <returns></returns>
        public static int GetDeqpt(string cEqpt, string cTraf, out TcTxDigital ptDigital)
        {
            int nRet;
            SuTraf tTraf;

            if ((nRet = GetDigital(cEqpt, cTraf, out ptDigital)) != 0)
            {
                /*	Not found.  Get the traffic cross reference */
                if ((nRet = Suutils.SuGetTraf(cTraf, cEqpt, out tTraf)) == 0)
                {
                    /*	Got the traffic record, now try to find the cross reference */
                    nRet = GetDigital(tTraf.xrefeqcde, tTraf.xreftrcde, out ptDigital);
                }
                /*	If we could not either get a cross-reference or retrieve the cross
                *		reference equipment, then get the defaults */
                if (nRet != 0)
                {
                    /*	Not found.  Use default worst case  */
                    nRet = GetDigital("DFLT80", "D7580", out ptDigital);
                }
            }

            return (nRet);
        }

        /// <summary>
        /// This method method requests a populated TcTxAnalog object, with 
        /// prescribed search key {cEqpt, cTraf}. The method first searches 
        /// the cache for an element with a matching key; if there is no cache
        /// 'hit' the TcTxAnalog object is populated with data fetched from the DB.
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="pTanalog"> - TcTxAnalog  object.</param>
        /// <returns></returns>
        public static int GetAnalog(string cEqpt, string cTraf, out TcTxAnalog pTanalog)
        {
            // 'out' requirement.
            pTanalog = null;

            //	exec sql end declare section;
            SQLLEN nIsNull;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            string cSQL;

            int nRet;

            // Check if it is in the cache; if so, get it and return it.
            if ((nRet = AnalogGetCache(cEqpt, cTraf, out pTanalog)) == Constant.SUCCESS)
            {
                return Constant.SUCCESS;
            }

            // It was not in the cache so retrieve it from the DB.
            cSQL = String.Format("select numchan, fmin, fm, sigma, nf, iffreq, sif, irf, ai70, ai140 from tsip.ctxaeqpt where ecode='{0}' and trafcode='{1}' ",
                                 cEqpt, cTraf);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nCtxUtil.GetAnalog(): ERROR: call to SQLExecDirect failed.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                int rv = Constant.FAILURE;
                if (ODBC.IsNoData(sqlRet))
                {
                    Log2.e("\nCtxUtil.GetAnalog(): ERROR: call to SQLFetch() returned SQL_NO_DATA.");
                    rv = ODBC.SQL_NO_DATA;
                }
                else
                {
                    Log2.e("\nCtxUtil.GetAnalog(): ERROR: call to SQLFetch() failed, returned: " + sqlRet);
                    rv = Error.ODBC_FETCH_FAILED;
                }
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return rv;
            }

            // The fetch succeeded, now retrieve the column data.
            pTanalog = new TcTxAnalog();
            pTanalog.ecode = cEqpt;
            pTanalog.trafcode = cTraf;

            try
            {
                Ssutil.DbGetInt(hStmt, 1, "numchan", out pTanalog.numchan, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 2, "fmin", out pTanalog.fmin, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 3, "fm", out pTanalog.fm, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 4, "sigma", out pTanalog.sigma, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 5, "nf", out pTanalog.nf, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 6, "iffreq", out pTanalog.iffreq, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 7, "sif", out pTanalog.sif, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 8, "irf", out pTanalog.irf, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 9, "ai70", out pTanalog.ai70, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 10, "ai140", out pTanalog.ai140, out nIsNull);
            }
            catch (Exception e)
            {
                Log2.e("\nCtxUtil.GetAnalog(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                string str = String.Format("getanalog04: Error retrieving field {0} for {1}-{2}", e.Message, cEqpt, cTraf);
                GenUtil.SetErr(str);

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_GET_FAILED;
            }

            GetAdArray(cEqpt, cTraf, "F", out pTanalog.nFilter, out pTanalog.aFiltFS, out pTanalog.aFiltVal);

            AnalogPutCache(pTanalog);

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method searches the cache of TxTxAnalog object looking for a 'hit'
        /// for the key {cEqpt, cTraf}. If a 'hit' is found the method returns the 
        /// matching object otherwise it returns null.
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="tAEqpt"> - TcTxAnalog object, or null.</param>
        /// <returns></returns>
        public static int AnalogGetCache(string cEqpt, string cTraf, out TcTxAnalog tAEqpt)
        {
            int nRet = Constant.FAILURE;

            // Create the key.
            TcTxAnalog key = new TcTxAnalog();
            key.ecode = cEqpt;
            key.trafcode = cTraf;

            tAEqpt = mAnalogCache.Get(key);

            if (tAEqpt != null)
            {
                nRet = Constant.SUCCESS;
            }

            return nRet;
        }

        /// <summary>
        /// This method puts a TcTxAnalog object into a cache.  
        /// </summary>
        /// <param name="t"> - TcTxAnalog object.</param>
        public static void AnalogPutCache(TcTxAnalog t)
        {
            mAnalogCache.Put(t);
        }

        /// <summary>
        /// This method fetches the filter or power spectrum array from the DB ctxfps table.
        /// It returns the arrays through its argument list and the methods integer return 
        /// value is the number of elements in the array.  
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="cType"> - either "F" for filter or "P" for power</param>
        /// <param name="nFilter"> - length of array found</param>
        /// <param name="dFs"> - output array of X-values (frequencies).</param>
        /// <param name="dVal"> - output array of Y-values (spectral value).</param>
        /// <returns></returns>
        public static int GetAdArray(string cEqpt, string cTraf,
                             string cType,          /*	Either F for filter or P for power*/
                             out int nFilter,       /*	length of array found */
                             out double[] dFs,        /*	address of pointer to array */
                             out double[] dVal)       /*	address of pointer to array */
        {
            // 'out' requirement.
            nFilter = 0;
            dFs = null;
            dVal = null;

            float fdbFs;
            float fdbVal;
            int ndbCount;
            int nInd;

            string cSQL;

            cSQL = String.Format("ecode='{0}' and trafcode='{1}' and ctype='{2}'",
                        cEqpt, cTraf, cType);


            ndbCount = Ssutil.DbCountRows("tsip.ctxfps", cSQL);

            if (ndbCount < 1)
            {
                return Error.NODATA;
            }

            SQLLEN nIsNull;
            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            // For ease of syntax, accumulate the retrieved data in a list and convert to
            // array later on.
            List<double> dFsList = new List<double>();
            List<double> dValList = new List<double>();

            /*	Allocate the arrays.  ** NOTE ** These arrays are for the use of the 
            *		supp[ata,dta,doratd] routines, which have been converted from FORTRAN.
            *		Consequently, these arrays are 1 based, not 0 based.  So we add an
            *		extra empty element at 0.			2003.11.05 - GJS - 1083 */
            dFsList.Add(0.0);
            dValList.Add(0.0);

            cSQL = String.Format("select fs, value from tsip.ctxfps where ecode='{0}' and trafcode='{1}' and ctype='{2}' order by fs ",
                              cEqpt, cTraf, cType);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nCtxUtil.GetAdArray(): ERROR: call to SQLExecDirect() failed, returned: " + sqlRet);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            // The select succeeded so now fetch the column data.
            int fetchCount = 0;
            nInd = 1;

            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    if (ODBC.IsNoData(sqlRet))
                    {
                        // No more dat available to fetch so break out of the while-loop.
                        break;
                    }
                    else
                    {
                        // Something bad happened.
                        Log2.e("\nCtxUtil.GetAdArray(): ERROR: call to SQLFetch() failed: " + sqlRet);
                        ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                        Ssutil.DisConn(hConn);
                        return Error.ODBC_FETCH_FAILED;
                    }
                }

                try
                {
                    Ssutil.DbGetFloat(hStmt, 1, "fs", out fdbFs, out nIsNull);
                    Ssutil.DbGetFloat(hStmt, 2, "value", out fdbVal, out nIsNull);
                }
                catch (Exception e)
                {
                    Log2.e("\nCtxUtil.GetAdArray(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                    string str = String.Format("getadarray04: Could not retrieve element of array {0} ({1}) for {2}-{3}-{4}",
                             nInd, e.Message, cEqpt, cTraf, cType);
                    GenUtil.SetErr(str);

                    ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                    Ssutil.DisConn(hConn);
                    return Error.ODBC_GET_FAILED; ;
                }

                // Accumulate the retrieved data.
                dFsList.Add(fdbFs);
                dValList.Add(fdbVal);

                fetchCount++;
                nInd++;
            } // while-loop

            // Check that the correct number of records have been retrieved.
            if (ndbCount != fetchCount)
            {
                Log2.e("\nCtxUtil.GetAdArray(): ERROR: incorrect number of records fetched from DB.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.INVALID_DATA;
            }

            // If we get here, we succeeded; convert the lists to arrays.
            nFilter = ndbCount;
            dFs = dFsList.ToArray();
            dVal = dValList.ToArray();

            // Free ODBC resources.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (ndbCount);
        }

        /// <summary>
        /// This method method requests a populated TcTxDigital object, with 
        /// prescribed search key {cEqpt, cTraf}. The method first searches 
        /// the cache for an element with a matching key; if there is no cache
        /// 'hit' the TcTxDigital object is populated with data fetched from the DB.
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="pTdigital"> - TcTxDigital  object.</param>
        /// <returns></returns>
        public static int GetDigital(string cEqpt, string cTraf, out TcTxDigital pTdigital)
        {
            // 'out' requirement.
            pTdigital = null;

            //	exec sql end declare section;
            SQLLEN nIsNull;

            SQLHDBC hConn = Ssutil.NewConn();
            SQLHANDLE hStmt;
            SQLRETURN sqlRet;

            string cSQL;

            int nRet;

            // Check if it is in the cache; if so, get it and return it.
            if ((nRet = DigitalGetCache(cEqpt, cTraf, out pTdigital)) == Constant.SUCCESS)
            {
                return Constant.SUCCESS;
            }

            // It was not in the cache so retrieve it from the DB.
            cSQL = String.Format("select bandwidth, nf, iffreq, ai70, ai140, thdcrit, sif, irf, nth from tsip.ctxdeqpt where ecode='{0}' and trafcode='{1}' ",
                             cEqpt, cTraf);

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            sqlRet = ODBC.SQLExecDirect(hStmt, cSQL, cSQL.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nCtxUtil.GetDigital(): ERROR: call to SQLExecDirect failed.");
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_EXECDIRECT_FAILED;
            }

            sqlRet = ODBC.SQLFetch(hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
                int rv = Constant.FAILURE;
                if (ODBC.IsNoData(sqlRet))
                {
                    Log2.e("\nCtxUtil.GetDigital(): ERROR: call to SQLFetch() returned SQL_NO_DATA.");
                    rv = ODBC.SQL_NO_DATA;
                }
                else
                {
                    Log2.e("\nCtxUtil.GetDigital(): ERROR: call to SQLFetch() failed, returned: " + sqlRet);
                    rv = Error.ODBC_FETCH_FAILED;
                }
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return rv;
            }

            // The fetch succeeded, now retrieve the column data.
            pTdigital = new TcTxDigital();
            pTdigital.ecode = cEqpt;
            pTdigital.trafcode = cTraf;

            try
            {
                Ssutil.DbGetDouble(hStmt, 1, "bandwidth", out pTdigital.bandwidth, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 2, "nf", out pTdigital.nf, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 3, "iffreq", out pTdigital.iffreq, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 4, "ai70", out pTdigital.ai70, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 5, "ai140", out pTdigital.ai140, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 6, "thdcrit", out pTdigital.thdcrit, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 7, "sif", out pTdigital.sif, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 8, "irf", out pTdigital.irf, out nIsNull);
                Ssutil.DbGetDouble(hStmt, 9, "nth", out pTdigital.nth, out nIsNull);
            }
            catch (Exception e)
            {
                Log2.e("\nCtxUtil.GetDigital(): ERROR: ODBC 'Get' attempt failed: " + e.Message);
                string str = String.Format("getdigital04: Error retrieving field {0} for {1}-{2}", e.Message, cEqpt, cTraf);
                GenUtil.SetErr(str);

                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return Error.ODBC_GET_FAILED;
            }

            GetAdArray(cEqpt, cTraf, "F", out pTdigital.nFilter, out pTdigital.aFiltFS, out pTdigital.aFiltVal);

            GetAdArray(cEqpt, cTraf, "P", out pTdigital.nSpect, out pTdigital.aSpectFS, out pTdigital.aSpectVal);

            DigitalPutCache(pTdigital);

            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            return (Constant.SUCCESS);
        }

        /// <summary>
        /// This method searches the cache of TxTxDigital object looking for a 'hit'
        /// for the key {cEqpt, cTraf}. If a 'hit' is found the method returns the 
        /// matching object otherwise it returns null.
        /// </summary>
        /// <param name="cEqpt"> - equipment code.</param>
        /// <param name="cTraf"> - traffic code.</param>
        /// <param name="tAEqpt"> - TcTxDigital object, or null.</param>
        /// <returns></returns>
        public static int DigitalGetCache(string cEqpt, string cTraf, out TcTxDigital tAEqpt)
        {
            int nRet = Constant.FAILURE;

            // Create the key.
            TcTxDigital key = new TcTxDigital();
            key.ecode = cEqpt;
            key.trafcode = cTraf;

            tAEqpt = mDigitalCache.Get(key);

            if (tAEqpt != null)
            {
                nRet = Constant.SUCCESS;
            }

            return nRet;
        }

        /// <summary>
        /// This method puts a TcTxDigital object into a cache.  
        /// </summary>
        /// <param name="t"> - TcTxDigital object.</param>
        public static void DigitalPutCache(TcTxDigital t)
        {
            mDigitalCache.Put(t);
        }

        /// <summary>
        /// This method retrieves meta-data on the effectiveness of the Analog cache:
        /// the maximum size of the cache, the number of 'slots' currently used, 
        /// the total number of attempted cache reads and the total number of successful
        /// cache 'hits'.
        /// </summary>
        /// <param name="nCalls"> - total number of attempted cache reads</param>
        /// <param name="nHits"> - total number of successful cache 'hits'.</param>
        /// <param name="nSize"> - maximum size (number of 'slots') of the cache.</param>
        /// <param name="nUsed"> - number of slots currently in use.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int GetAnalogCacheInfo(out int nCalls,       /*  Calls to the routine */
                                            out int nHits,        /*  Number found in cache */
                                            out int nSize,   /*  Size of cache.  Max storable */
                                            out int nUsed)     /*  Number of these used */
        {
            mAnalogCache.GetStats(out nCalls, out nHits, out nSize, out nUsed);
            return 0;
        }

        /// <summary>
        /// This method retrieves meta-data on the effectiveness of the Analog cache:
        /// the maximum size of the cache, the number of 'slots' currently used, 
        /// the total number of attempted cache reads and the total number of successful
        /// cache 'hits'.
        /// </summary>
        /// <param name="nCalls"> - total number of attempted cache reads</param>
        /// <param name="nHits"> - total number of successful cache 'hits'.</param>
        /// <param name="nSize"> - maximum size (number of 'slots') of the cache.</param>
        /// <param name="nUsed"> - number of slots currently in use.</param>
        /// <returns> - always returns Constant.SUCCESS</returns>
        public static int GetDigitalCacheInfo(out int nCalls,       /*  Calls to the routine */
                                            out int nHits,        /*  Number found in cache */
                                            out int nSize,   /*  Size of cache.  Max storable */
                                            out int nUsed)     /*  Number of these used */
        {
            mDigitalCache.GetStats(out nCalls, out nHits, out nSize, out nUsed);

            return 0;
        }


    }
}

```
