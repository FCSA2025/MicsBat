using System;
using System.Collections;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Security.Principal;
using System.Diagnostics;

namespace KillTable
{
    // this routine is used only by webmics to delete a pdf, sdf or tsipparm set of tables.

    // return codes are:
    // 0 - success
    // 1 - could not open SQL connection
    // 2 - error executing query to get default schema
    // 3 - no default schema found
    // 4 - invalid file type specified
    // 5 - elapsed time calculation failed
    // 6 - insertion of billing record failed
    // 7 - drop tables failed

    class Program
    {
        static string cn_str;       // db connection string 
        static StreamWriter sw;     // optional debug file
        static string sesSchema;    // users' schema
        static bool commit_trans;   // flag for committing of sql transaction
        static int diagflag;        // flag to determine if debug info is to be written to log file (0 indicates NO, > 0 indicates YES)
        static string ProgName;     // program name ("KillTable" in this case)
        static string userid;       // current user id
        static string logfile;      // only set if diagflag is > 0, default is "" 

        static int Main(string[] args)
        {
            ProgName = "KillTable";
            userid = Environment.GetEnvironmentVariable("MicsUser");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");
            string odbc = Environment.GetEnvironmentVariable("odbc");

            // get argument values
            string dbase = args[0];         // database
            string filetype = args[1];      // file type
            string oldname = args[2];       // original file name
            string projectCode = args[3];   // project code

            // MARS_Connection=yes is required to support TransactionScope
            cn_str = "DSN=" + odbc + ";DATABASE=" + dbase + ";Trusted_Connection = True;MARS_Connection=yes";

            // this function sets the diagnostic flag for output to D:\extractlogs
            // see notes in CheckDebugSetting() below for details
            diagflag = CheckDebugSetting(); // 0 means no diagnostics, >0 means write diagnostics

            logfile = "";

            if (diagflag > 0)
            {
                // dbase is used as proxy for site type for debug file prefix
                logfile = webdrive + "\\extractlogs\\" + dbase + "_" + userid + "KillTable.txt";
                sw = new StreamWriter(logfile, false);
                WriteDebug(DateTime.Now.ToString());
                WriteDebug("DIAGFLAG:" + diagflag);
                WriteDebug("ENVIRONMENT");
                foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                {
                    if (de.Key.ToString() != "Password") WriteDebug(de.Key + ":" + de.Value);
                }

                WriteDebug("USER    :" + userid);
                WriteDebug("THREAD  :" + WindowsIdentity.GetCurrent().Name);
                WriteDebug("odbc    :" + odbc);
                WriteDebug("DBASE   :" + dbase);
                WriteDebug("FILETYPE:" + filetype);
                WriteDebug("OLDNAME :" + oldname);
                WriteDebug("PROJECT :" + projectCode);
                WriteDebugFlush("CNSTR   :" + cn_str);
            }

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {            
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    WriteDebug("Error opening connection");
                    WriteDebugClose(e1.Message);
                    return 2;  // error opening connection
                }
                //dbconnect dbinfo = new dbconnect(batch_cnstr);
                //cn_str = dbinfo.ConnectString;

                // get default schema
                OdbcCommand getschema = new OdbcCommand("SELECT RTrim(dbo.user_schema2022('" + userid + "'))", cn);
                getschema.CommandType = CommandType.Text;

                try
                {
                    sesSchema = (string)getschema.ExecuteScalar();
                }
                catch (Exception e2)
                {
                    WriteDebug("Error getting default schema");
                    WriteDebugClose(e2.Message);
                    cn.Close();
                    return 2; // could not get default schema
                }

                WriteDebugFlush("New schema:" + sesSchema + ":");
            }

            Process thisProc = Process.GetCurrentProcess();

            string info = oldname;
            string retval = "";
            string module = "";
            int exit_code = 0;
            int intret = 0;

