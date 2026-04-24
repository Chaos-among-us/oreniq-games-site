param(
    [int]$Port = 8787,
    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"
$projectRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$collectorScript = Join-Path $PSScriptRoot "qa-upload-collector.mjs"

if (-not (Test-Path $collectorScript)) {
    throw "Missing collector script: $collectorScript"
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $projectRoot "Builds\QaCollectorInbox"
}

$candidateNodePaths = @(
    (Join-Path $env:USERPROFILE ".cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe")
)

$pathNode = Get-Command node -ErrorAction SilentlyContinue

if ($null -ne $pathNode) {
    $candidateNodePaths += $pathNode.Source
}

$nodePath = $null

foreach ($candidate in $candidateNodePaths) {
    if ([string]::IsNullOrWhiteSpace($candidate) -or -not (Test-Path $candidate)) {
        continue
    }

    & $candidate --version *> $null

    if ($LASTEXITCODE -eq 0) {
        $nodePath = $candidate
        break
    }
}

if ([string]::IsNullOrWhiteSpace($nodePath)) {
    throw "Node.js was not found or could not be started. Install Node.js or use the bundled Codex runtime."
}

$wifiIp = Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object {
        $_.IPAddress -notlike "127.*" -and
        $_.AddressState -eq "Preferred" -and
        $_.InterfaceAlias -notmatch "Loopback|vEthernet|Virtual|VMware|Bluetooth"
    } |
    Select-Object -First 1 -ExpandProperty IPAddress

Write-Host "Endless Dodge QA collector"
Write-Host "Repo folder: $projectRoot"
Write-Host "Output folder: $OutputRoot"
Write-Host ""

if (-not [string]::IsNullOrWhiteSpace($wifiIp)) {
    Write-Host "Phone upload URL: http://$($wifiIp):$Port/qa-upload"
    Write-Host "The QA build is currently configured for http://10.0.0.7:$Port/qa-upload."
    Write-Host ""
}

Write-Host "Leave this window open while testers submit QA packages."
Write-Host ""

& $nodePath $collectorScript --port $Port --output $OutputRoot
