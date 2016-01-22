@echo off

set MSBUILD=c:\Program Files (x86)\MSBuild\14.0\bin
set CONFUSER=%CD%\..\..\Tools\ConfuserEx

set PATH=%PATH%;%MSBUILD%;%CONFUSER%

if not exist "%MSBUILD%\msbuild.exe" (
	echo MSBuild Not Found, Aborting
	goto end
)

echo ---------- Cleaning Solution
del bin\Release\* /q /s
echo ---------- Building Solution
msbuild.exe CM3D2.Toolkit\CM3D2.Toolkit.csproj /p:Platform=AnyCPU /p:Configuration=Release
msbuild.exe CM3D2.Arc.Unpacker\CM3D2.Arc.Unpacker.csproj /p:Platform=AnyCPU /p:Configuration=Release
msbuild.exe CM3D2.Arc.Packer\CM3D2.Arc.Packer.csproj /p:Platform=AnyCPU /p:Configuration=Release
msbuild.exe CM3D2.Toolkit.Docs\CM3D2.Toolkit.Docs.shfbproj /p:Configuration=Release
echo ---------- Deleting Unwanted Data
del bin\Release\FSharp.Core.dll
del bin\Release\FSharp.Core.xml
del bin\Release\CommandLine.xml
echo ---------- Obfuscating Solution
rem copy bin\Release\CM3D2.Toolkit.dll bin\Release\CM3D2.Toolkit.Raw.dll
Confuser.CLI .crproj
:end
pause