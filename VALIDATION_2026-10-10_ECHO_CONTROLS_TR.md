# Echo Protocol — Echo kullanım düzeltmesi / 10 Ekim 2026

Final oyun kodu ve dağıtılabilir paket teknik olarak doğrulandı. İnsan deneyimi değerlendirmesi bekliyor.

| Gerçekten çalıştırılan kontrol | Sonuç |
| --- | --- |
| Runtime + Editor C# / kurulu Unity API | Geçti |
| Kayıt çekirdeği / regresyon | 14/14 ve 25/25; 1.000 üretilmiş kayıt |
| Gerçek Unity 2021.3.21f1 Play Mode / F6 yolu | 279/279 |
| Windows x64 build | 0 hata / 0 uyarı |
| Windows 1280×720 / 800×600 / 1920×1080 | Her birinde 279/279, çıkış 0, runtime hatası 0 |
| Öğretim, kayıt seçimi, süre sonu seçimi görüntüleri | Üç gerçek Windows çözünürlüğünde üretildi; boyut/karanlık görüntü kontrolü geçti |
| Kayıt dosyası | v2 / Layout 1; eski şema güvenliği korunur |
| Kaynak ZIP / Library olmadan temiz Unity importu | 279/279 |
| Yeni paylaşım ZIP / ayrı klasörden gerçek Windows | 279/279; çıkış 0; runtime hatası 0 |
| Paket bütünlüğü | 126 Windows build dosyası aynı; düzeltilmiş Unity DLL korunur |
| İnsan oynama denemesi | Henüz yapılmadı |

Kanıt: Verification/Results/2026-10-10/EchoClarity/. 1280×720 gerçek Windows
öğretim, dolu kayıt seçimi, süre sonu seçimi ve olay haritası PNG'leri ayrıca
gözle incelendi: başlıklar, düğmeler ve görev olayları okunur. Windows penceresi
computer-use becerisiyle etkinleştirildi; otomatik oyun kontrolleri insan
oynama denemesi olarak sayılmaz.

ReplayTimeline.cs SHA256: 5BB38EA3FC772C28C8901DFD958D145C78D1B8A8B4C6E61717652B5645C90A52.
TimelineSelfChecks.cs SHA256: 0BF14AEAB955B6A9AE4688514377934C1B5AF3F368349513D7C57F1FE72DCB2A.
İkisi de önceki korunmuş sürümle aynıdır.

Başlangıç: pre-echo-controls-20261010 / 2fe97cd.
Unity 2021.3.21f1 / Built-in / Windows x64 / Mono korunur.

## Somut oyuncu sorunu ve değişiklik

Oyuncu Echo’yu nasıl kontrol edeceğini anlamadığını, tuşlara basınca karakterin
bazen kendiliğinden hareket ettiğini söyledi. Bu geri bildirime göre:

- Bölüm girişinde süre durur, üç kısa adım oyuncunun hareketiyle otomatik
  Echo tekrarının farkını anlatır. Enter veya düğme ile kontrol başlar.
- HUD canlı oyuncunun kaydedildiğini, renkli Echo kartları ve dünya etiketleri
  kaydedilmiş geçmişin otomatik oynatıldığını açıkça belirtir.
- T, mevcut iki kayıtla başa döner; yeni Echo eklemez. Ölüm/reset/yükleme ve
  üç bağımsız kontrol noktasının davranışları korunur.
- Tab, olay saatlerini ve rota konumlarını 0–20 saniyede salt okuma gösterir.
  Bu önizleme dünyanın kapısını, muhafızını veya atışlarını simüle etmez.
- Dolu planda R oyuncudan hangi Echo’nun değişeceğini sorar; ikinci kayıt
  korunur. Tab ile vazgeçmek 20 saniyenin altında canlı denemeye geri döner.
