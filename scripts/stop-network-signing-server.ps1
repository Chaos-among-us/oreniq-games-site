$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$pidPath = Join-Path $repoRoot "UserSettings\\Android\\network-signing-server.pid"

if (-not (Test-Path $pidPath)) {
    Write-Host "No saved signing-server PID was found."
    exit 0
}

$pidValue = (Get-Content $pidPath -ErrorAction SilentlyContinue | Select-Object -First 1).Trim()

if ($pidValue) {
    $process = Get-Process -Id $pidValue -ErrorAction SilentlyContinue
    if ($process) {
        Stop-Process -Id $pidValue -Force
        Write-Host "Stopped network signing server PID $pidValue." -ForegroundColor Green
    }
}

Remove-Item $pidPath -Force -ErrorAction SilentlyContinue
