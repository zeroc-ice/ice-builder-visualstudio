## Changes in Ice Builder for Visual Studio 4.2.0

- Ice Builder now allows building Ice 3.7 distribution

- Stating with this version when the Output Directory for C++ projects contains an
  MSBuild property that expand to different values for each configuration, the
  project will include a generated file for each configuration and setup Exclude
  From Build property, to just build the file for the matching configuration.

- MSbuild tasks for building Ice for PHP and Ice for Python are now included with
  Ice Builder.

- C++ projects have a new property `IceBuilderHeaderOutputDir` that allow to set
  a separate directory for generated headers.

- Whenever Slice files are automatically compiled can now be configured in the
  builder options. The default is to not compile Slice files automatically this
  make project loading faster.

  See Tools > Options > Projects and Solutions > Ice Builder" in Visual Studio

- Visual Studio 2015 is now required to build Ice Builder, but the extension can
  still be used with Visual Studio 2012 and Visual Studio 2013.

## Changes in Ice Builder for Visual Studio 4.1.2

- Rebuild the builder with Visual Studio 2012 update 5, this fixes
  an issue with adding Slice files to C++ projects that affects,
  Visual Studio 2012 update 4 builds.

## Changes in Ice Builder for Visual Studio 4.1.1

- Refactor WinRT project property sheet properties.

- Fixed an issue that could cause a null pointer exception when
  disabling the builder for a WinRT based project.

- Fixed an issue that could cause a null pointer exception when
  opening a project without a solution.

- Fixed an issue were project properties were not correctly evaluated after
  the builder was enabled because MSBuild was using a cached project.

## Changes in Ice Builder for Visual Studio 4.1.0

- Source builds of Ice C++ 3.6 now use Nuget to download and install C/C++ third-party
  dependencies such as bzip2, expat and mcpp. When you set your Ice home directory (in
  Ice Builder) to point to such a source distribution, the Ice Builder automatically adds
  the directory of these third-party libraries to the Debugging/Environment PATH used
  by Visual Studio. This allows you to run Ice applications in Visual Studio without
  reconfiguring your PATH.

- Starting with Ice Builder 4.1, projects with Ice Builder enabled conditionally include
  property sheets in your Ice distribution. Previously, such Ice Builder projects included
  only property sheets from the Ice Builder distribution.
  Thanks to this improvement, the Ice Builder no longer needs to embed details about each
  new Ice version.

- The Add-In removal tool was improved. This tool (included in the Ice Builder) uninstalls
  the deprecated Ice Visual Studio Add-In.

## Changes in Ice Builder for Visual Studio 4.0.1

- Fixes and improvements for Ice home setting
