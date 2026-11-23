# Echo Protocol — Bakım Hattı / Eşzamanlılık

Son görsel sanat geçişi: metal kargo kasalarından siper, duvara yerleşik
tesis tabelaları, depolama prop'ları, yerel gölgeli iş ışıkları ve daha hafif
HUD panelleri. Güncel Unity/Windows doğrulaması 311/311 geçti; insan denemesi
henüz yapılmadı. Ayrıntılar: [VISUALS_2026-10-26_TR.md](VISUALS_2026-10-26_TR.md).

26 Ekim doğrulamasında yön bulma ipuçları iyileştirildi: servis/üst röle
kehribar, alt röle mavi, tahliye operatörü camgökyüzü, bellek terminali mor.
Bu renk dili sahneyle birlikte duraklatılmış rota haritasında ve hedef
göstergelerinde de kullanılır. Güncel teknik sonuçlar
`Verification/Results/2026-10-26/EchoWayfinding/` altındadır; insan oynama
sonucu değildir. Rapor: [VALIDATION_2026-10-26_WAYFINDING_TR.md](VALIDATION_2026-10-26_WAYFINDING_TR.md).

Mevcut teknik prototip, kayıt sistemi korunarak üç kısa bölümlü bir oyuna
genişletildi. Oyuncunun hareketi, konsol etkileşimleri ve ateşi kaydedilir.
Zaman sıfırlanınca Echo'lar bu hareketleri ve eylemleri yeniden uygular.

**Hedef: Windows x64 için indirilebilir, bulmaca ve çatışmayı dengeli birleştiren özgün bir FPS.**

Ders projesi kapsam sınırı kaldırıldı. Ürün yönü ve aşamalı kalite hedefleri
OYUN_VIZYONU_TR.md'de. Bakım Hattı finalinde kayıtlı Echo atışına eşlik etmek
artık eşzamanlı vuruş bonusu verir. Bir saniyelik atış önizlemesi, muhafızın
güncel hedefi ve isabet penceresi oyuncuya planını sahnede okumayı sağlar.

10 Ekim örnek bölümü **Bakım Hattı / Eşzamanlılık**, üç bağımsız kontrol
noktası içerir. İlk güvenli problemde geçmişteki E işlemi kısa kapıyı açar.
İkinci problemde Echo sensörde bekleyip üst röleye ilerler; başka bir kayıt
açılan atış hattından rezonatörü vurur ve alt röleyi kullanır. Finalde Echo
muhafızı oyalar; zamanlanmış rezonans ve operatör işlemi tahliyeyi açar.
Muhafızı öldürmek ana çözümün zorunluluğu değildir. İsteğe bağlı gözlem
belleği, muhafızın Echo'ya yönelmesi ve rezonans penceresiyle toplanır.

Örnek bölüm ana menüde **ÖRNEK BÖLÜMÜ OYNA** seçeneğidir. **İLK DEMO / ENTER**
ve sağ sektör kartları eski üç odayı karşılaştırma için açar.
İki Echo / 20 saniye korunur; üçüncü Echo veya 6–8 bölüm henüz uygulanmadı.
8–12 dakika ilk oynama hedefidir; insan denemesi yapılmadı ve süre ölçülmedi.
Somut çözüm ve 10 maddelik bulmaca tasarımları ORNEK_BOLUM_TASARIM_TR.md'de;
3–5 kişi için çözümü açıklamayan yönerge/form INSAN_DENEMESI_TR.md'de.
Sonraki fikirler BOLUM_GENISLETME_ADAYLARI_TR.md'de, insan sonuçlarına bağlıdır.

Örnek bölüm kayıtları v2 şeması / Layout 1 ile echo-session-v2.json dosyasına
yazılır. Eski v1 dosyası değiştirilmeden bir kez içe alınır. Kontrol noktasında
önceki bağımsız başarı korunur, odanın yerel Echo'ları temizlenir. Ölüm/T mevcut
odanın planını korur; X ve duraklatma menüsündeki yeniden başlatma yerel planı
temizler. DEVAM tamamlanmış Echo'larla yeni döngüden başlar; yarım kayıt yüklenmez.
R yeni Echo oluşturur veya seçtiğin Echo'yu değiştirir; diğerini korur.
Yeni etkileşim Device=3'tür; kayıt çekirdeği değiştirilmedi.
Bu sürümün yeni oynanış ve atmosfer notları `IMMERSION_TR.md` içindedir.

