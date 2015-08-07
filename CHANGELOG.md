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
