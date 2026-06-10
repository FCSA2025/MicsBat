# Documented File: ErrorUtils.cs
**Repository Path:** `utilities\ErrorUtils.cs`
**Primary Layer:** `utilities`
**Namespace:** `ErrorUtilities`

## Source Code Representation
```csharp
﻿using SesUtilities;
using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace ErrorUtilities
{
    public sealed class ErrorUtils
    {
        // All methods are static, so this can be private
        private ErrorUtils()
        { }

        // Log an Exception
        public static void LogException(Exception exc, string source)
        {
            // Include enterprise logic for logging exceptions
            HttpContext ctx = HttpContext.Current;

            string logFile = "App_Data/ErrorLog.txt";
            logFile = HttpContext.Current.Server.MapPath(logFile);

            // Open the log file for append and write the log
            StreamWriter sw = new StreamWriter(logFile, true);
            sw.WriteLine("********** {0} **********", DateTime.Now);
            sw.WriteLine("From ErrorUtils");
            if (exc.InnerException != null)
            {
                sw.Write("Inner Exception Type: ");
                sw.WriteLine(exc.InnerException.GetType().ToString());
                sw.Write("Inner Exception: ");
                sw.WriteLine(exc.InnerException.Message);
                sw.Write("Inner Source: ");
                sw.WriteLine(exc.InnerException.Source);
                if (exc.InnerException.StackTrace != null)
                {
                    sw.WriteLine("Inner Stack Trace: ");
                    sw.WriteLine(exc.InnerException.StackTrace);
                }
            }
            sw.Write("Exception Type: ");
            sw.WriteLine(exc.GetType().ToString());
            sw.WriteLine("Exception: " + exc.Message);
            sw.WriteLine("Source: " + source);
            sw.WriteLine("Stack Trace: ");
            if (exc.StackTrace != null)
            {
                sw.WriteLine(exc.StackTrace);
                sw.WriteLine();
            }
            sw.Close();
        }

        // Notify System Operators about an exception
        public static void NotifySystemOps(Exception exc, string source)
        {
            string logFile;
            string site_type = "site unknown";

            try
            {
                HttpContext ctx = HttpContext.Current;
                site_type = ctx.Application["site_type"].ToString();
                logFile = "App_Data/ErrorLog.txt";
                logFile = HttpContext.Current.Server.MapPath(logFile);
            }
            catch
            {
                logFile = "D:\\extractlogs\\ErrorLog.txt";
            }
            MailMessage Message = new MailMessage();

            Message.Subject = "MICS ERROR - " + site_type;
            StringBuilder mb = new StringBuilder("", 1000);
            mb.Append("THIS INFO WAS ALSO WRITTEN TO: " + logFile);
            mb.Append(Environment.NewLine);
            mb.Append("********** " + DateTime.Now.ToString() + " **********");
            mb.Append(Environment.NewLine);
            mb.Append("From ErrorUtils");
            mb.Append(Environment.NewLine);

            if (exc.InnerException != null)
            {
                mb.Append("Inner Exception Type: " + exc.InnerException.GetType().ToString());
                mb.Append(Environment.NewLine);
                mb.Append("Inner Exception: " + exc.InnerException.Message);
                mb.Append(Environment.NewLine);
                mb.Append("Inner Source: " + exc.InnerException.Source);
                mb.Append(Environment.NewLine);
                if (exc.InnerException.StackTrace != null)
                {
                    mb.Append("Inner Stack Trace: " + exc.InnerException.StackTrace);
                    mb.Append(Environment.NewLine);
                }
            }
            mb.Append("Exception Type: " + exc.GetType().ToString());
            mb.Append(Environment.NewLine);
            mb.Append("Exception: " + exc.Message);
            mb.Append(Environment.NewLine);
            mb.Append("Source: " + source);
            mb.Append(Environment.NewLine);
            mb.Append("Stack Trace: ");
            mb.Append(Environment.NewLine);
            if (exc.StackTrace != null)
            {
                mb.Append(exc.StackTrace);
                mb.Append(Environment.NewLine);
            }

            Message.Body = mb.ToString();

            if (site_type.IndexOf("micsdev") == -1 && site_type.IndexOf("micstest") == -1)  // send to fcsa if not in micsdev 
            {
                SesUtils.send_email_message2(Message, 1, true);     // fcsa and venn
            }
            else
            {
                SesUtils.send_email_message2(Message, 3, false);    // venn only
            }

        }
    }
}

```
