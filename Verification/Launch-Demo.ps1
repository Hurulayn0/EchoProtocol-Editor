param(
    [switch]$Interactive
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $projectRoot 'Builds\EchoProtocol'
$exe = Join-Path $buildDir 'EchoProtocol.exe'
$log = Join-Path $PSScriptRoot 'Logs\live-play.log'

if (-not (Test-Path -LiteralPath $exe)) {
    throw "Executable not found at $exe. Run build first."
}

if ($Interactive) {
    Write-Output "Launching Echo Protocol in interactive playable mode (1280x720 windowed)..."
    Start-Process -FilePath $exe -WorkingDirectory $buildDir -ArgumentList @('-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720')
} else {
    Write-Output "Launching Echo Protocol in auto-play demonstration mode (1280x720 windowed)..."
    Start-Process -FilePath $exe -WorkingDirectory $buildDir -ArgumentList @('-echo-smoke-test', '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', $log)
}
