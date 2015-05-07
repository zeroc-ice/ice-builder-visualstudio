// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class Checksums
    {
        public static String checksum()
        {
            return Ice.SliceChecksums.checksums.FirstOrDefault(i => i.Key.Equals("::Test::Person")).Value;
        }
    }
}
