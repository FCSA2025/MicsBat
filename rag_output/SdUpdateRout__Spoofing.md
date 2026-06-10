# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateRout\Spoofing.cs`
**Primary Layer:** `SdUpdateRout`
**Namespace:** `SdUpdateRout`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateRout
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of the SDB main.sd_rout table
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_rout is 'spoofed' by the user's table hulme.sd_rout (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_rout.
        private static string[] SD_ROUTES = new string[]
        {
            "'ACC','100','ON','=ACR51','STEELON - NORTHERN 23GHz DIGITAL.','1992.12.09','10:54'",
            "'ALAS','440','YT','CHB378','CNC-ALAS DAVE-BEAVER CK','1992.12.09','10:55'",
            "'ATT1','441','NB','CFW53','NBT-ATT1 PLEASANT RG-COOPER HILL','1992.12.09','10:55'",
            "'BCHY','700','BC','XOK955','VANCOUVER ISLAND','1993.11.18','21:38'",
            "'HYQU','152','QC','XOJ641','CARLETON T - CARLETON NCD','2012.12.09','16:16'"
        };

        /// <summary>
        /// This method drops then recreates user a SDB sd_rout 'spoofing' table in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_rout]", schema));

            DoQuery(hStmt, String.Format(SdRout.CREATE_TABLE, schema));

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
                foreach (string sdRout in SD_ROUTES)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_rout] VALUES ({1})", schema, sdRout));
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
