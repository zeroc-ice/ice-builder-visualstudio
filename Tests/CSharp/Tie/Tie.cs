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
    public class PersonI : PersonOperations_
    {
        public void greet(String name, Ice.Current current)
        {
            Console.Write(String.Format("Hi {0}!", name));
        }
    }

    public class Tie
    {
        public static bool tie()
        {
            PersonTie_ servant = new PersonTie_(new PersonI());
            return true;
        }
    }
}
