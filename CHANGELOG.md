## Changes in Ice Builder for Visual Studio 4.3.4

- Update the builder to support using Ice 3.7 Nuget
  packages.

- Fix to allow setting Ice Home to point to Ice 3.6 source
  distribution

## Changes in Ice Builder for Visual Studio 4.3.3

- Add support to generate TLog files with C++ builds, this ensure
  that Visual Studio correctly rebuild projects when out of project
  dependencies change.

## Changes in Ice Builder for Visual Studio 4.3.2

- Fixed a bug that cause Slice files not being compiled after a project was
  clean.

## Changes in Ice Builder for Visual Studio 4.3.1

- Improve build dependencies to consider Ice Builder options, generated source
  files are considered outdated if the build options changes since the last build.

- Improve build dependencies to consider the Slice compiler build date, if generated
  source files are older than the Slice compiler the files are considered outdated.

- Change the way how Ice Builder projects are imported when configuring a project
  to use the builder, files are now imported directly from the install location rather
  than requiring the builder to copy them under local app data directory.

- Fixed a bug that can cause build to fail when a solution is configured to use
  "Mixed Platforms".

- Fixed a bug that can cause C++ project changes to be lost when configuring a
  project to use the Ice Builder.

## Changes in Ice Builder for Visual Studio 4.3.0

- Add support to use Ice Builder with the upcoming Ice 3.7.0 Nuget packages.

- Improve the layout of the CSharp configuration dialog.

## Changes in Ice Builder for Visual Studio 4.2.4

- Fixed a bug that could cause a NullReferenceException when removing the
  builder from a project.

- Fixed a bug that could cause a System.OverflowException when building a
  solution where some projects failed to load.

## Changes in Ice Builder for Visual Studio 4.2.3

- Added support for reading CSharp settings from Ice distribution.

## Changes in Ice Builder for Visual Studio 4.2.2

- Fixed an issue that could cause an COMException when accessing the project
  GUID.

- Fixed an issue in saving C# project settings.

- Fixed an issue where setting the "Build Automatically" option introduced in
  version 4.2.0 could result in a InvalidOperationException exception being
  throw.

## Changes in Ice Builder for Visual Studio 4.2.1

- Fixed an issue that could cause a null pointer exception when
  reading the extension settings.

## Changes in Ice Builder for Visual Studio 4.2.0

- Added support for building the Ice 3.7 source distribution.

- Starting with this release, when the Output Directory for a C++ project contains a
  MSBuild property that expands to different values for each configuration, the
  project includes generated files in each configuration and set the Exclude
  From Build property to build these generated C++ files only for the target configuration.

- Added MSbuild tasks to build Ice for PHP and Ice for Python.

- C++ projects have a new property `IceBuilderHeaderOutputDir` that allows to move
  the generated C++ header files into a separate directory.

- Added ability to enable or disable the automatic compilation of Slice files.
  See `Tools > Options > Projects and Solutions > Ice Builder in Visual Studio`.
  Automatic Slice file compilation is disabled by default, which speeds up
  the loading of solutions with many projects.

- Visual Studio 2015 is now required to build the Ice Builder, but the resulting
  extension remain usable with Visual Studio 2012 and Visual Studio 2013.

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
