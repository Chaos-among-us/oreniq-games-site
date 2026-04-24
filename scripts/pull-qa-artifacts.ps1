param(
    [string]$Serial = "",
    [string]$OutputRoot = "",
    [switch]$SkipVideos
)

. "$PSScriptRoot\wireless-adb-common.ps1"

$adbPath = Get-UnityAdbPath
$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $projectRoot "Builds\PhoneQaArtifacts"
}

function Get-ConnectedAndroidSerial {
    $deviceLines = & $adbPath devices -l |
        Where-Object { $_ -match '^\S+\s+device(\s|$)' }

    $serials = @($deviceLines | ForEach-Object { ($_ -split '\s+')[0] })

    if ($serials.Count -eq 0) {
        throw "No connected Android device was found. Run scripts\status-wireless-adb.ps1 or connect the phone first."
    }

    $wirelessSerials = @($serials | Where-Object { $_ -like "*:*" })

    if ($wirelessSerials.Count -eq 1) {
        return $wirelessSerials[0]
    }

    if ($serials.Count -eq 1) {
        return $serials[0]
    }

    throw "Multiple Android devices are connected. Rerun with -Serial and one of: $($serials -join ', ')"
}

if ([string]::IsNullOrWhiteSpace($Serial)) {
    $Serial = Get-ConnectedAndroidSerial
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$outputDirectory = Join-Path $OutputRoot $timestamp
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

$remoteSources = @(
    [pscustomobject]@{
        Name = "downloaded-packages"
        Path = "/sdcard/Download/EndlessDodgeQA"
    },
    [pscustomobject]@{
        Name = "qa-reports"
        Path = "/sdcard/Android/data/com.oreniq.endlessdodge/files/qa-reports"
    }
)

if (-not $SkipVideos) {
    $remoteSources += [pscustomobject]@{
        Name = "qa-videos"
        Path = "/sdcard/Android/data/com.oreniq.endlessdodge/files/Movies/qa"
    }
}

Write-Host "Pulling Endless Dodge QA artifacts from $Serial..."

foreach ($source in $remoteSources) {
    $targetDirectory = Join-Path $outputDirectory $source.Name
    New-Item -ItemType Directory -Force -Path $targetDirectory | Out-Null

    & $adbPath -s $Serial shell ls -d $source.Path *> $null

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Phone path not found: $($source.Path)"
        continue
    }

    Write-Host "Pulling $($source.Path)"
    & $adbPath -s $Serial pull "$($source.Path)/." $targetDirectory

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to pull $($source.Path)"
    }
}

$reportFiles = Get-ChildItem -Path $outputDirectory -Recurse -Filter "*.txt" -ErrorAction SilentlyContinue
$zipFiles = Get-ChildItem -Path $outputDirectory -Recurse -Filter "*.zip" -ErrorAction SilentlyContinue
$videoFiles = Get-ChildItem -Path $outputDirectory -Recurse -Filter "*.mp4" -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "QA artifacts copied to:"
Write-Host $outputDirectory
Write-Host ""
Write-Host "Pulled $($reportFiles.Count) report(s), $($zipFiles.Count) exported package(s), and $($videoFiles.Count) video(s)."

if ($reportFiles.Count -gt 0) {
    Write-Host ""
    Write-Host "Reports:"
    $reportFiles | ForEach-Object { Write-Host " - $($_.FullName)" }
}
