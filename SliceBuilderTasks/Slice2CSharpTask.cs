using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace IceBuilder
{
    public class Slice2CSharpTask : SliceCompilerTask
    {
        protected override string Compiler()
        {
            return "slice2cs.exe";
        }

        private bool tie_ = false;
        public Boolean Tie
        {
            get
            {
                return tie_;
            }
            set
            {
                tie_ = value;
            }
        }

        protected override List<String> CommandLineArgs()
        {
            try
            {
                List<String> args = base.CommandLineArgs();
                if (Tie)
                {
                    args.Add("--tie");
                }
                return args;
            }
            catch (System.Exception ex)
            {
                Log.LogMessage(MessageImportance.High, ex.ToString());
                throw;
            }
        }
    }
}
