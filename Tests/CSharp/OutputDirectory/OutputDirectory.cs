// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Test
{
    public class OutputDirectory
    {
        public static bool outputDirectory()
        {
            String baseDir = Path.GetFullPath(".");
            while (!baseDir.EndsWith("CSharp"))
            {
                baseDir = Path.GetDirectoryName(baseDir);
            }
            return File.Exists(Path.Combine(baseDir, "OutputDirectory", "generated", "client", "Test.cs"));
        }
    }
}
