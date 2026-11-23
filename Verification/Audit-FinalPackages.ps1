param([string]$PackageName = 'EchoProtocol_Bakim_Hatti_Ornek_2026-10-10.zip', [string]$ReleaseDate = '2026-10-10', [string]$ResultName = 'MaintenanceSample')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$delivery = Join-Path $projectRoot 'Deliverables'
$evidence = Join-Path $PSScriptRoot ('Results\' + $ReleaseDate + '\' + $ResultName)
$expectedReleasePrefix = 'EP-' + $ReleaseDate.Replace('-', '') + '-SAMPLE'
Add-Type -AssemblyName System.IO.Compression.FileSystem
function Entry-Hash($entry) {
    $sha = [System.Security.Cryptography.SHA256]::Create(); $stream = $entry.Open()
    try { return [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','').ToLowerInvariant() }
    finally { $stream.Dispose(); $sha.Dispose() }
}
$source = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $delivery 'EchoProtocol_Unity_Source.zip'))
$shared = [System.IO.Compression.ZipFile]::OpenRead((Join-Path $delivery $PackageName))
try {
    $fixtureLine = Get-Content (Join-Path $evidence 'package-fixtures.txt') | Where-Object { $_.StartsWith('Source fixture: ') }
    $fixtureSource = [System.IO.Path]::GetFullPath($fixtureLine.Substring(16))
    $fixtureBoundary = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'PackageSmoke')) + '\'
    if (-not $fixtureSource.StartsWith($fixtureBoundary, [System.StringComparison]::OrdinalIgnoreCase)) { throw 'Unexpected fresh-import fixture path.' }
    $sourceCount = 0
    $files = foreach ($part in @('Assets','Packages','ProjectSettings')) { Get-ChildItem -LiteralPath (Join-Path $projectRoot $part) -Recurse -File }
    $files += Get-ChildItem -LiteralPath $projectRoot -Filter '*.md' -File
    $files += Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object { $_.Extension -in '.cs','.ps1' }
    foreach ($file in $files) {
        $relative = $file.FullName.Substring($projectRoot.Length + 1).Replace('\','/')
        $entry = $source.GetEntry($relative)
        if (-not $entry -or (Entry-Hash $entry) -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()) { throw "Final source ZIP differs from workspace: $relative" }
        if ($relative.StartsWith('Assets/') -or $relative.StartsWith('Packages/') -or $relative.StartsWith('ProjectSettings/')) {
            $freshFile = Join-Path $fixtureSource $relative
            if (-not (Test-Path -LiteralPath $freshFile) -or (Get-FileHash -LiteralPath $freshFile -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash) { throw "Final runtime payload differs from fresh-import verified source: $relative" }
        }
        $sourceCount++
    }
    $buildRoot = Join-Path $projectRoot 'Builds\EchoProtocol'
    $buildCount = 0
    foreach ($file in (Get-ChildItem -LiteralPath $buildRoot -Recurse -File)) {
        $relative = 'EchoProtocol/' + $file.FullName.Substring($buildRoot.Length + 1).Replace('\','/')
        $entry = $shared.GetEntry($relative)
        if (-not $entry -or (Entry-Hash $entry) -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant()) { throw "Final shared ZIP differs from validated build: $relative" }
        $buildCount++
    }
    foreach ($name in @('BASLA_BURADAN.txt','DENEME_REHBERI.txt','GERI_BILDIRIM.txt')) {
        $entry = $shared.GetEntry($name); if (-not $entry) { throw "Missing playtest guide: $name" }
        $reader = [System.IO.StreamReader]::new($entry.Open())
        try { $text = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($text.Length -lt 300 -or -not $text.Contains($expectedReleasePrefix)) { throw "Incomplete or wrong-version guide: $name" }
    }
    @("PASS: final source ZIP matches $sourceCount code/asset/config/document files", "PASS: final shared ZIP matches all $buildCount validated Windows build files", 'PASS: three nonempty SAMPLE playtest guides / blank feedback form', 'Source runtime payload is unchanged from the fresh-import verified candidate; final report/evidence is repackaged.') |
        Set-Content (Join-Path $evidence 'final-package-audit-result.txt') -Encoding utf8
    Write-Output "PASS: final archive payload audit / $sourceCount source files / $buildCount Windows files"
} finally { $source.Dispose(); $shared.Dispose() }
