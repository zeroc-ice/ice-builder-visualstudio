// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

module Test
{

#ifdef FOO
class Base
{

};
#endif

#ifdef BAR
class ;
#endif
};