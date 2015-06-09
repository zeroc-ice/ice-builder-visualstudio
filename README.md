# Ice Builder for Visual Studio

The Ice Builder for Visual Studio manages the compilation of Slice (`.ice`) files to C++ and C#. It compiles your Slice files with `slice2cpp` and `slice2cs`, and allows you to specify the parameters provided to these compilers.

The Ice Builder is a Visual Studio extension compatible with Visual Studio 2012, 2013, and 2015. An Ice installation with `slice2cpp` and `slice2cs` version 3.6.0 or higher is also required.

## Contents
- [Installation](#installation)
- [Overview](#overview)
- [Migration from the Ice Add-in for Visual Studio](#migration-from-the-ice-add-in-for-visual-studio)
- [Ice Home Configuration](#ice-home-configuration)
- [C++ Usage](#c-usage)
  - [Adding Slice Files to a C++ Project](#adding-slice-files-to-a-c-project)
  - [Ice Builder Configuration for a C++ Project](#ice-builder-configuration-for-a-c-project)
- [C# Usage](#c-usage-1)
  - [Adding Slice Files to a C# Project](#adding-slice-files-to-a-c-project-1)
  - [Ice Builder Configuration for a C# Project](#ice-builder-configuration-for-a-c-project-1)
- [MSBuild Usage](#msbuild-usage)
- [Building Ice Builder from Source](#building-ice-builder-from-source)

## Installation

The Ice Builder is available as a Visual Studio extension in the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/).

If you build it from sources, simply double-click on `IceBuilder.vsix` to install the extension into Visual Studio.

## Overview

With the Ice Builder, you can add one or more Slice (`.ice`) files to a Visual Studio project. The Ice Builder will then compile these files by launching `slice2cpp` (for C++ projects) or `slice2cs` (for C# projects). All the Slice files in a given project are compiled through a single Slice compiler invocation.

The Ice Builder compiles and recompiles a Slice file as needed:
- when the generated C++ or C# source files are missing or older than this Slice file
- when the generated C++ or C# source files are older than a Slice file included directly or indirectly by this Slice file

The Ice Builder checks whether Slice files need to be compiled or recompiled each time Visual Studio loads a project, and each time you build a project. Saving a Slice file also triggers the (re)compilation of this Slice file. And when you remove or rename a Slice file, the Ice Builder automatically removes the corresponding generated files.

## Migration from the Ice Add-in for Visual Studio

The Ice Builder for Visual Studio replaces the old Ice add-in for Visual Studio. Each time you start Visual Studio with both the Ice Builder extension and the Ice add-in installed, the Ice Builder will offer to remove the Ice add-in.

Project files created with the Ice add-in are not compatible with the Ice Builder. When you open such a project file, the Ice Builder will offer to convert your project's configuration to the Ice Builder format. You should backup your project files first, as this conversion is irreversible.

## Ice Home Configuration

The Ice Builder relies on a specific Ice installation on your system. In Visual Studio, you can view or edit the home directory of this Ice installation through the `TOOLS` > `Options` > `Project and Solutions` > `Ice Builder` options page.

This installation can correspond to a binary distribution, such as `C:\Program Files (x86)\ZeroC\Ice-3.6.0`, or to a source tree, such as`C:\users\mike\github\zeroc-ice\ice`. When using a source tree, this source tree must be built beforehand with the version of Visual Studio you are currently using.

## C++ Usage

### Adding Slice Files to a C++ Project

Follow these steps:

1. Add the Ice Builder to your C++ project by right-clicking on the project and selecting `Add Ice Builder to Project`. Alternatively, you can select the project and use the menu item `TOOLS` > `Add Ice Builder to Project`.

   Adding the Ice Builder creates a `Slice Files` filter in your project.

2. Add one or more Slice (`.ice`) files to your project. While these Slice files can be anywhere, you may want to select a customary location such as the project's home directory or a sub-directory named `slice`.

3. Review the Ice Builder configuration of your project, as described in the section below.

### Ice Builder Configuration for a C++ Project

The Ice Builder adds an `Ice Builder` property page to the `Common Properties` of your C++ project:

(screenshot)

These properties are the same for all configurations and platforms, and allow you to specify the [parameters](https://doc.zeroc.com/display/Ice36/slice2cpp+Command-Line+Options) passed to `slice2cpp` when compiling the project's Slice files.

| Property                                | MSBuild Property                            | Default Value              | Corresponding `slice2cpp` parameter |
| --------------------------------------- | ------------------------------------------- | -------------------------- | ----------------------------------- |
| Output Directory                        | IceBuilderOutputDir                         | $(ProjectDir)\generated    | `--output-dir`                      |
| Allow Reserved Ice Identifiers          | IceBuilderAllowIcePrefix                    | No                         | `--ice`                             |
| Allow Underscores In Identifiers        | IceBuilderUnderscore                        | No                         | `--underscore`                      |
| Include Directories                     | IceBuilderIncludeDirectories                | $(IceHome)\slice           | `-I`                                |
| Base Directory For Generated #include   | IceBuilderBaseDirectory ForGeneratedInclude |                            | `--include-dir`                     |
| DLL Export Macro                        | IceBuilderDLLExport                         |                            | `--dll-export`                      |
| Generated Header Extension              | IceBuilderHeaderExt                         | .h                         | `--header-ext`                      |
| Generate Helper Functions For Streaming | IceBuilderStream                            | No                         | `--stream`                          |
| Generate Slice Checksums                | IceBuilderChecksum                          | No                         | `--checksum`                        |
| Generated Source Extension              | IceBuilderSourceExt                         | .cpp                       | `--source-ext`                      |
| Additional Options                      | IceBuilderAdditionalOptions                 |                            | (any)                               |

## C# Usage

### Adding Slice Files to a C# Project

Follow these steps:

1. Add the Ice Builder to your C# project by right-clicking on the project and selecting `Add Ice Builder to Project`. Alternatively, you can select the project and use the menu item `TOOLS` > `Add Ice Builder to Project`.

   Adding the Ice Builder creates a `Slice Files` filter in your project.

2. Add one or more Slice (`.ice`) files to your project. While these Slice files can be anywhere, you may want to select a customary location such as the project's home directory or a sub-directory named `slice`.

3. Review the Ice Builder configuration of your project, as described in the section below.

### Ice Builder Configuration for a C# Project

The Ice Builder adds an `Ice Builder` tab to the properties of your C# project:

(screenshot)

These properties are the same for all configurations and platforms, and allow you to specify the [parameters](https://doc.zeroc.com/display/Ice36/slice2cs+Command-Line+Options) passed to `slice2cs` when compiling the project's Slice files.

| Property                                | MSBuild Property             | Default Value              | Corresponding `slice2cs` parameter |
| --------------------------------------- | -----------------------------| -------------------------- | ---------------------------------- |
| Output directory                        | IceBuilderOutputDir          | $(ProjectDir)\generated    | `--output-dir`                     |
| Allow reserved Ice identifiers          | IceBuilderAllowIcePrefix     | (unchecked)                | `--ice`                            |
| Allow underscores in identifiers        | IceBuilderUnderscore         | (unchecked)                | `--underscore`                     |
| Generate helper methods for streaming   | IceBuilderStream             | (unchecked)                | `--stream`                         |
| Generate Slice checksums                | IceBuilderChecksum           | (unchecked)                | `--checksum`                       |
| Generate tie classes                    | IceBuilderTie                | (unchecked)                | `--tie`                            |
| Include directories                     | IceBuilderIncludeDirectories | $(IceHome)\slice           | `-I`                               |
| Additional options                      | IceBuilderAdditionalOptions  |                            | (any)                              |

The Ice Builder automatically adds a reference to the Ice assembly, and allows you to easily add references to more Ice-related assemblies, such as IceGrid or Glacier2. All of these references are added with `Specific Version` set to False.

## MSBuild Usage

The Ice Builder uses [MSBuild](https://msdn.microsoft.com/en-us/library/dd393574.aspx) tasks to build Slice files using `slice2cpp` and `slice2cs`. As a result, you can build Visual Studio projects that enable Ice Builder directly with MSBuild.

The simplest and most common way to configure Ice Builder is in Visual Studio. You can nevertheless also configure Ice Builder directly in your MSBuild project, by importing two Ice Builder projects into your project.

For a C++ project, you need:

1. `$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.Cpp.props` - This project defines the default settings for Ice Builder in C++
2. `$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.Cpp.targets` - This projet defines the targets required to build C++ projects with Ice Builder

The import order matters for MSBuild. `IceBuilder.Cpp.props` depends on common properties defined in `Microsoft.Cpp.props` and must be imported after this project. Likewise, `IceBuilder.Cpp.targets` depends on targets defined in `Microsoft.Cpp.targets` and must be imported after this project.

For a C# project, you need:

1. `$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.CSharp.props` - This project defines the default settings for Ice Builder in C# projects
2. `$(LOCALAPPDATA)\ZeroC\IceBuilder\IceBuilder.CSharp.targets` - This project defines the targets required to build C# projects with Ice Builder

Like for C++, the import order is important. Both `IceBuilder.CSharp.props` and `IceBuilder.CSharp.targets` must be imported after `Microsoft.CSharp.targets`.

The actual configuration of your C++ or C# project uses the MSBuild Properties listed in the sections above.

You add Slice files to your project with the `Include` attribute of the `IceBuilder` element, for example `<IceBuilder Include="Hello.ice"/>`. Note that you also need to add the generated files to your project. In C++, with the default settings, you need to add `<ClCompile Include="generated\Hello.cpp"/>` and `<ClInclude Include="generated\Hello.h"\>`. In C#, with the default settings, you need to add `<ClCompile Include="generated\Hello.cs"/>`.

Ice Builder adds two targets, `IceBuilder_Compile` and `IceBuilder_Clean`, that resp. compile and clean Slice files. For example, you can compile the Slice files in a project `MyProject` within solution `MySolution.sln` with `msbuild MySolution.sln /t:MyProject:IceBuilder_Compile`. Please refer to the [Microsoft MSBuild documentation](https://msdn.microsoft.com/en-us/library/dd393574.aspx) for more details about MSBuild.

## Building Ice Builder from Source

### Build Requirements

To build Ice Builder for Visual Studio you will need to:

Be running one of the following versions of Visual Studio:
- Visual Studio 2012
- Visual Studio 2013
- Visual Studio 2015 RC

**AND**
  
Install ALL of the following Visual Studio SDKs:
- [Visual Studio 2012 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=30668)
- [Visual Studio 2013 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=40758)
- [Visual Studio 2015 RC SDK](https://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs.aspx)


### Build Instructions

To build the Ice Builder for Visual Studio, open the `IceBuilder.sln` solution file in Visual Studio.

After building the Ice Builder extension, the VSIX package will be placed in the build output directory
`IceBuilder\bin\Debug\IceBuilder.vsix` for debug builds, and `IceBuilder\bin\Release\IceBuilder.vsix`
for release builds.

If you want to sign your extension with Authenticode, set the environment variable `SIGN_CERTIFICATE` to 
the path of your PFX certificate store, and the `SIGN_PASSWORD` environment variable to the password
used by your certificate store.
