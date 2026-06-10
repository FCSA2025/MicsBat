# Documented File: TsipEmail BACKUP.cs
**Repository Path:** `TsipInitiator\TsipEmail BACKUP.cs`
**Primary Layer:** `TsipInitiator`
**Namespace:** `TsipInitiator`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net.Mail;
using System.Configuration;
using _NewLib;
using _Configuration;
using System.Text;

namespace TsipInitiator
{
    /// <summary>
    /// This class provides private members and public methods that encapsulate
    /// the data and functionality required to send an email to a WebMICS user
    /// whose attachments are TSIP output reports.
    /// </summary>
    public static class TsipEmail
    {
        private static string mTsipFileFolder;
        private static string mTsipFileRoot;
        private static string mEmailAddress;
        private static string mDelFlag;
        private static List<string> mRuns;

        public static string TsipFileFolder
        {
            get { return mTsipFileFolder; }
            set { mTsipFileFolder = value; }
        }
        public static string TsipFileRoot
        {
            get { return mTsipFileRoot; }
            set { mTsipFileRoot = value; }
        }
        public static string EmailAddress
        {
            get { return mEmailAddress; }
            set { mEmailAddress = value; }
        }
        public static string DelFlag
        {
            get { return mDelFlag; }
            set { mDelFlag = value; }
        }
        public static List<string> Runs
        {
            get { return mRuns; }
            set { mRuns = value; }
        }

        /// <summary>
        /// This is the private static class constructor; it initializes all private
        /// string members to empty strings and instantiates a list of string that
        /// will hold the TSIP paramater file 'run' names.
        /// </summary>
        static TsipEmail()
        {
            mTsipFileFolder = "";
            mTsipFileRoot = "";
            mEmailAddress = "";
            mDelFlag = "";
            mRuns = new List<string>();
        }

        /// <summary>
        /// This method returns true if the currently set values of the email parameters
        /// are all valid.
        /// </summary>
        /// <returns></returns>
        public static bool EmailParametersAreValid()
        {
            bool result = true;

            if (
                String.IsNullOrWhiteSpace(mTsipFileFolder) ||
                String.IsNullOrWhiteSpace(mTsipFileRoot) ||
                String.IsNullOrWhiteSpace(mEmailAddress) ||
                String.IsNullOrWhiteSpace(mDelFlag) ||
                mRuns == null ||
                mRuns.Count == 0
                )
            {
                result = false;
            }

            return result;
        }

        /// <summary>
        /// This method returns an annotated, formatted, multi-line string that
        /// provides the current values of the static class's members.
        /// </summary>
        /// <returns></returns>
        public new static string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("\ntsipFileFolder = " + mTsipFileFolder);
            sb.Append("\ntsipFileRoot   = " + mTsipFileRoot);
            sb.Append("\nemailAddress   = " + mEmailAddress);
            sb.Append("\ndelFlag        = " + mDelFlag);

