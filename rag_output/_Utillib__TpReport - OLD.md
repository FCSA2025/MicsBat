# Documented File: TpReport - OLD.cs
**Repository Path:** `_Utillib\TpReport - OLD.cs`
**Primary Layer:** `_Utillib`
**Namespace:** `_Utillib`

## Source Code Representation
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _Utillib
{
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

    public class TpReport
    {

        /// <summary>
        /// This code creates temporary tables that hold the station summary data.  
        /// </summary>
        /// <param name="proposed"></param>
        /// <param name="siteName"></param>
        /// <param name="cStatTab"></param>
        /// <returns></returns>
        public static int CreateTTStatRep(string proposed, string siteName, out string cStatTab)
        {
            string str = String.Format("\nTpReport.CreateTTStatRep(): Entry: proposed = {0},   siteName = {1}", proposed, siteName);
            //...Log2.v(str);

            string sqlstmt;

            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            /*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            + Here we have code to implement a change to the station summary report works
            + This was introduced in modification 194.  FCSA would like all TSTS STATSUM
            + reports to provide information on interfering call1's and victim call1's.
            +
            + So take the case where A-B (proposed) is analyzed with X-Y (environment).
            + NOTE - this does not mean AB interfers with XY
            + The STATSUM report will include A on the proposed side and X on the
            + environment side.
            +
            ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
            GenUtil.MkUnique(out cStatTab, "tsip_stat", "", "");

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("create table {0} (  tmpinter 	varchar(1),  tmpcall1 	varchar(10),  tmpname 	varchar(32),  tmpoper 	varchar(6), 	tmplatit 	int, 	tmplongit int, 	tmpgrnd 	float)",
                            cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: A. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep01: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -1;
            }

            /*++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            + We first populate the temporary table with victim call1 information.
            +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

            sqlstmt = String.Format("insert into {0} (tmpinter, tmpcall1) select distinct 'P', viccall1 from {1} where interferer = 'E'", cStatTab, siteName);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: B. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep02: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -2;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("insert into {0} (tmpinter, tmpcall1) select distinct 'E', viccall1 from {1} where interferer = 'P'", cStatTab, siteName);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: C. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep03: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -3;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("insert into {0} (tmpinter, tmpcall1) select distinct interferer, intcall1 from {1} where not exists (select * from {2} where intcall1 = tmpcall1)",
                            cStatTab, siteName, cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: D. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep04: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -4;
            }

            /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            + Now we can update the tsip_stat_rep table to fill in all information required
            + for the STATSUM report.  For the proposed data this is not a problem.
            + We know that proposed data must be in the TSIP tables
            +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

            sqlstmt = String.Format("create view x_{0} (inter, intcall, intname1, intoper, intlatit, intlongit, intgrnd) as Select interferer, intcall1, min(intname1), min(intoper), min(intlatit), min(intlongit), min(intgrnd) From {1} Group By interferer, intcall1",
                    cStatTab, siteName);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: E. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep05: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -5;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("create view y_{0} (inter, viccall, vicname1, vicoper, viclatit, viclongit, vicgrnd) as Select interferer, viccall1, min(vicname1), min(vicoper), min(viclatit), min(viclongit), min(vicgrnd) From {1} Group By interferer, viccall1",
                    cStatTab, siteName);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: F. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep06: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -6;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("update {0} set tmpname = intname1, tmpoper = intoper, tmplatit = intlatit, tmplongit = intlongit, tmpgrnd = intgrnd from x_{1} where inter = 'P' and tmpcall1 = intcall",
                   cStatTab, cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: G. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep07: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -7;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("update {0} set tmpname = vicname1, tmpoper = vicoper, tmplatit = viclatit, tmplongit = viclongit, tmpgrnd = vicgrnd from y_{1} where inter = 'P' and tmpcall1 = viccall",
                    cStatTab, cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: H. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep08: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -8;
            }

            /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            + For the environment data two steps are required.  Environment data can be
            + 1 or 2 places, the TSIP tables or the environment tables (mt_site).  We
            + cannot be sure where this data is going to be found but we know
            + that if it is in both places the data from the TSIP tables must take
            + precedence.  So we update the tsip_stat_rep table from the TSIP tables
            + and fill in as much data as we can THEN we update from the environment
            + table for any record that has not been filled out
            +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

            sqlstmt = String.Format("update {0} set tmpname = vicname1, tmpoper = vicoper, tmplatit = viclatit, tmplongit = viclongit, tmpgrnd = vicgrnd from y_{1} where inter = 'E' and tmpcall1 = viccall",
                    cStatTab, cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: I. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep09: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -9;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("update {0} set tmpname = name, tmpoper = oper, tmplatit = latit, tmplongit = longit, tmpgrnd = grnd from main.mt_site where tmpinter = 'E' and tmpcall1 = call1 and tmpname is null",
                            cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet) && !ODBC.IsNoData(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: J. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep10: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -10;
            }

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("DROP view x_{0}, y_{1}", cStatTab, cStatTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);
            if (!ODBC.IsOK(sqlRet))
            {
                Log2.e("\nTpReport.CreateTTStatRep(): ERROR: K. Could not create temporary table: " + cStatTab);
                Ssutil.DbGetDiagStmt(hStmt, "createTTStatRep11: Could not create temporary table: " + cStatTab);
                ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
                Ssutil.DisConn(hConn);
                return -11;
            }

            // Release resources and disconnect.
            ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nTpReport.CreateTTStatRep(): Exit");
            return 0;
        }

        /// <summary>
        /// This code creates temporary tables that hold the station summary data.  
        /// </summary>
        /// <param name="proposed"></param>
        /// <param name="siteName"></param>
        /// <param name="anteName"></param>
        /// <param name="cPropTab"></param>
        /// <param name="cEnvTab"></param>
        public static void CreateETStatRep(string proposed,
                                         string siteName,
                                         string anteName,
                                         out string cPropTab,
                                         out string cEnvTab)
        {
            //...Log2.v("\nTpReport.CreateETStatRep(): Entry");

            SQLHANDLE hStmt;
            SQLRETURN sqlRet;
            SQLHDBC hConn = Ssutil.NewConn();

            string cSiteName;
            string sqlstmt;

            cSiteName = siteName;      /*  Copy the site name for ingres */
                                       /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                                       + Here we have code to create temporary table for the STATSUM report to
                                       + report from.  This methodality was introduced in task 363
                                       + We use 2 tables tsip_stat_propint and tsip_stat_envint.
                                       +	(Task 1104 - GJS - 2000 08) We have to make these tables with unique names
                                       +	since more than one tsip job can be running at once for the same user.
                                       +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
            GenUtil.MkUnique(out cPropTab, "tsip_prop", "", "");
            GenUtil.MkUnique(out cEnvTab, "tsip_env", "", "");

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            Ssutil.DropTable(cPropTab);
            Ssutil.DropTable(cEnvTab);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("create table {0} ( tmpinter varchar(1),  tmplocat varchar(11),  tmpcall1 varchar(10),  tmpname varchar(32), 	tmpoper varchar(6), 	tmplatit int,  tmplongit int,  tmpgrnd real)",
                            cPropTab);
            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            /*	Ensure that this is temporary by setting the saveto date. */
            //	Don't know how to do this in SQL Server yet.
            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("create table {0} ( 	tmpinter 	varchar(1), 	tmplocat 	varchar(11), 	tmpcall1 	varchar(10), 	tmpname 	varchar(32), 	tmpoper 	varchar(6), 	tmplatit 	int, 	tmplongit int, 	tmpgrnd 	real)",
                            cEnvTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            /*  Create two views that we can use to uniquely identify the
                earthstations and the terrestrial stations for the updates.  All we
                use them for is the lat/longs etc, so these views are fine.

                      GJS - 2000 07 - Task 1103; They are not fine actually.
                      When a user runs two TSIP runs at once, they can cause problems with
                      locking.  To this end, we use the randomized name of the prop tab to
                      disambiguate them. */
            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("DROP view x_{0}", cPropTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("create view x_{0} (earthlocation,earthname, earthoper, earthlatit, earthlongit, earthgrnd ) as select earthlocation, min(earthname) , min(earthoper) , min(earthlatit), min(earthlongit), min(earthgrnd) from {1} group by earthlocation", cPropTab, siteName);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("DROP view y_{0}", cPropTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            sqlstmt = String.Format("create view y_{0} (terrcall1,terrname1, terroper, terrlatit, terrlongit, terrgrnd ) as select terrcall1, min(terrname1) , min(terroper) , min(terrlatit), min(terrlongit), min(terrgrnd) from {1} group by terrcall1", cPropTab, siteName);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            + We populate the first temporary table with proposed call1 information.
            +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

            if (proposed[0] == 'T')
            {
                sqlstmt = String.Format("insert into {0}  (tmpinter, tmplocat, tmpcall1) select distinct  'I', terrcall1, terrcall1  from {1} where interferer = 'T'",
                                cPropTab, anteName);

                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("insert into {0} (tmpinter, tmplocat, tmpcall1) select distinct  'V', earthlocation, earthcall1  from {1} where interferer = 'T'",
                                cPropTab, anteName);

                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("insert into {0}  (tmpinter, tmplocat, tmpcall1) select distinct  'I', earthlocation, earthcall1 from {1} where interferer = 'E'",
                                                cEnvTab, anteName);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("insert into {0}  (tmpinter, tmplocat, tmpcall1) select distinct  'V', terrcall1, terrcall1 from {1} where interferer = 'E'",
                                cEnvTab, anteName);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                + Now we can update the tsip_stat_* tables to fill in all information required
                + for the STATSUM report.  For the proposed data this is not a problem.
                + We know that proposed data must be in the TSIP tables
                +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

                sqlstmt = String.Format("update {0} set tmpname = terrname1, tmpoper = terroper, tmplatit = terrlatit, tmplongit = terrlongit, tmpgrnd = terrgrnd from y_{1} where tmpinter = 'I' and tmpcall1 = terrcall1",
                                cPropTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = terrname1, tmpoper = terroper, tmplatit = terrlatit, tmplongit = terrlongit, tmpgrnd = terrgrnd from y_{1} where tmpinter = 'V' and tmpcall1 = terrcall1",
                                cEnvTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                + For the environment data two steps are required.  Environment data can be
                + 1 or 2 places, the TSIP tables or the environment tables (mt_site).  We
                + cannot be sure where this data is going to be found but we know
                + that if it is in both places the data from the TSIP tables must take
                + precedence.  So we update the tsip_stat_rep table from the TSIP tables
                + and fill in as much data as we can THEN we update from the environment
                + table for any record that has not been filled out
                +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/
                sqlstmt = String.Format("update {0} set tmpname = earthname, tmpoper = earthoper, tmplatit = earthlatit, tmplongit = earthlongit, tmpgrnd = earthgrnd from x_{1} where tmpinter = 'V' and tmplocat=earthlocation",
                                cPropTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = name, tmpoper = oper, tmplatit = latit, tmplongit = longit, tmpgrnd = grnd from main.me_site where tmpinter = 'V' and tmplocat = location and tmpname is null",
                                cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = earthname, tmpoper = earthoper, tmplatit = earthlatit, tmplongit = earthlongit, tmpgrnd = earthgrnd from x_{1} where tmpinter = 'I' and tmplocat=earthlocation",
                                cEnvTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = name, tmpoper = oper, tmplatit = latit, tmplongit = longit, tmpgrnd = grnd from main.me_site where tmpinter = 'I' and tmplocat = location and tmpname is null",
                                cEnvTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            }
            else
            {
                sqlstmt = String.Format("insert into {0} (tmpinter, tmplocat, tmpcall1) select distinct 'I', earthlocation, earthcall1 from {1} where interferer = 'E'",
                                cPropTab, anteName);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("insert into {0} (tmpinter, tmplocat, tmpcall1) select distinct 'V', terrcall1, terrcall1 from {1} where interferer = 'E'",
                                cPropTab, anteName);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("insert into {0} (tmpinter, tmplocat, tmpcall1) select distinct 'I', terrcall1, terrcall1 from {1} where interferer = 'T'",
                                cEnvTab, anteName);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("insert into {0} (tmpinter, tmplocat, tmpcall1) select distinct 'V', earthlocation, earthcall1 from {1} where interferer = 'T'",
                                cEnvTab, anteName);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                /*+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                + Now we can update the tsip_stat_* tables to fill in all information required
                + for the STATSUM report.  For the proposed data this is not a problem.
                + We know that proposed data must be in the TSIP tables
                +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++*/

                sqlstmt = String.Format("update {0} set tmpname = earthname,     tmpoper = earthoper,     tmplatit = earthlatit,     tmplongit = earthlongit,     tmpgrnd = earthgrnd from x_{1} where tmpinter = 'I' and tmplocat=earthlocation",
                                    cPropTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = earthname, tmpoper = earthoper, tmplatit = earthlatit, tmplongit = earthlongit, tmpgrnd = earthgrnd from x_{1} where tmpinter = 'V' and tmplocat=earthlocation",
                                cEnvTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = terrname1, tmpoper = terroper, tmplatit = terrlatit, tmplongit = terrlongit, tmpgrnd = terrgrnd from y_{1} where tmpinter = 'V' and tmpcall1 = terrcall1",
                                cPropTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = name, tmpoper = oper, tmplatit = latit, tmplongit = longit, tmpgrnd = grnd from main.mt_site where tmpinter = 'V' and tmpcall1 = call1 and tmpname is null",
                                cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = terrname1, tmpoper = terroper, tmplatit = terrlatit, tmplongit = terrlongit, tmpgrnd = terrgrnd from y_{1} where tmpinter = 'I' and tmpcall1 = terrcall1",
                                cEnvTab, cPropTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                sqlstmt = String.Format("update {0} set tmpname = name, tmpoper = oper, tmplatit = latit, tmplongit = longit, tmpgrnd = grnd from main.mt_site where tmpinter = 'I' and tmpcall1 = call1 and tmpname is null",
                                cEnvTab);
                //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

                sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            }
            sqlstmt = String.Format("Drop view x_{0}, y_{1}", cPropTab, cPropTab);

            //...Log2.v("\nTpReport.CreateTTStatRep(): SQLExecDirect():\n" + sqlstmt + "\n");

            sqlRet = ODBC.SQLExecDirect(hStmt, sqlstmt, sqlstmt.Length);

            // Release resources and disconnect.
            sqlRet = ODBC.SQLFreeHandle(ODBC.SQL_HANDLE_STMT, hStmt);
            Ssutil.DisConn(hConn);

            //...Log2.v("\nTpReport.CreateETStatRep(): Exit");
        }





    }
}

```
