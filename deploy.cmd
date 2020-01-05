@echo off
SETLOCAL
set MSBUILD="%ProgramFiles(x86)%\MSBuild\MSBuild.exe"
::if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe"
::if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\MSBuild\16.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"
if not exist %MSBUILD% set MSBUILD="%ProgramFiles(x86)%\MSBuild\12.0\Bin\MSBuild.exe"
if not exist %MSBUILD% goto :BADEXIT

set _configuration=Release
set _verbosity=m
if not %2_==_ set _configuration=%2
if not %3_==_ set _verbosity=%3

set OutputDir=ReleaseBin

IF _%1==_ GOTO :BADEXIT
IF %1==rcb GOTO :BuildRCB

ECHO Unknown target.
exit /b 1

:BuildRCB
set R=RecuberationDemo\bin\Release
%MSBUILD% RecuberationDemo\RecuberationDemo.csproj -p:Configuration=%_configuration% /verbosity:%_verbosity%
del /Q /S /F %OutputDir%
mkdir %OutputDir%
xcopy %R%\HexGL.dll %OutputDir%
xcopy %R%\Recuberation.dll %OutputDir%
xcopy %R%\RecuberationDemo.exe %OutputDir%
xcopy %R%\RecuberationDemo.exe.config %OutputDir%
exit /b 0

:BADEXIT
ECHO ERROR!
exit /b 1
