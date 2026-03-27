using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _Utillib;
using _NewLib;
using _DataStructures;

namespace Tools
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
    using System.IO;
    public class SpoofTablesForFtValidate
    {
        private const string SITES_TO_SPOOF_FILE_PATH = @"D:\Users\ahulme\MICS#\Tools\SitesToSpoofForFtValidate.txt";
        public static void Go()
        {
            //...Log2.v("\n\nSpoofTablesForFtValidate.Go(): Entry");

            // Attempt to read the TS sites to spoof from the prescribed text file.
            List<string> sitesToSpoof = new List<string>();

            try
            {
                string[] lines = File.ReadAllLines(SITES_TO_SPOOF_FILE_PATH);

                foreach (string callSign in lines)
                {
                    string candidate = callSign.Trim().ToUpper();

                    if (String.IsNullOrWhiteSpace(candidate)) continue;
                    if (candidate.StartsWith("//")) continue;
                    if (candidate.StartsWith("REM ")) continue;

                    sitesToSpoof.Add(candidate);
                }
            }
            catch (Exception e)
            {
                string str = String.Format("\r\nSpoofTablesForFtValidate.CreateSpoofMdbTables(): ERROR: exception: \n{0}\n{1}\n", e.Message, e.StackTrace);
                Log2.e(str);
                Console.Write(str);
                Application.ExitQuietly(666);
            }

            // Get environment variables required to begin a database session.
            // Use static object Info to collate environment variables, process IDs etc.
            Info.DbName = "fcsa";
            Info.MicsUserName = Environment.GetEnvironmentVariable("MicsUser");     // REQUIRED.
            Info.Password = Environment.GetEnvironmentVariable("Password");         // REQUIRED (but can be anything).

            /* Attempt connection to the prescribed database */
            int rc = Ssutil.UtConnect(Info.DbName, 1);
            if (rc != 0)
            {
                string str = String.Format("\r\nCannot connect to database {0}, reason {1:D}\r\n", Info.DbName, rc);
                Console.Write(str);
                Log2.e("\n\nSpoofTablesForFtValidate.Go(): ERROR: " + str);
                Application.Exit(98);
            }
            //...Log2.v("\n\nSpoofTablesForFtValidate.Go(): Successfully connected to database: " + Info.DbName);

            CreateSpoofMdbForFtValidate(Info.GlobalSchema);

            PopulateSpoofMdbForFtValidate(Info.GlobalSchema, sitesToSpoof);

            UpdateSpoofMdbAlign14Jan2020(Info.GlobalSchema);

            //The following call frees up the ODBC handle to 'Environment' 
            //resources that were used during the session.
            Ssutil.UtDisconnect(1);

            //...Log2.v("\n\nSpoofTablesForFtValidate.Go(): Disconnected from database: " + Info.DbName);
            //...Log2.v("\n\nSpoofTablesForFtValidate.Go(): Exit");
        } // Go() method

        /// <summary>
        /// This method drops then recreates the set of TS MDB 'spoofing' tables
        /// in the user's SQL Server schema.
        /// </summary>
        /// <param name="schema"> - the user's SQL Server schema (e.g. hulme).</param>
        /// <returns></returns>
        public static int CreateSpoofMdbForFtValidate(string schema)
        {
            //...Log2.v("\nSpoofTablesForFtValidate.CreateSpoofMdbTables(): Entry: schema = " + schema);

            // First of all, protect against an improper call in which schema
            // evaluates to 'main'.
            string str = schema.Trim().ToLower();
            if (str.EndsWith("main"))
            {
                str = "\r\nSpoofTablesForFtValidate.CreateSpoofMdbTables(): ERROR: attempt to create spoofing tables for schema: 'main'\r\n";
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
                //...Log2.v("\nSpoofTablesForFtValidate.CreateSpoofMdbTables(): audit trail table already exists: " + schema + ".audit_trail");
            }
            else
            {
                //...Log2.v("\nSpoofTablesForFtValidate.CreateSpoofMdbTables(): creating audit trail table: " + schema + ".audit_trail");
                DoQuery(hStmt, String.Format(AuditTrail.CREATE_TABLE, schema));
            }

            DoQuery(hStmt, "SET ANSI_PADDING OFF");

            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\nSpoofTablesForFtValidate.CreateSpoofMdbTables(): Exit");
            return sqlRet;
        }

        /// <summary>
        /// This method populates a set of TS MDB 'spoofing' tables with
        /// current records from the MDB.
        /// </summary>
        /// <param name="schema"> - the user's SQL Server schema (e.g. hulme).</param>
        /// <param name="callSigns"> - list of call signs to spoof.</param>
        /// <returns></returns>
        public static int PopulateSpoofMdbForFtValidate(string schema, List<string> callSigns)
        {
            //...Log2.v("\nSpoofTablesForFtValidate.PopulateSpoofMdbTables(): Entry: schema = " + schema);

            // First of all, protect against an improper call in which schema
            // evaluates to 'main'.
            string str = schema.Trim().ToLower();
            if (str.EndsWith("main"))
            {
                str = "\r\nSpoofTablesForFtValidate.PopulateSpoofMdbTables(): ERROR: attempt to populate spoofing tables for schema: 'main'\r\n";
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

            foreach (string callSign in callSigns)
            {
                string qualifier = DynMdbSite.MdbRecordExists(callSign) ? "Exists in main.mt_site" : "";

                Console.Write("\nSpoof site:   {0,-9}  :  {1}", callSign, qualifier);

                string insertSite = String.Format("INSERT INTO {0}.mt_site SELECT {1} FROM main.mt_site WHERE  call1='{2}'", schema, MtSite.AllColumnsForSqlSelectexceptSiteCoords, callSign);
                string insertAnte = String.Format("INSERT INTO {0}.mt_ante SELECT {1} FROM main.mt_ante WHERE  call1='{2}'", schema, "*", callSign);
                string insertChan = String.Format("INSERT INTO {0}.mt_chan SELECT {1} FROM main.mt_chan WHERE  call1='{2}'", schema, "*", callSign);
                string insertRout = String.Format("INSERT INTO {0}.sd_rout SELECT {1} FROM main.sd_rout WHERE rtcall='{2}'", schema, "*", callSign);
                string insertTown = String.Format("INSERT INTO {0}.sd_town SELECT {1} FROM main.sd_town WHERE  call1='{2}'", schema, "*", callSign);

                DoQuery(hStmt, insertSite);
                DoQuery(hStmt, insertAnte);
                DoQuery(hStmt, insertChan);
                DoQuery(hStmt, insertRout);
                DoQuery(hStmt, insertTown);
            }

            DoQuery(hStmt, "SET ANSI_PADDING OFF");

            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\nSpoofTablesForFtValidate.PopulateSpoofMdbTables(): Exit");
            return sqlRet;
        }

        /// <summary>
        /// This method updates some of the records in the TS MDB 'spoofing' tables to
        /// align with the contents of the MDB on 14-Jan-2020.
        /// </summary>
        /// <param name="schema"> - the user's SQL Server schema (e.g. hulme).</param>
        /// <returns></returns>
        public static int UpdateSpoofMdbAlign14Jan2020(string schema)
        {
            //...Log2.v("\nSpoofTablesForFtValidate.UpdateSpoofMdbTables(): Entry: schema = " + schema);

            // First of all, protect against an improper call in which schema
            // evaluates to 'main'.
            string str = schema.Trim().ToLower();
            if (str.EndsWith("main"))
            {
                str = "\r\nSpoofTablesForFtValidate.UpdateSpoofMdbTables(): ERROR: attempt to populate spoofing tables for schema: 'main'\r\n";
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

            string updateSite = "";
            string updateAnte = "";
            string updateChan = "";
            string fields = "";
            string where = "";
            string query = "";
            int retVal = -666;

            updateSite = "UPDATE " + schema + ".mt_site SET";
            updateAnte = "UPDATE " + schema + ".mt_ante SET";
            updateChan = "UPDATE " + schema + ".mt_chan SET";

            // Chan: CFX24,VEK982,6D,1001
            fields =   " eqpttx='MR41U6EC', traftx='D48MBS', stattx='3', feetx='F'";
            fields += ", eqptrx='MR41U6EC', trafrx='D48MBS', statrx='3', feerx='F'";
            fields += ", mdate='2008.05.03', mtime='09:21'";
            where = String.Format(" WHERE call1='{0}' AND call2='{1}' AND bndcde='{2}' AND chid='{3}'", "CFX24", "VEK982", "6D", "1001");
            query = updateChan + fields + where;
            retVal = DoQuery(hStmt, query);
            Console.Write("\n\n{0}\nretVal = {1}", query, retVal);

            // Chan: VEK982,CFX24,6D,1001
            fields = " eqpttx='MR41U6EC', traftx='D48MBS', stattx='3', feetx='F'";
            fields += ", eqptrx='MR41U6EC', trafrx='D48MBS', statrx='3', feerx='F'";
            where = String.Format(" WHERE call1='{0}' AND call2='{1}' AND bndcde='{2}' AND chid='{3}'", "VEK982", "CFX24", "6D", "1001");
            query = updateChan + fields + where;
            retVal = DoQuery(hStmt, query);
            Console.Write("\n\n{0}\nretVal = {1}", query, retVal);
#if false
            // Site: =TLUSMCZZ
            fields = " strlatit='49-11-32.40', strlongit='122-47-56.71', latit='17709240', longit='44207671'";
                where = String.Format(" WHERE call1='{0}'", "=TLUSMCZZ");
            query = updateSite + fields + where;
            retVal = DoQuery(hStmt, query);
            Console.Write("\n\n{0}\nretVal = {1}", query, retVal);
#endif
            DoQuery(hStmt, "SET ANSI_PADDING OFF");

            Ssutil.DisConnStmt(hConn, hStmt);

            //...Log2.v("\nSpoofTablesForFtValidate.PopulateSpoofMdbTables(): Exit");
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

    } // class
} // namespace
