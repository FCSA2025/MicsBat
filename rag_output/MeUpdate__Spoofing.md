# Documented File: Spoofing.cs
**Repository Path:** `MeUpdate\Spoofing.cs`
**Primary Layer:** `MeUpdate`
**Namespace:** `MeUpdate`

## Source Code Representation
```csharp
﻿using _Utillib;
using _NewLib;
using _DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeUpdate
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
    /// This class provides methods that support the 'spoofing' of ES-specific MDB tables
    /// by reading/writing to tables in the user's own schema, e.g. the MDB table
    /// main.me_site is 'spoofed' by the user's table hulme.me_site (say).
    /// </summary>
    class Spoofing
    {
        private static string[] ME_SITES = new string[1]
        {
            "'CWN-2','LAKE COWICHAN-2','BC','TGLB','17583433','48-50-34.33','N','44665985','124-04-19.85','W','260','B','4',NULL,'5',NULL,'CE','CC','2008.05.03','08:19','fmda'"
        };

        private static string[] ME_ANTES = new string[1]
        {
            "'CWN-2','=TGLBCW04','6A','3C','CCIR5357','CCIR5357','34.5','35','8','2','2','3.999073','3.999073','-64800000','180','E','243.014','13.18369','-180','0.5','200','200','200','200',NULL,'INTELSAT 701','5',NULL,'IS','4','GS','2008.05.03','08:19','fmda'"
        };

        private static string[] ME_AZIMS = new string[13]
        {
            "'CWN-2','=TGLBCW04','5','12.5','2',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','20','18.8','1.55',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','50','18.5','1.61',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','80','10','3.18',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','109.6','1.8','7.88',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','142.5','2.2','8.33',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','169.8','5.1','8.79',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','203.5','4.5','5.15',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','234','3','10.39',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','264','2.2','12.72',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','290','1.9','33.33',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','321.5','13.2','2.67',NULL,'1997.11.28','10:11','fmda2'",
            "'CWN-2','=TGLBCW04','355','6.7','2.18',NULL,'1997.11.28','10:11','fmda2'"
        };

        private static string[] ME_CHANS = new string[6]
        {
            "'CWN-2','=TGLBCW04','01','6320000','R','32','25.4','38.6','MDU45','DIS45M','5','F',NULL,NULL,NULL,NULL,NULL,NULL,'-142','-130','-130',NULL,NULL,'DIGVID',NULL,'2008.05.03','08:19','fmda'",
            "'CWN-2','=TGLBCW04','02','6387250','L','32','18.6','32.4','SDM8448','DIS8MB','5','D',NULL,NULL,NULL,NULL,NULL,NULL,'-149','-137','-137',NULL,NULL,'DIGVID',NULL,'2008.05.03','08:19','fmda'",
            "'CWN-2','=TGLBCW04','03','6395125','L','32','18.6','32.4','SDM8448','DIS8MB','5','D',NULL,NULL,NULL,NULL,NULL,NULL,'-149','-137','-137',NULL,NULL,'DIGVID',NULL,'2008.05.03','08:19','fmda'",
            "'CWN-2','=TGLBCW04','04','6403000','L','26','18.6','32.4','SDM8448','DIS8MB','5','D',NULL,NULL,NULL,NULL,NULL,NULL,'-149','-137','-137',NULL,NULL,'DIGVID',NULL,'2008.05.03','08:19','fmda'",
            "'CWN-2','=TGLBCW04','05','6410875','L','32','18.6','32.4','SDM8448','DIS8MB','5','D',NULL,NULL,NULL,NULL,NULL,NULL,'-149','-137','-137',NULL,NULL,'DIGVID',NULL,'2008.05.03','08:19','fmda'",
            "'CWN-2','=TGLBCW04','06','6418750','L','32','18.6','32.4','SDM8448','DIS8MB','5','D',NULL,NULL,NULL,NULL,NULL,NULL,'-149','-137','-137',NULL,NULL,'DIGVID',NULL,'2008.05.03','08:19','fmda'"
        };

        /// <summary>
        /// This method drops then recreates user ES MDB 'spoofing' tables and (optionally)
        /// populates them with a small set of data that can be used for testing.
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[me_ante]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[me_azim]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[me_chan]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[me_site]", schema));

            DoQuery(hStmt, String.Format(MeAnte.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(MeAzim.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(MeChan.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(MeSite.CREATE_TABLE, schema));

            DoQuery(hStmt, String.Format(AuditTrail.CREATE_TABLE, schema));

            if (isWriteSpoofTestData)
            {
                foreach (string site in ME_SITES)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[me_site] VALUES ({1})", schema, site));
                }

                foreach (string ante in ME_ANTES)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[me_ante] VALUES ({1})", schema, ante));
                }

                foreach (string azim in ME_AZIMS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[me_azim] VALUES ({1})", schema, azim));
                }

                foreach (string chan in ME_CHANS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[me_chan] VALUES ({1})", schema, chan));
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

            //...Log2.v("\n");
            //...Log2.v("\nSpoofing.ExecQuery(): query  = " + query);
            //...Log2.v("\nSpoofing.ExecQuery(): sqlRet = " + sqlRet);
            //...Log2.v("\n");

            return sqlRet;
        }


    }
}

```
