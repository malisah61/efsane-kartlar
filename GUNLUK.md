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

## 2026-11-24 — Başlangıç Yolu, Mitoloji Muhafızları, etkinlik mağazası, ulti yakın planı
- Yeni: **Başlangıç Yolu** — yeni oyuncuya oyunu sırayla tanıtan 10 adım (ilk zafer, seviye atlatma, çağırma, arena, kule, ilk boss, efsane bağı, 2★, etkinlik puanı, aşama 12). Ana sayfanın tepesinde parlayan panel, "Git" ilgili ekrana götürür, "Al" ödülü verir (toplam 5.000 altın, 410 elmas, 3 parşömen). Aşama 12'yi geçmiş eski kayıtlar görmez.
- Yeni: **Mitoloji Muhafızları** — etkinlik boss'u: haftanın mitolojisinin en güçlü 5 kartı, senin takımının ortalama seviye/yıldızında ve tam mitoloji uyumuyla. Günde 3 hak, enerjisiz; zafer 4 kat etkinlik puanı + 800 altın, yenilgi 10 puan.
- Yeni: **Etkinlik Mağazası** — her etkinlik puanı 1 jeton da verir. Altın, ruh özü, parşömen, etkinlik mitolojisinden rastgele nadir/epik kart, etkinlik kahramanı kopyası (haftalık sınırlı). Jetonlar hafta sonunda sıfırlanır.
- İyileştirme: **ulti yakın planı** — ulti atılınca kartın portresi ve ulti adı ekranı kesen renkli bir şeritte kayarak girer (Hız 3'te ve "Atla"da kapalı).
- İyileştirme: etkinlik penceresi sıkılaştırıldı, Muhafızlar/Mağaza düğmeleri eklendi; ana sayfadaki etkinlik düğmesi kalan muhafız hakkını gösterir.
- Takım simyası: bağı olmayan 19 kartın hepsine 14 yeni bağ (toplam 62): Ergenekon'dan Çıkış (Asena+Demir Alp), Gök Yolculuğu (Kam+Ülgen), Lohusa Bekçisi (Umay+Al Karısı), Ragnarök Düellosu (Heimdall+Loki), Seiðr Büyüsü (Freya+Völva), Rünlerin Sırrı (Odin+Rün Ustası), Nil'in Bekçileri (Sobek+Nil Kâhini), Yedi Akrep (İsis+Akrep Savaşçısı), Hathor'un Rahibesi, Kurama Dağı (Tengu+Samuray), Kalkan Duvarı (Bahadır+Viking Kalkancı+Hoplit), Su Ruhları (Su İyesi+Kappa+Nimf), Ölümcül Ezgi (Siren+Yuki-onna), Kadim Bekçiler (Sfenks+Minotor).
- Denge: destek sınıfı %47,4 / savaşçı %52,7 (fark 5,3) → destek can 840→850, saldırı 90→93; Qilin (%32,5, en düşük) iyileştirme 1.5→1.8 + küçük kalkan.

## 2026-11-23 (gece) — Koleksiyon başarımları, kayıtlı dizilişler, rehber oku
- Yeni: **Koleksiyon Başarımları** (Kartlar ekranında "Başarımlar" düğmesi, menü rozeti): her mitolojinin yarısı ve tamamı, 15/30/50/70/tüm kart, ilk 3★/5★/7★ ve 5/15 kart 3★, 5 kart 5★, "Bağ Koleksiyoncusu" (bütün kartlarına sahip olunan bağ sayısı 5/15/30/tümü). Her biri bir kez elmas/parşömen/ruh özü/altın verir; "Hepsini Al".
- Yeni: **Kayıtlı Dizilişler** — 3 takım kaydı (ör. hikâye/arena/kule), Takım ekranında "Dizilişler" düğmesi; Yükle / Üzerine Kaydet, güç gösterimi. Artık sahip olunmayan kart boş kalır.
- İyileştirme: Başlangıç Yolu'nun gösterdiği sekmenin üstünde zıplayan altın ok (ödül hazırsa Ana Sayfa'yı gösterir).
- İyileştirme: Kartlar ekranındaki mitoloji filtresinde koleksiyon sayısı (ör. "Türk 12/16"); tamamlanan mitoloji altın renkli.
- Takım simyası: 4 yeni bağ (toplam 66): Şoki ve Oni (Zhong Kui+Oni), Kilden İnsan (Nüwa+Ülgen), Göklerin Hükümdarları (Yeşim İmparator+Zeus+Odin), Odin'in Seçtikleri (Odin+Valkür).
- Denge (önceki test: saldıran %50,1, zaman aşımı %8,0, sınıflar %48,0–52,5): Völva 2.3→2.5 + zayıflatma tüm düşmanlara, Valkür 2.3→2.2, Chang'e 0.75→0.85 + sersemletme %18, Zhong Kui kendine kalkan, Taocu 2×1.0→3×0.85, Dede Korkut takıma küçük iyileştirme, Hel iyileştirme 1.35→1.5 + enerji 15; Köroğlu 2.2→2.1 (güçlendirme %12), Sun Wukong 0.95→0.9.

