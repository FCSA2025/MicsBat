# Documented File: MainForm.cs
**Repository Path:** `SendLAMLreports\MainForm.cs`
**Primary Layer:** `SendLAMLreports`
**Namespace:** `SendLAMLreports`

## Source Code Representation
```csharp
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace SendLAMLreports
{
    /// <summary>
    /// An instance of this class provides the top-level functionality of the 
    /// application's main window.
    /// </summary>
    public partial class MainForm : Form
    {
        private static Button mButton;
        private Color mColor;

        /// <summary>
        /// This constructor defines and activates all of the application-specific component-level
        /// elements of the Windows Form.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// This event occurs before a Windows Form is displayed for the first time; it
        /// is used to perform application-specific initializations and pre-processing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // Get the instance of the Form object that is running.
            mThisForm = SendLAMLreports.thisForm;

            CreateDictionayOfRecordsMappedToGUI(out mRecordsDict);

            ReadDataFromFile();

            mPathToReportsDirectory = GetLatestReportsFolder();
            tbDirectoryPath.Text = mPathToReportsDirectory;
        }

        /// <summary>
        /// Event-handler for a mouse-click on the SEND EMAIL button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bSendEmail_clicked(object sender, MouseEventArgs e)
        {
            SendEmailToSelectedMembers();
        }

        /// <summary>
        /// Event-handler for a mouse-click on the SAVE button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bSave_Clicked(object sender, MouseEventArgs e)
        {
            // Change the button's background color to green.
            mButton = (Button)sender;
            mColor = mButton.BackColor;
            mButton.BackColor = Color.Green;

            SaveDataToFile();

            // Create a timer with a 1 second interval.
            System.Timers.Timer aTimer = new System.Timers.Timer(1000);

            // Hook up the Elapsed event handler for the timer.
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEventSaveButtonClicked);

            // Start the timer.
            aTimer.Enabled = true;
        }

        /// <summary>
        /// Event-handler for the timer used to turn off the background highlighting of the 
        /// SAVE button after it receives a mouse-click.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void OnTimedEventSaveButtonClicked(object source, ElapsedEventArgs e)
        {
            // Change button color
            mButton.BackColor = mColor;
        }

        /// <summary>
        /// Event-handler for a mouse-click on the EXIT button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bExit_Clicked(object sender, MouseEventArgs e)
        {
            string currentCheckSum = CalculateCheckSum();

            // If the current checksum differs from the checksum at 'last save' then
            // popup a dialog to ask the user what he wants to do.
            if (currentCheckSum != mCheckSumLastSave)
            {
                string msg = "One or more field values have been changed since the last time the data was saved to file.";
                msg += "\n\n Do you want to save the current data before exiting?";

                string caption = "Exit";

                var result = MessageBox.Show(msg, caption,
                                             MessageBoxButtons.YesNo,
                                             MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    SaveDataToFile();
                }

            }

            Terminate();
        }

        /// <summary>
        /// Event-handler for a mouse-click on the CHECK DATA button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bCheckData_Clicked(object sender, MouseEventArgs e)
        {
            bool dataIsOK = UserDataIsOKforSelectedMembers(out mErrMsg);

            if (dataIsOK) mErrMsg = "The selected email addresses and salutations are valid.\n\nReady to send emails to selected members.";

            MessageBox.Show(mErrMsg);
        }

        /// <summary>
        /// Event-handler for the user typing into the REPORTS DIRECTORY PATH Textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbDirectoryPath_Changed(object sender, EventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            mPathToReportsDirectory = textBox.Text;
        }

        /// <summary>
        /// Event-handler for a mouse-click on the BROWSE button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bBrowse_Clicked(object sender, MouseEventArgs e)
        {
            using (BetterFolderBrowser bfb = new BetterFolderBrowser())
            {
                bfb.RootFolder = LAML_REPORTS_HOME;
                bfb.Multiselect = false;
                bfb.Title = "Select the folder containing the latest LAML reports";

                DialogResult result = bfb.ShowDialog();

                if (result == DialogResult.OK)
                {
                    mPathToReportsDirectory = bfb.SelectedFolder;
                    tbDirectoryPath.Text = mPathToReportsDirectory;
                }
            }

            MessageBox.Show(mPathToReportsDirectory);
        }

        /// <summary>
        /// Event-handler for a mouse-click on the SELECT NONE radio button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bSelectNone_Clicked(object sender, MouseEventArgs e)
        {
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                kvp.Value.SendEmailYesNo.Checked = false;
            }
        }

        /// <summary>
        /// Event-handler for a mouse-click on the SELECT ALL radio button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bSelectAll_Clicked(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, Record> kvp in mRecordsDict)
            {
                kvp.Value.SendEmailYesNo.Checked = true;
            }
        }

        /// <summary>
        /// Event-handler for a mouse-click on the USER GUIDE button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bUserGuide_Clicked(object sender, MouseEventArgs e)
        {
            MessageBox.Show(CreateUserNotes(), "User Guide");
        }
    }
}

```
