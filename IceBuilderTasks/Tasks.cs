// **********************************************************************
//
// Copyright (c) 2009-2015 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#region using
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using System.Text;
using System.Xml;
using System.Xml.XPath;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
#endregion

namespace IceBuilder
{
    #region SliceCompilerTask
    public abstract class SliceCompilerTask : ToolTask
    {
        public SliceCompilerTask()
        {
        }

        [Required]
        public String WorkingDirectory
        {
            get;
            set;
        }

        [Required]
        public String IceHome
        {
            get;
            set;
        }

        [Required]
        public String OutputDir
        {
            get;
            set;
        }

        [Required]
        public ITaskItem[] Sources
        {
            get;
            set;
        }

        public Boolean Ice
        {
            get;
            set;
        }

        public Boolean Underscore
        {
            get;
            set;
        }

        public Boolean Stream
        {
            get;
            set;
        }

        public Boolean Checksum
        {
            get;
            set;
        }

        public String[] IncludeDirectories
        {
            get;
            set;
        }

        public String AdditionalOptions
        {
            get;
            set;
        }

        protected override string GetWorkingDirectory()
        {
            return WorkingDirectory;
        }

        protected override String GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder(false);
            if(!String.IsNullOrEmpty(OutputDir))
            {
                builder.AppendSwitch("--output-dir");
                builder.AppendFileNameIfNotNull(OutputDir);
            }

            if(Ice)
            {
                builder.AppendSwitch("--ice");
            }

            if(Underscore)
            {
                builder.AppendSwitch("--underscore");
            }

            if(Stream)
            {
                builder.AppendSwitch("--stream");
            }

            if(Checksum)
            {
                builder.AppendSwitch("--checksum");
            }

            if (IncludeDirectories != null)
            {
                foreach (String path in IncludeDirectories)
                {
                    builder.AppendSwitchIfNotNull("-I", path);
                }
            }

            if(!String.IsNullOrEmpty(AdditionalOptions))
            {
                builder.AppendTextUnquoted(AdditionalOptions);
            }

            builder.AppendFileNamesIfNotNull(Sources, " ");

            return builder.ToString();
        }

        protected override string GenerateFullPathToTool()
        {
            List<String> paths = new List<String>(new String[]
                    {
                        Path.Combine(IceHome, "bin", ToolName),
                        Path.Combine(IceHome, "cpp", "bin", ToolName)
                    });

            String path = paths.FirstOrDefault(p => File.Exists(p));
            if(String.IsNullOrEmpty(path))
            {
                const String message =
                    "Slice compiler `{0}' not found. Review Ice Home setting in Visual Studio 'Tool > Options > Ice'";
                Log.LogError(String.Format(message, ToolName));
            }
            return path;
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (singleLine.IndexOf(String.Format("{0}:", ToolName)) != -1)
            {
                int i = singleLine.IndexOf("error:");
                if (i != -1)
                {
                    Log.LogError("", "", "", "", 0, 0, 0, 0, singleLine.Substring(i + 1));
                }
            }
            else
            {
                String s = singleLine.Trim();
                if(s.StartsWith(WorkingDirectory))
                {
                    s = s.Substring(WorkingDirectory.Length);
                }

                bool warning = false;
                String file = "";
                int line = 0;
                String description = "";

                //
                // Skip the drive letter
                //
                int i = s.IndexOf(":");
                if(i <= 1 && s.Length > i + 1)
                {
                    i = s.IndexOf(":", i + 1);
                }

                if(i != -1)
                {
                    file = Path.GetFullPath(s.Substring(0, i).Trim().Trim('"'));
                    if(file.IndexOf(WorkingDirectory) != -1)
                    {
                        file = file.Substring(WorkingDirectory.Length)
                                   .Trim(Path.DirectorySeparatorChar);
                    }

                    if(s.Length > i + 1)
                    {
                        s = s.Substring(i + 1);

                        i = s.IndexOf(":");
                        if (i != -1)
                        {
                            if(Int32.TryParse(s.Substring(0, i), out line))
                            {
                                if(s.Length > i + 1)
                                {
                                    s = s.Substring(i + 1);
                                }
                            }
                            else
                            {
                                s = s.Substring(i);
                            }
                        }

                        i = s.IndexOf("warning:");
                        if(i != -1)
                        {
                            warning = true;
                            if(s.Length > i + 1)
                            {
                                s = s.Substring(i + 1);
                            }
                        }
                        description = s.Trim();
                    }
                }

                if(warning)
                {
                    Log.LogWarning("", "", "", file, line, 0, 0, 0, description);
                }
                else
                {
                    Log.LogError("", "", "", file, line, 0, 0, 0, description);
                }
            }
        }

