// Copyright (c) ZeroC, Inc. All rights reserved.

namespace IceBuilder
{
    public interface IVsProjectHelperFactory
    {
        IVCUtil VCUtil { get; }
        INuGet NuGet { get; }
        IVsProjectHelper ProjectHelper { get; }
    }

    public class ProjectFactoryHelperInstance
    {
        public static void Init(IVCUtil vcutil, INuGet nuget, IVsProjectHelper projectHelper)
        {
            VCUtil = vcutil;
            NuGet = nuget;
            ProjectHelper = projectHelper;
        }

        public static IVCUtil VCUtil { get; private set; }

        public static INuGet NuGet { get; private set; }

        public static IVsProjectHelper ProjectHelper { get; private set; }
    }
}
