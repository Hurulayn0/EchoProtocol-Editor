# Bakım Hattı / Eşzamanlılık — uygulama tasarımı

Tarih: 10 Ekim 2026. Unity 2021.3.21f1 / Built-in / Windows x64.
Başlangıç noktası: `verified-demo-20261009`, commit
`e9ac584e3e5d401d8bcf64346508d90086ef2857`. Mevcut kayıt çekirdeği korunur.

Bu belge sahne üretiminden önce yazıldı. Aşağıdaki süreler tasarım tahminidir;
gerçek CharacterController çözüm kayıtlarının süreleri doğrulama raporunda ayrıca
verilir. İlk kez oynama hedefi 8–12 dakika; insan denemesi henüz yapılmadı.

Hikâye katkısı: **Bakım tesisi çalışanları, sistemin onları kendi geçmişleriyle
işbirliği yapmaya zorladığını fark etmiş ve bu özelliği tahliye için kullanmıştır.**

Tek bir örnek bölüm, üç bağımsız kontrol noktası içerir. İki Echo yeterlidir.
Eski üç oda korunur ve karşılaştırma için ana menüden erişilir. Yeni bölüm ilk
önerilen menü seçeneğidir. Yeni mekanikler önce başka bölümlere çoğaltılmaz.

## 1. Servis geçidi / güvenli keşif

1. İlk problem: karşıdaki yük asansörüne ulaş; kapıyı açan servis düğmesi başka
   tarafta. Düğme kapıyı yalnızca 1,4 saniye açar; muhafız yoktur.
2. Öğrenme: E ile yaptığın kısa bir işlemi R ile kaydet; Echo yürüyüp düğmeye
   ulaştığında sen kapı tarafında olabilirsin. Son konumda beklemek çözüm değildir.
3. Yerleşim: başlangıç `(3.8,0,-4)`, servis düğmesi `(-5,1.2,-3)`; geçit
   `(5,2,3.5)`, yük asansörü `(5,0,6)`. Bölme duvarı yalnızca sağdaki geçidi açık
   bırakır. İki hedef başlangıçtan görülebilir; zemindeki kablo ikisini bağlar.
4. Kayıt 1: yaklaşık 0–1,7 sn sola koş, 1,8 sn düğmeye E, 2 sn R.
5. İkinci kayıt gerekmez. Daha geç E içeren kayıt oyuncuya daha geniş hazırlanma
   payı verir; Q ile yanlış kayıt geri alınabilir.
6. Son döngü: 0–1,6 sn sağ geçide koş; Echo yaklaşık 1,8 sn'de açar; 2–3 sn
   aralığında fiziksel kapıdan geçip asansöre ulaş.
7. Gerekçe: düğme–kapı arası düz mesafe yaklaşık 12 m; 6 m/s koşuda bile en az
   2 sn. Açıklık 1,4 sn olduğundan oyuncunun iki işi tek başına yapması yetmez.
8. Alternatif: Echo atış yapmadan farklı rotadan düğmeye gider; oyuncu kapıda
   bekler. Kayıt zamanı serbesttir, tek bir ezberlenmiş saniye gerekmez.
9. Hatalar: yanlış yöne bakmak, düğmeyi kaydetmemek veya geç kalmak. Düğme
   etkileşimi, kalan kapı süresi ve Echo'nun E zamanı gösterilir. Süre biterse
   kapı kapanır; R yeni deneme, Q son kaydı kaldırır.
10. Güvence: bölme 4 m yüksek; zıplama duvarı aşamaz. Kapı eşiğinde duran oyuncu
    ezilmez; kapanma güvenliği eşiği boşalana kadar kapıyı tutar. Başarısızlık
    kayıtları kilitlemez. Asansöre ulaşınca kalıcı kontrol noktası alınır ve bu
    kısım bir daha zorunlu oynatılmaz; Echo'lar yeni odanın koordinatlarına taşınmaz.

## 2. Atış penceresi / bir Echo, iki görev

