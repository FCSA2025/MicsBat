# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateOper\Spoofing.cs`
**Primary Layer:** `SdUpdateOper`
**Namespace:** `SdUpdateOper`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateOper
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of the SDB main.sd_oper table
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_oper is 'spoofed' by the user's table hulme.sd_oper (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_oper.
        // 'oper','nameop','cooper','mdbm','addr','city','prstat','zippc','dept','namep','phonep','faxnum','telecom','opnote','admin','email','mdate','mtime'

        private static string[] SD_OPERS = new string[]
        {
"'1000IS','THOUSAND ISLANDS BRIDGE AUTHORITY',NULL,'ICAN','43530 INTERSTATE 81','ALEXANDRIA','NY','13607',NULL,'TOM DWYER',NULL,NULL,'N','UC',NULL,NULL,'2016.12.14','17:54'",
"'110514','1105145 ONTARIO INC.','110514','ICAN','244 NEWKIRK RD.','RICHMOND HILL','ON','L4C 3S5',NULL,'MR J. STEWART',NULL,NULL,'N','CC',NULL,NULL,'2005.07.11','13:36'",
"'1147AB','11475435 ALBERTA LTD.',NULL,'ICAN','8882-170 STREET 239A EDMONTON MALL','EDMONTON','AB','T5T 4M2',NULL,'J. YERXA',NULL,NULL,'N','CC',NULL,NULL,'2013.12.06','15:55'",
"'1540','RADIO 1540 LIMITED','1540','BELL','637 COLLEGE STREET','TORONTO','ON','M6G 1B6',NULL,NULL,NULL,NULL,'N','CC','1540',NULL,'2007.05.04','09:19'",
"'166397','1663975 BC LTD. MOOSE FM CKFU',NULL,'ICAN','10423 101 AVENUE','FORT ST. JOHN','BC','V1J 2B7',NULL,NULL,NULL,NULL,'N','CC',NULL,NULL,'2013.12.06','15:55'",
"'178881','1788813 ONTARIO INC.',NULL,'ICAN','P.O. BOX 412','NIAGARA FALLS','ON','L2E 6T8',NULL,NULL,NULL,NULL,'N','CC',NULL,NULL,'2013.12.06','15:55'",
"'19079Q','19079-3670 QUEBEC INC.',NULL,'ICAN','1068 BOUL. VACHON NORD SUITE 101','STE. MARIE-DE-B','QC','G6E 1M6',NULL,NULL,NULL,NULL,'N','CC',NULL,NULL,'2013.12.06','15:55'"
        };

        /// <summary>
        /// This method drops then recreates user a SDB sd_oper 'spoofing' table in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_oper]", schema));

            DoQuery(hStmt, String.Format(SdOper.CREATE_TABLE, schema));

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
                foreach (string sdOper in SD_OPERS)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_oper] VALUES ({1})", schema, sdOper));
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
