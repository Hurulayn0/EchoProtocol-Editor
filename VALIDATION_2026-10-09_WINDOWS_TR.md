# Echo Protocol — Windows ders demosu doğrulaması / 9 Ekim 2026

Mevcut üç bölümlük proje geliştirildi. Unity 2021.3.21f1 Personal, Built-in /
D3D11, Windows x64 / Mono kullanıldı. Kayıt çekirdeği korunur; bağımsız C#
derlemesi ile gerçek Unity/Windows çalıştırmaları ayrı kontrol edilir.

## Güncel sonuçlar

| Kontrol | Sonuç |
| --- | --- |
| Runtime ve editor C# API derlemesi | Geçti |
| Orijinal kayıt kontrolleri | 14/14 |
| Kayıt regresyonu | 25/25; 1.000 üretilmiş kayıt |
| Gerçek Unity Play Mode | 157/157 |
| Windows x64 build | Başarılı; 0 hata, 0 uyarı |
| Düzeltilmiş Windows EXE / 1280×720 | 157/157; çıkış 0; runtime hatası 0 |
| Düzeltilmiş Windows EXE / 800×600 | 157/157; çıkış 0; runtime hatası 0 |
| Düzeltilmiş Windows EXE / 1920×1080 | 157/157; çıkış 0; runtime hatası 0 |
| Kaynak ZIP / önbelleksiz Unity importu | 157/157; temiz ayrı klasörden |
| Arkadaş ZIP / ayrı klasörden Windows çalıştırma | 157/157; çıkış 0; runtime hatası 0 |
| Unity güvenlik düzeltmesi | Resmî Application Patcher 1.3.3; imza geçerli; DLL düzeltilmiş |
| Orijinal kayıt çekirdeği | ReplayTimeline.cs ve TimelineSelfChecks.cs orijinal ZIP ile aynı SHA256 |

Sonuçlar `Verification/Results/2026-10-09/WindowsDemo/` içindedir. Önceki
121 kontrolün sonuçları `VALIDATION_2026-10-08_TR.md` ve 8 Ekim sonuçlarında
korunur; bu sürüm toplam 157 kontrol içerir.

## 36 yeni canlı kontrol

- Kayıt edilen tüm kare/eylemlerin saklanması; JSON dönüşümünden sonra hareket,
  dönüş, bakış ve sürenin korunması. Gerçek UTF8 dosyaya yazma/okuma.
- Ana menüde DEVAM işlemiyle yüklenen Echo’nun dünyayı yeni döngüden başlatması,
  konsolu açması, son pozda plakayı tutması ve fiziksel kapıyı açması.
- Eksik/bozuk/çok büyük kayıt; geçerli yedeğe dönüş; bozuk dosya onarılırken iyi
  yedeğin korunması. Bilinmeyen sürüm/bölüm, üç Echo, sırasız zamanlar, NaN konum,
  geçersiz dönüş, yanlış konsol, kayıt sonrasındaki eylem ve tutarsız sayaçlar.
- Ölümde immutable Echo çözümünün korunması ve tekrar çalışması. Açık sektör
  sıfırlamasının hem canlı hem saklanan kayıtları temizlemesi.
- Tab ile haritanın açılıp kapanması; hareket, muhafızlar, zaman, ateş ve kayıt
  komutlarının durması; Escape/ana menü geçişlerinde gizli donmanın kalmaması.
- Harita çizgisi uçlarının dört çözünürlükte gerçek plaka işaretleriyle çakışması.
- Bölüm kilidi ve süre/kayıt rekorlarının gerçek PlayerPrefs ile saklanması;
  yalnızca açılmış bölümün seçilebilmesi ve kişisel rekorların iyileştirilmesi.
- JSON’dan yüklenen iki Echo’nun altı ateş olayını ve iki konsol olayını
  tekrarlaması, iki plakayı tutması ve gerçek CharacterController çıkışıyla
  son bölümün kazanma ekranına ulaşması.

Önceki 121 kontrol de korunur: kayıt/interpolasyon, konsol raycast’i, siper,
ateş/cooldown/ölüm, iki Echo sınırı, hareket/zıplama/koşu/nişan, menüler/ayarlar,
odak kaybı, üç bölümün kontrol girdileriyle tamamlanması, robot/ortam assetleri,
45 sektör yenilemesi ve 180 kayıt/reset sonrasında sınırlı nesne/materyal sayısı.

