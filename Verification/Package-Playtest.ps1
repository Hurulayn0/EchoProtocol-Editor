param([string]$ReleaseDate = '2026-10-09', [string]$PackageName = '', [string]$ReleaseLabel = 'PLAYTEST')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$deliveryRoot = Join-Path $projectRoot 'Deliverables'
$sourceZip = Join-Path $deliveryRoot 'EchoProtocol_Windows.zip'
$releaseId = 'EP-' + $ReleaseDate.Replace('-', '') + '-' + $ReleaseLabel
$zipName = if ($PackageName) { $PackageName } else { 'EchoProtocol_Arkadas_Test_' + $ReleaseDate + '.zip' }
if ([System.IO.Path]::GetFileName($zipName) -ne $zipName -or -not $zipName.EndsWith('.zip')) { throw 'PackageName must be a ZIP filename without a directory.' }
$targetZip = Join-Path $deliveryRoot $zipName
$checksMatch = [regex]::Match((Get-Content (Join-Path $projectRoot 'Assets/EchoProtocol/Runtime/EchoGameChecks.cs') -Raw), 'ExpectedCheckCount = (\d+)')
if (-not $checksMatch.Success) { throw 'Cannot read the expected game check count.' }
$expectedChecks = $checksMatch.Groups[1].Value
$releaseHashes = Get-Content (Join-Path $deliveryRoot 'RELEASE_SHA256.txt')
$sourceHashLine = $releaseHashes | Where-Object { $_ -match '  EchoProtocol_Windows\.zip$' }
if (-not $sourceHashLine -or (Get-FileHash $sourceZip -Algorithm SHA256).Hash.ToLowerInvariant() -ne $sourceHashLine.Substring(0, 64)) {
    throw 'The Windows ZIP does not match the verified release checksum.'
}

$startGuide = @"
ECHO PROTOCOL — ARKADAŞ DENEMESİ
Sürüm kimliği: $releaseId
Paket tarihi: $ReleaseDate
Platform: Windows 64 bit

BAŞLATMA
1. ZIP'e sağ tıkla ve TÜMÜNÜ AYIKLA ile tamamını bir klasöre çıkar.
2. Çıkardığın klasörde EchoProtocol klasörüne gir.
3. EchoProtocol.exe dosyasını aç.
4. Menüde ÖRNEK BÖLÜMÜ OYNA'yı seç. KONTROLLER ve AYARLAR menülerini kullanabilirsin.

Unity kurulumu gerekmez. Oyunu ZIP'in içinden çalıştırma; tüm dosyalar
çıkarılmış olmalı. EXE, EchoProtocol_Data, MonoBleedingEdge ve DLL dosyaları
birlikte kalmalı. Ek bir kurulum programı yoktur.

KONTROLLER
WASD / fare: hareket / bakış
Shift: koş     Space: zıpla
Sol tık: ateş  Sağ tık: nişan al
E: yakındaki ve nişan aldığın terminali / bellek kasasını kullan
R: rotanı Echo olarak kaydet ve yeni döngüyü başlat
Q: son Echo kaydını sil; varsa diğerini koru
X: yalnızca bu alanın Echo kayıtlarını sil ve alanı sıfırla
Tab: rota haritası; zaman durur
Escape: duraklat / devam et
Enter: başlat, bölüm sonunda devam et, ölümde yeniden dene
F6: otomatik test     Alt+F4: çıkış

AMAÇ
Bakım Hattı'nın tahliye yolunu aç. Örnek bölüm servis geçidi, atış penceresi ve
tahliye olmak üzere üç bağımsız kontrol noktasıdır. Tamamlanan noktalar korunur;
yerel Echo planları sonraki odaya taşınmaz. Hareketin, E etkileşimlerin ve
ateşin kaydedilir; Echo kendi rotasını otomatik oynatır. En fazla iki Echo,
20 saniyelik döngü vardır. İki Echo varken R'ye basınca değiştirilecek Echo'yu
seçersin; diğer kayıt korunur. Döngü süresi dolması mevcut kayıtları silmez.
Bellek gözlemi isteğe bağlıdır ve ana menüde arşivlenir.

Önce kendin oyna; sonra DENEME_REHBERI.txt ile kısa kontrolleri yap.
Sonuçlarını GERI_BILDIRIM.txt şablonuna yazıp oyunu gönderen arkadaşına ilet.
Kaynak model ve lisans bilgileri Lisanslar klasöründedir.
"@

$testGuide = @"
ECHO PROTOCOL — DENEME REHBERİ
Sürüm: $releaseId

