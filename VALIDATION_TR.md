# Echo Protocol — Bakım Hattı örnek bölüm doğrulaması / 10 Ekim 2026

Güncel yönlendirme/arayüz açıklığı ve 26 Ekim doğrulamaları:
[GAMEPLAY_CLARITY_2026-10-26_TR.md](GAMEPLAY_CLARITY_2026-10-26_TR.md).

En güncel görsel sanat geçişi ve 26 Ekim koşusu:
[VISUALS_2026-10-26_TR.md](VISUALS_2026-10-26_TR.md). Bir önceki yön bulma
iyileştirmesi: [VALIDATION_2026-10-26_WAYFINDING_TR.md](VALIDATION_2026-10-26_WAYFINDING_TR.md).

Güncel ortak çatışma sürümü ve gerçek kontrolleri:
[VALIDATION_2026-10-10_TACTICS_TR.md](VALIDATION_2026-10-10_TACTICS_TR.md).
Önceki sürümlerin sonuçları aşağıda tarihsel olarak korunmuştur.

## Echo kontrolü güncellemesi

Bu dosyanın aşağıdaki sonuçları `maintenance-sample-20261010` etiketli ilk
örnek bölüme aittir. Güncel kullanım düzeltmesi, ayrı raporda tutulur:
[VALIDATION_2026-10-10_ECHO_CONTROLS_TR.md](VALIDATION_2026-10-10_ECHO_CONTROLS_TR.md).
Yeni öğretim, T ile aynı planı deneme, seçili Echo değiştirme ve salt okuma rota
önizlemesi için [ECHO_KULLANIM_TR.md](ECHO_KULLANIM_TR.md) belgesini kullan.

## İlk örnek bölümün korunmuş doğrulaması

Unity 2021.3.21f1, Built-in / D3D11, Windows x64 / Mono.
Önceki çalışan sürüm Git commit e9ac584e3e5d401d8bcf64346508d90086ef2857 ve
verified-demo-20261009 etiketi ile değişikliklerden önce korunmuştur.
Önceki 202 kontrollü sürüm raporu VALIDATION_2026-10-09_IMMERSION_TR.md'dedir.

## Teknik sonuçlar

| Kontrol | Sonuç |
| --- | --- |
| Runtime ve editor C# / kurulu Unity API | Geçti |
| Orijinal kayıt kontrolleri | 14/14 |
| Kayıt regresyonu | 25/25; 1.000 üretilmiş kayıt |
| Gerçek Unity Play Mode / aynı F6 yolu | 253/253 |
| Windows x64 build | Başarılı; 0 hata, 0 uyarı |
| Windows 1280×720 / 800×600 / 1920×1080 | 253/253 her çözünürlükte; çıkış 0; runtime hatası 0 |
| Kaynak ZIP / temiz Unity importu | 253/253; geçti |
| Paylaşım ZIP / ayrı klasörden Windows | 253/253; çıkış 0; runtime hatası 0 |
| Unity resmî Application Patcher 1.3.3 | Geçerli Unity imzası; düzeltilmiş DLL korunur |
| ReplayTimeline.cs / TimelineSelfChecks.cs | Orijinal SHA256 ile aynı |

Kanıt dizini: Verification/Results/2026-10-10/MaintenanceSample/.
Kanıtlar son görsel düzeltmeleri içeren çalıştırmaya aittir.

## Uygulanan örnek bölüm

Bakım Hattı / Eşzamanlılık tek bir bölüm, üç bağımsız kontrol noktasıdır.
Eski üç sektör karşılaştırma için korunur; ana menünün önerilen düğmesi yeni
örnek bölümü açar. İki Echo / 20 saniye / tek silah / aynı muhafız kullanılır.

- Servis geçidi: kaydedilmiş uzak E işlemi 1,4 saniyelik kapıyı açar; oyuncu
  başka rotadan geçer. Güvenli öğrenme; muhafız yok. Kapalı kapıya yaklaşmak
  açmaz; açık eşikteki oyuncu süre dolunca içeride kilitlenmez.
- Atış penceresi: Echo sensörde lensi açık tutar, sonra rampayla üst röleye gider.
  Başka Echo kayıtlı atışıyla rezonatörü tetikler, alt röleye ilerler. Başta
  enerjisiz kalan E niyeti kaydedilir; yeni döngüde enerji koşuluyla uygulanır.
  Dar açıklık ve yan muhafaza gerçek raycast ile erken/yan atışı engeller.