- Boş yuva varken sürenin dolması artık istemsiz Echo oluşturmaz; duraklatılmış
  seçim açılır. Saklama veya T/Tab ile denemeyi bırakma seçilebilir. Dolu planın
  süre sonu iki Echo’yu koruyarak döngüyü yeniden başlatır.
- Kontroller, dağıtım rehberi ve yeni oyuncu formu güncellendi.

Rota veya hedef komutu veren bir NPC yönetimi eklenmedi. Oyunun “geçmişteki
kendimle problemi çözdüm” deneyimi, oyuncunun gerçek hareketlerinin kaydından
oluşur. Daha sonraki fırsat geri sayımı, dikkat yönü ve görev geçişi tasarımları
ECHO_KULLANIM_TR.md içinde ayrı, uygulanmamış adaylar olarak açıklanır.

## Yeni kontrollerin kapsadığı davranışlar

26 anlamlı oyun kontrolü: öğretim duraklaması, gerçek Enter başlangıcı, R'nin
iki boş yuvayı doldurması, iki kayıt doluyken seçim, seçim iptali, yanlış indeks,
her iki slotun ayrı ayrı değişimi, diğer kaydın değişmemesi, eski kaydın
salt okunur kalması, T'nin sağlığı ve cihazları sıfırlaması, tuş önceliği,
duraklama engeli, kayıt yükleme, 20 saniyede istemsiz kayıt olmaması ve süre
sonu iptalinin seçim ekranına kilitlememesi. İlk 253 kontrol korunmuştur;
eski örnek bölüm testlerinin sadece yeni açık öğretimi geçmesi ve dolu planın
silme seçimini açıkça yapması güncellenmiştir. Kayıt çekirdeği değişmedi.

Kayıt dosyası v2 / Layout 1 kalır. Seçim ve harita kaydırıcısı geçici arayüz
bilgisidir; kayda yazılmaz. DEVAM tamamlanmış iki kaydı temiz döngüden başlatır.

## Geliştirme sırasında bulunan durum

İlk çalıştırma 264 başarılı kontrol sonrasında yeni bir testte 10×0,05 saniyeyi
0,5 ile tam eşit karşılaştırdığı için durdu. Float zaman için 0,001 tolerans
kullanıldı; bu test düzeltmesi kayıt davranışını değiştirmedi. Daha sonraki
278 kontrol sürümü Unity ve Windows üç çözünürlükte geçti. Süre sonunda iptal
akışı düzeltildi ve final beklenti 279 kontrole çıkarıldı; final kanıtlar ayrı.

## İnsan deneyimi

Bu geri bildirim kullanıcıdan geldi; 3–5 yeni oyuncu ile planlı insan denemesi
henüz yapılmadı. Kolaylık, ilk hedefi anlama, tekrar deneme isteği ve etkileyicilik
kanıtlanmış kabul edilmiyor. 8–12 dakika hedefi henüz gerçek ilk oynayış süresi
olarak ölçülmedi. İnsan formuna R/T farkı, karakter kontrolü ve istemsiz Echo
oluştuğu hissi için yeni sorular eklendi.

## Denenecek sürüm ve geri dönüş

Paket: Deliverables/EchoProtocol_Echo_Kontrolu_2026-10-10.zip.
Sürüm kimliği: EP-20261010-SAMPLE-CONTROLS; önceki SAMPLE ZIP'i korunmuştur.
Yerel oyun: Builds/EchoProtocol/EchoProtocol.exe. ÖRNEK BÖLÜMÜ OYNA yeni öğretimi
başlatır; ENTER / İLK DEMO önceki karşılaştırma kampanyasını açar.

Git sürüm etiketi: echo-controls-20261010. Yeni raporlar ve kanıtlar doğrulama
sonrasında pakete yeniden alınır; oyun kodu, assetler ve Windows motor dosyaları
teknik kontrollerin geçtiği adayla aynıdır. Son dosya eşleştirmesi
final-package-audit-result.txt içinde kaydedilir.
