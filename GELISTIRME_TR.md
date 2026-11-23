# Echo Protocol — tamamlanan geliştirme aşamaları

Kullanıcının onayladığı kapsam: üç kısa 3D FPS/Action bölüm, en fazla iki Echo,
tek silah, tek düşman türü, menü ve kazanma/kaybetme ekranları.

8 Ekim 2026'da kullanıcı mevcut prototipin doğrulamasından sonra kalan oyunun
geliştirilmesini açıkça istedi. Aşağıdaki kapsam aynı proje üzerinde tamamlandı.

| Aşama | Sonuç |
| --- | --- |
| Mevcut prototipin korunması | Bağımsız ReplayTimeline değişmedi; çekirdek altı canlı kontrol yeni testin başlangıcında korunuyor |
| Tek silah | Pulse carbine; hitscan, ateş bekleme süresi, görünür model, iz, geri tepme ve isabet göstergesi |
| Echo ateşi | Kaydedilen ışın konumu/yönüyle gerçek dünya raycast'i; bağımsız imleçler |
| Tek düşman türü | Devriye muhafızı; can, gerçek collider, görüş hattı ve yakın Echo'ya yönelen saldırı |
| Dünya reseti | Oyuncu ve muhafız canı, muhafız konumu, güç, kapı ve silah süreleri sıfırlanıyor |
| Üç kısa bölüm | Araştırma laboratuvarı, Bakım hattı, Reaktör; son bölüm iki ayrı konsol/plaka ve iki Echo gerektiriyor |
| Menü / ekranlar | Başlat, kontroller, çıkış, duraklatma, mevcut bölümü yeniden deneme, bölüm sonu ve final kazanma |
| Görsel / ses geri bildirimi | Sektör renkleri, rehber ışıklar, kapalı laboratuvar, siperler, HUD ve üretilmiş kısa sesler |
| Windows paketleme | 64 bit Mono build; oynanabilir klasör ve ZIP |
| Doğrulama | 14/14 bağımsız kayıt ve 121/121 canlı oyun kontrolü; gerçek EXE de çalıştırıldı |

Çalışan temel dönem projesi sürümü hazır. Uzun klavye/fare oynama seansı ve
dersin teslim rubriğine göre değerlendirme otomatik testlerin dışında kalır.
Rubrikte ayrıca talep edilmedikçe yeni silah/düşman türü veya daha fazla bölüm
bu kapsama eklenmez.

İlk doğrulanmış prototipin kaynak yedeği yerelde
Verification/Backups/verified-prototype-20261008.zip içinde korunuyor.
Güncel ayrıntılı sonuçlar VALIDATION_TR.md'de.

## Ayrıntılı doğrulama ve kullanım iyileştirmeleri

Son incelemede muhafızın hedef seçiminde bir boşluk düzeltildi: en yakın hedef
siper arkasındayken görünür diğer hedefi yok sayıyordu. Artık yalnızca görünür
hedefler arasında seçim yapıyor. Hasar çağrıları sıfır/negatif hasarı ve oyun
duraklatıldığında oyuncuya hasar verilmesini reddediyor.

Fare hassasiyeti, dikey fare yönü, ses seviyesi ve görüş açısı ayarları;
sektör tamamlandıktan sonra bölümden devam etme; görev durumuna göre ipuçları;
son saniye uyarısı; son bölümde ayrı A/B güç/plaka bilgisi eklendi. Reset,
eski ateş izlerini ve silah geri bildirimini de temizliyor. Arayüz dört en/boy
oranında biçimini koruyan ölçekleme kullanıyor ve Türkçe karakterler gösteriyor.

Orijinal 34 kontrol korundu. 63 yeni kontrol; gerçek controller hareketi ve
zıplama, ortak giriş yolu, menü/ayar durumları, PlayerPrefs, sınır koşulları,
eşzamanlı iki Echo ateşi, üç bölümün kontrol komutlarıyla çözümü ve 45 sektör
yenilemesi / 180 kayıt resetini kapsıyor. Kayıt çekirdeğine ayrıca 25 bağımsız
sınır/regresyon kontrolü ve 1.000 üretilmiş kayıt senaryosu uygulandı.

Oyun kapsamı üç bölüm, iki Echo, tek silah ve tek düşman türü olarak korunur.
Son doğrulama ve oynanabilir paketler VALIDATION_TR.md'de belirtilir.


## Oynanabilirlik ve ortam yenilemesi

Shift ile koşu, sağ tuşla nişan, Q ile son Echo'yu geri alma, iki yuva doluyken
süre sonunda kayıtları koruma eklendi. Görev işaretleri, konsol/plaka ataması
gösteren Echo kartları, muhafız can çubuğu, ateş öncesi lazer uyarısı ve
etkisiz hale getirme geri bildirimi HUD'a bağlandı.

Üç sektör Quaternius'un CC0 modüler model ve dokularıyla yeniden giydirildi.
İkinci sektörün konsol/plaka rotası sağ arka bakım hattına taşındı. Robotlar
yürüme/bekleme animasyonları kullanır. Silah modeli mevcut tek silahın
mekaniğini korur. Masa, dolap ve variller fiziksel engeldir; ana mekanik
collider'ları ve bağımsız kayıt çekirdeği korunur. Asset kaynakları, lisanslar
ve Unity dönüşümü ASSET_CREDITS_TR.md'de açıklanır.

24 yeni canlı kontrol; koşu, nişan, kameranın reseti, Q öncelikleri, süre
sonunda iki Echo'yu koruma, bilerek üçüncü kaydı oluşturma, görev atamasını
tanıma, gerçek robot ölçeği/dokusu/animasyonları, üç ortam ve ateş uyarısını
kapsar. Ayrıntılı güncel sonuç VALIDATION_TR.md'de bulunur.


## Rezonans, bellek ve atmosfer / 9 Ekim 2026

Mevcut oyun üzerine isteğe bağlı süreli yan bulmaca, üç kalıcı hikâye kaydı,
Echo'nun da tetikleyebildiği tek kullanımlık rezonans darbesi, zayıf nokta hasarı
ve saldırı kesme eklendi. Yeni gözlem odaları ve büyük reaktör manzarası,
parçacıklar, beş kısa efekt sesi, ortam uğultusu ve kamera sunumu kuruldu.
Yeni modül bağımlılığı Unity'nin yerleşik particlesystem modülüdür.
Yeni canlı kontrollerle toplam 202'ye çıkıldı. Ayrıntılar IMMERSION_TR.md,
gerçek koşu ve dağıtım kanıtları VALIDATION_TR.md içindedir.
