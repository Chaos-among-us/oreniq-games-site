param(
    [string]$DeviceHostPort
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\wireless-adb-common.ps1"

if ([string]::IsNullOrWhiteSpace($DeviceHostPort)) {
    Invoke-UnityAdb -Arguments @("disconnect")
}
else {
    Invoke-UnityAdb -Arguments @("disconnect", $DeviceHostPort)
}

Write-Host ""
Write-Host "Remaining devices:" -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("devices", "-l")
