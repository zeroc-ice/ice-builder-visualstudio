# Ice Builder for Visual Studio

The Ice Builder for Visual Studio manages the compilation of Slice (`.ice`) files to C++ and C#. It recompiles automatically Slice files that have changed, keeps track of dependencies between Slice files to recompile dependent files, and removes generated files that have become obsolete.

The Ice Builder for Visual Studio is compatible with Ice version 3.6.0 and later.

(toc)

## Installation

The Ice Builder for Visual Studio is available as a Visual Studio extension in the [Visual Studio Gallery](https://visualstudiogallery.msdn.microsoft.com/).

If you build it from sources, simply double-click on `IceBuilder.vsix` to install the extension into Visual Studio.

## Migration from the Ice Add-in for Visual Studio

The Ice Builder for Visual Studio replaces the old Ice add-in for Visual Studio. Each time you start Visual Studio with both the Ice Builder extension and the Ice add-in enabled, the Ice Builder will offer you to remove the Ice add-in.

Project files created with the Ice add-in are not compatible with the Ice Builder. When you open such a project file, the Ice Builder will offer you to convert your project's configuration to the Ice Builder format. You should backup your project files first, as this convertion is irreversible.

## Ice Home Configuration

In Visual Studio, use `Tools` > `Options` > `Project and Solutions` > `Ice Builder` to view or edit the home directory of the Ice installation or Ice source tree selected by the Ice Builder. For example:
  Ice home directory = `C:\Program Files (x86)\ZeroC\Ice-3.6.0` (binary distribution)
  Ice home directory = `C:\users\mike\github\zeroc-ice\ice` (source tree)
  
## C++ Usage

### 

### Configuring C++ Project Settings

The Ice Builder adds an `Ice Builder` sheet to the `Common Properties` of your project, for example:

(screenshot)

These properties are the same for all configurations and platforms, and allow you to specify the parameters passed by `slice2cpp` when compiling the project's Slice files.

(table)


## C# Usage

### Adding Slice Files to a C# Project

### Configuring C# Project Settings

