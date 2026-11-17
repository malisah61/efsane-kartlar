# Proje notları — Efsane Kartlar

- Kullanıcıyla Türkçe konuş; oyundaki tüm metinler Türkçe.
- Tür: kahraman toplamalı kart savaş RPG'si, **asenkron PvP** (rakip çevrimiçi olmak zorunda değil; kayıtlı savunma takımına karşı oynanır).
- Motor: Godot 4.3 .NET (C#), GL Compatibility, **dikey ekran 720x1280** (expand). Hedef Android arm64 APK.
- Arayüz tamamen koddan kurulur (`src/Oyun.cs`, yardımcılar `src/Arayuz.cs`); sahne yalnızca `main.tscn`.
- Dosyalar:
  - `src/Veri.cs` kart tanımları (`assets/veri/kartlar.json`), stat formülleri, portre yükleme.
  - `src/Savas.cs` deterministik 5v5 savaş simülasyonu → olay listesi (`Olay`). Arayüzden bağımsız.
  - `src/SavasEkrani.cs` olay listesini animasyonla oynatır.
  - `src/Kayit.cs` profil/ekonomi (`user://kayit.json`): altın, elmas, parşömen, kupa, kartlar, takım, günlük ödül/görevler, çağırma oranları.
  - `src/Rakipler.cs` hikâye aşamaları (60), arena hayalet rakipleri, haftalık sıralama (prototip: botlar).
  - `src/Onizleme.cs` bulutta ekran görüntüsü (`--onizleme=klasör`) ve denge testi (`--denge=dosya`).
- Kart verisi kaynağı `tasarim/kartlar.py` → `python3 tasarim/kartlar.py` ile `assets/veri/kartlar.json` üretilir. JSON'u elle düzenleme, betiği düzenle.
- Portreler: `assets/kartlar/<id>.png` (128x128 piksel art). SpriteCook ile 2x2 sayfa halinde üretilir (gpt-image-2, quality low = 2 kredi/sayfa = 4 kart).
  Sayfa bağlantıları `tasarim/indir.txt`'e eklenir → `gorsel-indir.yml` bulutta `tasarim/ham/`'a indirir (çalışma ortamından SpriteCook'a erişim yok; bağlantılar ~1 saat geçerli).
  Sonra `tasarim/sayfalar.txt`'e satır ekle ve `python3 tasarim/kes.py` çalıştır.
- İş akışları: `android.yml` her main gönderiminde APK'yı `son-surum` sürümüne yükler; `onizleme.yml` C# derler, denge testini çalıştırır ve ekran görüntülerini `onizleme/`'ye geri yazar. Kod değişikliğinden sonra `onizleme/derleme.txt`, `onizleme/denge.txt` ve görüntüleri kontrol et.
- Formüller: hasar = Saldırı × çarpan × S/(S+Savunma); element Ateş>Doğa>Su>Ateş (×1.3 / ×0.8); kritik savaşçı %20 diğer %8 (×1.6);
  seviye çarpanı (1+0.07(sv-1))·(1+0.12(yıldız-1)); max seviye 10+10·yıldız; mitoloji uyumu 2/3/4/5 kart → +%5/10/15/20.
- Yol haritası: `TASARIM.md`.
