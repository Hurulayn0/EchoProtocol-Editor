# Echo Protocol — gelişmiş Windows demosu / 9 Ekim 2026

Bu sürüm mevcut üç sektörlü projeyi daha oynanabilir ve atmosferli hale getirir.
Üç kısa bölüm, iki Echo, tek silah ve tek muhafız türü korunur. Bağımsız kayıt
çekirdeği değiştirilmedi. Windows'ta indirilebilir ders demosu hazırlanır.

## Oynanış

- **Rezonans darbesi:** parlayan küreye ateş etmek yakın muhafızları iki saniye
  durdurur, yakındaki oyuncuyu en fazla 20 can iyileştirir. Her döngüde bir kez.
  Aynı atışı Echo'ya kaydetmek ilerlerken kendi geçmişinden destek almayı sağlar.
- **Bellek kasası:** konsollar açıkken plakaları gerçek Echo'lara devret.
  Rezonatörü vur; altı saniye içinde terminale nişan alıp E'ye bas.
  Her sektörde bir isteğe bağlı yan görev var; ana çıkış için zorunlu değil.
- **Üç parçalık hikâye:** toplanan kayıtlar ana menüde BELLEK ARŞİVİ'nde okunur.
  Geri sarma, ölüm, yeni oyun ve uygulamayı kapatma bellekleri silmez.
- **Zayıf nokta:** muhafızın üst kırmızı bölgesine isabet 50 hasar verir ve
  saldırıyı keser. Gövde 25 hasar; 60 canlık muhafız iki üst isabetle düşer.
  Echo'nun ateşi de aynı gerçek raycast ve hasar hesabını kullanır.
- **Tab haritası:** P rezonatörü, V kasayı gösterir. Plan açıkken darbe penceresi,
  muhafız sersemlemesi ve dünya zamanı durur.

## Görsel ve ses

Laboratuvar camından tüpler ve bekleyen Echo'lar; bakım hattında camın arkasında
boru/tüp tesisi; son sektörde kapının ötesinde büyük bir reaktör manzarası var.
Yerleşimler mevcut CC0 modellerden ve proje içinde üretilen geometriyle kuruldu.
Cam, havalandırma buharı, sektör ışıkları, gölgeler, hafif renk düzenlemesi,
geri sarma çizgileri, vuruş/ölüm kıvılcımları ve yürüyüşte silah salınımı eklendi.
Silah, isabet, geri sarma, rezonans, kayıt toplama sesleri ve tesis uğultusu
kodla üretilir; beş efekt dört ses kaynağını paylaşır. Ücretli asset eklenmedi.

## Nasıl denersin?

1. `Deliverables/EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip` dosyasını tamamen çıkar.
2. `EchoProtocol/EchoProtocol.exe` dosyasını aç; Unity kurulumu gerekmez.
3. Önce normal konsol/plaka görevini Echo'ya kaydet. Çıkış ana hedefindir.
4. Kasayı denemek için rezonatörü vur ve süre bitmeden yaklaşıp E'ye bas.
5. Son sektörde kasaya erişmek için iki plakada da birer Echo bulunmalı.
6. İstersen F6 ile 202 kontrollü gerçek Unity test takımını çalıştır.

Paketin Türkçe başlatma, deneme, geri bildirim ve lisans dosyaları vardır.
Ayrıntılı güncel sonuçlar **VALIDATION_TR.md** ve
`Verification/Results/2026-10-09/Immersion/` içindedir. Önceki 157 kontrollü
arkadaş paketi `EchoProtocol_Arkadas_Test_2026-10-09.zip` yerelde korunur.

## Sonraki değerlendirme

Arkadaşların yeni rezonans/kasa hedefini yardım almadan anlayabilmesi,
altı saniyelik sürenin uygunluğu ve üç bölümdeki tekrar hissi değerlendirilmelidir.
Bu sürüm kısa bir ders demosudur; insan oynama geri bildirimi zorluk ve akışın
son ayarlarını belirler. Otomatik kontrol, eğlencenin garantisi değildir.
