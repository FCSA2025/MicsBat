# Documented File: SMTP.cs
**Repository Path:** `SMTP\SMTP.cs`
**Primary Layer:** `SMTP`
**Namespace:** `SMTP`

## Source Code Representation
```csharp
﻿using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This application sends a test email to a prescribed email address using a prescribed 
/// SMTP server, sender email account and password.
/// </summary>
/// <remarks>
/// The command-line usage is:
/// \image html "Usage - SMTP.png" ""
/// </remarks>
namespace SMTP
{
    /// <summary>
    /// This class provides the Main() method for the SMTP.exe application.
    /// </summary>
    public class SMTP
    {
        private static string mSmtpHostUrl = "";
        private static int mSmtpPort = 0;
        private static bool mEnableSsl = true;
        private static string mSmtpAccount = "";
        private static string mSmtpPassword = "";
        private static string mFromAddress = "";
        private static string mToAddress = "";

        /// <summary>
        /// This is the Main() method of the SMTP.exe application.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                ParseCommandLineArgs(args);

                int retVal;
                string errMsg = "";

                if (String.IsNullOrWhiteSpace(mSmtpPassword))
                {
                    // Get the SMTP password from the known registry key.
                    string pwd;
                    retVal = MicsEmail.GetKeyValue(out pwd);
                    if (retVal != Constant.SUCCESS)
                    {
                        errMsg = String.Format("ERROR: call to GetKeyValue() failed.");
                        Console.WriteLine("\n{0}\n", errMsg);
                        Log2.e("\n\nMicsEmail.Main(): " + errMsg + "\n");
                        Application.ExitQuietly(1);
                    }
                    else
                    {
                        mSmtpPassword = pwd;
                        Console.Write("\nThe email account password passed in the command-line was blank,");
                        Console.Write("\nso the password stored in the registry will be used.");
                    }
                }

                string subject = "Zen and the Art of Motorcycle Maintenance.";
                string body = "This is the body.";

                string filePath1 = @"d:\MicsBatchLogs\attach1.txt";
                string filePath2 = @"d:\MicsBatchLogs\attach2.txt";
                string filePath3 = @"d:\MicsBatchLogs\attach3.txt";

                File.WriteAllText(filePath1, "One.");
                File.WriteAllText(filePath2, "Two.");
                File.WriteAllText(filePath3, "Three.");

                List<string> attachFilePaths = new List<string>();

                attachFilePaths.Add(filePath1);
                attachFilePaths.Add(filePath2);
                attachFilePaths.Add(filePath3);

                retVal = Send(mSmtpHostUrl, mSmtpPort, mSmtpAccount, mSmtpPassword, mFromAddress, mToAddress, subject, body, attachFilePaths, false, out errMsg);

                if (retVal == Constant.SUCCESS)
                {
                    Console.Write("\n\nCall to MicsEmail.Send() SUCCEEDED.");
                }
                else
                {
                    Console.Write("\n\nCall to MicsEmail.Send() FAILED, retVal = {0}\n\n{1}", retVal, errMsg);
                }

                // Clean Up.
                File.Delete(filePath1);
                File.Delete(filePath2);
                File.Delete(filePath3);

                Console.Write("\n\nNo Exceptions thrown.");
            }
            catch (Exception e)
            {
                Console.Write("\nERROR: Exception: " + e.Message);
            }
        }

        /// <summary>
        /// This method parses the command line arguments and uses their values to set
        /// various associated internal variables and flags.
        /// </summary>
        /// <param name="args"> - command line arguments.</param>
        private static void ParseCommandLineArgs(string[] args)
        {
            args = ParseForChangesToUpperCase(ref args);

            switch (args.Length)
            {
                case 6:
                    mSmtpHostUrl = args[0];
                    mSmtpPort = Convert.ToInt32(args[1]);
                    mEnableSsl = Convert.ToBoolean(args[2]);
                    mSmtpAccount = args[3];
                    mSmtpPassword = args[4];
                    mToAddress = args[5];
                    mFromAddress = mSmtpAccount;
                    break;
                default:
                    WriteUsageToConsole();

                    Environment.Exit(666);
                    break;
            }

            Console.Write("\n");
            Console.Write("\n          SmtpHostUrl  = {0}", mSmtpHostUrl);
            Console.Write("\n          SmtpPort     = {0}", mSmtpPort);
            Console.Write("\n          EnableSsl    = {0}", mEnableSsl);
            Console.Write("\n          SmtpAccount  = {0}", mSmtpAccount);
            Console.Write("\n          SmtpPassword = {0}", mSmtpPassword);
            Console.Write("\n          FromAddress  = {0}", mFromAddress);
            Console.Write("\n          ToAddress    = {0}", mToAddress);
            Console.Write("\n");
        }

        /// <summary>
        /// This method writes a 'usage' message to Console.Out that provides a 
        /// succinct summary of mandatory and optional arguments when the program
        /// is run from the Windows command line.
        /// </summary>
        public static void WriteUsageToConsole()
        {
            Console.Write("\r\n");
            Console.Write("\r\n This program sends a test email to a prescribed email address using a   +");
            Console.Write("\r\n prescribed SMTP server, sender email account and password.");
            Console.Write("\r\n");
            Console.Write("\r\n USAGE: SMTP <SmtpHostUrl> <SmtpPort> <EnableSsl> <SenderMailBox> <SenderPassword> <ToAddress>");
            Console.Write("\r\n ===== ");
            Console.Write("\r\n");
            Console.Write("\r\n        SmtpHostUrl      : URL of SMTP host, e.g. smtp.office365.com or 10.1.1.26");
            Console.Write("\r\n        SmtpPort         : SMTP port number to be used.");
            Console.Write("\r\n        EnableSsl        : Set to true is Tsl/Ssl is required by the SMTP server, otherwise false.");
            Console.Write("\r\n        SenderMailBox    : Email address of the sender's email account on the SMTP server.");
            Console.Write("\r\n        SenderPassword   : Password for the sender's email account:");
            Console.Write("\r\n                               - SEE NOTE BELOW w.r.t. case sensitivity.");
            Console.Write("\r\n                               - To use the email password in the registry enter \"\".");
            Console.Write("\r\n        ToAddress        : The recipient's email address.");
            Console.Write("\r\n");
            Console.Write("\r\n <...>  indicates a mandatory argument.\r\n");
            Console.Write("\r\n [...]  indicates an optional argument.");
            Console.Write("\r\n");
            Console.Write("\r\n NOTE: Windows command lines are case-insensitive and upper-case characters are passed");
            Console.Write("\r\n       via args[] as lower-case. Passwords are usually case-sensitive. To force a character");
            Console.Write("\r\n       to be handled as upper-case preceed it by the '\\' character.");
            Console.Write("\r\n       e.g. 'boAt' is passed as 'boat' but 'bo\\at' is passed as 'boAt'. ");
            Console.Write("\r\n");
            Console.Write("\r\n e.g.");
            Console.Write("\r\n     SMTP smtp.office365.com 587 true mics@fcsa.ca \\The\\Password chris_p.bacon@rogers.com");
            Console.Write("\r\n     SMTP smtp.office365.com 587 true mics@fcsa.ca \"\"            chris_p.bacon@rogers.com");
            Console.Write("\r\n");
            Console.Write("\r\n Notes:");
            Console.Write("\r\n       1. This program *must* be called with all qty. 6 command-line arguments.");
            Console.Write("\r\n       2. The 'from' address is set equal to the SenderMailBox address.");
            Console.Write("\r\n\r\n Build: {0}\r\n", CollateExeMetaData());
        }

        /// <summary>
        /// Returns a string giving the date and time at which the currently executing
        /// MICS program was compiled, 32 or 64-bit executable and Release or Debug build.
        /// e.g. "171019-1057/64-R"  =  Compiled at  10:57am on 19th Nov. 2017, 64-bit, Release version.
        /// </summary>
        /// <returns></returns>
        public static string CollateExeMetaData()
        {
            string result = "";

            Assembly assembly = Assembly.GetEntryAssembly();
            TimeZoneInfo target = null;

            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;       //The offset to PE signature is given as an Int32 starting at byte 60.
            const int c_SignatureOffset = 4;       //The letters P and E followed by two null bytes.
            const int c_MachineOffset = 0;         //Two bytes that encode the Machine type (x86 or x64).
            const int c_LinkerTimestampOffset = 4; //Offset from Signature.
            const UInt16 c_x64 = 0x8664;           //PE machine code for x86-64.
            const UInt16 c_x32 = 0x014c;           //PE machine code for x86.

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                stream.Read(buffer, 0, 2048);

            //Find the offset for the start of the PE header.
            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);

            //Get the Machine code as Int16.
            var machineCode = BitConverter.ToUInt16(buffer, offset + c_SignatureOffset + c_MachineOffset);
            string machine = "??";
            switch ((Int32)machineCode)
            {
                case (c_x32):
                    machine = "32";
                    break;
                case (c_x64):
                    machine = "64";
                    break;
            }

            //Get the linker's time-stamp.
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_SignatureOffset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            //The ?? operator is called the null-coalescing operator. 
            //It returns the left-hand operand if the operand is not null; 
            //otherwise it returns the right hand operand.
            var tz = target ?? TimeZoneInfo.Local;

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            //string yearStr = localTime.Year.ToString().Substring(2, 2);
            //string monthStr = String.Format("{0:00}", localTime.Month);
            //string dayStr = String.Format("{0:00}", localTime.Day);
            //string hourStr = String.Format("{0:00}", localTime.Hour);
            //string minuteStr = String.Format("{0:00}", localTime.Minute);

            //result += yearStr + monthStr + dayStr + "-" + hourStr + minuteStr;
            result = YYMMDD_HHMM(localTime);
            result += "/" + machine;
#if DEBUG
            result += "-D";
#else
            result += "-R";
#endif
            return result;
        }

        /// <summary>
        /// This method returns a string representing a prescribed date
        /// and time using the format YYMMDD_HHMM.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string YYMMDD_HHMM(DateTime dateTime)
        {
            StringBuilder sb = new StringBuilder();
            string yearStr = dateTime.Year.ToString().Substring(2, 2);
            string monthStr = String.Format("{0:00}", dateTime.Month);
            string dayStr = String.Format("{0:00}", dateTime.Day);
            string hourStr = String.Format("{0:00}", dateTime.Hour);
            string minuteStr = String.Format("{0:00}", dateTime.Minute);
            sb.Append(yearStr);
            sb.Append(monthStr);
            sb.Append(dayStr);
            sb.Append("-");
            sb.Append(hourStr);
            sb.Append(minuteStr);
            return sb.ToString();
        }

        /// <summary>
        /// The Windows command line is case insensitive and so if a mixed-case password
        /// is entered it is received by this program as all lower-case; to get around
        /// this limitation we must 'escape' each upper case password character with '\\';
        /// this method handles the parsing the password for escape characters and 
        /// conversion of the next character to upper case; so 'boAt' is passed as 'boat' 
        /// but 'bo\\at' is passed as 'boAt'. ");
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string[] ParseForChangesToUpperCase(ref string[] args)
        {
            string[] correctedArgs = new string[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                char[] chars = args[i].ToCharArray();
                bool convertToUpperCase = false;
                string str = "";

                for (int j = 0; j < chars.Length; j++)
                {
                    if (chars[j] == '\\')
                    {
                        convertToUpperCase = true;
                    }
                    else
                    {
                        char c = chars[j];

                        if (convertToUpperCase)
                        {
                            c = Char.ToUpper(c);
                            convertToUpperCase = false;
                        }

                        str += c;
                    }
                }

                correctedArgs[i] = str;
            }

            return correctedArgs;
        }


        /// <summary>
        /// This method attempts to send an email to a prescribed address with prescribed subject, body-text 
        /// and file attachments.
        /// </summary>
        /// <param name="SMTP_HOST"> URL of SMTP server, e.g. smtp.office365.com</param>
        /// <param name="SMTP_PORT"> SMTP server port number to use.</param>
        /// <param name="smtpAccount"> name of the account on the SMTP server.</param>
        /// <param name="pwd"> password for the account on the SMTP server.</param>
        /// <param name="from"> sender's email address.</param>
        /// <param name="to"> recipient's email address.</param>
        /// <param name="subject"> subject of the email.</param>
        /// <param name="body"> body text content of the email.</param>
        /// <param name="attachmentFilePaths"> list of file paths of files to be attached.</param>
        /// <param name="addTxtExtn"> boolean - if TRUE the extension '.txt' is added to the attachment file names to make it is easier for the member to open the attachments with a text editor.</param>
        /// <param name="errMsg"> returns a descriptive error message.</param>
        /// <returns></returns>
        public static int Send(string SMTP_HOST, int SMTP_PORT, string smtpAccount, string pwd, string from, string to, string subject, string body, List<string> attachmentFilePaths, bool addTxtExtn, out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            int retVal = Constant.SUCCESS;
            MailMessage message = null;
            SmtpClient client = null;

            try
            {
#if false
                // Check that the email parameters are all valid.
                int errorCode = ValidateEmailParameters(to, subject, body, attachmentFilePaths);
                if (errorCode != Constant.SUCCESS)
                {
                    errMsg = Error.MsgForCode(errorCode);
                    Log2.e("\n\nMicsEmail.Send(): ERROR: " + errMsg);
                    return errorCode;
                }
#endif
                // Instantiate a SmtpClient object using the constructor that requires the
                // SMTP host server's URL and the prescribed port number.
                client = new SmtpClient(SMTP_HOST, SMTP_PORT);

                // Transport Layer Security (TLS), the successor of the now-deprecated Secure Sockets Layer (SSL), 
                // is a cryptographic protocol designed to provide communications security over a computer network.
                // The SmtpClient class only supports the SMTP Service Extension for Secure SMTP over TLS as 
                // defined in RFC 3207. In this mode, the SMTP session begins on an unencrypted channel, then a 
                // STARTTLS command is issued by the client to the server to switch to secure communication using SSL.
                // Use of "smtp.office365.com" requires us to enable SSL in our SMTP client.
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.EnableSsl = true;

                //...Log2.v("\nMicsEmail.Main(): pwd = " + pwd);

                // Set the SMTP client's network credentials.
                var credentials = new System.Net.NetworkCredential(smtpAccount, pwd);
                client.Credentials = credentials;

                // Instantiate 'to' and 'from' MailAddress objects.
                MailAddress fromMA = new MailAddress(from, "FCSA");
                MailAddress toMA = new MailAddress(to);

                // Start to build the final email message.
                // Specify the message body content.
                message = new MailMessage(fromMA, toMA);
                message.Body = body;

                // Specify the subject of the email.
                message.Subject = subject;

                // Attach the prescribed files to the email message.
                foreach (string attachmentFilePath in attachmentFilePaths)
                {
                    Attachment Att = new Attachment(attachmentFilePath);

                    // If the filename does not have the extension '.txt' the caller can elect to add this so
                    // that it is easier for the member to open the attachments with a text editor.
                    if (addTxtExtn)
                    {
                        if (!Att.Name.Trim().ToLower().EndsWith(".txt"))
                        {
                            Att.Name = Att.Name + ".txt";
                        }
                    }

                    message.Attachments.Add(Att);

                }

                // Office 365 now requires the use of the TLS1.2 security protocol.
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // Send the email message.
                client.Send(message);

            }
            catch (Exception e)
            {
                retVal = Error.EMAILSENDATTEMPTFAILED;


                Log2.e("\n\nMicsEmail.Send(): ERROR: exception: " + e.Message);
                Log2.e("\n" + e.StackTrace);
                TsipQ.WriteToTsipLog("\n\nMicsEmail.Send(): ERROR: exception: " + e.Message + "\n" + e.StackTrace);

                //Check if inner exception is not null before accessing Message property
                //else, you may get Null Reference Excception
                if (e.InnerException != null)
                {
                    Log2.e("\nInner Exception : " + e.InnerException.Message + "\n" + e.InnerException.StackTrace);
                    TsipQ.WriteToTsipLog("\nInner Exception : " + e.InnerException.Message + "\n" + e.InnerException.StackTrace);
                }

                errMsg = e.Message;
            }
            finally
            {
                // Clean up.
                if (message != null) message.Dispose();
                if (client != null) client.Dispose();
            }

            return retVal;
        }

    }
}

```
