# Echo Protocol — rezonans ve atmosfer doğrulaması / 9 Ekim 2026

Unity 2021.3.21f1 Personal, Built-in / D3D11, Windows x64 / Mono.
Mevcut proje geliştirildi; kayıt çekirdeği korunur. Önceki 157 kontrollü sürümün
raporu VALIDATION_2026-10-09_WINDOWS_TR.md içindedir.

## Sonuçlar

| Kontrol | Sonuç |
| --- | --- |
| Runtime ve editor C# API derlemesi | Geçti |
| Orijinal kayıt kontrolleri | 14/14 |
| Kayıt regresyonu | 25/25; 1.000 üretilmiş kayıt |
| Gerçek Unity Play Mode | 202/202 |
| Windows x64 build | Başarılı; 0 hata, 0 uyarı |
| Windows EXE / 1280×720 | 202/202; çıkış 0; runtime hatası 0 |
| Windows EXE / 800×600 | 202/202; çıkış 0; runtime hatası 0 |
| Windows EXE / 1920×1080 | 202/202; çıkış 0; runtime hatası 0 |
| Kaynak ZIP / önbelleksiz Unity importu | 202/202; geçti |
| Yeni paylaşım ZIP / ayrı klasörden çalıştırma | 202/202; geçti |
| Unity güvenlik düzeltmesi | Resmî Application Patcher 1.3.3; geçerli Unity imzası; düzeltilmiş DLL |
| Orijinal kayıt çekirdeği | ReplayTimeline.cs ve TimelineSelfChecks.cs orijinal SHA256 ile aynı |

Kanıtlar `Verification/Results/2026-10-09/Immersion/` içindedir.

## 45 yeni canlı kontrol

- Kaynaklardan yüklenen kamera/şeffaf efekt shader'larının gerçek çalıştırmada
  desteklenmesi; parçacık, rezonatör ve kasa collider'larının oluşturulması.
- Beş efekt klibi, dört paylaşılan ses kaynağı ve ortam klibinin bir kez kurulması.
- Gerçek üst raycast isabetiyle 50 hasar, 0,45 saniye saldırı kesme; kaydedilip
  geri yüklenen Echo'nun iki zayıf nokta atışıyla muhafızı düşürmesi.
- Reset'in muhafız canı, sersemleme, kıvılcım listesi ve silah salınımını temizlemesi.
- Gerçek rezonatör isabetiyle altı saniyelik erişim, iki saniye sersemletme ve 20
  iyileştirme; darbenin aynı döngüde birikmemesi, süresinin dolması ve reset'i.
- Tab'ın rezonans sayacını ve muhafız sersemlemesini de dondurması.
- JSON dönüşümünden sonra Echo'nun aynı rezonatörü tetiklemesi; ikinci atışın
  harcanmış rezonatörü tekrar çalıştıramaması.
- Üç sektörün her birinde kasanın Echo ve rezonans olmadan kilitli kalması;
  gerçek kontrol yolu üzerinden çözüm kaydeden bir/iki Echo'nun plakaları tutması.
- Spawn'dan gerçek CharacterController koşusuyla, altı saniyelik pencere içinde
  kasaya gidilmesi; terminal raycast'iyle toplama ve Archive olayının kaydı.
- Tekrar okumanın ek olay/toplama üretmemesi; yeni olayın dosya şemasını geçmesi;
  belleklerin resetten sonra korunması ve üç kayıtla hikâyenin tamamlanması.
- Arşiv menüsünden Escape; ayrı PlayerPrefs alanında belleklerin gerçek saklanması
  ve geçersiz bitlerin üç sektör sınırına indirgenmesi.

Önceki 157 kontrolün tamamı korunur: konsol/plaka/kapı, gerçek Echo ateşi, siper,
hareket, zıplama, koşu, nişan, ayarlar, odak kaybı, menüler, ölüm, rekorlar,
bozuk kayıt/yedek, üç sektörün çıkışı, kayıtlı çift Echo ile final, harita
ölçekleri ve 45 sektör / 180 kayıt-reset yük kontrolü.

## Bulunan ve düzeltilen sorunlar

