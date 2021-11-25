// Copyright (c) ZeroC, Inc. All rights reserved.

using System;

public class PackageInitializationException : ApplicationException
{
    public PackageInitializationException()
    {
    }

    public PackageInitializationException(string reason) : base(reason)
    {
    }
}
