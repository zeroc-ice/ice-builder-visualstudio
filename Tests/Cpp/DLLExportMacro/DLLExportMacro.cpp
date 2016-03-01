// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include <Test.h>

#include "DLLExportMacro.h"

bool Test::dllExportMacro()
{
#ifdef TEST_PERSON
	return true;
#else
	return false;
#endif
}