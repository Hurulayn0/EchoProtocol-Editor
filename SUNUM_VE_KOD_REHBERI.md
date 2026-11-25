# ECHO PROTOCOL — Sunum & Kod Savunma Rehberi (SEN0416)

**Ekip:** Batın Kazankaya · Kübra Öyküm Aydın · Nazlı İpekyüz · Huriye Kayadelen · Tuğçe Topuz  
**Ders:** SEN0416 Game Programming — İstanbul Kültür Üniversitesi  

---

## 1. Proje Klasör Yapısı (Unity `Assets/` ve `Hierarchy`)

### Unity `Assets/` Klasörleri
- `Assets/Scenes/` → Ana menü ve dört bağımsız oyun sahnesi
- `Assets/Sesler/Resources/EchoAudio/` → **32 adet `.wav` müzik ve ses efekti** (`Music`, `Movement`, `Weapon`, `Sentry`, `Echo`, `Devices`, `UI`)
- `Assets/Arayuz_Gorselleri/` → 38 adet HUD / UI görsel sprite dosyası
- `Assets/EchoProtocol/Runtime/` → **Türkçe gruplanmış tüm C# scriptleri:**
  - `01_Oyun_Yoneticisi/` → Oyun döngüsü, giriş kontrolü, kayıt & tercihler (`EchoPrototypeGame.cs`, `EchoInput.cs`, `EchoPreferences.cs`, `EchoSession.cs`, `EchoSaveStore.cs`, `EchoEditPreviewMarker.cs`)
  - `02_Oyuncu_ve_Silah/` → FPS karakter hareketi, eğilme, ateş etme, aşırı ısınma & vuruş geri bildirimi (`EchoPlayer.cs`, `EchoGameCombat.cs`, `EchoGameplayPolish.cs`)
  - `03_Echo_Zaman_Dongusu/` → Zaman kaydı ve Echo tekrar oynatma sistemi (`ReplayTimeline.cs`, `EchoAction.cs`, `EchoTactics.cs`, `EchoGuidance.cs`)
  - `04_Dusman_ve_Animasyon/` → Muhafız yapay zekâsı, iskelet/namlu animasyonları, kamera efektleri (`EchoEnemy.cs`, `EchoGameArt.cs`, `EchoGamePresentation.cs`, `EchoCameraFinish.cs`)
  - `05_Bolumler_ve_Mekanikler/` → 4 bölüm tasarımı, EMP Jeneratörü & Veri Kasası, Bölüm 3 (3 Aşama) (`EchoLevelDefinition.cs`, `EchoGameWorld.cs`, `EchoResonance.cs`, `EchoSampleLevel.cs`, `EchoSampleWorld.cs`)
  - `06_Arayuz_Harita_Hikaye/` → Ana menü, HUD, Canlı Taktik Harita, Hikaye Brifingi & Credits (`EchoGameUI.cs`, `EchoStoryBriefing.cs`, `EchoHierarchyAndMinimap.cs`, `EchoPlanningMap.cs`, `EchoSampleUI.cs`)
  - `07_Ses_ve_Efektler/` → 32 WAV ses yöneticisi, müzik geçişleri ve parçacık efektleri (`EchoAudioBank.cs`, `EchoImmersion.cs`)

### Unity `Hierarchy` Sahne Klasörleri
Sahne hiyerarşisini incelemek için Assets/Scenes altındaki ilgili sahneyi açın.
- `01_Yoneticiler` → `EchoProtocolRuntime`
- `02_Isik_ve_Kamera` → Ana kamera ve tesis aydınlatmaları
- `03_Cevre_ve_Harita` → Zemin, duvarlar, kapılar, kolonlar ve tesis objeleri
- `04_Etkilesimli_Nesneler` → Konsol, basınç plakası, EMP küresi, Veri Kasası, çıkış alanı
- `05_Karakterler` → Oyuncu, Muhafız ve Echo önizlemeleri
- `06_Arayuz_UI` → HUD Canvas
- `07_Ses_Sistemi` → Müzik ve efekt `AudioSource` bileşenleri

---