F6, komut satırı ve editör aynı test girişini kullanır. Her başarılı koşuda
157 ayrı kontrol, tek başarı sonucu ve çıkış kodu 0 gerekir. Exception, eksik
kontrol ve siyah framebuffer başarılı sayılmaz. Testler gerçek oyuncunun kayıt
ve profilini değiştirmez; geçici dosyalar ve ayrı test anahtarları temizlenir.

## Bulunan ve düzeltilen sorunlar

- Unity proje modüllerinde JSON kapalıydı; API derlemesi geçse bile gerçek
  Unity importu başarısızdı. Yerleşik `com.unity.modules.jsonserialize` eklendi.
- Bozuk kayıt doğrulamasının InvalidDataException sonucu ayrıca ele alındı;
  geçersiz dosya oyunu durdurmak yerine güvenli başlangıç/yedeğe dönüş sağlar.
- Bozuk ana dosya onarılırken sağlam yedeğin üzerine bozuk dosya yazılması
  engellendi; dosya yazımı geçici dosya ve File.Replace üzerinden yapılır.
- Haritada aynı plakayı tutan Echo ile plaka etiketleri ayrıldı.
- Küçük çözünürlükte GUI pivot dönüşümü rota çizgilerini kaydırıyordu. Çizgiler
  arayüzün ölçek/yer değiştirme matrisinden türetilir; dört oran için regresyon
  kontrolü ve gerçek Windows görüntüsü eklendi.

## Görsel ve performans kanıtı

Windows pencereleri görünür halde çalıştırılır. Menü, kontroller, ayarlar,
üç ortam, savaş, ölüm, çift Echo, geri yükleme sonrası final ve boş/çift Echo’lu
planlama haritası yakalanır. Piksel doluluğu ve framebuffer boyutları kontrol
edilir; harita etiketleri/rotaları görsel olarak ayrıca incelenir.

120 karelik yerel örnekte RTX 3060 Laptop GPU üzerinde Windows medyan/p95
kayıtları yaklaşık 6,9 / 6,9 ms, Unity tahsisleri yaklaşık 41 MiB idi.
Bu değerler kısa yerel ölçümdür; toplam Windows süreç RAM’i veya başka
bilgisayarlarda performans garantisi değildir. Ayrıntılı ölçümler sonuç dosyalarındadır.

Otomatik testler ortak ProcessControls/EchoPlayer.Tick yoluna kontrol kareleri
verir ve gerçek raycast, fizik, yapay zekâ ve UI durumlarını çalıştırır.
İşletim sisteminden klavye/fareyle uzun bir insan oynama seansı yapılmış gibi
sunulmaz. Anlaşılırlık, zorluk ve tekrar hissi için arkadaş denemesi gerekir.

## Güvenlik ve paket

Dağıtılan UnityPlayer.dll, resmî araçla düzeltilmiştir. Son doğrulamadaki kod
183, aracın düzeltmenin zaten uygulanmış olduğunu doğrulayan sonucudur.
Düzeltilmiş SHA256:
`00d986fa021b98163dcd6134e0c274f489c6c893a4db6a3fee05c776713e33e2`.
Kurulu editörün sürümü değişmedi; yeni build’lerde `Patch-Windows.ps1` tekrar
çalışır. Kaynak ZIP, Unity’nin derlenmiş player DLL’i yerine proje kaynaklarını içerir.

Kaynak: [Unity güvenlik duyurusu](https://unity.com/security/sept-2025-01),
[resmî yama aracı](https://discussions.unity.com/t/cve-2025-59489-patcher-tool/1688032).

Paketler: `EchoProtocol_Windows.zip`, `EchoProtocol_Unity_Source.zip`,
`EchoProtocol_Arkadas_Test_2026-10-09.zip`. Arkadaş paketindeki oyun dosyaları
standart Windows paketiyle byte düzeyinde karşılaştırılır; rehber ve CC0 lisansları
eklenir. 8 Ekim arkadaş paketi geçmiş sürüm olarak bırakıldı; paylaşım için
9 Ekim paketi kullanılmalıdır. SHA256 listeleri Deliverables klasöründedir.

## Tekrar çalıştırma

1. `Verification/Validate-Release.ps1 -ResultName WindowsDemo`
2. `Verification/Package-Release.ps1`
3. Windows/kaynak ZIP SHA256 değerleriyle `Deliverables/RELEASE_SHA256.txt` yaz.
4. `Verification/Package-Playtest.ps1 -ReleaseDate 2026-10-09`
5. `Verification/Verify-Packages.ps1 -ReleaseDate 2026-10-09`