En fazla 20 dakika ayır. 8–12 dakika ilk oynayış için tasarım hedefidir;
insan denemesinde henüz ölçülmedi. İlk denemede hedef metinlerinin kendi
planını kurmana alan bırakıp bırakmadığını fark et.

OYUNCU DENEYİMİ
- Konsol, plaka ve Echo arasındaki ilişkiyi ne zaman anladın?
- Hangi bölümde veya işlemde takıldın? Hedef işaretleri yardımcı oldu mu?
- Hareket, koşu, nişan ve ateş rahat mı? Fare/ses/görüş ayarlarını deneyebilirsin.
- Üç ortam yeterince farklı mı? Sıkıldığın veya tekrar hissettiğin yer var mı?
- Muhafızın kırmızı lazeri saldırıyı fark etmek ve sipere geçmek için yeterli mi?
- Rezonatör ve bellek kasası görevini anladın mı? Altı saniyelik pencere uygun mu?
- Muhafızın üst zayıf noktasını ve rezonansla durmasını fark ettin mi?

KISA İŞLEV KONTROLÜ
1. Konsolu E ile aç; plakada kalıp R ile kaydet. Echo konsolu tekrar açıp
   plakada beklerken sen kapıdan geçebiliyor musun?
2. Q ile son Echo'yu geri al. İki Echo varsa öncekinin kaldığını kontrol et.
3. İki Echo varken 20 saniyelik sürenin bitmesini bekle; ikisi korunuyor mu?
4. Escape ile duraklatıp ayarlara gir, ardından oyuna dön.
5. Muhafızlı bölümde ölürsen ENTER ile yeniden dene.
6. Tab ile haritayı aç; zamanın durduğunu ve rotaların görünmesini kontrol et.
7. Bir Echo kaydet, oyunu kapatıp yeniden aç. DEVAM ile Echo geri geliyor mu?
8. Ana menüde açılmış bir sektörü tekrar oyna.
9. Son sektörde iki plakanın da Echo ile tutulmasını ve final ekranını dene.
10. Plakaları Echo'lara devret, rezonatörü vur ve altı saniye içinde bellek
    kasasını E ile aç. Yeni döngüde kaydın korunduğunu kontrol et.
11. Üç kaydı bulabilirsen ana menüde BELLEK ARŞİVİ'ni oku.

ECHO İPUCU
Sıfırlama muhafızları geri getirir. Ateş kaydı da tekrar oynar; Echo'nun
ateş hattının önüne geçersen atışı engelleyebilirsin. Aynı rotayı yeniden
yürümek yerine görev ataması tamamlanmış Echo'nun ilerlemesini bekleyebilirsin.

OTOMATİK KONTROL (İSTEĞE BAĞLI)
Normal oynama denemeni yaptıktan sonra F6'ya basabilirsin. Test kendi
senaryosunu oynatır ve ana menüye döner. Beklenen sonuç:
PASS: $expectedChecks/$expectedChecks Unity game checks
Testi başlatınca mevcut deneme sona erer; ana menüden tekrar başlayabilirsin.

HATA OLURSA
Sektörü, yaptığın işlemi ve gördüğün sonucu yaz. Mümkünse ekran görüntüsü
veya kısa video ekle. Tekrar denediğinde aynı sorunun oluşup oluşmadığını belirt.

Oyun açılmıyorsa ZIP'i tamamen çıkardığını ve klasördeki dosyaların birlikte
olduğunu kontrol et. Görünen hata metnini gönder. Varsa Unity'nin Player.log
dosyasını da geri bildirime ekleyebilirsin. Windows+R ile şu klasörü aç:
%USERPROFILE%\AppData\LocalLow\IKU Student Project\Echo Protocol

Bu paketin önceki doğrulaması: Unity ve Windows'ta $expectedChecks/$expectedChecks;
1280×720, 800×600 ve 1920×1080. Arkadaş testi oynanabilirlik ve farklı
bilgisayarlardaki deneyim için yapılır.
"@

$feedback = @"
ECHO PROTOCOL — GERİ BİLDİRİM
Sürüm: $releaseId

İsim / takma ad:
Windows sürümü:
Ekran kartı (biliyorsan):
Oynadığın çözünürlük:
Yaklaşık oynama süresi:
Ulaştığın / tamamladığın sektör:

1. Echo mantığını ne kadar kolay anladın? (1: zor — 5: çok kolay)
Puan:
Açıklama:

2. Oynamak ne kadar keyifliydi? (1: sıkıcı — 5: çok keyifli)
Puan:
Açıklama:

3. Tekrar hissettiğin veya sıkıldığın bölüm/işlem:

