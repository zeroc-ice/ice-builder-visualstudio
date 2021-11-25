// Copyright (c) ZeroC, Inc. All rights reserved.

namespace IceBuilder
{
    public class ProjectHelperFactoryI : IVsProjectHelperFactory
    {
        public IVCUtil VCUtil => new VCUtilI();
        public INuGet NuGet => new NuGetI();
        public IVsProjectHelper ProjectHelper => new ProjectHelper();
    }
}
