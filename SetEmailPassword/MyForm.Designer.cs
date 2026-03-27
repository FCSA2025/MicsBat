namespace SetEmailPassword
{
    partial class MyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MyForm));
            this.existingPwdBox = new System.Windows.Forms.TextBox();
            this.newPwdBox1 = new System.Windows.Forms.TextBox();
            this.newPwdBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.fcsaLogoBox = new System.Windows.Forms.PictureBox();
            this.testButton = new System.Windows.Forms.Button();
            this.hideNewPasswordBox = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.fcsaLogoBox)).BeginInit();
            this.SuspendLayout();
            // 
            // existingPwdBox
            // 
            this.existingPwdBox.Location = new System.Drawing.Point(211, 232);
            this.existingPwdBox.Name = "existingPwdBox";
            this.existingPwdBox.PasswordChar = '*';
            this.existingPwdBox.Size = new System.Drawing.Size(223, 20);
            this.existingPwdBox.TabIndex = 2;
            this.existingPwdBox.UseSystemPasswordChar = true;
            this.existingPwdBox.TextChanged += new System.EventHandler(this.ExistingPwdBox_TextChanged);
            // 
            // newPwdBox1
            // 
            this.newPwdBox1.Location = new System.Drawing.Point(211, 284);
            this.newPwdBox1.Name = "newPwdBox1";
            this.newPwdBox1.PasswordChar = '*';
            this.newPwdBox1.Size = new System.Drawing.Size(223, 20);
            this.newPwdBox1.TabIndex = 3;
            this.newPwdBox1.UseSystemPasswordChar = true;
            // 
            // newPwdBox2
            // 
            this.newPwdBox2.Location = new System.Drawing.Point(211, 313);
            this.newPwdBox2.Name = "newPwdBox2";
            this.newPwdBox2.PasswordChar = '*';
            this.newPwdBox2.Size = new System.Drawing.Size(223, 20);
            this.newPwdBox2.TabIndex = 4;
            this.newPwdBox2.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(104, 235);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Enter old password:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(99, 284);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Enter new password:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(90, 316);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Confirm new password:\r\n";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(207, 366);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(54, 31);
            this.okButton.TabIndex = 8;
            this.okButton.Tag = "";
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(383, 366);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(51, 31);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // fcsaLogoBox
            // 
            this.fcsaLogoBox.Image = ((System.Drawing.Image)(resources.GetObject("fcsaLogoBox.Image")));
            this.fcsaLogoBox.ImageLocation = "";
            this.fcsaLogoBox.InitialImage = null;
            this.fcsaLogoBox.Location = new System.Drawing.Point(12, 12);
            this.fcsaLogoBox.Name = "fcsaLogoBox";
            this.fcsaLogoBox.Size = new System.Drawing.Size(180, 62);
            this.fcsaLogoBox.TabIndex = 10;
            this.fcsaLogoBox.TabStop = false;
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(287, 366);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(70, 31);
            this.testButton.TabIndex = 11;
            this.testButton.Text = "Test Email";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // hideNewPasswordBox
            // 
            this.hideNewPasswordBox.AutoSize = true;
            this.hideNewPasswordBox.Checked = true;
            this.hideNewPasswordBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.hideNewPasswordBox.Location = new System.Drawing.Point(13, 366);
            this.hideNewPasswordBox.Name = "hideNewPasswordBox";
            this.hideNewPasswordBox.Size = new System.Drawing.Size(119, 17);
            this.hideNewPasswordBox.TabIndex = 12;
            this.hideNewPasswordBox.Text = "Hide new password";
            this.hideNewPasswordBox.UseVisualStyleBackColor = true;
            this.hideNewPasswordBox.CheckedChanged += new System.EventHandler(this.OnHideNewPasswordsChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.Info;
            this.label1.Location = new System.Drawing.Point(221, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(357, 165);
            this.label1.TabIndex = 13;
            this.label1.Text = resources.GetString("label1.Text");
            this.label1.Click += new System.EventHandler(this.label1_Click_1);
            // 
            // MyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 403);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hideNewPasswordBox);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.fcsaLogoBox);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.newPwdBox2);
            this.Controls.Add(this.newPwdBox1);
            this.Controls.Add(this.existingPwdBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MyForm";
            this.Text = "Change the password for the mics@fcsa.ca email account.";
            this.Load += new System.EventHandler(this.MyForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.fcsaLogoBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox existingPwdBox;
        private System.Windows.Forms.TextBox newPwdBox1;
        private System.Windows.Forms.TextBox newPwdBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.PictureBox fcsaLogoBox;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.CheckBox hideNewPasswordBox;
        private System.Windows.Forms.Label label1;
    }
}

