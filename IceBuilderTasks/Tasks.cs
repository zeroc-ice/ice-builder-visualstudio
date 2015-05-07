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
    public class TaskUtil
    {
        public static String MakeRelative(String from, String to)
        {
            if (!Path.IsPathRooted(from))
            {
                throw new ArgumentException(String.Format("from: `{0}' must be an absolute path", from));
            }
            else if (!Path.IsPathRooted(to))
            {
                return to;
            }

            string[] firstPathParts = Path.GetFullPath(from).Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
            string[] secondPathParts = Path.GetFullPath(to).Trim(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);

            int sameCounter = 0;
            while (sameCounter < Math.Min(firstPathParts.Length, secondPathParts.Length) &&
                String.Equals(firstPathParts[sameCounter], secondPathParts[sameCounter],
                StringComparison.CurrentCultureIgnoreCase))
            {
                ++sameCounter;
            }

            // Different volumes, relative path not possible.
            if (sameCounter == 0)
            {
                return to;
            }

            // Pop back up to the common point.
            String newPath = "";
            for (int i = sameCounter; i < firstPathParts.Length; ++i)
            {
                newPath += ".." + Path.DirectorySeparatorChar;
            }
            // Descend to the target.
            for (int i = sameCounter; i < secondPathParts.Length; ++i)
            {
                newPath += secondPathParts[i] + Path.DirectorySeparatorChar;
            }
            return newPath.TrimEnd(Path.DirectorySeparatorChar);
        }
    }
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

        [Required]
        public String DependFile
        {
            get;
            set;
        }

        public Boolean Depend
        {
            get;
            set;
        }

        public Boolean AllowIcePrefix
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

        protected override String GetWorkingDirectory()
        {
            return WorkingDirectory;
        }

        protected abstract String GeneratedExtensions
        {
            get;
        }

        protected override String GenerateCommandLineCommands()
        {
            UsageError = false;
            CommandLineBuilder builder = new CommandLineBuilder(false);
            if(Depend)
            {
                builder.AppendSwitch("--depend-xml");
                builder.AppendSwitch("--depend-file");
                builder.AppendFileNameIfNotNull(Path.Combine(OutputDir, DependFile));
            }

            if(!String.IsNullOrEmpty(OutputDir))
            {
                builder.AppendSwitch("--output-dir");
                builder.AppendFileNameIfNotNull(OutputDir);
            }

            if(AllowIcePrefix)
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
                builder.AppendTextUnquoted(" ");
                builder.AppendTextUnquoted(AdditionalOptions);
            }

            builder.AppendFileNamesIfNotNull(Sources, " ");

            return builder.ToString();
        }

        protected override int ExecuteTool(string pathToTool, string responseFileCommands, string commandLineCommands)
        {
            foreach (ITaskItem source in Sources)
            {
                if(!Depend)
                {
                    Log.LogMessage(MessageImportance.High,
                        String.Format(
                            "Compiling {0} -> Generating {1}.{2}",
                            source.GetMetadata("Identity"),
                            TaskUtil.MakeRelative(WorkingDirectory, Path.Combine(OutputDir, source.GetMetadata("Filename"))),
                            GeneratedExtensions));
                }
            }
            return base.ExecuteTool(pathToTool, responseFileCommands, commandLineCommands);
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

        protected override void LogToolCommand(string message)
        {
            Log.LogMessage(MessageImportance.Low, message);
        }

        private bool UsageError
        {
            get;
            set;
        }

        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            if (UsageError)
            {
                return;
            }

            int i = singleLine.IndexOf(String.Format("{0}:", ToolName));
            if(i != -1)
            {
                i += (ToolName.Length + 1);
                Log.LogError("", "", "", "", 0, 0, 0, 0, 
                    String.Format("{0}: {1}", Path.GetFileName(ToolName), singleLine.Substring(i)));
                UsageError = true;
            }
            else
            {
                String s = singleLine.Trim();
                if(s.StartsWith(WorkingDirectory))
                {
                    s = s.Substring(WorkingDirectory.Length);
                }

                String file = "";
                int line = 0;
                String description = "";

                //
                // Skip the drive letter
                //
                i = s.IndexOf(":");
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

                        
                        description = s.Trim();
                        description += Environment.NewLine;
                    }
                }

                if(description.IndexOf("warning:") == 0)
                {
                    //
                    // Don't emit warnings while parsing dependencies otherwise
                    // they will appear twices in the Error List and Output.
                    //
                    if(!Depend)
                    {
                        Log.LogWarning("", "", "", file, line - 1, 0, 0, 0, description.Substring("warning:".Length));
                    }
                }
                else if (description.IndexOf("error:") == 0)
                {
                    Log.LogError("", "", "", file, line - 1, 0, 0, 0, description.Substring("error:".Length));
                }
                else if(!String.IsNullOrEmpty(description))
                {
                    Log.LogError("", "", "", file, line - 1, 0, 0, 0, description);
                }
            }
        }
    }
    #endregion

    #region Slice2CppTask
    public class Slice2CppTask : SliceCompilerTask
    {
        public Slice2CppTask()
        {
            HeaderExt = "h";
            SourceExt = "cpp";
        }

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

        public String BaseDirectoryForGeneratedInclude
        {
            get;
            set;
        }

        protected override String GeneratedExtensions
        {
            get
            {
                return String.Format("{0},{1}", HeaderExt, SourceExt);
            }
        }

        protected override String GenerateCommandLineCommands()
        {
            CommandLineBuilder builder = new CommandLineBuilder(false);

            if (!String.IsNullOrEmpty(DLLExport))
            {
                builder.AppendSwitch("--dll-export");
                builder.AppendFileNameIfNotNull(DLLExport);
            }

            if (!HeaderExt.Equals("h"))
            {
                builder.AppendSwitch("--header-ext");
                builder.AppendFileNameIfNotNull(HeaderExt);
            }

            if (!SourceExt.Equals("cpp"))
            {
                builder.AppendSwitch("--source-ext");
                builder.AppendFileNameIfNotNull(SourceExt);
            }

            if (!String.IsNullOrEmpty(BaseDirectoryForGeneratedInclude))
            {
                builder.AppendSwitch("--include-dir");
                builder.AppendFileNameIfNotNull(BaseDirectoryForGeneratedInclude);
            }
            builder.AppendTextUnquoted(" ");
            builder.AppendTextUnquoted(base.GenerateCommandLineCommands());

            return builder.ToString();
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

        protected override String GeneratedExtensions
        {
            get
            {
                return "cs";
            }
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

    #region SliceDependTask
    public abstract class SliceDependTask : Task
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

        [Required]
        public String DependFile
        {
            get;
            set;
        }

        [Required]
        public String WorkingDirectory
        {
            get;
            set;
        }

        [Output]
        public ITaskItem[] ComputedSources
        {
            get;
            private set;
        }

        [Output]
        public bool UpdateDepends
        {
            get;
            private set;
        }

        protected abstract String ToolName
        {
            get;
        }

        protected String GetGeneratedPath(ITaskItem item, String outputDir, String ext)
        {
            return Path.Combine(
                outputDir,
                Path.GetFileName(Path.ChangeExtension(item.GetMetadata("Identity"), ext)));
        }

        abstract protected ITaskItem[] GeneratedItems(ITaskItem source);

        public override bool Execute()
        {
            List<ITaskItem> computed = new List<ITaskItem>();
            UpdateDepends = false;

            String dependFile = Path.Combine(OutputDir, DependFile);
            
            XmlDocument dependsDoc = new XmlDocument();
            bool dependExists = File.Exists(dependFile);
            if(dependExists)
            {
                try
                {
                    dependsDoc.Load(dependFile);
                }
                catch (XmlException)
                {
                    try
                    {
                        File.Delete(dependFile);
                    }
                    catch(IOException)
                    { 
                    }
                    Log.LogMessage(MessageImportance.Low,
                        String.Format("Build required because depend file: {0} has some invalid data",
                            TaskUtil.MakeRelative(WorkingDirectory, dependFile)));
                }
            }
            else
            {
                Log.LogMessage(MessageImportance.Low,
                    String.Format("Build required because depend file: {0} doesn't exists",
                                  TaskUtil.MakeRelative(WorkingDirectory, dependFile)));
            }

            foreach(ITaskItem source in Sources)
            {
                bool skip = true;
                if(!dependExists)
                {
                    skip = false;
                }
                Log.LogMessage(MessageImportance.Low,
                    String.Format("Computing dependencies for {0}", source.GetMetadata("Identity")));

                ITaskItem[] generatedItems = GeneratedItems(source);

                FileInfo sourceInfo = new FileInfo(source.GetMetadata("FullPath"));
                if(!sourceInfo.Exists)
                {
                    Log.LogMessage(MessageImportance.Low,
                        String.Format("Build required because source: {0} doesn't exists",
                            source.GetMetadata("Identity")));
                    skip = false;
                }

                FileInfo generatedInfo = null;

                if (skip)
                {
                    foreach (ITaskItem item in generatedItems)
                    {
                        generatedInfo = new FileInfo(item.GetMetadata("FullPath"));
                        if (!generatedInfo.Exists)
                        {
                            Log.LogMessage(MessageImportance.Low,
                                String.Format("Build required because generated: {0} doesn't exists",
                                    TaskUtil.MakeRelative(WorkingDirectory, generatedInfo.FullName)));
                            skip = false;
                            break;
                        }
                        else if (sourceInfo.LastWriteTime.ToFileTime() > generatedInfo.LastWriteTime.ToFileTime())
                        {
                            Log.LogMessage(MessageImportance.Low,
                                String.Format("Build required because source: {0} is older than target {1}",
                                    source.GetMetadata("Identity"),
                                    TaskUtil.MakeRelative(WorkingDirectory, generatedInfo.FullName)));
                            skip = false;
                            break;
                        }
                    }
                }

                if (skip)
                {
                    XmlNodeList depends = dependsDoc.DocumentElement.SelectNodes(
                                String.Format("/dependencies/source[@name='{0}']/dependsOn", source.GetMetadata("Identity")));
                    if (depends != null)
                    {
                        List<String> dependPaths = new List<String>();
                        foreach (XmlNode depend in depends)
                        {
                            dependPaths.Add(depend.Attributes["name"].Value);
                        }

                        foreach (String path in dependPaths)
                        {
                            FileInfo dependencyInfo = new FileInfo(path);
                            if (!dependencyInfo.Exists)
                            {
                                skip = false;
                                Log.LogMessage(MessageImportance.Low,
                                String.Format("Build required because dependency: {0} doesn't exists",
                                    TaskUtil.MakeRelative(WorkingDirectory, dependencyInfo.FullName)));
                                break;
                            }
                            else if (dependencyInfo.LastWriteTime > generatedInfo.LastWriteTime)
                            {
                                skip = false;
                                Log.LogMessage(MessageImportance.Low,
                                String.Format("Build required because source: {0} is older than target {1}",
                                    source.GetMetadata("Identity"),
                                    TaskUtil.MakeRelative(WorkingDirectory, dependencyInfo.FullName)));
                                break;
                            }
                        }
                    }
                }

                if (skip)
                {
                    Log.LogMessage(MessageImportance.Normal,
                        String.Format(
                            "Skipping {0} -> {1}.{2} up to date",
                            source.GetMetadata("Identity"),
                            TaskUtil.MakeRelative(WorkingDirectory, Path.Combine(OutputDir, source.GetMetadata("Filename"))),
                            (ToolName.Equals("slice2cpp.exe") ? "[h,cpp] are" : "cs is")));
                }

                ITaskItem computedSource = new TaskItem(source.ItemSpec);
                source.CopyMetadataTo(computedSource);
                computedSource.SetMetadata("BuildRequired", skip ? "False" : "True");
                computed.Add(computedSource);

                if(!UpdateDepends && !skip)
                {
                    UpdateDepends = true;
                }
            }
            ComputedSources = computed.ToArray();
            return true;
        }
    }
    #endregion

    #region Slice2CppDependTask
    public class Slice2CppDependTask : SliceDependTask
    {
        [Required]
        public String SourceExt
        {
            get;
            set;
        }

        [Required]
        public String HeaderExt
        {
            get;
            set;
        }

        protected override string ToolName
        {
            get
            {
                return "slice2cpp.exe";
            }
        }

        protected override ITaskItem[] GeneratedItems(ITaskItem source)
        {
            return new ITaskItem[]
                {
                    new TaskItem(GetGeneratedPath(source, OutputDir, SourceExt)),
                    new TaskItem(GetGeneratedPath(source, OutputDir, HeaderExt)),
                };
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

        protected override ITaskItem[] GeneratedItems(ITaskItem source)
        {
            return new ITaskItem[]
                {
                    new TaskItem(GetGeneratedPath(source, OutputDir, ".cs")),
                };
        }
    }
    #endregion
}
