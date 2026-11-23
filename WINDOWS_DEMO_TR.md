> Güncel geliştirilen oyun: **IMMERSION_TR.md**. Yeni paylaşım dosyası:
> `Deliverables/EchoProtocol_Gelistirilmis_Demo_2026-10-09.zip`.
> Aşağıdaki notlar önceki Windows demosunun geliştirme kaydıdır.

# Echo Protocol — Windows ders demosu / 9 Ekim 2026

Hedef indirilebilir bir Windows x64 oyunu. Üç kısa bölüm, iki Echo, tek silah
ve tek muhafız türü korunur. Mevcut proje genişletildi; kayıt çekirdeği yeniden
yazılmadı. Bir mağaza yayını yapılmadı.

## Bu sürümde eklenenler

- Tamamlanan Echo rotaları ve eylemleri cihazda saklanır. DEVAM, bu kayıtları
  yükleyerek temiz bir döngü başlatır. Henüz tamamlanmamış deneme saklanmaz.
- Ölümde yeniden deneme mevcut Echo’ları korur. X ve “Sektörü baştan al” açıkça
  kayıtları temizler; Q yalnızca son Echo’yu kaldırır.
- Tab ile açılan planlama haritası zamanı durdurur. Kayıtlı rotalar ve son
  konumlar, plakalar, muhafızlar ve çıkış görünür. Tab ile oyuna dönülür;
  Escape duraklatma menüsüne geçer.
- Ana menüde açılan sektörler tekrar oynanır. En iyi tamamlanma süresi ve en az
  kayıt sayısı cihazda tutulur. Bu iki kişisel rekor bağımsızdır.
- Kayıt boyutu, zaman sırası, konumlar, dönüşler, ateş ışınları, konsol kimlikleri
  ve sürüm doğrulanır. Bozuk ana dosyada geçerli yedek denenir. Geçersiz dosyalar
  oyunu durdurmaz. Her sürüm değişiminde kayıt uyumluluğu yeniden doğrulanmalıdır.
- Windows build’i Unity Application Patcher 1.3.3 ile güvenlik düzeltmesinden
  geçirilir; otomatik Windows testleri düzeltilmiş DLL üzerinde çalıştırılır.

## Nasıl deneyebilirsin?

1. `Deliverables/EchoProtocol_Arkadas_Test_2026-10-09.zip` dosyasını tamamen çıkar.
2. İçindeki `EchoProtocol/EchoProtocol.exe` dosyasını aç. Unity kurulumu gerekmez.
3. İlk bölümde konsolu E ile aç, plakada R ile Echo kaydet; Tab ile rotana bak.
4. Oyunu kapatıp yeniden aç. DEVAM ile Echo’nun geri geldiğini kontrol et.
5. Üç bölümü bitir; sektör kartlarından bir bölümü tekrar oyna.

F6 aynı canlı test takımını çalıştırır ve ana menüye döner. Oynama deneyimini
ayrıca insanlar değerlendirmeli; otomatik testler bulmacanın ne kadar anlaşılır
ve eğlenceli olduğunu tek başına ölçmez. Test için kullanıcı kayıt dosyaları
yerine ayrı geçici dosyalar ve ayrı PlayerPrefs anahtarları kullanılır.

## Bundan sonraki geliştirme sırası

| Aşama | Somut iş | Tamamlanma ölçütü |
| --- | --- | --- |
| Ders demosunun oyuncu denemesi | Arkadaşların ilk bölümde nerede takıldığını ve tekrar hissettiğini topla | Konsol/plaka/Echo ilişkisini açıklama almadan çözebilmeleri |
| Bölüm akışı ve atmosfer | Bu geri bildirimle rotaları, hedef yönlendirmelerini ve ortam seslerini düzenle | Üç bölümün gereksiz yeniden yürüyüş olmadan tamamlanması |
| Çatışma hissi | Muhafız tepkisi, isabet sesleri ve silah geri bildirimi üzerinde çalış | Muhafız/Echo/oyuncu ateşinin anlaşılır olması; tekrar oynatmanın tutarlılığı |
| Sunum ve dağıtım | Başlangıç/bitiş sunumu, hedef bilgisayarlarda deneme, temiz Windows paketi | Ders teslimi ve arkadaş denemesi için aynı doğrulanmış paket |

Ek silahlar, düşman çeşitleri, uzun kampanya ve Steam hazırlığı bu üç bölümlük
ders demosunun tesliminden sonra ayrı kapsam olarak ele alınabilir.

## Yeniden build alma

`Verification/Validate-Release.ps1 -ResultName WindowsDemo` derleme, gerçek
Unity Play Mode, build, resmî Unity düzeltmesi ve üç Windows çözünürlüğü için
test çalıştırır. Ardından `Package-Release.ps1` ve `Package-Playtest.ps1`
kullanılır. Paketleme öncesi güncel SHA256 listesi oluşturulmalıdır.

Kurulu editör 2021.3.21f1’dir; resmî yama dağıtılan Windows uygulamasına uygulanır.
Editör güvenlik güncellemesi yerine geçmez. Editör yükseltmesi yapılırsa mevcut
kayıt, asset importu, canlı oyun testleri ve Windows paketi tekrar doğrulanmalıdır.

Güvenlik kaynakları: [Unity duyurusu](https://unity.com/security/sept-2025-01),
[resmî düzeltme aracı](https://discussions.unity.com/t/cve-2025-59489-patcher-tool/1688032).
