param(
    [Parameter(Mandatory = $true)]
    [string]$ServerRoot,
    [string]$DestinationRoot = ([IO.Path]::Combine([Environment]::GetFolderPath("MyDocuments"), "EndlessDodge1", "NetworkSigning", "Android"))
)

$ErrorActionPreference = "Stop"

$normalizedServerRoot = $ServerRoot.Trim().TrimEnd('/')
New-Item -ItemType Directory -Path $DestinationRoot -Force | Out-Null

$configUrl = "$normalizedServerRoot/release-signing.json"
$configPath = Join-Path $DestinationRoot "release-signing.json"

Invoke-WebRequest -Uri $configUrl -OutFile $configPath -UseBasicParsing

$config = Get-Content $configPath | ConvertFrom-Json

if ([string]::IsNullOrWhiteSpace($config.keystoreName)) {
    throw "Downloaded release-signing.json did not contain a keystoreName."
}

$keystoreUrl = "$normalizedServerRoot/$($config.keystoreName)"
$keystorePath = Join-Path $DestinationRoot $config.keystoreName

Invoke-WebRequest -Uri $keystoreUrl -OutFile $keystorePath -UseBasicParsing

[Environment]::SetEnvironmentVariable("ENDLESSDODGE_SIGNING_ROOT", $DestinationRoot, "User")
$env:ENDLESSDODGE_SIGNING_ROOT = $DestinationRoot

Write-Host ""
Write-Host "Shared Android signing synced locally." -ForegroundColor Green
Write-Host "Source: $normalizedServerRoot"
Write-Host "Local cache: $DestinationRoot"
Write-Host ""
Write-Host "ENDLESSDODGE_SIGNING_ROOT has been set for this user." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Restart Unity if it is already open."
Write-Host "2. Run powershell -ExecutionPolicy Bypass -File scripts/bootstrap-workstation.ps1"
Write-Host "3. Build with Tools/Android/Build And Install Debug APK"
