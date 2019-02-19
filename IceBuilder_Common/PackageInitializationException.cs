// **********************************************************************
//
// Copyright (c) 2009-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

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
