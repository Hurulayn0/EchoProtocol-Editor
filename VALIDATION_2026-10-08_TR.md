# Echo Protocol — oynanabilirlik ve görsel sürüm doğrulaması / 8 Ekim 2026

Mevcut prototip üzerinde geliştirilen sürüm Unity 2021.3.21f1 Personal,
Built-in / D3D11, Windows x64 / Mono ile çalıştırıldı. Ortam assetleri,
animasyonlu robot, yeni hareketler ve azaltılan tekrar akışı bu sonuçlara dahildir.

## Güncel sonuçlar

| Kontrol | Sonuç |
| --- | --- |
| Runtime ve editor C# API derlemesi | Geçti |
| Orijinal bağımsız kayıt kontrolleri | **14/14** |
| Kayıt sınır/regresyon kontrolleri | **25/25; 1.000 üretilmiş kayıt senaryosu** |
| Gerçek Unity Play Mode | **121/121** |
| Windows x64 build | **Başarılı; 0 hata, 0 uyarı** |
| Windows EXE / 1280×720 | **121/121; çıkış 0; runtime hatası 0** |
| Windows EXE / 800×600 | **121/121; çıkış 0; runtime hatası 0** |
| Windows EXE / 1920×1080 | **121/121; çıkış 0; runtime hatası 0** |
| Kaynak ZIP / temiz Unity importu | **121/121; önbelleksiz ayrı klasörden** |
| Windows ZIP / ayrı klasörden başlatma | **121/121; çıkış 0; runtime hatası 0** |
| Orijinal kayıt çekirdeği | ReplayTimeline.cs ve TimelineSelfChecks.cs ilk kullanıcı ZIP'iyle byte düzeyinde aynı |
| Görsel kanıt | Üç sektör, robot/Echo, konsol/plaka/kapı, savaş, menü, ayarlar ve final görüntülendi |

Her koşuda 121 ayrı kontrol ve tek başarı sonucu gerekir. Eksik/tekrarlanan
kontrol, exception/error/assert, başarısız çıkış kodu veya siyah ekran geçerli
başarı değildir. F6 aynı test girişini kullanır; gerçek oyuncu ayar ve sektör
kaydını değiştirmez.

## 121 canlı kontrolün kapsamı

Önceki 97 kontrol korundu. 24 yeni kontrol oynanabilirlik ve assetleri kapsar.

- Kayıt, interpolasyon, konsol raycast'i, reset, Echo'nun son pozda plaka tutması
  ve gerçek CharacterController ile kapıdan geçiş.
- Hitscan silah, hasar/cooldown, ölüm, Echo ateşi, zaman sıfırındaki olaylar,
  bağımsız cursor restartları ve iki Echo'nun eşzamanlı ateşi.
- Normal/çapraz hareket, zıplama/yerçekimi, duvar ve kapı çarpışmaları, bakış
  sınırları, hassasiyet ve ters fare ayarı.
- Muhafız görüş hattı, siper, görünür oyuncu/Echo hedef seçimi, menzil,
  başlangıç saldırı süresi, ölüm ve respawn.
- Menü, Enter/Escape, duraklatma, odak kaybı işleyicisi, kontrol engelleme,
  ayar ekranı dönüşleri, gerçek PlayerPrefs roundtrip ve bozuk ayar temizliği.
- Bölümden devam, testlerde gerçek checkpoint'in korunması, kalan muhafız
  varken çıkışın engellenmesi ve süre sınırında tamamlanmanın korunması.
- Üç bölümün **ProcessControls / EchoPlayer.Tick** ile gerçek fizik üzerinde
  hareket, nişan, ateş, etkileşim ve kayıt komutlarıyla tamamlanması.
- **45 sektör yenilemesi ve 180 kayıt/reset** sonrasında tek dünya, sabit
  paylaşılan materyal/ses sayısı ve en fazla iki Echo; izlerin temizlenmesi.
- Shift koşusu 6 m/s, çapraz hız sınırı, koşu kaydının aynı hızda tekrarı.
- Sağ tuş nişanı: 2,6 m/s hareket, hassas bakış ve seçilen FOV'a bağlı zoom;
  bırakma ve reset sonrası kamera/silah/geri bildirim temizliği.
- Q boş kayıt davranışı, yalnızca son Echo'yu kaldırması, önceki donmuş kaydı
  koruması, R ile eşzamanlı Q, X ile eşzamanlı Q ve duraklatmada Q'nun engellenmesi.
- İki yuva doluyken süre sonunda iki çözümün korunması; R ile bilerek üçüncü
  kayıt yapılınca en eskisinin değiştirilmesi.
