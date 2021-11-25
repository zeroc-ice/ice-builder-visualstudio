// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using MSProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    public interface IVsProjectHelper
    {
        void UpdateProject(IVsProject project, Action<MSProject> action, bool switchToMainThread = false);

        T WithProject<T>(IVsProject project, Func<MSProject, T> func, bool switchToMainThread = false);

        IDisposable OnProjectUpdate(IVsProject project, Action onProjectUpdate);

        string GetItemMetadata(IVsProject project, string identity, string name, string defaultValue = "");

        string GetDefaultItemMetadata(IVsProject project, string name, bool evaluated, string defaultValue = "");

        void SetItemMetadata(IVsProject project, string itemType, string label, string name, string value);

        void SetItemMetadata(IVsProject project, string name, string value);

        void SetGeneratedItemCustomMetadata(
            IVsProject project,
            string slice,
            string generated,
            List<string> excludedConfigurations = null);

        void RemoveGeneratedItemCustomMetadata(IVsProject project, List<string> paths);

        void AddFromFile(IVsProject project, string file);

        void RemoveGeneratedItemDuplicates(IVsProject project);
    }
}
