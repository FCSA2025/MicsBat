using _Configuration;
using _NewLib;
using _Utillib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TsipInitiator
{
    /// <summary>
    /// This class provides private members and public methods that encapsulate
    /// the data and functionality required to send an email to a WebMICS user
    /// whose attachments are TSIP output reports.
    /// </summary>
    public static class TsipEmail
    {
        private const string CLAVIS_SEMITA = @"SOFTWARE\AlphaSoft\PDFconverter\Local\Defaults\Nuntius";
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

        /// <summary>
        /// This method implements the sending of TSIP result reports to the user's
        /// email address using an SMTP server.
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static int Send(out string errMsg)
        {
            // 'out' requirement.
            errMsg = "";

            string subject = "";
            string body = "";

            int retVal = Constant.SUCCESS;

            try
            {
                bool IsDel;
                string cBodyfile = "";
                List<FileInfo> fiDeleteList = new List<FileInfo>();

                DateTime theNu = DateTime.Now;
                Console.WriteLine("\nSending TSIP reports to user via email with timestamp: {0}", theNu);

                if (mDelFlag == "D")
                {
                    //	The tsip run was deleted while executing or queued.
                }
                else
                {
                    cBodyfile = mTsipFileFolder + "\\" + mTsipFileRoot + ".ERR";
                    if (File.Exists(cBodyfile))
                    {
                        Console.Write("\nSending files:- {0}\n", mTsipFileFolder + "\\" + mTsipFileRoot + "_" + mRuns[0]);
                    }
                    else
                    {
                        errMsg = String.Format("Could not find files for: {0}", mTsipFileFolder + "\\" + mTsipFileRoot + "_" + mRuns[0]);
                        Console.Write("\n\n{0}\n", errMsg);
                        return 126;
                    }
                }

                // Specify the message content.
                StreamReader BodyFile = new StreamReader(cBodyfile);
                body = BodyFile.ReadToEnd();
                BodyFile.Close();

                // Construct a list of strings whose elements are paths to files to be
                // attached to the email.
                //List<string> attachmentFilePaths = new List<string>();

                // Construct a string to hold a list of hat separated file paths
                StringBuilder filelist = new StringBuilder("^^^",100);
                string hat = "";
                
                // Scan the body for the name of the first file in the err report
                int nInd = body.IndexOf("Proposed Name...........:") + 26;
                //	Now find the end of the file name.
                const int FILE_LENGTH = 32;
                char[] end_chars = { ' ', '\u000D', '\u000A' };
                int nInd1 = body.IndexOfAny(end_chars, nInd, nInd + FILE_LENGTH);
                if (nInd1 == -1)
                {
                    nInd1 = nInd + FILE_LENGTH;
                }
                String cFileName = body.Substring(nInd, nInd1 - nInd);
                 
                if (mDelFlag != "D")
                {
                    subject = "TSIP output for " + mTsipFileRoot + ", first filename: " + cFileName + " at " + theNu.ToString();

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
                                string str = fiFile.FullName;

                                filelist.Append(hat + fiFile.FullName);
                                hat = "^";

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
                        body = "No files found, this is the .ERR file:-\n\n";
                        BodyFile = new StreamReader(cBodyfile);
                        body += BodyFile.ReadToEnd();
                        BodyFile.Close();
                    }
                }
                else
                {
                    subject = "TSIP Run for " + mTsipFileRoot + " DELETED.";
                    body = "See Subject.";
                }

                // show the email message
                Console.WriteLine("To: ablesonb@icloud.com");
                Console.WriteLine("Subject: " + subject);
                Console.WriteLine("Body: " + body);
                Console.WriteLine("Files: " + filelist.ToString());


                // Send the email message.
                //send_email_sql("ablesonb@icloud.com", subject, body, filelist.ToString(), true, out errMsg);

                //if (retVal != Constant.SUCCESS)
                //{
                //    string msg = String.Format("\n\nTsipEmail.Send(): ERROR: call to MicsEmail.Send() FAILED, retVal = {0}\n{1}", retVal, Error.MsgForCode(retVal));
                //    Console.Write(msg);
                //    Log2.e(msg);
                //}

                //if (fiDeleteList.Count() > 0)
                //{
                //    foreach (FileInfo fi in fiDeleteList)
                //    {
                        //	The delete switch was set.  We have to save the file names and delete them
                        //	after sending because they are not included in the message until they are
                        //	sent, and so are kept open and locked.
                //        fi.Delete();
                //    }
                //    Console.WriteLine("Deleted {0} files.", fiDeleteList.Count());
                //}

            }
            catch (Exception e)
            {
                retVal = 314259;
                Log2.e("\n\nTsipEmail.Send(): ERROR: exception: " + e.Message);
                errMsg = e.Message + "\n" + e.StackTrace;
            }

            if (retVal == Constant.SUCCESS) Console.WriteLine("\nEmail sent.");

            return retVal;
        }
        /*
        public static bool send_email_sql(string ToAddress, string subject, string body, List<string> attachmentFilePaths, bool retval, out string errMsg)
        {
            string dbgfile = "D:\\MicsBatchLogs\\SendTsipEmailSql.txt";
            StreamWriter sw = new StreamWriter(dbgfile, false);
            DateTime logTime = DateTime.Now;
            sw.WriteLine("LOGTIME:" + logTime.ToString("yyyyMMddHHmmss.ffff"));

            // set mandatory FROM addres
            string From = "mics@fcsa.ca";

            // build SQL
            //sw.WriteLine("Before try");
            //string attachfiles = "\\\\EC2AMAZ-2013EDB\\REMICS-D\\inetpub\\remicsdev\\mics\\userdirs\\venn\\venn1\\tsip_tcomm2502a_TS1.HILO";

            if (From == "")
            {
                sw.WriteLine("From is empty");
                sw.Close();
                errMsg = "No From address was specified";
                return false;
            }
            else
            {
                sw.WriteLine(From.ToString());
            }


            if (ToAddress.ToString() == "")
            {
                sw.WriteLine("To is empty");
                sw.Close();
                errMsg = "No To address was specified";
                return false;
            }
            else
            {
                sw.WriteLine(To.ToString());
            }

            if (subject == "")
            {
                sw.WriteLine("Subject is empty");
                sw.Close();
                errMsg = "No Subject was specified";
                return false;
            }
            else
            {
                sw.WriteLine(subject);
            }

            if (Body == "")
            {
                sw.WriteLine("Body is empty");
                sw.Close();
                errMsg = "No Body was specified";
                return false;
            }
            else
            {
                sw.WriteLine(body);
                sw.Flush();
            }

            // split attachment list from body
            string body1;
            string body2;
            int ATTptr;

            ATTptr = body.IndexOf("^^^");
            if (ATTptr == -1) // no attachments
            {
                body1 = body;
                body2 = "";
            }
            else
            {
                body1 = body.ToString().Substring(0, ATTptr);
                body2 = body.ToString().Substring(ATTptr + 3).Replace("^", ";");
            }

            body = body1;
            sw.WriteLine(body1);
            sw.WriteLine(body2);
            sw.Flush();
            string strSql;

                       
            try
            {
                //string cnstr = "DSN=remicsdev;DATABASE=remicsdev;Trusted_Connection=yes";
                //using (OdbcConnection cn = new OdbcConnection(cnstr)

                 //cn.Open();

                        strSql = "INSERT INTO adm.t_EmailQueue " +
                                " (mailFrom, mailTo, mailCC, mailSubject, mailBody, mailAttachments) " +
                                " VALUES ('mics@fcsa.ca" +
                                "','" + ToAddress +
                                "','" + subject +
                                "','" + body +
                                "','" + body1 +
                                "','" + body2 + "')";

                        sw.WriteLine(strSql);
                        sw.Flush();
                        //string attachfiles = "\\\\EC2AMAZ-2013EDB\\REMICS-D\\inetpub\\remicsdev\\mics\\userdirs\\venn\\venn1\\area402.kml";
                        //string attachfiles = "\\\\EC2AMAZ-2013EDB\\REMICS-D\\inetpub\\remicsdev\\mics\\userdirs\\venn\\venn1\\tsip_tcomm2502a_TS1.HILO";
                        //sw.WriteLine("Attachments:" + inMessage.Attachments);

                        sw.WriteLine(strSql);
                        sw.Flush();

                        using (OdbcCommand sendmail = new OdbcCommand(strSql, TsipEmail.connection))
                
                    sendmail.ExecuteNonQuery();
                }

                sw.Close();
                return true;
            }
            }
            catch (Exception)   // write error log and notify user
            {
     
                 {
                     sw.WriteLine("From:" + inMessage.From);
                     sw.WriteLine("TO:" + inMessage.To);
                     sw.WriteLine("CC:" + inMessage.CC);
                     sw.WriteLine("mailSubject:" + inMessage.mailSubject);
                     sw.WriteLine("Body:" + inMessage.Body);
                     //sw.WriteLine("Attachments:" + inMessage.Body);
                     sw.WriteLine("ERROR:" + em1.Message);
                     sw.WriteLine("");
                 }
                 catch (Exception ea)
                 {
                     sw.WriteLine("Error writing email info (send_email_message2):" + ea.Message);
                 }
  
                sw.Close();
                //inMessage.Dispose();
                return false;
            }

        }
        */


    }
}
