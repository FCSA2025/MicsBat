# Documented File: WriteOnlyConsoleForm.Designer.cs
**Repository Path:** `SendLAMLreports\WriteOnlyConsoleForm.Designer.cs`
**Primary Layer:** `SendLAMLreports`
**Namespace:** `SendLAMLreports`

## Source Code Representation
```csharp
﻿
namespace SendLAMLreports
{
    /// <summary>
    /// An instance of this class provides the 'write-only console' Form window that is
    /// used to display the progress of the email send attempts.
    /// </summary>
    partial class WriteOnlyConsoleForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            this.tbOutputOnlyConsole = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbOutputOnlyConsole
            // 
            this.tbOutputOnlyConsole.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbOutputOnlyConsole.Location = new System.Drawing.Point(3, 3);
            this.tbOutputOnlyConsole.Multiline = true;
            this.tbOutputOnlyConsole.Name = "tbOutputOnlyConsole";
            this.tbOutputOnlyConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbOutputOnlyConsole.Size = new System.Drawing.Size(792, 444);
            this.tbOutputOnlyConsole.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(347, 455);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(122, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Return to main form";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bReturnToMainForm_Clicked);
            // 
            // WriteOnlyConsoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 484);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.tbOutputOnlyConsole);
            this.Name = "WriteOnlyConsoleForm";
            this.Text = "Sending LAML reports emails";
            this.TopMost = true;
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bReturnToMainForm_Clicked);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox tbOutputOnlyConsole;
        private System.Windows.Forms.Button button1;
    }
}
```
