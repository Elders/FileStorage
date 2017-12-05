@echo off

%FAKE% %NYX% "target=clean" -st
%FAKE% %NYX% "target=RestoreNugetPackages" -st

IF NOT [%1]==[] (set RELEASE_NUGETKEY="%1")
IF NOT [%2]==[] (set RELEASE_TARGETSOURCE="%2")

SET RELEASE_NOTES=RELEASE_NOTES.md

SET SUMMARY="FileStorage Abstractions"
SET SUMMARY_S3="AmazonS3 FileStorage"
SET SUMMARY_AZURE="Azure FileStorage"
SET SUMMARY_FILESYSTEM="FileSystem FileStorage"
SET SUMMARY_WEBAPI="FileStorage Web API integration"

SET DESCRIPTION="FileStorage Abstractions"
SET DESCRIPTION_S3="AmazonS3 FileStorage"
SET DESCRIPTION_AZURE="Azure FileStorage"
SET DESCRIPTION_FILESYSTEM="FileSystem FileStorage"
SET DESCRIPTION_WEBAPI="FileStorage Web API integration"

%FAKE% %NYX% appName=FileStorage appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_WEBAPI% appDescription=%DESCRIPTION_WEBAPI% nugetPackageName=FileStorage.Abstractions nugetkey=%RELEASE_NUGETKEY%
IF errorlevel 1 (echo Faild with exit code %errorlevel% & exit /b %errorlevel%)
%FAKE% %NYX% appName=FileStorage.AmazonS3 appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_S3% appDescription=%DESCRIPTION_S3% nugetPackageName=FileStorage.AmazonS3 nugetkey=%RELEASE_NUGETKEY%
IF errorlevel 1 (echo Faild with exit code %errorlevel% & exit /b %errorlevel%)
%FAKE% %NYX% appName=FileStorage.Azure appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_AZURE% appDescription=%DESCRIPTION_AZURE% nugetPackageName=FileStorage.Azure nugetkey=%RELEASE_NUGETKEY%
IF errorlevel 1 (echo Faild with exit code %errorlevel% & exit /b %errorlevel%)
%FAKE% %NYX% appName=FileStorage.FileSystem appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_FILESYSTEM% appDescription=%DESCRIPTION_FILESYSTEM% nugetPackageName=FileStorage.FileSystem nugetkey=%RELEASE_NUGETKEY%
IF errorlevel 1 (echo Faild with exit code %errorlevel% & exit /b %errorlevel%)
%FAKE% %NYX% appName=FileStorage.WebApi appReleaseNotes=%RELEASE_NOTES% appSummary=%SUMMARY_WEBAPI% appDescription=%DESCRIPTION_WEBAPI% nugetPackageName=FileStorage.WebApi nugetkey=%RELEASE_NUGETKEY%
IF errorlevel 1 (echo Faild with exit code %errorlevel% & exit /b %errorlevel%)