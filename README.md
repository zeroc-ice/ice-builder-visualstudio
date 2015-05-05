# Ice Builder for Visual Studio

The Ice Builder for Visual Studio manages the compilation of Slice (`.ice`) files to C++ and C#. It automatically recompiles Slice files that have changed, keeps track of dependencies between Slice files to recompile dependent files, and removes generated files that have become obsolete.

The Ice Builder for Visual Studio is compatible with Ice version 3.6.0 and later.

(toc)

## Installation

The Ice Builder for Visual Studio is available as a Visual Studio extension in the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/).

If you build it from sources, simply double-click on `IceBuilder.vsix` to install the extension into Visual Studio.

## Migration from the Ice Add-in for Visual Studio

The Ice Builder for Visual Studio replaces the old Ice add-in for Visual Studio. Each time you start Visual Studio with both the Ice Builder extension and the Ice add-in enabled, the Ice Builder will offer to remove the Ice add-in.

Project files created with the Ice add-in are not compatible with the Ice Builder. When you open such a project file, the Ice Builder will offer to convert your project's configuration to the Ice Builder format. You should backup your project files first, as this conversion is irreversible.

## Ice Home Configuration

The Ice Builder relies on a specific Ice installation on your system. In Visual Studio, you can view or edit the home directory of this Ice installation through the `TOOLS` > `Options` > `Project and Solutions` > `Ice Builder` options sheet.

This installation can correspond to a binary distribution, such as `C:\Program Files (x86)\ZeroC\Ice-3.6.0`, or to a source tree, such as`C:\users\mike\github\zeroc-ice\ice`.
  
## C++ Usage

### Adding Slice Files to a C++ Project

Follow these steps:

1. Add the Ice Builder to your C++ project by right-clicking on the project and selecting `Add Ice Builder to Project`. Alternatively, you can select the project and use the menu item `TOOLS` > `Add Ice Builder to Project`.

   Adding the Ice Builder creates a `Slice Files` filter in your project.

2. Add one or more Slice (`.ice`) files to the `Slice Files` filter. While these Slice files can be anywhere, you may want to select a customary location such as the project's home directory or a sub-directory named `slice`. 

3. Review the Ice Builder configuration of your project, as described in the section below. 

### Ice Builder Configuration

The Ice Builder adds an `Ice Builder` sheet to the `Common Properties` of your project:

(screenshot)

These properties are the same for all configurations and platforms, and allow you to specify the parameters passed to `slice2cpp` when compiling the project's Slice files.

(table)


## C# Usage

### Adding Slice Files to a C# Project

### Ice Builder Configuration