- ParticleSystem bileşeni eklenince hemen oynamaya başladığı için duration ayarı
  native Unity hatası üretiyordu. Önce StopEmittingAndClear, modül/renderer
  kurulumu, ardından Play/Emit sırası kullanıldı.
- Eski görsel kontrol her MeshRenderer'da _MainTex varsayıyordu. Yeni cam shader'ı
  farklı olduğundan özellik okumadan önce HasProperty kontrolü eklendi.
- Plaka görseli modüler zeminin altında kalıyordu; üst yüzey yüksekliği düzeltildi.
- Yeni P/V harita işaretlerinin açıklamaları ve oyun içi kontroller güncellendi.

## Gerçek görüntüler ve ölçüm

Üç çözünürlükte gerçek Windows framebuffer boyutları ve dolu pikseller kontrol
edilir. Yeni gözlem/reaktör ortamları, üç kasa, arşiv, P/V haritası; eski menü,
ayar, savaş, ölüm, çift Echo ve geri yükleme sonrası final görüntüleri yakalanır.
Görüntüler ayrıca görsel okunabilirlik için incelenir.

120 karelik kısa yerel final-sektör ölçümleri (RTX 3060 Laptop GPU):

- 1280x720: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=43MiB`
- 1280x720: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=43MiB`
- 1280x720: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=43MiB`
- 800x600: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=44MiB`
- 800x600: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=44MiB`
- 800x600: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=44MiB`
- 1920x1080: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=43MiB`
- 1920x1080: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=43MiB`
- 1920x1080: `ECHO BENCHMARK (120 frames, final sector, 2 Echoes): median=6.9ms p95=6.9ms Unity allocated=43MiB`

Bunlar yerel örneklerdir; başka bilgisayarlar için performans garantisi değildir.
Unity tahsisleri, toplam Windows süreç RAM ölçümü değildir.

Testler gerçek ProcessControls/EchoPlayer.Tick, fizik, raycast, AI, kayıt ve
arayüz kodunu çalıştıran sentetik girişler kullanır. İşletim sisteminden uzun
bir insan klavye/fare oynama seansı yapılmış gibi sunulmaz. Anlaşılırlık, zorluk
ve tekrar hissi arkadaşların oynayarak değerlendireceği konulardır.
F6 gerçek kayıt/profili değiştirmez; testler ayrı dosya ve PlayerPrefs kullanır.

## Dağıtım ve uyumluluk

Yeni paylaşım dosyası `EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip`.
Eski 8/9 Ekim arkadaş ZIP'leri korunur. Windows ve paylaşım paketinin oyun
dosyaları SHA256 ile eşleştirilir; son içerik doğrulaması ayrı kanıtta tutulur.
Kaynak ZIP Library/Temp, ham log, build, yama aracı ve yedekleri içermez.

Yeni Archive eylemi mevcut şemaya doğrulanmış bir tür olarak eklendi. Önceki
konsol/ateş kayıtları yüklenir. Yeni Archive olayı içeren kaydı eski EXE ile
açmak desteklenmez; eski EXE bilinmeyen türü güvenle reddeder. Bu sürümde yeni
kayıt ve yüklenen çift Echo'nun final çıkışı yeniden test edilir.

UnityPlayer.dll düzeltilmiş SHA256:
`00d986fa021b98163dcd6134e0c274f489c6c893a4db6a3fee05c776713e33e2`.
Resmî araç bu koşuda 183 koduyla zaten uygulanmış düzeltmeyi doğruladı.
Kurulu editör sürümü değişmedi; her yeni build sonrası yama tekrar uygulanır.
Kaynak: [Unity duyurusu](https://unity.com/security/sept-2025-01),
[resmî araç](https://discussions.unity.com/t/cve-2025-59489-patcher-tool/1688032).

## Tekrar çalıştırma

1. `Verification/Validate-Release.ps1 -ResultName Immersion`
2. `Verification/Package-Release.ps1`
3. Windows/kaynak ZIP SHA256 değerleriyle `Deliverables/RELEASE_SHA256.txt` yaz.
4. `Verification/Package-Playtest.ps1 -ReleaseDate 2026-10-09 -PackageName EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip -ReleaseLabel IMMERSION`
5. `Verification/Verify-Packages.ps1 -ReleaseDate 2026-10-09 -ResultName Immersion -PackageName EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip`
