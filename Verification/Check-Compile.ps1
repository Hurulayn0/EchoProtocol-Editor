param(
    [string]$UnityEditorPath = 'C:\Program Files\Unity\Hub\Editor\2021.3.21f1\Editor'
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$unityData = Join-Path $UnityEditorPath 'Data'
$monoPath = Join-Path $unityData 'MonoBleedingEdge\bin\mono.exe'
$compilerPath = Join-Path $unityData 'MonoBleedingEdge\lib\mono\4.5\csc.exe'
$outputPath = Join-Path $PSScriptRoot 'Logs'
New-Item -ItemType Directory -Path $outputPath -Force | Out-Null

function Invoke-CompilerCheck {
    param([string[]]$CompilerArguments)
    & $monoPath $compilerPath -nologo @CompilerArguments
    if ($LASTEXITCODE -ne 0) { throw "C# compiler failed with exit code $LASTEXITCODE" }
}

# These are API compilation checks outside the Editor, not Unity import or Play tests.
$engineReferences = Get-ChildItem -LiteralPath (Join-Path $unityData 'Managed\UnityEngine') -Filter '*.dll' |
    Where-Object { $_.Name -notlike 'UnityEditor*' } |
    ForEach-Object { '-r:' + $_.FullName }
$engineReferences += '-r:' + (Join-Path $unityData 'MonoBleedingEdge\lib\mono\4.5\Facades\netstandard.dll')
$runtimeSources = (Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets\EchoProtocol\Runtime') -Filter '*.cs' -Recurse).FullName
$runtimeAssembly = Join-Path $outputPath 'EchoProtocol.Runtime.dll'
Invoke-CompilerCheck (@('-target:library', '-define:UNITY_2021_3,UNITY_2021_3_OR_NEWER,ENABLE_LEGACY_INPUT_MANAGER', ('-out:' + $runtimeAssembly)) + $engineReferences + $runtimeSources)
Write-Output 'PASS: runtime C# compilation against installed Unity API assemblies'

$editorReferences = Get-ChildItem -LiteralPath (Join-Path $unityData 'Managed\UnityEngine') -Filter 'UnityEditor*.dll' |
    ForEach-Object { '-r:' + $_.FullName }
$editorReferences += '-r:' + $runtimeAssembly
$editorSources = (Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Assets\EchoProtocol\Editor') -Filter '*.cs').FullName
Invoke-CompilerCheck (@('-target:library', '-define:UNITY_EDITOR,UNITY_2021_3,UNITY_2021_3_OR_NEWER', ('-out:' + (Join-Path $outputPath 'EchoProtocol.Editor.dll'))) + $engineReferences + $editorReferences + $editorSources)
Write-Output 'PASS: editor C# compilation against installed Unity API assemblies'

$timelineExecutable = Join-Path $outputPath 'EchoTimelineChecks.exe'
Invoke-CompilerCheck @('-target:exe', ('-out:' + $timelineExecutable), (Join-Path $projectRoot 'Assets\EchoProtocol\Runtime\ReplayTimeline.cs'), (Join-Path $projectRoot 'Assets\EchoProtocol\Runtime\TimelineSelfChecks.cs'), (Join-Path $PSScriptRoot 'TimelineRunner.cs'), (Join-Path $PSScriptRoot 'TimelineRegressionChecks.cs'))
& $monoPath $timelineExecutable
if ($LASTEXITCODE -ne 0) { throw "Timeline checks failed with exit code $LASTEXITCODE" }
