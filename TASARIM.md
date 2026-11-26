# Efsane Kartlar — Tasarım ve Yol Haritası

## Oyun özeti
Türk, Yunan, İskandinav, Mısır, Japon ve Çin mitolojilerinden 86 kahraman kartı topla, yükselt, 5 kişilik takım kur.
Savaşlar otomatik ve sıra tabanlıdır; strateji takım kurmada: diziliş (2 ön, 3 arka), element üstünlüğü, mitoloji uyumu ve ultilerin uyumu.

## Kartlar (çıkış seti 48 + ek kartlar)
| | Türk | Yunan | İskandinav | Mısır |
|---|---|---|---|---|
| Efsanevi | Ülgen | Zeus | Odin | Ra |
| Epik | Asena, Umay Ana, Tepegöz | Akhilleus, Athena, Apollon | Thor, Freya, Buz Devi Ymir | Anubis, İsis, Sfenks |
| Nadir | Şahmeran, Alp Er Tunga, Dede Korkut, Demir Alp | Medusa, Herakles, Minotor, Asklepios | Loki, Valkür, Heimdall, Idun | Thoth, Horus, Sobek, Bastet |
| Sıradan | Akıncı, Kalkanlı Bahadır, Kam, Su İyesi | Hoplit, Amazon Okçu, Siren, Nimf | Viking Kalkancı, Berserker, Völva, Rün Ustası | Mumya, Akrep Savaşçısı, Nil Kâhini, Ankh Rahibesi |

Her mitolojide 3 tank, 3 savaşçı, 3 büyücü, 3 destek. Elementler: Ateş, Su, Doğa.

Ek kartlar (etkinlik kahramanları, epik): Köroğlu (Türk savaşçı, ateş), Perseus (Yunan tank, su), Fenrir (İskandinav savaşçı, doğa), Sekhmet (Mısır büyücü, ateş) — 2026-11-19.

## Savaş
- Her tur canlı kartlar hıza göre sırayla hareket eder. Normal saldırı enerji verir (+38, destek +45), hasar alınca +10.
- Enerji 100 → ulti (karta özel etkiler: alan hasarı, iyileştirme, kalkan, sersemletme, yanma/zehir, güçlendirme, zayıflatma, alay).
- Ön sıra önce vurulur; büyücüler normal saldırıda rastgele hedef seçer; alay eden tank tüm saldırıları üstüne çeker.
- 9. turdan sonra **öfke**: her tur hasar +%15 (uzayan savaşları bitirir). 15 turda biten olmazsa kalan can oranı yüksek olan kazanır (eşitlikte savunan).

## İlerleme ve ekonomi
- **Altın**: seviye atlatma. **Kopya**: yıldız atlatma (1/2/4/8 kopya). **Elmas**: çağırma, arena hakkı, takas. **Parşömen**: ucuz çağırma.
- Çağırma: Elmas (300 / 10x 2700): Efsanevi %3, Epik %13, Nadir %34, Sıradan %50; 40 çekimde efsanevi garantisi. Parşömen: Epik %4, Nadir %26, Sıradan %70.
- Hikâye: 10 bölüm × 6 aşama, her 6. aşama boss. İlk geçiş elmas/parşömen verir.
- Arena: günde 5 hak, 3 rakip (kolay/denk/zor), kupa ve lig (Bronz → Gümüş 400 → Altın 900 → Elmas 1600 → Efsane 2500), haftalık sıralama.
- Günlük: 7 günlük giriş ödülü döngüsü, 5 günlük görev, ödüllü reklamlar.
- Hızlı Geç: geçilmiş aşama savaşmadan geçilir (aynı enerji, tekrar ödülü), x1 / x5.
- **Efsaneler Kulesi**: 100 kat, enerji yok, ödül yalnızca ilk geçişte (her 5 katta elmas, her 10. kat boss + parşömen + ruh özü). Her 10 kat bir mitolojinin katları.
- **Mitoloji Haftası (haftalık etkinlik)**: her hafta sırayla Türk → Yunan → İskandinav → Mısır. O mitolojinin kartları hikâye ve kulede +%10 can/saldırı (arenada yok), çağırmada 2 kat sık. Zafer puanı: hikâye/kule 10, arena 15, takımdaki etkinlik kartı başına +5. Eşikler 100/250/450/700/1000/1400/2000 → altın, elmas, parşömen, ruh özü; 2000'de haftanın etkinlik kahramanı.
- **Efsane Yolu (sezon kartı)**: aylık, 30 seviye × 250 puan. Puan: hikâye +5, arena +10, yeni kule katı +20, günlük görev +20. Ücretsiz yol (≈525 elmas, 6 parşömen) + Altın Yol ürünü `sezon_karti` (1.380 elmas, 12 parşömen, 150 ruh özü, epik kahraman).

