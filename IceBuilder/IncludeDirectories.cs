
// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace IceBuilder
{
    public partial class IncludeDirectories : UserControl
    {
        public PropertyPage PropertyPage
        {
            get;
            set;
        }

        public IncludeDirectories()
        {
            InitializeComponent();
        }

        public List<string> InitialValues
        {
            get;
            set;
        }

        public List<string> Values
        {
            set
            {
                includeList.Items.Clear();
                foreach(string v in value)
                {
                    includeList.Items.Add(v);
                    if(Path.IsPathRooted(v))
                    {
                        includeList.SetItemCheckState(includeList.Items.Count - 1, CheckState.Checked);
                    }
                }
                InitialValues = value;
            }
            get
            {
                List<string> values = new List<string>();
                foreach(object o in includeList.Items)
                {
                    values.Add(o.ToString());
                }
                return values;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if(_editing)
            {
                EndEditing(true);
            }
            includeList.Items.Add("");
            includeList.SelectedIndex = includeList.Items.Count - 1;
            _editingIndex = includeList.SelectedIndex;
            BeginEditing();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if(includeList.SelectedIndex != -1)
            {
                _editingIndex = includeList.SelectedIndex;
                BeginEditing();
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            int index = includeList.SelectedIndex;
            if(_editing)
            {
                index = _editingIndex;
                EndEditing(true);
            }
            if(index > -1 && index < includeList.Items.Count)
            {
                int selected = index;
                includeList.Items.RemoveAt(selected);
                if(includeList.Items.Count > 0)
                {
                    if(selected > 0)
                    {
                        selected -= 1;
                    }
                    includeList.SelectedIndex = selected;
                }
                PropertyPage.ConfigurationView.Dirty = true;
            }
            Cursor = Cursors.Default;
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if(_editing)
            {
                EndEditing(true);
            }
            int index = includeList.SelectedIndex;
            if(index > 0)
            {
                string current = includeList.SelectedItem.ToString();
                includeList.Items.RemoveAt(index);
                includeList.Items.Insert(index - 1, current);
                includeList.SelectedIndex = index - 1;
                ResetCheckState();
            }
            Cursor = Cursors.Default;
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if(_editing)
            {
                EndEditing(true);
            }
            int index = includeList.SelectedIndex;
            if(index < includeList.Items.Count - 1 && index > -1)
            {
                string current = includeList.SelectedItem.ToString();
                includeList.Items.RemoveAt(index);
                includeList.Items.Insert(index + 1, current);
                includeList.SelectedIndex = index + 1;
                ResetCheckState();
            }
            Cursor = Cursors.Default;
        }

        private void ResetCheckState()
        {
            for(int i = 0; i < includeList.Items.Count; ++i)
            {
                string path = includeList.Items[i].ToString();
                if(!string.IsNullOrEmpty(path))
                {
                    if(Path.IsPathRooted(path))
                    {
                        includeList.SetItemCheckState(i, CheckState.Checked);
                    }
                    else
                    {
                        includeList.SetItemCheckState(i, CheckState.Unchecked);
                    }
                }
            }
        }

        private void BeginEditing()
        {
            EndEditing(true);
            _editing = true;
            if(_editingIndex != -1)
            {
                _txtInclude = new TextBox();
                _txtInclude.Leave += txtInclude_Leave;
                _txtInclude.Text = includeList.Items[includeList.SelectedIndex].ToString();
                _editingIncludeDir = _txtInclude.Text;
                includeList.SelectionMode = SelectionMode.One;

                Rectangle rect = includeList.GetItemRectangle(includeList.SelectedIndex);
                _txtInclude.Location = new Point(includeList.Location.X + 2,
                                                 includeList.Location.Y + rect.Y);
                _txtInclude.Width = includeList.Width - 50;
                _txtInclude.Parent = includeList;
                _txtInclude.KeyUp += new KeyEventHandler(txtInclude_KeyUp);

                _btnSelectInclude = new Button();
                _btnSelectInclude.Text = "...";
                _btnSelectInclude.Location = new Point(includeList.Location.X + _txtInclude.Width,
                                                       includeList.Location.Y + rect.Y);
                _btnSelectInclude.Width = 50;
                _btnSelectInclude.Height = _txtInclude.Height;
                _btnSelectInclude.Click += new EventHandler(btnSelectInclude_Clicked);


                _txtInclude.Show();
                _txtInclude.BringToFront();
                _txtInclude.Focus();

                _btnSelectInclude.Show();
                _btnSelectInclude.BringToFront();
            }
        }

        void txtInclude_Leave(object sender, EventArgs e)
        {
            EndEditing(true);
        }

        private void txtInclude_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode.Equals(Keys.Escape))
            {
                EndEditing(false);
            }
            else if(e.KeyCode.Equals(Keys.Enter))
            {
                EndEditing(true);
            }
        }

        private void btnSelectInclude_Clicked(object sender, EventArgs e)
        {
            string projectDir = Path.GetFullPath(Path.GetDirectoryName(ProjectUtil.GetProjectFullPath(PropertyPage.Project)));
            string selectedPath = UIUtil.BrowserFolderDialog(Handle, "Slice Include Directory",
                string.IsNullOrEmpty(_editingIncludeDir) ? projectDir : _editingIncludeDir);

            if(string.IsNullOrEmpty(selectedPath) && !string.IsNullOrEmpty(_editingIncludeDir))
            {
                _txtInclude.Text = _editingIncludeDir;
            }
            else
            {
                _txtInclude.Text = FileUtil.RelativePath(projectDir, selectedPath);
            }
            EndEditing(true);
        }

        private void EndEditing(bool save)
        {
            if(_editing)
            {
                _editing = false;


                if(_txtInclude == null || _btnSelectInclude == null)
                {
                    return;
                }

                string path = _txtInclude.Text;
                _txtInclude = null;
                _btnSelectInclude = null;

                if(save)
                {
                    if(_editingIndex != -1)
                    {
                        if(!string.IsNullOrEmpty(path))
                        {
                            includeList.Items[_editingIndex] = path;
                        }
                        else
                        {
                            includeList.Items.RemoveAt(_editingIndex);
                            includeList.SelectedIndex = includeList.Items.Count - 1;
                            _editingIndex = -1;
                        }
                        PropertyPage.ConfigurationView.Dirty = true;
                    }
                }
                ResetCheckState();
            }
        }

        private void includeList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if(!_editing)
            {
                string path = includeList.Items[e.Index].ToString();
                if(!string.IsNullOrEmpty(path) && path.IndexOf('$') == -1)
                {
                    string projectDir = Path.GetFullPath(Path.GetDirectoryName(ProjectUtil.GetProjectFullPath(PropertyPage.Project)));
                    bool absolute = Path.IsPathRooted(path);

                    if(e.NewValue == CheckState.Unchecked)
                    {
                        if(absolute)
                        {
                            includeList.Items[e.Index] = FileUtil.RelativePath(projectDir, path);
                            PropertyPage.ConfigurationView.Dirty = true;
                        }
                    }
                    else if(e.NewValue == CheckState.Checked)
                    {
                        if(!absolute)
                        {
                            includeList.Items[e.Index] = Path.GetFullPath(Path.Combine(projectDir, path));
                            PropertyPage.ConfigurationView.Dirty = true;
                        }
                    }
                }
            }
        }

        private void IncludeDirectories_Leave(object sender, EventArgs e)
        {
            EndEditing(true);
        }

        private string _editingIncludeDir;
        private TextBox _txtInclude;
        private Button _btnSelectInclude;

        private bool _editing;
        private int _editingIndex = -1;
    }
}
