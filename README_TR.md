# ECHO: Loop Protocol

Kendi eylemlerini zaman döngüsüne kaydettiğin birinci şahıs bulmaca ve aksiyon oyunu. Echo kayıtları sonraki döngülerde otomatik oynar.

## Unity projesini açma

1. Unity Hub’da proje kökünü seç ve **Unity 2021.3.21f1** ile aç.
2. **Assets/Scenes/MainMenu.unity** sahnesini aç ve Play’e bas.
3. Yeni oyun başlat veya kayıt varsa **Devam** seçeneğini kullan.

| Sıra | Sahne |
| --- | --- |
| Menü | MainMenu |
| 1 | Level1_ArastirmaLabi |
| 2 | Level2_BakimHatti |
| 3 | Level3_Eszamanlilik |
| 4 | Level4_Reaktor |

Bölümler Assets/Scenes altında ayrı düzenlenir. C# dosyaları Assets/EchoProtocol/Runtime altında görevlerine göre gruplanır. Oyuncu ayarları Player Operator, silah ayarları Pulse Carbine ve döngü süreleri ilgili EchoRoomLayout bileşeninden düzenlenir.

## Kontroller

| Girdi | İşlev |
| --- | --- |
| WASD / fare | Hareket ve bakış |
| Space / Shift / Ctrl | Zıplama, koşma ve eğilme |
| Sol / sağ fare | Ateş ve nişan |
| E | Etkileşim |
| R | Kaydı Echo olarak sakla ve yeni döngüyü başlat |
| T | Echo kayıtlarıyla döngüyü yeniden başlat |
| Q / X | Son Echo’yu sil veya kayıtları temizle |
| Tab / Esc | Planlama haritası ve duraklatma |

## Windows

Windows paketini çıkarıp EchoProtocol.exe dosyasını aç. EXE, EchoProtocol_Data, MonoBleedingEdge ve DLL dosyaları aynı klasörde kalmalı. Yeni bir build için Windows Build Support kurulu Unity 2021.3.21f1’de **File → Build Settings → Windows x86_64 → Build** yolunu kullan.

[Asset kaynakları ve lisansları](ASSET_CREDITS_TR.md) · [Ders sunum rehberi](SUNUM_VE_KOD_REHBERI.md)
