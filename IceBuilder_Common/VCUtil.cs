// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public interface VCUtil
    {
        bool SetupSliceFilter(EnvDTE.Project project);
        string Evaluate(EnvDTE.Configuration config, string value);

        void AddGenerated(IVsProject project, string path, string filter, string platform, string configuration);
    }
}
