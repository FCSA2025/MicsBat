using System;

namespace SetEmailPassword
{
    /// <summary>
    /// Partial class definition comprising top-level GUI control elements like
    /// ShowDialog() and Dispose().
    /// </summary>
    partial class ConsoleMessageBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// This method shows the Dialog object.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caption"></param>
        public void ShowDialog(string message, string caption)
        {

            this.Display.Text = HandleNewLines(message);
            this.Text = caption;

            this.ShowDialog();
        }

        /// <summary>
        /// This method handles the case where the Dialog's message text contains
        /// troublesome line break characters - these are converted to the non-printing
        /// ACK character (ANSI character #6).
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string HandleNewLines(string message)
        {
            // Environment.NewLine is "\r\n" for Windows systems.
            // Replace all bona fide "\r\n" dyads with a marker.
            char markerChr = (char) 6; // The non-printing ACK character;

            message = message.Replace("\r\n", markerChr.ToString());

            message = message.Replace("\n", "\r\n");

            message = message.Replace(markerChr.ToString(), "\r\n");

            return message;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.OK = new System.Windows.Forms.Button();
            this.Display = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(174, 362);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 0;
            this.OK.Text = "Understood";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            this.OK.MouseClick += new System.Windows.Forms.MouseEventHandler(this.OK_MouseClick);
            // 
            // Display
            // 
            this.Display.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Display.Location = new System.Drawing.Point(0, 0);
            this.Display.Multiline = true;
            this.Display.Name = "Display";
            this.Display.ReadOnly = true;
            this.Display.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Display.Size = new System.Drawing.Size(426, 343);
            this.Display.TabIndex = 1;
            // 
            // ConsoleMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 395);
            this.Controls.Add(this.Display);
            this.Controls.Add(this.OK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConsoleMessageBox";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ConsoleMessageBox";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.TextBox Display;
    }
}