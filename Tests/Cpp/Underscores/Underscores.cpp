// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include <Test.h>

#include "Underscores.h"

bool Test::underscores()
{
	Test::PersonPtr p = new Test::Person();
	p->p_name = "Foo";
	p->p_email = "foo@bar.org";
	return true;
}