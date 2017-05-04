// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
{
    partial class IncludeDirectories
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
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.includeList = new System.Windows.Forms.CheckedListBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblDescription = new System.Windows.Forms.Label();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            //
            // btnAdd
            //
            this.btnAdd.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAdd.Location = new System.Drawing.Point(436, 3);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 52;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            //
            // btnEdit
            //
            this.btnEdit.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnEdit.Location = new System.Drawing.Point(436, 26);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(75, 23);
            this.btnEdit.TabIndex = 53;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            //
            // includeList
            //
            this.includeList.FormattingEnabled = true;
            this.includeList.HorizontalScrollbar = true;
            this.includeList.Location = new System.Drawing.Point(107, 3);
            this.includeList.Name = "includeList";
            this.includeList.Size = new System.Drawing.Size(323, 154);
            this.includeList.TabIndex = 51;
            this.toolTip1.SetToolTip(this.includeList, "Specifies one or more directories to add to the Slice include path.");
            this.includeList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.includeList_ItemCheck);
            this.includeList.Leave += new System.EventHandler(this.IncludeDirectories_Leave);
            //
            // btnRemove
            //
            this.btnRemove.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRemove.Location = new System.Drawing.Point(436, 49);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 54;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            //
            // lblDescription
            //
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(3, 3);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(98, 13);
            this.lblDescription.TabIndex = 57;
            this.lblDescription.Text = "Include Directories:";
            //
            // btnUp
            //
            this.btnUp.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnUp.Location = new System.Drawing.Point(436, 72);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(75, 23);
            this.btnUp.TabIndex = 55;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            //
            // btnDown
            //
            this.btnDown.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDown.Location = new System.Drawing.Point(436, 95);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(75, 23);
            this.btnDown.TabIndex = 56;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            //
            // IncludeDirectories
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.includeList);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.lblDescription);
            this.Controls.Add(this.btnRemove);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "IncludeDirectories";
            this.Size = new System.Drawing.Size(514, 160);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnEdit;
        private System.Windows.Forms.CheckedListBox includeList;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
