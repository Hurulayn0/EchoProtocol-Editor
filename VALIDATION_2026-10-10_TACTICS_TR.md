# Ortak çatışma doğrulaması / 10 Ekim 2026

Başlangıç: `pre-tactical-20261010`, commit `c0e9d97`. Mevcut çalışan proje
genişletildi. Unity 2021.3.21f1 / Built-in / Windows x64 korunur.
Oyuncunun tercihi bulmaca ve çatışmanın dengeli birleşimidir; ders projesi
kapsam sınırı ürün hedefinden çıkarıldı. Ürün yönü OYUN_VIZYONU_TR.md'de.

## Uygulanan değişiklikler

- Echo atışından önceki bir saniyede kendi renginde kayıtlı atış hattı ve geri
  sayım. Çizgi mevcut fizik dünyasının ilk temasında biter; ateş etmez ve
  gelecekteki isabeti garanti etmez. Atış sonrası çizgi kapanır.
- Muhafızın güncel hedefi, saldırı hazırlığı, canı ve sersemleme durumu.
  Önceki saldırı hedefi, güncel hedef olarak kullanılmaz. Eski saldırı verisi
  isteğe bağlı bellek görevinin gerçek saldırı kanıtı olarak korunur.
- Final odasında farklı aktörlerin 1,2 saniyelik ortak isabeti +10 hasar;
  yaşayan hedefe 0,8 sn sersemleme. Aynı aktörün tekrarı bonus vermez. Ödül
  yeniden tetiklenmeden önce 1,2 sn geçmelidir. Oyuncu + Echo ve iki Echo
  desteklenir. Zaman sınırı float hassasiyetiyle güvenli karşılaştırılır.
- Eşzamanlı vuruşa kısa ekran cevabı, parçacık, yer halkası, ses ve olay
  geçmişi. Bakım Hattı Echo 2'nin gerçek atış izi kendi mor rengini kullanır.
- Oyuncunun kayıtlı çatışma atışını kapatması açıkça bildirilir; bonus oluşmaz.
- Daha kısa Echo kartları ve bağlama göre görünen final hedefleri. Dünya
  hedef etiketleri sağ/sol HUD panellerinin içine taşmaz.

Kayıt çekirdeği, dosya şeması ve cihaz kimlikleri değişmedi. v1 kampanya ve
v2 örnek bölüm kayıtları mevcut yoldan okunur. Ortak isabet ve önizleme
yalnızca mevcut döngünün geçici durumudur; yükleme, ölüm sonrası tekrar,
kontrol noktası veya T sonrası yeni döngüde temizlenir.

## Kontroller

Son aday için gerçek Unity Play Mode **303/303** geçti. Önceki **279**
kontrol korunur; yeni **24** kontrol gerçek kayıtlı atışı, ortak hasarı,
aynı aktörü, iki Echo'yu, sınır zamanını, engellenmiş atışı, yükleme/reset,
güncel hedef/sersemlemeyi ve 12 tekrar sonrası nesne sınırını kapsar.
Bağımsız kayıt kontrolleri **14/14**; ek regresyon **25/25**, 1.000 üretilmiş
kayıt senaryosu. Windows build sonucu **0 hata / 0 uyarı**.

Son arayüz düzeltmesi dahil gerçek Windows uygulaması **1280×720,
800×600 ve 1920×1080** çözünürlüklerinde ayrı ayrı **303/303** geçti.
Her çalıştırmada çıkış kodu **0**, runtime hatası **0**. Gerekli ekran
görüntülerinin boyutları ve boş olmaması otomatik kontrol edildi; yeni
önizleme, isabet penceresi ve ortak bitiriş görüntüleri ayrıca incelendi.
Kanıt dizini: Verification/Results/2026-10-10/EchoTactics.

Pencereyi öne alma aracı iki denemede `failed to activate captured window`
döndürdü. Oyun testleri kendi yollarında tamamlandı; doğrulama günlükleri ve
uygulamanın kaydettiği gerçek framebuffer görüntüleri kullanıldı.

Kaynak ZIP, Library önbelleği olmayan yeni klasöre çıkarılarak gerçek Unity
importu/Play Mode ile **303/303** geçti. Paylaşım ZIP'inden çıkarılan Windows
uygulaması ayrıca **303/303** geçti; çıkış **0**, runtime hatası **0**.
Standart Windows ve paylaşım ZIP'lerinde **126** build dosyasının içerikleri
birebir karşılaştırıldı; güvenlik düzeltmeli UnityPlayer.dll aynı kaldı.

Oynanabilir paket: `Deliverables/EchoProtocol_Ortak_Catisma_2026-10-10.zip`.
Sürüm kimliği: **EP-20261010-SAMPLE-TACTICS**. Önceki paylaşım ZIP'leri korunur.
Kaynak proje: `Deliverables/EchoProtocol_Unity_Source.zip`. Son belge/kanıt
paketlemesi sonrası `final-package-audit-result.txt`, kaynak dosyalarını
çalışma alanıyla, Assets/Packages/ProjectSettings içeriklerini temiz import
adayıyla ve bütün Windows dosyalarını doğrulanmış build ile karşılaştırır.
Bu son paketleme çalışma kodunu değiştirmez. Son denetim **363** kaynak/
asset/config/belge dosyası, **126** Windows dosyası ve üç doğru sürümlü,
boş olmayan deneme yönergesi/formu için geçti.

İlk genişletme turunda üçüncü solo atış kontrolü kapalı görüş hattı yüzünden
başarısız oldu. Testin üçüncü atışı açık açıya taşındı; normal üç atışlı çözüm
yeniden kontrol edildi. Ayrıca oyuncunun gerçek Echo atışını engellediği
ayrı senaryo eklendi. Ara turda 302 kontrol, son UI/nesne dayanıklılığı
eklendikten sonra 303 kontrol çalıştırıldı. Ara tur son sürüm kanıtı sayılmaz.

Kayıt çekirdeği SHA256:

- ReplayTimeline.cs: `5BB38EA3FC772C28C8901DFD958D145C78D1B8A8B4C6E61717652B5645C90A52`
- TimelineSelfChecks.cs: `0BF14AEAB955B6A9AE4688514377934C1B5AF3F368349513D7C57F1FE72DCB2A`

## İnsan değerlendirmesi

**Yeni sürüm için bağımsız insan denemesi yapılmadı.** Anlaşılırlık, ortak
atışın keyfi, ilk oynama süresi ve tekrar deneme isteği ölçülmedi.
3–5 oyuncu formuna hedef okuma, renkli atış önizlemesi, bonusun fark edilmesi
ve bulmaca/çatışma dengesi soruları eklendi. Form INSAN_DENEMESI_TR.md'de.
Yeni taktik rotasının yazılı süreleri tasarım tahminidir; otomatik çatışma
senaryosunun süresi insan tamamlama süresi değildir.
