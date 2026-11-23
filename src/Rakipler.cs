using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

public class ArenaRakip
{
	public string Ad;
	public int Kupa, Guc, Zorluk;   // Zorluk 0 kolay, 1 denk, 2 zor
	public string OyuncuId;         // gerçek oyuncu ise sunucu kimliği (bot ise null)
	public List<TakimUyesi> Takim;
	public ulong Tohum;
}

/// <summary>
/// Hikâye aşamaları ve asenkron arena rakipleri.
/// Prototipte arena rakipleri oyuncunun gücüne göre üretilen "hayalet" takımlardır;
/// sunucu eklenince gerçek oyuncuların kaydettiği savunma takımları bunların yerini alacak.
/// </summary>
public static class Rakipler
{
	public const int AsamaSayisi = 60;
	public const int BolumBasina = 6;

	static readonly string[] BolumAdlari =
	{
		"Bozkır Yolu", "Olimpos Eteği", "Buzlu Fiyort", "Nil Kıyısı", "Labirent", "Bifröst",
		"Ölüler Diyarı", "Gök Çadırı", "Titanlar Savaşı", "Ragnarök",
	};
	public static string BolumAd(int asama) => BolumAdlari[Math.Clamp((asama - 1) / BolumBasina, 0, BolumAdlari.Length - 1)];
	public static bool BossAsamasi(int asama) => asama % BolumBasina == 0;

	public static int AsamaSeviye(int asama) => Math.Max(1, (int)MathF.Round((asama - 1) * 0.75f));
	public static int AsamaDusmanSayisi(int asama) => asama <= 2 ? 3 : asama <= 5 ? 4 : 5;

	public static List<TakimUyesi> AsamaTakimi(int asama)
	{
		var r = new Rng((ulong)(asama * 7919 + 13));
		int sev = AsamaSeviye(asama);
		// aşama ilerledikçe nadirlik artar
		float epik = Math.Clamp((asama - 8) / 50f, 0, 0.45f), nadir = asama <= 3 ? 0 : Math.Clamp(0.1f + asama / 80f, 0, 0.5f);
		var takim = new List<TakimUyesi>();
		var kullanilan = new HashSet<string>();
		// tutarlı diziliş: 2 ön (tank/savaşçı ağırlıklı), 3 arka
		string[] sinifSira = { "tank", "savasci", "buyucu", "destek", "buyucu" };
		int adet = AsamaDusmanSayisi(asama);
		for (int i = 0; i < 5; i++)
		{
			if (i >= adet) { takim.Add(null); continue; }
			string nad;
			if (BossAsamasi(asama) && i == 2) nad = asama >= 18 ? "efsanevi" : "epik";
			else { float x = r.Kesir(); nad = x < epik ? "epik" : x < epik + nadir ? "nadir" : "siradan"; }
			string sinif = sinifSira[i];
			if (r.Kesir() < 0.3f) sinif = i < 2 ? (sinif == "tank" ? "savasci" : "tank") : (r.Kesir() < 0.5f ? "buyucu" : "savasci");
			var havuz = Veri.Kartlar.Where(k => k.Nadirlik == nad && k.Sinif == sinif && !kullanilan.Contains(k.Id)).ToList();
			if (havuz.Count == 0) havuz = Veri.Kartlar.Where(k => k.Nadirlik == nad && !kullanilan.Contains(k.Id)).ToList();
			var k = havuz[r.Aralik(havuz.Count)];
			kullanilan.Add(k.Id);
			int yildiz = 1 + Math.Min(2, asama / 20);
			takim.Add(new TakimUyesi(k.Id, Math.Min(sev, Kayit.MaxSeviye(yildiz)), yildiz));
		}
		return takim;
	}

	public static (int altin, int elmas, int parsomen) AsamaOdulu(int asama, bool ilkKez)
	{
		int altin = 130 + asama * 35;
		if (!ilkKez) return (altin / 2, 0, 0);
		return (altin, BossAsamasi(asama) ? 50 : 10, asama % 3 == 0 ? 1 : 0);
	}

	/// <summary>Bölümün arka planı (mitoloji teması).</summary>
	public static string BolumArka(int asama) => ((asama - 1) / BolumBasina) switch
	{
		0 or 7 => "turk", 1 => "yunan", 4 or 8 => "japon", 2 or 5 or 9 => "iskandinav", _ => "misir"
	};

	public static string TakimArka(IEnumerable<TakimUyesi> t)
	{
		var g = t.Where(u => u != null && Veri.Harita.ContainsKey(u.Id)).GroupBy(u => Veri.Harita[u.Id].Mitoloji).OrderByDescending(x => x.Count()).FirstOrDefault();
		return g?.Key ?? "turk";
	}