## 2. Hoca Sorarsa: "Bu Özelliği Hangi Script ve Fonksiyonla Yaptınız?"

| Özellik | Klasör & Script Dosyası | Ana Fonksiyon(lar) | Hocaya Söylenecek Kısa Açıklama |
|---|---|---|---|
| **FPS Hareket, Zıplama & Eğilme (`C/Ctrl`)** | `02_Oyuncu_ve_Silah/EchoPlayer.cs` | `Tick(float dt, EchoInputFrame input)` | `CharacterController.Move` ile yürüme/koşma/zıplama yapar; `input.Crouch` basılıyken kamera yüksekliğini `1.6m → 1.02m` arası yumuşakça indirir. |
| **Klavye & Fare Girişleri** | `01_Oyun_Yoneticisi/EchoInput.cs` | `EchoInput.Read()` | WASD, Fare, `E`, `R`, `T`, `Q`, `X`, `C/Ctrl`, `TAB`, `ESC` tuşlarını tek bir `EchoInputFrame` yapısında toplar. |
| **Silah Ateş Etme, Raycast & Kafa Vuruşu (2x)** | `02_Oyuncu_ve_Silah/EchoGameCombat.cs` | `TryFirePlayer()`, `ExecuteShot(EchoAction, bool)` | Kamera merkezinden `Physics.Raycast` atar; düşmanın kafa bölgesine gelirse `50` (2x) hasar + sendeletme uygular. |
| **Silah Enerjisi, Aşırı Isınma & Ekran Sarsıntısı** | `02_Oyuncu_ve_Silah/EchoGameplayPolish.cs` | `ConsumeWeaponEnergy()`, `TickWeaponEnergy()`, `AddCameraShake()`, `ShowEmpArc()` | Atış enerjisini yönetir, dolunca aşırı ısınma başlatır, patlamalarda kamera titretir ve EMP yıldırımlarını (`LineRenderer`) çizer. |
| **Echo Zaman Kaydı & Geri Oynatma** | `03_Echo_Zaman_Dongusu/` + `ReplayTimeline.cs` | `CommitLoop()`, `Advance(float dt)`, `Timeline<T>.Sample()` | Oyuncunun her `0.05` saniyedeki pozisyonunu ve `[E]` / ateş eylemlerini kaydedip sonraki döngüde `Ghost` kopyası olarak birebir oynatır. |
| **Muhafız Yapay Zekâsı & Zorluk Kademeleri** | `04_Dusman_ve_Animasyon/EchoEnemy.cs` | `SetDifficultyTier()`, `Tick()`, `TakeDamage()`, `Stagger()` | Oyuncu ve Echo'lar arasında en yakın hedefi seçer, şarj ışınıyla uyarır, ateş eder; bölüm zorluğuna göre can/hasar/hız artar. |
| **Muhafız & Echo İskelet / Ateş Animasyonları** | `04_Dusman_ve_Animasyon/EchoGameArt.cs` | `EchoRobotMotion.Update()`, `TriggerAttack()`, `TriggerHit()` | Robotların `Rig|Idle`, `Rig|Walk`, `Rig|Charge`, `Rig|Attack`, `Rig|Hit`, `Rig|TurnOff` animasyonlarını ve çift namlu ateş efektini oynatır. |
| **EMP Jeneratörü (Küre) & Veri Kasası Hackleme** | `05_Bolumler_ve_Mekanikler/EchoResonance.cs` | `BuildResonance()`, `PulseResonance()`, `TryArchive()`, `TickResonance()` | 3B dönen halkalı EMP küresini oluşturur; vurulunca odadaki tüm muhafızlara zincirleme yıldırım (`-20 Can`, `3.5sn` felç) atar, `+25 Can` + `4sn Aşırı Yükleme` verir ve Veri Kasası'nı 10 sn açar. |
| **4 Bölüm Harita & Siper Tasarımı** | `05_Bolumler_ve_Mekanikler/EchoLevelDefinition.cs`, `EchoGameWorld.cs` | `Campaign[]`, `BuildRoom()`, `UpdateDoor()` | Kolaydan Zora 4 bölümü, konsol/plaka konumlarını, taktik siperleri ve kapı açılma koşullarını tanımlar. |
| **Bölüm 3 (3 Aşamalı Eşzamanlılık)** | `05_Bolumler_ve_Mekanikler/EchoSampleLevel.cs`, `EchoSampleWorld.cs` | `LoadSampleStage()`, `UseSampleDevice()`, `UpdateSampleDoor()` | Servis Geçidi (1.4s), Atış Penceresi (mercek + röleler) ve Tahliye (zorunlu Gözlem Belleği) aşamalarını yönetir. |
| **Canlı Taktik Harita (Minimap)** | `06_Arayuz_Harita_Hikaye/EchoHierarchyAndMinimap.cs` | `DrawLiveMinimap()`, `MiniDot()` | Oyuncu konumunu, görüş konisini (FOV), Echo rotalarını, muhafızları, EMP/Kasa güç hattını ve radar tarama çizgisini anlık çizer. |
| **Ana Menü, HUD, Ayarlar & S/A/B/C Notu** | `06_Arayuz_Harita_Hikaye/EchoGameUI.cs` | `OnGUI()`, `DrawMainMenu()`, `DrawHud()`, `DrawSettings()` | Ana menüyü (4 bölüm kartı), oyun içi HUD'u, ses/hassasiyet ayarlarını ve performans notunu çizer. |
| **Bölüm Hikayeleri & Final Credits Ekranı** | `06_Arayuz_Harita_Hikaye/EchoStoryBriefing.cs` | `OpenStoryBriefing()`, `DrawStoryBriefing()`, `DrawFinalEpilogueAndCredits()` | Her bölüm başında kademeli kısalan daktilo efektli hikaye ekranını ve finalde epilog + 5 kişilik ekip ekranını gösterir. |
| **32 Ses Efekti & Müzik Geçiş Sistemi** | `07_Ses_ve_Efektler/EchoAudioBank.cs` | `CreateAudioBank()`, `TickAudioMusic()`, `PlayBankWorld()` | `Assets/Sesler/Resources/EchoAudio/` altındaki 32 WAV dosyasını yükler, menü ve oyun içi müziklerini yumuşakça geçirir. |
| **Kayıt & Ayarların Saklanması** | `01_Oyun_Yoneticisi/EchoPreferences.cs`, `EchoSaveStore.cs` | `Load()`, `Save()`, `Write()`, `TryRead()` | Ayarları ve en iyi dereceleri `PlayerPrefs` ile, Echo zaman kayıtlarını JSON dosyasıyla diskte saklar. |

