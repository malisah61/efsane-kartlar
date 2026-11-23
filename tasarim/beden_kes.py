"""Tam boy savaş sprite sayfalarını (şeffaf ızgara) tek tek dosyalara böler.

Çalıştır:  python3 tasarim/beden_kes.py
Girdi:     tasarim/bedenler.txt + tasarim/ham/beden*.png
Çıktı:     assets/beden/<id>.png  (boşlukları kırpılmış, ayaklar alt kenarda)
"""
import os
from PIL import Image

KOK = os.path.join(os.path.dirname(__file__), "..")

def en_buyuk_parca(im):
    """Hücreye taşan komşu kırıntılarını atmak için: alfa maskesinde küçük parçaları sil."""
    a = im.getchannel("A").point(lambda v: 255 if v > 40 else 0)
    w, h = a.size
    px = a.load()
    gorulen = set()
    parcalar = []
    for y in range(h):
        for x in range(w):
            if px[x, y] and (x, y) not in gorulen:
                yigin = [(x, y)]; gorulen.add((x, y)); p = []
                while yigin:
                    cx, cy = yigin.pop(); p.append((cx, cy))
                    for nx, ny in ((cx+1, cy), (cx-1, cy), (cx, cy+1), (cx, cy-1)):
                        if 0 <= nx < w and 0 <= ny < h and px[nx, ny] and (nx, ny) not in gorulen:
                            gorulen.add((nx, ny)); yigin.append((nx, ny))
                parcalar.append(p)
    if not parcalar:
        return im
    enb = max(len(p) for p in parcalar)
    temiz = im.copy(); tp = temiz.load()
    for p in parcalar:
        if len(p) < enb * 0.04:   # çok küçük kırıntılar
            for (x, y) in p:
                tp[x, y] = (0, 0, 0, 0)
    return temiz

def en_buyuk_bant(im):
    """Boş satırlarla ayrılmış dikey bantlardan en yükseğini bırakır (2026-11-23: Ülgen'in altına komşu hücreden
    Tepegöz'ün kafası taşmıştı, ayakları alt kenarda değildi, savaşta havada duruyordu)."""
    a = im.getchannel("A")
    w, h = im.size
    px = a.load()
    dolu = [any(px[x, y] > 40 for x in range(w)) for y in range(h)]
    bantlar, bas = [], None
    for y in range(h + 1):
        if y < h and dolu[y]:
            bas = y if bas is None else bas
        elif bas is not None:
            bantlar.append((bas, y)); bas = None
    if len(bantlar) <= 1:
        return im
    b0, b1 = max(bantlar, key=lambda b: b[1] - b[0])
    temiz = Image.new("RGBA", im.size, (0, 0, 0, 0))
    temiz.paste(im.crop((0, b0, w, b1)), (0, b0))
    return temiz


def main():
    cikis = os.path.join(KOK, "assets", "beden")
    os.makedirs(cikis, exist_ok=True)
    n = 0
    for satir in open(os.path.join(KOK, "tasarim", "bedenler.txt"), encoding="utf-8"):
        satir = satir.strip()
        if not satir or satir.startswith("#"):
            continue
        ad, sutun, *idler = satir.split()
        sutun = int(sutun)
        yol = os.path.join(KOK, "tasarim", "ham", ad)
        if not os.path.exists(yol):
            print("yok:", ad); continue
        im = Image.open(yol).convert("RGBA")
        w, h = im.size
        satirSay = (len(idler) + sutun - 1) // sutun
        cw, ch = w / sutun, h / max(satirSay, sutun)
        for i, kid in enumerate(idler):
            x0, y0 = int((i % sutun) * cw), int((i // sutun) * ch)
            parca = im.crop((x0, y0, int(x0 + cw), int(y0 + ch)))
            parca = en_buyuk_parca(parca)
            parca = en_buyuk_bant(parca)
            kutu = parca.getbbox()
            if kutu:
                parca = parca.crop(kutu)
            parca.save(os.path.join(cikis, kid + ".png"))
            n += 1
    print(n, "beden yazıldı")

if __name__ == "__main__":
    main()
