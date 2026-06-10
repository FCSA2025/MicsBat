# Documented File: SesUtils.cs
**Repository Path:** `utilities\SesUtils.cs`
**Primary Layer:** `utilities`
**Namespace:** `SesUtilities`

## Source Code Representation
```csharp
using ErrorUtilities;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Data.Odbc;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Web;

namespace SesUtilities
{
    /// <summary>
    /// Summary description for SesUtils.
    /// </summary>
    public class SesUtils
    {

        public static void LogSessionEnd(string appWEBDRIVE, SessionInfo sesInfo)
        {
            // note that for some reason this function requires explicit impersonation
            // whereas LogSessionStart does not
            // open file to log session vars (debug)

            string logfile = "";
            string info = "";
            switch (sesInfo.sesCLOSETYPE)
            {
                case "C": // password change
                    logfile = appWEBDRIVE + "\\perflogs\\" + sesInfo.sesUID + sesInfo.sesSID + "newpwd.txt";
                    info = "New Password:" + sesInfo.sesSID;
                    break;
                case "L": // logout
                    logfile = appWEBDRIVE + "\\perflogs\\" + sesInfo.sesUID + sesInfo.sesSID + "logout.txt";
                    info = "Web Logout:" + sesInfo.sesSID;
                    break;
                case "R": // restart with new project
                    logfile = appWEBDRIVE + "\\perflogs\\" + sesInfo.sesUID + sesInfo.sesSID + "restart.txt";
                    info = "Web Restart:" + sesInfo.sesSID;
                    break;
                case "T": // timeout
                    logfile = appWEBDRIVE + "\\perflogs\\" + sesInfo.sesUID + sesInfo.sesSID + "timeout.txt";
                    info = "Web Timeout:" + sesInfo.sesSID;
                    break;
                default: // unknown
                    logfile = appWEBDRIVE + "\\perflogs\\" + sesInfo.sesUID + sesInfo.sesSID + "unknown.txt";
                    info = "Unknown:" + sesInfo.sesSID;
                    break;
            }

            DateTime logTime = DateTime.Now;

            StreamWriter sw = new StreamWriter(logfile);
            sw.WriteLine("FROM ROUTINE sesUtils.LogSessionEnd");
            sw.WriteLine();
            sw.Flush();

            sw.WriteLine("MICS USER:" + sesInfo.sesUID);

            sw.WriteLine();
            sw.WriteLine("LOGTIME:" + logTime.ToString("yyyyMMddHHmmss.ffff"));
            sw.WriteLine("NETSESS:" + sesInfo.sesNSID);
            sw.WriteLine("Session:" + sesInfo.sesSID);
            sw.WriteLine("User ID:" + sesInfo.sesUID);
            sw.WriteLine(" Schema:" + sesInfo.sesSchema);
            sw.WriteLine("Project:" + sesInfo.sesDEFPROJ);
            sw.WriteLine("  CNSTR:" + sesInfo.sesCNSTR);
            sw.Flush();

            try
            {
                WindowsPrincipal wp = (WindowsPrincipal)sesInfo.sesWINPRIN;

                // start impersonation
                using (WindowsImpersonationContext WIC = ((WindowsIdentity)wp.Identity).Impersonate())
                {
                    using (OdbcConnection cn = new OdbcConnection(sesInfo.sesCNSTR))
                    {
                        cn.Open();
                        sw.WriteLine("Open connection successful");
                        sw.Flush();

                        // clear any info from cull_temp tables

                        string strSql;

                        strSql = "DELETE from " + sesInfo.sesSchema + ".cull_temp1 where sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSql);
                        sw.Flush();

                        using (OdbcCommand delete1 = new OdbcCommand(strSql, cn))
                        {
                            delete1.ExecuteNonQuery();
                        }

                        strSql = "DELETE from " + sesInfo.sesSchema + ".cull_temp2 where sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSql);

                        using (OdbcCommand delete2 = new OdbcCommand(strSql, cn))
                        {
                            delete2.ExecuteNonQuery();
                        }

                        strSql = "DELETE from " + sesInfo.sesSchema + ".cull_temp3 where sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSql);

                        using (OdbcCommand delete3 = new OdbcCommand(strSql, cn))
                        {
                            delete3.ExecuteNonQuery();
                        }

                        strSql = "DELETE from " + sesInfo.sesSchema + ".cull_temp1_es where sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSql);

                        using (OdbcCommand delete4 = new OdbcCommand(strSql, cn))
                        {
                            delete4.ExecuteNonQuery();
                        }

                        strSql = "DELETE from " + sesInfo.sesSchema + ".cull_temp2_es where sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSql);

                        using (OdbcCommand delete5 = new OdbcCommand(strSql, cn))
                        {
                            delete5.ExecuteNonQuery();
                        }

                        strSql = "DELETE from " + sesInfo.sesSchema + ".cull_temp3_es where sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSql);

                        using (OdbcCommand delete6 = new OdbcCommand(strSql, cn))
                        {
                            delete6.ExecuteNonQuery();
                        }

                        sw.WriteLine(sesInfo.sesPROJSTART);
                        sw.Flush();

                        int year = Convert.ToInt32(sesInfo.sesPROJSTART.Substring(0, 4));
                        int month = Convert.ToInt32(sesInfo.sesPROJSTART.Substring(4, 2));
                        int day = Convert.ToInt32(sesInfo.sesPROJSTART.Substring(6, 2));
                        int hour = Convert.ToInt32(sesInfo.sesPROJSTART.Substring(8, 2));
                        int minute = Convert.ToInt32(sesInfo.sesPROJSTART.Substring(10, 2));
                        int second = Convert.ToInt32(sesInfo.sesPROJSTART.Substring(12, 2));

                        DateTime stTime = new DateTime(year, month, day, hour, minute, second);
                        DateTime curTime = DateTime.Now;

                        TimeSpan sesTime = curTime - stTime; // time in ticks
                        int sesTimeSec = sesTime.Hours * 3600 + sesTime.Minutes * 60 + sesTime.Seconds;

                        sw.WriteLine(" ");
                        sw.WriteLine("Timeout:" + sesInfo.sesTIMEOUT + "(mins)");
                        sw.WriteLine("  Start:" + stTime.ToString("yyyyMMddHHmmss"));
                        sw.WriteLine("    End:" + curTime.ToString("yyyyMMddHHmmss"));
                        sw.WriteLine(" Length:" + sesTimeSec.ToString() + "(secs)");

                        sw.WriteLine(" Billed:" + sesTimeSec.ToString() + "(secs)");
                        sw.WriteLine(" ");

                        string module = "WEB_CONN";

                        // create sql to insert billing info  
                        //SQL:VIEW:web.daily_usage_view 
                        string strSqlb = "insert into web.daily_usage_view values ('" +
                            sesInfo.sesSchema + "','" + sesInfo.sesUID + "',GetDate(),'" + sesInfo.sesDEFPROJ + "','" + module + "'," +
                            sesTimeSec + ",0,0,'" + info + "')";

                        sw.WriteLine(strSqlb);

                        string sessend = curTime.ToString("yyyyMMddHHmmss");

                        // build sql to delete sessionlogs record to record session end

                        //SQL:TABLE:web.mthly_connect
                        string strSqlw = "DELETE web.mthly_connect WHERE sessionid = '" + sesInfo.sesSID + "'";
                        sw.WriteLine(strSqlw);
                        sw.Flush();

                        // insert mics_billing
                        using (OdbcCommand insert1 = new OdbcCommand(strSqlb, cn))
                        {
                            insert1.ExecuteNonQuery();
                        }

                        // delete web.mthly_connect                                   
                        using (OdbcCommand deletew = new OdbcCommand(strSqlw, cn))
                        {
                            deletew.ExecuteNonQuery();
                        }
                    }

                    // delete session from application list of active sessions
                    // the T case (timeout) is handled in the calling routine (global.asx/SessionEnd)
                    // as the http context is not available there

                    //  COMMENTED OUT JUNE 14, 2022   - re-instated MAY 20, 2023 with try block
                    // this try block is required for case of user closing mics after failed 'forgot password' reset
                    // as in this case no user session has been created so the httpcontext variables are not yet loaded
                    try
                    {
                        if (sesInfo.sesCLOSETYPE != "T")
                        {
                            HttpApplication locApp = new HttpApplication();
                            locApp = (HttpApplication)HttpContext.Current.ApplicationInstance;

                            char[] delimiter = ",".ToCharArray();

                            locApp.Application.Lock();
                            sw.WriteLine("Before:" + locApp.Application["sessions"].ToString());
                            sw.Flush();
                            string[] loc_session_array = locApp.Application["sessions"].ToString().Split(delimiter);
                            string[] loc_uuser_array = locApp.Application["uusers"].ToString().Split(delimiter);
                            string[] loc_muser_array = locApp.Application["musers"].ToString().Split(delimiter);

                            locApp.Application["sessions"] = "";
                            locApp.Application["uusers"] = "";
                            locApp.Application["musers"] = "";
                            string comma = "";

                            for (int i = 0; i < loc_session_array.Length; i++)
                            {
                                sw.WriteLine(loc_session_array[i] + ":" + sesInfo.sesSID);
                                sw.Flush();
                                if (loc_session_array[i] != sesInfo.sesSID)
                                {
                                    locApp.Application["sessions"] = locApp.Application["sessions"] + comma + loc_session_array[i];
                                    locApp.Application["uusers"] = locApp.Application["uusers"] + comma + loc_uuser_array[i];
                                    locApp.Application["musers"] = locApp.Application["musers"] + comma + loc_muser_array[i];
                                    comma = ",";
                                }
                            }
                            locApp.Application.UnLock();
                            sw.WriteLine("After:" + locApp.Application["sessions"].ToString());
                            sw.Flush();


                            sw.WriteLine("Before:StoreMenuUse"); sw.Flush();
                            StoreMenuUse();  //insert menu use info into database
                            sw.WriteLine("After:StoreMenuUse"); sw.Flush();

                            sw.Close();
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ee)
            {
                ErrorUtils.NotifySystemOps(ee, "LogSessionEnd");
            }
        }
        public static void LogSessionStart(string appWEBDRIVE)
        {
            HttpContext ctx = HttpContext.Current;

            string sesUID = ctx.Session["s_user"].ToString();   // user id
            string sesSchema = ctx.Session["s_schema"].ToString();    // schema
            string sesSID = ctx.Session["FCSASESS"].ToString(); // FCSA session id
            string sesNSID = ctx.Session.SessionID.ToString();  // .NET session ID
            string sesDEFPROJ = ctx.Session["defProject"].ToString();  // default project 
            string sesCLOSETYPE = ctx.Session["CloseReason"].ToString();  // reason for session close
            string sesTIMEOUT = ctx.Session.Timeout.ToString(); // timeout value
            string sesPROJSTART = ctx.Session["ProjStart"].ToString();   // project start time
            string sesCNSTR = ctx.Session["s_cnString"].ToString(); // DB connection string

            string logfile = appWEBDRIVE + "\\perflogs\\" + sesUID + sesSID + ".txt";

            DateTime logTime = DateTime.Now;

            StreamWriter sw = new StreamWriter(logfile);
            sw.WriteLine("LOGTIME:" + logTime.ToString("yyyyMMddHHmmss.ffff"));
            sw.WriteLine("NETSESS:" + sesNSID);
            sw.WriteLine("Session:" + sesSID);
            sw.WriteLine("User ID:" + sesUID);
            sw.WriteLine("Schema :" + sesSchema);
            sw.WriteLine("Project:" + sesDEFPROJ);
            sw.WriteLine("  Start:" + sesPROJSTART);
            sw.WriteLine("Timeout:" + sesTIMEOUT);

            OdbcConnection cn;
            try
            {
                cn = new OdbcConnection(sesCNSTR);
                cn.Open();
                sw.WriteLine("Open connection successful");
            }
            catch (Exception ex)
            {
                sw.WriteLine("Database Connection failed");
                sw.WriteLine("Connection:" + sesCNSTR + ":" + ex.Message);
                sw.Close();
                return;
            }

            // create sql to insert mthly_connect record to record session start
            //SQL:TABLE:web.mthly_connect
            string strSqlw = "insert into web.mthly_connect values ('" + sesSID + "','" +
                sesSchema + "','" +
                sesUID + "','" +
                sesPROJSTART + "')";

            sw.WriteLine(strSqlw);

            // insert mthly_connect
            OdbcCommand insert1 = new OdbcCommand(strSqlw, cn);

            try
            {
                insert1.ExecuteNonQuery();
                sw.WriteLine("Insert mthly_connect successful");
            }
            catch (Exception i1)
            {
                cn.Close();
                sw.WriteLine("Insert mthly_connect failed:" + i1.Message);
            }

            // add user info application list of active sessions
            //f1.writeline("BEFORE:" + Application("sessions"));

            HttpApplication locApp = new HttpApplication();

            locApp = (HttpApplication)HttpContext.Current.ApplicationInstance;
            locApp.Application.Lock();

            if (locApp.Application["sessions"].ToString() == "")
            {
                locApp.Application["sessions"] = sesSID;
                locApp.Application["uusers"] = sesSchema;
                locApp.Application["musers"] = sesUID;
            }
            else
            {
                locApp.Application["sessions"] = locApp.Application["sessions"] + "," + sesSID;
                locApp.Application["uusers"] = locApp.Application["uusers"] + "," + sesSchema;
                locApp.Application["musers"] = locApp.Application["musers"] + "," + sesUID;
            }

            locApp.Application.UnLock();
            //f1.writeline("AFTER:" + Application("sessions"));

            cn.Close();
            sw.Close();

        }
        public static void LogMenuUse(string strMenuItem)
        {
            // this routine appends a new 'MenuLLog' Session entry to mics
            HttpContext ctx = HttpContext.Current;

            string key = "MenuLog" + DateTime.Now.ToString("O");
            string value = ctx.Session["s_user"].ToString() + "^" + ctx.Session["FCSASESS"].ToString() + "^" + strMenuItem;
            ctx.Session.Add(key, value);
        }
        private static void StoreMenuUse()
        {
            /*
            // this routine adds a Session Menulog entry to record the reason for closing the session
            // then writes the Menulog Sesson entries to the web.menulogs table
            HttpContext ctx = HttpContext.Current;

            string menulog = ctx.Application["web_drive"].ToString() + "\\perflogs\\" + ctx.Session["s_user"].ToString() + "menulog.txt";  // continuous log file for session ends
            StreamWriter swse = new StreamWriter(menulog, true);
            swse.WriteLine("");swse.Flush();

            // add menulog to Session for closure event
            switch (ctx.Session["CloseReason"].ToString())
            {
                case "C": // password change
                    LogMenuUse("Change Password");
                    break;
                case "L": // logout
                    LogMenuUse("Logout");
                    break;
                case "R": // restart with new project
                    LogMenuUse("Change Project");
                    break;
                case "T": // timeout
                    LogMenuUse("Timeout");
                    break;
                default: // unknown
                    LogMenuUse("Unknown");
                    break;
            }
            swse.WriteLine("Close Reason: " + ctx.Session["CloseReason"].ToString()); swse.Flush();

            DateTime curTime = DateTime.Now;
            string log_time = curTime.ToString("yyyy/MM/dd HH:mm:ss"); 

            // build menulog info into datatable (this is not used yet - the program currently inserts individual records)

            using (OdbcConnection cn = new OdbcConnection(ctx.Session["s_cnString"].ToString()))
            {
                cn.Open();
                using (DataTable oDTmenulogs = new DataTable())
                {
                    //OdbcDataAdapter Da1 = new OdbcDataAdapter();
                    // create DataTable modelled on web.menulogs
                    //Da1.SelectCommand = new OdbcCommand("SELECT * FROM web.menulogs WHERE 1 = 0)", cn);
                    //OdbcCommandBuilder sqlCb1 = new OdbcCommandBuilder(Da1);
                    //Da1.Fill(oDTgeo);

                    string strSql;

                    // loop through session variables to find all entries starting with 'MenuLog' 
                    foreach (string s1 in ctx.Session.Keys)
                    {

                        //swse.WriteLine("Before filter:" + s1); swse.Flush();
                        if (s1.IndexOf("MenuLog") == 0)
                        {
                            //swse.WriteLine("After filter:" + s1);swse.Flush();
                            //DataRow drnew = oDTmenulogs.NewRow();

                            //swse.WriteLine("s1:" + s1); swse.Flush();

                            string loc_menulog_text = ctx.Session[s1].ToString();
                            swse.WriteLine("loc_menulog_text:" + loc_menulog_text); swse.Flush();

                            // parse session keys and copy entries to DataTable
                            char[] delimiter2 = "^".ToCharArray();
                            string[] loc_menulog_array = loc_menulog_text.Split(delimiter2);

                            swse.WriteLine("parts:" + loc_menulog_array[0].ToString() + "-" + loc_menulog_array[1].ToString() + "-" + 
                                loc_menulog_array[2].ToString()); swse.Flush();

                            //drnew["starttime"] = s1.Substring(7,23);
                            //drnew["micsid"] = loc_menulog_array[0];
                            //drnew["fcsasess"] = loc_menulog_array[1];
                            //drnew["micsmenu"] = loc_menulog_array[2];
                            //oDTmenulogs.Rows.Add();

                            // s1 represents a key to the menulog session variable 
                            // this key is of the form MenuLog2022-01-16T09:38:28.3605955-05:00
                            // SQL will not store times with more than 3 digits for milliseconds
                            // so the line below reduces s1 to 2022-01-16T09:38:28.360 before attempting
                            // to insert the information into a web.menulogs record
                            string timetomillisecs = s1.Substring(7, 23);

                            strSql = "INSERT INTO web.menulogs VALUES('" + timetomillisecs + "','" + loc_menulog_array[0].ToString() + "','" +
                                loc_menulog_array[1].ToString() + "','" + loc_menulog_array[2].ToString() + "')";
                            swse.WriteLine(strSql);swse.Flush();

                            // insert menulogs record
                            try
                            {
                                using (OdbcCommand insert1 = new OdbcCommand(strSql, cn))
                                {
                                    insert1.ExecuteNonQuery();
                                }
                            }
                            catch (Exception ex)
                            {
                                swse.WriteLine("Insert failed:" + ex.Message); swse.Flush();
                            }
                        }

                        //swse.WriteLine(s1.Substring(7) + " " + ctx.Session[s1].ToString());
                    }
                    swse.Close();
                }
            }
            */

            // bulk load datatable
            /*
            try
            {
                SesUtils.LogSessionEnd(Application["web_drive"].ToString(), si);
                swse.WriteLine("Session end logged");
            }
            catch
            {
                swse.WriteLine("Session end log failed");
            }
            swse.Close();
            */
        }

        public static void EmailError(string esubject, string ebody, string eattachment)
        {
            // this routine is used to send emails of error conditions to Simin/Jason/Bill

            HttpContext ctx = HttpContext.Current;

            MailMessage Message = new MailMessage();
            Message.From = new MailAddress("mics@fcsa.ca");
            Message.To.Add("jscott@fcsa.ca");
            Message.To.Add("sbekhsat@fcsa.ca");
            Message.Bcc.Add("ablesonb@icloud.com");
            Message.Subject = esubject;

            // load body
            if (ebody == "")
            {
                ebody = "See attachment";
            }

            Message.Body = ebody;

            // load attachment
            if (eattachment != "")
            {
                Attachment attachfile = new Attachment(eattachment);
                Message.Attachments.Add(attachfile);
            }

            // send message
            send_email_message(Message);
            return;

        }
        public static bool CheckTimedOut()
        {
            // tried other versions which failed - haven't tested stuff below

            // this routine checks if session has timed out 
            // and returns true if it has
            //if (HttpContext.Current == null )
            //{
            //    return true;
            //}
            //else
            //{
            return false;
            //}
        }
        public static bool send_email_message(MailMessage inMessage)
        {
            StreamWriter sw;
            HttpContext ctx = HttpContext.Current;
            string sesMID;
            // this try block handles case of sending email related to login errors, as mics user id is not known
            try
            {
                sesMID = ctx.Session["s_user"].ToString();
            }
            catch
            {
                sesMID = "mics";
            }
            string dbgfile = "D:\\MicsWebLogs\\" + sesMID + "SendEmailMessage.txt";
            sw = new StreamWriter(dbgfile, true);
            DateTime logTime = DateTime.Now;
            sw.WriteLine("LOGTIME:" + logTime.ToString("yyyyMMddHHmmss.ffff"));
            sw.Flush();

            try
            {
                sw.WriteLine("From:" + inMessage.From.ToString());
                sw.Flush();
                sw.WriteLine("TO:" + inMessage.To.ToString());
                sw.Flush();
                sw.WriteLine("CC:" + inMessage.CC.ToString());
                sw.Flush();
                sw.WriteLine("Subject:" + inMessage.Subject.ToString());
                sw.Flush();
                sw.WriteLine("Body:" + inMessage.Body.ToString());
                sw.Flush();
            }
            catch (Exception ea)
            {
                sw.WriteLine("Error writing email info (send_email_message):" + ea.Message);
                sw.Close();
                return false;
            }

            // get current password
            string MD5hash = "BOp6zsKnnk+BZHe+uEAwTV6YV0t61WMgadqdQpjMezmX";
                        
            SmtpClient client;
           
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            
            try
            {   // set up client for Amazon SES
                client = new SmtpClient("AKIAV2PWGTB7TCENCNUC", 587);
                client.EnableSsl = true;

                client.Credentials = new System.Net.NetworkCredential("mics@fcsa.ca", MD5hash);

                // The following line is suggested on-line to resolve possible issues in sending to outside email addresses
                //client.TargetName = "STARTTLS/smtp.office365.com";

                // these lines allow for non-ASCII chars in Subject or Body - they can be activated if necessary
                inMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                inMessage.BodyEncoding = System.Text.Encoding.UTF8;

                sw.WriteLine("EMAIL credentials passed");
                
                /* set up client for O365
                client = new SmtpClient("smtp.office365.com", 587);
                client.EnableSsl = true;

                client.Credentials = new System.Net.NetworkCredential("mics@fcsa.ca", MD5hash);

                // The following line is suggested on-line to resolve possible issues in sending to outside email addresses
                client.TargetName = "STARTTLS/smtp.office365.com";

                // these lines allow for non-ASCII chars in Subject or Body - they can be activated if necessary
                inMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                inMessage.BodyEncoding = System.Text.Encoding.UTF8;

                sw.WriteLine("EMAIL credentials passed");
                */
            }
            catch (Exception em)
            {
                sw.WriteLine("EMAIL credentials failed:" + em.Message);
                sw.Close();
                return false;
            }

            try
            {
                // Office 365 now requires the use of the TLS1.2 security protocol.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Send(inMessage);
                inMessage.Dispose();
                sw.WriteLine("EMAIL send confirmed");
                sw.Close();
                return true;
            }
            catch (Exception em1)
            {
                sw.WriteLine("EMAIL send failed:" + em1.Message);
                sw.Close();
                inMessage.Dispose();
                return false;
            }
        }
        public static bool send_email_message2(MailMessage inMessage, Int32 FCSA, Boolean VENN)
        {
            // this routine assumes that inMessage contains all info except for FCSA or Venn recipients which are optional
            // if FCSA is 0, recipients in inMessage.To are used
            // if FCSA is 1, Jason's and Simin's emails used as recipients
            // if FCSA is 2, Jason's and Simin's emails are added to existing recipients
            // if FCSA is 3, bill's email is used as recipient(this is used so fcsa does not get emails during dev testing)
            // if VENN is true, bill's email is added to recipients
            // The routine was added to avoid errors that arose with trying to use application variables to store emails
            // That implementaion caused sporadic 'Object not set to an instance' errors

            //StreamWriter sw;
            HttpContext ctx = HttpContext.Current;

            // set mandatory FROM address
            inMessage.From = new MailAddress("mics@fcsa.ca");

            // add optional FCSA addresses
            switch (FCSA)
            {
                case 0: // just use inMessage.To value provided
                    break;
                case 1:     // set inMessage.To to Jason and Simin
                    inMessage.To.Clear();  // there should not be any, but this is just to make sure
                    inMessage.To.Add("jscott@fcsa.ca");
                    inMessage.To.Add("sbekhsat@fcsa.ca");
                    break;
                case 2:     // add Jason and Simin to inMessage.To
                    inMessage.To.Add("jscott@fcsa.ca");
                    inMessage.To.Add("sbekhsat@fcsa.ca");
                    break;
                case 3:     // set inMessage.To to bill(this option is only used for reporting exceptions from the dev environment)
                    inMessage.To.Clear();  // there should not be any, but this is just to make sure
                    inMessage.To.Add("ablesonb@icloud.com");
                    break;
            }

            // add optional VENN address
            if (VENN)
            {
                inMessage.Bcc.Add("ablesonb@icloud.com");
            }

            // get current MDhash
            string keyFullPath = @"SOFTWARE\AlphaSoft\PDFconverter\Local\Defaults\Nuntius";
            string MD5hash = Read_HKEY_LOCAL_MACHINE(keyFullPath);

            if (MD5hash.Substring(0, 5) == "ERROR")
            {
                string dbgfile = "D:\\extractlogs\\EmailSendError" + ctx.Session["s_user"].ToString() + ".txt";
                StreamWriter sw = new StreamWriter(dbgfile, true); // append to file
                DateTime logTime = DateTime.Now;
                sw.WriteLine("LOGTIME:" + logTime.ToString("yyyyMMddHHmmss.ffff"));
                sw.WriteLine(MD5hash);
                sw.Close();
                return false;
            }

            // set up client with credentials for O365
            SmtpClient client = new SmtpClient("smtp.office365.com", 587);
            client.EnableSsl = true;

            // replace this with second line once pwd change is accepted
            client.Credentials = new System.Net.NetworkCredential("mics@fcsa.ca", MD5hash);

            // The following line is suggested on-line to resolve possible issues in sending to outside email addresses
            client.TargetName = "STARTTLS/smtp.office365.com";

            // these lines allow for non-ASCII chars in Subject or Body - they can be activated if necessary
            inMessage.SubjectEncoding = System.Text.Encoding.UTF8;
            inMessage.BodyEncoding = System.Text.Encoding.UTF8;

            // send email
            try
            {
                // Office 365 now requires the use of the TLS1.2 security protocol.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                client.Send(inMessage);
                inMessage.Dispose();
                return true;
            }
            catch (Exception em1)   // write error log and notify user
            {
                string dbgfile = "D:\\extractlogs\\EmailSendError" + ctx.Session["s_user"].ToString() + ".txt";
                StreamWriter sw = new StreamWriter(dbgfile, true); // append to file
                DateTime logTime = DateTime.Now;
                sw.WriteLine("LOGTIME:" + logTime.ToString("yyyyMMddHHmmss.ffff"));
                sw.Flush();

                try
                {
                    sw.WriteLine("From:" + inMessage.From.ToString());
                    sw.WriteLine("TO:" + inMessage.To.ToString());
                    sw.WriteLine("CC:" + inMessage.CC.ToString());
                    sw.WriteLine("Subject:" + inMessage.Subject.ToString());
                    sw.WriteLine("Body:" + inMessage.Body.ToString());
                    sw.WriteLine("ERROR:" + em1.Message);
                    sw.WriteLine("");
                }
                catch (Exception ea)
                {
                    sw.WriteLine("Error writing email info (send_email_message):" + ea.Message);
                }
                sw.Close();
                inMessage.Dispose();
                return false;
            }
        }
        /// <summary>
        /// This method returns the string value of the registry key at the prescribed path
        /// below computer\HKEY_LOCAL_MACHINE.
        /// </summary>
        /// <param name="keyFullPath"></param>
        /// <returns>
        /// The value of the string at the prescribed key location below HKEY_LOCAL_MACHINE; if
        /// the returned value is null the read attempt failed.
        /// </returns>
        public static string Read_HKEY_LOCAL_MACHINE(string keyFullPath)
        {
            HttpContext ctx = HttpContext.Current;
            string readValue = null;

            string keyDirPath = Path.GetDirectoryName(keyFullPath);
            string keyName = Path.GetFileName(keyFullPath);

            try
            {
                using (Microsoft.Win32.RegistryKey key = Registry.LocalMachine.OpenSubKey(keyDirPath))
                {
                    if (key != null)
                    {
                        Object obj = key.GetValue(keyName);
                        if (obj != null)
                        {
                            readValue = (string)obj;
                        }
                        else
                        {
                            readValue = "ERROR - SubKey not found";
                            //Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR: obj == null");
                        }
                    }
                    else
                    {
                        readValue = "ERROR - No value found for subkey";
                        //Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR : key == null");
                    }
                }
            }
            catch (Exception e)
            {
                readValue = "ERROR: exception2: " + e.Message;
                //Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR: exception: {0}", e.Message);
            }

            return readValue;
        }
        public static void log_environment()
        {
            HttpContext ctx = HttpContext.Current;

            string logfile = "D:\\extractlogs\\" + ctx.Session["s_user"].ToString() + "logenv.txt";

            DateTime logTime = DateTime.Now;

            StreamWriter sw = new StreamWriter(logfile, false);
            sw.WriteLine(logTime.ToLongDateString());

            foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                sw.WriteLine(env.Key.ToString() + " " + env.Value);
            }
            sw.Close();

        }
    }

    public class SessionInfo
    {
        protected string m_sesUID;
        protected string m_sesSchema;
        protected string m_sesSID;
        protected string m_sesNSID;
        protected string m_sesCLOSETYPE;
        protected string m_sesTIMEOUT;
        protected string m_sesPROJSTART;
        protected string m_sesCNSTR;
        protected string m_sesDEFPROJ;
        protected WindowsPrincipal m_sesWINPRIN;

        public string sesUID
        {
            get { return m_sesUID; }
            set { m_sesUID = value; }
        }

        public string sesSchema
        {
            get { return m_sesSchema; }
            set { m_sesSchema = value; }
        }

        public string sesSID
        {
            get { return m_sesSID; }
            set { m_sesSID = value; }
        }

        public string sesNSID
        {
            get { return m_sesNSID; }
            set { m_sesNSID = value; }
        }

        public string sesCLOSETYPE
        {
            get { return m_sesCLOSETYPE; }
            set { m_sesCLOSETYPE = value; }
        }

        public string sesTIMEOUT
        {
            get { return m_sesTIMEOUT; }
            set { m_sesTIMEOUT = value; }
        }
        public string sesPROJSTART
        {
            get { return m_sesPROJSTART; }
            set { m_sesPROJSTART = value; }
        }

        public string sesCNSTR
        {
            get { return m_sesCNSTR; }
            set { m_sesCNSTR = value; }
        }

        public string sesDEFPROJ
        {
            get { return m_sesDEFPROJ; }
            set { m_sesDEFPROJ = value; }
        }

        public WindowsPrincipal sesWINPRIN
        {
            get { return m_sesWINPRIN; }
            set { m_sesWINPRIN = value; }
        }

        public SessionInfo(string sesUID, string sesSchema, string sesSID,
                            string sesNSID, string sesCLOSETYPE, string sesTIMEOUT,
                            string sesPROJSTART, string sesCNSTR, string sesDEFPROJ,
                            WindowsPrincipal sesWINPRIN)
        {
            m_sesUID = sesUID;
            m_sesSchema = sesSchema;
            m_sesSID = sesSID;
            m_sesNSID = sesNSID;
            m_sesCLOSETYPE = sesCLOSETYPE;
            m_sesTIMEOUT = sesTIMEOUT;
            m_sesPROJSTART = sesPROJSTART;
            m_sesCNSTR = sesCNSTR;
            m_sesDEFPROJ = sesDEFPROJ;
            m_sesWINPRIN = sesWINPRIN;
        }
    }
}
```
