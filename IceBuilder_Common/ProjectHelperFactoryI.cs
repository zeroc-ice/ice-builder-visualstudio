// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
{
    public class ProjectHelperFactoryI : IVsProjectHelperFactory
    {
        public VCUtil VCUtil
        {
            get
            {
                return new VCUtilI();
            }
        }

        public NuGet NuGet
        {
            get
            {
                return new NuGetI();
            }
        }

        public IVsProjectHelper ProjectHelper
        {
            get
            {
                return new ProjectHelper();
            }
        }
    }
}
