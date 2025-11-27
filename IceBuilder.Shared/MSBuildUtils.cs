// Copyright (c) ZeroC, Inc. All rights reserved.

using Microsoft.Build.Construction;
using System;
using System.IO;
using System.Linq;

namespace IceBuilder
{
    class MSBuildUtils
    {
        public static readonly string CSharpProjectGUI = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        public static readonly string IceBuilderCppProps = "$(IceBuilderCppProps)";
        public static readonly string IceBuilderCppTargets = "$(IceBuilderCppTargets)";
        public static readonly string IceBuilderCSharpProps = "$(IceBuilderCSharpProps)";
        public static readonly string IceBuilderCSharpTargets = "$(IceBuilderCSharpTargets)";

        public static readonly string IceBuilderCppPropsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.Cpp.props";

        public static readonly string IceBuilderCppTargetsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.Cpp.targets";

        public static readonly string IceBuilderCSharpPropsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.CSharp.props";

        public static readonly string IceBuilderCSharpTargetsPath =
            @"$(IceBuilderInstallDir)\Resources\IceBuilder.CSharp.targets";

        public static readonly string EnsureIceBuilderImportsError =
            @"This project requires the Ice Builder for Visual Studio extension. " +
            @"Use ""Tools &gt; Extensions and Updates"" to install it. " +
            @"For more information, see https://visualstudiogallery.msdn.microsoft.com/1a64e701-63f2-4740-8004-290e6c682ce0.";

        public static bool HasImport(Microsoft.Build.Evaluation.Project project, string path) =>
            project.Xml.Imports.FirstOrDefault(
                p => p.Project.Equals(path, StringComparison.CurrentCultureIgnoreCase)) != null;

        public static bool IsCSharpProject(Microsoft.Build.Evaluation.Project project)
        {
            if (project != null)
            {
                foreach (var p in project.Imports)
                {
                    if (p.ImportedProject.FullPath.EndsWith("Microsoft.CSharp.targets") ||
                       p.ImportedProject.FullPath.EndsWith("Microsoft.Windows.UI.Xaml.CSharp.targets"))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
