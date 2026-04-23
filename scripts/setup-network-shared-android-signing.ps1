param(
    [string]$ShareName = "EndlessDodgeSigning",
    [string]$SharePath = "$env:USERPROFILE\\OneDrive\\Documents\\EndlessDodge1\\SharedSigning\\Android",
    [string]$ReadAccess = "Everyone"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SharePath)) {
    throw "Share path not found: $SharePath"
}

$existingShare = Get-SmbShare -Name $ShareName -ErrorAction SilentlyContinue

if ($null -ne $existingShare) {
    if ($existingShare.Path -ne $SharePath) {
        throw "SMB share '$ShareName' already exists at '$($existingShare.Path)'. Remove or rename it before reusing this share name."
    }
}
else {
    New-SmbShare -Name $ShareName -Path $SharePath -ReadAccess $ReadAccess -Description "Endless Dodge shared Android signing (read-only)" | Out-Null
}

$shareUnc = "\\$env:COMPUTERNAME\$ShareName"

Write-Host ""
Write-Host "Network-shared Android signing is ready." -ForegroundColor Green
Write-Host "UNC path: $shareUnc"
Write-Host "Local path: $SharePath"
Write-Host "Read access: $ReadAccess"
Write-Host ""
Write-Host "Next step on the laptop:" -ForegroundColor Cyan
Write-Host "powershell -ExecutionPolicy Bypass -File scripts/use-network-shared-android-signing.ps1 -ShareRoot '$shareUnc'"