**Doğrulanan editör: Unity 2021.3.21f1 / Built-in render pipeline.**
Bağımsız kayıt kontrolleri 14/14; canlı Unity ve Windows uygulaması kontrolleri
311/311. Güncel sonuçlar VALIDATION_2026-10-10_TACTICS_TR.md'de.

Ek kayıt regresyonu: 25/25 kontrol ve 1.000 üretilmiş kayıt senaryosu.

## Hemen oyna

- `Builds/EchoProtocol/EchoProtocol.exe` dosyasını aç.
- **ÖRNEK BÖLÜMÜ OYNA** ile Bakım Hattı’nı başlat. ENTER eski demoyu açar.
- İlk kısa öğretimde süre durur; Enter ile başla. Echo'ya komut vermezsin:
  önce sen rotayı yaparsın, R'den sonra Echo aynılarını otomatik tekrarlar.
- **T** mevcut Echo'larla başa döner. **Tab** rota/olay önizlemesini açar.
- İki kayıt doluyken **R** hangi Echo'nun değişeceğini sorar; **Tab** ile iptal et.
- Boş yuva varken 20 saniye dolunca kayıt kendiliğinden Echo'ya dönüşmez;
  saklamayı seç veya **T / Tab** ile denemeyi bırak.
- Kullanım ayrıntıları ECHO_KULLANIM_TR.md'de.
- **KONTROLLER** düğmesi kısa oyun rehberini açar.
- **F6** veya **OTOMATİK TEST** üç bölümün otomatik doğrulamasını çalıştırır.
  İlk insan denemesinden sonra kullan; çözümü gösterebilir. Test bitince ana menüye döner; beklenen sonuç **PASS: 311/311 Unity game checks**.
- EXE'nin yanındaki `EchoProtocol_Data`, `MonoBleedingEdge` ve DLL dosyalarını
  ayırma. Paylaşırken tüm klasörü veya `Deliverables/EchoProtocol_Windows.zip`
  paketini kullan.
- Uygulamayı menüden ÇIKIŞ veya Alt+F4 ile kapat.

## Kontroller

| Girdi | İşlev |
| --- | --- |
| WASD / fare | Hareket / bakış |
| Sol fare tuşunu basılı tut | Pulse carbine ile ateş; sınırsız enerji |
| Space | Zıplama |
| Shift | Koşma (6 m/s) |
| Sağ fare tuşunu basılı tut | Nişan: yakınlaştırma ve daha hassas bakış |
| Q | Son Echo kaydını sil; varsa diğer kaydı koru |
| E | Yakındaki ve nişan alınan konsolu veya bellek kasasını kullan |
| R | Mevcut rotayı Echo olarak sakla ve yeni döngüyü başlat; iki yuva doluysa değişecek Echo’yu seç |
| T | Bakım Hattı'nda mevcut Echo kayıtlarını koruyarak döngüyü yeniden başlat |
| X | Bu alanın Echo kayıtlarını silip alanı sıfırla; tamamlanan kontrol noktaları korunur |
| Tab | Zamanı durdur, kayıtlı rota konumları/olayları incele; kayıt seçimini iptal et |
| Escape | Duraklat / devam et |
| Enter | Menüde başlat; bölüm sonunda devam et; ölümde yeniden dene |
| F6 | Otomatik oyun testi |
| Alt+F4 | Windows uygulamasını kapat |

Başka pencereye geçince oyun duraklar. Devam etmek için menüde DEVAM ET'i seç.

## Ayarlar ve bölümden devam etme

Ana menüden veya duraklatma ekranından **AYARLAR** açılır. Fare hassasiyeti,
dikey fare yönü, ses seviyesi ve görüş açısı değiştirilebilir; **KAYDET VE GERİ
DÖN / ESC** ile seçenekler cihazda saklanır. Ayarlar ekranında oyun durur.

Yeni oyun ilk sektörü kaydeder; sektör tamamlanınca sonraki sektör bir devam
noktası olur. Oyunu kapatıp açınca **DEVAM / SEKTÖR N** bu noktadan başlar.
Tamamlanmış Echo kayıtları hareket, bakış, konsol etkileşimi ve ateş olaylarıyla
dosyaya yazılır. **DEVAM**, bu Echo’ları koruyarak 100 canla yeni döngüden başlar.
Henüz R ile tamamlanmamış deneme kaldığı saniyeden yüklenmez.
**İLK DEMO / ENTER** eski ilk sektöre dönüp devam noktasını değiştirir.
**ÖRNEK BÖLÜMÜ OYNA** yeni bakım hattını başlatır; bağımsız kontrol noktaları kullanır.

