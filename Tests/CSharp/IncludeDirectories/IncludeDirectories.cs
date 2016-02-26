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

namespace Test
{
    public class IncludeDirectories
    {
        public static bool includeDirectories()
        {
            Module2.Derived d = new Module2.Derived();
            return true;
        }
    }
}
