param(
    [string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\2021.3.21f1\Editor\Unity.exe',
    [string]$ResultName = 'Hardening',
    [switch]$SkipBuild
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$logRoot = Join-Path $PSScriptRoot 'Logs'
$resultRoot = Join-Path $PSScriptRoot ('Results\' + (Get-Date -Format 'yyyy-MM-dd') + '\' + $ResultName)
New-Item -ItemType Directory -Path $logRoot, $resultRoot -Force | Out-Null
$expectedChecks = [int]([regex]::Match((Get-Content (Join-Path $projectRoot 'Assets/EchoProtocol/Runtime/08_Otomatik_Testler/EchoGameChecks.cs') -Raw), 'ExpectedCheckCount = (\d+)').Groups[1].Value)

function Invoke-UnityBatch {
    param([string]$Method, [string]$LogName)
    $logPath = Join-Path $logRoot $LogName
    $batchArguments = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', $Method, '-logFile', ('"' + $logPath + '"'))
    $batch = Start-Process -FilePath $UnityExe -ArgumentList $batchArguments -WindowStyle Hidden -PassThru
    $batch.WaitForExit()
    if ($batch.ExitCode -ne 0) { throw "Unity failed: $Method / code $($batch.ExitCode). See $logPath" }
}

Push-Location -LiteralPath $projectRoot
try {
    & (Join-Path $PSScriptRoot 'Check-Compile.ps1') | Tee-Object -FilePath (Join-Path $resultRoot 'compile-timeline-result.txt')
    Invoke-UnityBatch 'EchoProtocol.Editor.EchoPlayValidation.BatchSmokeChecks' 'unity-hardening-final.log'
    $playResult = Get-Content -LiteralPath (Join-Path $logRoot 'playmode-result.txt') -Raw
    if ($playResult -notmatch "PASS: $expectedChecks/$expectedChecks Unity game checks") { throw 'Play Mode did not complete the expected suite.' }
    Copy-Item -LiteralPath (Join-Path $logRoot 'playmode-result.txt') -Destination $resultRoot -Force
    Write-Output "PASS: Unity Play Mode $expectedChecks/$expectedChecks"
    if (-not $SkipBuild) { Invoke-UnityBatch 'EchoProtocol.Editor.EchoPlayValidation.BatchBuildPrototype' 'unity-hardening-build-final.log' }
    $buildResult = Get-Content -LiteralPath (Join-Path $logRoot 'build-result.txt') -Raw
    if ($buildResult -notmatch 'Succeeded' -or $buildResult -notmatch 'Errors: 0' -or $buildResult -notmatch 'Warnings: 0') { throw 'Build must succeed without errors or warnings.' }
    Copy-Item -LiteralPath (Join-Path $logRoot 'build-result.txt') -Destination $resultRoot -Force
    Write-Output 'PASS: Windows build / 0 errors / 0 warnings'
    & (Join-Path $PSScriptRoot 'Patch-Windows.ps1') -EvidenceDirectory $resultRoot
    Add-Type -AssemblyName System.Drawing
    foreach ($resolution in @(@(1280,720), @(800,600), @(1920,1080))) {
        $label = "$($resolution[0])x$($resolution[1])"
        $playerLog = Join-Path $logRoot ('hardening-' + $label + '.log')
        $capturePrefix = Join-Path $resultRoot ('game-' + $label)
        $playerArguments = @('-echo-smoke-test', '-echo-capture', ('"' + $capturePrefix + '"'), '-screen-fullscreen', '0', '-screen-width', $resolution[0], '-screen-height', $resolution[1], '-logFile', ('"' + $playerLog + '"'))
        # A hidden/minimized Windows player may skip OnGUI and produce black framebuffers.
        # This is an interactive game verification, so keep its test window visible.
        $game = Start-Process -FilePath (Join-Path $projectRoot 'Builds\EchoProtocol\EchoProtocol.exe') -ArgumentList $playerArguments -WindowStyle Normal -PassThru
        $game.WaitForExit()
        $contents = Get-Content -LiteralPath $playerLog
        $checks = @($contents | Where-Object { $_ -like 'ECHO CHECK PASS:*' })
        $pass = @($contents | Where-Object { $_ -like 'ECHO STANDALONE PASS:*' })
        if ($game.ExitCode -ne 0 -or $checks.Count -ne $expectedChecks -or ($checks | Sort-Object -Unique).Count -ne $expectedChecks -or $pass.Count -ne 1) {
            throw "Standalone $label failed: exit $($game.ExitCode), $($checks.Count) checks. See $playerLog"
        }
        $errors = @($contents | Where-Object { $_ -match 'Exception:|error CS|ECHO SMOKE FAIL|ECHO STANDALONE FAIL' })
        if ($errors.Count -gt 0) { throw "Standalone $label contains runtime errors: $($errors -join '; ')" }
        foreach ($capture in @('echo-shot-anticipation', 'echo-shot-response', 'echo-team-hit', 'echo-guard-focus', 'echo-teaching', 'echo-record-choice', 'echo-deadline-choice', 'sample-observation', 'sample-archive', 'sample-service', 'sample-transfer', 'sample-event-map', 'sample-transfer-open', 'sample-combat', 'sample-memory', 'sample-final-plan', 'sample-win', 'archives', 'archive-sector-1', 'archive-sector-2', 'archive-sector-3', 'immersion-sector-1', 'immersion-sector-2', 'immersion-sector-3', 'resonance-map-3', 'planning-dual-echo', 'planning', 'restored-win', 'result', 'settings', 'controls', 'win', 'combat', 'loss', 'level-3-dual-echo', 'art-sector-1', 'art-sector-2', 'art-sector-3')) {
            $resultImage = [System.Drawing.Bitmap]::new($capturePrefix + '-' + $capture + '.png')
            try {
                if ($resultImage.Width -ne $resolution[0] -or $resultImage.Height -ne $resolution[1]) {
                    throw "Unexpected actual framebuffer dimensions for $label / $capture"
                }
                $litSamples = 0
                for ($gridX = 1; $gridX -lt 10; $gridX++) {
                    for ($gridY = 1; $gridY -lt 10; $gridY++) {
                        $pixel = $resultImage.GetPixel([int]($resultImage.Width * $gridX / 10), [int]($resultImage.Height * $gridY / 10))
                        if (($pixel.R + $pixel.G + $pixel.B) -gt 30) { $litSamples++ }
                    }
                }
                if ($litSamples -lt 10) { throw "Blank/unrendered capture: $label / $capture" }
            } finally { $resultImage.Dispose() }
        }
        @("Windows standalone $label", "PASS: $expectedChecks/$expectedChecks", 'Exit code: 0', 'Runtime errors: 0') + $checks +
            @($contents | Where-Object { $_ -like 'ECHO BENCHMARK*' }) |
            Set-Content -LiteralPath (Join-Path $resultRoot ('standalone-' + $label + '-result.txt')) -Encoding utf8
        Write-Output "PASS: Windows standalone $label / $expectedChecks checks / runtime errors 0"
    }
    Copy-Item -LiteralPath (Join-Path $logRoot 'level-3-dual-echo.png') -Destination $resultRoot -Force
    Write-Output "Release checks completed. Evidence: $resultRoot"
} finally { Pop-Location }
