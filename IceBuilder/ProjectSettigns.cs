// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder;

public class ProjectSettigns
{
    public ProjectSettigns(IVsProject project) => _project = project;

    public void Load()
    {
        OutputDir = _project.GetDefaultItemMetadata(ItemMetadataNames.OutputDir, false);
        IncludeDirectories = _project.GetDefaultItemMetadata(ItemMetadataNames.IncludeDirectories, false);
        AdditionalOptions = _project.GetDefaultItemMetadata(ItemMetadataNames.AdditionalOptions, false);
    }

    public void Save()
    {
        _project.SetItemMetadata(ItemMetadataNames.OutputDir, OutputDir);
        _project.SetItemMetadata(ItemMetadataNames.IncludeDirectories, IncludeDirectories);
        _project.SetItemMetadata(ItemMetadataNames.AdditionalOptions, AdditionalOptions);
    }

    public bool IsMSBuildIceBuilderInstalled() => _project.IsMSBuildIceBuilderInstalled();

    public string OutputDir { get; set; }

    public string IncludeDirectories { get; set; }

    public string AdditionalOptions { get; set; }

    public IVsProject _project;
}
