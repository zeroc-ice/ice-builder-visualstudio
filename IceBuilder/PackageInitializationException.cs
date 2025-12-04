// Copyright (c) ZeroC, Inc. All rights reserved.

using System;

namespace IceBuilder;

public class PackageInitializationException : ApplicationException
{
    public PackageInitializationException()
    {
    }

    public PackageInitializationException(string reason) : base(reason)
    {
    }
}
