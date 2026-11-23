using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>Deterministik rastgele sayı üreteci (aynı tohum = aynı savaş).</summary>
public class Rng
{
	ulong _s;
	public Rng(ulong tohum) { _s = tohum == 0 ? 0x9E3779B97F4A7C15UL : tohum; Sonraki(); }
	public ulong Sonraki()
	{
		_s ^= _s >> 12; _s ^= _s << 25; _s ^= _s >> 27;
		return _s * 2685821657736338717UL;
	}
	public float Kesir() => (Sonraki() >> 40) / (float)(1UL << 24);
	public int Aralik(int n) => n <= 0 ? 0 : (int)(Kesir() * n) % n;
	public bool Sans(float p) => Kesir() < p;
}

public class Durum
{
	public string Tip;      // kalkan, sersem, yanma, guclendir, zayiflat, alay
	public float Deger;
	public int Kalan;
}

public class Birim
{
	public KartTanim T;
	public int Idx, Taraf, Konum, Seviye, Yildiz;   // Konum 0-1 ön sıra, 2-4 arka sıra
	public float MaxCan, Can, Sal, Sav, Hiz, Enerji;
	public List<Durum> Durumlar = new();
	public bool Canli => Can > 0;
	public bool OnSira => Konum < 2;
	public float Kalkan => Durumlar.Where(d => d.Tip == "kalkan").Sum(d => d.Deger);
	public bool Alayli => Durumlar.Any(d => d.Tip == "alay");
	public float EtkinSal => Sal * (1f + Durumlar.Where(d => d.Tip == "guclendir").Sum(d => d.Deger));
	public float EtkinSav => Sav * MathF.Max(0.2f, 1f - Durumlar.Where(d => d.Tip == "zayiflat").Sum(d => d.Deger));
}

/// <summary>Savaş ekranının sırayla oynattığı olay.</summary>
public class Olay
{
	public string Tip;      // tur, saldiri, ulti, hasar, iyiles, kalkan, durum, olum, enerji, atla, bitis
	public int Kaynak = -1, Hedef = -1;
	public float Deger;
	public float KalanCan, KalanKalkan;
	public bool Kritik;
	public int Ustunluk;    // element: 1 avantaj, -1 dezavantaj
	public string Metin = "";
}

public class TakimUyesi
{
	public string Id; public int Seviye = 1, Yildiz = 1;
	public TakimUyesi() { }
	public TakimUyesi(string id, int seviye, int yildiz) { Id = id; Seviye = seviye; Yildiz = yildiz; }
}

/// <summary>
/// 5'e 5 otomatik savaş. Taraf 0 saldıran (oyuncu), taraf 1 savunan.
/// Her tur canlı birimler hıza göre sırayla hareket eder; enerji 100 olunca ulti kullanılır.
/// MaxTur sonunda kimse kazanmadıysa savunan kazanır.
/// </summary>
public class Savas
{
	public const int MaxTur = 15;
	/// <summary>Bu turdan sonra öfke başlar: her tur hasar +%15 (uzayan savaşları bitirir).</summary>
	public const int OfkeTur = 9;
	public static float OfkeCarpan(int tur) => tur > OfkeTur ? 1f + 0.15f * (tur - OfkeTur) : 1f;
	public readonly List<Birim> Birimler = new();
	public readonly List<Olay> Olaylar = new();
	public int Kazanan = -1;
	public int Tur;
	readonly Rng _rng;
	List<Birim> _son = new();
	readonly string _bonusMit;
	readonly float _bonusOran;

	/// <summary>bonusMitoloji: saldıran taraftaki bu mitolojinin kartları +bonusOran can/saldırı alır (haftalık etkinlik).</summary>
	public Savas(IList<TakimUyesi> a, IList<TakimUyesi> b, ulong tohum, string bonusMitoloji = null, float bonusOran = 0f)
	{
		_rng = new Rng(tohum);
		_bonusMit = bonusMitoloji; _bonusOran = bonusOran;
		Ekle(a, 0);
		Ekle(b, 1);
	}

