# Geliştirme Günlüğü

## 2026-11-17 — Prototip
- 48 kart (4 mitoloji × 12), hepsi piksel art portreli (SpriteCook, 12 sayfa × 2 kredi).
- Deterministik 5v5 otomatik savaş + animasyonlu oynatma (hız 1-3x, atla).
- Ekranlar: Ana Sayfa (hikâye, günlük ödül, görevler, mağaza), Kartlar (koleksiyon, detay, seviye/yıldız), Takım (diziliş, mitoloji uyumu, otomatik diz), Çağır (elmas/parşömen, 40'ta efsanevi garantisi), Arena (3 hayalet rakip, kupa, lig, haftalık sıralama).
- Denge: saldıran %49.7, zaman aşımı %17.7 (süre dolunca kalan can oranı kazanır). Destek sınıfı biraz zayıf (%44.8), Thor güçlü — sıradaki ayar.

## 2026-11-18 — Kule, Efsane Yolu, kart çevirme animasyonu
- Yeni: **Efsaneler Kulesi** (100 kat, enerjisiz, ilk geçiş ödülü, her 10 katta mitoloji teması ve boss).
- Yeni: **Efsane Yolu** aylık sezon kartı (30 seviye, ücretsiz + Altın Yol `sezon_karti` ürünü ₺59,99).
- İyileştirme: çağırmada kart çevirme animasyonu (kart arkası, epik/efsanevide parıltı + titreme + parçacık, efsanevide ekran flaşı, "Hepsini Aç").
- İyileştirme: geçilmiş aşamalar için **Hızlı Geç** x1/x5; alt menüde yapılacak iş rozetleri (görev, yıldız, boş takım yeri, parşömen, arena hakkı).
- Denge: 9. turdan sonra öfke (her tur hasar +%15) — zaman aşımı %15.1'di; destek can 820→840, saldırı 80→90; savaşçı saldırı 112→110 (destek %46.1'di).

## 2026-11-19 — Mitoloji Haftası, 4 yeni kahraman, seviye kutlaması
- Yeni: **Mitoloji Haftası** haftalık etkinliği (Türk → Yunan → İskandinav → Mısır). O mitolojinin kartları hikâye/kulede +%10 can ve saldırı, çağırmada 2 kat sık; zaferler etkinlik puanı verir (takımdaki etkinlik kartı başına +5). 7 ödül eşiği, 2000 puanda haftanın etkinlik kahramanı. Ana sayfada etkinlik düğmesi + menü rozeti.
- Yeni kartlar (epik): **Köroğlu** (Türk savaşçı, ateş), **Perseus** (Yunan tank, su), **Fenrir** (İskandinav savaşçı, doğa), **Sekhmet** (Mısır büyücü, ateş). Portreler SpriteCook (1 sayfa, 2 kredi), Türkçe ulti replikleri Speko; Thor'un eksik repliği de eklendi.
- İyileştirme: seviye/yıldız atlatınca kartta parlama, parçacık ve süzülen yazı; altının yettiği kadar **çoklu seviye** (en fazla +5) düğmesi.
- Denge: Siren 2.1→2.5, Medusa 1.9→2.2, Völva 1.95→2.3, Nil Kâhini 1.1→1.2, Asklepios iyileştirme artırıldı; Thor alan hasarı 0.45→0.40. Önceki test: saldıran %49.2, zaman aşımı %7.9, sınıflar %47.6–52.7.

