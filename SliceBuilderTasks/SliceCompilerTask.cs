using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IceBuilder
{
    public class StreamReader
    {
        public void appendData(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
            {
                _data += outLine.Data + "\n";
            }
        }

        public string data()
        {
            return _data;
        }

        private string _data = "";
    }

    //
    // Base task to run Slice compilers.
    //
    public abstract class SliceCompilerTask : Task
    {
        protected abstract String Compiler();

        private String iceHome_;
        [Required]
        public String IceHome
        {
            get
            {
                return iceHome_;
            }
            set
            {
                iceHome_ = value;
            }
        }

        private String outputDir_;
        [Required]
        public String OutputDir
        {
            get
            {
                return outputDir_;
            }
            set
            {
                outputDir_ = value;
            }
        }

        private ITaskItem[] sources_;
        [Required]
        public ITaskItem[] Sources
        {
            get
            {
                return sources_;
            }
            set
            {
                sources_ = value;
            }
        }

        private bool ice_;
        public Boolean Ice
        {
            get
            {
                return ice_;
            }
            set
            {
                ice_ = value;
            }
        }

        private bool underscore_;
        public Boolean Underscore
        {
            get
            {
                return underscore_;
            }
            set
            {
                underscore_ = value;
            }
        }

        private bool stream_;
        public Boolean Stream
        {
            get
            {
                return stream_;
            }
            set
            {
                stream_ = value;
            }
        }

        private bool checksum_;
        public Boolean Checksum
        {
            get
            {
                return checksum_;
            }
            set
            {
                checksum_ = value;
            }
        }

        private String[] sliceIncludePath_;
        public String[] SliceIncludePath
        {
            get
            {
                return sliceIncludePath_;
            }
            set
            {
                sliceIncludePath_ = value;
            }
        }

        private String additionalOptions_ = "";
        private String AdditionalOptions
        {
            get 
            {
                return additionalOptions_;
            }
            set
            {
                additionalOptions_ = value;
            }
        }

        protected virtual List<String> CommandLineArgs()
        {
            List<String> args = new List<String>();
            if(!String.IsNullOrEmpty(OutputDir))
            {
                args.Add("--output-dir");
                args.Add(OutputDir);
            }

            if(Ice)
            {
                args.Add("--ice");
            }

            if(Underscore)
            {
                args.Add("--underscore");
            }

            if(Stream)
            {
                args.Add("--stream");
            }

            if(Checksum)
            {
                args.Add("--checksum");
            }

            if (SliceIncludePath != null)
            {
                foreach (String path in SliceIncludePath)
                {
                    args.Add(String.Format("-I\"{0}\"", path));
                }
            }

            if (!String.IsNullOrEmpty(AdditionalOptions))
            { 
                args.Concat(new List<String>(AdditionalOptions.Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries)));
            }

            return args;
        }

        public override bool Execute()
        {
            String compiler = null;
            if(File.Exists(Path.Combine(IceHome, "bin", Compiler())))
            {
                compiler = Path.Combine(IceHome, Compiler());
            }
            else if (File.Exists(Path.Combine(IceHome, "cpp", "bin", Compiler())))
            {
                compiler = Path.Combine(IceHome, "cpp", "bin", Compiler());
            }
            else
            {
                Log.LogMessage(MessageImportance.High, 
                    String.Format("Unable to locale `{0}' compiler in `{1}', You may need to set Ice Home in 'Tools > Options > Ice'",
                        Compiler(), IceHome));
                return false;
            }

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = compiler;

            List<String> args = CommandLineArgs();
            foreach(ITaskItem item in Sources)
            {
                args.Add(item.ToString());
            }
            process.StartInfo.Arguments = String.Join(" ", args);
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(OutputDir);
            StreamReader reader = new StreamReader();
            process.OutputDataReceived += new DataReceivedEventHandler(reader.appendData);


            try
            {
                process.Start();

                //
                // When StandardError and StandardOutput are redirected, at least one
                // should use asynchronous reads to prevent deadlocks when calling
                // process.WaitForExit; the other can be read synchronously using ReadToEnd.
                //
                // See the Remarks section in the below link:
                //
                // http://msdn.microsoft.com/en-us/library/system.diagnostics.process.standarderror.aspx
                //

                // Start the asynchronous read of the standard output stream.
                process.BeginOutputReadLine();
                // Read Standard error.
                string stderr = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if(process.ExitCode != 0)
                {
                    return false;
                }
                return true;
            }
            catch(InvalidOperationException ex)
            {
                return false;
            }
            catch(System.ComponentModel.Win32Exception ex)
            {
                return false;
            }
            finally
            {
                process.Close();
            }
        }
    }
}
