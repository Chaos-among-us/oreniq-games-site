$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectVersionPath = Join-Path $repoRoot "ProjectSettings/ProjectVersion.txt"
$templateSourcePath = Join-Path $repoRoot "config/templates/android-release-signing.template.json"
$userSettingsAndroidPath = Join-Path $repoRoot "UserSettings/Android"
$localTemplatePath = Join-Path $userSettingsAndroidPath "release-signing.template.json"
$localConfigPath = Join-Path $userSettingsAndroidPath "release-signing.json"
$localKeystorePath = Join-Path $userSettingsAndroidPath "oreniq-release.keystore"

function Get-OneDriveRoot {
    foreach ($candidate in @($env:OneDrive, $env:OneDriveConsumer, $env:OneDriveCommercial)) {
        if (-not [string]::IsNullOrWhiteSpace($candidate)) {
            return $candidate
        }
    }

    return $null
}

$oneDriveRoot = Get-OneDriveRoot
$sharedSigningPath = if (-not [string]::IsNullOrWhiteSpace($oneDriveRoot)) {
    Join-Path $oneDriveRoot "Documents\\EndlessDodge1\\SharedSigning\\Android"
}
else {
    Join-Path ([Environment]::GetFolderPath("MyDocuments")) "EndlessDodge1\\SharedSigning\\Android"
}
$sharedConfigPath = Join-Path $sharedSigningPath "release-signing.json"
$sharedReleaseKeystorePath = Join-Path $sharedSigningPath "oreniq-release.keystore"
$sharedDebugKeystorePath = Join-Path $sharedSigningPath "shared-debug.keystore"

New-Item -ItemType Directory -Path $userSettingsAndroidPath -Force | Out-Null

if ((Test-Path $templateSourcePath) -and -not (Test-Path $localTemplatePath))
{
    Copy-Item $templateSourcePath $localTemplatePath
}

$unityVersion = "unknown"
if (Test-Path $projectVersionPath)
{
    $versionLine = Select-String -Path $projectVersionPath -Pattern "^m_EditorVersion:" | Select-Object -First 1
    if ($versionLine)
    {
        $unityVersion = $versionLine.Line.Split(":")[1].Trim()
    }
}

Write-Host ""
Write-Host "EndlessDodge1 workstation bootstrap complete." -ForegroundColor Green
Write-Host "Repo root: $repoRoot"
Write-Host "Unity version: $unityVersion"
Write-Host ""
Write-Host "Local Android signing folder: $userSettingsAndroidPath"
Write-Host "Shared Android signing folder: $sharedSigningPath"

if (Test-Path $localConfigPath)
{
    Write-Host "- release-signing.json present" -ForegroundColor Green
}
else
{
    Write-Host "- release-signing.json missing (okay for normal editor work)" -ForegroundColor Yellow
}

if (Test-Path $localKeystorePath)
{
    Write-Host "- oreniq-release.keystore present" -ForegroundColor Green
}
else
{
    Write-Host "- oreniq-release.keystore missing (needed for signed release builds)" -ForegroundColor Yellow
}

if (Test-Path $localTemplatePath)
{
    Write-Host "- release-signing.template.json ready" -ForegroundColor Green
}

if (Test-Path $sharedConfigPath)
{
    Write-Host "- shared release-signing.json present" -ForegroundColor Green
}
else
{
    Write-Host "- shared release-signing.json missing" -ForegroundColor Yellow
}

if (Test-Path $sharedReleaseKeystorePath)
{
    Write-Host "- shared oreniq-release.keystore present" -ForegroundColor Green
}
elseif (Test-Path $sharedDebugKeystorePath)
{
    Write-Host "- shared shared-debug.keystore present (temporary cross-PC debug bridge)" -ForegroundColor Green
}
else
{
    Write-Host "- shared keystore missing" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Read logs.md, ROADMAP.md, and RELEASE_SPRINT.md."
Write-Host "2. Open Unity $unityVersion and wait for import/compile."
Write-Host "3. If this is the primary PC, run scripts/setup-shared-android-signing.ps1 once to seed the shared cross-PC signing folder."
Write-Host "4. If you need a signed Android build on any PC, prefer the shared signing folder over machine-local copies."