	void Ekle(IList<TakimUyesi> takim, int taraf)
	{
		var uyeler = takim.Where(u => u != null && Veri.Harita.ContainsKey(u.Id)).Take(5).ToList();
		// mitoloji uyumu: aynı mitolojiden 2/3/4/5 kart → +%5/10/15/20 can ve saldırı
		int enCok = uyeler.Count == 0 ? 0 : uyeler.GroupBy(u => Veri.Harita[u.Id].Mitoloji).Max(g => g.Count());
		float bonus = MitolojiBonus(enCok);
		var baglar = Veri.AktifBaglar(uyeler.Select(u => u.Id));
		for (int i = 0; i < takim.Count && i < 5; i++)
		{
			var u = takim[i];
			if (u == null || !Veri.Harita.TryGetValue(u.Id, out var t)) continue;
			var s = Veri.Stat(t, u.Seviye, u.Yildiz);
			if (taraf == 0 && _bonusMit != null && t.Mitoloji == _bonusMit) { s.can *= 1 + _bonusOran; s.sal *= 1 + _bonusOran; }
			// takım bağları (simya): yalnızca bağın üyeleri
			foreach (var bag in baglar.Where(b => b.Kartlar.Contains(t.Id)))
				switch (bag.Stat)
				{
					case "can": s.can *= 1 + bag.Oran; break;
					case "saldiri": s.sal *= 1 + bag.Oran; break;
					case "savunma": s.sav *= 1 + bag.Oran; break;
					case "hiz": s.hiz *= 1 + bag.Oran; break;
				}
			Birimler.Add(new Birim
			{
				T = t, Idx = taraf * 5 + i, Taraf = taraf, Konum = i, Seviye = u.Seviye, Yildiz = u.Yildiz,
				MaxCan = s.can * (1 + bonus), Can = s.can * (1 + bonus), Sal = s.sal * (1 + bonus), Sav = s.sav, Hiz = s.hiz,
				Enerji = t.Sinif == "destek" ? 20 : 0,
			});
		}
	}

	public static float MitolojiBonus(int ayniSayisi) => ayniSayisi switch { >= 5 => 0.20f, 4 => 0.15f, 3 => 0.10f, 2 => 0.05f, _ => 0f };

	public Birim Bul(int idx) => Birimler.FirstOrDefault(b => b.Idx == idx);
	IEnumerable<Birim> Canlilar(int taraf) => Birimler.Where(b => b.Taraf == taraf && b.Canli);
	bool TarafBitti(int taraf) => !Canlilar(taraf).Any();

	public void Oyna()
	{
		if (TarafBitti(0)) { Kazanan = 1; Olaylar.Add(new Olay { Tip = "bitis", Deger = 1 }); return; }
		if (TarafBitti(1)) { Kazanan = 0; Olaylar.Add(new Olay { Tip = "bitis", Deger = 0 }); return; }
		for (Tur = 1; Tur <= MaxTur && Kazanan < 0; Tur++)
		{
			Olaylar.Add(new Olay { Tip = "tur", Deger = Tur });
			if (Tur > OfkeTur) Olaylar.Add(new Olay { Tip = "durum", Hedef = -1, Metin = $"Öfke! Hasar +%{(OfkeCarpan(Tur) - 1) * 100:0}" });
			var sira = Birimler.Where(b => b.Canli)
				.OrderByDescending(b => b.Hiz * (0.95f + 0.1f * _rng.Kesir()))
				.ToList();
			foreach (var b in sira)
			{
				if (Kazanan >= 0) break;
				if (!b.Canli) continue;
				Hamle(b);
				KontrolEt();
			}
		}
		if (Kazanan < 0)
		{
			// süre doldu: kalan can oranı yüksek olan kazanır (eşitlikte savunan)
			float Oran(int t) => Birimler.Where(b => b.Taraf == t).Sum(b => b.Can) / MathF.Max(1, Birimler.Where(b => b.Taraf == t).Sum(b => b.MaxCan));
			Kazanan = Oran(0) > Oran(1) ? 0 : 1;
			Olaylar.Add(new Olay { Tip = "durum", Hedef = -1, Metin = "Süre doldu" });
		}
		Olaylar.Add(new Olay { Tip = "bitis", Deger = Kazanan });
	}