1. İlk problem: enerjisiz aktarım köprüsü. Koruma kafesinin içindeki rezonatörü
   vurmak ve üst/alt iki servis rölesini aynı enerji penceresinde etkinleştirmek.
2. Öğrenme: Echo bir süre sensörde durabilir, ardından sensörü bırakıp başka
   göreve ilerleyebilir. Kaydedilmiş başarısız E denemesi de yeni dünyada
   koşullar doğru olduğunda başarılı olabilir; niyet kaydedilir, sonuç değil.
3. Yerleşim: başlangıç `(0,0,-7)`; lens sensörü `(-4,0,-2)`, atış noktası
   `(0,0,-2)`, korumalı rezonatör `(0,1.35,4)`. Kafesin önü z=2, yalnızca
   0,4 m genişliğinde bir atış yarığı var; oyuncu içeri yürüyemez. Yan/arka
   yüzler ışını keser. Sol rampa üst röleye `(-5,2.2,4.8)`; sağ yol alt röleye
   `(5,1.2,4.8)` gider. Kafes kenarları yürüyüş için açıktır.
4. Kayıt 1: 0–2 sn sensöre ulaş; 2–6,5 sn sensörde kal; 6,5–8,5 sn sol rampadan
   üst röleye git, E denemesini kaydet; yaklaşık 9 sn R. İlk döngüde enerji
   yoksa röle çalışmaz ama E denemesi kayda girer. Sensör bırakılınca yarık kapanır.
5. Kayıt 2: 0–1 sn atış noktasına git; Echo'nun sensörde olduğu anda yaklaşık
   5,5 sn rezonatörü vur; sağ röleye koşup yaklaşık 8 sn E; 9 sn R.
6. Son döngü: oyuncu köprü girişine hazırlanır. Echo 1 yarığı açar, Echo 2
   ateş eder; Echo 1 üst röleye ilerlerken Echo 2 alt röleyi etkinleştirir.
   Yaklaşık 8,5–11,5 sn'de köprü açıktır; oyuncu karşıya geçer.
7. Gerekçe: sensördeki aktörün açtığı atış hattı farklı aktörün atışını mümkün
   kılar; sensörün ardından üst röleye yürümek tek kayıtta iki farklı görevdir.
8. Alternatif: bir Echo sensörü tutarken gerçek oyuncu ateşi ve iki röleyi
   gerçekleştirir. Daha fazla hareket ve hassas zamanlama gerektirir; kabul edilir.
9. Hatalar: Echo sensörden ayrıldıktan sonra ateş etmek, atış hattına girmek,
   röleyi enerji gelmeden/enerji bittikten sonra denemek. HUD lens/enerji/röle
   durumunu ve kapalı köprü nedenini ayrı gösterir; engellenen Echo atışı bildirilir.
10. Güvence: her R'de sensör, röleler ve rezonatör sıfırlanır; kayıtlar korunur.
    Kapalı kafesin içinden etkileşim veya ateş yoktur. Üst yolun korkulukları
    rotayı okunur kılar; aşağı düşmek zemine indirir, çözümsüz yer bırakmaz.
    Aktarım bitince bu bağımsız problem kalıcı tamamlanır.

## 3. Tahliye / planlanmış çatışma ve isteğe bağlı gözlem

1. İlk problem: uzaktan görülen tahliye kapısı kilitlidir. Operatör ekranı
   "sol röle + muhafızın durdurulması" koşullarını gösterir.
2. Öğrenme: geçmişteki hareketin muhafızın dikkatini çekerken farklı açıdan
   ilerle; bir Echo planlanmış rezonans atışından sonra operatör işini de yapabilir.
3. Yerleşim: başlangıç `(0,0,-7)` ve önünde görüşü kesen makine. Sol alçak yol
   yem alanına `(-4,0,0)` ve servis rölesine `(-4,1.2,4.8)` gider. Sağ rampalı
   yol üst operatör paneline `(5,2.2,4.8)` çıkar. Muhafız `(0,0,2)`; rezonatör
   `(0,1.35,4)`. Sol üst cam odada gözlem belleği `(-5,2.2,4.8)` vardır.
