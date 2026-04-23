param(
    [Parameter(Mandatory = $true)]
    [string]$ShareRoot
)

$ErrorActionPreference = "Stop"

$normalizedRoot = $ShareRoot.Trim().TrimEnd('\')

if (-not (Test-Path $normalizedRoot)) {
    throw "The shared signing root is not reachable: $normalizedRoot"
}

[Environment]::SetEnvironmentVariable("ENDLESSDODGE_SIGNING_ROOT", $normalizedRoot, "User")
$env:ENDLESSDODGE_SIGNING_ROOT = $normalizedRoot

Write-Host ""
Write-Host "ENDLESSDODGE_SIGNING_ROOT set for this user." -ForegroundColor Green
Write-Host "Value: $normalizedRoot"
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Restart Unity if it is already open."
Write-Host "2. Run powershell -ExecutionPolicy Bypass -File scripts/bootstrap-workstation.ps1"
Write-Host "3. Build with Tools/Android/Build And Install Debug APK"