Güncel kayıt dosyası `Application.persistentDataPath/echo-session-v2.json` konumundadır;
önceki geçerli kayıt `.bak` yedeğinde tutulur. Windows’ta varsayılan klasör:
`%USERPROFILE%/AppData/LocalLow/IKU Student Project/Echo Protocol/`.
Kayıt tamamlandığında, duraklatınca, ana menüye dönünce ve normal çıkışta kaydedilir.

Ana menüde açılmış sektörleri tek tek tekrar oynayabilirsin. En iyi süre ve
en az kayıt sayısı cihazda saklanır. Bölüm seçimi mevcut devam denemesini değiştirir.
F6 ve komut satırı testleri oyuncunun gerçek bölüm kaydını değiştirmez.

HUD güç ve plaka durumuna göre çözüm ipucu gösterir. Son beş saniyede otomatik
kayıt uyarısı gelir. Son bölümde A/B cihazlarının ayrı durumları da görünür.

## Üç kısa bölüm

| Bölüm | İçerik | Çözüm fikri |
| --- | --- | --- |
| 01 / Araştırma laboratuvarı | Bir konsol, bir plaka | Konsolu aç; plakada kalırken R bas; Echo kapıyı açsın |
| 02 / Bakım hattı | Bir muhafız, bir konsol, siperler | Muhafızı vur; konsol/plaka çözümünü kaydet; Echo ateşini ve etkileşimini tekrar etsin |
| 03 / Reaktör | İki muhafız, iki konsol ve plaka | Sol ve sağ plakada ayrı Echo'lar bırak; muhafızları temizle; iki Echo kapıyı birlikte açsın |

Bölüm sonunda **SONRAKİ SEKTÖR**, son bölümde **PROTOCOL VERIFIED** ekranı gelir.
Bölüm değişince eski kayıtlar temizlenir. Çıkışa ulaşmak için o bölümün tüm
muhafızlarının da etkisiz hale gelmesi gerekir.

## Rezonans ve isteğe bağlı bellek kasaları

Her sektörde parlayan bir rezonatör ve bir bellek kasası var. Küreye ateş etmek
altı saniyelik kasa penceresi açar, altı metre içindeki muhafızları iki saniye
durdurur ve yakındaki oyuncuya en fazla 20 can kazandırır. Bir döngüde yalnızca
bir kez tetiklenir; Echo'nun kaydedilmiş atışı da küreyi çalıştırabilir.

Kasa için tüm konsolların gücü açık ve bütün plakalar **Echo'lar tarafından**
tutuluyor olmalı. Rezonatörü vurduktan sonra altı saniye içinde terminale
nişan alıp **E**'ye bas. Oyuncunun plakada durması tek başına bu yan bulmacayı
çözmez. Başaramazsan sonraki döngüde yeniden deneyebilirsin; ana çıkış için
kasayı açmak zorunlu değildir. Her sektördeki hikâye kaydı bir kez toplanır ve
ana menüdeki **BELLEK ARŞİVİ**'nde saklanır. R, X ve yeni oyun bellekleri silmez.
Tab haritasında P rezonatörü, V kasayı gösterir.

## Kayıt ve reset kuralları

- Döngü **20 saniye**. R daha erken tamamlar. Bakım hattında boş yuva varsa süre sonunda kaydı saklayıp saklamayacağını seçersin. İki yuva doluysa mevcut iki Echo korunarak yeni döngü başlar. Eski karşılaştırma demosunda boş yuvaya otomatik kayıt davranışı korunur.
- En fazla **iki Echo**. Bakım hattında dolu planda R hangi Echo’nun değişeceğini sorar. Tab içinden sonraki R için bir yuva seçebilirsin. Q son sıradaki yuvayı siler (iki Echo varsa Echo 2); en son değiştirdiğin yuvayı otomatik bulmaz. Eski karşılaştırma demosunda R en eski kaydı çıkarır.
- Echo tekrar sonunda son konumunda bekler; oyuncuya fiziksel engel olmaz.
- R güç, kapı, oyuncu canı, muhafız canı/konumu ve silah bekleme süresini sıfırlar.
  Tamamlanan kayıtlar korunur. Ölümde **YENİDEN DENE** Echo’ları korur.
  X ve duraklatma menüsündeki **SEKTÖRÜ BAŞTAN AL** kayıtları temizler.
