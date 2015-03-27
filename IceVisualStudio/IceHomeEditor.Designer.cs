// **********************************************************************
//
// Copyright (c) 2003-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace ZeroC.IceVisualStudio
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.txtIceHome = new System.Windows.Forms.TextBox();
            this.btnSelectIceHome = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Ice Home:";
            // 
            // txtIceHome
            // 
            this.txtIceHome.AcceptsReturn = true;
            this.txtIceHome.Location = new System.Drawing.Point(6, 20);
            this.txtIceHome.Name = "txtIceHome";
            this.txtIceHome.Size = new System.Drawing.Size(324, 20);
            this.txtIceHome.TabIndex = 1;
            this.txtIceHome.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtIceHome_KeyPress);
            this.txtIceHome.LostFocus += new System.EventHandler(this.txtIceHome_LostFocus);
            // 
            // btnSelectIceHome
            // 
            this.btnSelectIceHome.Location = new System.Drawing.Point(337, 20);
            this.btnSelectIceHome.Name = "btnSelectIceHome";
            this.btnSelectIceHome.Size = new System.Drawing.Size(53, 20);
            this.btnSelectIceHome.TabIndex = 2;
            this.btnSelectIceHome.Text = "...";
            this.btnSelectIceHome.UseVisualStyleBackColor = true;
            this.btnSelectIceHome.Click += new System.EventHandler(this.btnSelectIceHome_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Red;
            this.lblInfo.Location = new System.Drawing.Point(3, 56);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(0, 13);
            this.lblInfo.TabIndex = 4;
            this.lblInfo.Visible = false;
            // 
            // IceHomeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnSelectIceHome);
            this.Controls.Add(this.txtIceHome);
            this.Controls.Add(this.label1);
            this.Name = "IceHomeEditor";
            this.Size = new System.Drawing.Size(396, 82);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIceHome;
        private System.Windows.Forms.Button btnSelectIceHome;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
