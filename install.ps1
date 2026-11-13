<#
.SYNOPSIS
    Builds iRDCav and installs it for the current Windows user.

.DESCRIPTION
    Publishes a self-contained build (no .NET runtime needed to run it) to
    %LOCALAPPDATA%\Programs\iRDCav, copies the example configurations next to it and
    adds an "iRDCav" Start menu shortcut. Run it again to update an installation.
    Settings in %APPDATA%\iRDCav are never touched.

    Building requires the .NET 8 SDK.

.PARAMETER Uninstall
    Removes the installed app and its shortcut instead of installing.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File install.ps1
#>
[CmdletBinding()]
param(
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'

$InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\iRDCav'
$Shortcut = Join-Path ([Environment]::GetFolderPath('Programs')) 'iRDCav.lnk'
$Project = Join-Path $PSScriptRoot 'IRDCav\IRDCav\IRDCav.csproj'
$PublishDir = Join-Path ([IO.Path]::GetTempPath()) 'iRDCav-publish'

function Assert-Windows {
    if ([Environment]::OSVersion.Platform -ne 'Win32NT') {
        throw 'iRDCav can only be installed on Windows.'
    }
}

function Assert-DotnetSdk {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        throw 'The .NET 8 SDK is required to build iRDCav: https://dotnet.microsoft.com/download/dotnet/8.0'
    }
}

function Build-App {
    Remove-Item $PublishDir -Recurse -Force -ErrorAction SilentlyContinue
    dotnet publish $Project -c Release -r win-x64 --self-contained true -o $PublishDir
    if ($LASTEXITCODE -ne 0) {
        throw "Building iRDCav failed (dotnet publish exit code $LASTEXITCODE)."
    }
}

# A running instance locks its files, so it is closed before files are replaced or removed.
function Remove-App {
    Stop-Process -Name 'IRDCav' -ErrorAction SilentlyContinue
    Remove-Item $InstallDir -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $Shortcut -Force -ErrorAction SilentlyContinue
}

function Copy-App {
    New-Item $InstallDir -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $PublishDir '*') $InstallDir -Recurse
    Copy-Item (Join-Path $PSScriptRoot 'config-examples') $InstallDir -Recurse
}

function New-StartMenuShortcut {
    $shell = New-Object -ComObject WScript.Shell
    $link = $shell.CreateShortcut($Shortcut)
    $link.TargetPath = Join-Path $InstallDir 'IRDCav.exe'
    $link.WorkingDirectory = $InstallDir
    $link.Description = 'iRacing Data Companion overlays'
    $link.Save()
}

Assert-Windows
if ($Uninstall) {
    Remove-App
    Write-Host 'iRDCav was removed. Your settings in %APPDATA%\iRDCav were kept.'
    return
}

Assert-DotnetSdk
Build-App
Remove-App
Copy-App
New-StartMenuShortcut
Remove-Item $PublishDir -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "iRDCav was installed to $InstallDir. Start it from the Start menu entry 'iRDCav'."