- Bir ateş olayı ray başlangıcını ve yönünü saklar. Echo kaydedilmiş hasar sonucunu
  kopyalamaz; yeni dünyanın gerçek collider'larına yeniden raycast yapar.
- Tek silah **Pulse carbine**: 25 hasar, 0,24 saniye bekleme, sınırsız enerji.
  Muhafız 60 cana sahiptir; üç gövde veya iki üst zayıf nokta isabetiyle düşer. Zayıf nokta 50 hasar verir ve saldırıyı keser.
- Tek düşman türü **devriye muhafızı**. Yakındaki oyuncu/Echo
  hedeflerinden görüş hattı açık olan en yakına yönelir. Yakın ve görünür Echo
  ateşi üzerine çekebilir; siperler iki tarafın ışınlarını keser. Siper arkasındaki
  bir Echo, görünür oyuncuyu muhafızdan saklamaz.
- Oyuncu 100 canla başlar. Can sıfırlandığında kaybetme ekranı açılır;
  **YENİDEN DENE** mevcut bölümü korunmuş Echo’larla yeni döngüden başlatır.

## Unity'de açma

1. Unity Hub'a Assets, Packages ve ProjectSettings içeren bu EchoProtocol
   klasörünü ekle.
2. Unity **2021.3.21f1** ile aç. Unity 6'da ayrıca çalıştırma yapılmadı.
3. **Echo Protocol → Open Prototype** menüsünü seç.
4. Play'e bas. Oda ve nesneler çalışma sırasında oluşturulur; editörde sahnenin
   boş görünmesi normaldir.
5. F6 ile 311 kontrolü veya **Echo Protocol → Run Timeline Checks** ile 14 kayıt
   kontrolünü çalıştır.

Başka render pipeline'a import edilerek doğrulanmadı. Giriş ayarı
Input Manager (Old); kod yeni Input System'e ilişkin koşullu yolu da korur.

## Kaynak düzeni

- `ReplayTimeline.cs`, `TimelineSelfChecks.cs`: korunmuş bağımsız kayıt sistemi ve 14 kontrol.
- `EchoPrototypeGame.cs`: mevcut oyun çekirdeği; kayıt, reset, Echo, bölüm ve ekran durumu.
- `EchoAction.cs`: konsol, ateş ışını ve bellek kasası olayları.
- `EchoLevelDefinition.cs`: üç bölümün cihaz, muhafız ve siper yerleşimleri.
- `EchoGameWorld.cs`: oda, materyaller, oyuncu ve Echo görselleri.
- `EchoGameCombat.cs`, `EchoEnemy.cs`: tek silah, hasar, muhafız davranışı ve üretilmiş sesler.
- `EchoGameUI.cs`: ana menü, kontroller, HUD, duraklatma, bölüm sonu, kazanma ve kaybetme.
- `EchoGameChecks.cs`: çekirdek prototipin altı kontrolüyle başlayan 311 canlı kontrol.
- `EchoTactics.cs`, `EchoTacticalChecks.cs`: atış önizlemesi, ortak isabet ve 24 yeni kontrol.
- `EchoDetailedChecks.cs`: ortak kontrol yolu üzerinden hareket/zıplama, üç bölümün
  tamamı, ayarlar, kayıt sınırları, iki Echo ateşi ve 45 sektör / 180 reset yük testi.
- `EchoPreferences.cs`: ayarlar, sektör devam noktası, bölüm kilitleri ve kişisel rekorlar.
- `EchoSession.cs`, `EchoSaveStore.cs`: tamamlanmış Echo’ların dosyaya kaydı ve geri yüklenmesi.
- `EchoPlanningMap.cs`: zamanı durduran rota haritası.
- `EchoSessionChecks.cs`: 36 yeni kayıt/geri yükleme, harita, ölüm ve tekrar oynama kontrolü.
- `Editor/EchoPlayValidation.cs`: Play Mode testi, görüntü kaydı ve Windows build.
- `Resources/EchoShaders`: build'de shader varyantlarını koruyan materyaller.
- `EchoResonance.cs`: her sektörde altı saniyelik yan bulmaca ve kalıcı bellek arşivi.
- `EchoImmersion.cs`, `EchoCameraFinish.cs`: yeni ortam, cam, parçacık, ses ve kamera efektleri.
- `EchoImmersionChecks.cs`: 45 yeni canlı çatışma, rezonans ve bellek kontrolü.
- `Verification/Results/2026-10-10/EchoTactics`: güncel testler ve gerçek Windows görüntüleri.
- `Verification/Results/2026-10-09/Immersion`: önceki atmosfer sürümünün kanıtları.
- `Verification/Results/2026-10-09/WindowsDemo`: önceki 157 kontrollü sürüm.
- `Verification/Results/2026-10-08/Campaign`: önceki sürüm sonuçları ve ekran görüntüleri.
- `Verification/Results/2026-10-08/Hardening`: son ayrıntılı sürümün sonuçları;
  Campaign klasörü önceki 34 kontrollü sürümün geçmiş kanıtıdır.

