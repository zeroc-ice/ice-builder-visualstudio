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
    public class Services
    {
        public static bool services()
        {
            typeof(Glacier2.Application).Assembly.FullName.EndsWith("Glacier2.dll");
            typeof(Ice.Application).Assembly.FullName.EndsWith("Ice.dll");
            typeof(IceBox.Service).Assembly.FullName.EndsWith("IceBox.dll");
            typeof(IceDiscovery.Lookup).Assembly.FullName.EndsWith("IceDiscovery.dll");
            typeof(IceGrid.AdapterObserver).Assembly.FullName.EndsWith("IceGrid.dll");
            typeof(IceLocatorDiscovery.Lookup).Assembly.FullName.EndsWith("IceLocatorDiscovery.dll");
            typeof(IcePatch2.ByteSeqSeqHelper).Assembly.FullName.EndsWith("IcePatch2.dll");
            typeof(IceSSL.CertificateVerifier).Assembly.FullName.EndsWith("IceSSL.dll");
            return true;
        }
    }
}