---

## 3. Sunumda 5 Kişilik Örnek Görev Paylaşımı

1. **Batın Kazankaya:** Ana oyun mimarisi (`EchoPrototypeGame.cs`), Echo Zaman Kayıt & Geri Oynatma motoru (`ReplayTimeline.cs`), EMP Jeneratörü & Veri Kasası mekanikleri (`EchoResonance.cs`), otomatik testler.
2. **Kübra Öyküm Aydın:** Karakter kontrolü (`EchoPlayer.cs` — hareket, zıplama, `C` ile eğilme), silah sistemi & aşırı ısınma/vuruş hissi (`EchoGameCombat.cs`, `EchoGameplayPolish.cs`).
3. **Nazlı İpekyüz:** 4 Bölümlük seviye tasarımı ve zorluk dengesi (`EchoLevelDefinition.cs`, `EchoGameWorld.cs`, `EchoSampleLevel.cs` / `EchoSampleWorld.cs`), siper yerleşimleri.
4. **Huriye Kayadelen:** Muhafız yapay zekâsı (`EchoEnemy.cs`), robot/Echo iskelet ve ateş animasyonları (`EchoGameArt.cs`), görsel sahne hiyerarşisi (`EchoSceneBuilder.cs`).
5. **Tuğçe Topuz:** Arayüz (UI), Ana Menü, Canlı Taktik Harita (`EchoGameUI.cs`, `EchoHierarchyAndMinimap.cs`), Hikaye Brifingleri & Credits (`EchoStoryBriefing.cs`), 32 ses & müzik entegrasyonu (`EchoAudioBank.cs`).
