using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>
/// Haftalık etkinlik: "Mitoloji Haftası". Her hafta sırayla bir mitoloji öne çıkar:
///  - O mitolojinin kartları hikâye ve kulede +%10 can/saldırı alır (arenada yok, PvP adil kalsın).
///  - Çağırmada o mitolojinin kartları kendi nadirlik havuzunda 2 kat sık çıkar.
///  - Zaferler etkinlik puanı verir (takımdaki etkinlik kartı başına ek puan); puan eşiklerinde ödül,
///    son eşikte haftanın etkinlik kahramanı (o mitolojinin yeni epik kartı).
/// </summary>
public static class Etkinlik
{
	static readonly string[] Sira = { "turk", "yunan", "iskandinav", "misir", "japon", "cin" };
	static readonly Dictionary<string, string> Kahramanlar = new()
	{
		["turk"] = "koroglu", ["yunan"] = "perseus", ["iskandinav"] = "fenrir", ["misir"] = "sekhmet", ["japon"] = "susanoo", ["cin"] = "guan_yu",
	};
	public const float GucBonus = 0.10f;
	public const float CekimAgirlik = 2f;

	public static int Hafta => Rakipler.Hafta();
	public static string Mitoloji => Sira[((Hafta % Sira.Length) + Sira.Length) % Sira.Length];
	public static string Ad => Veri.MitolojiAd(Mitoloji) + " Haftası";
	public static string KahramanId => Kahramanlar[Mitoloji];
	public static KartTanim Kahraman => Veri.Harita.TryGetValue(KahramanId, out var t) ? t : null;
	public static TimeSpan Kalan => Rakipler.SezonKalan();

	public record Odul(int Puan, int Altin = 0, int Elmas = 0, int Parsomen = 0, int Oz = 0, bool KahramanKopya = false)
	{
		public string Yazi()
		{
			var l = new List<string>();
			if (Altin > 0) l.Add($"{Altin:N0} altın");
			if (Elmas > 0) l.Add($"{Elmas} elmas");
			if (Parsomen > 0) l.Add($"{Parsomen} parşömen");
			if (Oz > 0) l.Add($"{Oz} ruh özü");
			if (KahramanKopya) l.Add(Kahraman?.Ad ?? "etkinlik kahramanı");
			return string.Join(", ", l);
		}
	}

	public static readonly Odul[] Oduller =
	{
		new(100, Altin: 3000),
		new(250, Elmas: 60),
		new(450, Parsomen: 1),
		new(700, Oz: 40),
		new(1000, Elmas: 120),
		new(1400, Parsomen: 2),
		new(2000, KahramanKopya: true),
	};

	/// <summary>Hafta değiştiyse puanı ve alınan ödülleri sıfırlar.</summary>
	public static void Kontrol()
	{
		var p = Kayit.P;
		if (p.EtkinlikHafta == Hafta) return;
		p.EtkinlikHafta = Hafta;
		p.EtkinlikPuan = 0;
		p.EtkinlikAlinan.Clear();
		p.EtkinlikJeton = 0;
		p.EtkinlikAlim.Clear();
	}

	public static int Puan { get { Kontrol(); return Kayit.P.EtkinlikPuan; } }

	public static int TakimdakiSayi(IEnumerable<TakimUyesi> t) =>
		t.Count(u => u != null && Veri.Harita.TryGetValue(u.Id, out var k) && k.Mitoloji == Mitoloji);

	/// <summary>Zafer puanı: hikâye/kule 10, arena 15; takımdaki her etkinlik kartı +5.</summary>
	public static int ZaferPuani(bool arena) => (arena ? 15 : 10) + 5 * TakimdakiSayi(Kayit.TakimUyeleri());

	public static int PuanEkle(int n)
	{
		Kontrol();
		Kayit.P.EtkinlikPuan += n;
		Kayit.P.EtkinlikJeton += n;   // kazanılan her puan mağazada harcanabilir bir jeton da verir
		return n;
	}

	public static bool Alindi(int i) { Kontrol(); return Kayit.P.EtkinlikAlinan.Contains(i); }
	public static bool Alinabilir(int i) => !Alindi(i) && Puan >= Oduller[i].Puan;
	public static int AlinabilirSayi() => Enumerable.Range(0, Oduller.Length).Count(Alinabilir);

	public static string Al(int i)
	{
		if (!Alinabilir(i)) return null;
		var o = Oduller[i];
		var p = Kayit.P;
		p.EtkinlikAlinan.Add(i);
		p.Altin += o.Altin; p.Elmas += o.Elmas; p.Parsomen += o.Parsomen; p.RuhOzu += o.Oz;
		string ek = "";
		if (o.KahramanKopya && Kahraman != null)
			ek = Kayit.KartEkle(KahramanId) ? " (YENİ KART!)" : " (+1 kopya)";
		Kayit.Kaydet();
		return o.Yazi() + ek;
	}

	public static List<string> HepsiniAl()
	{
		var l = new List<string>();
		for (int i = 0; i < Oduller.Length; i++) { var s = Al(i); if (s != null) l.Add(s); }
		return l;
	}

	/// <summary>Çağırma havuzundan seçer; etkinlik mitolojisinin kartları daha ağır basar.</summary>
	public static KartTanim HavuzdanSec(List<KartTanim> havuz, Random r)
	{
		string m = Mitoloji;
		double toplam = havuz.Sum(k => k.Mitoloji == m ? CekimAgirlik : 1.0);
		double x = r.NextDouble() * toplam;
		foreach (var k in havuz)
		{
			x -= k.Mitoloji == m ? CekimAgirlik : 1.0;
			if (x < 0) return k;
		}
		return havuz[^1];
	}

