// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#include "CppUnitTest.h"

#include <Ice\Ice.h>
#include <AdditionalOptions\AdditionalOptions.h>
#include <Checksums\Checksums.h>
#include <HeaderDir\HeaderDir.h>
#include <IcePrefix\IcePrefix.h>
#include <IncludeDirectories\IncludeDirectories.h>
#include <OutputDirectory\OutputDirectory.h>
#include <Services\Services.h>
#include <SourceHeaderExt\SourceHeaderExt.h>
#include <Stream\Stream.h>
#include <Underscores\Underscores.h>

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace UnitTest
{		
TEST_CLASS(UnitTest1)
{
public:
		
	TEST_METHOD(TestAdditionalOptions)
	{
		Assert::IsTrue(Test::additionalOptions());
	}

	TEST_METHOD(TestChecksums)
	{
		Assert::IsTrue(Test::checksums());
	}

	TEST_METHOD(TestHeaderDir)
	{
		Assert::IsTrue(Test::headerDir());
	}

	TEST_METHOD(TestIcePrefix)
	{
		Assert::IsTrue(Test::icePrefix());
	}

	TEST_METHOD(TestIncludeDirectories)
	{
		Assert::IsTrue(Test::includeDirectories());
	}

	TEST_METHOD(TestOutputDirectory)
	{
		Assert::IsTrue(Test::outputDirectory());
	}

	TEST_METHOD(TestServices)
	{
		Assert::IsTrue(Test::services());
	}

	TEST_METHOD(TestSourceHeaderExt)
	{
		Assert::IsTrue(Test::sourceHeaderExt());
	}

	TEST_METHOD(TestStream)
	{
		Assert::IsTrue(Test::stream());
	}

	TEST_METHOD(TestUnderscores)
	{
		Assert::IsTrue(Test::underscores());
	}
};
}