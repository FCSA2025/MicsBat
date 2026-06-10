# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateTown\Spoofing.cs`
**Primary Layer:** `SdUpdateTown`
**Namespace:** `SdUpdateTown`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateTown
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of the SDB main.sd_town table
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_town is 'spoofed' by the user's table hulme.sd_town (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_town.
        private static string[] SD_TOWNS = new string[]
        {
            "'%CF01','CF','PR',NULL,'1','N','N',NULL,'1970',NULL,'1992.08.01','2000.01.19','07:09'",
            "'%CF02','CF','PR','7.4','1','N','N',NULL,'1970',NULL,'1998.09.01','2000.01.19','07:09'",
            "'%HYQU04','HYQU','PR','7.3','1','N','N',NULL,'1983',NULL,NULL,'1995.10.13','14:43'",
            "'%HYQU05','HYQU','PR','6.7','1','N','N',NULL,'1983',NULL,NULL,'1995.10.13','14:43'",
            "'%HYQU06','HYQU','PR','13.7','1','N','N',NULL,'1982',NULL,NULL,'1995.10.13','14:43'",
            "'%HYQU07','HYQU','PR','9.1','1','N','N',NULL,'1991',NULL,NULL,'1995.10.13','14:43'",
            "'%HYQU10','HYQU','PR','13.7','1','N','N',NULL,'1982',NULL,NULL,'2001.07.05','15:42'",
            "'%HYQU11','HYQU','PR','8','1','N','N',NULL,'1984',NULL,NULL,'1995.10.13','14:43'"
        };

        /// <summary>
        /// This method drops then recreates user a SDB sd_town 'spoofing' table in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_town]", schema));

            DoQuery(hStmt, String.Format(SdTown.CREATE_TABLE, schema));

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
                foreach (string sdTown in SD_TOWNS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_town] VALUES ({1})", schema, sdTown));
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
