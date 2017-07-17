# Ice Builder for Visual Studio

The Ice Builder for Visual Studio manages the compilation of Slice (`.ice`) files to C++ and C#. It compiles your Slice files with `slice2cpp` and `slice2cs`, and allows you to specify the parameters provided to these compilers.

The Ice Builder is a Visual Studio extension compatible with Visual Studio 2010, 2012, 2013, 2015 and 2017. An Ice installation with `slice2cpp` and `slice2cs` version 3.6.0 or higher is also required.

## Contents
- [Installation](#installation)
- [Overview](#overview)
- [Ice Home Configuration](#ice-home-configuration)
- [C++ Usage](#c-usage)
  - [Adding Slice Files to a C++ Project](#adding-slice-files-to-a-c-project)
  - [Ice Builder Configuration for a C++ Project](#ice-builder-configuration-for-a-c-project)
  - [Known Issue with Visual Studio 2017](#known-issue-with-visual-studio-2017)
- [C# Usage](#c-usage-1)
  - [Adding Slice Files to a C# Project](#adding-slice-files-to-a-c-project-1)
  - [Ice Builder Configuration for a C# Project](#ice-builder-configuration-for-a-c-project-1)
- [MSBuild Usage](#msbuild-usage)
- [Building Ice Builder from Source](#building-ice-builder-from-source)
- [Building Ice Builder for Visual Studio 2010 from Source](#building-ice-builder-for-visual-studio-2010-from-source)
- [Migration from the Ice Add-in for Visual Studio](#migration-from-the-ice-add-in-for-visual-studio)

## Installation

The Ice Builder is available as two Visual Studio extensions in the
Visual Studio Marketplace: [Ice Builder](https://marketplace.visualstudio.com/items?itemName=ZeroCInc.IceBuilder) for Visual Studio 2012, 2013, 2015 and 2017 and [Ice Builder for Visual Studio 2010](https://marketplace.visualstudio.com/items?itemName=ZeroCInc.IceBuilderforVisualStudio2010).

If you build Ice Builder from sources, simply double-click on `IceBuilder.vsix` or `IceBuilder_VS2010.vsix` to install the extension into Visual Studio.

## Overview

With the Ice Builder, you can add one or more Slice (`.ice`) files to a Visual Studio project. The Ice Builder will then compile these files by launching `slice2cpp` (for C++ projects) or `slice2cs` (for C# projects). All the Slice files in a given project are compiled through a single Slice compiler invocation.

The Ice Builder compiles and recompiles a Slice file as needed:
- when the generated C++ or C# source files are missing or older than this Slice file
- when the generated C++ or C# source files are older than a Slice file included directly or indirectly by this Slice file

The Ice Builder checks whether Slice files need to be compiled or recompiled each time Visual Studio loads a project, and each time you build a project. And when you remove or rename a Slice file, the Ice Builder automatically removes the corresponding generated files.

The Ice Builder for Visual Studio 2010 does not allow you to add `.ice` files to your projects or configure Slice compilation options in the Visual Studio IDE. You need to edit the `.vcxproj` file in a text editor or with the Ice Builder in the IDE of a newer version of Visual Studio.

## Ice Home Configuration

The Ice Builder relies on a specific Ice installation on your system. In Visual Studio, you can view or edit the home directory of this Ice installation through the `Tools` > `Options` > `Project and Solutions` > `Ice Builder` options page.

This installation can correspond to a binary distribution, such as `C:\Program Files\ZeroC\Ice-3.7.0`, or to a source tree, such as `C:\users\mike\github\zeroc-ice\ice`.

![Ice home screenshot](https://github.com/zeroc-ice/ice-builder-visualstudio/raw/master/Screenshots/vs2015-options.png)

:warning: The Ice Home setting is ignored when a project uses an Ice NuGet package. Installing an Ice NuGet package into a project automatically configures the project to use the Ice SDK provided by that NuGet package.

If the automatic build option is selected, Slice files will be compiled each time they are saved, otherwise they will be compiled only during project builds.

### Setting Ice Home with Visual Studio 2010

Since the Ice Builder for Visual Studio 2010 does not support setting Ice Builder properties in the Visual Studio IDE, you have several ways to set Ice Home:
 - set Ice Home with Ice Builder and a more recent version of Visual Studio. Ice Builder and Ice Builder for Visual Studio 2010 share the same Ice Home configuration.
 - set Ice Home in the Windows registry, by editing `IceHome` in `HKEY_CURRENT_USER\SOFTWARE\ZeroC\IceBuilder`.
 - set the ICE_HOME environment variable
 - set the IceHome MSbuild property `/p:IceHome=<Ice Install Dir>`

## C++ Usage

### Adding Slice Files to a C++ Project

Follow these steps:

1. Add the Ice Builder to your C++ project by right-clicking on the project and selecting `Add Ice Builder to Project`. Alternatively, you can select the project and use the menu item `Tools` > `Add Ice Builder to Project`.

   Adding the Ice Builder creates a `Slice Files` filter in your project.

2. Add one or more Slice (`.ice`) files to your project. While these Slice files can be anywhere, you may want to select a customary location such as the project's home directory or a sub-directory named `slice`.

3. Review the Ice Builder configuration of your project, as described in the section below.

### Ice Builder Configuration for a C++ Project

The Ice Builder adds an `Ice Builder` property page to the `Common Properties` of your C++ project:

![Alt text](https://github.com/zeroc-ice/ice-builder-visualstudio/raw/master/Screenshots/cpp-property-page.png)

These properties are the same for all configurations and platforms, and allow you to specify the [parameters](https://doc.zeroc.com/display/Ice36/slice2cpp+Command-Line+Options) passed to `slice2cpp` when compiling the project's Slice files.

| Property                                | MSBuild Property                            | Default Value              | Corresponding `slice2cpp` parameter |
| --------------------------------------- | ------------------------------------------- | -------------------------- | ----------------------------------- |
| Output Directory                        | IceBuilderOutputDir                         | $(ProjectDir)\generated    | `--output-dir`                      |
| Header Output Directory                 | IceBuilderHeaderOutputDir                   | $(IceBuilderOutputDir)     | (none)                              |
| Include Directories                     | IceBuilderIncludeDirectories                | $(IceHome)\slice           | `-I`                                |
| Base Directory For Generated #include   | IceBuilderBaseDirectoryForGeneratedInclude |                            | `--include-dir`                     |
| Generated Header Extension              | IceBuilderHeaderExt                         | .h                         | `--header-ext`                      |
| Generated Source Extension              | IceBuilderSourceExt                         | .cpp                       | `--source-ext`                      |
| Additional Options                      | IceBuilderAdditionalOptions                 |                            | (any)                               |

If you leave `Base Directory For Generated #include` unset, Ice Builder automatically adds `Header Output Directory` (when set) or `Output Directory` to the include directories used during the C++ compilation of your project. With MSBuild, this corresponds to the property `AdditionalIncludeDirectories`.

### Known Issue with Visual Studio 2017

The `Ice Builder` property page does not always appear in C++ projects with Visual Studio 2017: it appears with some installations of Visual Studio 2017, but not all of them. We are currently researching this issue. Universal Windows (UWP) and C# projects are not affected by this problem.

## C# Usage

### Adding Slice Files to a C# Project

Follow these steps:

1. Add the Ice Builder to your C# project by right-clicking on the project and selecting `Add Ice Builder to Project`. Alternatively, you can select the project and use the menu item `Tools` > `Add Ice Builder to Project`.

   Adding the Ice Builder creates a `Slice Files` filter in your project.

2. Add one or more Slice (`.ice`) files to your project. While these Slice files can be anywhere, you may want to select a customary location such as the project's home directory or a sub-directory named `slice`.

3. Review the Ice Builder configuration of your project, as described in the section below.

### Ice Builder Configuration for a C# Project

The Ice Builder adds an `Ice Builder` tab to the properties of your C# project:

![Alt text](https://github.com/zeroc-ice/ice-builder-visualstudio/raw/master/Screenshots/csharp-property-page.png)

These properties are the same for all configurations and platforms, and allow you to specify the [parameters](https://doc.zeroc.com/display/Ice36/slice2cs+Command-Line+Options) passed to `slice2cs` when compiling the project's Slice files.

| Property                                | MSBuild Property             | Default Value              | Corresponding `slice2cs` parameter |
| --------------------------------------- | -----------------------------| -------------------------- | ---------------------------------- |
| Output directory                        | IceBuilderOutputDir          | $(ProjectDir)\generated    | `--output-dir`                     |
| Include directories                     | IceBuilderIncludeDirectories | $(IceHome)\slice           | `-I`                               |
| Additional options                      | IceBuilderAdditionalOptions  |                            | (any)                              |

The Ice Builder automatically adds a reference to the Ice assembly, and allows you to easily add references to more Ice-related assemblies, such as IceGrid or Glacier2. All of these references are added with `Specific Version` set to False.

## MSBuild Usage

The Ice Builder uses [MSBuild](https://msdn.microsoft.com/en-us/library/dd393574.aspx) tasks to build Slice files using `slice2cpp` and `slice2cs`. As a result, you can build Visual Studio projects that enable Ice Builder directly with MSBuild.

The simplest and most common way to configure Ice Builder is in Visual Studio. You can nevertheless also configure Ice Builder directly in your MSBuild project, by importing two Ice Builder projects from the Ice Builder install into your project.

The Ice builder install directory can be read from the Windows registry and assigned to the `IceBuilderInstallDir` MSBuild property using the following code:

    <PropertyGroup>
        <IceBuilderInstallDir>$([MSBuild]::GetRegistryValue('HKEY_CURRENT_USER\SOFTWARE\ZeroC\IceBuilder',
                                                    'InstallDir.$(VisualStudioVersion)'))</IceBuilderInstallDir>
    </PropertyGroup>

Then for a C++ project, you need:

1. `$(IceBuilderInstallDir)\Resources\IceBuilder.Cpp.props` - This project defines the default settings for Ice Builder in C++
2. `$(IceBuilderInstallDir)\Resources\IceBuilder.Cpp.targets` - This project defines the targets required to build C++ projects with Ice Builder

The import order matters for MSBuild. `IceBuilder.Cpp.props` depends on common properties defined in `Microsoft.Cpp.props` and must be imported after this project. Likewise, `IceBuilder.Cpp.targets` depends on targets defined in `Microsoft.Cpp.targets` and must be imported after this project.

And for a C# project, you need:

1. `$(IceBuilderInstallDir)\Resources\IceBuilder.CSharp.props` - This project defines the default settings for Ice Builder in C# projects
2. `$(IceBuilderInstallDir)\Resources\IceBuilder.CSharp.targets` - This project defines the targets required to build C# projects with Ice Builder

Like for C++, the import order is important. Both `IceBuilder.CSharp.props` and `IceBuilder.CSharp.targets` must be imported after `Microsoft.CSharp.targets`.

The actual configuration of your C++ or C# project uses the MSBuild Properties listed in the sections above.

You add Slice files to your project with the `Include` attribute of the `IceBuilder` element, for example `<IceBuilder Include="Hello.ice"/>`. Note that you also need to add the generated files to your project. In C++, with the default settings, you need to add `<ClCompile Include="generated\Hello.cpp"/>` and `<ClInclude Include="generated\Hello.h"\>`. In C#, with the default settings, you need to add `<ClCompile Include="generated\Hello.cs"/>`.

Ice Builder adds two targets, `IceBuilder_Compile` and `IceBuilder_Clean`, that resp. compile and clean Slice files. For example, you can compile the Slice files in a project `MyProject` within solution `MySolution.sln` with `msbuild MySolution.sln /t:MyProject:IceBuilder_Compile`. Please refer to the [Microsoft MSBuild documentation](https://msdn.microsoft.com/en-us/library/dd393574.aspx) for more details about MSBuild.

## Building Ice Builder from Source

### Build Requirements

You need Visual Studio 2017

**AND**

to install ALL of the following Visual Studio SDKs:
- [Visual Studio 2012 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=30668)
- [Visual Studio 2013 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=40758)
- [Visual Studio 2015 SDK](https://msdn.microsoft.com/en-us/library/bb166441.aspx)
- [Visual Studio 2017 SDK](https://docs.microsoft.com/en-us/visualstudio/extensibility/installing-the-visual-studio-sdk)

### Build Instructions

Open the `IceBuilder.sln` solution file in Visual Studio 2017.

After building the Ice Builder extension, the VSIX package will be placed in the build output directory:
`IceBuilder\bin\Debug\IceBuilder.vsix` for debug builds, and `IceBuilder\bin\Release\IceBuilder.vsix`
for release builds.

You can sign your extension with Authenticode by setting the environment variable `SIGN_CERTIFICATE` to
the path of your PFX certificate store, and the `SIGN_PASSWORD` environment variable to the password
used by your certificate store.

## Building Ice Builder for Visual Studio 2010 from Source

### Build Requirements

To build Ice Builder for Visual Studio 2010, you need Visual Studio 2010 SP1 and Visual Studio 2010 SP1 SDKs:
- [Visual Studio 2010 SP1 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=21835)

### Build Instructions

Open the `IceBuilder_2010.sln` solution file in Visual Studio 2010.

After building the Ice Builder extension, the VSIX package will be placed in the build output directory:
`IceBuilder\bin\Debug\IceBuilder_2010.vsix` for debug builds, and `IceBuilder_2010\bin\Release\IceBuilder_2010.vsix`
for release builds.

You can sign your extension with Authenticode by setting the environment variable `SIGN_CERTIFICATE` to
the path of your PFX certificate store, and the `SIGN_PASSWORD` environment variable to the password
used by your certificate store.

## Migration from the Ice Add-in for Visual Studio

The Ice Builder for Visual Studio replaces the old Ice add-in for Visual Studio. Each time you start Visual Studio with both the Ice Builder extension and the Ice add-in installed, the Ice Builder will offer to remove the Ice add-in.

Project files created with the Ice add-in are not compatible with the Ice Builder. When you open such a project file, the Ice Builder will offer to convert your project's configuration to the Ice Builder format. You should backup your project files first, as this conversion is irreversible.

All C++ projects configured to use the Ice add-in import the file `%ALLUSERSPROFILE%\ZeroC\Ice.props`. This file must exist to allow
the project to load and get converted to the new Ice Builder format. You can supply this file by installing a binary distribution
for Ice 3.6 or older, or you can create an `Ice.props` file in `%ALLUSERSPROFILE%\ZeroC` with the following contents:
```
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="4.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
    <!-- Temporary file for projects configured to use the old Ice add-in for Visual Studio -->
</Project>
```