	// ------------------------------------------------------------ etkinlik mağazası (jetonla takas)
	public record Urun(string Ad, int Fiyat, int Limit, string Tur);

	public static Urun[] Urunler => new Urun[]
	{
		new("2.000 altın", 120, 5, "altin"),
		new("20 ruh özü", 200, 3, "oz"),
		new("1 parşömen", 300, 2, "parsomen"),
		new($"Rastgele nadir {Veri.MitolojiAd(Mitoloji)} kartı", 400, 2, "nadir"),
		new($"Rastgele epik {Veri.MitolojiAd(Mitoloji)} kartı", 1000, 1, "epik"),
		new($"{Kahraman?.Ad ?? "Etkinlik kahramanı"} kopyası", 1600, 1, "kahraman"),
	};

	public static int Jeton { get { Kontrol(); return Kayit.P.EtkinlikJeton; } }
	public static int Alinan(int i) { Kontrol(); return Kayit.P.EtkinlikAlim.Count(x => x == i); }
	public static bool Alinabilir(Urun u, int i) => Alinan(i) < u.Limit && Jeton >= u.Fiyat;

	/// <summary>Mağazadan ürün alır. Dönüş: sonuç yazısı (alınamadıysa null).</summary>
	public static string UrunAl(int i, Random r = null)
	{
		var u = Urunler[i];
		if (!Alinabilir(u, i)) return null;
		var p = Kayit.P;
		r ??= new Random();
		string sonuc;
		switch (u.Tur)
		{
			case "altin": p.Altin += 2000; sonuc = "+2.000 altın"; break;
			case "oz": p.RuhOzu += 20; sonuc = "+20 ruh özü"; break;
			case "parsomen": p.Parsomen++; sonuc = "+1 parşömen"; break;
			case "nadir":
			case "epik":
			{
				var havuz = Veri.Kartlar.Where(k => k.Mitoloji == Mitoloji && k.Nadirlik == u.Tur && k.Id != KahramanId).ToList();
				if (havuz.Count == 0) havuz = Veri.Kartlar.Where(k => k.Nadirlik == u.Tur).ToList();
				var k = havuz[r.Next(havuz.Count)];
				sonuc = k.Ad + (Kayit.KartEkle(k.Id) ? " (YENİ KART!)" : " (+1 kopya)");
				break;
			}
			default:
				if (Kahraman == null) return null;
				sonuc = Kahraman.Ad + (Kayit.KartEkle(KahramanId) ? " (YENİ KART!)" : " (+1 kopya)");
				break;
		}
		p.EtkinlikJeton -= u.Fiyat;
		p.EtkinlikAlim.Add(i);
		Kayit.Kaydet();
		return sonuc;
	}

	// ------------------------------------------------------------ mitoloji muhafızları (günlük etkinlik boss'u)
	public const int MuhafizHak = 3;
	public static int MuhafizKalan => Math.Max(0, MuhafizHak - Kayit.P.MuhafizSavas);

	/// <summary>Takımın ortalama seviye/yıldızına göre ölçeklenen, o haftanın mitolojisinden en güçlü 5 kart.</summary>
	public static List<TakimUyesi> MuhafizTakimi()
	{
		var bizim = Kayit.TakimUyeleri().Where(u => u != null).ToList();
		int sev = bizim.Count == 0 ? 1 : (int)Math.Round(bizim.Average(u => u.Seviye));
		int yil = bizim.Count == 0 ? 1 : Math.Max(1, (int)Math.Round(bizim.Average(u => u.Yildiz)));
		var r = new Rng((ulong)(Hafta * 7727 + 11));
		var kullanilan = new HashSet<string>();
		var takim = new List<TakimUyesi>();
		foreach (var sinif in new[] { "tank", "savasci", "buyucu", "destek", "buyucu" })
		{
			var havuz = Veri.Kartlar.Where(k => k.Mitoloji == Mitoloji && !kullanilan.Contains(k.Id)).ToList();
			var uygun = havuz.Where(k => k.Sinif == sinif).ToList();
			if (uygun.Count > 0) havuz = uygun;
			int enIyi = havuz.Max(k => k.NadirlikSira);
			// en yüksek nadirlikten biri, ama efsanevi en fazla bir kez (orta yer)
			var aday = havuz.Where(k => k.NadirlikSira == enIyi).ToList();
			if (enIyi >= 3 && takim.Any(u => Veri.Harita[u.Id].NadirlikSira >= 3))
			{
				var alt = havuz.Where(k => k.NadirlikSira < 3).ToList();
				if (alt.Count > 0) { int m = alt.Max(k => k.NadirlikSira); aday = alt.Where(k => k.NadirlikSira == m).ToList(); }
			}
			var k = aday[r.Aralik(aday.Count)];
			kullanilan.Add(k.Id);
			takim.Add(new TakimUyesi(k.Id, Math.Min(sev, Kayit.MaxSeviye(yil)), yil));
		}
		return takim;
	}

	/// <summary>Muhafız zaferi: 4 kat zafer puanı + altın; yenilgide teselli puanı.</summary>
	public static List<string> MuhafizSonuc(bool kazandi)
	{
		var p = Kayit.P;
		var l = new List<string>();
		if (kazandi)
		{
			int puan = PuanEkle(ZaferPuani(false) * 4);
			p.Altin += 800;
			l.Add($"+{puan} etkinlik puanı ve jeton");
			l.Add("+800 altın");
		}
		else l.Add($"+{PuanEkle(10)} etkinlik puanı (teselli)");
		Kayit.Kaydet();
		return l;
	}
}