4. Kayıt 1: 0–2 sn sol siperden yem alanına; yaklaşık 2–5 sn görünür kalıp
   muhafızın lazerini kendine çek; 5–6 sn sol röleye git ve E; 6,5 sn R.
5. Kayıt 2: sağ yoldan operatör katına çık; Echo 1 sol röleyi açtıktan sonra
   yaklaşık 7–8 sn rezonatörü vur ve operatöre E; 8,5 sn R. E başarısızsa
   deneme kaydedilir, bir sonraki döngüde tekrar uygulanır.
6. Son döngü: iki Echo planı gerçekleştirirken oyuncu tahliyeye veya sol üst
   cam odaya ilerler. Operatör işi tamamlanınca tahliye o döngü için açık kalır;
   oyuncunun son iki saniyeye yetişmek zorunda olması gerekmez. Fiziksel çıkış
   finali başlatır; enerji odağı aydınlanır ve yapılan planın olayları görünür.
7. Gerekçe: Echo canlı olmayan ama görünür bir hedef olarak lazeri üzerine çeker;
   gerçek oyuncu diğer yolu güvenle kullanır. Rezonansın zamanı operatörün
   çalışmasını belirler; öldürme ana bulmacadan bağımsız bir görev değildir.
8. Alternatif: muhafızı vurup sol röle/operatörü tamamlamak geçerli bir taktik
   çözümüdür. Bir Echo + gerçek oyuncunun rezonans/operatör işi de kabul edilir.
9. Hatalar: yem kaydı görünmez kalırsa muhafız gerçek oyuncuyu seçer; operatör
   erken E alırsa sol röle veya sersemletme eksikliği gösterilir. Ölüm mevcut
   kontrol noktasını ve Echo'ları korur. Kırmızı lazer hedefi görünür kılar.
10. Güvence: tahliye anahtarları her döngüde yenilenir; öldürme/sersemletme iki
    geçerli yol sağlar. Başarı bağımsız kontrolleri yeniden oynatmaz. İki Echo
    doluyken R'nin çıkaracağı kayıt önceden gösterilir; Q/X ile plan değişir.

İsteğe bağlı bellek: muhafızın en az bir kez Echo'ya ateş ettiğini gözlemle,
rezonans penceresi açıkken sol üst cam terminalini oku. Plaka/konsol bulmacasını
tekrar etmek yerine düşmanın hedef seçimini başka amaçla kullanır. Muhafızı
önce öldürmek bu gözlem verisini üretmez; yeni döngüde farklı plan denenebilir.
Bellek kalıcıdır; ana çıkış için zorunlu değildir.

## Kayıt, kontrol noktası ve teknik kabul

- Örnek bölümün veri düzeni v2, eski kampanya v1. Yeni dosya
  `echo-session-v2.json`; eski dosya silinmez. V1 kampanya kaydı güvenli içe
  aktarılır; yeni Device olayları yanlış sahnede oynatılmaz.
- Kontrol noktası: odanın çözümü tamamlandığında sonraki oda numarası ve
  tamamlanma bitleri saklanır, yerel Echo'lar temizlenir. R/ölüm mevcut odada
  kayıtları korur; X yalnızca mevcut odanın planını temizler.
- Önce eski 202 kontrol çalışacak, sonra gerçek controller ile yeni çözümler,
  başarısız atış/etkileşim, zaman pencereleri, ölüm, v2 dosyası ve kontrol noktası
  yüklemesi kontrol edilecek. Paket ayrıca ayrı klasörden çalıştırılacak.
- F6 gerçek kullanıcı dosyalarına yazmayacak. İnsan sonuçları üretilmeyecek;
  3–5 yeni oyuncu için form ve kısa yönerge hazırlanacak.
