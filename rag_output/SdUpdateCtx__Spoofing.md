# Documented File: Spoofing.cs
**Repository Path:** `SdUpdateCtx\Spoofing.cs`
**Primary Layer:** `SdUpdateCtx`
**Namespace:** `SdUpdateCtx`

## Source Code Representation
```csharp
﻿using System;

namespace SdUpdateCtx
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of SDB main.sd_ctx and main.sd_ctxd tables
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_ctx is 'spoofed' by the user's table hulme.sd_ctx (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_ctx.
        private static string[] SD_CTX = new string[]
        {
            "'A0005','DRB147','LD1506','62.3','-10','62.3','42','DRB147 DIG RADIO BROADCAST INTO ANLG 5CH','1997.10.08','14:11'",
            "'A0012','D75100','UNKNOWN','70.9','-10','70.9','55','DIG DEF 100 KHZ BW INTO ANALOG 12 VF CH','2003.12.23','11:45'"
        };

        // Rows of test data to be inserted into spoof table sd_ctxd.
        private static string[] SD_CTXD = new string[]
        {
            "'A0005','DRB147','LD1506','0','62.3','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','120','62.3','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','620','62.3','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','640','62.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','680','62.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','700','62','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','720','61.8','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','740','61.6','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','760','61.4','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','780','61.1','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','800','60.8','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','820','60.5','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','840','60.1','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','860','59.6','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','880','59.1','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','900','58.7','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','920','58.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','940','57.6','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','960','57','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','980','56.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','1000','55.3','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','1200','43.4','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','1400','40.3','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','1600','33.7','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','1800','33','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','2000','32.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','2200','31.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','2400','30','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','2600','28.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','2800','25.2','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','3000','19.5','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','3200','8.9','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','3400','4.9','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','3600','-2.1','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','3800','-7.1','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','4000','-9.1','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','4200','-9.7','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','4400','-9.8','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','4600','-9.9','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','5000','-9.9','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','5200','-10','1997.10.08','14:11'",
            "'A0005','DRB147','LD1506','300000','-10','1997.10.08','14:11'",
            "'A0012','D75100','UNKNOWN','0','70.9','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','120','70.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','140','70.3','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','160','66.2','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','180','64.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','200','62.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','220','60.4','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','240','58.1','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','260','55.8','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','280','53.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','300','50.9','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','320','50','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','560','50','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','580','50','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','600','49.8','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','620','49.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','640','49.1','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','660','48.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','680','47.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','700','46.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','720','45.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','740','44.3','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','760','43.2','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','780','42.1','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','800','41.4','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','820','40.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','840','39.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','860','38.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','880','37.7','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','900','36.8','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','920','36.1','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','940','35.3','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','960','34.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','980','33.7','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','1000','32.9','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','1200','26.2','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','1400','20.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','1600','15.6','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','1800','11.4','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','2000','7.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','2200','4.1','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','2400','0.9','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','2600','-2','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','2800','-4.7','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','3000','-7.2','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','3200','-9.5','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','3400','-10','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','68000','-10','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','70000','20.9','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','72000','-10','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','74000','-10','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','120000','-10','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','140000','40.9','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','160000','-10','2003.12.23','11:45'",
            "'A0012','D75100','UNKNOWN','300000','-10','2003.12.23','11:45'"
         };

        /// <summary>
        /// This method drops then recreates user SDB sd_ctx and sd_ctxd 'spoofing' tables in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_ctx]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_ctxd]", schema));

            DoQuery(hStmt, String.Format(SdCtx.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(SdCtxD.CREATE_TABLE, schema));

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
                foreach (string sdCtx in SD_CTX)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_ctx] VALUES ({1})", schema, sdCtx));
                }

                foreach (string sdCtxd in SD_CTXD)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_ctxd] VALUES ({1})", schema, sdCtxd));
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
