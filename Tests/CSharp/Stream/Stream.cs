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
    public class Stream
    {
        public static bool stream()
        {
            Ice.Communicator communicator = Ice.Util.initialize();
            Ice.OutputStream output = Ice.Util.createOutputStream(communicator);
            PersonPrx person = PersonPrxHelper.uncheckedCast(communicator.stringToProxy("person:default"));
            PersonPrxHelper.write(output, person);
            communicator.destroy();
            return true;
        }
    }
}
