# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateAnte\Spoofing.cs`
**Primary Layer:** `SdUpdateAnte`
**Namespace:** `SdUpdateAnte`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateAnte
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of SDB main.sd_ante and main.sd_antd tables
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_ante is 'spoofed' by the user's table hulme.sd_ante (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_ante.
        private static string[] SD_ANTES = new string[]
        {
            "'USX8-7W','0',NULL,'42.9','1.1','4','07 GHZ','ANDREW','7397','USX8-7W','67','36','8 FT PARABOLIC','DPP','78','7125000','8500000','7B;8B','2019.02.12','08:50'",
            "'04F1015D','0','WORST6GT','47.6','0','0','06 GHZ','UNKNOWN','','PERISCOPIC','0','0','4 FT/10X15/150-300','PERISCPC','41','5850000','6425000','6A','2002.01.30','14:16'"
        };

        // Rows of test data to be inserted into spoof table sd_antd.
        private static string[] SD_ANTDS = new string[]
        {
            "'USX8-7W','0','0','36','0','36',NULL,'0','2019.02.12','08:50'",
            "'USX8-7W','0.2','0.2','36','0.2','36',NULL,'5','2019.02.12','08:50'",
            "'USX8-7W','0.3','0.8','36','0.5','36',NULL,'13','2019.02.12','08:50'",
            "'USX8-7W','0.4','1.25','36','0.8','36',NULL,'7','2019.02.12','08:50'",
            "'USX8-7W','0.5','1.7','36','1.7','36',NULL,'5','2019.02.12','08:50'",
            "'USX8-7W','0.65','3.1','36','3.1','36',NULL,'5','2019.02.12','08:50'",
            "'USX8-7W','0.8','4.8','36','4.8','36',NULL,'5','2019.02.12','08:50'",
            "'USX8-7W','1','6.9','36','6.9','36',NULL,'5','2019.02.12','08:50'"
         };

        /// <summary>
        /// This method drops then recreates user SDB sd_ante and sd_antd 'spoofing' tables in the
        /// user's DB schema and (optionally) populates them with a small set of record data that 
        /// can be used for 'safe' testing that does not endanger the main SDB tables.
        /// </summary>
        /// <param name="schema"> - the user's SQL Server schema (e.g. hulme).</param>
        /// <param name="isWriteSpoofTestData"> - boolean that prescribes whether the spoofing tables should be populated with test data, or not.</param>
        /// <returns></returns>
        public static int CreateSpoofSdbTables(string schema, bool isWriteSpoofTestData)
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_ante]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_antd]", schema));

            DoQuery(hStmt, String.Format(SdAnte.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(SdAntd.CREATE_TABLE, schema));

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
                foreach (string sdAnte in SD_ANTES)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_ante] VALUES ({1})", schema, sdAnte));
                }

                foreach (string sdAntd in SD_ANTDS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_antd] VALUES ({1})", schema, sdAntd));
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

```