4. Ortamlar, robotlar ve silah görünümü hakkında görüşün:

5. Hareket, nişan, ateş ve arayüz hakkında görüşün:

6. En iyi bulduğun şey:

7. Önce değiştirmemizi istediğin şey:

HATA BİLDİRİMİ (varsa her hata için bu kısmı çoğalt)
Sektör:
Hata öncesinde sırayla yaptığın işlemler:
Beklediğin sonuç:
Gerçekte olan:
Aynı adımlarla tekrar oluşuyor mu?:
Ekran görüntüsü / video / Player.log (varsa):

F6 otomatik test sonucu (çalıştırdıysan):
"@

if ($ReleaseLabel -like 'SAMPLE*') {
    $startGuide = @"
ECHO PROTOCOL — BAKIM HATTI / EŞZAMANLILIK
Sürüm: $releaseId / Windows x64

ZIP'i tamamen çıkar. EchoProtocol/EchoProtocol.exe dosyasını aç.
Ana menüde ÖRNEK BÖLÜMÜ OYNA'yı seç. ENTER eski karşılaştırma demosunu açar.
Unity kurulumu gerekmez. EXE, Data, MonoBleedingEdge ve DLL'ler birlikte kalmalı.

WASD / fare: hareket / bakış. Shift: koş. Space: zıpla.
E: nişan aldığın terminal. Sol tık: ateş. Sağ tık: nişan.
R: hareket, etkileşim ve atışını Echo'ya kaydet; yeni döngü.
Q: son Echo kaydını sil. X: bu kontrol noktasının kayıtlarını temizle.
TAB: zaman durmuşken planı ve olayları incele. ESC: duraklat.
Ölümde ENTER: aynı Echo'larla yeniden dene. Alt+F4: çıkış.

Bakım hattının tahliye yolunu aç. En fazla iki Echo, 20 saniyelik döngü.
Çözülmüş bağımsız kontrol noktaları korunur; yerel Echo'lar yeni odaya taşınmaz.
Echo’ya yön komutu verilmez; önce sen rotayı yaparsın, R sonrası Echo otomatik tekrarlar.
İki kayıt doluyken R hangi Echo’nun değişeceğini sorar; diğerini korur.
T: mevcut Echo kayıtlarını koruyarak döngüyü yeniden başlat.
Echo atışından önce renkli hat ve geri sayım görünür; önizleme ateş etmez.
Finalde muhafızın güncel hedefini oku. Farklı aktörlerin kısa aralıklı isabeti
eşzamanlı vuruş bonusu verir. Yeni tuş yok; normal ateş ederek eşlik edersin.
Boş yuva varken süre dolunca oyun kaydı saklayıp saklamayacağını sorar.
DEVAM mevcut kontrol noktasını tamamlanmış kayıtlarla yeni döngüden yükler.
Bellek görevi isteğe bağlıdır. Toplanan kayıt ana menüde BELLEK ARŞİVİ'ndedir.

İlk oynayışta F6 kullanma: otomatik test çözümü gösterebilir.
Önce kendin dene; sonra GERI_BILDIRIM.txt formunu doldur.
3–5 kişi için gözlemci yönergesi DENEME_REHBERI.txt içindedir.
8–12 dakika tasarım hedefidir; insan denemesinde ölçülmüş süre değildir.
F6 teknik doğrulaması: $expectedChecks kontrol. Teknik geçiş, insan değerlendirmesi değildir.
AYARLAR'da ses, fare ve görüş açısını düzenleyebilirsin.
Paket Unity'nin resmî Windows güvenlik düzeltmesini içerir; lisanslar eklenmiştir.
"@
    $testGuide = "Sürüm: $releaseId`r`n`r`n" + (Get-Content (Join-Path $projectRoot 'INSAN_DENEMESI_TR.md') -Raw)
    $humanText = Get-Content (Join-Path $projectRoot 'INSAN_DENEMESI_TR.md') -Raw
    $feedback = "Sürüm: $releaseId`r`n`r`n" + [regex]::Match($humanText, '(?s)```text\r?\n(.*?)```').Groups[1].Value
}

$assetCredits = @'
ECHO PROTOCOL — ASSET KAYNAKLARI
Modüler ortamlar: Quaternius / Modular Sci-Fi MegaKit / Standard
https://quaternius.com/packs/modularscifimegakit.html
Robot, silah ve dekor: Quaternius / Sci-Fi Essentials Kit / Standard
https://quaternius.com/packs/scifiessentialskit.html
Paketlerdeki özgün CC0 lisans metinleri bu klasördedir.
Oyunun kısa sesleri kodla üretilir. Ortam düzenleri mevcut Echo Protocol
projesinde modüler parçalarla kuruldu.
'@

