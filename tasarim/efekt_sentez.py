"""Savaş ve arayüz efektlerini sıfırdan sentezler (lisans gerektirmez).

Çalıştır:  python3 tasarim/efekt_sentez.py
Çıktı:     assets/ses/efekt/<ad>.wav  (sonra ogg'a çevrilir)
"""
import os, subprocess
import numpy as np
from scipy.signal import butter, sosfilt

SR = 44100
KOK = os.path.join(os.path.dirname(__file__), "..")
CIKIS = os.path.join(KOK, "assets", "ses", "efekt")
rng = np.random.default_rng(42)

def t(sure): return np.linspace(0, sure, int(SR * sure), endpoint=False)
def gurultu(sure): return rng.uniform(-1, 1, int(SR * sure))
def zarf(n, atak=0.005, bozulma=0.2, egri=3.0):
    x = np.arange(n) / SR
    a = np.clip(x / max(atak, 1e-4), 0, 1)
    d = np.exp(-np.maximum(x - atak, 0) / max(bozulma, 1e-4) * egri / 3)
    return a * d
def bant(x, alt, ust, derece=4):
    sos = butter(derece, [alt, ust], btype="band", fs=SR, output="sos"); return sosfilt(sos, x)
def alcak(x, f, derece=4):
    sos = butter(derece, f, btype="low", fs=SR, output="sos"); return sosfilt(sos, x)
def yuksek(x, f, derece=4):
    sos = butter(derece, f, btype="high", fs=SR, output="sos"); return sosfilt(sos, x)
def kayan_sin(f0, f1, sure, egri=1.0):
    x = t(sure); f = f0 + (f1 - f0) * (x / sure) ** egri
    return np.sin(2 * np.pi * np.cumsum(f) / SR)
def yanki(x, gecikme=0.09, geri=0.35, adet=4):
    y = np.concatenate([x, np.zeros(int(SR * gecikme * adet))])
    for i in range(1, adet + 1):
        d = int(SR * gecikme * i); y[d:d + len(x)] += x * geri ** i
    return y
def karistir(*parcalar):
    n = max(len(p) for p in parcalar); y = np.zeros(n)
    for p in parcalar: y[:len(p)] += p
    return y
def kaydet(ad, x):
    x = x / (np.max(np.abs(x)) + 1e-9) * 0.9
    os.makedirs(CIKIS, exist_ok=True)
    wav = os.path.join(CIKIS, ad + ".wav"); ogg = wav[:-4] + ".ogg"
    import wave
    with wave.open(wav, "wb") as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR)
        w.writeframes((x * 32767).astype(np.int16).tobytes())
    subprocess.run(["ffmpeg", "-y", "-loglevel", "error", "-i", wav, "-c:a", "libvorbis", "-q:a", "4", ogg], check=True)
    os.remove(wav)

# --- kılıç savuruşu + metal çarpma
def kilic():
    s = 0.45
    savurma = bant(gurultu(s), 1500, 6000) * zarf(int(SR * s), 0.06, 0.12) * 0.5
    x = t(0.4); carp = sum(np.sin(2 * np.pi * f * x) * np.exp(-x * d) for f, d in [(2350, 9), (3170, 12), (4410, 14), (1720, 7)])
    carp = karistir(np.zeros(int(SR * 0.07)), carp * 0.6 + bant(gurultu(0.4), 2000, 9000) * zarf(int(SR * 0.4), 0.001, 0.03))
    return karistir(savurma, carp)

# --- ağır darbe (tank, sopa)
def darbe():
    x = t(0.5)
    govde = np.sin(2 * np.pi * (90 * np.exp(-x * 6) + 45) * x) * np.exp(-x * 9)
    tik = alcak(gurultu(0.5), 1800) * zarf(len(x), 0.001, 0.05)
    return govde + tik * 0.7

