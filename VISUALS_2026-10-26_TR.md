# Echo Protocol — Bakım Hattı görsel sanat geçişi / 26 Ekim 2026

Bu geçiş, çalışan örnek bölümün sahne kurulum kodunda ve mevcut Quaternius
modüler varlıkları kullanılarak yapıldı. Yeni render pipeline veya dış asset
paketi eklenmedi.

## Görsel değişiklikler

- Tahliye odasındaki uzatılmış mavi örtü siper, dört dokulu metal kargo
  kasasıyla değiştirildi. Oyun mantığının kullandığı 3,3 × 2,4 × 0,9 birimlik
  fiziksel siper hacmi ve atış görüşü değişmedi.
- Örnek bölümün üç kontrol noktasına, yaklaşma yönüne bakan tesis tabelaları
  eklendi. Önceki havada duran büyük öğretici yazı kaldırıldı; etkileşim
  yönergeleri HUD'da kalıyor.
- Servis alanına dolap, masa ve terminal; odalara küçük ölçekli depolama
  detayları eklendi. Bu modeller mevcut yürüme ve atış rotalarının dışına
  yerleştirildi.
- Servis paneli ve reaktör çevresine yumuşak gölgeli iş ışıkları eklendi;
  genel ortam/ana ışık şiddeti düşürülerek sıcak yerel ışıkların ayrımı
  artırıldı.
- Oyun içi bilgi panellerinin arka planı hafifçe saydamlaştırıldı.

Bu, tek örnek bölüm için ilk görsel geçiştir; insan oyuncuların daha gerçekçi
veya daha okunaklı bulduğu henüz ölçülmedi. Bölüm tasarımı topluca diğer
alanlara uygulanmadı.

## Teknik doğrulama

Unity 2021.3.21f1, Built-in render pipeline, Windows x64:

| Kontrol | Sonuç |
| --- | --- |
| Runtime ve Editor C# derlemesi | Geçti |
| Kayıt zaman çizelgesi | 14/14 |
| Kayıt regresyonu | 25/25; 1.000 üretilmiş kayıt |
| Unity Play Mode / F6 yolu | **306/306 geçti** |
| Windows x64 build | 0 hata, 0 uyarı |
| Windows 1280×720 | 306/306; 0 runtime hatası |
| Windows 800×600 | 306/306; 0 runtime hatası |
| Windows 1920×1080 | 306/306; 0 runtime hatası |
| Yeni görsel regresyonlar | Tabela/iş ışıkları, dört kasa ve değişmeyen siper çarpışması geçti |

Güncel ekran görüntüleri ve test kayıtları
`Verification/Results/2026-10-26/EchoRealism/` içindedir. Görsel örnekler
`game-1280x720-sample-service.png` ve `game-1280x720-sample-combat.png`.

Bu rapordaki sonuçlar teknik testlerdir. 3–5 kişilik insan denemesi, gerçek
oynama süresi ve görsel geri bildirim henüz yoktur; gözlem formu
[INSAN_DENEMESI_TR.md](INSAN_DENEMESI_TR.md) içinde.
