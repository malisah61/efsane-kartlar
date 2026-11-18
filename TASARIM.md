# Efsane Kartlar — Tasarım ve Yol Haritası

## Oyun özeti
Türk, Yunan, İskandinav ve Mısır mitolojilerinden 48 kahraman kartı topla, yükselt, 5 kişilik takım kur.
Savaşlar otomatik ve sıra tabanlıdır; strateji takım kurmada: diziliş (2 ön, 3 arka), element üstünlüğü, mitoloji uyumu ve ultilerin uyumu.

## Kartlar (çıkış seti: 48)
| | Türk | Yunan | İskandinav | Mısır |
|---|---|---|---|---|
| Efsanevi | Ülgen | Zeus | Odin | Ra |
| Epik | Asena, Umay Ana, Tepegöz | Akhilleus, Athena, Apollon | Thor, Freya, Buz Devi Ymir | Anubis, İsis, Sfenks |
| Nadir | Şahmeran, Alp Er Tunga, Dede Korkut, Demir Alp | Medusa, Herakles, Minotor, Asklepios | Loki, Valkür, Heimdall, Idun | Thoth, Horus, Sobek, Bastet |
| Sıradan | Akıncı, Kalkanlı Bahadır, Kam, Su İyesi | Hoplit, Amazon Okçu, Siren, Nimf | Viking Kalkancı, Berserker, Völva, Rün Ustası | Mumya, Akrep Savaşçısı, Nil Kâhini, Ankh Rahibesi |

Her mitolojide 3 tank, 3 savaşçı, 3 büyücü, 3 destek. Elementler: Ateş, Su, Doğa.

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
- **Efsane Yolu (sezon kartı)**: aylık, 30 seviye × 250 puan. Puan: hikâye +5, arena +10, yeni kule katı +20, günlük görev +20. Ücretsiz yol (≈525 elmas, 6 parşömen) + Altın Yol ürünü `sezon_karti` (1.380 elmas, 12 parşömen, 150 ruh özü, epik kahraman).

## Para kazanma (planlanan)
- Elmas paketleri (Google Play Billing), tek seferlik başlangıç paketi, aylık sezon kartı, ödüllü reklam (AdMob).
- Ücretsiz oyuncu da her karta ulaşabilmeli: parşömen, günlük ödüller, garantili efsanevi.

## Yapılacaklar
1. [x] Prototip: kartlar, savaş, koleksiyon, takım, çağırma, hikâye, arena (hayalet rakipler), günlük ödüller, mağaza ekranı
2. [x] Savaş sahnesi: mitoloji arka planları, tam boy piksel karakterler, koşarak saldırı, büyü topları, yıldırım, kalkan/iyileşme/sersemletme efektleri
2b. [x] Yavaş ekonomi: enerji, pahalı yükseltme, az ödül; haftalık arena sezonu ve sıra ödülleri
2c. [~] Denge: `onizleme/denge.txt` sonuçlarına göre kart değerleri (sürekli; 2026-11-18: öfke mekaniği, destek güçlendi)
3. [x] Sunucu (Supabase): hesap, bulut kaydı, gerçek oyunculu arena, sıralamalar, arkadaş, mesaj, lonca
   - [ ] Sunucuda savaş doğrulaması (sonuç şu an istemcide hesaplanıyor)
4. [x] Google Play Billing + AdMob ödüllü/geçiş reklamı (Play Console'da ürün kimlikleri oluşturulmalı)
5. [~] Canlı içerik
   - [x] Sezon kartı (Efsane Yolu, aylık) — 2026-11-18
   - [x] Efsaneler Kulesi (100 kat) — 2026-11-18
   - [ ] Lonca boss'u (sunucu: lonca üyelerinin toplam hasarı, haftalık ödül)
   - [ ] Haftalık etkinlik (ör. "Mısır Haftası": o mitolojiden kartlara +%10 ve etkinlik görevleri, etkinlik kahramanı)
   - [ ] Kule'ye haftalık "mitoloji kısıtlı" katlar
6. [~] Ses/müzik [x], savaş efektleri [x], kart çekme animasyonu [x] (kart çevirme, nadirlik parıltısı, efsanevide ekran flaşı), menü rozetleri [x]
   - [ ] Seviye/yıldız atlatma animasyonu, ulti yakın plan
7. [ ] Yeni kartlar (ayda 2-4): hazır bedenler koroglu, perseus, fenrir, sekhmet — portreleri çizilecek
8. [ ] Yeni oyuncu rehberi (ilk 10 dakika: ok işaretli adımlar)
