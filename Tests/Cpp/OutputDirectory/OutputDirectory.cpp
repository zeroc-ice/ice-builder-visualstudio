// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include "OutputDirectory.h"
#include <Test.h>

#include <sys/types.h>
#include <sys/stat.h>

bool Test::outputDirectory()
{
	struct _stat buf;
	return _stat("../../OutputDirectory/generated/client/Test.cpp", &buf) == 0 &&
		   _stat("../../OutputDirectory/generated/client/Test.h", &buf) == 0;
}
