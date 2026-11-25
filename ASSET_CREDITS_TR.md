# Echo Protocol — görsel asset kaynakları

Ortamlar mevcut proje içinde modüler parçalarla kuruldu. Hazır bir oyunun
haritası kullanılmadı. Bölüm yerleşimleri Assets/Scenes altındaki sahnelerde düzenlenir.

## Quaternius / Modular Sci-Fi MegaKit / Standard

- Üretici: Quaternius.
- Kaynak: https://quaternius.com/packs/modularscifimegakit.html
- Üreticinin dağıtımı: https://opengameart.org/content/modular-sci-fi-megakit
- Lisans: CC0 1.0 Universal; proje içindeki özgün metin:
  `Assets/ThirdParty/Quaternius/LICENSE_ModularSciFi.txt`.
- Kullanım: modüler duvar ve zeminler, kapı ve çerçeve, kolonlar, kablolar,
  ışık armatürleri, konsol bilgisayarı. Ücretsiz Standard sürüm seçildi.

## Quaternius / Sci-Fi Essentials Kit / Standard

- Üretici: Quaternius.
- Kaynak: https://quaternius.com/packs/scifiessentialskit.html
- Üreticinin dağıtımı: https://opengameart.org/content/sci-fi-essentials-kit
- Lisans: CC0 1.0 Universal; proje içindeki özgün metin:
  `Assets/ThirdParty/Quaternius/LICENSE_Essentials.txt`.
- Kullanım: QuadShell robotu ve yürüyüş/bekleme animasyonları, Rifle modeli,
  masalar, sandalyeler, dolaplar, variller ve kasalar. Silah hâlâ mevcut
  Pulse carbine mekaniğini kullanır. Tek düşman türü korunur.

## Unity uyarlaması

Seçilmiş 22 FBX modelinin kaynakları ve dokuları projededir. Materyaller
Built-in Standard shader'a bağlandı; albedo, normal ve mevcut emissive
dokuları kullanılır. GPU dokuları 1024 piksel üst sınırla içe aktarılır.
Hazırlanan prefab ve materyaller `Assets/EchoProtocol/Resources/EchoArt`
altındadır.

Assetler görsel kaplama olarak kullanılır. Kapı, siper, zemin, konsol ve
oyuncunun doğrulanan fizik collider'ları oyun kurallarını yürütür.
Echo robotları fiziksel engel oluşturmaz. Muhafız, gerçek hitscan collider'ı
ve aynı 60 can/12 hasar davranışıyla çalışır. Orijinal kayıt çekirdeği korunur.

İndirilen ZIP'ler `Verification/AssetDownloads` içinde yerel hazırlık
verisidir; teslim kaynak paketine alınmaz. Kullanılan model, doku, prefab,
materyal, meta dosyası ve lisans metinleri teslim paketine dahil edilir.
