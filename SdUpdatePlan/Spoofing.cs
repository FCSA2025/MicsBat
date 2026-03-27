using System;

namespace SdUpdatePlan
{
    using _DataStructures;
    using _NewLib;
    using _Utillib;
    using SQLHANDLE = IntPtr;
    using SQLHDBC = IntPtr;
    using SQLRETURN = Int16;

    /// <summary>
    /// This class provides methods that support the 'spoofing' of SDB main.sd_plan and main.sd_plnd tables
    /// by reading/writing to tables in the user's own schema, e.g. the SDB table
    /// main.sd_plan is 'spoofed' by the user's table hulme.sd_plan (say).
    /// </summary>
    class Spoofing
    {
        // Rows of test data to be inserted into spoof table sd_plan.
        private static string[] SD_PLAN = new string[]
        {
            "'10A','03',NULL,NULL,NULL,NULL,'1993.01.25','12:34'"
        };

        // Rows of test data to be inserted into spoof table sd_plnd.
        private static string[] SD_PLND = new string[]
        {
            "'10A','03','1','10552500',NULL,'10617500',NULL,'10557500',NULL,'10622500',NULL,'1993.01.25','14:08'",
            "'10A','03','2','10562500',NULL,'10627500',NULL,'10567500',NULL,'10632500',NULL,'1993.01.25','14:08'",
            "'10A','03','3','10572500',NULL,'10637500',NULL,'10577500',NULL,'10642500',NULL,'1993.01.25','14:08'",
            "'10A','03','4','10582500',NULL,'10647500',NULL,'10587500',NULL,'10652500',NULL,'1993.01.25','14:08'",
            "'10A','03','5','10592500',NULL,'10657500',NULL,'0',NULL,'0',NULL,'1993.01.25','14:08'"
         };

        /// <summary>
        /// This method drops then recreates user SDB sd_plan and sd_plnd 'spoofing' tables in the
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

            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_plan]", schema));
            DoQuery(hStmt, String.Format("DROP TABLE [{0}].[sd_plnd]", schema));

            DoQuery(hStmt, String.Format(SdPlan.CREATE_TABLE, schema));
            DoQuery(hStmt, String.Format(SdPlnd.CREATE_TABLE, schema));

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
                foreach (string sdPlan in SD_PLAN)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_plan] VALUES ({1})", schema, sdPlan));
                }

                foreach (string sdPlnd in SD_PLND)
                {
                    DoQuery(hStmt, String.Format("INSERT INTO [{0}].[sd_plnd] VALUES ({1})", schema, sdPlnd));
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

