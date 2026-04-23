$ErrorActionPreference = "Stop"

function Get-OneDriveRoot {
    foreach ($candidate in @($env:OneDrive, $env:OneDriveConsumer, $env:OneDriveCommercial)) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            return $candidate
        }
    }

    return $null
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$oneDriveRoot = Get-OneDriveRoot
$documentsRoot = [Environment]::GetFolderPath("MyDocuments")

if (-not [string]::IsNullOrWhiteSpace($oneDriveRoot)) {
    $sharedSigningRoot = Join-Path $oneDriveRoot "Documents\\EndlessDodge1\\SharedSigning\\Android"
    $syncDescription = "OneDrive Documents shared folder"
}
else {
    $sharedSigningRoot = Join-Path $documentsRoot "EndlessDodge1\\SharedSigning\\Android"
    $syncDescription = "local Documents folder (OneDrive not detected)"
}

$sharedConfigPath = Join-Path $sharedSigningRoot "release-signing.json"
$sharedReleaseKeystorePath = Join-Path $sharedSigningRoot "oreniq-release.keystore"
$sharedDebugKeystorePath = Join-Path $sharedSigningRoot "shared-debug.keystore"
$sharedReadmePath = Join-Path $sharedSigningRoot "README.txt"

$localReleaseConfigPath = Join-Path $repoRoot "UserSettings\\Android\\release-signing.json"
$localReleaseKeystorePath = Join-Path $repoRoot "UserSettings\\Android\\oreniq-release.keystore"
$localDebugKeystorePath = Join-Path $env:USERPROFILE ".android\\debug.keystore"

New-Item -ItemType Directory -Path $sharedSigningRoot -Force | Out-Null

$mode = $null

if ((Test-Path $localReleaseConfigPath) -and (Test-Path $localReleaseKeystorePath)) {
    Copy-Item $localReleaseConfigPath $sharedConfigPath -Force
    Copy-Item $localReleaseKeystorePath $sharedReleaseKeystorePath -Force
    $mode = "release"
}
elseif (Test-Path $localDebugKeystorePath) {
    Copy-Item $localDebugKeystorePath $sharedDebugKeystorePath -Force

    $debugConfig = [ordered]@{
        keystoreName     = "shared-debug.keystore"
        keystorePassword = "android"
        keyaliasName     = "androiddebugkey"
        keyaliasPassword = "android"
    } | ConvertTo-Json

    Set-Content -Path $sharedConfigPath -Value $debugConfig -Encoding UTF8
    $mode = "debug-bridge"
}
else {
    throw "No signing source was found. Looked for `"$localReleaseConfigPath`" + release keystore and `"$localDebugKeystorePath`"."
}

$readme = @"
EndlessDodge1 shared Android signing
===================================

This folder is the canonical cross-PC signing source for local Android builds.

Current mode: $mode

- If mode is release, builds use the recovered release keystore and can stay consistent across both PCs.
- If mode is debug-bridge, builds use the shared copy of the Android debug keystore only so both PCs can update the same phone install during QA.
- Do not commit anything from this folder to Git.
- When the real release keystore is recovered, rerun this script on the primary PC so the shared folder switches from debug-bridge to release mode.
"@

Set-Content -Path $sharedReadmePath -Value $readme -Encoding UTF8

Write-Host ""
Write-Host "Shared Android signing prepared." -ForegroundColor Green
Write-Host "Mode: $mode"
Write-Host "Folder: $sharedSigningRoot"
Write-Host "Sync source: $syncDescription"
Write-Host ""

if ($mode -eq "debug-bridge") {
    Write-Host "This is a temporary device-testing bridge, not the final Play release key." -ForegroundColor Yellow
    Write-Host "Both PCs can now use the same shared debug signing identity when Unity builds a debug APK." -ForegroundColor Yellow
}
else {
    Write-Host "Shared release signing is ready for both PCs." -ForegroundColor Green
}
