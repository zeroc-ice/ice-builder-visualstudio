# Ice Builder for Visual Studio

Ice Builder for Visual Studio is a Visual Studio extension that configures [Ice Builder for MSBuild](https://github.com/zeroc-ice/ice-builder-msbuild) for your C++ and C# projects, all within the Visual Studio IDE. It serves as a front-end for Ice Builder for MSBuild: all the build-time processing is performed by Ice Builder for MSBuild.

Ice Builder for Visual Studio is compatible with Visual Studio 2012, 2013, 2015 and 2017, and works best with the following Ice installations:
 * Ice NuGet package for Ice 3.7 or greater
 * Ice Web installation or MSI installation for Ice 3.6

## Contents

* [Installation](#installation)
* [Overview](#overview)
* [Ice Builder Options](#ice-builder-options)
  * [Compile on Save Configuration](#compile-on-save-configuration)
  * [Ice Home Configuration (Ice 3.6)](#ice-home-configuration-ice-36)
* [C++ Usage](#c-usage)
  * [Adding Slice Files to a C++ Project](#adding-slice-files-to-a-c-project)
  * [Customizing the Slice to C++ Compilation](#customizing-the-slice-to-c-compilation)
* [C# Usage](#c-usage-1)
  * [Adding Slice Files to a C# Project](#adding-slice-files-to-a-c-project-1)
  * [Customizing the Slice to C# Compilation](#customizing-the-slice-to-c-compilation-1)
* [Migration from the Ice Add-in](#migration-from-the-ice-add-in)
* [Building Ice Builder from Source](#building-ice-builder-from-source)
  * [Build Requirements](#build-requirements)
  * [Build Instructions](#build-instructions)

## Installation

The latest version of Ice Builder is published in the [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=ZeroCInc.IceBuilder).

You can also install older versions or preview releases of Ice Builder by downloading the desired `IceBuilder.vsix` from the [GitHub Releases page](https://github.com/zeroc-ice/ice-builder-visualstudio/releases), and then double-clicking on `IceBuilder.vsix`.

## Overview

Ice Builder for MSBuild provides support for compiling Slice source files (`.ice` files) in MSBuild projects, including projects created by Visual Studio. It compiles these Slice files using the Slice to C++ compiler (`slice2cpp`) or the Slice to C# compiler (`slice2cs`) provided by your Ice installation.

You tell Ice Builder for MSBuild which Slice files to compile by adding these files to your project, as described in the sections below. Ice Builder checks whether Slice files need to be compiled or recompiled each time Visual Studio loads a project, and each time you build a project. And if you remove or rename a Slice file with the Visual Studio IDE, Ice Builder for Visual Studio automatically removes the corresponding generated files.

## Ice Builder Options

You can configure the Ice Builder global options on the `Tools` > `Options` > `Project and Solutions` > `Ice Builder` page.

![Ice home screenshot](/Screenshots/options.png)

### Compile on Save Configuration

If the `Compile Slice files immediately after save` box is checked, Ice Builder compiles a Slice file when you save it, otherwise it compiles Slice files only during project builds.

### Ice Home Configuration (Ice 3.6)

With Ice 3.6, you need to specify the Ice installation used by Ice Builder. This Ice Home setting is ignored by projects that install Ice as a NuGet package.

## C++ Usage

### Adding Slice Files to a C++ Project

Follow these steps:

1. Add the Ice Builder NuGet package (`zeroc.icebuilder.msbuild`) to your C++ project.

   Adding Ice Builder creates a `Slice Files` filter in your project.

2. Add one or more Slice (`.ice`) files to your project.

3. Add the directory where Ice Builder outputs generated C++ header files (`$(IntDir)` by default) to your project's C/C++ Additional Include Directories:

![Missing cpp additional include directories](/Screenshots/cpp-additional-include-directories.png)

:memo: Make sure to select `All Configurations` and `All Platforms` as shown above.

### Customizing the Slice to C++ Compilation

Ice Builder allows you to change the options given to `slice2cpp` when compiling a Slice file. You can specify the options that apply to all the Slice files in a project with the `Ice Builder` property page in the `Configuration Properties` of your project:

![Missing cpp property page](/Screenshots/cpp-property-page.png)

You can also specify options that apply to a single Slice file with the `Ice Builder` property page of that file (this is less common):

![Missing file cpp property page](/Screenshots/file-cpp-property-page.png)

These options are always the same for all configurations and platforms, and map to item metadata of the SliceCompile type:

| Property                              | Corresponding SliceCompile Item Metadata |
| ------------------------------------- | ---------------------------------------- |
| Output Directory                      | OutputDir                                |
| Header Output Directory               | HeaderOutputDir                          |
| Include Directories                   | IncludeDirectories                       |
| Base Directory For Generated #include | BaseDirectoryForGeneratedInclude         |
| Generated Header Extension            | HeaderExt                                |
| Generated Source Extension            | SourceExt                                |
| Additional Options                    | AdditionalOptions                        |

See [Customizing the Slice to C++ Compilation](https://github.com/zeroc-ice/ice-builder-msbuild/blob/master/README.md#customizing-the-slice-to-c-compilation) with Ice Builder for MSBuild for further details.

## C# Usage

### Adding Slice Files to a C# Project

Follow these steps:

1. Add the Ice Builder NuGet package (`zeroc.icebuilder.msbuild`) to your C# project.

   Adding Ice Builder creates a `Slice Files` filter in your project.

2. Reload your project if it targets .NET Framework.

3. Add one or more Slice (`.ice`) files to your project.

### Customizing the Slice to C# Compilation

Ice Builder allows you to change the options given to `slice2cs` when compiling a Slice file. You can specify the options that apply to all the Slice files in a project with the `Ice Builder` tab of your project's properties:

![Missing csharp property page](/Screenshots/csharp-property-page.png)

These options are the same for all configurations and platforms, and map to item metadata of the SliceCompile type:

| Property            | Corresponding SliceCompile Item Metadata |
| ------------------- | -----------------------------------------|
| Output Directory    | OutputDir                                |
| Include Directories | IncludeDirectories                       |
| Additional Options  | AdditionalOptions                        |

See [Customizing the Slice to C# Compilation](https://github.com/zeroc-ice/ice-builder-msbuild/blob/master/README.md#customizing-the-slice-to-c-compilation-1) with Ice Builder for MSBuild for further details.

## Migration from the Ice Add-in

Ice Builder no longer supports direct migration from the old Ice add-in for Visual Studio to Ice Builder. The migration from the Ice add-in to Ice Builder is now a two-step process:
 * Install [Ice Builder 4.3.10](https://github.com/zeroc-ice/ice-builder-visualstudio/releases/tag/v4.3.10) to migrate your projects to the Ice Builder 4.3.10 format
 * Install the latest Ice Builder to convert your projects (that are now using Ice Builder 4.3.10) to the latest Ice Builder format

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
