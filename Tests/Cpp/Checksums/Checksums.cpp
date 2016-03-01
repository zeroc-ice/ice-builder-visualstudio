// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include <Test.h>

#include "Checksums.h"

bool Test::checksums()
{
	Ice::CommunicatorPtr communicator = Ice::initialize();
	Ice::SliceChecksumDict checksums = Ice::sliceChecksums();
    return checksums.find("::Test::Person") != checksums.end();
}