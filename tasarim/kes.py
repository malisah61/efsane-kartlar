"""2x2 portre sayfalarını tek tek kart görsellerine böler.

Çalıştır:  python3 tasarim/kes.py
Girdi:     tasarim/sayfalar.txt + tasarim/ham/*.png
Çıktı:     assets/kartlar/<id>.png  (128x128)
Adı _ham ile biten sayfalar işlenmemiş büyük görseldir; önce 256 piksele indirilir.
"""
import os
from PIL import Image

KOK = os.path.join(os.path.dirname(__file__), "..")
BOYUT = 128

def main():
    cikis = os.path.join(KOK, "assets", "kartlar")
    os.makedirs(cikis, exist_ok=True)
    n = 0
    for satir in open(os.path.join(KOK, "tasarim", "sayfalar.txt"), encoding="utf-8"):
        satir = satir.strip()
        if not satir or satir.startswith("#"):
            continue
        ad, *idler = satir.split()
        yol = os.path.join(KOK, "tasarim", "ham", ad)
        if not os.path.exists(yol):
            print("yok:", ad); continue
        im = Image.open(yol).convert("RGB")
        if ad.endswith("_ham.png") or im.width > 600:
            im = im.resize((256, 256), Image.BOX)
            im = im.quantize(colors=96, dither=Image.Dither.NONE).convert("RGB")
        w, h = im.size
        for i, kid in enumerate(idler):
            x, y = (i % 2) * w // 2, (i // 2) * h // 2
            parca = im.crop((x, y, x + w // 2, y + h // 2))
            parca = parca.resize((BOYUT, BOYUT), Image.NEAREST)
            parca.save(os.path.join(cikis, kid + ".png"))
            n += 1
    print(n, "portre yazıldı")

if __name__ == "__main__":
    main()
