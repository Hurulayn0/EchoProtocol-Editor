# Echo Protocol — hedef bulma ipuçları doğrulaması / 26 Ekim 2026

Bu rapor, 10 Ekim'de doğrulanan Bakım Hattı / Eşzamanlılık sürümüne
istenen görsel yön bulma iyileştirmesini ekler. Önceki sürüm ve oyun
mekaniklerinin geçmişi için [VALIDATION_TR.md](VALIDATION_TR.md) ve
[VALIDATION_2026-10-10_TACTICS_TR.md](VALIDATION_2026-10-10_TACTICS_TR.md).

## Yapılan değişiklik

- Bakım ve üst röle panelleri kehribar, rezonansla etkinleşen alt röle mavi,
  tahliye operatörü camgökyüzü, isteğe bağlı bellek kasası mor renktedir.
- Aynı sabit renkler dünya hedef göstergelerinde ve duraklatılmış Echo rota
  haritasında kullanılır. Döngü sıfırlanınca panel kendi bekleme rengine döner.
- Oda geometrisi, kayıt sistemi, etkileşim mesafeleri ve bulmaca koşulları
  değiştirilmedi.
- İnsan deneme formuna hedef renklerini eşleştirme sorusu eklendi.

## Gerçekten çalıştırılan kontroller

| Kontrol | Sonuç |
| --- | --- |
| Runtime/editor C# derleme | Geçti |
| Kayıt zaman çizelgesi kontrolleri | 14/14 |
| Kayıt regresyonu | 25/25; 1.000 üretilmiş kayıt |
| Unity 2021.3.21f1 Play Mode / F6 test yolu | **305/305 geçti** |
| Windows x64 build | Başarılı; 0 hata, 0 uyarı |
| Windows standalone 1280×720 | 305/305; 0 runtime hatası |
| Windows standalone 800×600 | 305/305; 0 runtime hatası |
| Windows standalone 1920×1080 | 305/305; 0 runtime hatası |
| Görsel regresyon kontrolleri | Servis/üst röle renkleri, rota haritası ve tahliye oda işaretleri geçti |

Unity doğrulama ekran görüntüleri `Verification/Results/2026-10-26/EchoWayfinding/`
altındadır. Örnekler `game-1280x720-sample-event-map.png`,
`game-1280x720-sample-combat.png`, `game-1280x720-sample-memory.png` ve
`game-1280x720-sample-win.png` dosyalarıdır. Windows çözünürlük başına tam
otomatik test kayıtları da aynı klasördedir.

## İnsan deneyimi durumu

Bu bir teknik doğrulamadır. Oyunu bilmeyen kişilerle oynanabilirlik denemesi
henüz yapılmadı; renkleri daha kolay seçip seçmedikleri veya rota planlamanın
kolaylaştığı iddia edilmiyor. 3–5 kişi için güncel gözlem maddeleri
[INSAN_DENEMESI_TR.md](INSAN_DENEMESI_TR.md) içindedir. 8–12 dakika hâlâ
ölçülmemiş ilk oynama hedefidir.
