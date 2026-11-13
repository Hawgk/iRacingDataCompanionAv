<#
.SYNOPSIS
    Sets up iRDCav on Windows: downloads, builds and installs it for the current user.

.DESCRIPTION
    This single file is all that is needed. It
      1. uses the source next to it, or downloads it from GitHub when run on its own,
      2. uses an installed .NET 8 SDK, or installs one privately for iRDCav (no admin rights,
         nothing added to the system PATH),
      3. builds a self-contained iRDCav, so running it needs no .NET runtime,
      4. installs it to %LOCALAPPDATA%\Programs\iRDCav with the example configurations and
         adds an "iRDCav" Start menu shortcut.
    Run it again to update. Settings in %APPDATA%\iRDCav are never touched.

.PARAMETER Branch
    The branch to download when the script is run on its own. Defaults to main.

.PARAMETER Uninstall
    Removes the app, its shortcut and the private .NET SDK instead of installing.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File install.ps1
#>
[CmdletBinding()]
param(
    [string]$Branch = 'main',
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$Repository = 'https://github.com/Hawgk/iRacingDataCompanionAv'
$ProjectPath = 'IRDCav\IRDCav\IRDCav.csproj'
$InstallDir = Join-Path $env:LOCALAPPDATA 'Programs\iRDCav'
$ToolsDir = Join-Path $env:LOCALAPPDATA 'iRDCav'
$PrivateSdkDir = Join-Path $ToolsDir 'dotnet'
$Shortcut = Join-Path ([Environment]::GetFolderPath('Programs')) 'iRDCav.lnk'
$WorkDir = Join-Path ([IO.Path]::GetTempPath()) 'iRDCav-setup'

function Assert-Windows {
    if ([Environment]::OSVersion.Platform -ne 'Win32NT') {
        throw 'iRDCav can only be installed on Windows.'
    }
}

function Initialize-WorkDir {
    Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue
    New-Item $WorkDir -ItemType Directory -Force | Out-Null
    # Windows PowerShell 5.1 does not enable TLS 1.2 by default, which GitHub and Microsoft require.
    [Net.ServicePointManager]::SecurityProtocol = [Net.ServicePointManager]::SecurityProtocol -bor [Net.SecurityProtocolType]::Tls12
}

# The source next to this script, or a fresh download of the branch from GitHub.
function Get-SourceDir {
    if ($PSScriptRoot -and (Test-Path (Join-Path $PSScriptRoot $ProjectPath))) {
        return $PSScriptRoot
    }
    Write-Host "Downloading iRDCav ($Branch) from GitHub..."
    $archive = Join-Path $WorkDir 'source.zip'
    Invoke-WebRequest "$Repository/archive/refs/heads/$Branch.zip" -OutFile $archive -UseBasicParsing
    Expand-Archive $archive -DestinationPath (Join-Path $WorkDir 'source') -Force
    $sourceDir = Get-ChildItem (Join-Path $WorkDir 'source') -Directory | Select-Object -First 1
    return $sourceDir.FullName
}

function Test-Dotnet8Sdk([string]$dotnet) {
    try {
        return [bool](& $dotnet --list-sdks 2>$null | Where-Object { $_ -match '^8\.' })
    }
    catch {
        return $false
    }
}

# An installed .NET 8 SDK, or a private one in %LOCALAPPDATA%\iRDCav\dotnet.
function Get-Dotnet {
    $privateDotnet = Join-Path $PrivateSdkDir 'dotnet.exe'
    if ((Get-Command dotnet -ErrorAction SilentlyContinue) -and (Test-Dotnet8Sdk 'dotnet')) {
        return 'dotnet'
    }
    if ((Test-Path $privateDotnet) -and (Test-Dotnet8Sdk $privateDotnet)) {
        return $privateDotnet
    }
    Write-Host 'Installing the .NET 8 SDK for iRDCav...'
    $installer = Join-Path $WorkDir 'dotnet-install.ps1'
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer -UseBasicParsing
    # Out-Host keeps the installer's messages out of this function's return value.
    & $installer -Channel 8.0 -InstallDir $PrivateSdkDir -NoPath | Out-Host
    return $privateDotnet
}

function Build-App([string]$dotnet, [string]$sourceDir) {
    Write-Host 'Building iRDCav...'
    $env:DOTNET_NOLOGO = '1'
    & $dotnet publish (Join-Path $sourceDir $ProjectPath) -c Release -r win-x64 --self-contained true -o (Join-Path $WorkDir 'publish')
    if ($LASTEXITCODE -ne 0) {
        throw "Building iRDCav failed (dotnet publish exit code $LASTEXITCODE)."
    }
}

# A running instance locks its files, so it is closed before files are replaced or removed.
function Remove-App {
    Stop-Process -Name 'IRDCav' -ErrorAction SilentlyContinue
    Wait-Process -Name 'IRDCav' -Timeout 10 -ErrorAction SilentlyContinue
    Remove-Item $InstallDir -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $Shortcut -Force -ErrorAction SilentlyContinue
}

function Copy-App([string]$sourceDir) {
    New-Item $InstallDir -ItemType Directory -Force | Out-Null
    Copy-Item (Join-Path $WorkDir 'publish\*') $InstallDir -Recurse
    $examples = Join-Path $sourceDir 'config-examples'
    if (Test-Path $examples) {
        Copy-Item $examples $InstallDir -Recurse
    }
}

function New-StartMenuShortcut {
    $shell = New-Object -ComObject WScript.Shell
    $link = $shell.CreateShortcut($Shortcut)
    $link.TargetPath = Join-Path $InstallDir 'IRDCav.exe'
    $link.WorkingDirectory = $InstallDir
    $link.Description = 'iRacing Data Companion overlays'
    $link.Save()
}

function Install-App {
    Initialize-WorkDir
    try {
        $sourceDir = Get-SourceDir
        Build-App (Get-Dotnet) $sourceDir
        Remove-App
        Copy-App $sourceDir
        New-StartMenuShortcut
    }
    finally {
        Remove-Item $WorkDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "iRDCav was installed to $InstallDir. Start it from the Start menu entry 'iRDCav'."
}

function Uninstall-App {
    Remove-App
    Remove-Item $ToolsDir -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'iRDCav was removed. Your settings in %APPDATA%\iRDCav were kept.'
}

Assert-Windows
if ($Uninstall) {
    Uninstall-App
}
else {
    Install-App
}
