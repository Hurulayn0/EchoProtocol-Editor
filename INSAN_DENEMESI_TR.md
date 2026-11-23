# Bakım Hattı / Eşzamanlılık — 3–5 yeni oyuncu denemesi

Durum: **Hazırlanmış deneme; henüz insan sonucu yok.** Teknik test sonuçları
ayrı rapordadır. 8–12 dakika ilk oynayış hedefidir; ölçülmüş süre değildir.

Oyunu ve çözümünü bilmeyen 3–5 kişi aynı Windows ZIP'ini oynasın. Her kişi için
bir form doldur. Kişiler önceki oyuncunun çözümünü izlemesin. Paket sürümünü
forma yaz; farklı sürümlerin sürelerini tek sonuç gibi birleştirme.

## Oyuncuya verilecek kısa yönerge

"ZIP'i tamamen çıkarıp EchoProtocol.exe'yi aç. Ana menüde ÖRNEK BÖLÜMÜ OYNA'yı
seç. Bakım hattının tahliye yolunu açmaya çalış. WASD/fare hareket ve bakış,
E etkileşim, R kayıt, T aynı planla tekrar deneme, Shift koşu, sol tık ateş. Diğer kontroller oyunda var.
Lütfen düşündüğünü sesli söyle; yanlış denemek sorun değil."

Çözüm sırası, sensörde bekleme süresi veya Echo'nun hangi göreve gideceği
önceden anlatılmaz. İlk oynama denemesinde F6 çalıştırılmaz.

## Gözlemci

1. Örnek bölüme başlama anında gerçek süreyi başlat. Duraklama ve dış kesinti
   sürelerini ayrı kaydet. Oyunun finalde gösterdiği aktif süreyi de yaz.
2. İlk hedefi ne zaman kendi sözleriyle anlattığını ve ilk E/R keşfini kaydet.
3. Her kontrol noktasına giriş/çıkış anını not et. Başarısız denemede hangi
   koşul eksikti, oyuncu bunu HUD/lazer/olay kartından anlayabildi mi?
4. İlk 90 saniyelik takılmada çözümü söylemeden "Hangi işi geçmişteki sen
   yapabilir?" gibi bir fikir ipucu verilebilir. Yardım zamanı ve sözünü yaz.
   Gerekirse ikinci ipucu ver; yardım verilmiş süreyi yardımsız sonuç sayma.
5. Oyuncu 20 dakikada bitiremezse ulaştığı noktayı ve nedeni kaydet; bitirmiş
   gibi süre üretme. İsterse sonra birlikte çözüm konuşulabilir.
6. Sıkıcı yürüyüş/bekleme, yanlış kayıt değiştirme, görünmeyen etkileşim,
   kapalı atış hattı, ölüm ve tekrar deneme anlarını işaretle.
7. Finalden sonra aşağıdaki soruları sor. Yönlendirmeden oyuncunun kendi
   sözlerini kaydet. Bellek görevi yapılmadıysa "yapılmadı" olarak yaz.

## Her oyuncu için kopyalanacak form

```text
Sürüm / ZIP adı:
Oyuncu takma adı (isteğe bağlı):
Oyunu ilk kez mi görüyor?:
Windows / çözünürlük / ekran kartı (biliniyorsa):

Gerçek başlangıç saati:
Gerçek bitiş / bırakma saati:
Toplam süre (dk):
Dış kesinti / duraklama süresi:
Final ekranındaki aktif oyun süresi / kayıt / ölüm:
Tamamlandı mı? Değilse nerede bırakıldı?:

Servis geçidi süresi:
Atış penceresi süresi:
Tahliye süresi:
İlk hedefi kendi sözleriyle anlatabildiği an:
Ekranda gördüğü ilk hedef metnini kendi cümlesiyle nasıl yorumladı?:
Hedef metni çözümü fazla açıkladı mı, yoksa kendi planını kurmasına alan bıraktı mı?:
Echo'nun kayıtlı hareketini ilk anladığı an:
Echo'yu yön tuşlarıyla kontrol edeceğini sandı mı?:
R ve T farkını kendi sözleriyle açıklayabildi mi?:
Q ve X'e basmadan önce bu tuşların ne yapacağını doğru tahmin etti mi?:
Kendi isteği dışında Echo oluştuğunu düşündüğü an / nedeni:
Echo değiştirme seçimi ve Tab rota önizlemesi anlaşılır mıydı?:
Röle, operatör ve bellek hedeflerini yazı/renk işaretleriyle ayırabildi mi? Echo renklerini hedef renkleriyle karıştırdı mı?:
Tesis tabelaları mekânın parçası ve okunaklı göründü mü? Neden?:
Kargo kasası siperi, eski mavi örtüden daha inandırıcı göründü mü?:
Işık/karanlık dengesi hedefleri ve düşmanı seçmeyi kolaylaştırdı mı?:

Yardım sayısı:
Her yardımın zamanı ve tam cümlesi:
Başarısızlık nedenleri ve oyuncunun açıklaması:
Yanlış silinen/değiştirilen Echo oldu mu?:
Gereksiz tekrar yürüyüşü veya bekleme nerede/ne kadar?:
Kontrol noktasının neyi koruduğunu anladı mı?:
Bellek görevini fark etti mi / yaptı mı?:
Muhafızın şu anda kimi hedeflediğini anlayabildi mi?:
Renkli atış çizgisini tehlike mi, Echo önizlemesi mi sandı?:
Echo atışına eşlik etti mi / bonusu fark etti mi?:
Bulmaca ve çatışma dengesi (çok bulmaca / dengeli / çok çatışma):

Oyuncunun öğrendiği mekanik, kendi cümlesiyle:
"Geçmişteki hareketimi planımda kullandım" hissinin oluştuğu an:
Başarısızlığın nedeni anlaşılır mıydı? (1–5):
Yeni bir plan denemek istedim. (1–5):
Çözüm kendi düşüncemden doğdu. (1–5):
Gereksiz tekrar yüzünden sıkıldım. (1–5; yüksek kötü):
Genel keyif (1–5):
En iyi an:
En karışık an:
Önce değiştirilmesini istediği şey:
Tekrar oynamak ister mi, neden?:

Teknik sorun: konum, adımlar, beklenen/görülen sonuç, tekrar oluşma:
Varsa görüntü / Player.log:
```

## Sonuçları değerlendirme

Yardımsız ve yardımlı tamamlamaları ayrı say. 3–5 kişiyle istatistiksel garanti
iddia etme; yinelenen sorunları belirle. Süre hedefinden sapmayı oyuncunun
karar verme süresi, anlaşılmayan geri bildirim veya gereksiz hareket olarak ayır.

Bir fikri başka bölümlere yaymadan önce oyuncuların amacı anlayabilmesi,
başarısızlığın nedenini açıklayabilmesi ve yeni plan denemek istemesi gözlenmeli.
Sadece hızlı bitirme veya otomatik test başarısı yeterli kabul değildir.
Sonuçlar gelene kadar bu örneğin tasarımını topluca çoğaltma; teknik/görsel
bağımsız iyileştirmeler yapılabilir.
