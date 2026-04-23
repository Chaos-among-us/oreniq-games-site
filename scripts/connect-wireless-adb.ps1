param(
    [Parameter(Mandatory = $true)]
    [string]$DeviceHostPort
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\wireless-adb-common.ps1"

Write-Host ""
Write-Host "Connecting to Android device over Wi-Fi..." -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("connect", $DeviceHostPort)

Write-Host ""
Write-Host "Connected devices:" -ForegroundColor Green
Invoke-UnityAdb -Arguments @("devices", "-l")
