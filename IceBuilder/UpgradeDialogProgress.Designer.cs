namespace IceBuilder
{
    partial class UpgradeDialogProgress
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
            this.ProgressBar = new System.Windows.Forms.ProgressBar();
            this.Cancel = new System.Windows.Forms.Button();
            this.InfoLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // ProgressBar
            //
            this.ProgressBar.Location = new System.Drawing.Point(12, 40);
            this.ProgressBar.Name = "ProgressBar";
            this.ProgressBar.Size = new System.Drawing.Size(433, 23);
            this.ProgressBar.TabIndex = 0;
            //
            // Cancel
            //
            this.Cancel.Location = new System.Drawing.Point(191, 71);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.CancelButton_Clicked);
            //
            // InfoLabel
            //
            this.InfoLabel.Location = new System.Drawing.Point(12, 9);
            this.InfoLabel.Name = "InfoLabel";
            this.InfoLabel.Size = new System.Drawing.Size(433, 23);
            this.InfoLabel.TabIndex = 2;
            //
            // UpgradeDialogProgress
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 100);
            this.Controls.Add(this.InfoLabel);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.ProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "UpgradeDialogProgress";
            this.Text = "Ice Builder Project Upgrade Progress";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ProgressBar ProgressBar;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Label InfoLabel;
    }
}
