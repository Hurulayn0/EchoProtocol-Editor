# Echo Protocol — 10 Ekim 2026 örnek bölüm özeti

Güncel hedef: ders projesi kapsamına bağlı kalmadan, Windows için bulmaca ve
çatışmayı dengeli birleştiren özgün bir FPS. Ürün yönü OYUN_VIZYONU_TR.md'de.
Son güncelleme Echo'nun yaklaşan atışını renkli hat/geri sayımla gösterir;
muhafızın güncel hedefini ve farklı aktörlerin ortak isabet ödülünü ekler.
Güncel 303 kontrol ve dağıtım kanıtları VALIDATION_2026-10-10_TACTICS_TR.md'de.

Unity 2021.3.21f1 / Built-in / Windows x64. Aynı çalışan proje ve kayıt
çekirdeği üzerinde geliştirilir. Başlangıç sürümü Git etiketi
verified-demo-20261009 (e9ac584e3e5d401d8bcf64346508d90086ef2857) ile korunur.

**Temel deneyim:** “İlk denememde yaptığım hareket, sonraki denemede planımın
bir parçasına dönüştü. Geçmişteki kendimle birlikte problemi çözdüm.”

**Yeni uygulanan örnek:** Bakım Hattı / Eşzamanlılık. Üç kontrol noktası:
uzaktaki E ile kısa kapı; sensör tutma → atış penceresi → iki röle;
muhafızı oyalama → planlanmış rezonans → tahliye operatörü.
İki Echo yeterli; önceki bağımsız başarılar tekrar yaptırılmaz.
Tek mevcut silah ve muhafız kullanılır. Doğrudan çatışma alternatif çözümdür.

Modüler duvar/makineler, yükseltilmiş rampalı servis yürüyüşleri, camlı gözlem
odası ve uzaktan görülen halkalı enerji çekirdeği eklendi. İsteğe bağlı
Gözlem 04 bellek görevi tesis çalışanlarının Echo'larla tahliye planı kurduğunu
anlatır. Sonuç ekranı gerçek olay zamanlarını gösterir. HUD hangi koşulun
eksik olduğunu, Echo'nun sıradaki görevini ve R'nin değiştireceği kaydı açıklar.

Yeni kayıtların v2 şeması, checkpoint/death/tapes, bozuk dosyadan yedek ve eski
v1 aktarımı denetlenir. Güncel teknik sonuçlar VALIDATION_TR.md'de.
İnsan değerlendirmesi henüz yoktur; 8–12 dakika hedefi ölçülmüş değildir.
3–5 yeni oyuncu için yönerge/form INSAN_DENEMESI_TR.md'de hazırdır.
6–8 bölüm fikri sonuçlardan sonra seçilecek; mevcut örnek topluca çoğaltılmadı.

---
Önceki kısa demo kapsamı (karşılaştırma için korunur):

# Echo Protocol — dönem projesi özeti

**Tür:** 3D birinci şahıs FPS / Action, zaman döngüsü bulmacaları.

Oyuncu, kapalı bir laboratuvarın üç sektöründen çıkmaya çalışır. Her 20 saniyelik
döngüde hareketleri, konsol etkileşimleri ve ateşi kaydedilir. Zamanı başa aldığında
önceki kaydı bir Echo karakteri tekrar oynatır. En fazla iki Echo ile oyuncu
kendi geçmişini takım arkadaşı gibi kullanır.

İlk bölüm konsol/plaka mekaniğini öğretir. İkinci bölüm tek silah ve devriye
muhafızıyla çatışmayı ekler. Son bölümde iki farklı konsol ve plaka aynı anda
tutulmalıdır; Echo'lar muhafızlara ateş ederken oyuncu çıkışa ilerler.

**Tamamlanan kapsam:** üç kısa bölüm, tek hitscan silah, tek düşman türü,
oyuncu/düşman canı, kayıtlı ateşin yeniden uygulanması, tutarlı dünya reseti,
ana menü, kontroller, duraklatma, yeniden deneme ve kazanma/kaybetme ekranları.
Ayrıca kalıcı fare/ses/görüş ayarları, tamamlanmış Echo kayıtlarıyla devam,
zamanı durduran planlama haritası, bölüm seçimi ve kişisel rekorlar vardır.
Her sektörde Echo plakaları tutarken altı saniyelik rezonans penceresinde
açılan isteğe bağlı bellek kasası, kalıcı üç parçalık hikâye ve arşiv menüsü
eklendi. Rezonatör atışı yakın muhafızları geçici durdurur ve oyuncuyu iyileştirir;
Echo aynı atışı tekrarlayabilir. Üst zayıf nokta vuruşu daha çok hasar verir.
Cam gözlem alanları, büyük reaktör manzarası, sis, ışık/gölge, yeni silah
ve geri sarma sesleri, kıvılcımlar ve kamera efektleri atmosferi güçlendirir.
Ölümde Echo kayıtları korunur; oyuncu hatalı kaydı Q ile geri alabilir.

**Teknoloji:** Unity 2021.3.21f1, C#, Built-in render pipeline, Windows x64.
Oyun mevcut teknik prototip üzerinden geliştirildi. Kayıt çekirdeği Unity'den
bağımsızdır; görseller CC0 modüler bilimkurgu assetleriyle kurulur, kısa sesler kodla üretilir.
Araştırma laboratuvarı, bakım hattı ve reaktör farklı dekor, renk ve rota kullanır.
Robot karakterler animasyonludur; koşu, nişan, son Echo’yu geri alma ve görev
işaretleri oynanabilirliği destekler. Asset kaynakları ASSET_CREDITS_TR.md’de.

**Kanıt:** bağımsız kayıt kontrolleri 14/14; Unity Play Mode ve gerçek Windows
uygulamasında 202/202 canlı kontrol. Üç bölümün çıkış koşulları, Echo ateşi,
iki Echo sınırı, reset, siperler, ölüm/yeniden deneme ve duraklatma çalıştırıldı.
Ek 25 kayıt regresyon kontrolünde 1.000 üretilmiş kayıt senaryosu doğrulandı;
gerçek kontrol işleme yolu üzerinden üç bölümün tamamı çözüldü.
Görüntüler ve ayrıntılar VALIDATION_TR.md'de. Otomatik rotalar klavye/fareyle
uzun bir kullanıcı oynama seansı olarak sunulmaz.