- Görev yönlendirmesinin kayıtlı konsol + son plaka atamasını tanıması;
  konsol eylemi olmayan Echo'nun eksik görevinin gösterilmesi.
- Dokulu robot, yürüme animasyonu ve gerçek runtime boyutunun importtan sonra
  doğrulanması; üç ayrı dokulu ortam; bakım hattındaki farklı sağ rota.
- Ateşten önce kırmızı lazer uyarısı, doğru zamanda tek hasar ve sipere
  girildiğinde uyarının iptal edilmesi.

## Görüntü ve performans

Windows pencereleri görünür halde çalıştırıldı. Menü, kontroller, ayarlar,
savaş, ölüm, çift Echo ve final görüntülerinin gerçek çözünürlüğü ve dolu
framebuffer'ı kontrol edildi. Üç sektörün üç çözünürlükte toplam dokuz yeni
ortam görüntüsü de boyut/piksel kontrolünden geçti. Editörün ayrı kamera
panoramaları ve Windows HUD görüntüleri görsel olarak incelendi.

Son sektörde iki muhafız ve iki Echo ile 120 karelik örnekte bu bilgisayarın
Windows medyan/p95 değeri yaklaşık **6,9 / 6,9 ms**, Unity tahsisleri
**41–42 MiB** idi. Play Mode yaklaşık **16,7 / 16,7 ms**, Unity tahsisleri
**80 MiB** idi. GPU NVIDIA GeForce RTX 3060 Laptop GPU. Unity tahsis değeri
Windows sürecinin toplam RAM'i değildir; kısa yerel örnek başka donanıma
performans garantisi vermez.

## Bu turda bulunan ve düzeltilen sorunlar

- Yeni sağ rota ilk konumunda sipere ateş ediyordu. Konsol/plaka geriye alınarak
  gerçek collider'larla açık bir görüş hattı sağlandı.
- Animasyonlu FBX'in ilk editor karelerindeki skin bounds bilgisi normalizasyon
  için uygun değildi; robot çok küçük görünüyordu. Ölçek bind mesh üzerinden
  kuruldu, merkezleme animasyon hiyerarşisinin dışına taşındı. Runtime görünür
  boyut ayrıca ölçülür; robot/Echo görüntüleri incelendi.
- İkinci sektörün yürüyüş testi oyuncuyu Echo'nun kaydedilmiş ateş hattına
  sokuyordu. Gerçek raycast oyuncunun collider'ında durdu. Çözüm rotası Echo'nun
  atışlarını bitirmesini bekler; HUD ateş hattını açık tutmayı hatırlatır.
- Üst üste gelen bölüm yazıları sadeleştirildi, tavan panelleri ve görünür
  reaktör çekirdekleriyle son görsel düzenleme tekrar doğrulandı.

## Kanıt ve yeniden çalıştırma

Güncel temiz sonuçlar **Verification/Results/2026-10-08/Playability/** altında:

- `compile-timeline-result.txt`, `playmode-result.txt`, `build-result.txt`.
- Üç `standalone-...-result.txt` dosyası ve `game-...png` görüntüleri.
- `clean-source-import-result.txt`: ayrı kaynaktan gerçek Unity Play Mode.
- `portable-windows-package-result.txt` ve `.png`: başka klasöre çıkarılan Windows paketi.
- `art-sector-1/2/3.png`: Editör ortam panoramaları.

Eski 97 kontrole ait Hardening sonuçları aynı tarih altındaki Hardening
klasöründe tarihsel kanıt olarak korunur; güncel sürümün sonucu 121 kontroldür.

```powershell
& Verification/Check-Compile.ps1
& Verification/Validate-Release.ps1 -ResultName Playability
& Verification/Package-Release.ps1
```

`Deliverables/EchoProtocol_Windows.zip` oynanabilir klasörü;
`Deliverables/EchoProtocol_Unity_Source.zip` Assets/Packages/ProjectSettings,
kod, rehber, asset lisansları ve temiz sonuçları içerir. İndirme ZIP'leri,
Library/Temp ve test import klasörleri teslim kaynağına alınmaz.
Kullanılan assetlerin kaynakları ve uyarlaması **ASSET_CREDITS_TR.md** içindedir.

Otomatik kampanya rotaları gerçek Unity kontrol/physics yolunda çalışır;
uzun bir insan klavye/fare oynama seansı olarak sunulmaz. İşletim sisteminden
Alt+Tab ve gerçek fare hislerinin kullanıcının oynama seansıyla değerlendirilmesi
ayrıdır. Ders rubriği tarafımıza verilmedi; kapsam üç kısa bölüm, iki Echo,
tek silah ve tek düşman türü olarak korunur.