            if (mRuns == null)
            {
                sb.Append("\nRuns           = NULL");
            }
            else if (mRuns.Count == 0)
            {
                sb.Append("\nRuns           = Is empty.");
            }
            else
            {
                int n = 0;
                foreach (string run in mRuns)
                {
                    sb.Append(String.Format("\n    run[{0}] = {1}", n++, run));
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method attempts to send an email to a WebMICS user using the currently
        /// set member values and instantiating an SMTP client; the network's SMTP connection
        /// parameters are read from a configuration file that is expected to exist in the same
        /// directory as the executable and having a name like "TsipInitiator.exe.config"; <b>the 
        /// password value in the .config file should be obfuscated</b> and this method will read it
        /// and de-obfuscate it.
        /// </summary>
        /// <remarks>
        /// During the compilation of a C# application, Visual Studio looks for a file called 'app.config'
        /// in the same directory as the Project's .csproj file. The contents of this file are read and
        /// copied to a file (e.g. 'TsipInitiator.exe.config') alongside the application's executable (e.g. 
        /// 'TsipInitiator.exe') in the Project's build target folder.
        /// 
        /// The 'app.config' (and hence the 'TsipInitiator.exe.config') file should contain the following SMTP
        /// incantations:
        /// <code>
        /// <?xml version="1.0" encoding="utf-8"?>
        /// <configuration>
        ///   <startup>
        ///     <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5.2"/>
        ///   </startup>
        ///   <system.net>
        ///     <mailSettings>
        ///       <smtp deliveryMethod="Network" from="mics@fcsa.ca">
        /// 		<!-- The password is obfuscated. -->
        /// 		<network host="10.1.1.14" userName="mics@fcsa.ca" password=",6Hw~cL?"/>
        ///       </smtp>
        ///     </mailSettings>
        ///   </system.net>
        ///   <appSettings>
        ///     <add key="FromEmailAddress" value="mics@fcsa.ca"/>
        ///   </appSettings>
        /// </configuration>        /// </code>
        /// </remarks>
        /// <returns></returns>
        public static int Send()
        {
            // Check that the email parameters are all valid.
            if (!EmailParametersAreValid())
            {
                string str = ToString();
                Console.Write(str);
                Log2.e(str);
                Application.ExitQuietly(Error.TSIPSENDEMAILFAILED);
            }

            //...Log2.v(ToString());

            bool IsDel;
            string cBodyfile = "";
            List<FileInfo> fiDeleteList = new List<FileInfo>();

            DateTime theNu = DateTime.Now;
            Console.WriteLine("tsipemail {0}\n", theNu);

            if (mDelFlag == "D")
            {
                //	The tsip run was deleted while executing or queued.
            }
            else
            {
                cBodyfile = mTsipFileFolder + "\\" + mTsipFileRoot + ".ERR";
                if (File.Exists(cBodyfile))
                {
                    Console.WriteLine("Sending files:- {0}\n", mTsipFileFolder + "\\" + mTsipFileRoot + "_" + mRuns[0]);
                }
                else
                {
                    Console.WriteLine("Could not find files for: {0}\n", mTsipFileFolder + "\\" + mTsipFileRoot + "_" + mRuns[0]);
                    return 126;
                }
            }

            // Instantiate a SmtpClient object.
            // We will use the constructor that has no arguments; in this case, SmtpClient
            // reads its configuration parameters from the file TsipEmail.exe.config that
            // should exist in the same directory as TsipEmail.exe.
            // During Visual Studio's C# compilation, the contents of the App.config (in
            // the VS Project directory) are copied into the TsipEmail.exe.config in the
            // Project's build target directory.

            SmtpClient client = new SmtpClient();

            // Specify the e-mail sender.
            // Create a mailing address that includes a UTF8 character in the display name.
            // Get the 'from' address via the 'tag' FromEmailAddress that is explicitely set in the .config file.
            string cFromAddr = ConfigurationManager.AppSettings["FromEmailAddress"];

            // De-obfuscate the password.
            string obfPwd = client.Credentials.GetCredential(client.Host, client.Port, "Basic").Password;
            string pwd = Obfuscate.UnHide(obfPwd);

            Log2.v("\nTsipEmail.Main(): C: FromAddr            = " + cFromAddr);
            Log2.v("\nTsipEmail.Main(): C: DeliveryMethod      = " + client.DeliveryMethod);
            Log2.v("\nTsipEmail.Main(): C: EnableSsl           = " + client.EnableSsl);
            Log2.v("\nTsipEmail.Main(): C: Host                = " + client.Host);
            Log2.v("\nTsipEmail.Main(): C: UserName            = " + client.Credentials.GetCredential(client.Host, client.Port, "Basic").UserName);
            Log2.v("\nTsipEmail.Main(): C: Password (obfus.)   = " + obfPwd);
            Log2.v("\nTsipEmail.Main(): C: Password (en clair) = " + pwd);

            if (String.IsNullOrWhiteSpace(cFromAddr))
            {
                Console.WriteLine("\nPut the 'from' email address in the configuration file.\n");
                Log2.e("\nTsipEmail.Main(): ERROR: cFromAddr is null, empty or whitespace.");
                return 125;
            }

            MailAddress from = new MailAddress(cFromAddr);

            // Set destinations for the e-mail message.
            MailAddress to = new MailAddress(mEmailAddress);

            // Specify the message content.
            MailMessage message = new MailMessage(from, to);
            StreamReader BodyFile = new StreamReader(cBodyfile);
            message.Body = BodyFile.ReadToEnd();
            BodyFile.Close();

            //	Scan the body for the name of the first file in the err report
            int nInd = message.Body.IndexOf("Proposed Name...........:") + 26;
            //	Now find the end of the file name.
            const int FILE_LENGTH = 32;
            char[] end_chars = { ' ', '\u000D', '\u000A' };
            int nInd1 = message.Body.IndexOfAny(end_chars, nInd, nInd + FILE_LENGTH);
            if (nInd1 == -1)
            {
                nInd1 = nInd + FILE_LENGTH;
            }
            String cFileName = message.Body.Substring(nInd, nInd1 - nInd);


            if (mDelFlag != "D")
            {
                message.Subject = "TSIP output for " + mTsipFileRoot + ", first filename: " + cFileName + " at " + theNu.ToString();

                IsDel = mDelFlag.ToUpper() == "Y";
                DirectoryInfo dDir = new DirectoryInfo(mTsipFileFolder);
                if (dDir.Exists)
                {
                    int nEndmark = mTsipFileRoot.Length;

                    foreach (string run in mRuns)
                    {
                        FileInfo[] fiDirFiles = dDir.GetFiles(mTsipFileRoot + "_" + run + ".*");
                        foreach (FileInfo fiFile in fiDirFiles)
                        {
                            Attachment Att = new Attachment(fiFile.FullName);
                            if (!Att.Name.Trim().ToLower().EndsWith(".txt"))
                            {
                                Att.Name = Att.Name + ".txt";
                            }
                            message.Attachments.Add(Att);
                            if (IsDel)
                            {
                                fiDeleteList.Add(fiFile);
                            }
                        }
                    }
                    if (IsDel)
                    {
                        fiDeleteList.Add(new FileInfo(cBodyfile));
                    }

                }
                else
                {
                    //	Send the err file anyway.
                    message.Body = "No files found, this is the .ERR file:-\n\n";
                    BodyFile = new StreamReader(cBodyfile);
                    message.Body += BodyFile.ReadToEnd();
                    BodyFile.Close();
                }
            }
            else
            {
                message.Subject = "TSIP Run for " + mTsipFileRoot + " DELETED.";
                message.Body = "See Subject.";
            }

            // Send the email message.
            client.Send(message);

            // Clean up.
            message.Dispose();

            if (fiDeleteList.Count() > 0)
            {
                foreach (FileInfo fi in fiDeleteList)
                {
                    //	The delete switch was set.  We have to save the file names and delete them
                    //	after sending because they are not included in the message until they are
                    //	sent, and so are kept open and locked.
                    fi.Delete();
                }
                Console.WriteLine("Deleted {0} files.", fiDeleteList.Count());
            }

            Console.WriteLine("Done.");

            return 0;
        }





    }
}

```
