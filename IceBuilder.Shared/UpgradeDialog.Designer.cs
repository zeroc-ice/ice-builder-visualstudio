namespace IceBuilder
{
    partial class UpgradeDialog
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
            this.Cancel = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.description = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            //
            // Cancel
            //
            this.Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(537, 253);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.CancelButton_Clicked);
            //
            // OKButton
            //
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(456, 253);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Clicked);
            //
            // description
            //
            this.description.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.description.BackColor = System.Drawing.SystemColors.Control;
            this.description.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.description.Location = new System.Drawing.Point(16, 12);
            this.description.Name = "description";
            this.description.ReadOnly = true;
            this.description.Size = new System.Drawing.Size(594, 232);
            this.description.TabIndex = 6;
            this.description.Text = "";
            this.description.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.description_LinkClicked);
            //
            // UpgradeDialog
            //
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(624, 288);
            this.ControlBox = false;
            this.Controls.Add(this.description);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.Cancel);
            this.Name = "UpgradeDialog";
            this.Text = "Ice Builder Project Upgrade";
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.RichTextBox description;
    }
}
