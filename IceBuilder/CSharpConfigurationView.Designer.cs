// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
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
            this.lblOutput = new System.Windows.Forms.Label();
            this.lblSliceCompilerOptions = new System.Windows.Forms.Label();
            this.lblOutputDirectory = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnOutputDirectoryBrowse = new System.Windows.Forms.Button();
            this.txtAdditionalOptions = new System.Windows.Forms.TextBox();
            this.lblAdditionalOptions = new System.Windows.Forms.Label();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.lblSliceCompilerOptionsSep = new System.Windows.Forms.Label();
            this.lblOutputSep = new System.Windows.Forms.Label();
            this.lblIncludeDirectories = new System.Windows.Forms.Label();
            this.txtIncludeDirectories = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            //
            // lblOutput
            //
            this.lblOutput.AutoSize = true;
            this.lblOutput.Location = new System.Drawing.Point(3, 9);
            this.lblOutput.Name = "lblOutput";
            this.lblOutput.Size = new System.Drawing.Size(42, 13);
            this.lblOutput.TabIndex = 4;
            this.lblOutput.Text = "Output:";
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
            // lblOutputDirectory
            //
            this.lblOutputDirectory.AutoSize = true;
            this.lblOutputDirectory.Location = new System.Drawing.Point(11, 28);
            this.lblOutputDirectory.Name = "lblOutputDirectory";
            this.lblOutputDirectory.Size = new System.Drawing.Size(87, 13);
            this.lblOutputDirectory.TabIndex = 47;
            this.lblOutputDirectory.Text = "Output Directory:";
            //
            // txtOutputDir
            //
            this.txtOutputDir.Location = new System.Drawing.Point(112, 28);
            this.txtOutputDir.Name = "txtOutputDir";
            this.txtOutputDir.Size = new System.Drawing.Size(322, 20);
            this.txtOutputDir.TabIndex = 17;
            this.tooltip.SetToolTip(this.txtOutputDir, "Directory where generated files are created.");
            this.txtOutputDir.TextChanged += new System.EventHandler(this.txtOutputDir_TextChanged);
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
            // txtAdditionalOptions
            //
            this.txtAdditionalOptions.Location = new System.Drawing.Point(112, 113);
            this.txtAdditionalOptions.Name = "txtAdditionalOptions";
            this.txtAdditionalOptions.Size = new System.Drawing.Size(322, 20);
            this.txtAdditionalOptions.TabIndex = 34;
            this.tooltip.SetToolTip(this.txtAdditionalOptions, "Additional command line options to pass to Slice compiler");
            this.txtAdditionalOptions.TextChanged += new System.EventHandler(this.txtAdditionalOptions_TextChanged);
            this.txtAdditionalOptions.Leave += new System.EventHandler(this.AdditionalOptions_Leave);
            //
            // lblAdditionalOptions
            //
            this.lblAdditionalOptions.AutoSize = true;
            this.lblAdditionalOptions.Location = new System.Drawing.Point(11, 116);
            this.lblAdditionalOptions.Name = "lblAdditionalOptions";
            this.lblAdditionalOptions.Size = new System.Drawing.Size(95, 13);
            this.lblAdditionalOptions.TabIndex = 49;
            this.lblAdditionalOptions.Text = "Additional Options:";
            //
            // lblSliceCompilerOptionsSep
            //
            this.lblSliceCompilerOptionsSep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSliceCompilerOptionsSep.Location = new System.Drawing.Point(120, 66);
            this.lblSliceCompilerOptionsSep.Name = "lblSliceCompilerOptionsSep";
            this.lblSliceCompilerOptionsSep.Size = new System.Drawing.Size(412, 1);
            this.lblSliceCompilerOptionsSep.TabIndex = 54;
            //
            // lblOutputSep
            //
            this.lblOutputSep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblOutputSep.Location = new System.Drawing.Point(45, 17);
            this.lblOutputSep.Name = "lblOutputSep";
            this.lblOutputSep.Size = new System.Drawing.Size(487, 1);
            this.lblOutputSep.TabIndex = 55;
            //
            // lblIncludeDirectories
            //
            this.lblIncludeDirectories.AutoSize = true;
            this.lblIncludeDirectories.Location = new System.Drawing.Point(14, 87);
            this.lblIncludeDirectories.Name = "lblIncludeDirectories";
            this.lblIncludeDirectories.Size = new System.Drawing.Size(98, 13);
            this.lblIncludeDirectories.TabIndex = 56;
            this.lblIncludeDirectories.Text = "Include Directories:";
            //
            // txtIncludeDirectories
            //
            this.txtIncludeDirectories.Location = new System.Drawing.Point(112, 84);
            this.txtIncludeDirectories.Name = "txtIncludeDirectories";
            this.txtIncludeDirectories.Size = new System.Drawing.Size(322, 20);
            this.txtIncludeDirectories.TabIndex = 57;
            this.txtIncludeDirectories.TextChanged += new System.EventHandler(this.txtIncludeDirectories_TextChanged);
            //
            // CSharpConfigurationView
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.txtIncludeDirectories);
            this.Controls.Add(this.lblIncludeDirectories);
            this.Controls.Add(this.lblOutputSep);
            this.Controls.Add(this.lblSliceCompilerOptionsSep);
            this.Controls.Add(this.lblOutputDirectory);
            this.Controls.Add(this.txtAdditionalOptions);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.lblAdditionalOptions);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.lblSliceCompilerOptions);
            this.Controls.Add(this.btnOutputDirectoryBrowse);
            this.Name = "CSharpConfigurationView";
            this.Size = new System.Drawing.Size(535, 152);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblOutput;
        private System.Windows.Forms.Label lblSliceCompilerOptions;
        private System.Windows.Forms.Label lblOutputDirectory;
        private System.Windows.Forms.TextBox txtOutputDir;
        private System.Windows.Forms.Button btnOutputDirectoryBrowse;
        private System.Windows.Forms.TextBox txtAdditionalOptions;
        private System.Windows.Forms.Label lblAdditionalOptions;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.Label lblSliceCompilerOptionsSep;
        private System.Windows.Forms.Label lblOutputSep;
        private System.Windows.Forms.Label lblIncludeDirectories;
        private System.Windows.Forms.TextBox txtIncludeDirectories;
    }
}
