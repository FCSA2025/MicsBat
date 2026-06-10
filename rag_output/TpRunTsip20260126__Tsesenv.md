# Documented File: Tsesenv.cs
**Repository Path:** `TpRunTsip20260126\Tsesenv.cs`
**Primary Layer:** `TpRunTsip20260126`
**Namespace:** `TpRunTsip`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Configuration;
using _NewLib;
using _Utillib;
using System.IO;

namespace TpRunTsip
{
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
    /// Provides methods used to generate the Station Summary Report (.STATSUM) 
    /// for the case of an environmental site being a Ts-Es interferer.
    /// </summary>
    public class Tsesenv
    {
        private static int mglbCount = 0;
        private static string mPrevInt = "";

        /// <summary>
        /// This class extends the PrintLine base class to provide page header text
        /// specific to the report type.
        /// </summary>
        private class PrintLinerpenv : PrintLine
        {
            /// <summary>
            /// This method overrides that of the PrintLine base class to provide
            /// page header text specific to the report type.
            /// </summary>
            /// <param name=""></param>
            public override void PageHeader()
            {
                PageBefore();
                TsEsEnvPage(this);
            }
        }

        /// <summary>
        /// This method retrieves TSIP results data from the DB and produces
        /// the bulk of the text for the STATSUM report for Ts-Es interference.
        /// </summary>
        /// <param name="tw">- the TextWriter object assigned to the .STATSUM report.</param>
        /// <param name="cTableName"> - the unique ID substring of the Te DB table names.</param>
        /// <returns></returns>
        public static int TsEsEnv(TextWriter tw, string cTableName)
        {
            //...Log2.v("\nTsesenv.TsEsEnv(): Entry: cTable = " + cTableName);

            int nRet;
            string interferer;
            string call1;
            string name;
            string oper;
            int latit;
            int longit;
            double grnd;
            SQLLEN nullInd;
            int nCount;

            PrintLinerpenv pl = new PrintLinerpenv();
            pl.OutFile(tw);

            TsEsEnvPage(pl);

            //	Go through the table, this is the only place it is used, so a separate 
            //	access routine is not generated.
            SQLHDBC hConn = Ssutil.NewConn();
            SQLRETURN sqlRet = 0;
            SQLHANDLE hStmt;

            string cBuff;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);
            if (!ODBC.IsOK(sqlRet))
            {
            }

            cBuff = String.Format("select tmpinter, tmpcall1, tmpname, tmpoper, tmplatit, tmplongit, tmpgrnd from {0} order by tmpinter desc, tmpcall1",
                              cTableName);

            sqlRet = ODBC.SQLExecDirect(hStmt, cBuff, cBuff.Length);

            const int INTERFERER_SZ = 2;
            const int CALL1_SZ = 10;
            const int NAME_SZ = 33;
            const int OPER_SZ = 7;

            nCount = 0;
            while (true)
            {
                sqlRet = ODBC.SQLFetch(hStmt);
                if (!ODBC.IsOK(sqlRet))
                {
                    break;
                }

                // Setup automatic column indexing.
                Ssutil.DbStartGets();

                Ssutil.DbGetString(hStmt, 0, "interferer", out interferer, INTERFERER_SZ, out nullInd);
                Ssutil.DbGetString(hStmt, 0, "call1", out call1, CALL1_SZ, out nullInd);
                Ssutil.DbGetString(hStmt, 0, "name", out name, NAME_SZ, out nullInd);
                Ssutil.DbGetString(hStmt, 0, "oper", out oper, OPER_SZ, out nullInd);
                Ssutil.DbGetInt(hStmt, 0, "latit", out latit, out nullInd);
                Ssutil.DbGetInt(hStmt, 0, "longit", out longit, out nullInd);
                Ssutil.DbGetDouble(hStmt, 0, "grnd", out grnd, out nullInd);

                nCount++;
                nRet = TsEsEnvDet(pl, interferer, call1, name, oper, latit, longit, grnd);
            }

