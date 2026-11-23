param([string]$EvidenceDirectory)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$patcherRoot = Join-Path $PSScriptRoot 'Tools\UnityPatcher'
$patcherExe = Join-Path $patcherRoot 'UnityApplicationPatcherCLI.exe'
$officialUrl = 'https://security-patches.unity.com/bc0977e0-21a9-4f6e-9414-4f44b242110a/unity-patcher/UnityApplicationPatcher-1.3.3-Win.zip'
if (-not (Test-Path -LiteralPath $patcherExe)) {
    New-Item -ItemType Directory -Path $patcherRoot -Force | Out-Null
    $download = Join-Path $PSScriptRoot 'Tools\UnityPatcher.zip'
    Invoke-WebRequest -Uri $officialUrl -OutFile $download
    Expand-Archive -LiteralPath $download -DestinationPath $patcherRoot -Force
}
$signature = Get-AuthenticodeSignature -LiteralPath $patcherExe
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notlike '*O=Unity Technologies SF*') {
    throw 'Unity Application Patcher must have a valid Unity Technologies signature.'
}
$playerDll = Join-Path $projectRoot 'Builds\EchoProtocol\UnityPlayer.dll'
$before = (Get-FileHash -LiteralPath $playerDll -Algorithm SHA256).Hash
$logRoot = Join-Path $PSScriptRoot 'Logs'
New-Item -ItemType Directory -Path $logRoot -Force | Out-Null
$stdout = Join-Path $logRoot 'unity-patcher-stdout.txt'
$stderr = Join-Path $logRoot 'unity-patcher-stderr.txt'
$patcher = Start-Process -FilePath $patcherExe -ArgumentList @('-windows', '-unityPlayerLibrary', ('"' + $playerDll + '"')) -WorkingDirectory $patcherRoot -WindowStyle Hidden -RedirectStandardOutput $stdout -RedirectStandardError $stderr -Wait -PassThru
if ($patcher.ExitCode -notin 0,183) { throw "Official Unity Windows patch failed: $($patcher.ExitCode). See $stdout and $stderr." }
$after = (Get-FileHash -LiteralPath $playerDll -Algorithm SHA256).Hash
if ($patcher.ExitCode -eq 0 -and $before -eq $after) { throw 'Patcher reported success but UnityPlayer.dll did not change.' }
$evidence = @('Unity Application Patcher 1.3.3', 'Valid Authenticode: Unity Technologies SF', "Exit code: $($patcher.ExitCode)", "Before SHA256: $before", "After SHA256: $after", "Source: $officialUrl")
if ($EvidenceDirectory) {
    New-Item -ItemType Directory -Path $EvidenceDirectory -Force | Out-Null
    $evidence | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'security-patch-result.txt') -Encoding utf8
    Copy-Item -LiteralPath $stdout,$stderr -Destination $EvidenceDirectory -Force
}
$evidence
