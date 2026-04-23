param(
    [Parameter(Mandatory = $true)]
    [string]$PairHostPort,
    [string]$PairingCode
)

$ErrorActionPreference = "Stop"
. "$PSScriptRoot\wireless-adb-common.ps1"

if ([string]::IsNullOrWhiteSpace($PairingCode)) {
    $secureCode = Read-Host "Enter the six-digit pairing code shown on the phone"
    $PairingCode = $secureCode
}

Write-Host ""
Write-Host "Pairing with Android device over Wi-Fi..." -ForegroundColor Cyan
Invoke-UnityAdb -Arguments @("pair", $PairHostPort, $PairingCode)
