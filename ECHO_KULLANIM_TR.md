# Echo kullanımını anlaşılır hale getirme — ilk düzeltme

10 Ekim 2026. Başlangıç Git noktası: `pre-echo-controls-20261010` (`2fe97cd`).

Oyuncunun gerçek geri bildirimi: “Echo’yu nasıl kontrol edeceğimi bile anlamadım;
bazen bir şeylere basıyorum ve hareket ediyor gibi.” Bu, tamamlanmış bir insan
oynama denemesi değildir; ilk kullanımda yaşanan somut bir karışıklıktır.

## Kullanım

- Echo’ya yön veya hedef komutu verilmez. Beyaz oyuncu senin şu anki hareketindir.
- Önce kendin hareket et, E ile etkileş veya ateş et. Bunlar mevcut denemeye kaydolur.
- R ile bu denemeyi bir Echo’ya dönüştür. Zaman başa döner; camgöbeği Echo 1 ve
  mor Echo 2 aynı hareketleri ve olayları otomatik tekrarlar. Sen farklı işe gidersin.
- T: mevcut Echo’larla yeniden başla. Bitmemiş deneme bırakılır; kayıtlar korunur.
- Tab: zaman durur. Rotaları, olay saatlerini ve 0–20 saniye konumlarını incele.
  Kaydırıcı yalnızca harita önizlemesidir; kapı, muhafız, atış veya etkileşim çalıştırmaz.
- İki Echo doluyken R bir seçim ekranı açar. Hangisinin yerini alacağını seç.
  Diğer Echo korunur. Tab ile vazgeçince mevcut deneme aynen devam eder.
- Tab içindeki “Sonraki R / bunu değiştir” seçimi yalnızca bu döngü içindir.
  Seçilen mevcut Echo bu sırada oynamaya devam eder. R yeni rotayı onun yerine
  yazar; T, ölüm, kontrol noktası veya yükleme seçimi iptal eder.
- 20 saniye dolunca boş yuva varsa yeni Echo kendiliğinden oluşmaz. Oyun durur;
  kaydı saklamayı seçebilir veya T / Tab ile bu denemeyi bırakabilirsin. İki Echo varsa
  süre sonunda ikisi korunarak yeni deneme başlar.
- Q son sıradaki Echo’yu siler; X bu odanın tüm Echo’larını siler.
  Bu tuşlar tekrar deneme tuşu değildir. Bağımsız tamamlanmış odalar korunur.

## İlk bölümde öğretilen fark

Oyun kısa, duraklatılmış bir üç adım ekranıyla açılır. Oyuncu hazır olmadan döngü
başlamaz. Güvenli servis odasında oyuncu panelde yaptığı E işlemini kaydeder;
bir sonraki denemede kendisi geçide giderken Echo paneli kullanır. Ekran canlı
oyuncunun kaydedildiğini, renkli karakterin geçmişi oynadığını ayrı ayrı söyler.
Echo üstündeki etiket ve görev kartı karakterin ne yaptığını tanımlar.

## Sonraki tasarım kararları

10 Ekim ortak çatışma güncellemesi: kayıtlı atıştan önceki bir saniyede
Echo'nun renkli atış hattı ve geri sayımı görünür. Bu önizleme ateş etmez;
çizgi mevcut dünyadaki ilk temasta biter. Muhafızın göstergesi artık son
ateşinden kalmış bilgi yerine şu an kimi hedeflediğini anlatır. Final odasında
farklı aktörlerin 1,2 saniye içinde aynı muhafıza isabeti +10 hasar verir.
İki Echo da eşlik edebilir. Aynı kişiyle art arda atış bonus oluşturmaz.
Atışın önünde durmak isabeti engeller; oyun bunu bildirir. Yeni tuş gerekmez.
Yazılı ürün yönü ve somut alternatif akış OYUN_VIZYONU_TR.md'dedir.

Daha fazla tuş ve karakter eklemek bu sorunu çözmez. İlk insan denemesinde şu
soruların anlaşılması ölçülecek: “Kim şu anda benim kontrolümde?”, “R neyi
saklar?”, “T ile R’nin farkı ne?”, “Echo ne zaman E yapacak?”

Anlaşılırlık doğrulandıktan sonra şu fikirler sırayla değerlendirilecek:

1. **Görünür fırsat anı:** Echo’nun panel E’sinden hemen önce hedef üzerinde
   kısa bir geri sayım; açılan yolun görsel ve sesli cevabı. Oyuncu bir sayı
   ezberlemek yerine arkadaşının oluşturduğu fırsatı okuyabilsin.
2. **Bir kayıt, iki görev:** lensi tutan Echo’nun üst röleye geçişi bölümde zaten
   var. Bu geçişin görev kartında neden ve sonuçla okunması güçlendirilecek.
3. **Geçmişin yarattığı taktik:** muhafızın güncel Echo hedefi ve yaklaşan
   kayıtlı atış artık görünür. Sonraki adım bu göstergelerin insan denemesinde
   anlaşılabilirliğini ve farklı açıdan ortak ateşin keyfini değerlendirmektir.

Zaman kaydırma ile gerçek dünyayı simüle eden bir prova, kaydın bir bölümünü
kesme ve otomatik hedef atama henüz uygulanmadı. Bunlar kayıt/reset/yükleme
anlamını değiştirir; mevcut çekirdek üzerinde ayrıca tasarlanıp doğrulanmalıdır.
Bu ilk düzeltme kaydedilmiş geçmişle işbirliği deneyimini korur.

## İnsan değerlendirmesi

Bu değişikliklerin kolaylık veya etkileyicilik sağladığı henüz bağımsız insan
oynamasıyla doğrulanmadı. INSAN_DENEMESI_TR.md formuyla 3–5 yeni oyuncuda dene.
Çözümü göstermeden önce oyuncudan bir Echo oluşturmasını ve onunla farklı
bir görevi paylaşmasını iste. İlk bölümde amaç anlaşılmazsa bölüm sayısını
artırmadan öğretim ve geri bildirimi düzelt.
