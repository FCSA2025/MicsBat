# Documented File: ProgramSQL.cs
**Repository Path:** `CopyTable\ProgramSQL.cs`
**Primary Layer:** `CopyTable`
**Namespace:** `CopyTable`

## Source Code Representation
```csharp
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Text;
//using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security;
using AcctngUtilities;

namespace CopyTable
{
    // return codes are:
    // 0 - success
    // 1 - could not open SQL connection
    // 2 - error executing query to get default schema
    // 4 - invalid file type specified
    // 5 - elapsed time calculation failed
    // 6 - insertion of billing record failed
    // 7 - copy tables failed

    class Program
    {
        static string sqlcn_str;
        static StreamWriter sw;
        static string sesSchema;
        static bool commit_trans;

        static int Main(string[] args)
        {
            string userid = Environment.GetEnvironmentVariable("MicsUser");
            string webdrive = Environment.GetEnvironmentVariable("webdrive");

            string logfile = webdrive + "\\extractlogs\\" + userid + "CopyTable.txt";

            sw = new StreamWriter(logfile, false);
            sw.WriteLine(DateTime.Now);
            sw.Flush();

            // get argument values
            string dbase = args[0];         // database
            string new_copy = args[1];      // new or copy
            string filetype = args[2];      // file type
            string oldname = args[3];       // original file name
            string newname = args[4];       // new file name
            string projectCode = args[5];   // project code

            sw.WriteLine("USER    :" + userid);       
            sw.WriteLine("DBASE   :" + dbase);       
            sw.WriteLine("NEW_COPY:" + new_copy);    
            sw.WriteLine("FILETYPE:" + filetype);   
            sw.WriteLine("OLDNAME :" + oldname);    
            sw.WriteLine("NEWNAME :" + newname);    
            sw.WriteLine("PROJECT :" + projectCode);
            sw.Flush();

            // this section added to allow sql login testing on Venn's VSQL machines
            // get windows user name (it will be of form <machine>\<user> 
            string username = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToString();

            // get <machine> part of Windows user name
            char[] delimiter = "\\".ToCharArray();
            string[] keyparts = username.Split(delimiter);
            string machinename = keyparts[0];
 
            sqlcn_str = sqlConnectStr();
            SqlConnection sqlcn = new SqlConnection(sqlcn_str);
            
            // try to open sql connection
            try
            {
                sqlcn.Open();
            }
            catch (Exception e1)
            {
                sw.WriteLine(e1.Message);
                sw.Close();
                return 1;  // could not open connection
            }

            // get default schema
            SqlCommand getschema = new SqlCommand("SELECT RTrim(dbo.user_schema())", sqlcn);
            getschema.CommandType = CommandType.Text;

            try
            {
                sesSchema = (string)getschema.ExecuteScalar();
            }
            catch (Exception e2)
            {
                sw.WriteLine(e2.Message);
                sw.Close();
                return 2; // could not get default schema
            }

            sw.WriteLine("New schema:" + sesSchema + ":");
            sw.Flush();

            Process thisProc = Process.GetCurrentProcess();

            int exit_code = 0;

            string info = oldname + "-" + newname;
            string retval = "";
            string module = "";
            switch (filetype)
            {
                case "TS":
                    if((retval = copyTSpdf(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("TSERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TSCOPY";
                    break;
                case "ES":
                    if ((retval = copyESpdf(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("ESERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "ESCOPY";
                    break;
                case "Ante":
                    if ((retval = copySDante(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("AnteERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "ANTECOPY";
                    break;
                case "Band":
                    if ((retval = copySDband(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("BandERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "BANDCOPY";
                    break;
                case "Ctx":
                    if ((retval = copySDctx(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("CtxERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "CTXCOPY";
                    break;
                case "Eqpt":
                    if ((retval = copySDeqpt(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("EqptERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "EQPTCOPY";
                    break;
                case "Note":
                    if ((retval = copySDnote(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("NoteERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "NOTECOPY";
                    break;
                case "Oper":
                    if ((retval = copySDoper(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("OperERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "OPERCOPY";
                    break;
                case "Plan":
                    if ((retval = copySDplan(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("PlanERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "PLANCOPY";
                    break;
                case "Rout":
                    if ((retval = copySDrout(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("RoutERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "ROUTCOPY";
                    break;
                case "Town":
                    if ((retval = copySDtown(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("TownERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TOWNCOPY";
                    break;
                case "Towr":
                    if ((retval = copySDtowr(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("TowrERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TOWRCOPY";
                    break;
                case "Traf":
                    if ((retval = copySDtraf(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("TrafERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TRAFCOPY";
                    break;
                case "TsipParm":
                    if ((retval = copyTPparm(new_copy, oldname, newname, userid, projectCode)) != "OK")
                    {
                        sw.WriteLine("TsipParmERROR:" + retval);
                        exit_code = 7;
                    }
                    module = "TSIPCOPY";
                    break;
                default:
                    sw.WriteLine("Invalid file type: " + filetype);
                    exit_code = 4;
                    break;
            }

            if (exit_code == 0)
            {
                int intret = log_billing(thisProc, sesSchema, userid, projectCode, module, info);
                exit_code = intret;
            }

            sw.WriteLine("EXIT_CODE:" + exit_code.ToString());
            sw.Close();

            return exit_code;
         }
        private static int log_billing(Process inProc, String company, String user, String project, String module, String info)
        {
            SqlConnection sqlcn = new SqlConnection(sqlcn_str);
            // try to open sql connection
            try
            {
                sqlcn.Open();
            }
            catch (Exception e1)
            {
                sw.WriteLine(e1.Message);
                sw.Flush();
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
                sw.WriteLine("elapsed failed");
                sw.Flush();
                return 5;
            }
            
            string strSql = "insert into web.daily_usage_view values ('" +
                company + "','" + user + "',GetDate(),'" + project + "','" + module + "'," +
                elapsed.TotalSeconds + "," +
                inProc.UserProcessorTime.TotalMilliseconds + "," +
                inProc.TotalProcessorTime.TotalMilliseconds + ",'" + info + "')";
            sw.WriteLine(strSql);
            sw.Flush();
            // insert mics_billing
            SqlCommand insert1 = new SqlCommand(strSql, sqlcn);

            try
            {
                insert1.ExecuteNonQuery();
                sqlcn.Close();
                sw.WriteLine("record inserted");
                sw.Flush();
                return 0;
            }
            catch (Exception e1)
            {
                sw.WriteLine("billing insertion failed" + e1.Message);
                sw.Flush();
                sqlcn.Close();
                return 6;
            }

        }
        private static string copyTSpdf(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            Process thisProc = Process.GetCurrentProcess();

            string strSql;
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy ante table
                if ((retval = copy_table(sqlcn, new_copy, "ft", "ante", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy channel table
                if ((retval = copy_table(sqlcn, new_copy, "ft", "chan", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy chng table
                if ((retval = copy_table(sqlcn, new_copy, "ft", "chng", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy shrl table
                if ((retval = copy_table(sqlcn, new_copy, "ft", "shrl", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy site table
                if ((retval = copy_table(sqlcn, new_copy, "ft", "site", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy titl table
                if ((retval = copy_table(sqlcn, new_copy, "ft", "titl", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                if (new_copy == "new")  // insert
                {
                    strSql = "INSERT INTO " + sesSchema + ".ft_" + targetfile + "_titl " +
                             "(namef,validated) VALUES ('" +
                             targetfile + "','N')";
                    if ((retval = perform_sql(sqlcn, strSql)) != "OK") { sqlcn.Close(); return retval; }
                }
                else // update
                {
                    //update titl record to match new pdf
                    strSql = "UPDATE " + sesSchema + ".ft_" + targetfile + "_titl " +
                             "SET namef = '" + targetfile + "', validated = 'N'";
                    if ((retval = perform_sql(sqlcn, strSql)) != "OK") { sqlcn.Close(); return retval; }
                }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "0", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }
                
                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }

            return "OK";;

        }
        private static string copyESpdf(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string strSql;
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy ante table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "ante", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy azimuth table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "azim", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy ccal table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "ccal", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy channel table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "chan", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy cloc table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "cloc", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy shrl table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "shrl", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy site table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "site", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy titl table
                if ((retval = copy_table(sqlcn, new_copy, "fe", "titl", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                if (new_copy == "new")  // insert
                {
                    strSql = "INSERT INTO " + sesSchema + ".fe_" + targetfile + "_titl " +
                             "(namef,validated) VALUES ('" +
                             targetfile + "','N')";
                    if ((retval = perform_sql(sqlcn, strSql)) != "OK") { sqlcn.Close(); return retval; }
                }
                else // update
                {
                    //update titl record to match new pdf
                    strSql = "UPDATE " + sesSchema + ".fe_" + targetfile + "_titl " +
                             "SET namef = '" + targetfile + "', validated = 'N'";
                    if ((retval = perform_sql(sqlcn, strSql)) != "OK") { sqlcn.Close(); return retval; }
                }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "5", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
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
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy ante table
                if ((retval = copy_table(sqlcn, new_copy, "su", "ante", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy discrimination table
                if ((retval = copy_table(sqlcn, new_copy, "su", "antd", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "301", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDband(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy band table
                if ((retval = copy_table(sqlcn, new_copy, "su", "band", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "300", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDctx(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy ctx_ table
                if ((retval = copy_table(sqlcn, new_copy, "su", "ctx_", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy discrimination table
                if ((retval = copy_table(sqlcn, new_copy, "su", "ctxd", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "303", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDeqpt(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy ante table
                if ((retval = copy_table(sqlcn, new_copy, "su", "eqpt", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "305", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDnote(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy note table
                if ((retval = copy_table(sqlcn, new_copy, "su", "note", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "306", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDoper(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy oper table
                if ((retval = copy_table(sqlcn, new_copy, "su", "oper", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "308", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDplan(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy plan table
                if ((retval = copy_table(sqlcn, new_copy, "su", "plan", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                // copy discrimination table
                if ((retval = copy_table(sqlcn, new_copy, "su", "plnd", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables

                if ((retval = ins_user_table(sqlcn, "310", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDrout(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy rout table
                if ((retval = copy_table(sqlcn, new_copy, "su", "rout", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "309", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDtown(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy town table
                if ((retval = copy_table(sqlcn, new_copy, "su", "town", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "313", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDtowr(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy towr table
                if ((retval = copy_table(sqlcn, new_copy, "su", "towr", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "312", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copySDtraf(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy traf table
                if ((retval = copy_table(sqlcn, new_copy, "su", "traf", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "314", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copyTPparm(string new_copy, string sourcefile, string targetfile, string userid, string projectCode)
        {
            string retval = "";

            commit_trans = true;

            using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope())
            {
                SqlConnection sqlcn = new SqlConnection(sqlcn_str);
                // try to open sql connection
                try
                {
                    sqlcn.Open();
                }
                catch (Exception e1)
                {
                    return "ERROR: " + e1.Message;
                }

                // copy TSIP parm table
                if ((retval = copy_table(sqlcn, new_copy, "tp", "parm", sourcefile, targetfile, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                //insert new record into user_tables
                if ((retval = ins_user_table(sqlcn, "417", targetfile, userid, projectCode)) != "OK") { sqlcn.Close(); return retval; }

                sqlcn.Close();

                if (commit_trans)
                {
                    ts.Complete();
                }
            }
            return "OK";;

        }
        private static string copy_table(SqlConnection sqlcn, string new_copy, string prefix, string tablename, string sourcefile, string targetfile, string projectCode)
        {
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

            SqlCommand copy = new SqlCommand(strSql, sqlcn);

            try
            {
                copy.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                sw.WriteLine("ERRORSQL:" + strSql + ":" + e.Message);
                sw.Flush();
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
            SqlCommand select = new SqlCommand(strSql, sqlcn);
            SqlDataReader dr1;

            try
            {
                dr1 = select.ExecuteReader();
            }
            catch (Exception e2)
            {
                commit_trans = false;
                sw.WriteLine("ERRORSQL:" + strSql + ":" + e2.Message);
                sw.Flush();
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

                SqlCommand update = new SqlCommand(strSql, sqlcn);

                try
                {
                    update.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    commit_trans = false;
                    sw.WriteLine("ERRORSQL:" + strSql + ":" + e.Message);
                    sw.Flush();
                    return "ERRORSQL:" + strSql + ":" + e.Message;
                }
            }
            else
            {
                dr1.Close();
            }

            return "OK";

       }
        private static string ins_user_table(SqlConnection sqlcn, string tabletype, string targetfile, string userid, string projectCode)
        {
            string strSql = "";

            // insert entry into web.user_tables
            strSql = "INSERT INTO web.user_tables_view " +
                      "(operator, tabletype, file_name, micsid, project_code, validstat, create_date) " +
                      "VALUES ('" + sesSchema + "'," + tabletype + ",'" + targetfile + "','" + userid + "','" +
                      projectCode + "','N',getdate())";

            SqlCommand clr = new SqlCommand(strSql, sqlcn);

            try
            {
                clr.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                commit_trans = false;
                sw.WriteLine("ERRORSQL:" + strSql + ":" + e.Message);
                sw.Flush();
                return "ERRORSQL:" + strSql + ":" + e.Message;
            }

            return "OK";
        }
    
        private static string perform_sql(SqlConnection sqlcn, string inSql)
        {
            SqlCommand dosql = new SqlCommand(inSql, sqlcn);

            try
            {
                dosql.ExecuteNonQuery();
                return "OK";
            }
            catch (Exception e)
            {
                commit_trans = false;
                sw.WriteLine("ERRORSQL:" + inSql + ":" + e.Message);
                sw.Flush();
                return "ERRORSQL:" + inSql + ":" + e.Message;
            }
        }
        private static string sqlConnectStr()
        {
            string cline = System.Environment.CommandLine;
            string username;
            string password;
            string sqlinstance;
            string sessConnStr = "";

            string webdrive = Environment.GetEnvironmentVariable("webdrive");
            //string dbgfile = webdrive + "\\extractlogs\\" + userid + "CopyTable.txt";

            //StreamWriter sw = new StreamWriter(dbgfile, true);

            // try for test/bin or prod/bin
            try
            {
                // get user/password from environment
                username = Environment.GetEnvironmentVariable("MicsUser");
                password = Environment.GetEnvironmentVariable("Password");
                sqlinstance = Environment.GetEnvironmentVariable("SqlInstance");

                if (cline.ToLower().IndexOf("test\\bin") > 0)
                {
                      sessConnStr = "Server=" + sqlinstance + ";DATABASE=test;User ID=" + username + ";PWD=" + password + ";Trusted_Connection=no";
                }
                if (cline.ToLower().IndexOf("prod\\bin") > 0)
                {
                      sessConnStr = "Server=" + sqlinstance + ";DATABASE=fcsa;User ID=" + username + ";PWD=" + password + ";Trusted_Connection=no";
                }
                //sw.WriteLine("after second try" + sessConnStr);
                //sw.Flush();
            }
            catch (System.Exception Ex)  // no valid command line
            {
                //sw.WriteLine("second try failed" + sessConnStr);
                //sw.Flush();
                throw new Exception("*Error* Invalid connection string: " + Ex.Message);
            }
            
            return sessConnStr;
        }
    }
}

```
