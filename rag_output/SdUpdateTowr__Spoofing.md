# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateTowr\Spoofing.cs`
**Primary Layer:** `SdUpdateTowr`
**Namespace:** `SdUpdateTowr`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateTowr
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of the SDB main.sd_towr table
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_towr is 'spoofed' by the user's table hulme.sd_towr (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_towr.
        private static string[] SD_TOWRS = new string[]
        {
            "'CP','CONCRETE POLE','1992.08.17','16:02'",
            "'GS','SELF-SUPPORTING & GUYED TOWERS','1992.08.17','16:02'",
            "'GY','GUYED TOWER','1992.08.17','16:02'",
            "'H','H-TYPE STRUCTURE','1992.08.17','16:02'",
            "'MT','MULTI-TOWER SITE;SEE USER''S NOTE FILE FOR MORE INFO','1992.08.17','16:02'"
        };

        /// <summary>
        /// This method drops then recreates user a SDB sd_towr 'spoofing' table in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_towr]", schema));

            DoQuery(hStmt, String.Format(SdTowr.CREATE_TABLE, schema));

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
                foreach (string sdTowr in SD_TOWRS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_towr] VALUES ({1})", schema, sdTowr));
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
