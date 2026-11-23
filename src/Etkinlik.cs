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
	static readonly string[] Sira = { "turk", "yunan", "iskandinav", "misir", "japon" };
	static readonly Dictionary<string, string> Kahramanlar = new()
	{
		["turk"] = "koroglu", ["yunan"] = "perseus", ["iskandinav"] = "fenrir", ["misir"] = "sekhmet", ["japon"] = "susanoo",
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
}
