// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include <Test.h>

#include "Stream.h"

bool Test::stream()
{
	Ice::CommunicatorPtr communicator = Ice::initialize();
	Ice::OutputStreamPtr output = Ice::createOutputStream(communicator);
    Test::PersonPrx person = Test::PersonPrx::uncheckedCast(communicator->stringToProxy("person:default"));
    output->write(person);
    communicator->destroy();
	return true;
}