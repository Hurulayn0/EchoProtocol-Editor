param([string]$ReleaseDate = '2026-10-09', [string]$UnityExe = 'C:\Program Files\Unity\Hub\Editor\2021.3.21f1\Editor\Unity.exe', [string]$ResultName = 'WindowsDemo', [string]$PackageName = '')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$delivery = Join-Path $projectRoot 'Deliverables'
$evidence = Join-Path $PSScriptRoot ('Results\' + $ReleaseDate + '\' + $ResultName)
$sharedName = if ($PackageName) { $PackageName } else { 'EchoProtocol_Arkadas_Test_' + $ReleaseDate + '.zip' }
if ([System.IO.Path]::GetFileName($sharedName) -ne $sharedName -or -not $sharedName.EndsWith('.zip')) { throw 'PackageName must be a ZIP filename without a directory.' }
$expected = [int]([regex]::Match((Get-Content (Join-Path $projectRoot 'Assets\EchoProtocol\Runtime\EchoGameChecks.cs') -Raw), 'ExpectedCheckCount = (\d+)').Groups[1].Value)
$fixtureRoot = Join-Path $PSScriptRoot ('PackageSmoke\Demo-' + $ReleaseDate.Replace('-','') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8))
$sourceRoot = Join-Path $fixtureRoot 'Source'
$playerRoot = Join-Path $fixtureRoot 'Windows'
New-Item -ItemType Directory -Path $evidence,$sourceRoot,$playerRoot -Force | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Entry-Hash($entry) {
    $sha = [System.Security.Cryptography.SHA256]::Create(); $stream = $entry.Open()
    try { return [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','').ToLowerInvariant() }
    finally { $stream.Dispose(); $sha.Dispose() }
}
$standard = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $delivery 'EchoProtocol_Windows.zip'))
$shared = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $delivery $sharedName))
try {
    $matched = 0
    foreach ($entry in $standard.Entries) {
        if ($entry.FullName.EndsWith('/')) { continue }
        $peer = $shared.GetEntry($entry.FullName)
        if (-not $peer -or (Entry-Hash $entry) -ne (Entry-Hash $peer)) { throw "Shared package changed build payload: $($entry.FullName)" }
        $matched++
    }
    $dll = $shared.GetEntry('EchoProtocol/UnityPlayer.dll')
    if (-not $dll -or (Entry-Hash $dll) -ne (Get-FileHash (Join-Path $projectRoot 'Builds\EchoProtocol\UnityPlayer.dll')).Hash.ToLowerInvariant()) { throw 'Shared package does not contain the verified patched DLL.' }
    "PASS: $matched build files identical in Windows and friend ZIPs; patched DLL preserved" | Set-Content (Join-Path $evidence 'package-integrity-result.txt') -Encoding utf8
} finally { $standard.Dispose(); $shared.Dispose() }
Expand-Archive -LiteralPath (Join-Path $delivery 'EchoProtocol_Unity_Source.zip') -DestinationPath $sourceRoot
Expand-Archive -LiteralPath (Join-Path $delivery $sharedName) -DestinationPath $playerRoot
if (Test-Path -LiteralPath (Join-Path $sourceRoot 'Library')) { throw 'Source ZIP must not contain a Library cache.' }
$sourceLog = Join-Path $evidence 'source-import-playerchecks.log'
$editor = Start-Process -FilePath $UnityExe -ArgumentList @('-batchmode','-projectPath',('"' + $sourceRoot + '"'),'-executeMethod','EchoProtocol.Editor.EchoPlayValidation.BatchSmokeChecks','-logFile',('"' + $sourceLog + '"')) -WorkingDirectory $sourceRoot -WindowStyle Hidden -PassThru
$editor.WaitForExit()
$result = Get-Content (Join-Path $sourceRoot 'Verification\Logs\playmode-result.txt') -Raw
if ($editor.ExitCode -ne 0 -or $result -notmatch "PASS: $expected/$expected Unity game checks") { throw "Fresh source import failed. See $sourceLog" }
$result | Set-Content (Join-Path $evidence 'source-zip-result.txt') -Encoding utf8
Write-Output "PASS: fresh source ZIP import / $expected checks"
$playerLog = Join-Path $evidence 'shared-zip-player.log'
$capture = Join-Path $evidence 'shared-zip'
$game = Start-Process -FilePath (Join-Path $playerRoot 'EchoProtocol\EchoProtocol.exe') -ArgumentList @('-echo-smoke-test','-echo-capture',('"' + $capture + '"'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"' + $playerLog + '"')) -WorkingDirectory $playerRoot -WindowStyle Normal -PassThru
$game.WaitForExit()
$lines = Get-Content $playerLog
$checks = @($lines | Where-Object { $_ -like 'ECHO CHECK PASS:*' })
if ($game.ExitCode -ne 0 -or $checks.Count -ne $expected -or ($checks | Sort-Object -Unique).Count -ne $expected -or @($lines | Where-Object { $_ -like 'ECHO STANDALONE PASS:*' }).Count -ne 1 -or @($lines | Where-Object { $_ -match 'Exception:|error CS|ECHO SMOKE FAIL|ECHO STANDALONE FAIL' }).Count -ne 0) { throw "Extracted shared player failed. See $playerLog" }
@("PASS: extracted friend ZIP / $expected/$expected",'Exit code: 0','Runtime errors: 0') + $checks | Set-Content (Join-Path $evidence 'shared-zip-result.txt') -Encoding utf8
"Source fixture: $sourceRoot`nPlayer fixture: $playerRoot" | Set-Content (Join-Path $evidence 'package-fixtures.txt') -Encoding utf8
Write-Output "PASS: extracted friend ZIP / $expected checks"
