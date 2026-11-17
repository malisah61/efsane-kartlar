using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

public class ArenaRakip
{
	public string Ad;
	public int Kupa, Guc, Zorluk;   // Zorluk 0 kolay, 1 denk, 2 zor
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

	public static int AsamaSeviye(int asama) => Math.Max(1, (int)MathF.Round(1 + (asama - 1) * 0.85f));

	public static List<TakimUyesi> AsamaTakimi(int asama)
	{
		var r = new Rng((ulong)(asama * 7919 + 13));
		int sev = AsamaSeviye(asama);
		// aşama ilerledikçe nadirlik artar
		float epik = Math.Clamp((asama - 8) / 50f, 0, 0.45f), nadir = Math.Clamp(0.25f + asama / 60f, 0, 0.5f);
		var takim = new List<TakimUyesi>();
		var kullanilan = new HashSet<string>();
		// tutarlı diziliş: 2 ön (tank/savaşçı ağırlıklı), 3 arka
		string[] sinifSira = { "tank", "savasci", "buyucu", "destek", "buyucu" };
		for (int i = 0; i < 5; i++)
		{
			string nad;
			if (BossAsamasi(asama) && i == 2) nad = asama >= 18 ? "efsanevi" : "epik";
			else { float x = r.Kesir(); nad = x < epik ? "epik" : x < epik + nadir ? "nadir" : "siradan"; }
			string sinif = sinifSira[i];
			if (r.Kesir() < 0.3f) sinif = i < 2 ? (sinif == "tank" ? "savasci" : "tank") : (r.Kesir() < 0.5f ? "buyucu" : "savasci");
			var havuz = Veri.Kartlar.Where(k => k.Nadirlik == nad && k.Sinif == sinif && !kullanilan.Contains(k.Id)).ToList();
			if (havuz.Count == 0) havuz = Veri.Kartlar.Where(k => k.Nadirlik == nad && !kullanilan.Contains(k.Id)).ToList();
			var k = havuz[r.Aralik(havuz.Count)];
			kullanilan.Add(k.Id);
			int yildiz = 1 + Math.Min(4, asama / 15);
			takim.Add(new TakimUyesi(k.Id, Math.Min(sev, Kayit.MaxSeviye(yildiz)), yildiz));
		}
		return takim;
	}

	public static (int altin, int elmas, int parsomen) AsamaOdulu(int asama, bool ilkKez)
	{
		int altin = 200 + asama * 40;
		if (!ilkKez) return (altin / 2, 0, 0);
		return (altin, BossAsamasi(asama) ? 100 : 25, asama % 3 == 0 ? 1 : 0);
	}

	// ------------------------------------------------------------ arena
	static readonly string[] Onek = { "Kara", "Ak", "Gök", "Demir", "Kızıl", "Bozkurt", "Yıldız", "Fırtına", "Kartal", "Ay", "Gölge", "Altın", "Buz", "Ateş", "Rüzgar", "Tunç" };
	static readonly string[] Sonek = { "Han", "Bey", "Alp", "Kaya", "Ok", "Kılıç", "Er", "Tuğ", "Yürek", "Pençe", "Kalkan", "Bilge", "Avcı", "Atlı", "Yay" };

	public static string BotAdi(Rng r) => Onek[r.Aralik(Onek.Length)] + Sonek[r.Aralik(Sonek.Length)] + (r.Kesir() < 0.5f ? (r.Aralik(90) + 10).ToString() : "");

	public static string Lig(int kupa) => kupa switch
	{
		>= 2500 => "Efsane", >= 1600 => "Elmas", >= 900 => "Altın", >= 400 => "Gümüş", _ => "Bronz"
	};
	public static int SonrakiLigKupa(int kupa) => kupa < 400 ? 400 : kupa < 900 ? 900 : kupa < 1600 ? 1600 : kupa < 2500 ? 2500 : 0;

	public static List<ArenaRakip> ArenaRakipleri()
	{
		var p = Kayit.P;
		int gun = DateTime.Now.DayOfYear + DateTime.Now.Year * 400;
		ulong tohum = (ulong)(gun * 31 + p.ArenaYenileme * 977 + p.ArenaGalibiyet * 131 + p.ArenaMaglubiyet * 17 + 5);
		var r = new Rng(tohum);
		int guc = Math.Max(400, Kayit.TakimGucu());
		var l = new List<ArenaRakip>();
		float[] oran = { 0.82f, 1.0f, 1.18f };
		for (int z = 0; z < 3; z++)
		{
			var takim = HayaletTakim(r, (int)(guc * oran[z] * (0.96f + 0.08f * r.Kesir())));
			l.Add(new ArenaRakip
			{
				Ad = BotAdi(r), Zorluk = z, Takim = takim, Guc = Kayit.TakimGucu(takim),
				Kupa = Math.Max(0, p.Kupa + (z - 1) * 60 + r.Aralik(40) - 20), Tohum = r.Sonraki(),
			});
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
			else if (u.Yildiz < 5) u.Yildiz++;
		}
		return takim;
	}

	public static (int kupa, int altin, int elmas) ArenaOdulu(ArenaRakip r, bool kazandi)
	{
		if (!kazandi) return (-12, 50, 0);
		return (18 + r.Zorluk * 7, 250 + r.Zorluk * 100, 10 + r.Zorluk * 5);
	}

	/// <summary>Haftalık sıralama (prototip: botlar + oyuncu).</summary>
	public static List<(string ad, int kupa, bool oyuncu)> Siralama()
	{
		int hafta = (int)(DateTime.Now.DayOfYear / 7) + DateTime.Now.Year * 60;
		var r = new Rng((ulong)hafta * 2654435761UL);
		var l = new List<(string, int, bool)>();
		int taban = Math.Max(200, Kayit.P.Kupa);
		for (int i = 0; i < 49; i++)
			l.Add((BotAdi(r), Math.Max(0, (int)(taban * (0.4f + 1.4f * MathF.Pow(r.Kesir(), 1.7f))) + r.Aralik(60)), false));
		l.Add((Kayit.P.Ad, Kayit.P.Kupa, true));
		return l.OrderByDescending(x => x.Item2).ToList();
	}
}
