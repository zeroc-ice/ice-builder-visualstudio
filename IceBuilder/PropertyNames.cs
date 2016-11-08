// **********************************************************************
//
// Copyright (c) 2009-2016 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

namespace IceBuilder
{
    class PropertyNames
    {
        //
        // Common properties
        //
        public static readonly string OutputDir = "IceBuilderOutputDir";
        public static readonly string AllowIcePrefix = "IceBuilderAllowIcePrefix";
        public static readonly string Checksum = "IceBuilderChecksum";
        public static readonly string Stream = "IceBuilderStream";
        public static readonly string Underscore = "IceBuilderUnderscore";
        public static readonly string IncludeDirectories = "IceBuilderIncludeDirectories";
        public static readonly string AdditionalOptions = "IceBuilderAdditionalOptions";

        //
        // C++ properties
        //
        public static readonly string HeaderOutputDir = "IceBuilderHeaderOutputDir";
        public static readonly string HeaderExt = "IceBuilderHeaderExt";
        public static readonly string SourceExt = "IceBuilderSourceExt";
        public static readonly string DLLExport = "IceBuilderDLLExport";
        public static readonly string BaseDirectoryForGeneratedInclude = "IceBuilderBaseDirectoryForGeneratedInclude";

        //
        // C# properties
        //
        public static readonly string Tie = "IceBuilderTie";
    }
}
