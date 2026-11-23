# Oyun içi yönlendirme ve Echo açıklığı — 26 Ekim 2026

Bu geçiş, paylaşılan ZIP incelemesindeki yazı/arayüz önerilerini kaynak projeyle
karşılaştırır. İncelemeyi yapan kişi oyunu çalıştıramadığını açıkça belirttiği
için raporundaki maddeler insan oynama gözlemi değil, doğrulanabilir tasarım
hipotezleri olarak ele alındı.

## Değişiklikler

- Üç kontrol noktasının hedefleri çözüm sırası vermek yerine mevcut durumu
  bildiriyor. İlk öğretici Echo'nun kaydı otomatik oynattığını öğretiyor; kapıyı
  açmak için gidilecek tam rota ekrandan söylenmiyor.
- Alt görev HUD'ındaki servis odası ve tahliye için sabit çözüm adımları
  kaldırıldı. İsteğe bağlı arşiv/bellek görevinde Echo'ya yemleme, rezonans ve
  terminal sırasını baştan gösteren metin kaldırıldı; ipucu duruma göre açılıyor.
- Kontrollerde R, Q, X ve T'nin işlevi ve etkilediği kayıt kapsamı tutarlı
  anlatılıyor. X'in yalnızca mevcut oda/kontrol noktasındaki Echo kayıtlarını
  sildiği, oda durumunu yenilediği ve bölüm ilerlemesini koruduğu belirtiliyor.
- Kapı mesajı hiç kullanılmamış servis paneli ile süresi geçmiş 1,4 saniyelik
  geçiş penceresini ayırıyor. Enerji biterse üst/alt röle durumunu gösterip
  değiştirilecek gecikmiş E kaydını işaret ediyor; aynı planı yeniden denemeyi
  tek başına çözüm gibi sunmuyor.
- Kapanan atış hattı, Echo'nun atış anında rezonatörü göremediğini; oyuncunun
  Echo ile hedef arasına girmesi ise hattı kapattığını açıkça belirtiyor.
- Röle ve operatör terminal renkleri Echo 1'in cyan ve Echo 2'nin mor kimlik
  renklerinden ayrıldı. İnsan denemesi formu hedef adlarının/renklerinin
  karışıp karışmadığını ve Q/X davranışını önceden tahmin edip edemediğini soruyor.
- Windows oynama yönergesi, deneme rehberi ve geri bildirim formu örnek bölümü
  tarif edecek şekilde eşitlendi; eski üç sektörlü karşılaştırma demosu artık
  örnek bölümün yönergesiyle karışmıyor.

## Teknik doğrulama

| Kontrol | Sonuç |
| --- | --- |
| Runtime/editor C# derlemesi | Geçti |
| Kayıt zaman çizgisi kontrolleri | 14/14 |
| Kayıt regresyonu | 25/25; 1.000 üretilmiş kayıt |
| Unity Editör Play Mode / F6 yolu | 311/311 |
| Windows x64 derlemesi | Başarılı; 0 hata, 0 uyarı |
| Windows 1280×720 | 311/311; çalışma zamanı hatası yok |
| Windows 800×600 | 311/311; çalışma zamanı hatası yok |
| Windows 1920×1080 | 311/311; çalışma zamanı hatası yok |
| Play Mode ekran yakalamaları | Oluşturuldu; üç çözünürlükte boş olmayan kareler kontrol edildi |
| Kaynak ZIP'i temiz klasörden Unity'ye içe aktarma | 311/311 |
| Çıkarılmış arkadaş ZIP'i / Windows oyuncusu | 311/311; çıkış kodu 0; çalışma zamanı hatası yok |
| Son paket içerik denetimi | 366 kaynak dosyası ve 126 Windows dosyası eşleşti; rehber sürümü doğru |

Kanıtlar: `Verification/Results/2026-10-26/Clarity/` ve
`Verification/Logs/` içindedir. F6 ve Windows otomatik oyuncusu Unity'nin
kontrol yolunu çalıştırır; bir insanın fare/WASD ile oynadığı deneme değildir.

## İnsan değerlendirmesi

3–5 yeni oyuncu için deneme formu güncellendi, ancak insan oynama denemesi henüz
yapılmadı. Süre, anlaşılabilirlik, görev metinlerinin keşfe alan bırakması,
tekrar deneme isteği ve renk/tuş karışıklığı hakkında sonuç iddia edilmiyor.
Deneme formu: [INSAN_DENEMESI_TR.md](INSAN_DENEMESI_TR.md).

## Paket

İnsan denemesi için hazırlanan Windows paketi
`EchoProtocol_Bakim_Hatti_Yonerge_Duzeltmesi_2026-10-26.zip` dosyasıdır.
SHA-256 özeti `Deliverables/EchoProtocol_Bakim_Hatti_Yonerge_Duzeltmesi_2026-10-26.zip.sha256.txt`
yan dosyasında tutulur. Temiz çıkarma ve paket bütünlüğü sonucu final denetim
`Verification/Results/2026-10-26/Clarity/` içinde doğrulandı. Paylaşım ZIP'i
çıkarılıp test edildi; dosya SHA-256 yan dosyasıyla eşleşiyor.
