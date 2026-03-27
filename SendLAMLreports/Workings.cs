using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SendLAMLreports
{
    /// <summary>
    /// An instance of this class provides the top-level functionality of the 
    /// application's main window.
    /// </summary>
    public partial class MainForm : Form
    {
        public const int NUM_CSV_FIELDS = 6;
        public const string MSGBOX_TITLE = "SendLAMLreports";
        public const string LAML_REPORTS_HOME = @"D:\Reports\LicensedAndMissingLinks";
        public const string CSV_LOG_FILES_HOME = @"D:\prod\files";

        // This regex defines a valid Excel file name by its extension.
        public const string EXCEL_FILE_NAME_REGEX_PATTERN = @"[^.]\.(xlsm|xlsx|xls)$";
        // This regex defines a valid date string.
        private const string VALID_DATE_STRING_PATTERN = @"\d\d\d\d[-]{0,1}\d\d[-]{0,1}\d\d";

        private const string PATH_TO_CSV_DATA_FILE = CSV_LOG_FILES_HOME + @"\SendLAMLreports.csv";
        private const string PATH_TO_LOG_FILE = CSV_LOG_FILES_HOME + @"\SendLAMLreports.log";

        private static string mPathToReportsDirectory = LAML_REPORTS_HOME;

        private static Dictionary<string, Record> mRecordsDict;
        private static Dictionary<string, string> mReportFilePathsDict;

        private static string mCheckSumLastSave = "";

        private static string mErrMsg = "";
        private static MainForm mThisForm;

        private static List<string> mMembers = new List<string>() { "ABCCOM", "ALIANT", "BCHY", "BELL", "BMCE", "BRAGG", "DND", "GLW", "HYONE", "HYQU", "MTS", "NAVI", "NTTEL", "NWT", "ONT", "RCTL", "SHAW", "STEL", "TBAY", "TERAGO", "TLUSAB", "TLUSBC", "TLUSMC", "TLUSQC", "VDTR", "WIREIE", "XCI", "ZAYO" };

        /// <summary>
        /// This method is called before the Windows Form is displayed and reads record data
        /// in from a CSV text file whose path is unque to this application; if the CSV data
        /// file does not exist then this method creates one and populates it with essential
        /// initial data.
        /// </summary>
        private static void ReadDataFromFile()
        {
            try
            {
                // If this is the first use of this application by a Windows user then the
                // associated CSV data file will not yet exist. If this is the case then create
                // the data file and populate it with initial (default) content.
                if (!File.Exists(PATH_TO_CSV_DATA_FILE))
                {
                    TextWriter tw = File.CreateText(PATH_TO_CSV_DATA_FILE);
                    tw.Write(CreateInitalDataFileContents());
                    tw.Close();

                    MessageBox.Show("A new CSV data file has been created to contain 'saved' data.\npath = " + PATH_TO_CSV_DATA_FILE, MSGBOX_TITLE);
                }

                // So the CSV data file exists; read its contents line-by-line.
                string[] lines = File.ReadAllLines(PATH_TO_CSV_DATA_FILE);

                string[] fields;
                int numFieldsRead;
                int lineNum = 1;
                foreach (string line in lines)
                {
                    // Skip over any blank lines.
                    if (String.IsNullOrWhiteSpace(line)) continue;

                    numFieldsRead = CSV.ParseFields(line, out fields);

                    if (numFieldsRead != NUM_CSV_FIELDS)
                    {
                        mErrMsg = String.Format("ERROR: Line# {0} of CSV data file has {1} fields: should have {2}.\n\nPath = {3}", lineNum, numFieldsRead, NUM_CSV_FIELDS, PATH_TO_CSV_DATA_FILE);
                        TerminateWithMessage(mErrMsg);
                    }

                    Record record;
                    if (mRecordsDict.TryGetValue(fields[0], out record))
                    {
                        record.SendEmailYesNo.Checked = (fields[1].ToUpper().Equals("TRUE")) ? true : false;
                        record.EmailAddress.Text = fields[2];
                        record.FirstLine.Text = fields[3];
                        record.DateTimeLastSent = DateTime.Parse(fields[4]);
                        record.PathToReportLastSent = fields[5];
                    }
                    else
                    {
                        mErrMsg = String.Format("ERROR: line# {0} of data file: unknown member/operator: {1}", lineNum, fields[0]);
                        TerminateWithMessage(mErrMsg);
                    }

                    lineNum++;
                }

                mCheckSumLastSave = CalculateCheckSum();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Workings.ReadDataFromFile(): ERROR: exception: {0}\n{1}", e.Message, e.StackTrace), MainForm.MSGBOX_TITLE);
            }

        }

        /// <summary>
        /// This method returns the MD5 checksum of the concatanation of all current field values of
        /// all of the records used in this application as a 32-digit hexadecimal string; this
        /// is an expedient way of determining if any field values have changed compared to a
        /// previous time.
        /// </summary>
        /// <returns></returns>
        private static string CalculateCheckSum()
        {
            string checkSum = "";

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                sb.Append(kvp.Value.RecordToStringAsCSV());
            }

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                checkSum = BytesToHexString(hashBytes);
            }

            return checkSum;
        }

        /// <summary>
        /// This method returns the MD5 checksum of the concatonation of all strings in a precribed
        /// List as a 32-digit hexadecimal string..
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        private static string CalculateCheckSum(List<string> lines)
        {
            string checkSum = "";

            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                sb.Append(line);
            }

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                checkSum = BytesToHexString(hashBytes);
            }

            return checkSum;
        }

        /// <summary>
        /// This method converts an array of Byte values into a hexadecimal-encoded string.
        /// </summary>
        /// <param name="arrInput"></param>
        /// <returns></returns>
        public static string BytesToHexString(byte[] arrInput)
        {
            int i;
            StringBuilder sOutput = new StringBuilder(arrInput.Length);
            for (i = 0; i < arrInput.Length; i++)
            {
                sOutput.Append(arrInput[i].ToString("X2"));
            }

            return sOutput.ToString();
        }

        /// <summary>
        /// This method writes the application's current records data to a CSV formatted
        /// text file whose path unique to this application.
        /// </summary>
        private static void SaveDataToFile()
        {
            try
            {
                // Open the data file for writing; overwrite if it already exists.
                TextWriter tw = new StreamWriter(PATH_TO_CSV_DATA_FILE, false);

                foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
                {
                    tw.WriteLine(kvp.Value.RecordToStringAsCSV());
                }

                tw.Close();

                // Calculate and store the new checksum.
                mCheckSumLastSave = CalculateCheckSum();
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Workings.SaveDataToFile(): ERROR: exception: {0}\n{1}", mErrMsg, e.Message, e.StackTrace), MainForm.MSGBOX_TITLE);
            }
        }

        /// <summary>
        /// This method returns a string that provides initial essential content for the
        /// Windows CSV text file that the application uses to save the state of the previous
        /// user-session.
        /// </summary>
        /// <returns></returns>
        private static string CreateInitalDataFileContents()
        {
            StringBuilder sb = new StringBuilder();

            foreach (string member in MainForm.mMembers)
            {
                sb.Append(String.Format("\"{0}\", \"false\", \"\", \"\",  \"{1}\", \"\"\n", member, DateTime.MinValue.ToString("s")));
            }

            return sb.ToString();
        }

        /// <summary>
        /// This method cleanly terminates the execution of the application's main thread.
        /// </summary>
        private static void Terminate()
        {
            Application.Exit();
        }

        /// <summary>
        /// This method displays a MessageBox with a prescribed message and then cleanly 
        /// terminates the execution of the application's main thread.
        /// </summary>
        /// <param name="errMsg"></param>
        private static void TerminateWithMessage(string errMsg)
        {
            MessageBox.Show(errMsg, MSGBOX_TITLE);
            Application.Exit();
        }

        /// <summary>
        /// This method creates and fully populates a dictionary of <string, Record> entries
        /// where the key string is a member's 'oper' name (like RCTL etc).
        /// </summary>
        /// <param name="recordsDict"></param>
        private static void CreateDictionayOfRecordsMappedToGUI(out Dictionary<string, Record> recordsDict)
        {
            recordsDict = new Dictionary<string, Record>();

            Record record;

            record = new Record("ABCCOM", mThisForm.cbABCCOM, mThisForm.tbABCCOMemadd, mThisForm.tbABCCOM1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("ABCCOM", record);

            record = new Record("ALIANT", mThisForm.cbALIANT, mThisForm.tbALIANTemadd, mThisForm.tbALIANT1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("ALIANT", record);

            record = new Record("BCHY", mThisForm.cbBCHY, mThisForm.tbBCHYemadd, mThisForm.tbBCHY1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("BCHY", record);

            record = new Record("BELL", mThisForm.cbBELL, mThisForm.tbBELLemadd, mThisForm.tbBELL1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("BELL", record);

            record = new Record("BMCE", mThisForm.cbBMCE, mThisForm.tbBMCEemadd, mThisForm.tbBMCE1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("BMCE", record);

            record = new Record("BRAGG", mThisForm.cbBRAGG, mThisForm.tbBRAGGemadd, mThisForm.tbBRAGG1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("BRAGG", record);

            record = new Record("DND", mThisForm.cbDND, mThisForm.tbDNDemadd, mThisForm.tbDND1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("DND", record);

            record = new Record("GLW", mThisForm.cbGLW, mThisForm.tbGLWemadd, mThisForm.tbGLW1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("GLW", record);

            record = new Record("HYONE", mThisForm.cbHYONE, mThisForm.tbHYONEemadd, mThisForm.tbHYONE1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("HYONE", record);

            record = new Record("HYQU", mThisForm.cbHYQU, mThisForm.tbHYQUemadd, mThisForm.tbHYQU1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("HYQU", record);

            record = new Record("MTS", mThisForm.cbMTS, mThisForm.tbMTSemadd, mThisForm.tbMTS1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("MTS", record);

            record = new Record("NAVI", mThisForm.cbNAVI, mThisForm.tbNAVIemadd, mThisForm.tbNAVI1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("NAVI", record);

            record = new Record("NTTEL", mThisForm.cbNTTEL, mThisForm.tbNTTELemadd, mThisForm.tbNTTEL1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("NTTEL", record);

            record = new Record("NWT", mThisForm.cbNWT, mThisForm.tbNWTemadd, mThisForm.tbNWT1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("NWT", record);

            record = new Record("ONT", mThisForm.cbONT, mThisForm.tbONTemadd, mThisForm.tbONT1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("ONT", record);

            record = new Record("RCTL", mThisForm.cbRCTL, mThisForm.tbRCTLemadd, mThisForm.tbRCTL1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("RCTL", record);

            record = new Record("SHAW", mThisForm.cbSHAW, mThisForm.tbSHAWemadd, mThisForm.tbSHAW1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("SHAW", record);

            record = new Record("STEL", mThisForm.cbSTEL, mThisForm.tbSTELemadd, mThisForm.tbSTEL1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("STEL", record);

            record = new Record("TBAY", mThisForm.cbTBAY, mThisForm.tbTBAYemadd, mThisForm.tbTBAY1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("TBAY", record);

            record = new Record("TERAGO", mThisForm.cbTERAGO, mThisForm.tbTERAGOemadd, mThisForm.tbTERAGO1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("TERAGO", record);

            record = new Record("TLUSAB", mThisForm.cbTLUSAB, mThisForm.tbTLUSABemadd, mThisForm.tbTLUSAB1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("TLUSAB", record);

            record = new Record("TLUSBC", mThisForm.cbTLUSBC, mThisForm.tbTLUSBCemadd, mThisForm.tbTLUSBC1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("TLUSBC", record);

            record = new Record("TLUSMC", mThisForm.cbTLUSMC, mThisForm.tbTLUSMCemadd, mThisForm.tbTLUSMC1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("TLUSMC", record);

            record = new Record("TLUSQC", mThisForm.cbTLUSQC, mThisForm.tbTLUSQCemadd, mThisForm.tbTLUSQC1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("TLUSQC", record);

            record = new Record("VDTR", mThisForm.cbVDTR, mThisForm.tbVDTRemadd, mThisForm.tbVDTR1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("VDTR", record);

            record = new Record("WIREIE", mThisForm.cbWIREIE, mThisForm.tbWIREIEemadd, mThisForm.tbWIREIE1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("WIREIE", record);

            record = new Record("XCI", mThisForm.cbXCI, mThisForm.tbXCIemadd, mThisForm.tbXCI1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("XCI", record);

            record = new Record("ZAYO", mThisForm.cbZAYO, mThisForm.tbZAYOemadd, mThisForm.tbZAYO1stLine, DateTime.MinValue, PATH_TO_CSV_DATA_FILE);
            recordsDict.Add("ZAYO", record);
        }

        /// <summary>
        /// This method returns true if all of the user-entered record fields, for all of the currently
        /// selected records are sufficient, and valid, to successfully send email(s) with attached reports.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private bool UserDataIsOKforSelectedMembers(out string msg)
        {
            msg = "";

            string std = "Check FAILED.\n\n";

            // Check that the TextBox that prescribes the path to the reports folder is not blank.
            if (String.IsNullOrWhiteSpace(tbDirectoryPath.Text))
            {
                msg = String.Format("{0}You must enter the path to the folder containing the reports.", std);
                return false;
            }

            // Check that the path to the reports folder exists and is a directory.
            if (!Directory.Exists(mPathToReportsDirectory))
            {
                msg = String.Format("{0}The path to the folder containing the LAML reports does not exist.", std);
                return false;
            }

            // Check if mPathToReportsDirectory contains any Excel files.
            string[] excelReportFilePaths = GetFilePathsMatchingRegexPattern(EXCEL_FILE_NAME_REGEX_PATTERN);
            if (excelReportFilePaths.Length == 0)
            {
                msg = String.Format("{0}The prescribed reports folder does not contain any EXCEL files.", std);
                return false;
            }

            // Populate the dictionary of <member, reportFilePaths>. 
            PopulateReportFilePathsDict(excelReportFilePaths);

            // Check that every member set to receive an emailed report has a path to its report file.
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    string reportFilePath;
                    if (!mReportFilePathsDict.TryGetValue(kvp.Value.Member, out reportFilePath))
                    {
                        msg = String.Format("{0}There is no Excel report file for member '{1}' in folder\n{2}.", std, kvp.Value.Member, mPathToReportsDirectory);
                        return false;
                    }
                }
            }

            // Check that every member set to receive an emailed report has a valid email address and first line. 
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    if (String.IsNullOrWhiteSpace(kvp.Value.EmailAddress.Text))
                    {
                        msg = String.Format("{0}No email address has been entered for member '{1}'.", std, kvp.Value.Member);
                        return false;
                    }

                    if (!Email.IsValidEmailAddress(kvp.Value.EmailAddress.Text))
                    {
                        msg = String.Format("{0}The email address entered for member '{1}' has invalid syntax.", std, kvp.Value.Member);
                        return false;
                    }

                    if (String.IsNullOrWhiteSpace(kvp.Value.FirstLine.Text))
                    {
                        msg = String.Format("{0}No salutation (first line of email) has been entered for member '{1}'.", std, kvp.Value.Member);
                        return false;
                    }
                }
            }

            // If we get here then the user data checks out OK.
            return true;
        }

        /// <summary>
        /// This method returns an array of paths of files that currently reside in the
        /// currently-selected reports directory and that match to a prescribed Regex pattern 
        /// (having a valid Excel extension).
        /// </summary>
        /// <param name="regexPattern"></param>
        /// <returns></returns>
        private static string[] GetFilePathsMatchingRegexPattern(string regexPattern)
        {

            string[] allFileNames;
            List<string> fileNamesMatchingRegexPattern = new List<string>();

            allFileNames = Directory.GetFiles(mPathToReportsDirectory);

            foreach (string fileName in allFileNames)
            {
                if (Regex.Match(fileName, regexPattern, RegexOptions.IgnoreCase).Success)
                {
                    fileNamesMatchingRegexPattern.Add(fileName);
                }
            }

            return fileNamesMatchingRegexPattern.ToArray();
        }

        /// <summary>
        /// This method updates the contents of the application's CSV data file
        /// following actual sending of emails; only those operator records that
        /// were selected to receive email get updates to their 'path to report file sent' 
        /// and 'date email last sent' fields.
        /// </summary>
        private static void UpdateOnlyLastSavedFieldsInDataFile()
        {
            try
            {
                List<string> newLines = new List<string>();
                Record record;
                string currentDateTimeStr, currentReportPath;
                bool dateTimeChanged, reportPathChanged;

                // The CSV data file should already exist; read its contents line-by-line.
                string[] lines = File.ReadAllLines(PATH_TO_CSV_DATA_FILE);

                string[] fields;
                foreach (string line in lines)
                {
                    if (String.IsNullOrWhiteSpace(line)) continue;

                    CSV.ParseFields(line, out fields);

                    record = mRecordsDict[fields[0]];

                    currentDateTimeStr = record.DateTimeLastSent.ToString("s");
                    currentReportPath = record.PathToReportLastSent.Trim();

                    dateTimeChanged = !fields[4].Equals(currentDateTimeStr);
                    reportPathChanged = !fields[5].ToUpper().Equals(currentReportPath.ToUpper());

                    if (dateTimeChanged || reportPathChanged)
                    {
                        StringBuilder sb = new StringBuilder();

                        sb.Append(String.Format("\"{0}\" ,", fields[0]));
                        sb.Append(String.Format("\"{0}\" ,", fields[1]));
                        sb.Append(String.Format("\"{0}\" ,", fields[2]));
                        sb.Append(String.Format("\"{0}\" ,", fields[3]));
                        sb.Append(String.Format("\"{0}\" ,", currentDateTimeStr));
                        sb.Append(String.Format("\"{0}\"", currentReportPath));

                        newLines.Add(sb.ToString());
                    }
                    else
                    {
                        newLines.Add(line);
                    }

                }

                File.WriteAllLines(PATH_TO_CSV_DATA_FILE, newLines);

                // Calculate and store the new checksum.
                mCheckSumLastSave = CalculateCheckSum(newLines);
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Workings.SaveDataToFile(): ERROR: exception: {0}\n{1}", mErrMsg, e.Message, e.StackTrace), MainForm.MSGBOX_TITLE);
            }
        }

        /// <summary>
        /// This method is responsible for sending emails, with reports attached, to the 
        /// currently-selected member operators; the method provides several stages of 'fail-safe'
        /// state checking and user-interaction to reduce the possibility of emails being sent
        /// unintentionally.
        /// </summary>
        private void SendEmailToSelectedMembers()
        {
            string msg = "";
            DialogResult result;

            // Check if any member-operators have been selected using the radio buttons
            // on the left-hand side. If not, give a message and return.
            bool someMembersSelected = false;
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    someMembersSelected = true;
                    break;
                }
            }

            if (!someMembersSelected)
            {
                MessageBox.Show("You need to select at least one member to recieve email.", "Select members");
                return;
            }

            // Check whether the user-entered data is valid.
            // If it isn't valid show the appropriate error message and return.
            bool dataIsOK = UserDataIsOKforSelectedMembers(out mErrMsg);
            if (!dataIsOK)
            {
                MessageBox.Show(mErrMsg);
                return;
            }

            // Check whether any of the selected members has received the report file
            // before, and how long ago. Ask user what to do: proceed or cancel?
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                string reportFilePath = "";
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    // Get report file path and date that we are attempting to send.
                    reportFilePath = mReportFilePathsDict[kvp.Value.Member];

                    // Compare the above with the file path and dateTime last read in from the 
                    // data file.
                    if (reportFilePath.Equals(kvp.Value.PathToReportLastSent))
                    {
                        TimeSpan timeSpan = DateTime.Now.Subtract(kvp.Value.DateTimeLastSent);

                        double numDays = timeSpan.TotalDays;
                        double numHours = timeSpan.TotalHours;
                        double numMinutes = timeSpan.TotalMinutes;
                        double numSeconds = timeSpan.TotalSeconds;

                        StringBuilder sb = new StringBuilder();
                        sb.Append("A report with the same file path was previously sent to " + " " + kvp.Value.Member);
                        if (numDays >= 1)
                        {
                            msg += Math.Floor(numDays).ToString() + " day(s) ago.";
                        }
                        else if (numHours >= 1)
                        {
                            msg += Math.Floor(numHours).ToString() + " hour(s) ago.";
                        }
                        else if (numMinutes >= 1)
                        {
                            msg += Math.Floor(numMinutes).ToString() + " minute(s) ago.";
                        }
                        else
                        {
                            msg += Math.Floor(numSeconds).ToString() + " second(s) ago.";
                        }

                        sb.Append("\n" + msg);
                        sb.Append("\n\nDo you want to proceed with sending this email?");
                        sb.Append("\n\nOK     =   send this (and all other) previously sent report(s).");
                        sb.Append("\n\nCANCEL = go back and check the path to the reports folder!");

                        result = MessageBox.Show(sb.ToString(), "Re-sending the same report ?",
                             MessageBoxButtons.OKCancel,
                             MessageBoxIcon.Question);

                        if (result == DialogResult.OK) break;
                        if (result == DialogResult.Cancel) return;
                    }
                }
            }

            // So the user-entered data is good.
            // Ask the user again, one final time, whether he wants to send emails,
            // with attached reports, to the selected members.
            string sendToList = "";
            int numMembersToSendTo = 0;
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    sendToList += ", " + kvp.Value.Member;
                    numMembersToSendTo++;
                }
            }
            sendToList = sendToList.Substring(2, sendToList.Length - 2) + ".";

            msg = String.Format("Emails, with reports attached, will be sent to the following \nqty. {0} members:\n\n{1}\n\nDo you wish to proceed?",
                                            numMembersToSendTo, sendToList);
            string caption = "Ready to send emails ?";

            result = MessageBox.Show(msg, caption,
                                         MessageBoxButtons.OKCancel,
                                         MessageBoxIcon.Question);

            if (result != DialogResult.OK) return;

            // Launch another form window to act as a write-only console that will
            // report progress as the emails are sent.

            WriteOnlyConsoleForm consoleForm = new WriteOnlyConsoleForm();
            consoleForm.Show();
            consoleForm.tbOutputOnlyConsole.AppendText(String.Format("Sending qty. {0} LAML reports to selected members ...\r\nReports folder = {1}\r\n", numMembersToSendTo, mPathToReportsDirectory));

            // Try to get a report 'valid for' date from a report file name.
            string validFor = GetDateValidForStr();

            // Send emails.
            string subject = String.Format("FCSA LAML report{0}", validFor);
            string stdBody = @"Please find attached an Excel file providing your updated Licensed and Missing Link(LAML) report.";
            string body;

            File.AppendAllText(PATH_TO_LOG_FILE, "\n");

            int numEmailsSuccessfullySent = 0;
            int numEmailAttemptFailures = 0;
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    List<string> pathsToAttachedFiles = new List<string>();
                    pathsToAttachedFiles.Add(mReportFilePathsDict[kvp.Value.Member]);

                    body = String.Format("{0}\n\n{1}\n\n{2}\n{3}", kvp.Value.FirstLine.Text.Trim(), stdBody, "Peter Lin,", "FCSA General Manager");

                    consoleForm.tbOutputOnlyConsole.AppendText(String.Format("\r\nSending email to {0} ({1}) with attachment {2} ... ", kvp.Value.Member, kvp.Value.EmailAddress.Text, Path.GetFileName(pathsToAttachedFiles[0])));

                    int retVal = Email.Send(kvp.Value.EmailAddress.Text,
                                            subject,
                                            body,
                                            pathsToAttachedFiles,
                                            false,
                                            out mErrMsg);

                    if (retVal == Email.SUCCESS)
                    {
                        numEmailsSuccessfullySent++;
                        kvp.Value.PathToReportLastSent = pathsToAttachedFiles[0];
                        kvp.Value.DateTimeLastSent = DateTime.Now;
                        consoleForm.tbOutputOnlyConsole.AppendText(String.Format("\r\n         ... SUCCESS"));
                        msg = String.Format("\n{0} : {1} sent to {2} ({3})", kvp.Value.DateTimeLastSent.ToString("s"), kvp.Value.PathToReportLastSent, kvp.Value.Member, kvp.Value.EmailAddress.Text);
                        File.AppendAllText(PATH_TO_LOG_FILE, msg);
                    }
                    else
                    {
                        numEmailAttemptFailures++;

                        consoleForm.tbOutputOnlyConsole.AppendText(String.Format("\r\n         ... FAILURE: {0} : {1} : {2} : {3}", kvp.Value.Member, pathsToAttachedFiles[0], retVal, mErrMsg));
                    }
                }
            }

            UpdateOnlyLastSavedFieldsInDataFile();

            consoleForm.tbOutputOnlyConsole.AppendText(String.Format("\r\n\r\n------------------------"));
            consoleForm.tbOutputOnlyConsole.AppendText(String.Format("\r\nNumber of SUCCESSFUL email send attempts = {0,2}", numEmailsSuccessfullySent));
            consoleForm.tbOutputOnlyConsole.AppendText(String.Format("\r\nNumber of FAILED     email send attempts = {0,2}", numEmailAttemptFailures));
        }

        /// <summary>
        /// This method returns a path for the directory containing the most recent LAML reports. 
        /// </summary>
        /// <returns></returns>
        private static string GetLatestReportsFolder()
        {
            string latestReportsDirPath = LAML_REPORTS_HOME;

            try
            {
                string[] dirPaths = Directory.GetDirectories(LAML_REPORTS_HOME);

                int mostRecentDateInt = 0;
                string mostRecentDirPath = "";

                foreach (string dirPath in dirPaths)
                {
                    string dirName = Path.GetFileName(dirPath);

                    Match match = Regex.Match(dirName, VALID_DATE_STRING_PATTERN, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        int dateInt = Convert.ToInt32(match.Value.Replace("-", ""));
                        
                        if (dateInt > mostRecentDateInt)
                        {
                            mostRecentDateInt = dateInt;
                            mostRecentDirPath = dirPath;
                        }                        
                    }
                }

                if (!String.IsNullOrWhiteSpace(mostRecentDirPath)) latestReportsDirPath = mostRecentDirPath;
            }
            catch
            {
                // Something went wrong.
                // Just use LAML_REPORTS_HOME.
            }

            return latestReportsDirPath;
        }

        /// <summary>
        /// This method returns a string providing the 'valid for' date for an email's
        /// title/heading.
        /// </summary>
        /// <returns></returns>
        private static string GetDateValidForStr()
        {
            string validFor = "";

            // First try to extract a date from the currently selected reports folder.
            string reportsDirName = Path.GetFileName(mPathToReportsDirectory);
            Match match = Regex.Match(reportsDirName, VALID_DATE_STRING_PATTERN);
            if (match.Success)
            {
                validFor = String.Format(", valid for {0}", match.Value);
                return validFor;
            }

            // No date was available from the current reports directory.
            // Try getting a date 'validFor' from the reports file names.
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                if (kvp.Value.SendEmailYesNo.Checked)
                {
                    string fileName = Path.GetFileNameWithoutExtension(mReportFilePathsDict[kvp.Value.Member]);

                    match = Regex.Match(fileName, VALID_DATE_STRING_PATTERN);
                    if (match.Success)
                    {
                        validFor = String.Format(", valid for {0}", match.Value);
                        return validFor;
                    }
                }
            }

            return validFor;
        }

        /// <summary>
        /// This method populates the dictionary <string, string> of member-operator report file paths
        /// using a prescribed array of candidate file paths; only those files whose name contains a
        /// member-operator 'oper' code are entered into the dictionary.
        /// </summary>
        /// <param name="excelReportFilePaths"></param>
        /// <returns></returns>
        private static int PopulateReportFilePathsDict(string[] excelReportFilePaths)
        {
            int numHits = 0;

            mReportFilePathsDict = new Dictionary<string, string>();

            foreach (string member in mMembers)
            {
                foreach (string reportFilepath in excelReportFilePaths)
                {
                    string fileNameNoExtn = Path.GetFileNameWithoutExtension(reportFilepath);
                    if (fileNameNoExtn.ToUpper().Contains(member))
                    {
                        // Add to the dictionary of <member, reportFilepath>
                        mReportFilePathsDict.Add(member, reportFilepath);
                        numHits++;
                        continue;
                    }
                }
            }

            return numHits;
        }

        private static string CreateUserNotes()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("The purpose of this application is to send LAML reports (Excel files) to selected FCSA members.");
            sb.AppendLine();
            sb.AppendLine("Steps:");
            sb.AppendLine("=====");
            sb.AppendLine();
            sb.AppendLine("[1]  Enter the path of the folder containing the LAML reports you want to send.");
            sb.AppendLine();
            sb.AppendLine("[2]  Select which members to send reports to using the radio buttons on the left-hand-side.");
            sb.AppendLine();
            sb.AppendLine("[3]  An email address and salutation is required for every member selected to receive a report.");
            sb.AppendLine();
            sb.AppendLine("[4]  Click the CHECK button to confirm that your entries are sufficient to send emails to the selected members.");
            sb.AppendLine();
            sb.AppendLine("[5]  When ready, click the SEND button.");
            sb.AppendLine();
            sb.AppendLine("Notes:");
            sb.AppendLine("=====");
            sb.AppendLine();
            sb.AppendLine(@"[6]  A new set of reports should be placed in their own folder under D:\Reports\LicensedAndMissingLinks");
            sb.AppendLine();
            sb.AppendLine(@"[7]  The new reports folder name should include a date string like '20230827' ."); 
            sb.AppendLine();
            sb.AppendLine(@"[8]  User data is loaded from, and saved to, the file D:\prod\files\SendLAMLreports.csv");
            sb.AppendLine();
            sb.AppendLine(@"[9]  All successful email send events are logged in the file D:\prod\files\SendLAMLreports.log");

            return sb.ToString();
        }



    }
}
