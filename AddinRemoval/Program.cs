// **********************************************************************
//
// Copyright (c) 2003-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace AddinRemoval
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args != null && args.Length > 0)
            {
                //
                // Known Add-in locations
                //
                List<String> paths = new List<String>();
                paths.Add(Path.Combine(Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                    "Microsoft\\VisualStudio\\11.0\\Addins\\Ice-VS2012.AddIn"));
                paths.Add(Path.Combine(Environment.GetEnvironmentVariable("ALLUSERSPROFILE"),
                    "Microsoft\\VisualStudio\\12.0\\Addins\\Ice-VS2013.AddIn"));

                foreach(String path in paths)
                {
                    //
                    // Only update the add-in config it if is in one of the
                    // known add-in loations and it matches the program first 
                    // argument.
                    //
                    if(path.Equals(args[0]))
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(path);
                        doc["Extensibility"]["Addin"]["LoadBehavior"].InnerText = "0";
                        doc["Extensibility"]["Addin"]["CommandPreload"].InnerText = "0";
                        doc.Save(path);
                        break;
                    }
                }
            }
        }
    }
}
