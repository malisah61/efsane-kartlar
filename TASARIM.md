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
- 15 turda biten olmazsa savunan kazanır.

## İlerleme ve ekonomi
- **Altın**: seviye atlatma. **Kopya**: yıldız atlatma (1/2/4/8 kopya). **Elmas**: çağırma, arena hakkı, takas. **Parşömen**: ucuz çağırma.
- Çağırma: Elmas (300 / 10x 2700): Efsanevi %3, Epik %13, Nadir %34, Sıradan %50; 40 çekimde efsanevi garantisi. Parşömen: Epik %4, Nadir %26, Sıradan %70.
- Hikâye: 10 bölüm × 6 aşama, her 6. aşama boss. İlk geçiş elmas/parşömen verir.
- Arena: günde 5 hak, 3 rakip (kolay/denk/zor), kupa ve lig (Bronz → Gümüş 400 → Altın 900 → Elmas 1600 → Efsane 2500), haftalık sıralama.
- Günlük: 7 günlük giriş ödülü döngüsü, 3 günlük görev, günde 1 ödüllü reklam.

## Para kazanma (planlanan)
- Elmas paketleri (Google Play Billing), tek seferlik başlangıç paketi, aylık sezon kartı, ödüllü reklam (AdMob).
- Ücretsiz oyuncu da her karta ulaşabilmeli: parşömen, günlük ödüller, garantili efsanevi.

## Yapılacaklar
1. [x] Prototip: kartlar, savaş, koleksiyon, takım, çağırma, hikâye, arena (hayalet rakipler), günlük ödüller, mağaza ekranı
2. [ ] Denge: `onizleme/denge.txt` sonuçlarına göre kart değerleri
3. [ ] Sunucu (Supabase/Firebase): gerçek oyuncuların savunma takımları, gerçek sıralama, hesap
4. [ ] Google Play Billing + AdMob ödüllü reklam
5. [ ] Sezon kartı, etkinlik kahramanları, lonca boss'u
6. [ ] Ses/müzik, savaş efektleri, kart çekme animasyonu
7. [ ] Yeni kartlar (ayda 2-4)