Ortam, robot ve silah modelleri Quaternius CC0 assetlerinden kurulur; kaynaklar
ve lisanslar ASSET_CREDITS_TR.md içindedir. Kısa sesler kodla üretilir.
Ücretli asset veya çevrimiçi oyun servisi bağımlılığı yoktur.

## Teslim dosyaları

- `Deliverables/EchoProtocol_Ortak_Catisma_2026-10-10.zip`: güncel paylaşılabilir oyun ve Türkçe rehberler.
- `Deliverables/EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip`: korunmuş eski demo.
- `Deliverables/EchoProtocol_Windows.zip`: Unity açmadan oynanabilen Windows paketi.
- `Deliverables/EchoProtocol_Unity_Source.zip`: kaynak proje; Library, Temp ve ham günlükler hariç.
- `PROJE_OZETI_TR.md`: oyun fikri ve tamamlanan kapsamın kısa özeti.
- `VALIDATION_TR.md`: gerçek test sonuçları ve tekrar çalıştırma komutları.

Otomatik testler sentetik oyuncu rotaları kullanır. Gerçek Unity raycast,
CharacterController, AI, ekran durumları ve kayıt kodu çalıştırılır; bunlar
klavye/fareyle uzun bir kullanıcı oynama seansı olarak sunulmaz.

Yeni testler ayrıca gerçek oyunun `ProcessControls` ve `EchoPlayer.Tick` yoluna
kontrol kareleri verir: üç sektör hareket, bakış, ateş ve etkileşim komutlarıyla
fiziksel olarak tamamlanır. İşletim sisteminden klavye/fare girdisi gönderilmez.
Tüm doğrulamayı yeniden çalıştırmak için `Verification/Validate-Release.ps1` kullan.



## Ortam ve oynanabilirlik

Üç sektör ayrı görsel kimlik kullanır: araştırma laboratuvarında masalar ve
bilgisayarlar; bakım hattında dolaplar, variller ve kablolar; reaktörde iki
enerji çekirdeği. Panelli zemin/tavan, kapı çerçevesi ve duvarlar gerçek
modüler mesh modelleridir. Muhafız ve Echo animasyonlu robot; Pulse carbine
görünümü de gerçek silah modelidir. Kaynak/lisanslar ASSET_CREDITS_TR.md'de.

İkinci sektörde konsol/plaka sağ arka bakım hattındadır. Görev işareti ve
Echo kartları sıradaki konsolu, plaka atamasını ve tekrar ilerlemesini gösterir.
Kırmızı lazer muhafızın ateş öncesi uyarısıdır; siper al veya Echo ile dikkatini
çek. Echo'nun atış hattını açık tut: kayıt, gerçek dünyada yeniden raycast yapar.

Yeni hareketler kayıt çekirdeğini değiştirmez. Echo, koşulan rotayı kaydedilen
hızda tekrar eder. Nişan alma sırasındaki hareket 2,6 m/s; normal hareket 4 m/s.

## 10 Ekim paylaşım paketi

Deliverables/EchoProtocol_Ortak_Catisma_2026-10-10.zip dosyasını tamamen çıkar; EchoProtocol/EchoProtocol.exe dosyasını aç ve ÖRNEK BÖLÜMÜ OYNA'yı seç. Paket Windows build'ini, başlangıç yönergesini, 3–5 yeni oyuncu için gözlemci rehberini, boş geri bildirim formunu ve asset lisanslarını içerir. İlk insan denemesinde F6 kullanma; çözümü gösterebilir. Kaynak proje Deliverables/EchoProtocol_Unity_Source.zip içindedir.
