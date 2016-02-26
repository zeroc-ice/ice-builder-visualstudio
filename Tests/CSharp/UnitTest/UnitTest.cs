// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestStream()
        {
            Assert.IsTrue(Test.Stream.stream(), "Stream test failed");
        }

        [TestMethod]
        public void TestChecksum()
        {
            Assert.AreNotEqual(null, Test.Checksums.checksum(), "Checksum test failed");
        }

        [TestMethod]
        public void TestTie()
        {
            Assert.IsTrue(Test.Tie.tie(), "Tie test failed");
        }

        [TestMethod]
        public void TestIcePrefix()
        {
            Assert.IsTrue(Test.IcePrefix.icePrefix(), "IcePrefix test failed");
        }

        [TestMethod]
        public void TestUnderscores()
        {
            Assert.IsTrue(Test.Underscores.underscores(), "Underscores test failed");
        }

        [TestMethod]
        public void TestIncludeDirectories()
        {
            Assert.IsTrue(Test.IncludeDirectories.includeDirectories(), "Include Directories test failed");
        }

        [TestMethod]
        public void TestOutputDirectory()
        {
            Assert.IsTrue(Test.OutputDirectory.outputDirectory(), "Output Directory test failed");
        }

        [TestMethod]
        public void TestAdditionalOptions()
        {
            Assert.IsTrue(Test.AdditionalOptions.additionalOptions(), "Additional Options test failed");
        }

        [TestMethod]
        public void TestServices()
        {
            Assert.IsTrue(Test.Services.services(), "Services test failed");
        }
    }
}
