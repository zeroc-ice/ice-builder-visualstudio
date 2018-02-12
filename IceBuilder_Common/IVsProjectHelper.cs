// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using Microsoft.VisualStudio.Shell.Interop;
using MSProject = Microsoft.Build.Evaluation.Project;
using System.Collections.Generic;

namespace IceBuilder
{
    public interface IVsProjectHelper
    {
        void UpdateProject(IVsProject project, Action<MSProject> action);

        T WithProject<T>(IVsProject project, Func<MSProject, T> func);

        IDisposable OnProjectUpdate(IVsProject project, Action onProjectUpdate);

        string GetItemMetadata(IVsProject project, string identity, string name, string defaultValue = "");

        string GetDefaultItemMetadata(IVsProject project, string name, bool evaluated, string defaultValue = "");

        void SetItemMetadata(IVsProject project, string itemType, string label, string name, string value);

        void SetItemMetadata(IVsProject project, string name, string value);

        void SetGeneratedItemCustomMetadata(IVsProject project, string slice, string generated,
                                            List<string> excludedConfigurations = null);

        void AddFromFile(IVsProject project, string file);

        void RemoveGeneratedItemDuplicates(IVsProject project);
    }
}