- Tahliye: yem Echo gerçek muhafız saldırısını üzerine çeker, ardından sol
  servisi açar. Operatör Echo doğru zamanda rezonans atışı ve üst terminali
  uygular. Gerçek oyuncu başka yolu kullanarak çıkışa veya gözlem belleğine gider.
  Muhafızı öldürmeden final doğrulandı; doğrudan çatışma da alternatif çözüm.
- Gözlem 04: muhafızın Echo'ya ateş etmesini gözlemle, rezonans penceresinde
  sol üst terminali oku. Kalıcı bellek ana menü arşivinde yeniden okunabilir.
  Ana çıkış için zorunlu değildir; eski plaka/konsol görevini tekrarlamaz.

Yerleşim, görüş hatları, 10 maddelik bulmaca tarifleri, alternatifler ve
20 saniyelik çözüm çizelgeleri ORNEK_BOLUM_TASARIM_TR.md'de, sahne üretiminden
önce yazıldı. Modüler makine/duvarlar, rampalı üst yollar, cam gözlem alanı,
terk edilmiş izler, kablolar ve uzaktaki halkalı çekirdek bölümün kompozisyonudur.

## 51 yeni canlı kontrol

Önceki 202 kontrol korunur. Yeni kontroller gerçek CharacterController hareketi,
aynı ProcessControls E/R/ateş yolu ve gerçek collider/raycast üzerinden çalışır.
Bu kontroller OS üzerinden insanın fare/WASD kullanması olarak sunulmaz.

