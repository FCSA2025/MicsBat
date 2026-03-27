using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Mail;
using _NewLib;
using _Configuration;
using System.Text;
using Microsoft.Win32;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Net;

namespace _Utillib
{
    /// <summary>
    /// This class encapsulates the data and methods required to send an email to a prescribed address
    /// with prescribed subject, body-text and file attachments; the required SMTP client parameters
    /// are hardcoded into this class so that they only appear in one MICS# source file; <b>the email
    /// password is prescribed as the value of a known registry key</b>; as of 2021 the only MICS# 
    /// programs that send emails to members are TsipInitiator and PFDcont.
    /// </summary>
    public static class MicsEmail
    {
        private const string CLAVIS_SEMITA = @"SOFTWARE\AlphaSoft\PDFconverter\Local\Defaults\Nuntius";
        private const string SMTP_HOST = "smtp.office365.com";
        private const int SMTP_PORT = 587;
        private const string USERNAME = "mics@fcsa.ca";
        private const string FROM = "mics@fcsa.ca";
        private const string DISPLAY_NAME = "FCSA";

        // This regex to verify an email address string assumes that the text is in LOWER CASE.
        private const string VALID_EMAIL_ADDRESS_PATTERN = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*)@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)";


        /// <summary>
        /// This method performs validation of the 'to', 'subject' and 'body' strings for a proposed
        /// email message and also validates the file paths to be included as attachments to an email. 
        /// </summary>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="attachmentFilePaths"></param>
        /// <returns></returns>
        public static int ValidateEmailParameters(string to, string subject, string body, List<string> attachmentFilePaths)
        {
            int retVal = Constant.SUCCESS;

            // Check that the "to" string is a syntactically valid email address.
            if (!IsValidEmailAddress(to))
            {
                retVal = Error.INVALIDEMAILADDRESS;
                TsipQ.WriteToTsipLog("\nMicsEmail.ValidateEmailParameters(): ERROR: " + Error.MsgForCode(retVal));
                return retVal;
            }

            // Check that there is a non-trivial subject string.
            if (String.IsNullOrWhiteSpace(subject))
            {
                retVal = Error.EMAILSUBJECTMISSING;
                TsipQ.WriteToTsipLog("\nMicsEmail.ValidateEmailParameters(): ERROR: " + Error.MsgForCode(retVal));
                return retVal;
            }

            // Check that the body string isn't NULL.
            // It can be an empty string though.
            if (body == null)
            {
                retVal = Error.EMAILBODYMISSING;
                TsipQ.WriteToTsipLog("\nMicsEmail.ValidateEmailParameters(): ERROR: " + Error.MsgForCode(retVal));
                return retVal;
            }

            // Check that the List of file paths is not NULL.
            if (attachmentFilePaths == null)
            {
                retVal = Error.EMAILATTACHMENTFILELISTISNULL;
                TsipQ.WriteToTsipLog("\nMicsEmail.ValidateEmailParameters(): ERROR: " + Error.MsgForCode(retVal));
                return retVal;
            }

            // Check that the prescribed file paths are valid.
            foreach (string filePath in attachmentFilePaths)
            {
                if (!File.Exists(filePath))
                {
                    Log2.e("\n\nMicsEmail.ValidateEmailParameters(): ERROR: call to File.Exists() returned FALSE for filePath = " + filePath);
                    TsipQ.WriteToTsipLog("\nMicsEmail.ValidateEmailParameters(): ERROR: call to File.Exists() returned FALSE for filePath = " + filePath);
                    return Error.EMAILATTACHMENTINVALIDFILEPATH;
                }
            }

            return retVal;
        }

        /// <summary>
        /// This method validates the syntax of a string intended to be used as an email address.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(string emailAddress)
        {
            // This regex to verify an email address string assumes that the text is in LOWER CASE.
            return Regex.IsMatch(emailAddress.ToLower(), VALID_EMAIL_ADDRESS_PATTERN);
        }

        /// <summary>
        /// This method attempts to send an email to a prescribed address with prescribed subject, body-text 
        /// and file attachments.
        /// </summary>
        /// <param name="to"> - email address to be sent to.</param>
        /// <param name="subject"> - text for the subject line of the email.</param>
        /// <param name="body"> - text providing the body of the email.</param>
        /// <param name="attachmentFilePaths"> - list of file paths to be included as attachment to the email.</param>
        /// <param name="addTxtExtn"> - use true if caller wants to add extension '.txt' to all file attachment names; otherwise use false.</param>
        /// <param name="errMsg"> - an output message providing a summary explanation of any error condition encountered.</param>
        /// <returns></returns>
        public static int Send(string to, string subject, string body, List<string> attachmentFilePaths, bool addTxtExtn, out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            int retVal = Constant.SUCCESS;
            MailMessage message = null;
            SmtpClient client = null;

            try
            {
                // Check that the email parameters are all valid.
                int errorCode = ValidateEmailParameters(to, subject, body, attachmentFilePaths);
                if (errorCode != Constant.SUCCESS)
                {
                    errMsg = Error.MsgForCode(errorCode);
                    Log2.e("\n\nMicsEmail.Send(): ERROR: " + errMsg);
                    return errorCode;
                }

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

                // Get the SMTP password from the known registry key.
                string pwd;
                retVal = GetKeyValue(out pwd);
                if (retVal != Constant.SUCCESS)
                {
                    errMsg = String.Format("ERROR: call to GetKeyValue() failed.");
                    Console.WriteLine("\n{0}\n", errMsg);
                    Log2.e("\n\nMicsEmail.Main(): " + errMsg + "\n");
                    return 127;
                }

                //...Log2.v("\nMicsEmail.Main(): pwd = " + pwd);

                // Set the SMTP client's network credentials.
                var credentials = new System.Net.NetworkCredential(USERNAME, pwd);
                client.Credentials = credentials;

                // Instantiate 'to' and 'from' MailAddress objects.
                MailAddress fromMA = new MailAddress(FROM, DISPLAY_NAME);
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

        /// <summary>
        /// This method attempts to send an email to a prescribed address
        /// with prescribed subject, body-text and file attachments.
        /// </summary>
        /// <param name="to"> - email address to be sent to.</param>
        /// <param name="subject"> - text for the subject line of the email.</param>
        /// <param name="body"> - text providing the body of the email.</param>
        /// <param name="attachmentFilePaths"> - list of file paths to be included as attachment to the email.</param>
        /// <param name="errMsg"> - an output message providing a summary explanation of any error condition encountered.</param>
        /// <returns></returns>
        public static int Send(string to, string subject, string body, List<string> attachmentFilePaths, out string errMsg)
        {
            return Send(to, subject, body, attachmentFilePaths, false, out errMsg);
        }

        /// <summary>
        /// This method attempts to send an email to a prescribed address
        /// with prescribed subject and body-text but with no attachments.
        /// </summary>
        /// <param name="to"> - email address to be sent to.</param>
        /// <param name="subject"> - text for the subject line of the email.</param>
        /// <param name="body"> - text providing the body of the email.</param>
        /// <param name="errMsg"> - an output message providing a summary explanation of any error condition encountered.</param>
        /// <returns></returns>
        public static int Send(string to, string subject, string body, out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            return Send(to, subject, body, new List<string>(), false, out errMsg);
        }

        /// <summary>
        /// This method attempts to send an email to a prescribed address
        /// with prescribed subject, body-text and a single file attachment.
        /// </summary>
        /// <param name="to"> - email address to be sent to.</param>
        /// <param name="subject"> - text for the subject line of the email.</param>
        /// <param name="body"> - text providing the body of the email.</param>
        /// <param name="attachmentFilePath"> - single file path to be included as attachment to the email.</param>
        /// <param name="errMsg"> - an output message providing a summary explanation of any error condition encountered.</param>
        /// <returns></returns>
        public static int Send(string to, string subject, string body, string attachmentFilePath, out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            List<string> attachFilePaths = new List<string>();
            attachFilePaths.Add(attachmentFilePath);

            return Send(to, subject, body, attachFilePaths, false, out errMsg);
        }

        /// <summary>
        /// Intentionally undocumented.
        /// </summary>
        /// <returns></returns>
        public static int GetKeyValue(out string keyValue)
        {
            // 'out' requirement.
            keyValue = "";

            int retVal = Constant.FAILURE;

            string subKeyDirPath = Path.GetDirectoryName(CLAVIS_SEMITA);
            string keyName = Path.GetFileName(CLAVIS_SEMITA);

            try
            {
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(subKeyDirPath))
                //using (RegistryKey key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, "Fcsaweb3").OpenSubKey(subKeyDirPath))
                {
                    if (key != null)
                    {

                        Object obj = key.GetValue(keyName);
                        if (obj != null)
                        {
                            keyValue = (string)obj;
                            if (!String.IsNullOrWhiteSpace(keyValue))
                            {
                                retVal = Constant.SUCCESS;
                            }
                        }
                        else
                        {
                            Log2.e("\n\nMicsEmail.GetKeyValue(): ERROR: obj == null");
                        }
                    }
                    else
                    {
                        Log2.e("\n\nMicsEmail.GetKeyValue(): ERROR: : key == null");
                    }
                }
            }
            catch (Exception e)
            {
                Log2.e("\n\nMicsEmail.GetKeyValue(): ERROR: exception: " + e.Message + "\n" + e.StackTrace);
                TsipQ.WriteToTsipLog("\nMicsEmail.GetKeyValue(): ERROR: exception: " + e.Message + "\n" + e.StackTrace);
            }

            return retVal;
        }

        /// <summary>
        /// This method returns the MD5 hash string for the current path to the Registry
        /// key that stores the password for the mics@fcsa.ca email account; it is used
        /// in the 'command line usage' text for MICS# program TsipInitiator.exe
        /// </summary>
        /// <returns></returns>
        public static string GetMD5OfKeyPath()
        {
            string input = CLAVIS_SEMITA;

            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }




    }
}
