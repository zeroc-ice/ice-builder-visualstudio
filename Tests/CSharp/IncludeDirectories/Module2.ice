// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include <Module1.ice>
#include <Ice/BuiltinSequences.ice>

module Module2
{

class Derived extends ::Module1::Base
{
	Ice::StringSeq names;
};

};