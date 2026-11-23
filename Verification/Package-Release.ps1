$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$deliveryRoot = Join-Path $projectRoot 'Deliverables'
$buildRoot = Join-Path $projectRoot 'Builds\EchoProtocol'
if (-not (Test-Path -LiteralPath (Join-Path $buildRoot 'EchoProtocol.exe'))) { throw 'Build the Windows player first.' }
New-Item -ItemType Directory -Path $deliveryRoot -Force | Out-Null

$checksMatch = [regex]::Match((Get-Content (Join-Path $projectRoot 'Assets/EchoProtocol/Runtime/EchoGameChecks.cs') -Raw), 'ExpectedCheckCount = (\d+)')
if (-not $checksMatch.Success) { throw 'Cannot read the game check count.' }
$expectedChecks = $checksMatch.Groups[1].Value
$playGuide = @"
ECHO PROTOCOL — WINDOWS ÖRNEK BÖLÜMÜ

BAŞLATMA
1. ZIP'i tamamen bir klasöre çıkar.
2. EchoProtocol.exe dosyasını aç.
3. Menüde ÖRNEK BÖLÜMÜ OYNA'yı seç. ENTER eski üç sektörlü karşılaştırma demosunu açar.
Unity kurulumu gerekmez. EXE, EchoProtocol_Data, MonoBleedingEdge ve DLL dosyaları birlikte kalmalı.

KONTROLLER
WASD / fare: hareket / bakış. Shift: koş. Space: zıpla.
Sol tık: ateş. Sağ tık: nişan. E: nişan aldığın paneli kullan.
R: rotayı Echo olarak sakla ve yeni döngüyü başlat.
T: mevcut Echo kayıtlarını koru ve döngüyü yeniden başlat.
Q: son Echo kaydını sil; diğerini koru.
X: yalnızca bu alanın Echo kayıtlarını sil ve alanı sıfırla.
TAB: zaman durmuşken rota/olay planını incele. ESC: duraklat.
Ölümde ENTER: mevcut Echo planlarıyla yeniden dene. Alt+F4: çıkış.

OYUN
Bakım Hattı; servis geçidi, atış penceresi ve tahliye olmak üzere üç bağımsız
kontrol noktasından oluşur. Hareket, E etkileşimi ve ateş kaydedilir. Echo'ya
yön komutu vermezsin; kendi kaydını otomatik oynatır. En fazla iki Echo ve
20 saniyelik döngü vardır. Tamamlanan kontrol noktaları ve toplanan bellek
korunur; alanın yerel Echo planları sonraki kontrol noktasına taşınmaz.
Echo yuvaları doluyken R, değiştireceğin kaydı önceden seçtirir.
F6 otomatik test içindir ve çözüm akışını gösterebilir; ilk insan denemesinde kullanma.

8–12 dakika ilk oynayış için tasarım hedefidir; insan denemesinde henüz ölçülmedi.
"@
Set-Content -LiteralPath (Join-Path $buildRoot 'OYNA_TR.txt') -Value $playGuide -Encoding utf8
$windowsZip = Join-Path $deliveryRoot 'EchoProtocol_Windows.zip'
Compress-Archive -LiteralPath $buildRoot -DestinationPath $windowsZip -Force

# Keep the project-relative hierarchy and include only source and clean evidence.
# Library, Temp, UserSettings, raw logs, backups and binary builds are excluded.
Add-Type -AssemblyName System.IO.Compression.FileSystem
$sourceZip = Join-Path $deliveryRoot 'EchoProtocol_Unity_Source.zip'
$sourceFolders = @('Assets', 'Packages', 'ProjectSettings', 'Verification\Results')
$files = foreach ($folder in $sourceFolders) {
    Get-ChildItem -LiteralPath (Join-Path $projectRoot $folder) -Recurse -File | Where-Object { $_.Extension -ne '.log' }
}
$files += Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object { $_.Extension -in '.cs', '.ps1' }
$files += Get-ChildItem -LiteralPath $projectRoot -File | Where-Object { $_.Extension -eq '.md' -or $_.Name -eq '.gitignore' }
$stream = [System.IO.File]::Open($sourceZip, [System.IO.FileMode]::Create)
$archive = [System.IO.Compression.ZipArchive]::new($stream, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    foreach ($file in ($files | Sort-Object -Property FullName -Unique)) {
        $relative = $file.FullName.Substring($projectRoot.Length + 1).Replace('\', '/')
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, $relative,
            [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
} finally { $archive.Dispose(); $stream.Dispose() }
Get-Item -LiteralPath $windowsZip, $sourceZip | Select-Object Name, Length
