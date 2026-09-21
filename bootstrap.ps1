#Requires -Version 5.1
<#
.SYNOPSIS
    Fresh Windows install entry point for win-setup.

.DESCRIPTION
    Downloads the latest win-setup release, installs it to
    %LOCALAPPDATA%\Programs\win-setup, adds it to the user PATH, runs status,
    then requests elevation for apply.

        irm https://raw.githubusercontent.com/Furyfree/win-setup/main/bootstrap.ps1 | iex
#>
[CmdletBinding()]
param(
    [string]$Repo = 'Furyfree/win-setup',
    [switch]$SkipApply
)

$ErrorActionPreference = 'Stop'

$release = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/latest"
$asset = $release.assets | Where-Object name -eq 'win-setup.exe' | Select-Object -First 1
if (-not $asset) {
    throw "win-setup.exe was not found in the latest release of $Repo"
}

$installDir = Join-Path $env:LOCALAPPDATA 'Programs\win-setup'
$installed = Join-Path $installDir 'win-setup.exe'
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Write-Host "Downloading $($asset.browser_download_url)"
$download = Join-Path $installDir ([IO.Path]::GetRandomFileName())
try {
    Invoke-WebRequest -UseBasicParsing $asset.browser_download_url -OutFile $download
    if ($asset.size -le 0 -or (Get-Item -LiteralPath $download).Length -ne $asset.size) {
        throw 'Release download is incomplete; the existing installation was preserved.'
    }
    Unblock-File -Path $download
    if (Test-Path -LiteralPath $installed) {
        [IO.File]::Replace($download, $installed, $null)
    }
    else {
        [IO.File]::Move($download, $installed)
    }
}
finally {
    if (Test-Path -LiteralPath $download) {
        Remove-Item -LiteralPath $download -Force
    }
}

$userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
if (($userPath -split ';') -notcontains $installDir) {
    $newPath = (@($userPath, $installDir) | Where-Object { $_ }) -join ';'
    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
    Write-Host "Added $installDir to the user PATH (new terminals)."
}
$env:Path = "$env:Path;$installDir"

& $installed status

if ($SkipApply) {
    return
}

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if ($isAdmin) {
    & $installed apply
    return
}

Write-Host 'Requesting elevation for apply...'
try {
    Start-Process -FilePath $installed -ArgumentList 'apply' -Verb RunAs -Wait
}
catch {
    Write-Host "Elevation was not granted. Open an elevated terminal and run: win-setup apply"
}
