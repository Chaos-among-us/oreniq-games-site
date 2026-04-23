$ErrorActionPreference = "Stop"
. "$PSScriptRoot\wireless-adb-common.ps1"

Write-Host ""
Write-Host "ADB version:" -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("version")

Write-Host ""
Write-Host "mDNS availability:" -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("mdns", "check")

Write-Host ""
Write-Host "Discovered mDNS services:" -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("mdns", "services")

Write-Host ""
Write-Host "Connected devices:" -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("devices", "-l")