Add-Type -AssemblyName System.IO.Compression.FileSystem
$encoding = [System.Text.UTF8Encoding]::new($true)
$inputArchive = [System.IO.Compression.ZipFile]::OpenRead($sourceZip)
$outputFile = [System.IO.File]::Open($targetZip, [System.IO.FileMode]::Create)
$outputArchive = [System.IO.Compression.ZipArchive]::new($outputFile, [System.IO.Compression.ZipArchiveMode]::Create)
function Add-TextEntry([string]$Name, [string]$Contents) {
    $entry = $outputArchive.CreateEntry($Name, [System.IO.Compression.CompressionLevel]::Optimal)
    $stream = $entry.Open()
    try { $bytes = $encoding.GetPreamble() + $encoding.GetBytes($Contents); $stream.Write($bytes, 0, $bytes.Length) }
    finally { $stream.Dispose() }
}
try {
    foreach ($original in $inputArchive.Entries) {
        if ($original.FullName.EndsWith('/')) { continue }
        if (-not $original.FullName.StartsWith('EchoProtocol/') -or $original.FullName.Contains('..')) { throw 'Unexpected release archive path.' }
        $entry = $outputArchive.CreateEntry($original.FullName, [System.IO.Compression.CompressionLevel]::Optimal)
        $entry.LastWriteTime = $original.LastWriteTime
        $sourceStream = $original.Open(); $targetStream = $entry.Open()
        try { $sourceStream.CopyTo($targetStream) } finally { $sourceStream.Dispose(); $targetStream.Dispose() }
    }
    Add-TextEntry 'BASLA_BURADAN.txt' $startGuide
    Add-TextEntry 'DENEME_REHBERI.txt' $testGuide
    Add-TextEntry 'GERI_BILDIRIM.txt' $feedback
    Add-TextEntry 'Lisanslar/ASSET_KAYNAKLARI.txt' $assetCredits
    foreach ($license in @('LICENSE_ModularSciFi.txt', 'LICENSE_Essentials.txt')) {
        Add-TextEntry ('Lisanslar/' + $license) (Get-Content (Join-Path $projectRoot ('Assets/ThirdParty/Quaternius/' + $license)) -Raw)
    }
} finally { $outputArchive.Dispose(); $outputFile.Dispose(); $inputArchive.Dispose() }

$sendMessage = @"
Arkadaşlara göndermek için hazırlanmış mesaj:

Echo Protocol dönem projemizin şu anki sürümünü denemenizi istiyorum.
Windows 64 bit için; Unity kurmanıza gerek yok. ZIP'i tamamen çıkarıp
EchoProtocol klasöründeki EchoProtocol.exe'yi açın. 15–20 dakika denemeniz
yeterli; mümkünse üç sektörü tamamlayın. Özellikle Echo mantığının anlaşılırlığı,
tekrar hissi, oynanış ve ortamlar hakkındaki görüşlerinizi bekliyorum.
Paketin içindeki GERI_BILDIRIM.txt şablonunu doldurup bana atabilirsiniz.
Hata olursa hangi bölümde ve hangi adımlarla oluştuğunu, varsa görüntüsünü ekleyin.

Eklenecek dosya: $zipName
Sürüm: $releaseId
"@
Set-Content (Join-Path $deliveryRoot 'ARKADASLARA_MESAJ.txt') $sendMessage -Encoding utf8
if ($ReleaseLabel -like 'SAMPLE*') {
    $sendMessage = "Echo Protocol'ün yeni Bakım Hattı örnek bölümünü deneyebilir misin? ZIP'i tamamen çıkarıp EchoProtocol/EchoProtocol.exe'yi aç; ÖRNEK BÖLÜMÜ OYNA seçeneğini kullan. İlk denemede F6 çalıştırma. Mümkünse sesli düşünerek oyna ve GERI_BILDIRIM.txt formunu doldur. 20 dakikada bitmezse nerede takıldığını yaz. Unity gerekmez. Sürüm: $releaseId. Dosya: $zipName"
    Set-Content (Join-Path $deliveryRoot 'ARKADASLARA_MESAJ.txt') $sendMessage -Encoding utf8
}
$hash = (Get-FileHash $targetZip -Algorithm SHA256).Hash.ToLowerInvariant()
Set-Content (Join-Path $deliveryRoot ($zipName + '.sha256.txt')) ($hash + '  ' + $zipName) -Encoding utf8
Get-Item $targetZip | Select-Object Name, Length