	void KontrolEt()
	{
		if (TarafBitti(1)) Kazanan = 0;
		else if (TarafBitti(0)) Kazanan = 1;
	}

	void Hamle(Birim b)
	{
		// yanma/zehir hasarı hamle başında
		foreach (var d in b.Durumlar.Where(d => d.Tip == "yanma").ToList())
		{
			HasarUygula(null, b, d.Deger, false, 0, "yanma");
			if (!b.Canli) return;
		}
		var sersem = b.Durumlar.FirstOrDefault(d => d.Tip == "sersem");
		if (sersem != null)
		{
			Olaylar.Add(new Olay { Tip = "atla", Kaynak = b.Idx, Metin = "Sersem!" });
			sersem.Kalan--;
			if (sersem.Kalan <= 0) b.Durumlar.Remove(sersem);
			SureAzalt(b);
			return;
		}
		if (b.Enerji >= 100) Ulti(b);
		else NormalSaldiri(b);
		SureAzalt(b);
	}

	void SureAzalt(Birim b)
	{
		foreach (var d in b.Durumlar.Where(d => d.Tip != "sersem").ToList())
		{
			d.Kalan--;
			if (d.Kalan <= 0) b.Durumlar.Remove(d);
		}
	}

	void EnerjiVer(Birim b, float miktar)
	{
		if (!b.Canli) return;
		b.Enerji = MathF.Min(100, b.Enerji + miktar);
		Olaylar.Add(new Olay { Tip = "enerji", Hedef = b.Idx, Deger = b.Enerji });
	}

	void NormalSaldiri(Birim b)
	{
		var hedef = b.T.Sinif == "buyucu" ? RastgeleDusman(b, saygiAlay: true) : TekHedef(b);
		if (hedef == null) return;
		Olaylar.Add(new Olay { Tip = "saldiri", Kaynak = b.Idx, Hedef = hedef.Idx });
		Vur(b, hedef, 1f, 0f);
		EnerjiVer(b, b.T.Sinif == "destek" ? 45 : 38);
	}

