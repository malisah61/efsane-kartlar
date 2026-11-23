"""Sprite Atölye sipariş çıktılarını oyunun görsellerine çevirir (2026-11-23'ten itibaren yeni kartlar).

Çalıştır:  python3 tasarim/atolye_al.py <sipariş klasörü>
Girdi:     <klasör>/portre_<id>/gorsel.png  ve  <klasör>/beden_<id>/gorsel.png
           (sprite-atolye → Actions → "Sipariş", hedef malisah61/efsane-kartlar, çıktılar deneme-sonuclari/siparis/efsane-kartlar/<zaman>/)
Çıktı:     assets/kartlar/<id>.png  128x128 portre, eski portreler gibi koyu zemin üstünde, alttan hizalı
           assets/beden/<id>.png   tam boy savaşçı, şeffaf, ~110 piksel boy, sağa bakar
Görseller atölyenin pixel stilinde arka planı silinmiş geliyor; burada küçültülüp renk sayısı azaltılıyor.
"""
import glob
import os
import sys

from PIL import Image

KOK = os.path.join(os.path.dirname(__file__), "..")
ZEMIN_UST, ZEMIN_ALT = (30, 28, 44), (10, 9, 16)   # eski portrelerin koyu lacivert zemini
BEDEN_BOY = 110


def kucult(im, boy):
    """Saydam görseli boy'a küçült (alan ortalaması), alfayı keskinleştir, renkleri azalt (piksel art görünümü)."""
    im = im.convert("RGBA")
    kutu = im.getchannel("A").point(lambda a: 255 if a > 40 else 0).getbbox()
    if kutu:
        im = im.crop(kutu)
    k = boy / im.height
    im = im.resize((max(1, round(im.width * k)), boy), Image.BOX)
    r, g, b, a = im.split()
    a = a.point(lambda v: 255 if v > 110 else 0)
    rgb = Image.merge("RGB", (r, g, b)).quantize(colors=48, dither=Image.Dither.NONE).convert("RGB")
    out = rgb.convert("RGBA")
    out.putalpha(a)
    return out


def zemin():
    z = Image.new("RGB", (128, 128))
    px = z.load()
    for y in range(128):
        for x in range(128):
            t = min(1.0, (((x - 64) ** 2 + (y - 50) ** 2) ** 0.5) / 100)
            px[x, y] = tuple(round(ZEMIN_UST[i] * (1 - t) + ZEMIN_ALT[i] * t) for i in range(3))
    return z


def portre(yol, kid):
    im = Image.open(yol).convert("RGBA")
    kutu = im.getchannel("A").point(lambda a: 255 if a > 40 else 0).getbbox()
    if kutu:
        im = im.crop(kutu)
    # büst: genişliği ~124 piksele sığdır, kafa üstte kalsın, alttan taşan kısmı kes
    k = min(124 / im.width, 150 / im.height)
    im = im.resize((max(1, round(im.width * k)), max(1, round(im.height * k))), Image.BOX)
    k = kucult(im, im.height)
    z = zemin().convert("RGBA")
    x = (128 - k.width) // 2
    y = max(4, 128 - k.height) if k.height < 124 else 4
    z.alpha_composite(k, (x, y))
    z.convert("RGB").save(os.path.join(KOK, "assets", "kartlar", kid + ".png"))


def beden(yol, kid, ters=False):
    im = kucult(Image.open(yol), BEDEN_BOY)
    if ters:
        im = im.transpose(Image.FLIP_LEFT_RIGHT)
    im.save(os.path.join(KOK, "assets", "beden", kid + ".png"))


def main():
    kaynak = sys.argv[1]
    ters = set(sys.argv[2].split(",")) if len(sys.argv) > 2 else set()   # sola bakan bedenler: aynala
    n = 0
    for d in sorted(glob.glob(os.path.join(kaynak, "*", "gorsel.png"))):
        ad = os.path.basename(os.path.dirname(d))
        if ad.startswith("portre_"):
            portre(d, ad[7:]); n += 1
        elif ad.startswith("beden_"):
            beden(d, ad[6:], ad[6:] in ters); n += 1
    print(n, "görsel yazıldı")


if __name__ == "__main__":
    main()
