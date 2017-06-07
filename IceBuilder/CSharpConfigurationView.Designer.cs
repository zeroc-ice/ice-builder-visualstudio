// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnOutputDirectoryBrowse = new System.Windows.Forms.Button();
            this.referencedAssemblies = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtAdditionalOptions = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.chkChecksum = new System.Windows.Forms.CheckBox();
            this.chkTie = new System.Windows.Forms.CheckBox();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.includeDirectories = new IceBuilder.IncludeDirectories();
            this.SuspendLayout();
            //
            // lblOutputDirectory
            //
            this.lblOutputDirectory.AutoSize = true;
            this.lblOutputDirectory.Location = new System.Drawing.Point(3, 9);
            this.lblOutputDirectory.Name = "lblOutputDirectory";
            this.lblOutputDirectory.Size = new System.Drawing.Size(42, 13);
            this.lblOutputDirectory.TabIndex = 4;
            this.lblOutputDirectory.Text = "Output:";
            //
            // lblSliceCompilerOptions
            //
            this.lblSliceCompilerOptions.AutoSize = true;
            this.lblSliceCompilerOptions.Location = new System.Drawing.Point(3, 59);
            this.lblSliceCompilerOptions.Name = "lblSliceCompilerOptions";
            this.lblSliceCompilerOptions.Size = new System.Drawing.Size(115, 13);
            this.lblSliceCompilerOptions.TabIndex = 27;
            this.lblSliceCompilerOptions.Text = "Slice Compiler Options:";
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 47;
            this.label2.Text = "Output Directory:";
            //
            // txtOutputDir
            //
            this.txtOutputDir.Location = new System.Drawing.Point(112, 28);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(322, 20);
            this.txtOutputDir.TabIndex = 17;
            this.tooltip.SetToolTip(this.txtOutputDir, "Directory where generated files are created.");
            this.txtOutputDir.TextChanged += new System.EventHandler(this.txtOutputDir_TextChanged);
            this.txtOutputDir.Leave += new System.EventHandler(this.OutputDirectory_Leave);
            //
            // btnOutputDirectoryBrowse
            //
            this.btnOutputDirectoryBrowse.Location = new System.Drawing.Point(440, 26);
            this.btnOutputDirectoryBrowse.Name = "btnOutputDirectoryBrowse";
            this.btnOutputDirectoryBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnOutputDirectoryBrowse.TabIndex = 18;
            this.btnOutputDirectoryBrowse.Text = "Browse...";
            this.btnOutputDirectoryBrowse.UseVisualStyleBackColor = true;
            this.btnOutputDirectoryBrowse.Click += new System.EventHandler(this.btnOutputDirectoryBrowse_Click);
            //
            // referencedAssemblies
            //
            this.referencedAssemblies.FormattingEnabled = true;
            this.referencedAssemblies.Location = new System.Drawing.Point(112, 422);
            this.referencedAssemblies.Name = "referencedAssemblies";
            this.referencedAssemblies.Size = new System.Drawing.Size(322, 139);
            this.referencedAssemblies.TabIndex = 53;
            this.referencedAssemblies.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ReferencedAssemblies_ItemChecked);
            //
            // label4
            //
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label4.Location = new System.Drawing.Point(128, 414);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(402, 1);
            this.label4.TabIndex = 52;
            //
            // txtAdditionalOptions
            //
            this.txtAdditionalOptions.Location = new System.Drawing.Point(112, 302);
            this.txtAdditionalOptions.Multiline = true;
            this.txtAdditionalOptions.Name = "txtAdditionalOptions";
            this.txtAdditionalOptions.Size = new System.Drawing.Size(322, 90);
            this.txtAdditionalOptions.TabIndex = 34;
            this.tooltip.SetToolTip(this.txtAdditionalOptions, "Additional command line options to pass to Slice compiler");
            this.txtAdditionalOptions.Leave += new System.EventHandler(this.AdditionalOptions_Leave);
            //
            // label5
            //
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 406);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 13);
            this.label5.TabIndex = 51;
            this.label5.Text = "Referenced Assemblies:";
            //
            // label8
            //
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 305);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(95, 13);
            this.label8.TabIndex = 49;
            this.label8.Text = "Additional Options:";
            //
            // chkChecksum
            //
            this.chkChecksum.AutoSize = true;
            this.chkChecksum.Location = new System.Drawing.Point(14, 80);
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
            this.chkTie.Location = new System.Drawing.Point(14, 102);
            this.chkTie.Name = "chkTie";
            this.chkTie.Size = new System.Drawing.Size(122, 17);
            this.chkTie.TabIndex = 45;
            this.chkTie.Text = "Generate tie classes";
            this.tooltip.SetToolTip(this.chkTie, "Generate TIE classes.");
            this.chkTie.UseVisualStyleBackColor = true;
            this.chkTie.CheckStateChanged += new System.EventHandler(this.Tie_CheckedChanged);
            //
            // label1
            //
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label1.Location = new System.Drawing.Point(120, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(412, 1);
            this.label1.TabIndex = 54;
            //
            // label3
            //
            this.label3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.label3.Location = new System.Drawing.Point(45, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(487, 1);
            this.label3.TabIndex = 55;
            //
            // includeDirectories
            //
            this.includeDirectories.AutoSize = true;
            this.includeDirectories.InitialValues = ((System.Collections.Generic.List<string>)(resources.GetObject("includeDirectories.InitialValues")));
            this.includeDirectories.Location = new System.Drawing.Point(6, 131);
            this.includeDirectories.Margin = new System.Windows.Forms.Padding(0);
            this.includeDirectories.Name = "includeDirectories";
            this.includeDirectories.PropertyPage = null;
            this.includeDirectories.Size = new System.Drawing.Size(518, 160);
            this.includeDirectories.TabIndex = 46;
            this.includeDirectories.Values = ((System.Collections.Generic.List<string>)(resources.GetObject("includeDirectories.Values")));
            //
            // CSharpConfigurationView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.referencedAssemblies);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtAdditionalOptions);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.lblOutputDirectory);
            this.Controls.Add(this.lblSliceCompilerOptions);
            this.Controls.Add(this.includeDirectories);
            this.Controls.Add(this.btnOutputDirectoryBrowse);
            this.Controls.Add(this.chkTie);
            this.Controls.Add(this.chkChecksum);
            this.Name = "CSharpConfigurationView";
            this.Size = new System.Drawing.Size(535, 570);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblOutputDirectory;
        private System.Windows.Forms.Label lblSliceCompilerOptions;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnOutputDirectoryBrowse;
        private System.Windows.Forms.TextBox txtAdditionalOptions;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox chkChecksum;
        private System.Windows.Forms.CheckBox chkTie;
        private IncludeDirectories includeDirectories;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.CheckedListBox referencedAssemblies;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
    }
}