	void Ulti(Birim b)
	{
		b.Enerji = 0;
		Olaylar.Add(new Olay { Tip = "ulti", Kaynak = b.Idx, Metin = b.T.UltiAd });
		Olaylar.Add(new Olay { Tip = "enerji", Hedef = b.Idx, Deger = 0 });
		_son = new List<Birim>();
		foreach (var e in b.T.Etkiler)
		{
			var hedefler = Hedefler(b, e);
			switch (e.Tip)
			{
				case "hasar":
					_son = new List<Birim>();
					foreach (var h in hedefler)
					{
						if (!h.Canli) continue;
						Olaylar.Add(new Olay { Tip = "saldiri", Kaynak = b.Idx, Hedef = h.Idx, Metin = "ulti" });
						Vur(b, h, e.Carpan, e.CanCal);
						_son.Add(h);
					}
					break;
				case "iyilestir":
					foreach (var h in hedefler) Iyilestir(h, b.EtkinSal * e.Carpan);
					break;
				case "kalkan":
					foreach (var h in hedefler)
					{
						float m = b.MaxCan * e.Oran;
						h.Durumlar.Add(new Durum { Tip = "kalkan", Deger = m, Kalan = Math.Max(1, e.Tur) });
						Olaylar.Add(new Olay { Tip = "kalkan", Hedef = h.Idx, Deger = m, KalanCan = h.Can, KalanKalkan = h.Kalkan });
					}
					break;
				case "sersemlet":
					foreach (var h in hedefler.Where(h => h.Canli))
						if (_rng.Sans(e.Sans))
						{
							var var_ = h.Durumlar.FirstOrDefault(d => d.Tip == "sersem");
							if (var_ != null) var_.Kalan = Math.Max(var_.Kalan, e.Tur);
							else h.Durumlar.Add(new Durum { Tip = "sersem", Kalan = Math.Max(1, e.Tur) });
							Olaylar.Add(new Olay { Tip = "durum", Hedef = h.Idx, Metin = "Sersem" });
						}
					break;
				case "yanma":
					foreach (var h in hedefler.Where(h => h.Canli))
					{
						h.Durumlar.Add(new Durum { Tip = "yanma", Deger = b.EtkinSal * e.Oran, Kalan = Math.Max(1, e.Tur) });
						Olaylar.Add(new Olay { Tip = "durum", Hedef = h.Idx, Metin = b.T.Element == "doga" ? "Zehir" : "Yanma" });
					}
					break;
				case "guclendir":
					foreach (var h in hedefler.Where(h => h.Canli))
					{
						h.Durumlar.Add(new Durum { Tip = "guclendir", Deger = e.Oran, Kalan = Math.Max(1, e.Tur) + (h == b ? 1 : 0) });
						Olaylar.Add(new Olay { Tip = "durum", Hedef = h.Idx, Metin = $"Saldırı +%{e.Oran * 100:0}" });
					}
					break;
				case "zayiflat":
					foreach (var h in hedefler.Where(h => h.Canli))
					{
						h.Durumlar.Add(new Durum { Tip = "zayiflat", Deger = e.Oran, Kalan = Math.Max(1, e.Tur) });
						Olaylar.Add(new Olay { Tip = "durum", Hedef = h.Idx, Metin = $"Savunma -%{e.Oran * 100:0}" });
					}
					break;
				case "alay":
					b.Durumlar.RemoveAll(d => d.Tip == "alay");
					b.Durumlar.Add(new Durum { Tip = "alay", Kalan = Math.Max(1, e.Tur) + 1 });
					Olaylar.Add(new Olay { Tip = "durum", Hedef = b.Idx, Metin = "Alay!" });
					break;
				case "enerji":
					foreach (var h in hedefler.Where(h => h.Canli && h != b)) EnerjiVer(h, e.Miktar);
					break;
				case "kacinma":
					// Japon mitolojisi (2026-11-23): kitsune yanılsaması — hedef saldırıları "sans" ihtimalle boşa çıkarır
					foreach (var h in hedefler.Where(h => h.Canli))
					{
						h.Durumlar.RemoveAll(d => d.Tip == "kacinma");
						h.Durumlar.Add(new Durum { Tip = "kacinma", Deger = e.Sans, Kalan = Math.Max(1, e.Tur) + (h == b ? 1 : 0) });
						Olaylar.Add(new Olay { Tip = "durum", Hedef = h.Idx, Metin = $"Kaçınma %{e.Sans * 100:0}" });
					}
					break;
			}
			if (TarafBitti(1 - b.Taraf)) break;
		}
	}

	void Vur(Birim s, Birim h, float carpan, float canCal)
	{
		var kac = h.Durumlar.FirstOrDefault(d => d.Tip == "kacinma");
		if (kac != null && _rng.Sans(kac.Deger))
		{
			// rastgele sayı yalnızca kaçınma durumu varken çekilir: eski takımların savaşları aynı kalır
			Olaylar.Add(new Olay { Tip = "durum", Hedef = h.Idx, Metin = "Kaçtı!" });
			if (h.Canli) EnerjiVer(h, 10);
			return;
		}
		float A = s.EtkinSal, D = h.EtkinSav;
		float hasar = 1.45f * A * carpan * (A / (A + D)) * OfkeCarpan(Tur);
		int ust = Veri.ElementUstunluk(s.T.Element, h.T.Element);
		if (ust > 0) hasar *= 1.3f; else if (ust < 0) hasar *= 0.8f;
		float kritSans = s.T.Sinif == "savasci" ? 0.2f : 0.08f;
		bool krit = _rng.Sans(kritSans);
		if (krit) hasar *= 1.6f;
		hasar *= 0.92f + 0.16f * _rng.Kesir();
		float verilen = HasarUygula(s, h, hasar, krit, ust, "");
		if (canCal > 0 && s.Canli) Iyilestir(s, verilen * canCal);
		if (h.Canli) EnerjiVer(h, 10);
	}

