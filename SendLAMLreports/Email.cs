using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SendLAMLreports
{
    /// <summary>
    /// This class provides methods that facilitate the sending of emails using the Amazon SES service interface
    /// </summary>
    public class Email
    {
        private const string USERNAME = "mics@fcsa.ca";
        private const string FROM = "mics@fcsa.ca";
        private const string DISPLAY_NAME = "FCSA";
        public const int SUCCESS = 0;
        public const int FAILURE = 1;

        // This regex to verify an email address string assumes that the text is in LOWER CASE.
        private const string VALID_EMAIL_ADDRESS_PATTERN = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*)@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)";

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
        public static int SendSql(string to, string subject, string body, List<string> attachmentFilePaths, bool addTxtExtn, out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            int retVal = SUCCESS;
            MailMessage message = null;
            SmtpClient client = null;

            try
            {
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
                client.SendSql(message);

                retVal = SUCCESS;
            }
            catch (Exception e)
            {
                retVal = FAILURE;

                errMsg = String.Format("ERROR: attempt to send email FAILED./n/nReason: {1}", e.Message);
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
        /// Intentionally undocumented.
        /// </summary>
        /// <returns></returns>
        public static int GetKeyValue(out string keyValue, out string errMsg)
        {
            // 'out' requirement.
            keyValue = "";
            errMsg = "";

            int retVal = FAILURE;

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
                                retVal = SUCCESS;
                            }
                        }
                        else
                        {
                            errMsg = "ERROR: GetKeyValue(): key.GetValue() returned NULL";
                        }
                    }
                    else
                    {
                        errMsg = "ERROR: GetKeyValue(): key is NULL";
                    }
                }
            }
            catch (Exception e)
            {
                errMsg = "ERROR: GetKeyValue(): Exception: " + e.Message;
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
            return Regex.IsMatch(emailAddress, VALID_EMAIL_ADDRESS_PATTERN, RegexOptions.IgnoreCase);
        }

        public class EmailMsg
        {
            protected string m_mailFrom;
            protected string m_mailTo;
            protected string m_mailCC;
            protected string m_mailSubject;
            protected string m_mailBody;
            protected string m_mailBodyFormat;
            protected string m_mailAttachments;
            protected char m_sentYN;
            protected DateTime m_SentDate;
            protected string m_ErrorMsg;
            protected int m_AttemptCount;
            protected DateTime m_LastAttempt;
            public string mailFrom
            {
                get { return m_mailFrom; }
                set { m_mailFrom = value; }
            }
            public string mailTo
            {
                get { return m_mailTo; }
                set { m_mailTo = value; }
            }
            public string mailCC
            {
                get { return m_mailCC; }
                set { m_mailCC = value; }
            }
            public string mailSubject
            {
                get { return m_mailSubject; }
                set { m_mailSubject = value; }
            }
            public string mailBody
            {
                get { return m_mailBody; }
                set { m_mailBody = value; }
            }
            public string mailBodyFormat
            {
                get { return m_mailBodyFormat; }
                set { m_mailBodyFormat = value; }
            }
            public string mailAttachments
            {
                get { return m_mailAttachments; }
                set { m_mailAttachments = value; }
            }
            public char sentYN
            {
                get { return m_sentYN; }
                set { m_sentYN = value; }
            }
            public DateTime SentDate
            {
                get { return m_SentDate; }
                set { m_SentDate = value; }
            }
            public string ErrorMsg
            {
                get { return m_ErrorMsg; }
                set { m_ErrorMsg = value; }
            }
            public int AttemptCount
            {
                get { return m_AttemptCount; }
                set { m_AttemptCount = value; }
            }
            public DateTime LastAttempt
            {
                get { return m_LastAttempt; }
                set { m_LastAttempt = value; }
            }




        }
}
