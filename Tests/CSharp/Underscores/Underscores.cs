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
    public class Underscores
    {
        public static bool underscores()
        {
            Person p = new Person();
            p.p_name = "Foo";
            p.p_email = "foo@bar.org";
            return true;
        }
    }
}
