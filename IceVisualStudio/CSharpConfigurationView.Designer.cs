namespace IceCustomProject
{
    partial class CSharpConfigurationView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblOutputDirectory = new System.Windows.Forms.Label();
            this.lblSliceCompilerOptions = new System.Windows.Forms.Label();
            this.lblTracingLevel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnOutputDirectoryBrowse = new System.Windows.Forms.Button();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkTie = new System.Windows.Forms.CheckBox();
            this.chkChecksum = new System.Windows.Forms.CheckBox();
            this.chkStreaming = new System.Windows.Forms.CheckBox();
            this.chkIce = new System.Windows.Forms.CheckBox();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.checkedListBox1 = new System.Windows.Forms.CheckedListBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtAdditionalOptions = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.cmbTracingLevel = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBoxGeneral.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblOutputDirectory
            // 
            this.lblOutputDirectory.AutoSize = true;
            this.lblOutputDirectory.Location = new System.Drawing.Point(2, 3);
            this.lblOutputDirectory.Name = "lblOutputDirectory";
            this.lblOutputDirectory.Size = new System.Drawing.Size(42, 13);
            this.lblOutputDirectory.TabIndex = 4;
            this.lblOutputDirectory.Text = "Output:";
            // 
            // lblSliceCompilerOptions
            // 
            this.lblSliceCompilerOptions.AutoSize = true;
            this.lblSliceCompilerOptions.Location = new System.Drawing.Point(2, 54);
            this.lblSliceCompilerOptions.Name = "lblSliceCompilerOptions";
            this.lblSliceCompilerOptions.Size = new System.Drawing.Size(115, 13);
            this.lblSliceCompilerOptions.TabIndex = 27;
            this.lblSliceCompilerOptions.Text = "Slice Compiler Options:";
            // 
            // lblTracingLevel
            // 
            this.lblTracingLevel.AutoSize = true;
            this.lblTracingLevel.Location = new System.Drawing.Point(2, 415);
            this.lblTracingLevel.Name = "lblTracingLevel";
            this.lblTracingLevel.Size = new System.Drawing.Size(38, 13);
            this.lblTracingLevel.TabIndex = 29;
            this.lblTracingLevel.Text = "Trace:";
            this.lblTracingLevel.Click += new System.EventHandler(this.lblTracingLevel_Click);
            // 
            // label1
            // 
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(50, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(599, 2);
            this.label1.TabIndex = 46;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtOutputDir);
            this.groupBox1.Controls.Add(this.btnOutputDirectoryBrowse);
            this.groupBox1.Location = new System.Drawing.Point(9, 24);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(640, 31);
            this.groupBox1.TabIndex = 48;
            this.groupBox1.TabStop = false;
            // 
            // btnOutputDirectoryBrowse
            // 
            this.btnOutputDirectoryBrowse.Location = new System.Drawing.Point(565, 2);
            this.btnOutputDirectoryBrowse.Name = "btnOutputDirectoryBrowse";
            this.btnOutputDirectoryBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnOutputDirectoryBrowse.TabIndex = 18;
            this.btnOutputDirectoryBrowse.Text = "Browse...";
            this.btnOutputDirectoryBrowse.UseVisualStyleBackColor = true;
            this.btnOutputDirectoryBrowse.Click += new System.EventHandler(this.btnOutputDirectoryBrowse_Click);
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Location = new System.Drawing.Point(236, 0);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(323, 20);
            this.txtOutputDir.TabIndex = 17;
            this.txtOutputDir.TextChanged += new System.EventHandler(this.txtOutputDir_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Output Directory:";
            // 
            // label3
            // 
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Location = new System.Drawing.Point(123, 61);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(523, 2);
            this.label3.TabIndex = 49;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtAdditionalOptions);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.btnAdd);
            this.groupBox2.Controls.Add(this.btnEdit);
            this.groupBox2.Controls.Add(this.checkedListBox1);
            this.groupBox2.Controls.Add(this.btnRemove);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.btnUp);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.btnDown);
            this.groupBox2.Controls.Add(this.chkIce);
            this.groupBox2.Controls.Add(this.chkStreaming);
            this.groupBox2.Controls.Add(this.chkChecksum);
            this.groupBox2.Controls.Add(this.chkTie);
            this.groupBox2.Location = new System.Drawing.Point(9, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(640, 342);
            this.groupBox2.TabIndex = 50;
            this.groupBox2.TabStop = false;
            // 
            // chkTie
            // 
            this.chkTie.AutoSize = true;
            this.chkTie.Location = new System.Drawing.Point(9, 88);
            this.chkTie.Name = "chkTie";
            this.chkTie.Size = new System.Drawing.Size(128, 17);
            this.chkTie.TabIndex = 45;
            this.chkTie.Text = "Generate TIE classes";
            this.chkTie.UseVisualStyleBackColor = true;
            // 
            // chkChecksum
            // 
            this.chkChecksum.AutoSize = true;
            this.chkChecksum.Location = new System.Drawing.Point(9, 66);
            this.chkChecksum.Name = "chkChecksum";
            this.chkChecksum.Size = new System.Drawing.Size(153, 17);
            this.chkChecksum.TabIndex = 44;
            this.chkChecksum.Text = "Generate Slice checksums";
            this.chkChecksum.UseVisualStyleBackColor = true;
            // 
            // chkStreaming
            // 
            this.chkStreaming.AutoSize = true;
            this.chkStreaming.Location = new System.Drawing.Point(9, 44);
            this.chkStreaming.Name = "chkStreaming";
            this.chkStreaming.Size = new System.Drawing.Size(217, 17);
            this.chkStreaming.TabIndex = 43;
            this.chkStreaming.Text = "Generated helper functions for streaming";
            this.chkStreaming.UseVisualStyleBackColor = true;
            // 
            // chkIce
            // 
            this.chkIce.AutoSize = true;
            this.chkIce.Location = new System.Drawing.Point(9, 0);
            this.chkIce.Name = "chkIce";
            this.chkIce.Size = new System.Drawing.Size(160, 17);
            this.chkIce.TabIndex = 42;
            this.chkIce.Text = "Allow reserved Ice identifiers";
            this.chkIce.UseVisualStyleBackColor = true;
            // 
            // btnDown
            // 
            this.btnDown.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDown.Location = new System.Drawing.Point(562, 196);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 41;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            // 
            // btnUp
            // 
            this.btnUp.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnUp.Location = new System.Drawing.Point(562, 174);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 40;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            // 
            // btnRemove
            // 
            this.btnRemove.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRemove.Location = new System.Drawing.Point(562, 152);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 39;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            // 
            // btnEdit
            // 
            this.btnEdit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnEdit.Location = new System.Drawing.Point(562, 130);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 38;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            // 
            // btnAdd
            // 
            this.btnAdd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAdd.Location = new System.Drawing.Point(562, 108);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 37;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            // 
            // checkedListBox1
            // 
            this.checkedListBox1.FormattingEnabled = true;
            this.checkedListBox1.Location = new System.Drawing.Point(236, 108);
            this.checkedListBox1.Name = "checkedListBox1";
            this.checkedListBox1.Size = new System.Drawing.Size(323, 154);
            this.checkedListBox1.TabIndex = 35;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(147, 13);
            this.label9.TabIndex = 50;
            this.label9.Text = "Additional Include Directories:";
            // 
            // txtAdditionalOptions
            // 
            this.txtAdditionalOptions.Location = new System.Drawing.Point(236, 268);
            this.txtAdditionalOptions.Multiline = true;
            this.txtAdditionalOptions.Name = "txtAdditionalOptions";
            this.txtAdditionalOptions.Size = new System.Drawing.Size(323, 69);
            this.txtAdditionalOptions.TabIndex = 34;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 268);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "Additional Options:";
            // 
            // label6
            // 
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label6.Location = new System.Drawing.Point(46, 422);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(600, 2);
            this.label6.TabIndex = 55;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.cmbTracingLevel);
            this.groupBox5.Location = new System.Drawing.Point(9, 431);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(637, 31);
            this.groupBox5.TabIndex = 56;
            this.groupBox5.TabStop = false;
            // 
            // cmbTracingLevel
            // 
            this.cmbTracingLevel.FormattingEnabled = true;
            this.cmbTracingLevel.Items.AddRange(new object[] {
            "Errors Only",
            "Info",
            "Debug"});
            this.cmbTracingLevel.Location = new System.Drawing.Point(236, 6);
            this.cmbTracingLevel.Name = "cmbTracingLevel";
            this.cmbTracingLevel.Size = new System.Drawing.Size(323, 21);
            this.cmbTracingLevel.TabIndex = 35;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 6);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 13);
            this.label7.TabIndex = 48;
            this.label7.Text = "Trace Level:";
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.Controls.Add(this.groupBox5);
            this.groupBoxGeneral.Controls.Add(this.label6);
            this.groupBoxGeneral.Controls.Add(this.groupBox2);
            this.groupBoxGeneral.Controls.Add(this.label3);
            this.groupBoxGeneral.Controls.Add(this.groupBox1);
            this.groupBoxGeneral.Controls.Add(this.label1);
            this.groupBoxGeneral.Controls.Add(this.lblTracingLevel);
            this.groupBoxGeneral.Controls.Add(this.lblSliceCompilerOptions);
            this.groupBoxGeneral.Controls.Add(this.lblOutputDirectory);
            this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxGeneral.Location = new System.Drawing.Point(0, 0);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(665, 465);
            this.groupBoxGeneral.TabIndex = 5;
            this.groupBoxGeneral.TabStop = false;
            this.groupBoxGeneral.Enter += new System.EventHandler(this.groupBoxGeneral_Enter);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(9, 22);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(170, 17);
            this.checkBox1.TabIndex = 46;
            this.checkBox1.Text = "Allow underscores in identifiers";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // CSharpConfigurationView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBoxGeneral);
            this.Name = "CSharpConfigurationView";
            this.Size = new System.Drawing.Size(685, 466);
            this.Load += new System.EventHandler(this.CSharpConfigurationView_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblOutputDirectory;
        private System.Windows.Forms.Label lblSliceCompilerOptions;
        private System.Windows.Forms.Label lblTracingLevel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnOutputDirectoryBrowse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox chkIce;
        private System.Windows.Forms.CheckBox chkStreaming;
        private System.Windows.Forms.CheckBox chkChecksum;
        private System.Windows.Forms.CheckBox chkTie;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.CheckedListBox checkedListBox1;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtAdditionalOptions;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmbTracingLevel;
        private System.Windows.Forms.GroupBox groupBoxGeneral;

    }
}
