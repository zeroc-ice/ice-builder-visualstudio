using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using ZeroC.IceVisualStudio;

namespace IceCustomProject
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

        public List<String> InitialValues
        {
            get;
            set;
        }

        public List<String> Values
        {
            set
            {
                includeList.Items.Clear();
                foreach(String v in value)
                {
                    includeList.Items.Add(v);
                    if (Path.IsPathRooted(v))
                    {
                        includeList.SetItemCheckState(includeList.Items.Count - 1, CheckState.Checked);
                    }
                }
                InitialValues = value;
            }
            get
            {
                List<String> values = new List<String>();
                foreach (object o in includeList.Items)
                {
                    values.Add(o.ToString());
                }
                return values;
            }
        }

        public Boolean MultipleValues
        {
            get;
            set;
        }

        public Boolean NeedSave
        {
            get
            {
                if(InitialValues == null)
                {
                    return false;
                }
                return !Values.SequenceEqual(InitialValues);
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
            if (_editing)
            {
                index = _editingIndex;
                EndEditing(true);
            }
            if (index > -1 && index < includeList.Items.Count)
            {
                int selected = index;
                includeList.Items.RemoveAt(selected);
                if (includeList.Items.Count > 0)
                {
                    if (selected > 0)
                    {
                        selected -= 1;
                    }
                    includeList.SelectedIndex = selected;
                }
            }
            Cursor = Cursors.Default;
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (_editing)
            {
                EndEditing(true);
            }
            int index = includeList.SelectedIndex;
            if (index > 0)
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
            if (_editing)
            {
                EndEditing(true);
            }
            int index = includeList.SelectedIndex;
            if (index < includeList.Items.Count - 1 && index > -1)
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
                String path = includeList.Items[i].ToString();
                if(!String.IsNullOrEmpty(path))
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
            if (_editingIndex != -1)
            {
                _txtInclude = new TextBox();
                _txtInclude.Text = includeList.Items[includeList.SelectedIndex].ToString();
                _editingIncludeDir = _txtInclude.Text;
                includeList.SelectionMode = SelectionMode.One;

                Rectangle rect = includeList.GetItemRectangle(includeList.SelectedIndex);
                _txtInclude.Location = new Point(includeList.Location.X + 2,
                                                 includeList.Location.Y + rect.Y);
                _txtInclude.Width = includeList.Width - 50;
                _txtInclude.Parent = includeList;
                _txtInclude.KeyDown += new KeyEventHandler(txtInclude_KeyDown);
                _txtInclude.KeyUp += new KeyEventHandler(txtInclude_KeyUp);
                groupBox1.Controls.Add(_txtInclude);

                _btnSelectInclude = new Button();
                _btnSelectInclude.Text = "...";
                _btnSelectInclude.Location = new Point(includeList.Location.X + _txtInclude.Width,
                                                       includeList.Location.Y + rect.Y);
                _btnSelectInclude.Width = 50;
                _btnSelectInclude.Height = _txtInclude.Height;
                _btnSelectInclude.Click += new EventHandler(btnSelectInclude_Clicked);
                groupBox1.Controls.Add(_btnSelectInclude);


                _txtInclude.Show();
                _txtInclude.BringToFront();
                _txtInclude.Focus();

                _btnSelectInclude.Show();
                _btnSelectInclude.BringToFront();
            }
        }

        private void txtInclude_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void txtInclude_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Escape))
            {
                EndEditing(false);
            }
            else if (e.KeyCode.Equals(Keys.Enter))
            {
                EndEditing(true);
            }
        }

        private void btnSelectInclude_Clicked(object sender, EventArgs e)
        {
            String projectDir = PropertyPage.GetProperty("MSBuildProjectDirectory");
            String selectedPath = UIUtil.BrowserFolderDialog(Handle, "Slice Include Directory",
                String.IsNullOrEmpty(_editingIncludeDir) ? projectDir : _editingIncludeDir);

            if(String.IsNullOrEmpty(selectedPath) && !String.IsNullOrEmpty(_editingIncludeDir))
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
            if (_editing)
            {
                _editing = false;
                

                if(_txtInclude == null || _btnSelectInclude == null)
                {
                    return;
                }

                String path = _txtInclude.Text;

                groupBox1.Controls.Remove(_txtInclude);
                _txtInclude = null;

                groupBox1.Controls.Remove(_btnSelectInclude);
                _btnSelectInclude = null;

                if (save)
                {
                    if (_editingIndex != -1)
                    {
                        if(!String.IsNullOrEmpty(path))
                        {
                            includeList.Items[_editingIndex] = path;
                            MultipleValues = false;
                        }
                        else
                        {
                            includeList.Items.RemoveAt(_editingIndex);
                            includeList.SelectedIndex = includeList.Items.Count - 1;
                            _editingIndex = -1;
                        }
                    }
                }
                ResetCheckState();
            }
        }

        private void includeList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_editing)
            {
                String path = includeList.Items[e.Index].ToString();
                if (!String.IsNullOrEmpty(path) && path.IndexOf('$') == -1)
                {
                    String projectDir = PropertyPage.GetProperty("MSBuildProjectDirectory");
                    bool absolute = Path.IsPathRooted(path);

                    if(e.NewValue == CheckState.Unchecked)
                    {
                        if(absolute)
                        {
                            path = FileUtil.RelativePath(projectDir, path);
                            MultipleValues = false;
                        }
                    }
                    else if(e.NewValue == CheckState.Checked)
                    {
                        if(!absolute)
                        {
                            path = Path.GetFullPath(Path.Combine(projectDir, path));
                            MultipleValues = false;
                        }
                    }
                    includeList.Items[e.Index] = path;
                }
            }
        }

        private void includeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            EndEditing(true);
        }

        private String _editingIncludeDir;
        private TextBox _txtInclude;
        private Button _btnSelectInclude;

        private bool _editing;
        private int _editingIndex = -1;
        private List<String> _initialValues;
    }
}
