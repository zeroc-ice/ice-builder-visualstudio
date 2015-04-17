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

namespace IceBuilder
{
    public class Output
    {
        private const string buildOutputPaneGuid = "{1BD8A850-02D1-11d1-BEE7-00A0C913D1F8}";
        private EnvDTE.OutputWindowPane OutputWindowPane
        {
            get;
            set;
        }

        public Output(EnvDTE80.DTE2 DTE2)
        {
            //
            // Initialize the output window
            //
            EnvDTE.OutputWindow window = (EnvDTE.OutputWindow)DTE2.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Object;
            foreach (EnvDTE.OutputWindowPane w in window.OutputWindowPanes)
            {
                if (w.Guid.Equals(buildOutputPaneGuid, StringComparison.CurrentCultureIgnoreCase))
                {
                    OutputWindowPane = w;
                    break;
                }
            }
        }

        public void WriteLine(String line)
        {
            OutputWindowPane.Activate();
            OutputWindowPane.OutputString(line);
            OutputWindowPane.OutputString(System.Environment.NewLine);
        }
    }
}
