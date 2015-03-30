// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;

namespace ZeroC.IceVisualStudio
{
    /// <summary>
    /// Interaction logic for IceHomeEditor.xaml
    /// </summary>
    public partial class IceHomeEditor : System.Windows.Controls.UserControl
    {
        public IceHomeEditor()
        {
            InitializeComponent();
        }

        internal IceOptionsPage optionsPage;

        public void Initialize()
        {
            txtIceHome.Text = optionsPage.IceHome;
        }

        private void txtIceHome_LostFocus(object sender, RoutedEventArgs e)
        {
            validateIceHome();
        }

        private void updateIceHome()
        {
            optionsPage.IceHome = txtIceHome.Text;
            optionsPage.SaveSettingsToStorage();
        }

        private bool validateIceHome()
        {
            if (txtIceHome.Text == optionsPage.IceHome)
            {
                lblInfo.Text = "";
                return true;
            }
            else
            {
                try
                {
                    if (String.IsNullOrEmpty(txtIceHome.Text) ||
                        System.IO.File.Exists(System.IO.Path.Combine(txtIceHome.Text, "bin", "slice2cpp.exe")) ||
                        System.IO.File.Exists(System.IO.Path.Combine(txtIceHome.Text, "cpp", "bin", "slice2cpp.exe")))
                    {
                        lblInfo.Text = "Ice SDK Location successfully updated";
                        updateIceHome();
                        return true;
                    }
                }
                catch (ArgumentException)
                {
                }

                lblInfo.Text = "Invalid Ice SDK Location";
                return false;
            }
        }

        private void btnSelectIceHome_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = optionsPage.IceHome;
            dialog.Description = "Select Ice SDK Location";
            DialogResult r = dialog.ShowDialog();
            if (r == DialogResult.OK)
            {
                txtIceHome.Text = dialog.SelectedPath;
                validateIceHome();
            }
        }

        private void txtIceHome_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = false;
            validateIceHome();
        }
    }
}