	// ------------------------------------------------------------ arena
	static readonly string[] Onek = { "Kara", "Ak", "Gök", "Demir", "Kızıl", "Bozkurt", "Yıldız", "Fırtına", "Kartal", "Ay", "Gölge", "Altın", "Buz", "Ateş", "Rüzgar", "Tunç" };
	static readonly string[] Sonek = { "Han", "Bey", "Alp", "Kaya", "Ok", "Kılıç", "Er", "Tuğ", "Yürek", "Pençe", "Kalkan", "Bilge", "Avcı", "Atlı", "Yay" };

	public static string BotAdi(Rng r) => Onek[r.Aralik(Onek.Length)] + Sonek[r.Aralik(Sonek.Length)] + (r.Kesir() < 0.5f ? (r.Aralik(90) + 10).ToString() : "");

	public static string Lig(int kupa) => kupa switch
	{
		>= 2500 => "Efsane", >= 1600 => "Elmas", >= 900 => "Altın", >= 400 => "Gümüş", _ => "Bronz"
	};
	/// <summary>O kupadaki tipik oyuncunun takım gücü (Bronz ~2.500 → Efsane ~12.500).</summary>
	public static float KupaGucu(int kupa) => 2500f + 4f * Math.Max(0, kupa);
	public static int LigSira(int kupa) => kupa >= 2500 ? 4 : kupa >= 1600 ? 3 : kupa >= 900 ? 2 : kupa >= 400 ? 1 : 0;
	public static int SonrakiLigKupa(int kupa) => kupa < 400 ? 400 : kupa < 900 ? 900 : kupa < 1600 ? 1600 : kupa < 2500 ? 2500 : 0;

	/// <summary>Rakipler sıralamadaki gerçek "komşu" oyunculardır: biri altta, biri yakında, biri üstte.</summary>
	public static List<ArenaRakip> ArenaRakipleri()
	{
		var p = Kayit.P;
		int hafta = Hafta();
		var botlar = Botlar(hafta, HaftaIlerleme());
		int guc = Math.Max(400, Kayit.TakimGucu());
		var r = new Rng((ulong)(hafta * 7919 + p.ArenaYenileme * 977 + p.ArenaGalibiyet * 131 + p.ArenaMaglubiyet * 17 + 5));
		var l = new List<ArenaRakip>();
		// Rakibin gücü: kendi gücün ile o kupadaki oyuncuların beklenen gücünün karışımı.
		// Böylece güçlenen oyuncu yukarı tırmanır, ama üst liglerde rakipler de güçlüdür.
		float[] oran = { 0.85f, 1.0f, 1.12f };
		(int, int)[] aralik = { (-250, -30), (-40, 60), (50, 300) };
		var kullanilan = new HashSet<string>();
		for (int z = 0; z < 3; z++)
		{
			var aday = botlar.Where(b => !kullanilan.Contains(b.Ad) && b.Kupa - p.Kupa >= aralik[z].Item1 && b.Kupa - p.Kupa <= aralik[z].Item2).ToList();
			if (aday.Count == 0) aday = botlar.Where(b => !kullanilan.Contains(b.Ad)).OrderBy(b => Math.Abs(b.Kupa - (p.Kupa + (z - 1) * 80))).Take(3).ToList();
			var bot = aday[r.Aralik(aday.Count)];
			kullanilan.Add(bot.Ad);
			var tr = new Rng(bot.Tohum ^ (ulong)(p.ArenaYenileme * 31 + 1));
			int hedef = (int)((0.35f * guc + 0.65f * KupaGucu(bot.Kupa)) * oran[z] * (0.96f + 0.08f * tr.Kesir()));
			var takim = HayaletTakim(tr, hedef);
			l.Add(new ArenaRakip { Ad = bot.Ad, Zorluk = z, Takim = takim, Guc = Kayit.TakimGucu(takim), Kupa = bot.Kupa, Tohum = tr.Sonraki() });
		}
		return l;
	}

