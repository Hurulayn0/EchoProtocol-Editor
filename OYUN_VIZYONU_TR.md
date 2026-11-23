# Echo Protocol — ürün yönü / 10 Ekim 2026

Hedef artık ders projesi kapsamına sığmak değil: Windows'ta dağıtılabilen,
ayırt edilebilir bir FPS yapmak. Oyuncu bulmaca ve çatışmayı dengeli istiyor.
Başarı vaadi yerine tekrar deneme isteği, anlaşılabilirlik ve insan oynama
verisiyle kaliteyi değerlendireceğiz. Teknik temel ve mevcut içerik korunur.

Oyunun vaadi: **Geçmişteki kendin, bir sonraki planının takım arkadaşı olur.**
Yeni içerik bu vaadi görünür biçimde güçlendirmelidir. Yalnızca içerik sayısını
artırmak, çok silah eklemek veya görüntüyü renklendirmek yeterli değildir.

## İlk uygulama: okunabilen ortak çatışma

Bakım Hattı finali mevcut röle/rezonans çözümünü korur. Alternatif saldırgan
çözümde aynı muhafıza 1,2 saniye içinde farklı aktörlerin (sen/Echo 1/Echo 2)
isabeti +10 hasar verir; yaşayan hedefin saldırı hazırlığını 0,8 sn keser.
İki gövde isabeti birlikte 60 hasar verir. Aynı kişinin iki isabeti 50'de
kalır. Tek aktörle üç isabet veya rezonansla canlı muhafızı durdurmak hâlâ
geçerli alternatiflerdir. Ödülün ardından 1,2 sn yeniden tetikleme sınırı vardır.

Somut akış, 20 saniyelik döngü:

1. Sol ve sağ siperler üzerinden muhafızı ve üst operatör alanını gör.
2. İlk kayıtta sol geçitten ilerle; yaklaşık 3–5s'de muhafıza gövde atışı yap,
   ardından sol servise ilerleyip yaklaşık 6–8s'de E kullan. R ile kaydet.
3. Son döngüde sağ açıya geç. Muhafızın HEDEF: ECHO durumunu okuyup ilerle.
   Kayıtlı atıştan önceki 1s'de Echo'nun renkli hattı ve geri sayımı görünür.
   Echo isabetinden sonraki 1,2s içinde kendi atışını yap: eşzamanlı vuruş
   muhafızı etkisizleştirir. Echo sol servisi tamamlayınca üst operatörü E ile
   kullan ve tahliyeye geç. İki Echo'nun atışları da birlikte aynı ödülü alır.

Yanlış plan: kapalı siper kayıtlı atışı engellerse ödül oluşmaz; farklı zamanda
veya aynı aktörün atışı bonus vermez. Muhafızın canı ve isabet penceresi bunu
gösterir. T aynı kayıtları koruyarak tekrar başlatır. Öngörü çizgisi yalnızca
yaklaşan atışı gösterir; hasar veya yeni kayıt üretmez. Gelecek dünya sonucu
garanti edilmez: muhafız hareketi, siper ve oyuncu hattı değiştirebilir.

Bu akışın süre aralıkları tasarım hedefidir; otomatik entegrasyon kontrolleri
kayıtlı atış/ortak isabet davranışını doğrular. İnsan rotası ve keyif henüz
ölçülmedi. Mevcut kayıtlı çözümün gerçek hareket süreleri önceki tasarım ve
doğrulama belgelerinde ayrı tutulur.

## Sonraki üretim sırası

1. Bu ortak çatışmanın ve mevcut zaman bulmacalarının kör insan denemesi.
2. Sonuca göre ilk örnek bölümde rota, siper, öğretim ve tempo düzenlemesi.
3. Ortamda belirgin mimari odak, karakter animasyonu ve özgün sesle sunum.
4. Başarılı fikirlerden farklı düşünme isteyen yeni sektörler; bölüm adedi,
   üç Echo gereksinimi ve toplam süre gerçek sonuçlardan sonra kesinleşir.
5. Performans, erişilebilirlik, kullanıcı ayarları ve dağıtım kalitesinin
   yayın öncesi kabul ölçütleriyle doğrulanması.

Bu belgede sıradaki işler uygulanmış sayılmaz. İnsan denemesi olmadan
"oyuncular eğlendi", "8–12 dakika sürdü" veya "yayına hazır" sonucu verilmez.

## 26 Ekim — hedef bulma ipuçları

Bakım Hattı terminalleri artık görevlerine göre sabit renk taşır: servis ve
üst röle kehribar, rezonatör penceresindeki alt röle mavi, tahliye operatörü
camgökyüzü, isteğe bağlı bellek terminali mor. Aynı renkler rota haritası ve
dünya dışı hedef göstergelerinde kullanılır; kayıt çekirdeği, oda geometrisi
ve etkileşim mesafeleri değişmez. Unity Play Mode ve üç Windows çözünürlük
doğrulaması 26 Ekim raporundadır. Renk algısı ve yön bulma iyileşmesi için
insan denemesi henüz yapılmadı.

## 26 Ekim — bakım hattı sanat geçişi

Tahliye siperinde dörtlü metal kasa kompozisyonu, yaklaşma yönüne bakan tesis
tabelaları, tekstürlü depolama prop'ları ve yumuşak gölgeli iş ışıkları
uygulandı. Tabela HUD'daki hedefi çoğaltan serbest bir yazı gibi değil,
bulkhead'e bağlı bir tesis işareti olarak sunulur. Siperin fiziksel hacmi ve
diğer çözüm rotaları korundu. Güncel ekran görüntüsü ve test özeti
VISUALS_2026-10-26_TR.md'de; insan denemesi bu geçiş için henüz yapılmadı.