## 2026-11-24 (gece) — Mühürlü Katlar, savaş öncesi diziliş seçici, element önerisi
- Yeni: **Mühürlü Katlar** (Kule penceresinde düğme): her hafta bir mitolojinin mührü (etkinlik mitolojisinden farklı). 10 kat; yalnızca o mitolojinin kartları savaşır, diğerlerinin yeri boş kalır. Düşmanlar başka mitolojilerden; zorluk hafta başında ana kule ilerlemesinin %70'ine sabitlenir. Ödüller haftada bir: her kat altın + 15 ruh özü, 5. kat 30 elmas, 10. kat (boss) 60 elmas + parşömen + 40 ruh özü. Ana sayfadaki kule düğmesi mühür ilerlemesini gösterir.
- Yeni: **savaş öncesi diziliş seçici** — Kule, Mühürlü Katlar ve Muhafızlar pencerelerinde "Diziliş: 1 / 2 / 3" satırı; kayıtlı dizilişe tek dokunuşla geçip pencere güncellenir.
- İyileştirme: **element önerisi** — aynı pencerelerde düşmanın baskın elementi ve ona üstün gelen element (takımında kaç tane olduğu) yazar.
- İyileştirme: Kule penceresinin alt satırı (Mühürlü Katlar + Kapat).
- Takım simyası: 4 yeni bağ (toplam 70): Kalbin Tartılışı (Anubis+Thoth), Gorgon'un Başı (Perseus+Medusa), Girit Boğası (Herakles+Minotor), Fırtına Tanrıları (Set+Raijin).
- Denge (önceki test: saldıran %50,3, zaman aşımı %8,1, sınıflar %48,2–52,4): Völva (%33,1, en düşük) ultisine 2 tur yanma, Nil Kâhini kum fırtınasına hafif yanma; Susanoo 1.8→1.7, Valkür 2.2→2.1.

## 2026-11-25 — Savaş istatistikleri, hikâye girişi, arena savunma dizilişi
- Yeni: **Savaş İstatistikleri** — sonuç penceresinde "İstatistikler": iki takımın her kahramanı için verilen hasar, iyileştirme + kalkan, alınan hasar çubukları, alt etme sayısı, düşenler soluk; kazanan takımda **MVP** (savaşın yıldızı). Sayaçlar savaşın sonucunu etkilemez (rastgelelik aynı).
- Yeni: **Hikâye girişi** — ilk açılışta Dede Korkut anlatır: Dünya Ağacı → Kaos'un Gölgesi → Ozanın Çağrısı → Yol Başlıyor (mitoloji arka planı, beliren portreler, daktilo yazısı, "Atla"). Eski kayıtlar görmez.
- Yeni: **Arena savunma dizilişi** — Arena ekranında "Savunma: Takımım / 1 / 2 / 3"; sunucuya seçili diziliş savunma olarak gider (eşleşme gücü saldırı ve savunmanın büyüğü, zayıf savunmayla kolay rakip alınamaz).
- İyileştirme: hikâye paneline ve Arena'ya **diziliş seçici** (kayıtlı diziliş varsa).
- Takım simyası: 5 yeni bağ (toplam 75): Völuspá (Odin+Völva), Şifa Soyu (Apollon+Asklepios), Gök Gürültüsü Onisi (Raijin+Oni), Ölüler Diyarının Köpekleri (Kerberos+Anubis), Uyumayan Gözler (Heimdall+Horus).
- Denge (önceki test: saldıran %50,4, zaman aşımı %8,1, sınıflar %48,1–52,2; sapan kart yok): Gök Ejderhası (epik, %51,3) alan hasarı 0.5→0.6 + kalkan 0.25; Heimdall takım kalkanı 0.08→0.10; Völva 2.5→2.6 + zayıflatma 0.25; Yuki-onna 0.8→0.85.

