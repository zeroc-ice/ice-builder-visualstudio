// Copyright (c) ZeroC, Inc. All rights reserved.

namespace IceBuilder
{
    public delegate void NuGetBatchEnd();

    public interface INuGet
    {
        void OnNugetBatchEnd(NuGetBatchEnd batchEnd);
    }
}