## Para kazanma (planlanan)
- Elmas paketleri (Google Play Billing), tek seferlik başlangıç paketi, aylık sezon kartı, ödüllü reklam (AdMob).
- Ücretsiz oyuncu da her karta ulaşabilmeli: parşömen, günlük ödüller, garantili efsanevi.

## Yapılacaklar
1. [x] Prototip: kartlar, savaş, koleksiyon, takım, çağırma, hikâye, arena (hayalet rakipler), günlük ödüller, mağaza ekranı
2. [x] Savaş sahnesi: mitoloji arka planları, tam boy piksel karakterler, koşarak saldırı, büyü topları, yıldırım, kalkan/iyileşme/sersemletme efektleri
2b. [x] Yavaş ekonomi: enerji, pahalı yükseltme, az ödül; haftalık arena sezonu ve sıra ödülleri
2c. [~] Denge: `onizleme/denge.txt` sonuçlarına göre kart değerleri (sürekli; 2026-11-18: öfke mekaniği, destek güçlendi; 2026-11-19: Siren/Medusa/Völva/Nil Kâhini güçlendi, Thor alan hasarı azaldı)
3. [x] Sunucu (Supabase): hesap, bulut kaydı, gerçek oyunculu arena, sıralamalar, arkadaş, mesaj, lonca
   - [ ] Sunucuda savaş doğrulaması (sonuç şu an istemcide hesaplanıyor)
4. [x] Google Play Billing + AdMob ödüllü/geçiş reklamı (Play Console'da ürün kimlikleri oluşturulmalı)
5. [~] Canlı içerik
   - [x] Sezon kartı (Efsane Yolu, aylık) — 2026-11-18
   - [x] Efsaneler Kulesi (100 kat) — 2026-11-18
   - [ ] Lonca boss'u (sunucu: lonca üyelerinin toplam hasarı, haftalık ödül)
   - [x] Haftalık etkinlik: Mitoloji Haftası (+%10, çağırma oranı, puan eşikleri, etkinlik kahramanı) — 2026-11-19
   - [x] Etkinliğe özel boss: Mitoloji Muhafızları (günde 3, enerjisiz, takım seviyesinde, 4 kat puan) — 2026-11-24
   - [x] Etkinlik mağazası: her puan 1 jeton; altın/ruh özü/parşömen/etkinlik mitolojisinden nadir-epik kart/kahraman kopyası, haftalık sınırlı — 2026-11-24
   - [x] Kule'ye haftalık "mitoloji kısıtlı" katlar: **Mühürlü Katlar** (10 kat, yalnızca haftanın mühür mitolojisinden kartlar; zorluk hafta başında kule ilerlemesinin %70'ine sabitlenir; haftada ~90 elmas, 1 parşömen) — 2026-11-24 gece
     - [x] 10. katta mitolojiye özel boss (mitolojinin kötüleri, ayna savaşı: Erlik'in Ordusu, Hades'in Mahkemesi, Ragnarök, Kaosun Yılanı, Yüz Yokai Geçidi, Göklerin İsyanı) — 2026-11-25 gece
     - [ ] Sonraki: mühürlü katlar için haftalık sıralama (sunucu)
