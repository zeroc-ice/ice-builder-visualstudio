// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
{
    public static class DTEProjectExtension
    {
        public static VSLangProj.References GetProjectRererences(this EnvDTE.Project project)
        {
            return ((VSLangProj.VSProject)project.Object).References;
        }
    }
}
