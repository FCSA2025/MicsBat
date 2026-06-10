# Documented File: Program.cs
**Repository Path:** `CopyTable\Program.cs`
**Primary Layer:** `CopyTable`
**Namespace:** `CopyTable`

## Source Code Representation
```csharp
﻿using System;
using System.IO;
using System.Data;
using System.Data.Odbc;
using System.Security.Principal;
using System.Collections;
using System.Diagnostics;
using System.Transactions;
using System.Runtime.Remoting;

namespace CopyTable
{
    // this routine is used only by webmics to copy a pdf, sdf or tsipparm set of tables.
    // new blank ones are copied from the appropriate tabledef schema table(s)
    // existing ones are copied from the current users' schema

    // return codes are:
    // 0 - success
    // 1 - could not open Odbc connection
    // 2 - error executing query to get default schema
    // 4 - invalid file type specified
    // 5 - elapsed time calculation failed
    // 6 - insertion of billing record failed
    // 7 - copy tables failed

    class Program
    {
        static string cn_str;       // db connection string
        static StreamWriter sw;     // optional debug file
        static string sesSchema;    // users' schema
        static bool commit_trans;   // flag for committing of sql transaction
        static int diagflag;        // flag to determine if debug info is to be written to log file (0 indicates NO, > 0 indicates YES)
        static string ProgName;     // program name ("CopyTable" in this case)
        static string userid;       // current user id
        static string logfile;      // only set if diagflag is > 0, default is "" 

        static int Main(string[] args)
        {
            ProgName = "CopyTable";
            userid = Environment.GetEnvironmentVariable("MicsUser");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");
            string odbc = Environment.GetEnvironmentVariable("odbc");

            // get argument values
            string dbase = args[0];         // database
            string new_copy = args[1];      // new or copy
            string filetype = args[2];      // file type
            string oldname = args[3];       // original file name
            string newname = args[4];       // new file name
            string projectCode = args[5];   // project code

            // MARS_Connection=yes is required to support TransactionScope
            cn_str = "DSN=" + odbc + ";DATABASE=" + dbase + ";Trusted_Connection=yes;MARS_Connection=yes";
            //cn_str = "DSN=" + odbc + ";DATABASE=" + dbase + ";Trusted_Connection = True";

            // this function sets the diagnostic flag for output to D:\extractlogs
            // see notes in CheckDebugSetting() below for details
            diagflag = CheckDebugSetting(); // 0 means no diagnostics, >0 means write diagnostics
                                            //diagflag = 2;

            logfile = "";

            if (diagflag > 0)
            {
                logfile = webdrive + "\\extractlogs\\" + dbase + "_" + userid + "CopyTable.txt";
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
                WriteDebug("NEW_COPY:" + new_copy);
                WriteDebug("FILETYPE:" + filetype);
                WriteDebug("OLDNAME :" + oldname);
                WriteDebug("NEWNAME :" + newname);
                WriteDebug("PROJECT :" + projectCode);
                WriteDebugFlush("CNSTR   :" + cn_str);
            }

            using (OdbcConnection cn = new OdbcConnection(cn_str))
            {    // try to open sql connection
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

            int exit_code = 0;

            string info = oldname + "-" + newname;
            string retval = "";
            string module = "";

            switch (filetype)
            {
                case "TS":
                    if ((retval = copyTSpdf(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TSERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TSCOPY";
                    break;
                case "ES":
                    if ((retval = copyESpdf(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("ESERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "ESCOPY";
                    WriteDebugFlush("ESCOPY");
                    break;
                case "Ante":
                    if ((retval = copySDante(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("AnteERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "ANTECOPY";
                    break;
                case "Band":
                    if ((retval = copySDband(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("BandERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "BANDCOPY";
                    break;
                case "Ctx":
                    if ((retval = copySDctx(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("CtxERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "CTXCOPY";
                    break;
                case "Eqpt":
                    if ((retval = copySDeqpt(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("EqptERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "EQPTCOPY";
                    break;
                case "Note":
                    if ((retval = copySDnote(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("NoteERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "NOTECOPY";
                    break;
                case "Oper":
                    if ((retval = copySDoper(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("OperERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "OPERCOPY";
                    break;
                case "Plan":
                    if ((retval = copySDplan(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("PlanERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "PLANCOPY";
                    break;
                case "Rout":
                    if ((retval = copySDrout(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("RoutERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "ROUTCOPY";
                    break;
                case "Town":
                    if ((retval = copySDtown(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TownERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TOWNCOPY";
                    break;
                case "Towr":
                    if ((retval = copySDtowr(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TowrERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TOWRCOPY";
                    break;
                case "Traf":
                    if ((retval = copySDtraf(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TrafERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TRAFCOPY";
                    break;
                case "TsipParm":
                    if ((retval = copyTPparm(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        WriteDebugFlush("TsipParmERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TSIPCOPY";
                    break;
                default:
                    WriteDebugFlush("Invalid file type: " + filetype);
                    exit_code = 4;
                    break;
            }

            if (exit_code == 0)
            {
                int intret = log_billing(thisProc, sesSchema, userid, projectCode, module, info);
                exit_code = intret;
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
            try
            {
                DateTime ExitTime = DateTime.Now;
                elapsed = ExitTime.Subtract(inProc.StartTime);
            }
            catch (Exception)
            {
                WriteDebugFlush("elapsed failed");
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
                WriteDebugFlush("record inserted");
                return 0;
            }
            catch (Exception e1)
            {
                WriteDebugFlush("billing insertion failed" + e1.Message);
                cn.Close();
                return 6;
            }

        }
        private static string copyTSpdf(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string strSql;
            string retval = "";

            // This version uses a try block to match sample code for distributed transaction coordination
            // if any error is detected in the try block, the transaction is automatically rolled back
            // Otherwise the transcope.Complete() at the end of the try block is executed. 
            // The original version used the glodal commit_trans variable defaulting to true, but reset to false if any
            // error occurs in a call to the copytable routine. In this case commit_trans would be false and the transcope.Complete() call would not be invoked.
            //
            // All the remaining copy... routines in this module retain the original structure.
            //
            commit_trans = true;
            
            WriteDebug("commit_trans:" + commit_trans);
            WriteDebugFlush("in copyTSpdf:" + cn_str + ":");

            try
            {
                using (System.Transactions.TransactionScope transcope = new System.Transactions.TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    WriteDebugFlush("in using transcope");
                    using (OdbcConnection cn = new OdbcConnection(cn_str))
                    {
                        // try to open sql connection
                        try
                        {
                            cn.Open();
                            WriteDebugFlush("connection opened");
                        }
                        catch (Exception e1)
                        {
                            WriteDebugFlush("connection failed" + e1.Message);
                            return "ERROR: " + e1.Message;
                        }

                        WriteDebugFlush(" ");
                        // copy ante table
                        WriteDebugFlush("before ante: " + commit_trans);

                        if ((retval = copy_table(cn, new_copy, "ft", "ante", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                        WriteDebugFlush(transcope.ToString());

                        // copy channel table
                        WriteDebugFlush("before chan: " + commit_trans);
                        if ((retval = copy_table(cn, new_copy, "ft", "chan", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                        WriteDebugFlush(transcope.ToString());

                        // copy chng table
                        WriteDebugFlush("before chng: " + commit_trans);
                        if ((retval = copy_table(cn, new_copy, "ft", "chng", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                        // copy shrl table
                        WriteDebugFlush("before shrl: " + commit_trans);
                        if ((retval = copy_table(cn, new_copy, "ft", "shrl", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                        // copy site table
                        WriteDebugFlush("before site: " + commit_trans);
                        if ((retval = copy_table(cn, new_copy, "ft", "site", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                        // copy titl table
                        WriteDebugFlush("before titl: " + commit_trans);
                        if ((retval = copy_table(cn, new_copy, "ft", "titl", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                        if (new_copy == "new")  // insert
                        {
                            strSql = "INSERT INTO " + sesSchema + ".ft_" + targetfile + "_titl " +
                                     "(namef,validated) VALUES ('" +
                                     targetfile + "','N')";
                            if ((retval = perform_sql(cn, strSql)) != "OK") { cn.Close(); return retval; }
                        }
                        else // update
                        {
                            //update titl record to match new pdf
                            strSql = "UPDATE " + sesSchema + ".ft_" + targetfile + "_titl " +
                                     "SET namef = '" + targetfile + "', validated = 'N'";
                            if ((retval = perform_sql(cn, strSql)) != "OK") { cn.Close(); return retval; }
                        }
                        WriteDebugFlush("titl: " + strSql);

                        //insert new record into user_tables
                        if ((retval = ins_user_table(cn, "0", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                        WriteDebugFlush("before commit_trans");

                        //if (commit_trans)
                        //{
                        transcope.Complete();
                        return "OK";
                        //}
                        //else
                        //{
                        //    return "";
                        //}
                    }
                }
            } //end of try
            catch (TransactionAbortedException ex)
            {
                WriteDebugFlush("TransactionAbortedException Message:" + ex.Message);
                    return "";
            }
        }
        private static string copyESpdf(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string strSql;
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope transcope = new System.Transactions.TransactionScope())
            {
                using (OdbcConnection cn = new OdbcConnection(cn_str))
                {
                    // try to open sql connection
                    try
                    {
                        cn.Open();
                    }
                    catch (Exception e1)
                    {
                        return "ERROR: " + e1.Message;
                    }

                    // copy ante table
                    if ((retval = copy_table(cn, new_copy, "fe", "ante", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_ante copied"); 

                    // copy azimuth table
                    if ((retval = copy_table(cn, new_copy, "fe", "azim", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_azim copied"); 

                    // copy ccal table
                    if ((retval = copy_table(cn, new_copy, "fe", "ccal", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_ccal copied"); 

                    // copy channel table
                    if ((retval = copy_table(cn, new_copy, "fe", "chan", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_chan copied"); 

                    // copy cloc table
                    if ((retval = copy_table(cn, new_copy, "fe", "cloc", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_cloc copied"); 

                    // copy shrl table
                    if ((retval = copy_table(cn, new_copy, "fe", "shrl", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                    // copy site table
                    if ((retval = copy_table(cn, new_copy, "fe", "site", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_site copied"); 

                    // copy titl table
                    if ((retval = copy_table(cn, new_copy, "fe", "titl", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("fe_titl copied"); 

                    if (new_copy == "new")  // insert
                    {
                        strSql = "INSERT INTO " + sesSchema + ".fe_" + targetfile + "_titl " +
                                 "(namef,validated) VALUES ('" +
                                 targetfile + "','N')";
                        if ((retval = perform_sql(cn, strSql)) != "OK") { cn.Close(); return retval; }
                        WriteDebugFlush("info inserted into new fe_titl"); 
                    }
                    else // update
                    {
                        //update titl record to match new pdf
                        strSql = "UPDATE " + sesSchema + ".fe_" + targetfile + "_titl " +
                                 "SET namef = '" + targetfile + "', validated = 'N'";
                        if ((retval = perform_sql(cn, strSql)) != "OK") { cn.Close(); return retval; }
                        WriteDebugFlush("info updated in copied fe_titl"); 
                    }

                    //insert new record into user_tables
                    if ((retval = ins_user_table(cn, "5", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }
                    WriteDebugFlush("info inserted into user_tables"); 

                    cn.Close();

                    if (commit_trans)
                    {
                        transcope.Complete();
                    }
                }
            }

            return "OK";

        }
        private static string copySDante(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy ante table
                if ((retval = copy_table(cn, new_copy, "su", "ante", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                // copy discrimination table
                if ((retval = copy_table(cn, new_copy, "su", "antd", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "301", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDband(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy band table
                if ((retval = copy_table(cn, new_copy, "su", "band", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "300", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDctx(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy ctx_ table
                if ((retval = copy_table(cn, new_copy, "su", "ctx_", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                // copy discrimination table
                if ((retval = copy_table(cn, new_copy, "su", "ctxd", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "303", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDeqpt(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy ante table
                if ((retval = copy_table(cn, new_copy, "su", "eqpt", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "305", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDnote(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy note table
                if ((retval = copy_table(cn, new_copy, "su", "note", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "306", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDoper(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy oper table
                if ((retval = copy_table(cn, new_copy, "su", "oper", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "308", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDplan(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy plan table
                if ((retval = copy_table(cn, new_copy, "su", "plan", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                // copy discrimination table
                if ((retval = copy_table(cn, new_copy, "su", "plnd", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables

                if ((retval = ins_user_table(cn, "310", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDrout(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy rout table
                if ((retval = copy_table(cn, new_copy, "su", "rout", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "309", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDtown(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy town table
                if ((retval = copy_table(cn, new_copy, "su", "town", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "313", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDtowr(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy towr table
                if ((retval = copy_table(cn, new_copy, "su", "towr", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "312", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copySDtraf(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy traf table
                if ((retval = copy_table(cn, new_copy, "su", "traf", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "314", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copyTPparm(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
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

                // copy TSIP parm table
                if ((retval = copy_table(cn, new_copy, "tp", "parm", sourcefile, targetfile, projectCode)) != "OK") { cn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(cn, "417", targetfile, userid, projectCode)) != "OK") { cn.Close(); return retval; }

                cn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK"; ;

        }
        private static string copy_table(OdbcConnection cn, string new_copy, string prefix, string tablename, string sourcefile, string targetfile, string projectCode)
        {
            //string tagBase = String.Format("copy_table() : {0} : {1} : ", prefix, tablename);

            string strSql = "";

            if (new_copy == "new")
            {
                // new table, copy info from tabledef master table
                strSql = "SELECT * INTO " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename +
                         " FROM tabledef.master_" + prefix + "_" + tablename;
            }
            else  // copy from a user table 
            {
                // copy of existing table from user's schema
                strSql = "SELECT * INTO " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename +
                         " FROM " + sesSchema + "." + prefix + "_" + sourcefile + "_" + tablename;
            }


            WriteDebugFlush("SQL:" + strSql); 

            OdbcCommand copy = new OdbcCommand(strSql, cn);


            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                WriteDebugFlush("ERRORSQL:" + strSql + ":" + e.Message);
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }


            // add primary key constraint if present in source table
            if (new_copy == "new")
            {
                strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                         " WHERE TABLE_SCHEMA = 'tabledef' AND TABLE_NAME = 'master_" +
                         prefix + "_" + tablename + "' " +
                         "ORDER BY ORDINAL_POSITION";
            }
            else
            {
                strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = '" + sesSchema + "' AND TABLE_NAME = '" +
                        prefix + "_" + sourcefile + "_" + tablename + "' " +
                        "ORDER BY ORDINAL_POSITION";
            }


            WriteDebugFlush("SQL:" + strSql);

            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                commit_trans = false;
                WriteDebugFlush("ERRORSQL:" + strSql + ":" + e2.Message);
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }


            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename +
                         " ADD CONSTRAINT PK_" + prefix + "_" + targetfile + "_" + tablename + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }


                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    commit_trans = false;
                    WriteDebugFlush("ERRORSQL:" + strSql + ":" + e.Message);
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }

            }
            else
            {
                dr1.Close();
            }


            return "OK";


        }

        private static string copy_table(System.Transactions.TransactionScope ts, OdbcConnection cn, string new_copy, string prefix, string tablename, string sourcefile, string targetfile, string projectCode)
        {
            string tagBase = String.Format("copy_table() : {0} : {1} : ", prefix, tablename);


            string strSql = "";

            if (new_copy == "new")
            {
                // new table, copy info from tabledef master table
                strSql = "SELECT * INTO " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename +
                         " FROM tabledef.master_" + prefix + "_" + tablename;
            }
            else  // copy from a user table 
            {
                // copy of existing table from user's schema
                strSql = "SELECT * INTO " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename +
                         " FROM " + sesSchema + "." + prefix + "_" + sourcefile + "_" + tablename;
            }

            WriteDebugFlush("SQL:" + strSql); 

            OdbcCommand copy = new OdbcCommand(strSql, cn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                WriteDebugFlush("ERRORSQL:" + strSql + ":" + e.Message);
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }


            // add primary key constraint if present in source table
            if (new_copy == "new")
            {
                strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                         " WHERE TABLE_SCHEMA = 'tabledef' AND TABLE_NAME = 'master_" +
                         prefix + "_" + tablename + "' " +
                         "ORDER BY ORDINAL_POSITION";
            }
            else
            {
                strSql = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE " +
                        " WHERE TABLE_SCHEMA = '" + sesSchema + "' AND TABLE_NAME = '" +
                        prefix + "_" + sourcefile + "_" + tablename + "' " +
                        "ORDER BY ORDINAL_POSITION";
            }


            WriteDebugFlush("SQL:" + strSql);

            OdbcCommand select = new OdbcCommand(strSql, cn);
            OdbcDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                commit_trans = false;
                WriteDebugFlush("ERRORSQL:" + strSql + ":" + e2.Message);
                return "ERRORSQL:" + strSql + ":" + e2.Message;
            }


            if (dr1.HasRows)
            {
                string comma = "";
                // create primary key script
                strSql = "ALTER TABLE " + sesSchema + "." + prefix + "_" + targetfile + "_" + tablename +
                         " ADD CONSTRAINT PK_" + prefix + "_" + targetfile + "_" + tablename + " PRIMARY KEY CLUSTERED (";

                while (dr1.Read())
                {
                    strSql += comma + dr1.GetString(0).Trim();
                    comma = ",";
                }


                strSql += ")";
                dr1.Close();

                OdbcCommand update = new OdbcCommand(strSql, cn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    commit_trans = false;
                    WriteDebugFlush("ERRORSQL:" + strSql + ":" + e.Message);
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }

            }
            else
            {
                dr1.Close();
            }


            return "OK";

        }
        private static string ins_user_table(OdbcConnection cn, string tabletype, string targetfile, string userid, string projectCode)
        {
            string strSql = "";

            // insert entry into web.user_tables
            //SQL:VIEW:web.user_tables_view
            strSql = "INSERT INTO web.user_tables_view " +
                      "(operator, tabletype, file_name, micsid, project_code, validstat, create_date) " +
                      "VALUES ('" + sesSchema + "'," + tabletype + ",'" + targetfile + "','" + userid + "','" +
                      projectCode + "','N',getdate())";

            OdbcCommand clr = new OdbcCommand(strSql, cn);

            try
            {
                clr.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                WriteDebugFlush("ERRORSQL:" + strSql + ":" + e.Message);
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
                WriteDebugFlush("ERRORSQL:" + inSql + ":" + e.Message);
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

```