# --- büyü topu fırlatma
def buyu():
    s = 0.6
    ton = kayan_sin(300, 1400, s, 0.6) * zarf(int(SR * s), 0.02, 0.4) * 0.5
    parilti = sum(np.sin(2 * np.pi * f * t(s)) for f in (2093, 2637, 3136)) * zarf(int(SR * s), 0.05, 0.3) * 0.15
    hava = bant(gurultu(s), 800, 4000) * zarf(int(SR * s), 0.1, 0.25) * 0.4
    return yanki(ton + parilti + hava, 0.07, 0.3, 3)

# --- ateş patlaması
def ates():
    s = 1.0; n = int(SR * s)
    patlama = alcak(gurultu(s), 900) * zarf(n, 0.002, 0.35) * 1.4
    citirti = yuksek(gurultu(s), 2500) * (rng.random(n) > 0.995) * 3 * zarf(n, 0.05, 0.8)
    gumburt = np.sin(2 * np.pi * 55 * t(s)) * np.exp(-t(s) * 7)
    return patlama + citirti * 0.5 + gumburt * 0.8

# --- su sıçraması
def su():
    s = 0.9; n = int(SR * s)
    sicrama = bant(gurultu(s), 400, 3500) * zarf(n, 0.003, 0.25)
    damla = np.zeros(n)
    for k in range(10):
        b = int(rng.uniform(0.05, 0.7) * SR); ln = int(0.06 * SR)
        f0 = rng.uniform(900, 2200); damla[b:b + ln] += kayan_sin(f0, f0 * 1.8, 0.06)[: n - b] * np.exp(-np.arange(min(ln, n - b)) / SR * 50) * 0.3
    return sicrama + damla

# --- doğa büyüsü: hışırtı + toprak gümbürtüsü
def doga():
    s = 0.9; n = int(SR * s)
    hisirti = bant(gurultu(s), 2000, 7000) * (0.5 + 0.5 * np.sin(2 * np.pi * 14 * t(s))) * zarf(n, 0.05, 0.5) * 0.6
    gum = alcak(gurultu(s), 300) * zarf(n, 0.01, 0.25) * 1.2
    ton = kayan_sin(220, 330, s) * zarf(n, 0.05, 0.5) * 0.25
    return hisirti + gum + ton

# --- yıldırım
def yildirim():
    s = 1.6; n = int(SR * s)
    catirti = yuksek(gurultu(s), 1500) * zarf(n, 0.001, 0.08) * 1.5
    gok = alcak(gurultu(s), 250) * zarf(n, 0.05, 1.0)
    gok *= 0.7 + 0.3 * np.sin(2 * np.pi * 3 * t(s))
    return catirti + gok * 1.6

# --- iyileşme: yükselen arp
def iyiles():
    notalar = [523, 659, 784, 1047, 1319]
    y = np.zeros(int(SR * 1.1))
    for i, f in enumerate(notalar):
        b = int(i * 0.08 * SR); x = t(0.6)
        ton = (np.sin(2 * np.pi * f * x) + 0.3 * np.sin(2 * np.pi * 2 * f * x)) * np.exp(-x * 5)
        y[b:b + len(x)] += ton[: len(y) - b] * 0.4
    return yanki(y, 0.11, 0.3, 3)

# --- kalkan: metalik uğultu
def kalkan():
    s = 0.7; x = t(s)
    ton = sum(np.sin(2 * np.pi * f * x) for f in (330, 495, 660, 990)) * zarf(len(x), 0.02, 0.4)
    titrek = 1 + 0.3 * np.sin(2 * np.pi * 18 * x)
    return yanki(ton * titrek * 0.4, 0.05, 0.25, 2)

# --- sersemletme: dönen çan
def sersem():
    s = 0.8; x = t(s)
    return (np.sin(2 * np.pi * 1800 * x + 6 * np.sin(2 * np.pi * 9 * x)) * zarf(len(x), 0.01, 0.6) * 0.5
            + np.sin(2 * np.pi * 2400 * x + 6 * np.sin(2 * np.pi * 7 * x)) * zarf(len(x), 0.05, 0.5) * 0.3)

