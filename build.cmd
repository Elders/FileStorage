@echo off

SETLOCAL

SET NUGET=%LocalAppData%\NuGet\NuGet.exe
SET FAKE=%LocalAppData%\FAKE\tools\Fake.exe
SET NYX=%LocalAppData%\Nyx\tools\build.fsx
SET GITVERSION=%LocalAppData%\GitVersion.CommandLine\tools\GitVersion.exe
SET MSBUILD14_TOOLS_PATH="%ProgramFiles(x86)%\MSBuild\14.0\bin\MSBuild.exe"
SET MSBUILD12_TOOLS_PATH="%ProgramFiles(x86)%\MSBuild\12.0\bin\MSBuild.exe"
SET BUILD_TOOLS_PATH=%MSBUILD14_TOOLS_PATH%

IF NOT EXIST %MSBUILD14_TOOLS_PATH% (
  echo In order to run this tool you need either Visual Studio 2015 or
  echo Microsoft Build Tools 2015 tools installed.
  echo.
  echo Visit this page to download either:
  echo.
  echo http://www.visualstudio.com/en-us/downloads/visual-studio-2015-downloads-vs
  echo.
  echo Attempting to fall back to MSBuild 12 for building only
  echo.
  IF NOT EXIST %MSBUILD12_TOOLS_PATH% (
    echo Could not find MSBuild 12.  Please install build tools ^(See above^)
    exit /b 1
  ) else (
    set BUILD_TOOLS_PATH=%MSBUILD12_TOOLS_PATH%
  )
)

echo Downloading latest version of NuGet.exe...
IF NOT EXIST %LocalAppData%\NuGet md %LocalAppData%\NuGet
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "$ProgressPreference = 'SilentlyContinue'; Invoke-WebRequest 'https://www.nuget.org/nuget.exe' -OutFile '%NUGET%'"

echo Downloading latest version of FAKE...
IF NOT EXIST %LocalAppData%\FAKE %NUGET% "install" "FAKE" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-Version" "4.4.4"

echo Downloading latest version of NuGet.Core...
IF NOT EXIST %LocalAppData%\NuGet.Core %NUGET% "install" "NuGet.Core" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-Version" "2.8.6"

echo Downloading latest version of GitVersion.CommandLine...
IF NOT EXIST %LocalAppData%\GitVersion.CommandLine %NUGET% "install" "GitVersion.CommandLine" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-Version" "3.3.0"

echo Downloading latest version of Nyx...
%NUGET% "install" "Nyx" "-OutputDirectory" "%LocalAppData%" "-ExcludeVersion" "-PreRelease"

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st
%FAKE% %NYX% "target=RestoreBowerPackages" -st

IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")
IF NOT [%2]==[] (set RELEASE_TARGETSOURCE="%2")

SET RELEASE_NOTES=RELEASE_NOTES.md

SET SUMMARY_S3="AmazonS3 FileStorage"
SET SUMMARY_AZURE="Azure FileStorage"
SET SUMMARY_FILESYSTEM="FileSystem FileStorage"

SET DESCRIPTION_S3="AmazonS3 FileStorage"
SET DESCRIPTION_AZURE="Azure FileStorage"
SET DESCRIPTION_FILESYSTEM="FileSystem FileStorage"

%FAKE% %NYX% appName=FileStorage.AmazonS3 appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_S3% appDescription=%DESCRIPTION_S3% nugetPackageName=FileStorage.AmazonS3 nugetkey=%RELEASE_NUGETKEY%
%FAKE% %NYX% appName=FileStorage.Azure appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_AZURE% appDescription=%DESCRIPTION_AZURE% nugetPackageName=FileStorage.Azure nugetkey=%RELEASE_NUGETKEY%
%FAKE% %NYX% appName=FileStorage.FileSystem appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_FILESYSTEM% appDescription=%DESCRIPTION_FILESYSTEM% nugetPackageName=FileStorage.FileSystem nugetkey=%RELEASE_NUGETKEY%

IF NOT [%1]==[] (%FAKE% %NYX% "target=Release" -st appReleaseNotes=%RELEASE_NOTES%)