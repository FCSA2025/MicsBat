//...using _NewLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SetEmailPassword
{
    /// <summary>
    /// This application-specific class extends the standard .NET API class 'Form'
    /// and provides the functional elements of the graphical user interface.
    /// </summary>
    public partial class MyForm : Form
    {
        //private const string KEY_FULL_PATH = @"SOFTWARE\Microsoft\TelemetryClient\SampleStore\sqm\Reliability\ServiceState\MD5hash";
        private const string KEY_FULL_PATH = @"SOFTWARE\AlphaSoft\PDFconverter\Local\Defaults\Nuntius";
        private const string INITIAL_PWD = @"FCSA";

        // This regex is used to verify the format and content of an email address string. 
        private const string VALID_EMAIL_ADDRESS_PATTERN = @"(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*)@(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?)";

        /// <summary>
        /// Constructor for a MyForm object.
        /// </summary>
        public MyForm()
        {
            // Enable or disable developmental run-time logging.
#if false
            string mLog2FilePath = @"d:\MicsBatchLogs\SetEmailPassword.log";
            if (Log2.SetLogFilePath(mLog2FilePath))
            {
                Log2.Erase();
                Log2.Set(Log2.FileOpenClose.PER_SESSION);
                Log2.Set(Log2.WriteMode.ENABLED);
                Log2.Set(Log2.Level.VERBOSE);
                Info.BuildMetaData = Info.CollateExeMetaData();
                Log2.v("\nBuild: " + Info.BuildMetaData);
            }
            else
            {
                Console.Error.Write("\r\nERROR: could not open Log2 file: " + mLog2FilePath);
            }
#endif

            // Check that user has openned this application "Run as Administrator".
            if (!UserIsAdministrator())
            {
                string msg = String.Format("ERROR: You need elevated priviliges to change the password.\n\nExit this application and RUN AS ADMINISTRATOR.\n\nPress OK to exit this application.");
                //...Log2.e("\n\nSetEmailPassword.Form(): \n" + msg);
                MessageBox.Show(msg);

                // Informs all message pumps that they must terminate, and then closes all 
                // application windows after the messages have been processed. This is the code to use if you are 
                // have called Application.Run (WinForms applications), this method stops all running message loops 
                // on all threads and closes all windows of the application.
                // The problem is that we are in the constructor for the MyForm class and so no message pumps exist yet.
                // Application.Exit();

                // This terminates this process and gives the underlying operating system the specified exit code.
                System.Environment.Exit(1);
            }

            InitializeComponent();

            // Initially disable the OK button.
            okButton.Enabled = false;

            // Set the ToolTips for the three buttons.
            ToolTip toolTip1 = new ToolTip();
            toolTip1.ShowAlways = true;
            toolTip1.SetToolTip(okButton, "Commit to the password change and exit.");
            toolTip1.SetToolTip(testButton, "Test the new password before committing.");
            toolTip1.SetToolTip(cancelButton, "Exit with no changes.");

            // Handle the case where no key-value pair currently exists,
            // as would happen when running this application for the first
            // time on a new Windows Server OS installation.
            string value = Read_HKEY_LOCAL_MACHINE(KEY_FULL_PATH);

            //...Log2.v("\n\nSetEmailPassword.Form(): Read_HKEY_LOCAL_MACHINE(KEY_FULL_PATH) returned: " + Strings.EnQuote(value));

            if (String.IsNullOrWhiteSpace(value))
            {
                //...Log2.v("\n\nSetEmailPassword.Form(): attempting to create and assign value to registry key.");

                // Try to set the initial password.
                int retVal = Write_HKEY_LOCAL_MACHINE(KEY_FULL_PATH, INITIAL_PWD);

                //...Log2.v("\n\nSetEmailPassword.Form(): Write_HKEY_LOCAL_MACHINE(KEY_FULL_PATH) returned retVal =  " + retVal);

                if (retVal == 0)
                {
                    string msg = @"";
                    msg += "No password key was found in this computer's registry.\n\n";
                    msg += "A password key has been successfully created.\n\n";
                    msg += "The initial password is:     '" + INITIAL_PWD + "'     (exclude the apostrophes).\n\n";
                    msg += "The input text box labelled 'Enter current password'  will be loaded with this initial password.";

                    existingPwdBox.Text = INITIAL_PWD;

                    //...Log2.v("\n\nSetEmailPassword.Form(): \n" + msg);
                    MessageBox.Show(msg);
                }
                else
                {
                    string msg = @"";
                    msg += "No password key was found.\n\n";
                    msg += "The attempt to create a password key FAILED.\n\n";
                    msg += "Press OK to exit this application.";

                    //...Log2.v("\n\nSetEmailPassword.Form(): \n" + msg);
                    MessageBox.Show(msg);
                    Application.Exit();
                }
            }

        }

        /// <summary>
        /// label1 left click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Event handler for changes to the existing password text box control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExistingPwdBox_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// label5 left click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label5_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// cancel button left click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        /// <summary>
        /// OK button left click event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, EventArgs e)
        {
            // Get the user's entries.
            string existingPassword = existingPwdBox.Text.Trim();
            string newPassword1 = newPwdBox1.Text.Trim();
            string newPassword2 = newPwdBox2.Text.Trim();

            // Check that all three password strings are valid.
            if (!UserEntriesAreValid(existingPassword, newPassword1, newPassword2)) return;

            // If we get here then all is well with the user's inputs.
            // Go ahead and change the value of the registry key to the new password.
            int retVal = Write_HKEY_LOCAL_MACHINE(KEY_FULL_PATH, newPassword1);

            if (retVal != 0)
            {
                MessageBox.Show("ERROR: could not set the registry key's value.");
                Application.Exit();
            }

            // Just to be extra cautious, read back the current value and check it.
            string currentKeyValue = Read_HKEY_LOCAL_MACHINE(KEY_FULL_PATH);

            if (currentKeyValue != newPassword1)
            {
                MessageBox.Show("ERROR: failed registry key value write-then-read check.");
                Application.Exit();
            }

            string str = String.Format("The MICS email password was SUCCESSFUL changed to \"{0}\".\n\nPress OK to exit this application.", currentKeyValue);
            //...Log2.v("\n\nSetEmailPassword.Write_HKEY_LOCAL_MACHINE(): \n" + str);
            MessageBox.Show(str);

            Application.Exit();
        }


        /// <summary>
        /// This method sets the string value of the registry key at the prescribed path
        /// below computer&#92;HKEY_LOCAL_MACHINE.
        /// </summary>
        /// <param name="keyFullPath"></param>
        /// <param name="keyValue"></param>
        /// <returns>
        /// 0 if write attempt succeeded; non-zero indicates failure.
        /// </returns>
        public static int Write_HKEY_LOCAL_MACHINE(string keyFullPath, string keyValue)
        {
            int retVal = -1; // Any non-zero value signifies failure to write the key's value.
            string str = "";

            if (String.IsNullOrWhiteSpace(keyFullPath))
            {
                MessageBox.Show("ERROR: Write_HKEY_LOCAL_MACHINE(): keyFullPath is null, empty or whitespace.");
                Application.Exit();
            }

            if (String.IsNullOrWhiteSpace(keyValue))
            {
                MessageBox.Show("ERROR: Write_HKEY_LOCAL_MACHINE(): keyValue is null, empty or whitespace.");
                Application.Exit();
            }

            try
            {
                string keyDirPath = Path.GetDirectoryName(keyFullPath);
                string keyName = Path.GetFileName(keyFullPath);

                using (Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(keyDirPath))
                {
                    regKey.SetValue(keyName, keyValue);

                    //As a check, read the key that was just written to and compare with the command line arg.
                    string target = Read_HKEY_LOCAL_MACHINE(keyFullPath);

                    if (target.Equals(keyValue))
                    {
                        retVal = 0; // Successful write.
                    }
                    else
                    {
                        str = String.Format("ERROR: attempt to write to the registry key FAILED.\n\nRead-back check failed.\n\nPress OK to exit this application.");
                        //...Log2.v("\n\nSetEmailPassword.Write_HKEY_LOCAL_MACHINE(): \n" + str);
                        MessageBox.Show(str);

                        retVal = 22; // failed.
                    }
                }
            }
            catch (Exception e)
            {
                bool accessDenied = e.Message.Contains("denied");
                if (accessDenied)
                {
                    str = String.Format("ERROR: You need elevated priviliges to change the password.\n\nExit this application and RUN AS ADMINISTRATOR.\n\nPress OK to exit this application.", e.Message);
                    //...Log2.v("\n\nSetEmailPassword.Write_HKEY_LOCAL_MACHINE(): \n" + str);
                    MessageBox.Show(str);

                    retVal = 25; // failed.
                }
                else
                {
                    str = String.Format("ERROR: attempt to write to the registry key FAILED.\n\nRuntime exception thrown.\n\nPress OK to exit this application.");
                    //...Log2.v("\n\nSetEmailPassword.Write_HKEY_LOCAL_MACHINE(): \n" + str);
                    MessageBox.Show(str);

                    retVal = 26; // failed.
                }

            }

            return retVal;
        }

        /// <summary>
        /// This method returns the string value of the registry key at the prescribed path
        /// below computer&#92;HKEY_LOCAL_MACHINE.
        /// </summary>
        /// <param name="keyFullPath"></param>
        /// <returns>
        /// The value of the string at the prescribed key location below HKEY_LOCAL_MACHINE; if
        /// the returned value is null the read attempt failed.
        /// </returns>
        public static string Read_HKEY_LOCAL_MACHINE(string keyFullPath)
        {
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
                            Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR: obj == null");
                        }
                    }
                    else
                    {
                        Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR : key == null");
                    }
                }
            }
            catch (Exception e)
            {
                Console.Write("\n\nRead_HKEY_LOCAL_MACHINE() : ERROR: exception: {0}", e.Message);
            }

            return readValue;
        }

        /// <summary>
        /// This method returns true if the current user has Administrator privileges.
        /// </summary>
        /// <returns></returns>
        public static bool UserIsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Event handler for the check box used to set hide/show the new password.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnHideNewPasswordsChanged(object sender, EventArgs e)
        {
            if (hideNewPasswordBox.Checked)
            {
                newPwdBox1.PasswordChar = '*';
                newPwdBox2.PasswordChar = '*';

                newPwdBox1.UseSystemPasswordChar = true;
                newPwdBox2.UseSystemPasswordChar = true;
            }
            else
            {
                newPwdBox1.PasswordChar = char.MinValue;
                newPwdBox2.PasswordChar = char.MinValue;

                newPwdBox1.UseSystemPasswordChar = false;
                newPwdBox2.UseSystemPasswordChar = false;
            }
        }

        /// <summary>
        /// Event handler for the button control that launches the SMTP test.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testButton_Click(object sender, EventArgs e)
        {
            string msg = "";

            DialogResult dialogResult;

            // Get the user's entries.
            string existingPassword = existingPwdBox.Text.Trim();
            string newPassword1 = newPwdBox1.Text.Trim();
            string newPassword2 = newPwdBox2.Text.Trim();

            // Check that all three password strings are valid.
            if (!UserEntriesAreValid(existingPassword, newPassword1, newPassword2)) return;

            // Attempt to get the user to enter the email address to be used as
            // the 'target' for the test.
            string emailAddr = "";
            bool enterEmailAddressOngoing = true;

            while (enterEmailAddressOngoing)
            {
                dialogResult = SimpleInputBox("Email Address", "Enter a target email address:", out emailAddr);

                // If the user pressed 'Cancel' then abandon the email test.
                if (dialogResult == DialogResult.Cancel) return;

                // Validate the email address that was entered.
                msg = "";
                if (String.IsNullOrWhiteSpace(emailAddr))
                {
                    msg = "No email address was entered - try again.";
                }
                else if (!Regex.IsMatch(emailAddr, VALID_EMAIL_ADDRESS_PATTERN))
                {
                    msg = "This is not a valid email address - try again.";
                }

                if (msg != "")
                {
                    MessageBox.Show(msg);
                }
                else
                {
                    // We're done.
                    enterEmailAddressOngoing = false;
                }
            }

            msg = String.Format("Press OK to send a test email to {0}\n\nBe patient ... the test can last up to a minute.", emailAddr);

            dialogResult = MessageBox.Show(msg);

            if (dialogResult == DialogResult.Cancel) return;

            // Set cursor as hourglass
            Cursor.Current = Cursors.WaitCursor;

            string mSmtpHostUrl = "smtp.office365.com";
            string mSmtpAccount = "mics@fcsa.ca";
            string mSmtpPassword = newPassword1;
            int mSmtpPort = 587;
            bool mEnableSsl = true;
            string mFromAddress = "mics@fcsa.ca";
            string mToAddress = emailAddr;

            string smtpParameters = "";
            smtpParameters += String.Format("    SmtpHostUrl:   {0}\n", mSmtpHostUrl);
            smtpParameters += String.Format("    SmtpAccount:   {0}\n", mSmtpAccount);
            smtpParameters += String.Format("    SmtpPassword:  {0}\n", mSmtpPassword);
            smtpParameters += String.Format("    SmtpPort:      {0}\n", mSmtpPort);
            smtpParameters += String.Format("    EnableSsl:     {0}\n", mEnableSsl);
            smtpParameters += String.Format("    FromAddress:   {0}\n", mFromAddress);
            smtpParameters += String.Format("    ToAddress:     {0}\n", mToAddress);

            try
            {
                SmtpClient smtpClient = new SmtpClient(mSmtpHostUrl);

                MailAddress from = new MailAddress(mFromAddress);
                MailAddress to = new MailAddress(mToAddress);

                MailMessage message = new MailMessage(from, to);

                message.Body = "This email was sent to test FCSA's email password for mics@fcsa.ca";
                message.Subject = "Test";

                smtpClient.Port = mSmtpPort;
                smtpClient.EnableSsl = mEnableSsl;
                smtpClient.Credentials = new NetworkCredential(mSmtpAccount, mSmtpPassword);

                smtpClient.Send(message);

                msg = "The new email password appears to work.\n\nSuccessful SMTP client send attempt using:\n\n";
                msg += smtpParameters;
                msg += "\nCheck that this test email arrived in your inbox.";
                msg += "\n\nClick the OK button in the main form to commit to the password change.";

                // Successful test - enable the OK button - disable the two new password text boxes.
                okButton.Enabled = true;
                newPwdBox1.Enabled = false;
                newPwdBox2.Enabled = false;

            }
            catch (Exception ex)
            {
                msg = "The new email password FAILED.\n\nUnsuccessful SMTP client send attempt using:\n\n";
                msg += smtpParameters;
                msg += "\n\nSMTP Client response:\n\n" + ex.Message;

                // Test failed - disable the OK button  - enable the two new password text boxes.
                okButton.Enabled = false;
                newPwdBox1.Enabled = true;
                newPwdBox2.Enabled = true;
            }

            ConsoleMessageBox cmb = new ConsoleMessageBox();

            cmb.ShowDialog(msg, "Test email - results");

            // Set cursor as default arrow
            Cursor.Current = Cursors.Default;
        }

        /// <summary>
        /// This method surveys the current values of the exist and new password text boxes
        /// and returns true if they are valid.
        /// </summary>
        /// <param name="existingPassword"></param>
        /// <param name="newPassword1"></param>
        /// <param name="newPassword2"></param>
        /// <returns></returns>
        private bool UserEntriesAreValid(string existingPassword, string newPassword1, string newPassword2)
        {
            bool retVal = true;

            // Check that all three password strings are valid.
            if (String.IsNullOrWhiteSpace(existingPassword))
            {
                MessageBox.Show("You have not entered the current password. Try again.");
                retVal = false;
            }
            else if (String.IsNullOrWhiteSpace(newPassword1))
            {
                MessageBox.Show("You have not entered the new password twice. Try again.");
                retVal = false;
            }
            else if (String.IsNullOrWhiteSpace(newPassword2))
            {
                MessageBox.Show("You have not entered the new password twice. Try again.");
                retVal = false;
            }
            else if (!newPassword1.Equals(newPassword2))
            {
                MessageBox.Show("The two new password entries are not identical. Try again.");
                retVal = false;
            }

            if (retVal == false) return false;

            // Check that the existing password entry is the same as the registry key value.
            string keyValue = Read_HKEY_LOCAL_MACHINE(KEY_FULL_PATH).Trim();

            if (String.IsNullOrWhiteSpace(keyValue))
            {
                MessageBox.Show("ERROR: could not read the registry key. Press OK then CANCEL to exit.");
                retVal = false;
            }
            else if (!existingPassword.Equals(keyValue))
            {
                MessageBox.Show("Your entry for the current password is incorrect. Try again.");
                retVal = false;
            }

            return retVal;
        }

        /// <summary>
        /// Event handler for 'on Form load'.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyForm_Load(object sender, EventArgs e)
        {

        }


        /// <summary>
        /// This method implements a simple GUI input box in which a question is displayed
        /// and the user's answer is returned as the argument 'inputString', for example
        /// it is used when the user is invited to enter a receipient's email address.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="promptText"></param>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static DialogResult SimpleInputBox(string title, string promptText, out string inputString)
        {
            // 'out'.
            inputString = "";

            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = inputString;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            inputString = textBox.Text;
            return dialogResult;
        }

        /// <summary>
        /// Event handler for clicking on label1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label1_Click_1(object sender, EventArgs e)
        {

        }




    }
}
