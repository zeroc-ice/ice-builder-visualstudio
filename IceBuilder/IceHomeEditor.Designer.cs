// Copyright (c) ZeroC, Inc. All rights reserved.

namespace IceBuilder
{
    partial class IceHomeEditor
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
            this.lblInfo = new System.Windows.Forms.Label();
            this.autoBuild = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Location = new System.Drawing.Point(6, 115);
            this.lblInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(346, 45);
            this.lblInfo.TabIndex = 51;
            // 
            // autoBuild
            // 
            this.autoBuild.AutoSize = true;
            this.autoBuild.Location = new System.Drawing.Point(4, 5);
            this.autoBuild.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.autoBuild.Name = "autoBuild";
            this.autoBuild.Size = new System.Drawing.Size(323, 24);
            this.autoBuild.TabIndex = 53;
            this.autoBuild.Text = "Compile Slice files immediately after save";
            this.autoBuild.UseVisualStyleBackColor = true;
            // 
            // IceHomeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.autoBuild);
            this.Controls.Add(this.lblInfo);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "IceHomeEditor";
            this.Size = new System.Drawing.Size(352, 38);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.CheckBox autoBuild;
    }
}
