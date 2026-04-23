function Get-UnityAdbPath {
    $adbPath = "C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe"

    if (-not (Test-Path $adbPath)) {
        throw "Unity adb.exe was not found at $adbPath"
    }

    return $adbPath
}

function Invoke-UnityAdb {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $adbPath = Get-UnityAdbPath
    & $adbPath @Arguments
}
