
set ICEBUILDERINSTALLDIR=%LOCALAPPDATA%\Microsoft\VisualStudio\14.0Exp\Extensions\ZeroC\Ice Builder\4.3.3

set IceBuilderCppProps=%ICEBUILDERINSTALLDIR%\Resources\IceBuilder.Cpp.props
set IceBuilderCppTargets=%ICEBUILDERINSTALLDIR%\Resources\IceBuilder.Cpp.targets

set IceBuilderCsharpProps=%ICEBUILDERINSTALLDIR%\Resources\IceBuilder.Csharp.props
set IceBuilderCsharpTargets=%ICEBUILDERINSTALLDIR%\Resources\IceBuilder.Csharp.targets

"%VSINSTALLDIR%\Common7\IDE\devenv.exe" /RootSuffix Exp