## 2026-11-23 — 10 yeni kahraman (görseller Sprite Atölye'den)
- Yeni kartlar: **Erlik Han** (Türk, efsanevi büyücü, su), **Al Karısı** (Türk, nadir büyücü, ateş), **Battal Gazi** (Türk, epik savaşçı, doğa), **Hades** (Yunan, efsanevi tank, su), **Artemis** (Yunan, epik savaşçı, doğa), **Kerberos** (Yunan, nadir tank, ateş), **Hel** (İskandinav, epik destek, su), **Tyr** (İskandinav, nadir savaşçı, su), **Set** (Mısır, efsanevi savaşçı, ateş), **Hathor** (Mısır, nadir destek, doğa). Toplam 62 kart, 7 efsanevi.
- Portreler ve savaş bedenleri kullanıcının kendi uygulaması Sprite Atölye ile (sipariş sistemi, `assets/siparis.json` → `tasarim/atolye_al.py`), Türkçe ulti replikleri Speko (gpt-4o-mini-tts).
- Ses: Al Karısı, Artemis, Hel, Hathor kadın ölüm sesi; Kerberos canavar. Simülasyondaki "4 efsanevinin hepsi" hedefi "Tüm efsaneviler" oldu.
- Denge (ilk test): Hel %43,1 (epik ortalaması %57,4, 10 puandan fazla altta) → iyileştirme 1.0→1.35, zayıflatma 0.2→0.25, +10 enerji; Kerberos %39,6 → kendine kalkan; Hathor %40,9 → iyileştirme 1.0→1.15. Set %65,7, Erlik %64,7, Hades %60,2 (efsanevi ortalaması %66,6).
- Görsel gözden geçirme (62 kart): Ülgen'in bedenine komşu hücreden Tepegöz'ün kafası taşmıştı (havada duruyordu) → düzeltildi, `beden_kes.py` artık en büyük dikey bandı bırakır.
  Yeniden çizildi (Sprite Atölye, beden çizilirken portre referans verildi): Battal Gazi (Selçuklu sivri miğferi, pul zırh, kırmızı pelerin, eğri kılıç), Kam (Kızılderili başlığı yerine baykuş tüylü, püsküllü Türk şaman başlığı), Hades (portre ve beden aynı kişi), Set (Set hayvanı başı), Hel, Hathor (inek boynuzları arasında güneş diski); Erlik, Al Karısı, Tyr bedenleri portreyle uyumlu yeniden. Kerberos'ta üç baş çizdirilemedi (tek baş kaldı).


## 2026-11-23 — Japon mitolojisi, kart hikâyeleri, takım simyası
- Yeni mitoloji **Japon** (12 kart: Amaterasu, Susanoo, Raijin, Ryujin, Kitsune, Oni, Tengu, Yuki-onna, Samuray, Ninja, Miko, Kappa). Yeni ulti etkisi `kacinma` (saldırıyı şansla boşa çıkarır, kaçınca +10 enerji). Etkinlik/kule/hikâye arka planı/filtre/müzik listelerine eklendi; repliklerı Speko ile.
- Her kartın **unvanı ve kısa biyografisi** (`tasarim/hikaye.py`): kart detayında "Efsanesi ve Bağları" düğmesi.
- **Takım simyası**: 36 efsane bağı (ör. Zeus+Hades, Loki+Fenrir+Hel, Amaterasu+Susanoo, mitolojiler arası Şimşek Tanrıları Zeus+Thor+Raijin). Bağın bütün kartları takımdaysa yalnızca onlar +%8–12 can/saldırı/savunma/hız alır; mitoloji uyumuyla birlikte işler. Takım ekranında aktif ve bir kartı eksik bağlar gösterilir.

## 2026-11-23 (akşam) — Çin mitolojisi
- 12 kart: Sun Wukong (efsanevi), Yeşim İmparator, Guan Yu, Gök Ejderhası (epik), Nezha, Nüwa, Chang'e, Zhong Kui (nadir), Terakota Askeri, Şaolin Keşişi, Taocu Büyücü, Qilin (sıradan). Portre, beden, arka plan (saray avlusu) Sprite Atölye; replikler Speko.
- 12 yeni bağ (toplam 48), ör. Batı'ya Yolculuk, Ejderha Kralları, Hilebazlar (Sun Wukong+Loki+Kitsune). Etkinlik kahramanı Guan Yu.
- Kartlar ekranındaki mitoloji filtresi 4 sütunlu ızgara oldu (mitoloji sayısı arttıkça satır ekler).