        protected List<String> TranslateDependPaths(List<String> paths)
        {
            List<String> newPaths = new List<string>();
            foreach(String path in paths)
            {
                String newPath = path;
                if(!Path.IsPathRooted(newPath))
                {
                    foreach (String includePath in IncludeDirectories)
                    {
                        newPath = Path.IsPathRooted(includePath) ?
                            Path.Combine(includePath, newPath) :
                            Path.Combine(WorkingDirectory, includePath, newPath);
                        if(File.Exists(newPath))
                        {
                            break;
                        }
                    }
                }
                newPaths.Add(Path.GetFullPath(newPath));
            }
            return newPaths;
        }
    }
    #endregion

    #region SliceDependTask
    public abstract class SliceDependTask : SliceCompilerTask
    {
        public String DependFile
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] Outputs
        {
            get;
            private set;
        }

        protected override String GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            builder.AppendSwitch("--depend-xml");
            builder.AppendSwitch("--depend-file");
            builder.AppendFileNameIfNotNull(DependFile);
            builder.AppendTextUnquoted(String.Format(" {0}", base.GenerateCommandLineCommands()));
            return builder.ToString();
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            int retval = base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
            if (retval == 0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(DependFile);
                Outputs = new TaskItem[Sources.Length];
                for (int i = 0; i < Sources.Length; ++i)
                {
                    ITaskItem item = Sources[i];
                    ITaskItem newItem = new TaskItem(item.ItemSpec);
                    item.CopyMetadataTo(newItem);

                    List<String> dependPaths = new List<string>();
                    XmlNodeList depends = doc.DocumentElement.SelectNodes(
                            String.Format("/dependencies/source[@name='{0}']/dependsOn", item.GetMetadata("Identity")));
                    if (depends != null)
                    {
                        foreach (XmlNode depend in depends)
                        {
                            dependPaths.Add(depend.Attributes["name"].Value);
                        }
                    }
                    newItem.SetMetadata("Depends", String.Join(";", TranslateDependPaths(dependPaths)));

                    Outputs[i] = newItem;
                }
            }
            return retval;
        }
    }
    #endregion

    #region Slice2CppDependTask
    public class Slice2CppDependTask : SliceDependTask
    {
        protected override string ToolName
        {
            get
            {
                return "slice2cpp.exe";
            }
        }
    }
    #endregion

    #region Slice2CSharpDependTask
    public class Slice2CSharpDependTask : SliceDependTask
    {
        protected override string ToolName
        {
            get
            {
                return "slice2cs.exe";
            }
        }
    }
    #endregion

    #region SliceGeneratedTask
    public abstract class SliceGeneratedTask : Task
    {
        [Required]
        public ITaskItem[] Sources
        {
            get;
            set;
        }

        [Required]
        public String OutputDir
        {
            get;
            set;
        }

        abstract protected void GeneratedItems();

        public override bool Execute()
        {
            GeneratedItems();
            return true;
        }
    }
    #endregion

    #region Slice2CppGeneratedTask
    public class Slice2CppGeneratedTask : SliceGeneratedTask
    {
        [Output]
        public ITaskItem[] GeneratedSources
        {
            get;
            private set;
        }

        [Output]
        public ITaskItem[] GeneratedHeaders
        {
            get;
            private set;
        }

        protected override void GeneratedItems()
        {
            GeneratedSources = new ITaskItem[Sources.Length];
            GeneratedHeaders = new ITaskItem[Sources.Length];

            for(int i = 0; i < Sources.Length; ++i)
            {
                ITaskItem item = Sources[i];
                GeneratedSources[i] = new TaskItem(
                        Path.Combine(OutputDir, item.GetMetadata("RelativeDir"),
                                     String.Format("{0}.cpp", item.GetMetadata("Filename"))));
                GeneratedHeaders[i] = new TaskItem(
                    Path.Combine(OutputDir, item.GetMetadata("RelativeDir"),
                                 String.Format("{0}.cpp", item.GetMetadata("Filename"))));
            }
        }
    }
    #endregion

    #region Slice2CSharpGeneratedTask
    public class Slice2CSharpGeneratedTask : SliceGeneratedTask
    {
        [Output]
        public ITaskItem[] GeneratedSources
        {
            get;
            private set;
        }

        protected override void GeneratedItems()
        {
            GeneratedSources = new ITaskItem[Sources.Length];

            for (int i = 0; i < Sources.Length; ++i)
            {
                ITaskItem item = Sources[i];
                GeneratedSources[i] = new TaskItem(
                        Path.Combine(OutputDir, item.GetMetadata("RelativeDir"),
                                     String.Format("{0}.cs", item.GetMetadata("Filename"))));
            }
        }
    }
    #endregion

    #region Slice2CppTask
    public class Slice2CppTask : SliceCompilerTask
    {
        protected override string ToolName
        {
            get
            {
                return "slice2cpp.exe";
            }
        }

        public String DLLExport
        {
            get;
            set;
        }

        public String HeaderExt
        {
            get;
            set;
        }

        public String SourceExt
        {
            get;
            set;
        }

        public String IncludeDir
        {
            get;
            set;
        }
    }
    #endregion

    #region Slice2CSharpTask
    public class Slice2CSharpTask : SliceCompilerTask
    {
        protected override String ToolName
        {
            get
            {
                return "slice2cs.exe";
            }
        }

        public Boolean Tie
        {
            get;
            set;
        }

        protected override String GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder();
            if (Tie)
            {
                builder.AppendSwitch("--tie ");
            }
            builder.AppendTextUnquoted(String.Format(" {0}", base.GenerateCommandLineCommands()));
            return builder.ToString();
        }
    }
    #endregion
}