# --- kritik vuruş
def kritik():
    x = t(0.6)
    bum = np.sin(2 * np.pi * (120 * np.exp(-x * 10) + 40) * x) * np.exp(-x * 6)
    cam = sum(np.sin(2 * np.pi * f * x) * np.exp(-x * 14) for f in (3520, 4186, 5274)) * 0.3
    return bum + cam + yuksek(gurultu(0.6), 3000) * zarf(len(x), 0.001, 0.05) * 0.6

# --- ölüm düşüşü
def dusus():
    s = 0.7; n = int(SR * s)
    y = np.zeros(n)
    for b, g in [(0.0, 1.0), (0.18, 0.6)]:
        i = int(b * SR); x = t(0.4)
        parca = (np.sin(2 * np.pi * 70 * x) * np.exp(-x * 12) + alcak(gurultu(0.4), 1200) * np.exp(-x * 20)) * g
        y[i:i + len(x)] += parca[: n - i]
    return y

# --- kart çekme
def kart():
    s = 0.5; n = int(SR * s)
    vin = bant(gurultu(s), 1500, 8000) * zarf(n, 0.12, 0.1)
    return vin * 0.7 + kayan_sin(600, 1800, s, 1.5) * zarf(n, 0.15, 0.2) * 0.25

# --- efsanevi kart: zafer çanı
def efsanevi():
    y = np.zeros(int(SR * 1.8))
    for i, f in enumerate([523, 659, 784, 1047]):
        b = int(i * 0.1 * SR); x = t(1.4)
        ton = sum(np.sin(2 * np.pi * f * k * x) / k for k in (1, 2, 3)) * np.exp(-x * 2.5)
        y[b:b + len(x)] += ton[: len(y) - b] * 0.35
    return yanki(y, 0.13, 0.35, 4)

# --- arayüz tıklaması
def tik():
    x = t(0.06)
    return np.sin(2 * np.pi * 1300 * x) * np.exp(-x * 80) + bant(gurultu(0.06), 2000, 6000) * np.exp(-x * 120) * 0.3

# --- para/ödül
def para():
    y = np.zeros(int(SR * 0.45))
    for i, f in enumerate([1568, 2093]):
        b = int(i * 0.07 * SR); x = t(0.35)
        y[b:b + len(x)] += (np.sin(2 * np.pi * f * x) + 0.5 * np.sin(2 * np.pi * f * 2.76 * x)) * np.exp(-x * 9)
    return y

# --- zafer ve yenilgi müzikal kısa parçaları
def zafer_muzik():
    y = np.zeros(int(SR * 2.4))
    for b, f, s in [(0, 392, 0.2), (0.2, 523, 0.2), (0.4, 659, 0.2), (0.6, 784, 1.6)]:
        i = int(b * SR); x = t(s)
        ton = sum(np.sin(2 * np.pi * f * k * x) / k ** 1.3 for k in (1, 2, 3, 4)) * zarf(len(x), 0.01, s * 0.8)
        y[i:i + len(x)] += ton * 0.4
    return yanki(y, 0.12, 0.3, 3)

def yenilgi_muzik():
    y = np.zeros(int(SR * 2.4))
    for b, f, s in [(0, 392, 0.35), (0.35, 349, 0.35), (0.7, 311, 0.35), (1.05, 262, 1.2)]:
        i = int(b * SR); x = t(s)
        ton = sum(np.sin(2 * np.pi * f * k * x) / k ** 1.5 for k in (1, 2, 3)) * zarf(len(x), 0.02, s * 0.8)
        y[i:i + len(x)] += ton * 0.4
    return yanki(alcak(y, 3000), 0.15, 0.3, 3)

if __name__ == "__main__":
    for ad, f in [("kilic", kilic), ("darbe", darbe), ("buyu", buyu), ("ates", ates), ("su", su), ("doga", doga),
                  ("yildirim", yildirim), ("iyiles", iyiles), ("kalkan", kalkan), ("sersem", sersem), ("kritik", kritik),
                  ("dusus", dusus), ("kart", kart), ("efsanevi", efsanevi), ("tik", tik), ("para", para),
                  ("zafer", zafer_muzik), ("yenilgi", yenilgi_muzik)]:
        kaydet(ad, f()); print("efekt:", ad)
