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
- Savaş sahnesi (`SavasEkrani.cs`): arka plan `assets/arka/<turk|yunan|iskandinav|misir>.png`, tam boy savaşçılar `assets/beden/<id>.png`
  (SpriteCook 3x3/4x4 şeffaf sayfalar → `tasarim/bedenler.txt` + `python3 tasarim/beden_kes.py`). Oyuncu solda sağa bakar, düşman sağda (yansıtılmış).
  Efektler `src/Efekt.cs`: tamamen koddan (CPUParticles2D, mermi, halka, yıldırım, kesik); harici dosya yok.
  `assets/beden/` içinde henüz kartı olmayan hazır bedenler var: koroglu, perseus, fenrir, sekhmet (yeni kart eklerken kullan; portreleri yok).
- Ekonomi bilerek yavaş: hikâye savaşı 6 enerji (max 60, 6 dakikada 1, günde 3 kez 50 elmasa +30), seviye maliyeti 100·sv^1.7, yıldız 2/4/8/12 kopya,
  haftalık arena sezonu (50 kişilik grup, sıra ödülü, kupa yarıya yakın sıfırlanır).
- Yol haritası: `TASARIM.md`.
- `EfsaneKartlar.sln` silinmemeli: yoksa Android dışa aktarımı C# kodunu APK'ya koymaz ve oyun açılışta kapanır. `android.yml` APK'da `EfsaneKartlar.dll` olduğunu kontrol eder.
- JSON için `System.Text.Json` değil Godot `Json` kullan (Android'de güvenli).
- Ses (`src/Ses.cs`): müzik `assets/ses/muzik/` (CC0, `muzik.yml` indirir; menu, savas1-3, boss), efektler `assets/ses/efekt/` (`tasarim/efekt_sentez.py` üretir),
  karakter replikleri `assets/ses/ses/<kart id>.ogg` + olum_erkek/olum_kadin + anons_basla/zafer/yenilgi (Speko, OpenAI gpt-4o-mini-tts, Türkçe; yeni kart eklerken repliğini de üret, Speko çağrılarını TEK TEK yap).
  Kaynaklar `assets/ses/KREDILER.md`. Ayarlar: Profil.MuzikAcik/EfektAcik (ana sayfadaki düğmeler).
