## Changes in Ice Builder for Visual Studio 6.0.4

- Fixed the installation of the Slice grammar and updated the grammar to the latest version.
- Fixed compatibility issues with Visual Studio 2022 17.10. See #29.
- Fixed registration of the property page used by .NET Framework projects. See #30.

## Changes in Ice Builder for Visual Studio 6.0.3

- Add support for Visual Studio 2022.

## Changes in Ice Builder for Visual Studio 6.0.2

- Fixed a bug that cause TFS projects checkout during build, even if there is
  no project modifications.

- Fixed the handling of Slice file renaming and deletion to ensure that the
  corresponding generated files are also removed.

## Changes in Ice Builder for Visual Studio 6.0.1

- Fixed a bug that can result in a Visual Studio crash when upgrading projects
  from 4.3.10 to 6.0.0

## Changes in Ice Builder for Visual Studio 6.0.0

- Add Support for Visual Studio 2019

- Drop Visual Studio 2012 and Visual Studio 2013 support

## Changes in Ice Builder for Visual Studio 5.0.3

- Add Slice syntax highlighting

- Only checkout projects in TFS when they are being modified

## Changes in Ice Builder for Visual Studio 5.0.2

- Fixes errors during .NET Core project clean.

- Fix deletion of SliceCompile dependency items
  for renamed or removed SliceCompile items.

## Changes in Ice Builder for Visual Studio 5.0.1

- Fixes to support .NET Core project system:

  - Fixed renaming of SliceCompile items in .NET Core project,
    that was causing Visual Studio to Freeze while adding the
    new generated item see dotnet/project-system#3229

  - Non generated items removed by Ice Builder in .NET Core
    projects that use EnableDefaultItems. SliceCompileSource
    metadata used to mark generated items was attach to the
    glob causing all files that originate in the glob to be
    mark as generated and eventually removed.

## Changes in Ice Builder for Visual Studio 5.0.0

- Split Ice Builder in two components:

  - Ice Builder for MSBuild, in the ice-builder-msbuild repository, distributed
    as a NuGet package on nuget.org.
  - Ice Builder for Visual Studio, a Visual Studio extension, distributed in the
    Visual Studio Marketplace.
    Ice Builder for Visual Studio is just a front-end to edit the configuration
    for Ice Builder for MSBuild within the Visual Studio IDE; Ice Builder for
    MSBuild controls the Slice compilation.

- Ice Builder no longer fully supports Ice source installations: if your project
  uses Ice built from source (in a source tree), you have to add the path to
  the Ice C++ include directory to your C++ include directories (etc.) yourself.

- You no longer add Ice Builder to your project. As of this release, you add
  the NuGet package `zeroc.icebuilder.msbuild` to our project.

- Added support for C# .NET Core projects.

- The Ice Builder 4.x MSBuild properties have been replaced by item metadata
  of SliceCompile in Ice Builder for MSBuild. The names have remained mostly the
  same, except without the IceBuilder prefix.

- Added C++ Mapping property to C++ projects. It defaults to C++11 with Visual
  Studio 2015 and greater, and to C++98 with earlier Visual Studio releases.

- When you open a solution with projects that use Ice Builder 4.x, Ice Builder
  offers you to convert these projects to the 5.0 format. This conversion adds
  `zeroc.icebuilder.msbuild` to the converted projects and is not reversible.

- The partial support for Visual Studio 2010 in Ice Builder 4.x is now
  provided by Ice Builder for MSBuild, and as a result Ice Builder for
  Visual Studio no longer supports Visual Studio 2010.

- Ice Builder no longer converts projects that use the old Ice Visual Studio
  Add-in.

## Changes in Ice Builder for Visual Studio 4.3.10

- Fix issue that cause the `Ice Builder` property page to not always
  appear in C++ projects with Visual Studio 2017.

- Fix issue that cause Slice files to not be recompile on save when
  automatic build of Slice files was enabled.

- Fix to allow building projects with ICE_HOME environment
  variable set to an Ice 3.6 source distribution.

## Changes in Ice Builder for Visual Studio 4.3.9

- Update the builder to ensure that projects in VCS are checkout
  before being modified.

- Fixed a bug in PHP and Python target files that still reference
  property that were removed in previous versions causing Ice PHP
  and Python builds failure.

## Changes in Ice Builder for Visual Studio 4.3.8

- Move --tie and --checksum Slice compiler options to additional
  options, projects will be automatically updated if required.

- Do not show popup window for unexpected exceptions. Exceptions are now logged
  to Visual Studio's output window.

## Changes in Ice Builder for Visual Studio 4.3.7

- Added support for Visual Studio 2017
- Added support for Visual Studio 2010 C++ builds
- Moved Slice compilers options that were deprecated in 3.7 to additional
  options and doesn't longer exists as explicit options in the respective
  configuration pages, projects will be automatically updated.

## Changes in Ice Builder for Visual Studio 4.3.6

- Fixed a bug that can cause an OverflowException when
  trasversing IVsHierachy, is a similar issue to the one
  fixed in 4.3.5 but affects a different code path.

## Changes in Ice Builder for Visual Studio 4.3.5

- Fixed a bug that can cause OverflowException when
  traversing IVsHierarchy.

- Improve msbuild project integration.

- Fix compatibility issue with older csharp projects using
  $(MSBuildBinPath)

## Changes in Ice Builder for Visual Studio 4.3.4

- Update the builder to support using Ice 3.7 NuGet
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

- Add support to use Ice Builder with the upcoming Ice 3.7.0 NuGet packages.

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

- Source builds of Ice C++ 3.6 now use NuGet to download and install C/C++ third-party
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
