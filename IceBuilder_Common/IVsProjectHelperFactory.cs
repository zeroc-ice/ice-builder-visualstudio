// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using Microsoft.VisualStudio.Shell.Interop;

namespace IceBuilder
{
    public interface IVsProjectHelperFactory
    {
        VCUtil VCUtil
        {
            get;
        }

        NuGet NuGet
        {
            get;
        }

        IVsProjectHelper ProjectHelper
        {
            get;
        }
    }

    public class ProjectFactoryHelperInstance
    {
        public static void Init(VCUtil vcutil,
                                NuGet nuget,
                                IVsProjectHelper projectHelper)
        {
            VCUtil = vcutil;
            NuGet = nuget;
            ProjectHelper = projectHelper;
        }

        public static VCUtil VCUtil
        {
            get;
            private set;
        }

        public static NuGet NuGet
        {
            get;
            private set;
        }

        public static IVsProjectHelper ProjectHelper
        {
            get;
            private set;
        }
    }
}
