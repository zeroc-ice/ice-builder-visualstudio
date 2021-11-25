// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public interface IVCUtil
    {
        bool SetupSliceFilter(EnvDTE.Project project);
        string Evaluate(EnvDTE.Configuration config, string value);

        void AddGenerated(IVsProject project, string path, string filter, string platform, string configuration);
    }
}