            switch (filetype)
            {
                case "TS":
                    if ((retval = dropTSpdf(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TS:" + retval);
                        exit_code = 7;
                    }
                    module = "TSDROP";
                    break;
                case "ES":
                    if ((retval = dropESpdf(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("ES:" + retval);
                        exit_code = 7;
                    }
                    module = "ESDROP";
                    break;
                case "Ante":
                    if ((retval = dropSDante(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Ante:" + retval);
                        exit_code = 7;
                    }
                    module = "ANTEDROP";
                    break;
                case "Band":
                    if ((retval = dropSDband(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Band:" + retval);
                        exit_code = 7;
                    }
                    module = "BANDDROP";
                    break;
                case "Ctx":
                    if ((retval = dropSDctx(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("CTX:" + retval);
                        exit_code = 7;
                    }
                    module = "CTXDROP";
                    break;
                case "Eqpt":
                    if ((retval = dropSDeqpt(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Eqpt" + retval);
                        exit_code = 7;
                    }
                    module = "EQPTDROP";
                    break;
                case "Note":
                    if ((retval = dropSDnote(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Note:" + retval);
                        exit_code = 7;
                    }
                    module = "NOTEDROP";
                    break;
                case "Oper":
                    if ((retval = dropSDoper(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Oper:" + retval);
                        exit_code = 7;
                    }
                    module = "OPERDROP";
                    break;
                case "Plan":
                    if ((retval = dropSDplan(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Plan:" + retval);
                        exit_code = 7;
                    }
                    module = "PLANDROP";
                    break;
                case "Rout":
                    if ((retval = dropSDrout(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Rout:" + retval);
                        exit_code = 7;
                    }
                    module = "ROUTDROP";
                    break;
                case "Town":
                    if ((retval = dropSDtown(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Town:" + retval);
                        exit_code = 7;
                    }
                    module = "TOWNDROP";
                    break;
                case "Towr":
                    if ((retval = dropSDtowr(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Towr:" + retval);
                        exit_code = 7;
                    }
                    module = "TOWRDROP";
                    break;
                case "Traf":
                    if ((retval = dropSDtraf(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("Traf:" + retval);
                        exit_code = 7;
                    }
                    module = "TRAFDROP";
                    break;
                case "TsipParm":
                    if ((retval = dropTPparm(oldname, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TsipParm:" + retval);
                        exit_code = 7;
                    }
                    module = "TSIPDROP";
                    break;
                default:
                    WriteDebugFlush("Invalid file type: " + filetype);
                    return 4;
            }

            if (exit_code == 0)
            {
                intret = log_billing(thisProc, sesSchema, userid, projectCode, module, info);
                exit_code = intret;
                WriteDebugFlush("Billing:" + exit_code);
            }

            WriteDebugClose("EXIT_CODE:" + exit_code.ToString());
            return exit_code;
        }
        private static int log_billing(Process inProc, String company, String user, String project, String module, String info)
        {
            OdbcConnection cn = new OdbcConnection(cn_str);
            // try to open sql connection
            try
            {
                cn.Open();
            }
            catch (Exception e1)
            {
                WriteDebugFlush(e1.Message);
                return 1;  // error opening connection
            }
            // create sql to insert billing info  

            TimeSpan elapsed;
            DateTime ExitTime;
            try
            {
                ExitTime = DateTime.Now;
                elapsed = ExitTime.Subtract(inProc.StartTime);
            }
            catch (Exception)
            {
                WriteDebugFlush("Elapsed time calculation failed");
                return 5;
            }
            //SQL:VIEW:web.daily_usage_view 
            string strSql = "insert into web.daily_usage_view values ('" +
                company + "','" + user + "',GetDate(),'" + project + "','" + module + "'," +
                elapsed.TotalSeconds + "," +
                inProc.UserProcessorTime.TotalMilliseconds + "," +
                inProc.TotalProcessorTime.TotalMilliseconds + ",'" + info + "')";
            WriteDebugFlush(strSql);
            // insert mics_billing
            OdbcCommand insert1 = new OdbcCommand(strSql, cn);

            try
            {
                insert1.ExecuteNonQuery();
                cn.Close();
                WriteDebugFlush("billing record record inserted");
                return 0;
            }
            catch (Exception e1)
            {
                WriteDebugFlush("billing insertion failed" + e1.Message);
                cn.Close();
                return 6;
            }

        }
        private static string dropTSpdf(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete ante table
                if ((retval = drop_table(cn, "ft", "ante", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete channel table
                if ((retval = drop_table(cn, "ft", "chan", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete chng table
                if ((retval = drop_table(cn, "ft", "chng", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete shrl table
                if ((retval = drop_table(cn, "ft", "shrl", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete site table
                if ((retval = drop_table(cn, "ft", "site", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete titl table
                if ((retval = drop_table(cn, "ft", "titl", targetfile)) != "OK") { cn.Close(); return retval; }

                //delete record from user_tables
                if ((retval = clr_user_table(cn, "0", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropESpdf(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete ante table
                if ((retval = drop_table(cn, "fe", "ante", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete azimuth table
                if ((retval = drop_table(cn, "fe", "azim", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete ccal table
                if ((retval = drop_table(cn, "fe", "ccal", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete channel table
                if ((retval = drop_table(cn, "fe", "chan", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete cloc table
                if ((retval = drop_table(cn, "fe", "cloc", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete shrl table
                if ((retval = drop_table(cn, "fe", "shrl", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete site table
                if ((retval = drop_table(cn, "fe", "site", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete titl table
                if ((retval = drop_table(cn, "fe", "titl", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "5", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDante(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete ante table
                if ((retval = drop_table(cn, "su", "ante", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete discrimination table
                if ((retval = drop_table(cn, "su", "antd", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "301", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDband(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete band table
                if ((retval = drop_table(cn, "su", "band", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "300", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDctx(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete ctx_ table
                if ((retval = drop_table(cn, "su", "ctx_", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete discrimination table
                if ((retval = drop_table(cn, "su", "ctxd", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "303", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDeqpt(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete ante table
                if ((retval = drop_table(cn, "su", "eqpt", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "305", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDnote(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete note table
                if ((retval = drop_table(cn, "su", "note", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "306", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDoper(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete oper table
                if ((retval = drop_table(cn, "su", "oper", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from dba.user_tables
                if ((retval = clr_user_table(cn, "308", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDplan(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete plan table
                if ((retval = drop_table(cn, "su", "plan", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete discrimination table
                if ((retval = drop_table(cn, "su", "plnd", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from dba.user_tables
                if ((retval = clr_user_table(cn, "310", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDrout(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete rout table
                if ((retval = drop_table(cn, "su", "rout", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from dba.user_tables
                if ((retval = clr_user_table(cn, "309", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDtown(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete town table
                if ((retval = drop_table(cn, "su", "town", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from dba.user_tables
                if ((retval = clr_user_table(cn, "313", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDtowr(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete towr table
                if ((retval = drop_table(cn, "su", "towr", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from dba.user_tables
                if ((retval = clr_user_table(cn, "312", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropSDtraf(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete traf table
                if ((retval = drop_table(cn, "su", "traf", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from dba.user_tables
                if ((retval = clr_user_table(cn, "314", targetfile)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string dropTPparm(string targetfile, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                OdbcConnection cn = new OdbcConnection(cn_str);
                // try to open sql connection
                try
                {
                    cn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // delete TSIP parm table
                if ((retval = drop_table(cn, "tp", "parm", targetfile)) != "OK") { cn.Close(); return retval; }

                // delete record from web.user_tables
                if ((retval = clr_user_table(cn, "417", targetfile)) != "OK") { cn.Close(); return retval; }
                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";

        }
        private static string drop_table(OdbcConnection cn, string prefix, string tablename, string targetfile)
        {
            string strSql = "";

            // delete specified table
            strSql = "DROP TABLE " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename;

            OdbcCommand drop = new OdbcCommand(strSql, cn);

            try
            {
                drop.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            return "OK";
        }
        private static string clr_user_table(OdbcConnection cn, string tabletype, string targetfile)
        {
            string strSql = "";

            // delete entry from web.user_tables
            //SQL:VIEW:web.user_tables_view
            strSql = "DELETE FROM web.user_tables_view " +
                     "WHERE operator='" + sesSchema +
                     "' AND tabletype=" + tabletype +
                     " AND file_name='" + targetfile + "'";

            OdbcCommand clr = new OdbcCommand(strSql, cn);

            try
            {
                clr.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            return "OK";
        }
        private static string perform_sql(OdbcConnection cn, string inSql)
        {
            OdbcCommand dosql = new OdbcCommand(inSql, cn);

            try
            {
                dosql.ExecuteNonQuery();
                return "OK";
            }
            catch (Exception e)
            {
                commit_trans = false;
                return "ERRORSQL:" + inSql + ":" + e.Message;
            }
        }
        private static int CheckDebugSetting()
        {
            // this routine checks if debug info is to be written to file webdrive + "\\extractlogs\\" + userid + "<ProgName>.txt" 
            //
            // if the table web.debuglogs does not exist in the database, it assumes no output is to be written
            // if the table web.debuglogs exists in the database for this module and user, and the value of debugflag is 0, no output is to be written
            // if the table web.debuglogs exists in the database for this module and user, and the value of debugflag is > 0, output is to be written

            // check debug value for this module and user
            //SQL:TABLE:web.debuglogs
            string strSql = " SELECT debugflag FROM web.debuglogs WHERE debugmodule ='" + ProgName + "' AND micsid = '" + userid + "'";

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {
                cn.Open();

                OdbcCommand select = new OdbcCommand(strSql, cn);
                OdbcDataReader dr1;

                try
                {
                    dr1 = select.ExecuteReader();
                }
                catch
                {
                    // select will fail if table does not exist
                    return 0;
                }

                if (dr1.HasRows)
                {
                    dr1.Read();
                    // return value of debugflag
                    return dr1.GetInt32(0);
                }
                else
                {
                    // record not found for this user/debug module
                    return 0;
                }
            }
        }
        private static void WriteDebug(string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
            }
        }
        private static void WriteDebugFlush(string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
                sw.Flush();
            }
        }
        private static void WriteDebugClose(string instring)
        {
            if (diagflag > 0)
            {
                sw.WriteLine(instring);
                sw.Close();
            }
        }
    }
}