	float HasarUygula(Birim s, Birim h, float hasar, bool krit, int ust, string metin)
	{
		hasar = MathF.Max(1, MathF.Round(hasar));
		float kalan = hasar;
		foreach (var k in h.Durumlar.Where(d => d.Tip == "kalkan").ToList())
		{
			float emilen = MathF.Min(k.Deger, kalan);
			k.Deger -= emilen; kalan -= emilen;
			if (k.Deger <= 0.5f) h.Durumlar.Remove(k);
			if (kalan <= 0) break;
		}
		h.Can = MathF.Max(0, h.Can - kalan);
		Olaylar.Add(new Olay
		{
			Tip = "hasar", Kaynak = s?.Idx ?? -1, Hedef = h.Idx, Deger = hasar, Kritik = krit, Ustunluk = ust,
			KalanCan = h.Can, KalanKalkan = h.Kalkan, Metin = metin
		});
		if (!h.Canli)
		{
			h.Durumlar.Clear();
			Olaylar.Add(new Olay { Tip = "olum", Hedef = h.Idx });
		}
		return hasar;
	}

	void Iyilestir(Birim h, float miktar)
	{
		if (!h.Canli) return;
		miktar = MathF.Round(MathF.Min(miktar, h.MaxCan - h.Can));
		if (miktar <= 0) return;
		h.Can += miktar;
		Olaylar.Add(new Olay { Tip = "iyiles", Hedef = h.Idx, Deger = miktar, KalanCan = h.Can, KalanKalkan = h.Kalkan });
	}

	// ------------------------------------------------------------ hedef seçimi
	Birim TekHedef(Birim b)
	{
		var d = Canlilar(1 - b.Taraf).ToList();
		if (d.Count == 0) return null;
		var alay = d.FirstOrDefault(x => x.Alayli);
		if (alay != null) return alay;
		var on = d.Where(x => x.OnSira).ToList();
		var havuz = on.Count > 0 ? on : d;
		return havuz[_rng.Aralik(havuz.Count)];
	}

	Birim RastgeleDusman(Birim b, bool saygiAlay)
	{
		var d = Canlilar(1 - b.Taraf).ToList();
		if (d.Count == 0) return null;
		if (saygiAlay)
		{
			var alay = d.FirstOrDefault(x => x.Alayli);
			if (alay != null) return alay;
		}
		return d[_rng.Aralik(d.Count)];
	}

	List<Birim> Hedefler(Birim b, Etki e)
	{
		var dusman = Canlilar(1 - b.Taraf).ToList();
		var dost = Canlilar(b.Taraf).ToList();
		switch (e.Hedef)
		{
			case "tek": { var t = TekHedef(b); return t == null ? new() : new() { t }; }
			case "tum": return dusman;
			case "arka": { var a = dusman.Where(x => !x.OnSira).ToList(); return a.Count > 0 ? a : dusman; }
			case "en_zayif": return dusman.Count == 0 ? new() : new() { dusman.OrderBy(x => x.Can).First() };
			case "rastgele":
			{
				var l = new List<Birim>();
				for (int i = 0; i < Math.Max(1, e.Adet); i++)
				{
					var c = dusman.Where(x => x.Canli).ToList();
					if (c.Count == 0) break;
					l.Add(c[_rng.Aralik(c.Count)]);
				}
				return l;
			}
			case "son": return _son.Where(x => x.Canli).Distinct().ToList();
			case "kendi": return new() { b };
			case "tum_dost": return dost;
			case "en_zayif_dost": return dost.Count == 0 ? new() : new() { dost.OrderBy(x => x.Can / x.MaxCan).First() };
		}
		return new();
	}
}
