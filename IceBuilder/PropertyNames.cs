using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IceBuilder
{
    class PropertyNames
    {
        //
        // Common properties
        //
        public static readonly String OutputDir = "IceBuilderOutputDir";
        public static readonly String AllowIcePrefix = "IceBuilderAllowIcePrefix";
        public static readonly String Checksum = "IceBuilderChecksum";
        public static readonly String Stream = "IceBuilderStream";
        public static readonly String Underscore = "IceBuilderUnderscore";
        public static readonly String IncludeDirectories = "IceBuilderIncludeDirectories";
        public static readonly String AdditionalOptions = "IceBuilderAdditionalOptions";

        //
        // C++ properties
        //
        public static readonly String HeaderOutputDir = "IceBuilderHeaderOutputDir";
        public static readonly String HeaderExt = "IceBuilderHeaderExt";
        public static readonly String SourceExt = "IceBuilderSourceExt";
        public static readonly String DLLExport = "IceBuilderDLLExport";
        public static readonly String BaseDirectoryForGeneratedInclude = "IceBuilderBaseDirectoryForGeneratedInclude";

        //
        // C# properties
        //
        public static readonly String Tie = "IceBuilderTie";
    }
}
