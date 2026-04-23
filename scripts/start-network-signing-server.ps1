param(
    [int]$Port = 8765,
    [string]$SharePath = "$env:USERPROFILE\\OneDrive\\Documents\\EndlessDodge1\\SharedSigning\\Android"
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $SharePath)) {
    throw "Share path not found: $SharePath"
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$stateFolder = Join-Path $repoRoot "UserSettings\\Android"
$pidPath = Join-Path $stateFolder "network-signing-server.pid"

function Get-PreferredIPv4Address {
    $preferred = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object {
            $_.IPAddress -notlike '169.*' -and
            $_.IPAddress -ne '127.0.0.1' -and
            $_.InterfaceAlias -notmatch 'vEthernet|Loopback'
        } |
        Sort-Object {
            if ($_.InterfaceAlias -match 'Wi-Fi|Wireless') { 0 }
            elseif ($_.InterfaceAlias -match 'Ethernet') { 1 }
            else { 2 }
        } |
        Select-Object -First 1

    if ($preferred) {
        return $preferred.IPAddress
    }

    return $null
}

New-Item -ItemType Directory -Path $stateFolder -Force | Out-Null

if (Test-Path $pidPath) {
    $existingPid = (Get-Content $pidPath -ErrorAction SilentlyContinue | Select-Object -First 1).Trim()
    if ($existingPid) {
        $existingProcess = Get-Process -Id $existingPid -ErrorAction SilentlyContinue
        if ($existingProcess) {
            Write-Host ""
            Write-Host "Network signing server is already running." -ForegroundColor Green
            Write-Host "PID: $existingPid"
            Write-Host "URL: http://$env:COMPUTERNAME`:$Port/"
            exit 0
        }
    }
}

$arguments = @(
    "-m"
    "http.server"
    "$Port"
    "--bind"
    "0.0.0.0"
    "--directory"
    $SharePath
)

$process = Start-Process -FilePath "python" -ArgumentList $arguments -PassThru -WindowStyle Hidden
Set-Content -Path $pidPath -Value $process.Id -Encoding ASCII

Start-Sleep -Seconds 2

$localUrl = "http://127.0.0.1:$Port/release-signing.json"
try {
    Invoke-WebRequest -Uri $localUrl -UseBasicParsing | Out-Null
}
catch {
    throw "The local signing server process started, but release-signing.json was not reachable at $localUrl"
}

$preferredIp = Get-PreferredIPv4Address

Write-Host ""
Write-Host "Network signing server started." -ForegroundColor Green
Write-Host "PID: $($process.Id)"
Write-Host "Local test URL: $localUrl"
Write-Host "Laptop URL: http://$env:COMPUTERNAME`:$Port/"
if ($preferredIp) {
    Write-Host "Fallback laptop URL on this network: http://$preferredIp`:$Port/"
}
Write-Host ""
Write-Host "Stop it later with:" -ForegroundColor Cyan
Write-Host "powershell -ExecutionPolicy Bypass -File scripts/stop-network-signing-server.ps1"
