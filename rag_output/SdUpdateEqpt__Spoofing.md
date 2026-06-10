# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateEqpt\Spoofing.cs`
**Primary Layer:** `SdUpdateEqpt`
**Namespace:** `SdUpdateEqpt`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateEqpt
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of the SDB main.sd_eqpt table
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_eqpt is 'spoofed' by the user's table hulme.sd_eqpt (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_eqpt.
        private static string[] SD_EQUIPMENTS = new string[]
        {
"'1013C1','0.001',NULL,'TOSHIBA',NULL,'THE BIG RED ONE','A',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL",
"'1013C5','0.001000','DFLT30','MTROLA','PTP18800','32QAM  101 MBS 30 MHZ BW','D','DPTP18','27M2D1D','70.0',NULL,'-75.0','18B',NULL,NULL",
"'108A','0.001','DFLT30','DRAGONWAVE','11EM30118-32QAMPTCM2','32QAM 118 MBS 30 MHZ BW','D','D1111A','30M0D7W',NULL,NULL,'-75.8','11A','2017.12.27','10:24'"
        };

        /// <summary>
        /// This method drops then recreates user a SDB sd_eqpt 'spoofing' table in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_eqpt]", schema));

            DoQuery(hStmt, String.Format(SdEqpt.CREATE_TABLE, schema));

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
                foreach (string sdEqpt in SD_EQUIPMENTS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_eqpt] VALUES ({1})", schema, sdEqpt));
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
