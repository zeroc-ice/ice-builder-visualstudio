// **********************************************************************
//
// Copyright (c) 2009-2017 ZeroC, Inc. All rights reserved.
//
// **********************************************************************


using System.Collections.Generic;

namespace IceBuilder
{
    public interface VCUtil
    {
        bool SetupSliceFilter(EnvDTE.Project project);
        void AddGeneratedFiles(EnvDTE.Project dteProject, EnvDTE.Configuration config, string filterName,
                               List<string> paths, bool generatedFilesPerConfiguration);
        string Evaluate(EnvDTE.Configuration config, string value);
    }
}
