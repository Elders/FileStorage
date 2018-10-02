@echo off

@powershell -File .nyx\build.ps1 '--appname=Elders.FileStorage' '--nugetPackageName=FileStorage.Abstractions'
@powershell -File .nyx\build.ps1 '--appname=Elders.FileStorage.Azure' '--nugetPackageName=FileStorage.Azure'
