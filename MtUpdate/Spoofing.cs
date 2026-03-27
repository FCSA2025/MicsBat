using _Utillib;
using _NewLib;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MtUpdate
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
    using SQLUINTEGER = UInt32;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of TS-specific MDB tables
    /// by reading/writing to tables in the user's own schema, e.g. the MDB table
    /// main.mt_site is 'spoofed' by the user's table hulme.mt_site (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table mt_site.
        private static string[] MT_SITES = new string[]
        {
            "'VEL415','KANATA SOUTH','ON','RCTL','16304066','45-17-20.66','N','27314134','075-52-21.34','W','102.1','5',NULL,'C1922','041090001349',NULL,NULL,NULL,'FT',NULL,NULL,'33024','0','0','0','0','0','0','0','2017.10.04','18:33','fwmda'",
            "'CHG946','HWY 417 & MARCH','ON','RCTL','16316496','45-19-24.96','N','27321247','075-53-32.47','W','94.8','5',NULL,'C5043','041090001349',NULL,NULL,NULL,'FT',NULL,NULL,'256','0','0','0','0','0','0','0','2017.10.04','18:33','fwmda'"
        };

        // Rows of test data to be inserted into spoof table mt_ante.
        private static string[] MT_ANTES = new string[]
        {
            "'VEL415','CHG946','18B','12','TR','VHLPX4-181GR','39','338.0176','-0.1980815','4.138518','N',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'2018.07.05','16:51','fwmda','010032315'",
            "'VEL415','CIO595','18B','12','TR','HSX4-180','30','277.3056','0.003058311','3.484038','N',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'2017.10.04','18:32','fwmda','010032985'",
            "'VEL415','VEL347','11A','12','TR','UHX4-107','28.5','259.1573','0.2921112','10.35507','N',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'2017.10.04','18:32','fwmda','010032231'",
            "'VEL415','XJV283','11A','12','TR','UHX4-107','33','336.275','-0.03087193','6.08842','N',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'2017.10.04','18:32','fwmda','010032247'",
            "'CHG946','VEL415','18B','12','TR','HSX4-180','33','158.0036','0.170185','4.138518','N',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'2018.07.05','16:51','fwmda','010032315'"
        };

        // Rows of test data to be inserted into spoof table mt_chan.
        private static string[] MT_CHANS = new string[]
        {
            "'VEL415','CHG946','18B',NULL,NULL,NULL,'1003','19480000','B','12',NULL,'ML18M131',NULL,'22','11','2.3',NULL,'D18131','131M','5','17920000','B','12',NULL,NULL,'ML18M131',NULL,'2.3',NULL,NULL,'-23.19239',NULL,NULL,'D18131',NULL,NULL,'131M','5',NULL,NULL,NULL,NULL,'NP','XABC','XABC',NULL,'J','J','2018.07.05','16:51','fwmda'",
            "'VEL415','CIO595','18B',NULL,NULL,NULL,'1002','19400000','B','12',NULL,'ML18M131',NULL,'22','5','7.3',NULL,'D18131','131M','5','17840000','B','12',NULL,NULL,'ML18M131',NULL,'2.3',NULL,NULL,'-26.51612',NULL,NULL,'D18131',NULL,NULL,'131M','5',NULL,NULL,NULL,NULL,NULL,'NP','XABC',NULL,'J','J','2012.09.07','11:00','fwoad'",
            "'VEL415','VEL347','11A',NULL,NULL,NULL,'1003','10735000','H','12',NULL,'ML11M131',NULL,'24','6','1.9',NULL,'D11131','ATPC','5','11225000','H','12',NULL,NULL,'ML11M131',NULL,'1.9',NULL,NULL,'-29.32418',NULL,NULL,'D11131',NULL,NULL,'ATPC','5',NULL,NULL,NULL,NULL,NULL,NULL,'NP',NULL,'J','J','2012.09.05','11:59','fwoad'",
            "'VEL415','VEL347','11A',NULL,NULL,NULL,'1004','10895000','V','12',NULL,'ML11M131',NULL,'24','6','1.9',NULL,'D11131','ATPC','5','11385000','V','12',NULL,NULL,'ML11M131',NULL,'1.9',NULL,NULL,'-28.85092',NULL,NULL,'D11131',NULL,NULL,'ATPC','5',NULL,NULL,NULL,NULL,NULL,NULL,'NP',NULL,'J','J','2012.09.05','11:59','fwoad'",
            "'VEL415','XJV283','11A',NULL,NULL,NULL,'1002','10905000','H','12',NULL,'TP11GOC3',NULL,'21','0','9',NULL,'DT11C3','OC3','5','11395000','H','12',NULL,NULL,'TP11GOC3',NULL,'3',NULL,NULL,'-39.57518',NULL,NULL,'DT11C3',NULL,NULL,'OC3','5',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,'J','J','2008.05.03','10:26','fmda'",
            "'CHG946','VEL415','18B',NULL,NULL,NULL,'1003','17920000','B','12',NULL,'ML18M131',NULL,'22','11','2.3',NULL,'D18131','131M','5','19480000','B','12',NULL,NULL,'ML18M131',NULL,'2.3',NULL,NULL,'-24.06524',NULL,NULL,'D18131',NULL,NULL,'131M','5',NULL,NULL,NULL,NULL,'NP','XABC','XABC',NULL,'J','J','2018.07.05','16:51','fwmda'"
        };

        // Rows of test data to be inserted into spoof table sd_town.
        private static string[] SD_TOWN = new string[]
        {
            "'VEL415','HULME','PR','7.3','1','N','Y','9.9','1983','2007.05.14','2016.06.04','1995.10.13','14:43'",
            "'CHG946','MURPHY','SN','4.5','2','Y','N','1.1','2019','2005.09.09','2017.06.06','2008.07.07','23:59'"
         };

        // Rows of test data to be inserted into table sd_rout.
        private static string[] SD_ROUT = new string[]
        {
            "'ACC','102','ON','VEL415','HELEN MINES - MAGPIE       23GHz DIGITAL.','1992.12.09','10:54'",
            "'BELL','53','ON','CHG946','LAKE OF TWO MTS.  1.5GHZ--SRS EXCHANGE BASE','2007.04.10','10:30'"
        };

        /// <summary>
        /// This method drops then recreates user TS MDB 'spoofing' tables and (optionally)
        /// populates them with a small set of row data that can be used for testing.
        /// </summary>
        /// <param name="schema"> - the user's SQL Server schema (e.g. hulme).</param>
        /// <param name="isWriteSpoofTestData"> - boolean that prescribes whether the spoofing tables should be populated with test data, or not.</param>
        /// <returns></returns>
        public static int CreateSpoofMdbTables(string schema, bool isWriteSpoofTestData)
        {
            //...Log2.v("\nSpoofing.CreateSpoofMdbTables(): ");

            // First of all, protect against an improper call in which schema
            // evaluates to 'main'.
            string str = schema.Trim().ToLower();
            if (str.EndsWith("main"))
            {
                str = "\r\nSpoofing.CreateSpoofMdbTables(): ERROR: attempt to create spoofing tables for schema: 'main'\r\n";
                Log2.e(str);
                Console.Write(str);
                Application.ExitQuietly(666);
            }

            SQLRETURN sqlRet = 0;

            SQLHDBC hConn = Ssutil.NewConn();

            SQLHANDLE hStmt;

            sqlRet = ODBC.SQLAllocHandle(ODBC.SQL_HANDLE_STMT, hConn, out hStmt);

            DoQuery(hStmt, "SET ANSI_NULLS ON");

            DoQuery(hStmt, "SET QUOTED_IDENTIFIER ON");

            DoQuery(hStmt, "SET ANSI_PADDING ON");

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[mt_ante]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[mt_chan]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[mt_site]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_town]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_rout]", schema));

            DoQuery(hStmt, String.Format(MtAnte.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(MtChan.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(MtSite.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(SuTown.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(SuRout.CREATE_TABLE, schema));

            string dummy;
            if (Ssutil.DbTableExists(schema + ".audit_trail", out dummy))
            {
                //...Log2.v("\nSpoofing.CreateSpoofMdbTables(): audit trail table already exists: " + schema + ".audit_trail");
            }
            else
            {
                //...Log2.v("\nSpoofing.CreateSpoofMdbTables(): creating audit trail table: " + schema + ".audit_trail");
                DoQuery(hStmt, String.Format(AuditTrail.CREATE_TABLE, schema));
            }

            if (isWriteSpoofTestData)
            {
                foreach (string site in MT_SITES)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[mt_site] VALUES ({1})", schema, site));
                }

                foreach (string ante in MT_ANTES)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[mt_ante] VALUES ({1})", schema, ante));
                }

                foreach (string chan in MT_CHANS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[mt_chan] VALUES ({1})", schema, chan));
                }

                foreach (string town in SD_TOWN)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_town] VALUES ({1})", schema, town));
                }

                foreach (string rout in SD_ROUT)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_rout] VALUES ({1})", schema, rout));
                }
            }

            DoQuery(hStmt, "SET ANSI_PADDING OFF");

            Ssutil.DisConn(hConn);
            return sqlRet;
        }

        /// <summary>
        /// This method encapsulates the submission of a query
        /// to the ODBC / SQL Server.
        /// </summary>
        /// <param name="hStmt"> - an open ODBC statement handle</param>
        /// <param name="query"> - SQL query to be submitted.</param>
        /// <returns></returns>
        private static int DoQuery(SQLHANDLE hStmt, string query)
        {
            SQLRETURN sqlRet = 0;

            sqlRet = ODBC.SQLExecDirect(hStmt, query, query.Length);

            //...Log2.v("\n" + ODBC.GetDiagnostics(hStmt, query));

            return sqlRet;
        }


    }
}