	/// <summary>Hedef güce yakın 5 kartlık takım üretir.</summary>
	public static List<TakimUyesi> HayaletTakim(Rng r, int hedefGuc)
	{
		var takim = new List<TakimUyesi>();
		var kullanilan = new HashSet<string>();
		string[] sinifSira = { "tank", "savasci", "buyucu", "destek", "savasci" };
		// güçlü oyunculara karşı daha nadir kartlar
		float nadirlikEgilim = Math.Clamp(hedefGuc / 12000f, 0, 1);
		for (int i = 0; i < 5; i++)
		{
			float x = r.Kesir();
			string nad = x < 0.04f + 0.2f * nadirlikEgilim ? "efsanevi" : x < 0.2f + 0.3f * nadirlikEgilim ? "epik" : x < 0.6f ? "nadir" : "siradan";
			var havuz = Veri.Kartlar.Where(k => k.Nadirlik == nad && k.Sinif == sinifSira[i] && !kullanilan.Contains(k.Id)).ToList();
			if (havuz.Count == 0) havuz = Veri.Kartlar.Where(k => !kullanilan.Contains(k.Id)).ToList();
			var k = havuz[r.Aralik(havuz.Count)];
			kullanilan.Add(k.Id);
			takim.Add(new TakimUyesi(k.Id, 1, 1 + (int)(nadirlikEgilim * 3 * r.Kesir())));
		}
		// seviyeleri hedef güce ulaşana kadar artır
		for (int adim = 0; adim < 400 && Kayit.TakimGucu(takim) < hedefGuc; adim++)
		{
			var u = takim[r.Aralik(5)];
			if (u.Seviye < Kayit.MaxSeviye(u.Yildiz)) u.Seviye++;
			else if (u.Yildiz < Kayit.MaxYildiz) u.Yildiz++;
		}
		return takim;
	}

	public static (int kupa, int altin, int elmas) ArenaOdulu(ArenaRakip r, bool kazandi)
	{
		if (!kazandi) return (-14, 30, 0);
		return (14 + r.Zorluk * 6, 90 + r.Zorluk * 30, 1 + r.Zorluk);
	}

	// ------------------------------------------------------------ haftalık sıralama
	public class SiraBot { public string Ad; public int Kupa; public ulong Tohum; }
	public const int GrupBoyu = 50;
	static readonly DateTime Baslangic = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);   // pazartesi

	/// <summary>Şu an (simülasyonda Kayit.BugunFn'in verdiği gün, gün ortası).</summary>
	static DateTime Simdi()
	{
		var b = Kayit.Bugun();
		if (DateTime.TryParseExact(b, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var g)
			&& g.Date != DateTime.UtcNow.Date)
			return DateTime.SpecifyKind(g.AddHours(12), DateTimeKind.Utc);
		return DateTime.UtcNow;
	}
	public static int Hafta() => (int)Math.Floor((Simdi() - Baslangic).TotalDays / 7);
	public static float HaftaIlerleme() => (float)(((Simdi() - Baslangic).TotalDays / 7) % 1.0);
	public static TimeSpan SezonKalan() => Baslangic.AddDays((Hafta() + 1) * 7) - DateTime.UtcNow;

	/// <summary>Lig grubundaki 49 rakip. Her hafta aynı tohumla üretilir; hafta ilerledikçe kupaları artar.</summary>
	public static List<SiraBot> Botlar(int hafta, float ilerleme)
	{
		var r = new Rng((ulong)hafta * 2654435761UL + 7);
		var l = new List<SiraBot>();
		for (int i = 0; i < GrupBoyu - 1; i++)
		{
			string ad = BotAdi(r);
			float taban = 80 + 2700 * MathF.Pow(r.Kesir(), 1.8f);
			float hiz = 0.25f + 0.5f * r.Kesir();   // hafta içinde ne kadar tırmandığı
			int kupa = (int)(taban * (1 - hiz + hiz * ilerleme)) + r.Aralik(30);
			l.Add(new SiraBot { Ad = ad, Kupa = kupa, Tohum = r.Sonraki() });
		}
		return l;
	}

	public static List<(string ad, int kupa, bool oyuncu)> Siralama() => Siralama(Hafta(), HaftaIlerleme(), Kayit.P.Kupa);

	public static List<(string ad, int kupa, bool oyuncu)> Siralama(int hafta, float ilerleme, int oyuncuKupa)
	{
		var l = Botlar(hafta, ilerleme).Select(b => (b.Ad, b.Kupa, false)).ToList();
		l.Add((Kayit.P.Ad, oyuncuKupa, true));
		return l.OrderByDescending(x => x.Item2).ThenBy(x => x.Item3 ? 0 : 1).ToList();
	}

	public static int OyuncuSirasi(int hafta, float ilerleme, int kupa) => Siralama(hafta, ilerleme, kupa).FindIndex(x => x.oyuncu) + 1;
	public static int OyuncuSirasi() => OyuncuSirasi(Hafta(), HaftaIlerleme(), Kayit.P.Kupa);

	public static int SezonOdulu(int sira) => sira switch { 1 => 400, <= 3 => 250, <= 10 => 120, <= 25 => 60, _ => 25 };
}