Servis raycast'i, kısa kapı, kapalı kapı/solo kısa yol, eşikte süre dolması,
Echo replay'i, iki bağımsız fiziksel çıkış, eski oda kayıtlarını temizleyen
checkpoint; sensör ve atış muhafazası; rampaya gerçek çıkış; iki iş yapan
rotalar; başarısız E niyetinin kaydı; Echo'lar arası olayların zaman sırasıyla
uygulanması; kaba adımda gerçek atış anının kullanılması; ölüm/retry/Tab;
çıkarılacak kaydın önizlemesi; düşmanın gerçek Echo hedefi ve saldırısı;
öldürmeden tahliye; isteğe bağlı gözlem ve kalıcılığı; final olaylarının yeniden
yüklenmesi; v2 disk roundtrip/bozuk dosya/yedek; yanlış aşama, tamamlanma bitleri,
yabancı cihaz ve sınırsız olay metninin reddi; v1 uyumluluğu ve gerçek disk
aktarımı (orijinal byte'lar korunur, yeni kayıt ezilmez, bozuk veri aktarılmaz);
alternatif çatışma; 15 oda yeniden kurulumu / 45 kayıt-reset ve native rampaların
sınırda kalması; eski demoya dönerken temizleme kontrol edildi.

## Gerçek 20 saniyelik çözüm kayıtları

| Kayıt | Ölçülen olay / rotanın süresi |
| --- | --- |
| Servis E | 1,47 sn; gerçek oyuncu kısa kapıdan geçti |
| Lens → üst röle E | sensör yaklaşık 0,9–6,7 sn; üst E 7,61 sn |
| Rezonans atışı → alt röle E | atış yaklaşık 5,53 sn; alt E 7,40 sn |
| Muhafız yemi → sol servis E | 5,71 sn |
| Rezonans → üst operatör E | yaklaşık 7,02 sn / 7,06 sn |

Tüm rotalar 20 saniyeye sığdı. Finalde gerçek oyuncu kapıdan fiziksel geçti.
Otomasyonun çözümü bilerek tamamlaması, ilk oynama süresi değildir.

## Kayıt ve tekrar deneme

Yeni örneğin şeması Version 2 / Layout 1; Device olayı 3 (eski türler aynı).
Stage, CompletedMask, ölüm, bellek ve en fazla iki tam Echo kaydı saklanır.
Finalin en fazla 8 kısa olay metni saklanır. Yeni dosya echo-session-v2.json;
eski echo-session-v1.json değiştirilmeden ilk geçişte içe alınır. Geçiş işareti
eski kaydın sonradan temizlenmiş bir oturumu yeniden diriltmesini önler.

Çözülmüş bağımsız sorun yeniden yaptırılmaz. Yeni checkpoint yerel Echo'ları
silip oyuncuyu sonraki karara taşır. R ve ölüm mevcut checkpoint planını korur;
Q son kaydı kaldırır, X/yeniden başlatma yerel planı temizler. DEVAM tam kayıtlarla
100 can ve yeni döngüden başlar; yarım kaydı kaldığı saniyeden devam ettirmez.
F6/otomatik test kullanıcı oturumuna yazmaz; test öncesi profil/oturum geri korunur.

## Görsel inceleme, performans ve sınırlamalar

Gerçek Windows framebuffer görüntüleri üç çözünürlükte boyut/dolu piksel
kontrolünden geçer. Servis, lens/röleler, olay haritası, tahliye, gözlem,
arşiv, camın arkasındaki terk edilmiş izler ve final ayrıca görsel incelendi. Gözlem camını örten dış duvarın görseli kaldırıldı; fiziksel sınır korundu. Harita açıklaması/düğme çakışması,
servis hedefinin görünürlüğü düzeltildi. Resetin ekranı kaplayan bant efekti kısa kenar ışığına çevrildi; görsel siperin altındaki kutu görünümü kapatıldı.

Bir Windows testinde arka plandaki pencere süre sınırına takıldı. Aynı EXE ön
planda tamamlandı; doğrulamalar görünür pencereyle yapıldı. Bu olay geçerli bir
oyuncu sonucu olarak sayılmaz. Tek makinedeki kısa benchmark donanım garantisi
vermez. Yeni bölüm / iki Echo için 120 gerçek kare ölçümü rapor sonuna eklenir.

## İnsan değerlendirmesi — bekleniyor

**İnsan oynama denemesi yapılmadı.** İlk kez oynama hedefi 8–12 dakika; gerçek
süre, anlaşılabilirlik, başarısızlığın okunması, tekrar isteği ve çözümün oyuncuya
ait hissettirmesi henüz ölçülmedi. Bu sürüm teknik olarak doğrulanmış bir insan
denemesi adayıdır; deneyim kabul koşullarının tamamı geçmiş sayılmaz.

INSAN_DENEMESI_TR.md, 3–5 oyunu bilmeyen kişi için kısa yönerge, 90 saniyelik
ipucu protokolü ve süre/yardım/başarısızlık/yürüyüş/öğrenme/tekrar isteği formunu
içerir. Paket formu çözüm anlatmaz; ilk oynayışta F6 kullanılmaz.

6–8 bölüm veya üçüncü Echo kesinleştirilmedi. BOLUM_GENISLETME_ADAYLARI_TR.md
farklı düşünme biçimlerini listeler; aynı örnek diğer bölümlere çoğaltılmadı.

## Yeni bölümün ölçülen kare süreleri

Windows / NVIDIA GeForce RTX 3060 Laptop GPU / D3D11. Her çözünürlükte 120 kare, iki Echo; yükleme süresi dahil değildir. Unity allocated native bellek sayacıdır; işletim sistemi/VRAM toplamı değildir.

1280x720 : ECHO BENCHMARK (120 frames, maintenance sample, 2 Echoes): median=6.9ms p95=7.0ms Unity allocated=44MiB

800x600 : ECHO BENCHMARK (120 frames, maintenance sample, 2 Echoes): median=6.9ms p95=7.0ms Unity allocated=44MiB

1920x1080 : ECHO BENCHMARK (120 frames, maintenance sample, 2 Echoes): median=6.9ms p95=7.0ms Unity allocated=46MiB

## Teslim adayı

Windows paketi: Deliverables/EchoProtocol_Bakim_Hatti_Ornek_2026-10-10.zip (EP-20261010-SAMPLE). Kaynak: Deliverables/EchoProtocol_Unity_Source.zip. Yeni kaynak, build ve raporlar maintenance-sample-20261010 Git etiketiyle korunur. Son paket dosyaları doğrulanan build ve temiz importun kaynaklarıyla byte/SHA256 üzerinden eşleştirilir. Teknik testlerden sonra yalnızca rapor/kanıtlar yeniden paketlenmiştir. Önceki 9 Ekim ZIP dosyaları korunur.