6. [~] Savaş arka planları piksel stilsiz, ayrıntılı yeniden üretildi (Sprite Atölye, Qwen; 2026-11-25 gece) [x], ses/müzik [x], savaş efektleri [x], kart çekme animasyonu [x] (kart çevirme, nadirlik parıltısı, efsanevide ekran flaşı), menü rozetleri [x]
   - [x] Seviye/yıldız atlatma kutlaması + çoklu seviye düğmesi (2026-11-19)
   - [x] Ulti yakın plan: portre + ulti adı kayan şerit (Hız 3'te kapalı) — 2026-11-24
7. [~] Yeni kartlar (ayda 2-4): Köroğlu, Perseus, Fenrir, Sekhmet eklendi (2026-11-19, Kasım kotası doldu). Sonraki: Aralık'ta 2-4 kart (bedenleri de çizilmeli)
8. [x] Yeni oyuncu rehberi: **Başlangıç Yolu** (10 adım, ana sayfada parlayan panel, Git/Al düğmesi; aşama 12'yi geçmiş eski oyunculara gösterilmez) — 2026-11-24
   - [x] Alt menüde rehberin gösterdiği sekmenin üstünde zıplayan ok — 2026-11-23 gece
   - [x] İlk açılışta kısa hikâye girişi: Dede Korkut anlatır, 4 sayfa (Dünya Ağacı, Kaos'un Gölgesi, Ozanın Çağrısı, Yol Başlıyor), daktilo yazısı, Atla — 2026-11-25
9. [~] Takım simyası: 85 bağ (2026-11-26 gece: Dünyanın Yaratılışı, Saz ve Kopuz, Soğuk Ölüm, Göksel Düello, Kâhinler; 2026-11-25 gece: Apep'in Düşmanları, İdun'un Elmaları, Gök Mağarası, Wedjat Gözü, Jötunheim Seferi; 2026-11-25: Völuspá, Şifa Soyu, Gök Gürültüsü Onisi, Ölüler Diyarının Köpekleri, Uyumayan Gözler; 2026-11-24 gece: Kalbin Tartılışı, Gorgon'un Başı, Girit Boğası, Fırtına Tanrıları; 2026-11-24: bağı olmayan 19 kartın hepsine bağ eklendi; 2026-11-23 gece: Şoki ve Oni, Kilden İnsan, Göklerin Hükümdarları, Odin'in Seçtikleri)
10. [x] Koleksiyon başarımları: mitoloji yarısı/tamamı, toplam kart, yıldız hedefleri, bağ koleksiyoncusu (ömür boyu ~2.000 elmas) — 2026-11-23 gece
11. [x] Kayıtlı dizilişler: 3 takım (hikâye/arena/kule için), Takım ekranında "Dizilişler" — 2026-11-23 gece
12. [~] Sıradaki fikirler
   - [x] Savaş öncesi "Diziliş" seçici (Kule, Mühürlü Katlar, Muhafızlar) + düşmanın baskın elementine karşı öneri — 2026-11-24 gece
   - [ ] Başarım rozeti/unvanı profilde (sunucu profiline de yazılmalı)
   - [ ] Lonca boss'u (sunucu)
   - [x] Hikâye/arena savaşı öncesi de diziliş seçici; arenada savunma dizilişini ayrı kaydetme (sunucuya seçili diziliş gider) — 2026-11-25
   - [x] Savaş sonu istatistikleri (AFK Arena tarzı): her kahramanın verdiği/aldığı hasar, iyileştirme+kalkan, alt etme, MVP — 2026-11-25
   - [x] Savaş tekrarı: son 10 savaş + 5 favori (tohum + takımlar, cihazda), sonuç penceresinde "Tekrar İzle", Arena'da "Tekrarlar" — 2026-11-25 gece
   - [ ] Sonraki: arenada savunma kayıtlarından tekrar (sunucu: `savaslar.tekrar jsonb` + `savas_tekrar_yaz` RPC; migration hazır, onay bekliyor)
   - [x] **Kahraman Ustalığı** (1. kısım): kart savaştıkça ustalık puanı (zafer 3, hikâye/arena yenilgisi 1, Kervansaray saatte 1); 5 seviye (30/80/160/280/450 puan), seviye başına kalıcı +%1 can/saldırı ve bir kez ödül (400 altın / 10 öz / 800 altın+10 elmas / 20 öz / 25 elmas+1 parşömen); kartta mor ustalık rozeti, kart detayında ilerleme çubuğu, Ustalık başarımları — 2026-11-26 gece
   - [ ] Sonraki (2. kısım): ustalık 5 kahramanlara rün yuvası (rün: ruh özüyle yapılan küçük stat taşı, kart başına 1-2 yuva)
   - [x] **Kervansaray** (AFK Arena tarzı boşta kazanç): 12 saate kadar altın + ruh özü + takıma ustalık puanı birikir, hikâye/kule ilerlemesiyle büyür; elmaslı Hızlı Sefer (60 elmas, 2 saatlik, günde 2) — 2026-11-26 gece
   - [ ] Sonraki: Kervansaray'a sefer seçimi (rota: altın / ruh özü / ustalık ağırlıklı) ve günlük ücretsiz reklamlı hızlı sefer
