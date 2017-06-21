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
            this.lblOutput = new System.Windows.Forms.Label();
            this.lblSliceCompilerOptions = new System.Windows.Forms.Label();
            this.lblOutputDirectory = new System.Windows.Forms.Label();
            this.txtOutputDir = new System.Windows.Forms.TextBox();
            this.btnOutputDirectoryBrowse = new System.Windows.Forms.Button();
            this.referencedAssemblies = new System.Windows.Forms.CheckedListBox();
            this.lblReferencedAssembliesSep = new System.Windows.Forms.Label();
            this.txtAdditionalOptions = new System.Windows.Forms.TextBox();
            this.lblReferencedAssemblies = new System.Windows.Forms.Label();
            this.lblAdditionalOptions = new System.Windows.Forms.Label();
            this.tooltip = new System.Windows.Forms.ToolTip(this.components);
            this.lblSliceCompilerOptionsSep = new System.Windows.Forms.Label();
            this.lblOutputSep = new System.Windows.Forms.Label();
            this.includeDirectories = new IceBuilder.IncludeDirectories();
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
            this.referencedAssemblies.Location = new System.Drawing.Point(112, 370);
            this.referencedAssemblies.Name = "referencedAssemblies";
            this.referencedAssemblies.Size = new System.Drawing.Size(322, 139);
            this.referencedAssemblies.TabIndex = 53;
            this.referencedAssemblies.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ReferencedAssemblies_ItemChecked);
            //
            // lblReferencedAssembliesSep
            //
            this.lblReferencedAssembliesSep.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblReferencedAssembliesSep.Location = new System.Drawing.Point(128, 362);
            this.lblReferencedAssembliesSep.Name = "lblReferencedAssembliesSep";
            this.lblReferencedAssembliesSep.Size = new System.Drawing.Size(402, 1);
            this.lblReferencedAssembliesSep.TabIndex = 52;
            //
            // txtAdditionalOptions
            //
            this.txtAdditionalOptions.Location = new System.Drawing.Point(112, 250);
            this.txtAdditionalOptions.Multiline = true;
            this.txtAdditionalOptions.Name = "txtAdditionalOptions";
            this.txtAdditionalOptions.Size = new System.Drawing.Size(322, 90);
            this.txtAdditionalOptions.TabIndex = 34;
            this.tooltip.SetToolTip(this.txtAdditionalOptions, "Additional command line options to pass to Slice compiler");
            this.txtAdditionalOptions.Leave += new System.EventHandler(this.AdditionalOptions_Leave);
            //
            // lblReferencedAssemblies
            //
            this.lblReferencedAssemblies.AutoSize = true;
            this.lblReferencedAssemblies.Location = new System.Drawing.Point(8, 354);
            this.lblReferencedAssemblies.Name = "lblReferencedAssemblies";
            this.lblReferencedAssemblies.Size = new System.Drawing.Size(121, 13);
            this.lblReferencedAssemblies.TabIndex = 51;
            this.lblReferencedAssemblies.Text = "Referenced Assemblies:";
            //
            // lblAdditionalOptions
            //
            this.lblAdditionalOptions.AutoSize = true;
            this.lblAdditionalOptions.Location = new System.Drawing.Point(11, 253);
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
            // includeDirectories
            //
            this.includeDirectories.AutoSize = true;
            this.includeDirectories.InitialValues = ((System.Collections.Generic.List<string>)(resources.GetObject("includeDirectories.InitialValues")));
            this.includeDirectories.Location = new System.Drawing.Point(6, 79);
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
            this.Controls.Add(this.lblOutputSep);
            this.Controls.Add(this.lblSliceCompilerOptionsSep);
            this.Controls.Add(this.referencedAssemblies);
            this.Controls.Add(this.lblReferencedAssembliesSep);
            this.Controls.Add(this.lblOutputDirectory);
            this.Controls.Add(this.lblReferencedAssemblies);
            this.Controls.Add(this.txtAdditionalOptions);
            this.Controls.Add(this.txtOutputDir);
            this.Controls.Add(this.lblAdditionalOptions);
            this.Controls.Add(this.lblOutput);
            this.Controls.Add(this.lblSliceCompilerOptions);
            this.Controls.Add(this.includeDirectories);
            this.Controls.Add(this.btnOutputDirectoryBrowse);
            this.Name = "CSharpConfigurationView";
            this.Size = new System.Drawing.Size(535, 519);
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
        private IncludeDirectories includeDirectories;
        private System.Windows.Forms.ToolTip tooltip;
        private System.Windows.Forms.CheckedListBox referencedAssemblies;
        private System.Windows.Forms.Label lblReferencedAssembliesSep;
        private System.Windows.Forms.Label lblReferencedAssemblies;
        private System.Windows.Forms.Label lblSliceCompilerOptionsSep;
        private System.Windows.Forms.Label lblOutputSep;
    }
}
