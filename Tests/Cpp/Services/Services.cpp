// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Ice\Ice.h>
#include <Glacier2\Glacier2.h>
#include <IceStorm\IceStorm.h>
#include <IceGrid\IceGrid.h>

#include "Services.h"

bool Test::services()
{
	IceStorm::TopicPrx::uncheckedCast(IceStorm::TopicPrx());
    Glacier2::RouterPrx::uncheckedCast(Glacier2::RouterPrx());
    IceGrid::AdminPrx::uncheckedCast(IceGrid::AdminPrx());
	return true;
}