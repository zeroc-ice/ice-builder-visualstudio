// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CSharpConfigurationView));
            this.lblOutputDirectory = new System.Windows.Forms.Label();
            this.lblSliceCompilerOptions = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnOutputDirectoryBrowse = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.referencedAssemblies = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAdditionalOptions = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkUnderscores = new System.Windows.Forms.CheckBox();
            this.chkIce = new System.Windows.Forms.CheckBox();
            this.chkStreaming = new System.Windows.Forms.CheckBox();
            this.chkChecksum = new System.Windows.Forms.CheckBox();
            this.chkTie = new System.Windows.Forms.CheckBox();
            this.includeDirectories = new IceBuilder.IncludeDirectories();
            this.groupBoxGeneral = new System.Windows.Forms.GroupBox();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
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
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Output Directory:";
            // 
            // txtOutputDir
            // 
            this.txtOutputDir.Location = new System.Drawing.Point(236, 2);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(323, 20);
            this.txtOutputDir.TabIndex = 17;
            this.tooltip.SetToolTip(this.txtOutputDir, "Directory where generated files are created.");
            this.txtOutputDir.Leave += new System.EventHandler(this.OutputDirectory_Leave);
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
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.referencedAssemblies);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.txtAdditionalOptions);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.chkUnderscores);
            this.groupBox2.Controls.Add(this.chkIce);
            this.groupBox2.Controls.Add(this.chkStreaming);
            this.groupBox2.Controls.Add(this.chkChecksum);
            this.groupBox2.Controls.Add(this.chkTie);
            this.groupBox2.Controls.Add(this.includeDirectories);
            this.groupBox2.Location = new System.Drawing.Point(9, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(640, 528);
            this.groupBox2.TabIndex = 50;
            this.groupBox2.TabStop = false;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 380);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(210, 13);
            this.label7.TabIndex = 55;
            this.label7.Text = "Add references to the following assemblies:";
            // 
            // referencedAssemblies
            // 
            this.referencedAssemblies.FormattingEnabled = true;
            this.referencedAssemblies.Location = new System.Drawing.Point(236, 380);
            this.referencedAssemblies.Name = "referencedAssemblies";
            this.referencedAssemblies.Size = new System.Drawing.Size(323, 139);
            this.referencedAssemblies.TabIndex = 53;
            this.referencedAssemblies.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ReferencedAssemblies_ItemChecked);
            // 
            // label4
            // 
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.Location = new System.Drawing.Point(118, 367);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(523, 2);
            this.label4.TabIndex = 52;
            // 
            // txtAdditionalOptions
            // 
            this.txtAdditionalOptions.Location = new System.Drawing.Point(236, 268);
            this.txtAdditionalOptions.Multiline = true;
            this.txtAdditionalOptions.Name = "txtAdditionalOptions";
            this.txtAdditionalOptions.Size = new System.Drawing.Size(323, 90);
            this.txtAdditionalOptions.TabIndex = 34;
            this.tooltip.SetToolTip(this.txtAdditionalOptions, "Additional command line options to pass to Slice compiler");
            this.txtAdditionalOptions.Leave += new System.EventHandler(this.AdditionalOptions_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(-3, 360);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 13);
            this.label5.TabIndex = 51;
            this.label5.Text = "Referenced Assemblies:";
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
            // chkUnderscores
            // 
            this.chkUnderscores.AutoSize = true;
            this.chkUnderscores.Location = new System.Drawing.Point(9, 22);
            this.chkUnderscores.Name = "chkUnderscores";
            this.chkUnderscores.Size = new System.Drawing.Size(170, 17);
            this.chkUnderscores.TabIndex = 46;
            this.chkUnderscores.Text = "Allow underscores in identifiers";
            this.tooltip.SetToolTip(this.chkUnderscores, "Allow underscores in Slice identifiers.");
            this.chkUnderscores.UseVisualStyleBackColor = true;
            this.chkUnderscores.CheckStateChanged += new System.EventHandler(this.Underscore_Changed);
            // 
            // chkIce
            // 
            this.chkIce.AutoSize = true;
            this.chkIce.Location = new System.Drawing.Point(9, 0);
            this.chkIce.Name = "chkIce";
            this.chkIce.Size = new System.Drawing.Size(160, 17);
            this.chkIce.TabIndex = 42;
            this.chkIce.Text = "Allow reserved Ice identifiers";
            this.tooltip.SetToolTip(this.chkIce, "Allow reserved Ice prefix in Slice identifiers.");
            this.chkIce.UseVisualStyleBackColor = true;
            this.chkIce.CheckedChanged += new System.EventHandler(this.AllowIcePrefix_CheckedChanged);
            // 
            // chkStreaming
            // 
            this.chkStreaming.AutoSize = true;
            this.chkStreaming.Location = new System.Drawing.Point(9, 44);
            this.chkStreaming.Name = "chkStreaming";
            this.chkStreaming.Size = new System.Drawing.Size(217, 17);
            this.chkStreaming.TabIndex = 43;
            this.chkStreaming.Text = "Generated helper functions for streaming";
            this.tooltip.SetToolTip(this.chkStreaming, "Generate marshaling support for public stream API.");
            this.chkStreaming.UseVisualStyleBackColor = true;
            this.chkStreaming.CheckStateChanged += new System.EventHandler(this.Stream_CheckedChanged);
            // 
            // chkChecksum
            // 
            this.chkChecksum.AutoSize = true;
            this.chkChecksum.Location = new System.Drawing.Point(9, 66);
            this.chkChecksum.Name = "chkChecksum";
            this.chkChecksum.Size = new System.Drawing.Size(153, 17);
            this.chkChecksum.TabIndex = 44;
            this.chkChecksum.Text = "Generate Slice checksums";
            this.tooltip.SetToolTip(this.chkChecksum, "Generate checksums for Slice definitions.");
            this.chkChecksum.UseVisualStyleBackColor = true;
            this.chkChecksum.CheckStateChanged += new System.EventHandler(this.Checksum_CheckedChanged);
            // 
            // chkTie
            // 
            this.chkTie.AutoSize = true;
            this.chkTie.Location = new System.Drawing.Point(9, 88);
            this.chkTie.Name = "chkTie";
            this.chkTie.Size = new System.Drawing.Size(122, 17);
            this.chkTie.TabIndex = 45;
            this.chkTie.Text = "Generate tie classes";
            this.tooltip.SetToolTip(this.chkTie, "Generate TIE classes.");
            this.chkTie.UseVisualStyleBackColor = true;
            this.chkTie.CheckStateChanged += new System.EventHandler(this.Tie_CheckedChanged);
            // 
            // includeDirectories
            // 
            this.includeDirectories.AutoSize = true;
            this.includeDirectories.InitialValues = ((System.Collections.Generic.List<string>)(resources.GetObject("includeDirectories.InitialValues")));
            this.includeDirectories.Location = new System.Drawing.Point(12, 105);
            this.includeDirectories.Margin = new System.Windows.Forms.Padding(0);
            this.includeDirectories.Name = "includeDirectories";
            this.includeDirectories.PropertyPage = null;
            this.includeDirectories.Size = new System.Drawing.Size(631, 158);
            this.includeDirectories.TabIndex = 46;
            this.includeDirectories.Values = ((System.Collections.Generic.List<string>)(resources.GetObject("includeDirectories.Values")));
            // 
            // groupBoxGeneral
            // 
            this.groupBoxGeneral.AutoSize = true;
            this.groupBoxGeneral.Controls.Add(this.groupBox2);
            this.groupBoxGeneral.Controls.Add(this.label3);
            this.groupBoxGeneral.Controls.Add(this.groupBox1);
            this.groupBoxGeneral.Controls.Add(this.label1);
            this.groupBoxGeneral.Controls.Add(this.lblSliceCompilerOptions);
            this.groupBoxGeneral.Controls.Add(this.lblOutputDirectory);
            this.groupBoxGeneral.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxGeneral.Location = new System.Drawing.Point(0, 0);
            this.groupBoxGeneral.Name = "groupBoxGeneral";
            this.groupBoxGeneral.Size = new System.Drawing.Size(665, 617);
            this.groupBoxGeneral.TabIndex = 5;
            this.groupBoxGeneral.TabStop = false;
            // 
            // CSharpConfigurationView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.groupBoxGeneral);
            this.Name = "CSharpConfigurationView";
            this.Size = new System.Drawing.Size(668, 620);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBoxGeneral.ResumeLayout(false);
            this.groupBoxGeneral.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblOutputDirectory;
        private System.Windows.Forms.Label lblSliceCompilerOptions;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnOutputDirectoryBrowse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtAdditionalOptions;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkUnderscores;
        private System.Windows.Forms.CheckBox chkIce;
        private System.Windows.Forms.CheckBox chkStreaming;
        private System.Windows.Forms.CheckBox chkChecksum;
        private System.Windows.Forms.CheckBox chkTie;
        private IncludeDirectories includeDirectories;
        private System.Windows.Forms.GroupBox groupBoxGeneral;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.CheckedListBox referencedAssemblies;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;


    }
}
