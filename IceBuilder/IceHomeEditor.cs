// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell;
using System;
using System.IO;
using System.Windows.Forms;

namespace IceBuilder
{
    public partial class IceHomeEditor : UserControl
    {
        public IceHomeEditor() => InitializeComponent();

        internal IceOptionsPage optionsPage;

        public bool AutoBuilding
        {
            set => autoBuild.Checked = value;
            get => autoBuild.Checked;
        }
    }
}
