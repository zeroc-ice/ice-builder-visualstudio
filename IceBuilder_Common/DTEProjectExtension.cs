// **********************************************************************
//
// Copyright (c) ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
{
    public enum IceBuilderProjectType
    {
        None,
        CppProjectType,
        CsharpProjectType
    }

    public static class DTEProjectExtension
    {
        public static VSLangProj.References GetProjectRererences(this EnvDTE.Project project)
        {
            return ((VSLangProj.VSProject)project.Object).References;
        }
    }
}