            pl.Output();
            pl.IntAt(0, "{0} Records.", nCount);
            pl.Output();

            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            sqlRet = (SQLRETURN)Ssutil.DisConn(hConn);

            //...Log2.v("\nTsesenv.TsEsEnv(): Exit");
            return 0;
        }

        /// <summary>
        /// This method produces text used for a Ts-Es page header.  
        /// </summary>
        /// <param name="pl"> - PrintLine object with header override.</param>
        /// <returns></returns>
        private static int TsEsEnvPage(PrintLinerpenv pl)
        {
            string cCurrDate;
            string cCurrTime;

            GenUtil.UtGetDateTime(out cCurrDate, out cCurrTime);

            pl.LeftAt(0, "FREQUENCY COORDINATION SYSTEM ASSOCIATION");
            pl.LeftAt(65, "DATE: {0}", cCurrDate);
            pl.Output();
            pl.LeftAt(0, "MASTER DATA BASE -- Interference Study Report - ");
            pl.LeftAt(72, "Page:");
            pl.IntAt(78, "{0,3:D}", pl.PageNumber);
            pl.Output();
            pl.LeftAt(0, "                    Environment as Interferer - Station Summary");
            pl.Output();
            pl.LeftAt(0, "Project Code [{0}]", Info.ProjectCode);
            pl.Output();
            pl.Output();
            pl.LeftAt(0, "Stations Passed To Analysis");
            pl.Output();
            pl.LeftAt(0, "Call Sign");
            pl.LeftAt(10, "Station Name");
            pl.LeftAt(43, "Oper");
            pl.LeftAt(50, "Latitude");
            pl.LeftAt(63, "Longitude");
            pl.LeftAt(77, "Grnd");
            pl.Output();

            pl.LeftAt(78, "(m)");
            pl.Output();

            pl.LeftAt(0, "---------");
            pl.LeftAt(-1, " --------------------------------");
            pl.LeftAt(-1, " ------");
            pl.LeftAt(-1, " -----------");
            pl.LeftAt(-1, "  ------------");
            pl.LeftAt(-1, " -----");
            pl.Output();

            return 0;
        }

        /// <summary>
        /// This method produces text for the 'detail' line results data for a Ts-Es interference case.  
        /// </summary>
        /// <param name="pl"> - PrintLine object with header override.</param>
        /// <param name="interferer"> - string indicating interferer ("I"), or not.</param>
        /// <param name="call1"> - call sign of the site.</param>
        /// <param name="name"> -the operator's assigned name for the site.</param>
        /// <param name="oper"> - the operator code.</param>
        /// <param name="latit"> - site's latitude.</param>
        /// <param name="longit"> - site's longitude.</param>
        /// <param name="grnd"> - ground height at site.</param>
        private static int TsEsEnvDet(PrintLinerpenv pl,
                             string interferer,
                             string call1,
                             string name,
                             string oper,
                             int latit,
                             int longit,
                             double grnd)
        {
            string cLat;
            string cLatSense;
            string cLong;
            string cLongSense;

            if (!interferer.Equals(mPrevInt))
            {
                mPrevInt = interferer;
                pl.Output();
                pl.Output();
                if (!interferer.Equals("I"))
                {
                    pl.LeftAt(10, "=== FROM PROPOSED FILE AS VICTIM ===");
                }
                else
                {
                    pl.LeftAt(10, "=== FROM ENVIRONMENT FILE AS INTERFERER ===");
                }
                pl.Output();
                pl.Output();
            }

            mglbCount++;

            GenUtil.UtLatConvStr(latit, out cLat, out cLatSense);
            GenUtil.UtLongConvStr(longit, out cLong, out cLongSense);

            pl.LeftAt(0, call1);
            pl.LeftAt(10, name);
            pl.LeftAt(43, oper);
            pl.LeftAt(50, cLat);
            pl.LeftAt(-1, "  ");
            pl.LeftAt(-1, cLong);
            pl.DoubleAt(-1, " {0,5:F0}", grnd);
            pl.Output();

            return 0;
        }









    }
}

```
