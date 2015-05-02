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
            this.txtIceHome = new System.Windows.Forms.TextBox();
            this.btnIceHome = new System.Windows.Forms.Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblIceHome = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtIceHome
            // 
            this.txtIceHome.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtIceHome.Location = new System.Drawing.Point(0, 16);
            this.txtIceHome.Name = "txtIceHome";
            this.txtIceHome.Size = new System.Drawing.Size(283, 20);
            this.txtIceHome.TabIndex = 49;
            // 
            // btnIceHome
            // 
            this.btnIceHome.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnIceHome.Location = new System.Drawing.Point(286, 15);
            this.btnIceHome.Margin = new System.Windows.Forms.Padding(0);
            this.btnIceHome.Name = "btnIceHome";
            this.btnIceHome.Size = new System.Drawing.Size(29, 22);
            this.btnIceHome.TabIndex = 50;
            this.btnIceHome.Text = "...";
            this.btnIceHome.UseVisualStyleBackColor = true;
            this.btnIceHome.Click += new System.EventHandler(this.btnIceHome_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Location = new System.Drawing.Point(0, 41);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(315, 61);
            this.lblInfo.TabIndex = 51;
            // 
            // lblIceHome
            // 
            this.lblIceHome.AutoSize = true;
            this.lblIceHome.Location = new System.Drawing.Point(0, 0);
            this.lblIceHome.Name = "lblIceHome";
            this.lblIceHome.Size = new System.Drawing.Size(97, 13);
            this.lblIceHome.TabIndex = 48;
            this.lblIceHome.Text = "Ice home directory:";
            // 
            // IceHomeEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtIceHome);
            this.Controls.Add(this.lblIceHome);
            this.Controls.Add(this.btnIceHome);
            this.Controls.Add(this.lblInfo);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "IceHomeEditor";
            this.Size = new System.Drawing.Size(319, 102);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtIceHome;
        private System.Windows.Forms.Button btnIceHome;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblIceHome;
    }
}
