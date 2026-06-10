# Documented File: MainForm.Designer.cs
**Repository Path:** `SendLAMLreports\MainForm.Designer.cs`
**Primary Layer:** `SendLAMLreports`
**Namespace:** `SendLAMLreports`

## Source Code Representation
```csharp
﻿
namespace SendLAMLreports
{
    /// <summary>
    /// An instance of this class provides the top-level functionality of the 
    /// application's main window.
    /// </summary>
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.cbABCCOM = new System.Windows.Forms.CheckBox();
            this.tbDirectoryPath = new System.Windows.Forms.TextBox();
            this.bBrowse = new System.Windows.Forms.Button();
            this.tbABCCOMemadd = new System.Windows.Forms.TextBox();
            this.bSave = new System.Windows.Forms.Button();
            this.bExit = new System.Windows.Forms.Button();
            this.cbALIANT = new System.Windows.Forms.CheckBox();
            this.tbALIANTemadd = new System.Windows.Forms.TextBox();
            this.tbBELLemadd = new System.Windows.Forms.TextBox();
            this.cbBELL = new System.Windows.Forms.CheckBox();
            this.tbBCHYemadd = new System.Windows.Forms.TextBox();
            this.cbBCHY = new System.Windows.Forms.CheckBox();
            this.tbGLWemadd = new System.Windows.Forms.TextBox();
            this.tbDNDemadd = new System.Windows.Forms.TextBox();
            this.tbBRAGGemadd = new System.Windows.Forms.TextBox();
            this.tbBMCEemadd = new System.Windows.Forms.TextBox();
            this.tbRCTLemadd = new System.Windows.Forms.TextBox();
            this.tbONTemadd = new System.Windows.Forms.TextBox();
            this.tbNWTemadd = new System.Windows.Forms.TextBox();
            this.tbNTTELemadd = new System.Windows.Forms.TextBox();
            this.tbNAVIemadd = new System.Windows.Forms.TextBox();
            this.tbMTSemadd = new System.Windows.Forms.TextBox();
            this.tbHYQUemadd = new System.Windows.Forms.TextBox();
            this.tbHYONEemadd = new System.Windows.Forms.TextBox();
            this.tbTLUSQCemadd = new System.Windows.Forms.TextBox();
            this.tbTLUSMCemadd = new System.Windows.Forms.TextBox();
            this.tbTLUSBCemadd = new System.Windows.Forms.TextBox();
            this.tbTLUSABemadd = new System.Windows.Forms.TextBox();
            this.tbTERAGOemadd = new System.Windows.Forms.TextBox();
            this.tbTBAYemadd = new System.Windows.Forms.TextBox();
            this.tbSTELemadd = new System.Windows.Forms.TextBox();
            this.tbSHAWemadd = new System.Windows.Forms.TextBox();
            this.tbZAYOemadd = new System.Windows.Forms.TextBox();
            this.tbXCIemadd = new System.Windows.Forms.TextBox();
            this.tbWIREIEemadd = new System.Windows.Forms.TextBox();
            this.tbVDTRemadd = new System.Windows.Forms.TextBox();
            this.cbGLW = new System.Windows.Forms.CheckBox();
            this.cbDND = new System.Windows.Forms.CheckBox();
            this.cbBRAGG = new System.Windows.Forms.CheckBox();
            this.cbBMCE = new System.Windows.Forms.CheckBox();
            this.cbNAVI = new System.Windows.Forms.CheckBox();
            this.cbMTS = new System.Windows.Forms.CheckBox();
            this.cbHYQU = new System.Windows.Forms.CheckBox();
            this.cbHYONE = new System.Windows.Forms.CheckBox();
            this.cbRCTL = new System.Windows.Forms.CheckBox();
            this.cbONT = new System.Windows.Forms.CheckBox();
            this.cbNWT = new System.Windows.Forms.CheckBox();
            this.cbNTTEL = new System.Windows.Forms.CheckBox();
            this.cbTERAGO = new System.Windows.Forms.CheckBox();
            this.cbTBAY = new System.Windows.Forms.CheckBox();
            this.cbSTEL = new System.Windows.Forms.CheckBox();
            this.cbSHAW = new System.Windows.Forms.CheckBox();
            this.cbTLUSQC = new System.Windows.Forms.CheckBox();
            this.cbTLUSMC = new System.Windows.Forms.CheckBox();
            this.cbTLUSBC = new System.Windows.Forms.CheckBox();
            this.cbTLUSAB = new System.Windows.Forms.CheckBox();
            this.cbZAYO = new System.Windows.Forms.CheckBox();
            this.cbXCI = new System.Windows.Forms.CheckBox();
            this.cbWIREIE = new System.Windows.Forms.CheckBox();
            this.cbVDTR = new System.Windows.Forms.CheckBox();
            this.labelMember = new System.Windows.Forms.Label();
            this.labelEmailAddress = new System.Windows.Forms.Label();
            this.tbABCCOM1stLine = new System.Windows.Forms.TextBox();
            this.labelSalutation = new System.Windows.Forms.Label();
            this.tbALIANT1stLine = new System.Windows.Forms.TextBox();
            this.tbHYQU1stLine = new System.Windows.Forms.TextBox();
            this.tbHYONE1stLine = new System.Windows.Forms.TextBox();
            this.tbGLW1stLine = new System.Windows.Forms.TextBox();
            this.tbDND1stLine = new System.Windows.Forms.TextBox();
            this.tbBRAGG1stLine = new System.Windows.Forms.TextBox();
            this.tbBMCE1stLine = new System.Windows.Forms.TextBox();
            this.tbBELL1stLine = new System.Windows.Forms.TextBox();
            this.tbBCHY1stLine = new System.Windows.Forms.TextBox();
            this.tbMTS1stLine = new System.Windows.Forms.TextBox();
            this.tbRCTL1stLine = new System.Windows.Forms.TextBox();
            this.tbONT1stLine = new System.Windows.Forms.TextBox();
            this.tbNAVI1stLine = new System.Windows.Forms.TextBox();
            this.tbNWT1stLine = new System.Windows.Forms.TextBox();
            this.tbNTTEL1stLine = new System.Windows.Forms.TextBox();
            this.tbTLUSBC1stLine = new System.Windows.Forms.TextBox();
            this.tbSHAW1stLine = new System.Windows.Forms.TextBox();
            this.tbSTEL1stLine = new System.Windows.Forms.TextBox();
            this.tbTLUSAB1stLine = new System.Windows.Forms.TextBox();
            this.tbTERAGO1stLine = new System.Windows.Forms.TextBox();
            this.tbTBAY1stLine = new System.Windows.Forms.TextBox();
            this.tbZAYO1stLine = new System.Windows.Forms.TextBox();
            this.tbXCI1stLine = new System.Windows.Forms.TextBox();
            this.tbWIREIE1stLine = new System.Windows.Forms.TextBox();
            this.tbVDTR1stLine = new System.Windows.Forms.TextBox();
            this.tbTLUSQC1stLine = new System.Windows.Forms.TextBox();
            this.tbTLUSMC1stLine = new System.Windows.Forms.TextBox();
            this.bSend = new System.Windows.Forms.Button();
            this.bCheckData = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.bSelectAll = new System.Windows.Forms.Button();
            this.bSelectNone = new System.Windows.Forms.Button();
            this.bUserGuide = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cbABCCOM
            // 
            this.cbABCCOM.AutoSize = true;
            this.cbABCCOM.Location = new System.Drawing.Point(36, 98);
            this.cbABCCOM.Name = "cbABCCOM";
            this.cbABCCOM.Size = new System.Drawing.Size(71, 17);
            this.cbABCCOM.TabIndex = 2;
            this.cbABCCOM.Text = "ABCCOM";
            this.cbABCCOM.UseVisualStyleBackColor = true;
            // 
            // tbDirectoryPath
            // 
            this.tbDirectoryPath.Location = new System.Drawing.Point(225, 22);
            this.tbDirectoryPath.Name = "tbDirectoryPath";
            this.tbDirectoryPath.Size = new System.Drawing.Size(354, 20);
            this.tbDirectoryPath.TabIndex = 3;
            this.tbDirectoryPath.TextChanged += new System.EventHandler(this.tbDirectoryPath_Changed);
            // 
            // bBrowse
            // 
            this.bBrowse.Location = new System.Drawing.Point(585, 22);
            this.bBrowse.Name = "bBrowse";
            this.bBrowse.Size = new System.Drawing.Size(56, 21);
            this.bBrowse.TabIndex = 5;
            this.bBrowse.Text = "Browse";
            this.bBrowse.UseVisualStyleBackColor = true;
            this.bBrowse.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bBrowse_Clicked);
            // 
            // tbABCCOMemadd
            // 
            this.tbABCCOMemadd.Location = new System.Drawing.Point(132, 96);
            this.tbABCCOMemadd.Name = "tbABCCOMemadd";
            this.tbABCCOMemadd.Size = new System.Drawing.Size(285, 20);
            this.tbABCCOMemadd.TabIndex = 7;
            // 
            // bSave
            // 
            this.bSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.bSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bSave.Location = new System.Drawing.Point(322, 746);
            this.bSave.Name = "bSave";
            this.bSave.Size = new System.Drawing.Size(95, 42);
            this.bSave.TabIndex = 8;
            this.bSave.Text = "Save";
            this.bSave.UseVisualStyleBackColor = false;
            this.bSave.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bSave_Clicked);
            // 
            // bExit
            // 
            this.bExit.AccessibleDescription = "";
            this.bExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.bExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bExit.Location = new System.Drawing.Point(436, 748);
            this.bExit.Name = "bExit";
            this.bExit.Size = new System.Drawing.Size(96, 42);
            this.bExit.TabIndex = 9;
            this.bExit.Text = "Exit";
            this.bExit.UseVisualStyleBackColor = false;
            this.bExit.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bExit_Clicked);
            // 
            // cbALIANT
            // 
            this.cbALIANT.AutoSize = true;
            this.cbALIANT.Location = new System.Drawing.Point(36, 122);
            this.cbALIANT.Name = "cbALIANT";
            this.cbALIANT.Size = new System.Drawing.Size(64, 17);
            this.cbALIANT.TabIndex = 11;
            this.cbALIANT.Text = "ALIANT";
            this.cbALIANT.UseVisualStyleBackColor = true;
            // 
            // tbALIANTemadd
            // 
            this.tbALIANTemadd.Location = new System.Drawing.Point(132, 119);
            this.tbALIANTemadd.Name = "tbALIANTemadd";
            this.tbALIANTemadd.Size = new System.Drawing.Size(285, 20);
            this.tbALIANTemadd.TabIndex = 12;
            // 
            // tbBELLemadd
            // 
            this.tbBELLemadd.Location = new System.Drawing.Point(132, 165);
            this.tbBELLemadd.Name = "tbBELLemadd";
            this.tbBELLemadd.Size = new System.Drawing.Size(285, 20);
            this.tbBELLemadd.TabIndex = 18;
            // 
            // cbBELL
            // 
            this.cbBELL.AutoSize = true;
            this.cbBELL.Location = new System.Drawing.Point(36, 168);
            this.cbBELL.Name = "cbBELL";
            this.cbBELL.Size = new System.Drawing.Size(52, 17);
            this.cbBELL.TabIndex = 17;
            this.cbBELL.Text = "BELL";
            this.cbBELL.UseVisualStyleBackColor = true;
            // 
            // tbBCHYemadd
            // 
            this.tbBCHYemadd.Location = new System.Drawing.Point(132, 142);
            this.tbBCHYemadd.Name = "tbBCHYemadd";
            this.tbBCHYemadd.Size = new System.Drawing.Size(285, 20);
            this.tbBCHYemadd.TabIndex = 15;
            // 
            // cbBCHY
            // 
            this.cbBCHY.AutoSize = true;
            this.cbBCHY.Location = new System.Drawing.Point(36, 144);
            this.cbBCHY.Name = "cbBCHY";
            this.cbBCHY.Size = new System.Drawing.Size(55, 17);
            this.cbBCHY.TabIndex = 13;
            this.cbBCHY.Text = "BCHY";
            this.cbBCHY.UseVisualStyleBackColor = true;
            // 
            // tbGLWemadd
            // 
            this.tbGLWemadd.Location = new System.Drawing.Point(132, 256);
            this.tbGLWemadd.Name = "tbGLWemadd";
            this.tbGLWemadd.Size = new System.Drawing.Size(285, 20);
            this.tbGLWemadd.TabIndex = 22;
            // 
            // tbDNDemadd
            // 
            this.tbDNDemadd.Location = new System.Drawing.Point(132, 233);
            this.tbDNDemadd.Name = "tbDNDemadd";
            this.tbDNDemadd.Size = new System.Drawing.Size(285, 20);
            this.tbDNDemadd.TabIndex = 21;
            // 
            // tbBRAGGemadd
            // 
            this.tbBRAGGemadd.Location = new System.Drawing.Point(132, 210);
            this.tbBRAGGemadd.Name = "tbBRAGGemadd";
            this.tbBRAGGemadd.Size = new System.Drawing.Size(285, 20);
            this.tbBRAGGemadd.TabIndex = 20;
            // 
            // tbBMCEemadd
            // 
            this.tbBMCEemadd.Location = new System.Drawing.Point(132, 187);
            this.tbBMCEemadd.Name = "tbBMCEemadd";
            this.tbBMCEemadd.Size = new System.Drawing.Size(285, 20);
            this.tbBMCEemadd.TabIndex = 19;
            // 
            // tbRCTLemadd
            // 
            this.tbRCTLemadd.Location = new System.Drawing.Point(132, 438);
            this.tbRCTLemadd.Name = "tbRCTLemadd";
            this.tbRCTLemadd.Size = new System.Drawing.Size(285, 20);
            this.tbRCTLemadd.TabIndex = 30;
            // 
            // tbONTemadd
            // 
            this.tbONTemadd.Location = new System.Drawing.Point(132, 415);
            this.tbONTemadd.Name = "tbONTemadd";
            this.tbONTemadd.Size = new System.Drawing.Size(285, 20);
            this.tbONTemadd.TabIndex = 29;
            // 
            // tbNWTemadd
            // 
            this.tbNWTemadd.Location = new System.Drawing.Point(132, 392);
            this.tbNWTemadd.Name = "tbNWTemadd";
            this.tbNWTemadd.Size = new System.Drawing.Size(285, 20);
            this.tbNWTemadd.TabIndex = 28;
            // 
            // tbNTTELemadd
            // 
            this.tbNTTELemadd.Location = new System.Drawing.Point(132, 369);
            this.tbNTTELemadd.Name = "tbNTTELemadd";
            this.tbNTTELemadd.Size = new System.Drawing.Size(285, 20);
            this.tbNTTELemadd.TabIndex = 27;
            // 
            // tbNAVIemadd
            // 
            this.tbNAVIemadd.Location = new System.Drawing.Point(132, 347);
            this.tbNAVIemadd.Name = "tbNAVIemadd";
            this.tbNAVIemadd.Size = new System.Drawing.Size(285, 20);
            this.tbNAVIemadd.TabIndex = 26;
            // 
            // tbMTSemadd
            // 
            this.tbMTSemadd.Location = new System.Drawing.Point(132, 324);
            this.tbMTSemadd.Name = "tbMTSemadd";
            this.tbMTSemadd.Size = new System.Drawing.Size(285, 20);
            this.tbMTSemadd.TabIndex = 25;
            // 
            // tbHYQUemadd
            // 
            this.tbHYQUemadd.Location = new System.Drawing.Point(132, 301);
            this.tbHYQUemadd.Name = "tbHYQUemadd";
            this.tbHYQUemadd.Size = new System.Drawing.Size(285, 20);
            this.tbHYQUemadd.TabIndex = 24;
            // 
            // tbHYONEemadd
            // 
            this.tbHYONEemadd.Location = new System.Drawing.Point(132, 278);
            this.tbHYONEemadd.Name = "tbHYONEemadd";
            this.tbHYONEemadd.Size = new System.Drawing.Size(285, 20);
            this.tbHYONEemadd.TabIndex = 23;
            // 
            // tbTLUSQCemadd
            // 
            this.tbTLUSQCemadd.Location = new System.Drawing.Point(132, 620);
            this.tbTLUSQCemadd.Name = "tbTLUSQCemadd";
            this.tbTLUSQCemadd.Size = new System.Drawing.Size(285, 20);
            this.tbTLUSQCemadd.TabIndex = 38;
            // 
            // tbTLUSMCemadd
            // 
            this.tbTLUSMCemadd.Location = new System.Drawing.Point(132, 597);
            this.tbTLUSMCemadd.Name = "tbTLUSMCemadd";
            this.tbTLUSMCemadd.Size = new System.Drawing.Size(285, 20);
            this.tbTLUSMCemadd.TabIndex = 37;
            // 
            // tbTLUSBCemadd
            // 
            this.tbTLUSBCemadd.Location = new System.Drawing.Point(132, 574);
            this.tbTLUSBCemadd.Name = "tbTLUSBCemadd";
            this.tbTLUSBCemadd.Size = new System.Drawing.Size(285, 20);
            this.tbTLUSBCemadd.TabIndex = 36;
            // 
            // tbTLUSABemadd
            // 
            this.tbTLUSABemadd.Location = new System.Drawing.Point(132, 551);
            this.tbTLUSABemadd.Name = "tbTLUSABemadd";
            this.tbTLUSABemadd.Size = new System.Drawing.Size(285, 20);
            this.tbTLUSABemadd.TabIndex = 35;
            // 
            // tbTERAGOemadd
            // 
            this.tbTERAGOemadd.Location = new System.Drawing.Point(132, 529);
            this.tbTERAGOemadd.Name = "tbTERAGOemadd";
            this.tbTERAGOemadd.Size = new System.Drawing.Size(285, 20);
            this.tbTERAGOemadd.TabIndex = 34;
            // 
            // tbTBAYemadd
            // 
            this.tbTBAYemadd.Location = new System.Drawing.Point(132, 506);
            this.tbTBAYemadd.Name = "tbTBAYemadd";
            this.tbTBAYemadd.Size = new System.Drawing.Size(285, 20);
            this.tbTBAYemadd.TabIndex = 33;
            // 
            // tbSTELemadd
            // 
            this.tbSTELemadd.Location = new System.Drawing.Point(132, 483);
            this.tbSTELemadd.Name = "tbSTELemadd";
            this.tbSTELemadd.Size = new System.Drawing.Size(285, 20);
            this.tbSTELemadd.TabIndex = 32;
            // 
            // tbSHAWemadd
            // 
            this.tbSHAWemadd.Location = new System.Drawing.Point(132, 460);
            this.tbSHAWemadd.Name = "tbSHAWemadd";
            this.tbSHAWemadd.Size = new System.Drawing.Size(285, 20);
            this.tbSHAWemadd.TabIndex = 31;
            // 
            // tbZAYOemadd
            // 
            this.tbZAYOemadd.Location = new System.Drawing.Point(132, 712);
            this.tbZAYOemadd.Name = "tbZAYOemadd";
            this.tbZAYOemadd.Size = new System.Drawing.Size(285, 20);
            this.tbZAYOemadd.TabIndex = 42;
            // 
            // tbXCIemadd
            // 
            this.tbXCIemadd.Location = new System.Drawing.Point(132, 689);
            this.tbXCIemadd.Name = "tbXCIemadd";
            this.tbXCIemadd.Size = new System.Drawing.Size(285, 20);
            this.tbXCIemadd.TabIndex = 41;
            // 
            // tbWIREIEemadd
            // 
            this.tbWIREIEemadd.Location = new System.Drawing.Point(132, 666);
            this.tbWIREIEemadd.Name = "tbWIREIEemadd";
            this.tbWIREIEemadd.Size = new System.Drawing.Size(285, 20);
            this.tbWIREIEemadd.TabIndex = 40;
            // 
            // tbVDTRemadd
            // 
            this.tbVDTRemadd.Location = new System.Drawing.Point(132, 643);
            this.tbVDTRemadd.Name = "tbVDTRemadd";
            this.tbVDTRemadd.Size = new System.Drawing.Size(285, 20);
            this.tbVDTRemadd.TabIndex = 39;
            // 
            // cbGLW
            // 
            this.cbGLW.AutoSize = true;
            this.cbGLW.Location = new System.Drawing.Point(36, 261);
            this.cbGLW.Name = "cbGLW";
            this.cbGLW.Size = new System.Drawing.Size(51, 17);
            this.cbGLW.TabIndex = 54;
            this.cbGLW.Text = "GLW";
            this.cbGLW.UseVisualStyleBackColor = true;
            // 
            // cbDND
            // 
            this.cbDND.AutoSize = true;
            this.cbDND.Location = new System.Drawing.Point(36, 237);
            this.cbDND.Name = "cbDND";
            this.cbDND.Size = new System.Drawing.Size(50, 17);
            this.cbDND.TabIndex = 51;
            this.cbDND.Text = "DND";
            this.cbDND.UseVisualStyleBackColor = true;
            // 
            // cbBRAGG
            // 
            this.cbBRAGG.AutoSize = true;
            this.cbBRAGG.Location = new System.Drawing.Point(36, 212);
            this.cbBRAGG.Name = "cbBRAGG";
            this.cbBRAGG.Size = new System.Drawing.Size(64, 17);
            this.cbBRAGG.TabIndex = 50;
            this.cbBRAGG.Text = "BRAGG";
            this.cbBRAGG.UseVisualStyleBackColor = true;
            // 
            // cbBMCE
            // 
            this.cbBMCE.AutoSize = true;
            this.cbBMCE.Location = new System.Drawing.Point(36, 189);
            this.cbBMCE.Name = "cbBMCE";
            this.cbBMCE.Size = new System.Drawing.Size(56, 17);
            this.cbBMCE.TabIndex = 47;
            this.cbBMCE.Text = "BMCE";
            this.cbBMCE.UseVisualStyleBackColor = true;
            // 
            // cbNAVI
            // 
            this.cbNAVI.AutoSize = true;
            this.cbNAVI.Location = new System.Drawing.Point(36, 353);
            this.cbNAVI.Name = "cbNAVI";
            this.cbNAVI.Size = new System.Drawing.Size(51, 17);
            this.cbNAVI.TabIndex = 62;
            this.cbNAVI.Text = "NAVI";
            this.cbNAVI.UseVisualStyleBackColor = true;
            // 
            // cbMTS
            // 
            this.cbMTS.AutoSize = true;
            this.cbMTS.Location = new System.Drawing.Point(36, 330);
            this.cbMTS.Name = "cbMTS";
            this.cbMTS.Size = new System.Drawing.Size(49, 17);
            this.cbMTS.TabIndex = 59;
            this.cbMTS.Text = "MTS";
            this.cbMTS.UseVisualStyleBackColor = true;
            // 
            // cbHYQU
            // 
            this.cbHYQU.AutoSize = true;
            this.cbHYQU.Location = new System.Drawing.Point(36, 305);
            this.cbHYQU.Name = "cbHYQU";
            this.cbHYQU.Size = new System.Drawing.Size(57, 17);
            this.cbHYQU.TabIndex = 58;
            this.cbHYQU.Text = "HYQU";
            this.cbHYQU.UseVisualStyleBackColor = true;
            // 
            // cbHYONE
            // 
            this.cbHYONE.AutoSize = true;
            this.cbHYONE.Location = new System.Drawing.Point(36, 282);
            this.cbHYONE.Name = "cbHYONE";
            this.cbHYONE.Size = new System.Drawing.Size(64, 17);
            this.cbHYONE.TabIndex = 55;
            this.cbHYONE.Text = "HYONE";
            this.cbHYONE.UseVisualStyleBackColor = true;
            // 
            // cbRCTL
            // 
            this.cbRCTL.AutoSize = true;
            this.cbRCTL.Location = new System.Drawing.Point(36, 443);
            this.cbRCTL.Name = "cbRCTL";
            this.cbRCTL.Size = new System.Drawing.Size(54, 17);
            this.cbRCTL.TabIndex = 70;
            this.cbRCTL.Text = "RCTL";
            this.cbRCTL.UseVisualStyleBackColor = true;
            // 
            // cbONT
            // 
            this.cbONT.AutoSize = true;
            this.cbONT.Location = new System.Drawing.Point(36, 421);
            this.cbONT.Name = "cbONT";
            this.cbONT.Size = new System.Drawing.Size(49, 17);
            this.cbONT.TabIndex = 67;
            this.cbONT.Text = "ONT";
            this.cbONT.UseVisualStyleBackColor = true;
            // 
            // cbNWT
            // 
            this.cbNWT.AutoSize = true;
            this.cbNWT.Location = new System.Drawing.Point(36, 396);
            this.cbNWT.Name = "cbNWT";
            this.cbNWT.Size = new System.Drawing.Size(52, 17);
            this.cbNWT.TabIndex = 66;
            this.cbNWT.Text = "NWT";
            this.cbNWT.UseVisualStyleBackColor = true;
            // 
            // cbNTTEL
            // 
            this.cbNTTEL.AutoSize = true;
            this.cbNTTEL.Location = new System.Drawing.Point(36, 373);
            this.cbNTTEL.Name = "cbNTTEL";
            this.cbNTTEL.Size = new System.Drawing.Size(61, 17);
            this.cbNTTEL.TabIndex = 63;
            this.cbNTTEL.Text = "NTTEL";
            this.cbNTTEL.UseVisualStyleBackColor = true;
            // 
            // cbTERAGO
            // 
            this.cbTERAGO.AutoSize = true;
            this.cbTERAGO.Location = new System.Drawing.Point(36, 533);
            this.cbTERAGO.Name = "cbTERAGO";
            this.cbTERAGO.Size = new System.Drawing.Size(71, 17);
            this.cbTERAGO.TabIndex = 78;
            this.cbTERAGO.Text = "TERAGO";
            this.cbTERAGO.UseVisualStyleBackColor = true;
            // 
            // cbTBAY
            // 
            this.cbTBAY.AutoSize = true;
            this.cbTBAY.Location = new System.Drawing.Point(36, 511);
            this.cbTBAY.Name = "cbTBAY";
            this.cbTBAY.Size = new System.Drawing.Size(54, 17);
            this.cbTBAY.TabIndex = 75;
            this.cbTBAY.Text = "TBAY";
            this.cbTBAY.UseVisualStyleBackColor = true;
            // 
            // cbSTEL
            // 
            this.cbSTEL.AutoSize = true;
            this.cbSTEL.Location = new System.Drawing.Point(36, 486);
            this.cbSTEL.Name = "cbSTEL";
            this.cbSTEL.Size = new System.Drawing.Size(53, 17);
            this.cbSTEL.TabIndex = 74;
            this.cbSTEL.Text = "STEL";
            this.cbSTEL.UseVisualStyleBackColor = true;
            // 
            // cbSHAW
            // 
            this.cbSHAW.AutoSize = true;
            this.cbSHAW.Location = new System.Drawing.Point(36, 463);
            this.cbSHAW.Name = "cbSHAW";
            this.cbSHAW.Size = new System.Drawing.Size(59, 17);
            this.cbSHAW.TabIndex = 71;
            this.cbSHAW.Text = "SHAW";
            this.cbSHAW.UseVisualStyleBackColor = true;
            // 
            // cbTLUSQC
            // 
            this.cbTLUSQC.AutoSize = true;
            this.cbTLUSQC.Location = new System.Drawing.Point(36, 622);
            this.cbTLUSQC.Name = "cbTLUSQC";
            this.cbTLUSQC.Size = new System.Drawing.Size(69, 17);
            this.cbTLUSQC.TabIndex = 86;
            this.cbTLUSQC.Text = "TLUSQC";
            this.cbTLUSQC.UseVisualStyleBackColor = true;
            // 
            // cbTLUSMC
            // 
            this.cbTLUSMC.AutoSize = true;
            this.cbTLUSMC.Location = new System.Drawing.Point(36, 600);
            this.cbTLUSMC.Name = "cbTLUSMC";
            this.cbTLUSMC.Size = new System.Drawing.Size(70, 17);
            this.cbTLUSMC.TabIndex = 83;
            this.cbTLUSMC.Text = "TLUSMC";
            this.cbTLUSMC.UseVisualStyleBackColor = true;
            // 
            // cbTLUSBC
            // 
            this.cbTLUSBC.AutoSize = true;
            this.cbTLUSBC.Location = new System.Drawing.Point(36, 575);
            this.cbTLUSBC.Name = "cbTLUSBC";
            this.cbTLUSBC.Size = new System.Drawing.Size(68, 17);
            this.cbTLUSBC.TabIndex = 82;
            this.cbTLUSBC.Text = "TLUSBC";
            this.cbTLUSBC.UseVisualStyleBackColor = true;
            // 
            // cbTLUSAB
            // 
            this.cbTLUSAB.AutoSize = true;
            this.cbTLUSAB.Location = new System.Drawing.Point(36, 552);
            this.cbTLUSAB.Name = "cbTLUSAB";
            this.cbTLUSAB.Size = new System.Drawing.Size(68, 17);
            this.cbTLUSAB.TabIndex = 79;
            this.cbTLUSAB.Text = "TLUSAB";
            this.cbTLUSAB.UseVisualStyleBackColor = true;
            // 
            // cbZAYO
            // 
            this.cbZAYO.AutoSize = true;
            this.cbZAYO.Location = new System.Drawing.Point(36, 713);
            this.cbZAYO.Name = "cbZAYO";
            this.cbZAYO.Size = new System.Drawing.Size(55, 17);
            this.cbZAYO.TabIndex = 94;
            this.cbZAYO.Text = "ZAYO";
            this.cbZAYO.UseVisualStyleBackColor = true;
            // 
            // cbXCI
            // 
            this.cbXCI.AutoSize = true;
            this.cbXCI.Location = new System.Drawing.Point(36, 691);
            this.cbXCI.Name = "cbXCI";
            this.cbXCI.Size = new System.Drawing.Size(43, 17);
            this.cbXCI.TabIndex = 91;
            this.cbXCI.Text = "XCI";
            this.cbXCI.UseVisualStyleBackColor = true;
            // 
            // cbWIREIE
            // 
            this.cbWIREIE.AutoSize = true;
            this.cbWIREIE.Location = new System.Drawing.Point(36, 666);
            this.cbWIREIE.Name = "cbWIREIE";
            this.cbWIREIE.Size = new System.Drawing.Size(65, 17);
            this.cbWIREIE.TabIndex = 90;
            this.cbWIREIE.Text = "WIREIE";
            this.cbWIREIE.UseVisualStyleBackColor = true;
            // 
            // cbVDTR
            // 
            this.cbVDTR.AutoSize = true;
            this.cbVDTR.Location = new System.Drawing.Point(36, 643);
            this.cbVDTR.Name = "cbVDTR";
            this.cbVDTR.Size = new System.Drawing.Size(56, 17);
            this.cbVDTR.TabIndex = 87;
            this.cbVDTR.Text = "VDTR";
            this.cbVDTR.UseVisualStyleBackColor = true;
            // 
            // labelMember
            // 
            this.labelMember.AutoSize = true;
            this.labelMember.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMember.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelMember.Location = new System.Drawing.Point(50, 70);
            this.labelMember.Name = "labelMember";
            this.labelMember.Size = new System.Drawing.Size(65, 17);
            this.labelMember.TabIndex = 95;
            this.labelMember.Text = "Member";
            // 
            // labelEmailAddress
            // 
            this.labelEmailAddress.AutoSize = true;
            this.labelEmailAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelEmailAddress.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelEmailAddress.Location = new System.Drawing.Point(224, 71);
            this.labelEmailAddress.Name = "labelEmailAddress";
            this.labelEmailAddress.Size = new System.Drawing.Size(111, 17);
            this.labelEmailAddress.TabIndex = 96;
            this.labelEmailAddress.Text = "Email Address";
            // 
            // tbABCCOM1stLine
            // 
            this.tbABCCOM1stLine.Location = new System.Drawing.Point(435, 96);
            this.tbABCCOM1stLine.Name = "tbABCCOM1stLine";
            this.tbABCCOM1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbABCCOM1stLine.TabIndex = 97;
            // 
            // labelSalutation
            // 
            this.labelSalutation.AutoSize = true;
            this.labelSalutation.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSalutation.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.labelSalutation.Location = new System.Drawing.Point(498, 71);
            this.labelSalutation.Name = "labelSalutation";
            this.labelSalutation.Size = new System.Drawing.Size(81, 17);
            this.labelSalutation.TabIndex = 98;
            this.labelSalutation.Text = "Salutation";
            // 
            // tbALIANT1stLine
            // 
            this.tbALIANT1stLine.Location = new System.Drawing.Point(435, 119);
            this.tbALIANT1stLine.Name = "tbALIANT1stLine";
            this.tbALIANT1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbALIANT1stLine.TabIndex = 99;
            // 
            // tbHYQU1stLine
            // 
            this.tbHYQU1stLine.Location = new System.Drawing.Point(435, 301);
            this.tbHYQU1stLine.Name = "tbHYQU1stLine";
            this.tbHYQU1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbHYQU1stLine.TabIndex = 100;
            // 
            // tbHYONE1stLine
            // 
            this.tbHYONE1stLine.Location = new System.Drawing.Point(435, 278);
            this.tbHYONE1stLine.Name = "tbHYONE1stLine";
            this.tbHYONE1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbHYONE1stLine.TabIndex = 101;
            // 
            // tbGLW1stLine
            // 
            this.tbGLW1stLine.Location = new System.Drawing.Point(435, 256);
            this.tbGLW1stLine.Name = "tbGLW1stLine";
            this.tbGLW1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbGLW1stLine.TabIndex = 102;
            // 
            // tbDND1stLine
            // 
            this.tbDND1stLine.Location = new System.Drawing.Point(435, 233);
            this.tbDND1stLine.Name = "tbDND1stLine";
            this.tbDND1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbDND1stLine.TabIndex = 103;
            // 
            // tbBRAGG1stLine
            // 
            this.tbBRAGG1stLine.Location = new System.Drawing.Point(435, 210);
            this.tbBRAGG1stLine.Name = "tbBRAGG1stLine";
            this.tbBRAGG1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbBRAGG1stLine.TabIndex = 104;
            // 
            // tbBMCE1stLine
            // 
            this.tbBMCE1stLine.Location = new System.Drawing.Point(435, 187);
            this.tbBMCE1stLine.Name = "tbBMCE1stLine";
            this.tbBMCE1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbBMCE1stLine.TabIndex = 105;
            // 
            // tbBELL1stLine
            // 
            this.tbBELL1stLine.Location = new System.Drawing.Point(435, 165);
            this.tbBELL1stLine.Name = "tbBELL1stLine";
            this.tbBELL1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbBELL1stLine.TabIndex = 106;
            // 
            // tbBCHY1stLine
            // 
            this.tbBCHY1stLine.Location = new System.Drawing.Point(435, 142);
            this.tbBCHY1stLine.Name = "tbBCHY1stLine";
            this.tbBCHY1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbBCHY1stLine.TabIndex = 107;
            // 
            // tbMTS1stLine
            // 
            this.tbMTS1stLine.Location = new System.Drawing.Point(435, 324);
            this.tbMTS1stLine.Name = "tbMTS1stLine";
            this.tbMTS1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbMTS1stLine.TabIndex = 108;
            // 
            // tbRCTL1stLine
            // 
            this.tbRCTL1stLine.Location = new System.Drawing.Point(435, 438);
            this.tbRCTL1stLine.Name = "tbRCTL1stLine";
            this.tbRCTL1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbRCTL1stLine.TabIndex = 109;
            // 
            // tbONT1stLine
            // 
            this.tbONT1stLine.Location = new System.Drawing.Point(435, 415);
            this.tbONT1stLine.Name = "tbONT1stLine";
            this.tbONT1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbONT1stLine.TabIndex = 110;
            // 
            // tbNAVI1stLine
            // 
            this.tbNAVI1stLine.Location = new System.Drawing.Point(435, 347);
            this.tbNAVI1stLine.Name = "tbNAVI1stLine";
            this.tbNAVI1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbNAVI1stLine.TabIndex = 111;
            // 
            // tbNWT1stLine
            // 
            this.tbNWT1stLine.Location = new System.Drawing.Point(435, 392);
            this.tbNWT1stLine.Name = "tbNWT1stLine";
            this.tbNWT1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbNWT1stLine.TabIndex = 112;
            // 
            // tbNTTEL1stLine
            // 
            this.tbNTTEL1stLine.Location = new System.Drawing.Point(435, 369);
            this.tbNTTEL1stLine.Name = "tbNTTEL1stLine";
            this.tbNTTEL1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbNTTEL1stLine.TabIndex = 113;
            // 
            // tbTLUSBC1stLine
            // 
            this.tbTLUSBC1stLine.Location = new System.Drawing.Point(435, 574);
            this.tbTLUSBC1stLine.Name = "tbTLUSBC1stLine";
            this.tbTLUSBC1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbTLUSBC1stLine.TabIndex = 114;
            // 
            // tbSHAW1stLine
            // 
            this.tbSHAW1stLine.Location = new System.Drawing.Point(435, 460);
            this.tbSHAW1stLine.Name = "tbSHAW1stLine";
            this.tbSHAW1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbSHAW1stLine.TabIndex = 115;
            // 
            // tbSTEL1stLine
            // 
            this.tbSTEL1stLine.Location = new System.Drawing.Point(435, 483);
            this.tbSTEL1stLine.Name = "tbSTEL1stLine";
            this.tbSTEL1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbSTEL1stLine.TabIndex = 116;
            // 
            // tbTLUSAB1stLine
            // 
            this.tbTLUSAB1stLine.Location = new System.Drawing.Point(435, 551);
            this.tbTLUSAB1stLine.Name = "tbTLUSAB1stLine";
            this.tbTLUSAB1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbTLUSAB1stLine.TabIndex = 117;
            // 
            // tbTERAGO1stLine
            // 
            this.tbTERAGO1stLine.Location = new System.Drawing.Point(435, 529);
            this.tbTERAGO1stLine.Name = "tbTERAGO1stLine";
            this.tbTERAGO1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbTERAGO1stLine.TabIndex = 118;
            // 
            // tbTBAY1stLine
            // 
            this.tbTBAY1stLine.Location = new System.Drawing.Point(435, 506);
            this.tbTBAY1stLine.Name = "tbTBAY1stLine";
            this.tbTBAY1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbTBAY1stLine.TabIndex = 119;
            // 
            // tbZAYO1stLine
            // 
            this.tbZAYO1stLine.Location = new System.Drawing.Point(435, 712);
            this.tbZAYO1stLine.Name = "tbZAYO1stLine";
            this.tbZAYO1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbZAYO1stLine.TabIndex = 120;
            // 
            // tbXCI1stLine
            // 
            this.tbXCI1stLine.Location = new System.Drawing.Point(435, 689);
            this.tbXCI1stLine.Name = "tbXCI1stLine";
            this.tbXCI1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbXCI1stLine.TabIndex = 121;
            // 
            // tbWIREIE1stLine
            // 
            this.tbWIREIE1stLine.Location = new System.Drawing.Point(435, 666);
            this.tbWIREIE1stLine.Name = "tbWIREIE1stLine";
            this.tbWIREIE1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbWIREIE1stLine.TabIndex = 122;
            // 
            // tbVDTR1stLine
            // 
            this.tbVDTR1stLine.Location = new System.Drawing.Point(435, 643);
            this.tbVDTR1stLine.Name = "tbVDTR1stLine";
            this.tbVDTR1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbVDTR1stLine.TabIndex = 123;
            // 
            // tbTLUSQC1stLine
            // 
            this.tbTLUSQC1stLine.Location = new System.Drawing.Point(435, 620);
            this.tbTLUSQC1stLine.Name = "tbTLUSQC1stLine";
            this.tbTLUSQC1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbTLUSQC1stLine.TabIndex = 124;
            // 
            // tbTLUSMC1stLine
            // 
            this.tbTLUSMC1stLine.Location = new System.Drawing.Point(435, 597);
            this.tbTLUSMC1stLine.Name = "tbTLUSMC1stLine";
            this.tbTLUSMC1stLine.Size = new System.Drawing.Size(208, 20);
            this.tbTLUSMC1stLine.TabIndex = 125;
            // 
            // bSend
            // 
            this.bSend.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.bSend.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bSend.ForeColor = System.Drawing.Color.White;
            this.bSend.Location = new System.Drawing.Point(653, 96);
            this.bSend.Name = "bSend";
            this.bSend.Size = new System.Drawing.Size(137, 71);
            this.bSend.TabIndex = 126;
            this.bSend.Text = "Send Email(s)";
            this.bSend.UseVisualStyleBackColor = false;
            this.bSend.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bSendEmail_clicked);
            // 
            // bCheckData
            // 
            this.bCheckData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.bCheckData.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bCheckData.ForeColor = System.Drawing.Color.Black;
            this.bCheckData.Location = new System.Drawing.Point(653, 183);
            this.bCheckData.Name = "bCheckData";
            this.bCheckData.Size = new System.Drawing.Size(137, 71);
            this.bCheckData.TabIndex = 127;
            this.bCheckData.Text = "Check Data";
            this.bCheckData.UseVisualStyleBackColor = false;
            this.bCheckData.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bCheckData_Clicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(132, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 128;
            this.label1.Text = "Reports folder:";
            // 
            // bSelectAll
            // 
            this.bSelectAll.Location = new System.Drawing.Point(10, 7);
            this.bSelectAll.Name = "bSelectAll";
            this.bSelectAll.Size = new System.Drawing.Size(75, 23);
            this.bSelectAll.TabIndex = 129;
            this.bSelectAll.Text = "Select All";
            this.bSelectAll.UseVisualStyleBackColor = true;
            this.bSelectAll.Click += new System.EventHandler(this.bSelectAll_Clicked);
            // 
            // bSelectNone
            // 
            this.bSelectNone.Location = new System.Drawing.Point(9, 35);
            this.bSelectNone.Name = "bSelectNone";
            this.bSelectNone.Size = new System.Drawing.Size(75, 23);
            this.bSelectNone.TabIndex = 130;
            this.bSelectNone.Text = "Select None";
            this.bSelectNone.UseVisualStyleBackColor = true;
            this.bSelectNone.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bSelectNone_Clicked);
            // 
            // bUserGuide
            // 
            this.bUserGuide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(255)))));
            this.bUserGuide.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bUserGuide.Location = new System.Drawing.Point(653, 668);
            this.bUserGuide.Name = "bUserGuide";
            this.bUserGuide.Size = new System.Drawing.Size(135, 64);
            this.bUserGuide.TabIndex = 131;
            this.bUserGuide.Text = "User Guide";
            this.bUserGuide.UseVisualStyleBackColor = false;
            this.bUserGuide.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bUserGuide_Clicked);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(800, 804);
            this.Controls.Add(this.bUserGuide);
            this.Controls.Add(this.bSelectNone);
            this.Controls.Add(this.bSelectAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bCheckData);
            this.Controls.Add(this.bSend);
            this.Controls.Add(this.tbTLUSMC1stLine);
            this.Controls.Add(this.tbTLUSQC1stLine);
            this.Controls.Add(this.tbVDTR1stLine);
            this.Controls.Add(this.tbWIREIE1stLine);
            this.Controls.Add(this.tbXCI1stLine);
            this.Controls.Add(this.tbZAYO1stLine);
            this.Controls.Add(this.tbTBAY1stLine);
            this.Controls.Add(this.tbTERAGO1stLine);
            this.Controls.Add(this.tbTLUSAB1stLine);
            this.Controls.Add(this.tbSTEL1stLine);
            this.Controls.Add(this.tbSHAW1stLine);
            this.Controls.Add(this.tbTLUSBC1stLine);
            this.Controls.Add(this.tbNTTEL1stLine);
            this.Controls.Add(this.tbNWT1stLine);
            this.Controls.Add(this.tbNAVI1stLine);
            this.Controls.Add(this.tbONT1stLine);
            this.Controls.Add(this.tbRCTL1stLine);
            this.Controls.Add(this.tbMTS1stLine);
            this.Controls.Add(this.tbBCHY1stLine);
            this.Controls.Add(this.tbBELL1stLine);
            this.Controls.Add(this.tbBMCE1stLine);
            this.Controls.Add(this.tbBRAGG1stLine);
            this.Controls.Add(this.tbDND1stLine);
            this.Controls.Add(this.tbGLW1stLine);
            this.Controls.Add(this.tbHYONE1stLine);
            this.Controls.Add(this.tbHYQU1stLine);
            this.Controls.Add(this.tbALIANT1stLine);
            this.Controls.Add(this.labelSalutation);
            this.Controls.Add(this.tbABCCOM1stLine);
            this.Controls.Add(this.labelEmailAddress);
            this.Controls.Add(this.labelMember);
            this.Controls.Add(this.cbZAYO);
            this.Controls.Add(this.cbXCI);
            this.Controls.Add(this.cbWIREIE);
            this.Controls.Add(this.cbVDTR);
            this.Controls.Add(this.cbTLUSQC);
            this.Controls.Add(this.cbTLUSMC);
            this.Controls.Add(this.cbTLUSBC);
            this.Controls.Add(this.cbTLUSAB);
            this.Controls.Add(this.cbTERAGO);
            this.Controls.Add(this.cbTBAY);
            this.Controls.Add(this.cbSTEL);
            this.Controls.Add(this.cbSHAW);
            this.Controls.Add(this.cbRCTL);
            this.Controls.Add(this.cbONT);
            this.Controls.Add(this.cbNWT);
            this.Controls.Add(this.cbNTTEL);
            this.Controls.Add(this.cbNAVI);
            this.Controls.Add(this.cbMTS);
            this.Controls.Add(this.cbHYQU);
            this.Controls.Add(this.cbHYONE);
            this.Controls.Add(this.cbGLW);
            this.Controls.Add(this.cbDND);
            this.Controls.Add(this.cbBRAGG);
            this.Controls.Add(this.cbBMCE);
            this.Controls.Add(this.tbZAYOemadd);
            this.Controls.Add(this.tbXCIemadd);
            this.Controls.Add(this.tbWIREIEemadd);
            this.Controls.Add(this.tbVDTRemadd);
            this.Controls.Add(this.tbTLUSQCemadd);
            this.Controls.Add(this.tbTLUSMCemadd);
            this.Controls.Add(this.tbTLUSBCemadd);
            this.Controls.Add(this.tbTLUSABemadd);
            this.Controls.Add(this.tbTERAGOemadd);
            this.Controls.Add(this.tbTBAYemadd);
            this.Controls.Add(this.tbSTELemadd);
            this.Controls.Add(this.tbSHAWemadd);
            this.Controls.Add(this.tbRCTLemadd);
            this.Controls.Add(this.tbONTemadd);
            this.Controls.Add(this.tbNWTemadd);
            this.Controls.Add(this.tbNTTELemadd);
            this.Controls.Add(this.tbNAVIemadd);
            this.Controls.Add(this.tbMTSemadd);
            this.Controls.Add(this.tbHYQUemadd);
            this.Controls.Add(this.tbHYONEemadd);
            this.Controls.Add(this.tbGLWemadd);
            this.Controls.Add(this.tbDNDemadd);
            this.Controls.Add(this.tbBRAGGemadd);
            this.Controls.Add(this.tbBMCEemadd);
            this.Controls.Add(this.tbBELLemadd);
            this.Controls.Add(this.cbBELL);
            this.Controls.Add(this.tbBCHYemadd);
            this.Controls.Add(this.cbBCHY);
            this.Controls.Add(this.tbALIANTemadd);
            this.Controls.Add(this.cbALIANT);
            this.Controls.Add(this.bExit);
            this.Controls.Add(this.bSave);
            this.Controls.Add(this.tbABCCOMemadd);
            this.Controls.Add(this.bBrowse);
            this.Controls.Add(this.tbDirectoryPath);
            this.Controls.Add(this.cbABCCOM);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Bulk Sending of LAML Reports";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.CheckBox cbABCCOM;
        private System.Windows.Forms.TextBox tbDirectoryPath;
        private System.Windows.Forms.Button bBrowse;
        private System.Windows.Forms.TextBox tbABCCOMemadd;
        private System.Windows.Forms.Button bSave;
        private System.Windows.Forms.Button bExit;
        private System.Windows.Forms.CheckBox cbALIANT;
        private System.Windows.Forms.TextBox tbALIANTemadd;
        private System.Windows.Forms.TextBox tbBELLemadd;
        private System.Windows.Forms.CheckBox cbBELL;
        private System.Windows.Forms.TextBox tbBCHYemadd;
        private System.Windows.Forms.CheckBox cbBCHY;
        private System.Windows.Forms.TextBox tbGLWemadd;
        private System.Windows.Forms.TextBox tbDNDemadd;
        private System.Windows.Forms.TextBox tbBRAGGemadd;
        private System.Windows.Forms.TextBox tbBMCEemadd;
        private System.Windows.Forms.TextBox tbRCTLemadd;
        private System.Windows.Forms.TextBox tbONTemadd;
        private System.Windows.Forms.TextBox tbNWTemadd;
        private System.Windows.Forms.TextBox tbNTTELemadd;
        private System.Windows.Forms.TextBox tbNAVIemadd;
        private System.Windows.Forms.TextBox tbMTSemadd;
        private System.Windows.Forms.TextBox tbHYQUemadd;
        private System.Windows.Forms.TextBox tbHYONEemadd;
        private System.Windows.Forms.TextBox tbTLUSQCemadd;
        private System.Windows.Forms.TextBox tbTLUSMCemadd;
        private System.Windows.Forms.TextBox tbTLUSBCemadd;
        private System.Windows.Forms.TextBox tbTLUSABemadd;
        private System.Windows.Forms.TextBox tbTERAGOemadd;
        private System.Windows.Forms.TextBox tbTBAYemadd;
        private System.Windows.Forms.TextBox tbSTELemadd;
        private System.Windows.Forms.TextBox tbSHAWemadd;
        private System.Windows.Forms.TextBox tbZAYOemadd;
        private System.Windows.Forms.TextBox tbXCIemadd;
        private System.Windows.Forms.TextBox tbWIREIEemadd;
        private System.Windows.Forms.TextBox tbVDTRemadd;
        private System.Windows.Forms.CheckBox cbGLW;
        private System.Windows.Forms.CheckBox cbDND;
        private System.Windows.Forms.CheckBox cbBRAGG;
        private System.Windows.Forms.CheckBox cbBMCE;
        private System.Windows.Forms.CheckBox cbNAVI;
        private System.Windows.Forms.CheckBox cbMTS;
        private System.Windows.Forms.CheckBox cbHYQU;
        private System.Windows.Forms.CheckBox cbHYONE;
        private System.Windows.Forms.CheckBox cbRCTL;
        private System.Windows.Forms.CheckBox cbONT;
        private System.Windows.Forms.CheckBox cbNWT;
        private System.Windows.Forms.CheckBox cbNTTEL;
        private System.Windows.Forms.CheckBox cbTERAGO;
        private System.Windows.Forms.CheckBox cbTBAY;
        private System.Windows.Forms.CheckBox cbSTEL;
        private System.Windows.Forms.CheckBox cbSHAW;
        private System.Windows.Forms.CheckBox cbTLUSQC;
        private System.Windows.Forms.CheckBox cbTLUSMC;
        private System.Windows.Forms.CheckBox cbTLUSBC;
        private System.Windows.Forms.CheckBox cbTLUSAB;
        private System.Windows.Forms.CheckBox cbZAYO;
        private System.Windows.Forms.CheckBox cbXCI;
        private System.Windows.Forms.CheckBox cbWIREIE;
        private System.Windows.Forms.CheckBox cbVDTR;
        private System.Windows.Forms.Label labelMember;
        private System.Windows.Forms.Label labelEmailAddress;
        private System.Windows.Forms.TextBox tbABCCOM1stLine;
        private System.Windows.Forms.Label labelSalutation;
        private System.Windows.Forms.TextBox tbALIANT1stLine;
        private System.Windows.Forms.TextBox tbHYQU1stLine;
        private System.Windows.Forms.TextBox tbHYONE1stLine;
        private System.Windows.Forms.TextBox tbGLW1stLine;
        private System.Windows.Forms.TextBox tbDND1stLine;
        private System.Windows.Forms.TextBox tbBRAGG1stLine;
        private System.Windows.Forms.TextBox tbBMCE1stLine;
        private System.Windows.Forms.TextBox tbBELL1stLine;
        private System.Windows.Forms.TextBox tbBCHY1stLine;
        private System.Windows.Forms.TextBox tbMTS1stLine;
        private System.Windows.Forms.TextBox tbRCTL1stLine;
        private System.Windows.Forms.TextBox tbONT1stLine;
        private System.Windows.Forms.TextBox tbNAVI1stLine;
        private System.Windows.Forms.TextBox tbNWT1stLine;
        private System.Windows.Forms.TextBox tbNTTEL1stLine;
        private System.Windows.Forms.TextBox tbTLUSBC1stLine;
        private System.Windows.Forms.TextBox tbSHAW1stLine;
        private System.Windows.Forms.TextBox tbSTEL1stLine;
        private System.Windows.Forms.TextBox tbTLUSAB1stLine;
        private System.Windows.Forms.TextBox tbTERAGO1stLine;
        private System.Windows.Forms.TextBox tbTBAY1stLine;
        private System.Windows.Forms.TextBox tbZAYO1stLine;
        private System.Windows.Forms.TextBox tbXCI1stLine;
        private System.Windows.Forms.TextBox tbWIREIE1stLine;
        private System.Windows.Forms.TextBox tbVDTR1stLine;
        private System.Windows.Forms.TextBox tbTLUSQC1stLine;
        private System.Windows.Forms.TextBox tbTLUSMC1stLine;
        private System.Windows.Forms.Button bSend;
        private System.Windows.Forms.Button bCheckData;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bSelectAll;
        private System.Windows.Forms.Button bSelectNone;
        private System.Windows.Forms.Button bUserGuide;
    }
}


```
