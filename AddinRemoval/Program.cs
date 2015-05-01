// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
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
            if(args.Length == 2)
            {
                //
                // Template files
                //
                String[] templateFiles = new String[]
                    {
                        @"VC#\CSharpProjectItems\Slice\slice.vsdir",
                        @"VC#\CSharpProjectItems\newslice.ice",
                        @"VC#\CSharpProjectItems\newslice.ico",
                        @"VC#\CSharpProjectItems\CSharpProjectItemsslice.vsdir",
                        @"Common7\IDE\Common7\IDE\ItemTemplates\CSharp\1033\Slice.zip",
                        @"Common7\IDE\Common7\IDE\ItemTemplates\CSharp\Code\1033\Slice.zip",
                        @"Common7\IDE\Common7\IDE\ItemTemplates\CSharp\Silverlight\1033\Slice.zip",
                        @"Common7\IDE\Common7\IDE\ItemTemplates\CSharp\Web\1033\Slice.zip",

                        @"VC\vcprojectitems\newslice.ice",
                        @"VC\vcprojectitems\newslice.ico",
                        @"VC\vcprojectitems\slice.vsdir",
                        @"VC\vcprojectitems\Slice\slice.vsdir"
                    };

                String[] templateDirs = new String[] 
                    {
                        @"CSharpProjectItems\Slice",
                        @"VC\vcprojectitems\Slice"
                    };

                String devenvPath = args[0];
                String addinPath = args[1];

                System.Console.Write("Removing Ice Add-in for Visual Studio... ");

                //
                // Remove the .AddIn file
                //
                if (addinPath.EndsWith("Ice-VS2012.AddIn") || addinPath.EndsWith("Ice-VS2013.AddIn"))
                {
                    if (File.Exists(addinPath))
                    {
                        try
                        {
                            File.Delete(addinPath);
                        }
                        catch (IOException)
                        {
                        }
                    }
                }

                //
                // Reset the add-in registration state
                //
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo.FileName = devenvPath;
                process.StartInfo.Arguments = String.Format("/ResetAddin Ice.VisualStudio.Connect /Command File.Exit");
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                process.WaitForExit();

                //
                // Remove the templates
                //
                String rootPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(devenvPath)));

                foreach (String templateFile in templateFiles)
                {
                    String path = Path.Combine(rootPath, templateFile);
                    if (File.Exists(path))
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (IOException)
                        {
                        }
                    }
                }

                foreach (String templateDir in templateDirs)
                {
                    try
                    {
                        Directory.Delete(Path.Combine(rootPath, templateDir));
                    }
                    catch (IOException)
                    {
                    }
                }
                System.Console.WriteLine("ok");

                //
                // Reset the templates
                //
                System.Console.Write("Resetting Visual Studio Templates...");
                process = new System.Diagnostics.Process();
                process.StartInfo.FileName = devenvPath;
                process.StartInfo.Arguments = String.Format("/InstallVSTemplates");
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.Start();
                while (!process.WaitForExit(500))
                {
                    Console.Write(".");
                }
                Console.WriteLine("ok");
            }
        }
    }
}
