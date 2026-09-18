#Requires -Version 5.1
<#
.SYNOPSIS
    Fresh Windows install entry point for win-setup.

.DESCRIPTION
    Downloads the latest win-setup release and runs status, then apply.
    Run from an elevated PowerShell or let the script elevate itself:

        irm https://raw.githubusercontent.com/Furyfree/win-setup/main/bootstrap.ps1 | iex
#>
[CmdletBinding()]
param(
    [string]$Repo = 'Furyfree/win-setup',
    [switch]$SkipApply
)

$ErrorActionPreference = 'Stop'

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
    [Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host 'Elevating...'
    $command = "& { irm -UseBasicParsing https://raw.githubusercontent.com/$Repo/main/bootstrap.ps1 | iex }"
    Start-Process powershell -Verb RunAs -ArgumentList @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-NoExit', '-Command', $command)
    return
}

$release = Invoke-RestMethod "https://api.github.com/repos/$Repo/releases/latest"
$asset = $release.assets | Where-Object name -eq 'win-setup.exe' | Select-Object -First 1
if (-not $asset) {
    throw "win-setup.exe was not found in the latest release of $Repo"
}

$exe = Join-Path $env:TEMP 'win-setup.exe'
Write-Host "Downloading $($asset.browser_download_url)"
Invoke-WebRequest -UseBasicParsing $asset.browser_download_url -OutFile $exe
Unblock-File -Path $exe

& $exe status
if (-not $SkipApply) {
    & $exe apply
}
