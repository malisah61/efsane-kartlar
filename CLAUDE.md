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
  - `src/Kule.cs` Efsaneler Kulesi (100 kat), `src/SezonKarti.cs` aylık Efsane Yolu, `src/OyunEk.cs` bunların ekranları + menü rozetleri + hızlı geçiş (Oyun partial).
  - `src/Onizleme.cs` bulutta ekran görüntüsü (`--onizleme=klasör`) ve denge testi (`--denge=dosya`).
- Kart hikâyeleri (unvan + biyografi) ve takım bağları (simya: belirli kartlar birlikte → yalnızca o kartlara stat bonusu) `tasarim/hikaye.py`'de; yeni kart eklerken HIKAYE'ye de satır ekle (betik eksikse hata verir). Japon mitolojisi (2026-11-23) `kacinma` ulti etkisini getirdi.
- Kart verisi kaynağı `tasarim/kartlar.py` → `python3 tasarim/kartlar.py` ile `assets/veri/kartlar.json` üretilir. JSON'u elle düzenleme, betiği düzenle.
- Portreler: `assets/kartlar/<id>.png` (128x128 piksel art). **2026-11-23'ten itibaren yeni kart görselleri kullanıcının kendi uygulaması Sprite Atölye ile üretilir** (kullanıcı isteği): `assets/siparis.json`'a `portre_<id>` ve `beden_<id>` satırları → sprite-atolye Actions "Sipariş" (hedef malisah61/efsane-kartlar) → çıktılar sprite-atolye `deneme-sonuclari/siparis/efsane-kartlar/<zaman>/` → `python3 tasarim/atolye_al.py <klasör> [sola_bakan_id,...]` portre (koyu zemin) ve bedeni (şeffaf, ~110 px) yazar. Eski yöntem: SpriteCook ile 2x2 sayfa halinde üretilir (gpt-image-2, quality low = 2 kredi/sayfa = 4 kart).
  Sayfa bağlantıları `tasarim/indir.txt`'e eklenir → `gorsel-indir.yml` bulutta `tasarim/ham/`'a indirir (çalışma ortamından SpriteCook'a erişim yok; bağlantılar ~1 saat geçerli).
  Sonra `tasarim/sayfalar.txt`'e satır ekle ve `python3 tasarim/kes.py` çalıştır.
- İş akışları: `android.yml` her main gönderiminde APK'yı `son-surum` sürümüne yükler; `onizleme.yml` C# derler, denge testini çalıştırır ve ekran görüntülerini `onizleme/`'ye geri yazar. Kod değişikliğinden sonra `onizleme/derleme.txt`, `onizleme/denge.txt` ve görüntüleri kontrol et.
- Formüller: hasar = Saldırı × çarpan × S/(S+Savunma); element Ateş>Doğa>Su>Ateş (×1.3 / ×0.8); kritik savaşçı %20 diğer %8 (×1.6);
  seviye çarpanı (1+0.07(sv-1))·(1+0.12(yıldız-1)); max seviye 10+10·yıldız; mitoloji uyumu 2/3/4/5 kart → +%5/10/15/20.
- Savaş sahnesi (`SavasEkrani.cs`): arka plan `assets/arka/<turk|yunan|iskandinav|misir>.png`, tam boy savaşçılar `assets/beden/<id>.png`
  (SpriteCook 3x3/4x4 şeffaf sayfalar → `tasarim/bedenler.txt` + `python3 tasarim/beden_kes.py`). Oyuncu solda sağa bakar, düşman sağda (yansıtılmış).
  Efektler `src/Efekt.cs`: tamamen koddan (CPUParticles2D, mermi, halka, yıldırım, kesik); harici dosya yok.
  Köroğlu, Perseus, Fenrir, Sekhmet etkinlik kahramanı olarak eklendi (2026-11-19); `assets/beden/` içinde kartsız hazır beden kalmadı — yeni kartlar için beden de çizilmeli.
- Haftalık etkinlik `src/Etkinlik.cs` (Mitoloji Haftası; `Savas` yapıcısına bonusMitoloji verilir, yalnızca hikâye/kule), ekranı `src/OyunEtkinlik.cs`.
- Ekonomi bilerek yavaş: hikâye savaşı 6 enerji (max 60, 6 dakikada 1, günde 3 kez 50 elmasa +30), seviye maliyeti 100·sv^1.7, yıldız 2/4/8/12 kopya,
  haftalık arena sezonu (50 kişilik grup, sıra ödülü, kupa yarıya yakın sıfırlanır).
- Yol haritası: `TASARIM.md`.
- `EfsaneKartlar.sln` silinmemeli: yoksa Android dışa aktarımı C# kodunu APK'ya koymaz ve oyun açılışta kapanır. `android.yml` APK'da `EfsaneKartlar.dll` olduğunu kontrol eder.
- JSON için `System.Text.Json` değil Godot `Json` kullan (Android'de güvenli).
- Ses (`src/Ses.cs`): müzik `assets/ses/muzik/` (CC0, `muzik.yml` indirir; menu, savas1-3, boss), efektler `assets/ses/efekt/` (`tasarim/efekt_sentez.py` üretir),
  karakter replikleri `assets/ses/ses/<kart id>.ogg` + olum_erkek/olum_kadin + anons_basla/zafer/yenilgi (Speko, OpenAI gpt-4o-mini-tts, Türkçe; yeni kart eklerken repliğini de üret, Speko çağrılarını TEK TEK yap).
  Kaynaklar `assets/ses/KREDILER.md`. Ayarlar: Profil.MuzikAcik/EfektAcik (ana sayfadaki düğmeler).
- Google Play: `play.yml` Godot 4.7.2'ye geçirip gradle ile derler, GodotGooglePlayBilling (kaynaktan) + AdMob v7.0 (`addons/AdmobPlugin`+`GMPShared`) ekler; sırlar PLAY_KEYSTORE_BASE64/PLAY_KEYSTORE_PASSWORD varsa imzalı .aab (`play-surum`), yoksa deneme APK'sı. AdMob uygulama kimliği depo değişkeni ADMOB_APP_ID; birim kimlikleri `src/Reklam.cs` (şimdilik Google test kimlikleri, Gercek=false).
- Para/reklam: `src/Magaza.cs` ürünler (reklam_kaldir kalıcı/VIP), `src/Odeme.cs` Play Billing, `src/Reklam.cs` ödüllü (elmas/2x altın/enerji/ücretsiz çağrı, günlük hak `Kayit.ReklamHak`) + geçiş reklamı (7 savaşta bir, günün ilk 10 savaşı hariç). Yıldız sınırı 7★ (`Kayit.MaxYildiz`).
- Ekonomi kontrolü: `onizleme/ilerleme.md` (Simulasyon.cs, 4 bot tipi × 365 gün) ve `onizleme/denge.txt`.
- Çevrimiçi sunucu: Supabase projesi `efsane-kartlar` (gqswczthqabfgiwtfege, Frankfurt). Tüm yazmalar `security definer` RPC'lerle; tablolarda RLS açık, anon yalnızca `kullanici_adi_musait` çağırabilir.
  Edge function'lar: `giris` (kullanıcı adı/e-posta + şifre), `play_giris` (Play Games sunucu kodu → hesap bul/oluştur; sırlar GOOGLE_WEB_CLIENT_ID/SECRET). pg_cron `donem_kontrol` saatlik: haftalık/aylık sıralama ve lonca ödüllerini `odul_kutusu`na dağıtır, eski mesaj/savaşları siler.
  İstemci: `src/Sunucu.cs` (HTTP, oturum `user://oturum.json`, bulut kaydı 20 sn gecikmeli), `src/Sosyal.cs` (Sosyal sekmesi: giriş/kayıt, sıralamalar, arkadaş, mesaj, oyuncu ara, profil/dostluk maçı, lonca, ikili savaş, ödül kutusu, savunma kayıtları, hesap silme), `src/PlayGames.cs` (WebIstemciKimligi boşsa kapalı; play.yml PLAY_GAMES_ID depo değişkeni varsa eklentiyi ekler).
  Canlı test: `sunucu-testi.yml` (elle) → `onizleme/sunucu.txt`; TestBot1/TestBot2 hesapları testten önce SQL ile oluşturulmalı (şifre Deneme-1234), sonra silinmeli.
  Bilinen eksik: savaş sonucu istemcide hesaplanıyor (sunucu yalnızca sınır/kupa hesabı yapıyor); ileride sunucuda savaş doğrulaması.
