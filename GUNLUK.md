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
