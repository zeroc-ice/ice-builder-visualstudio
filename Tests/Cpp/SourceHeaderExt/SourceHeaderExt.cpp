// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include "SourceHeaderExt.h"
#include <Test.hxx>

#include <sys/types.h>
#include <sys/stat.h>

bool Test::sourceHeaderExt()
{
	struct _stat buf;
	return _stat("../../SourceHeaderExt/generated/Test.cxx", &buf) == 0 &&
		   _stat("../../SourceHeaderExt/generated/Test.hxx", &buf) == 0;
}
