// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include "IncludeDirectories.h"
#include <Module2.h>

bool Test::includeDirectories()
{
	Module2::DerivedPtr d = new Module2::Derived();
	return true;
}