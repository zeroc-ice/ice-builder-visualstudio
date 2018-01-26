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
        void AddGeneratedFiles(IVsProject roject, List<GeneratedFileSet> filesets);
        string Evaluate(EnvDTE.Configuration config, string value);
    }
}
