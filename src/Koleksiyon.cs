using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>
/// Koleksiyon başarımları: mitoloji tamamlama (yarısı / hepsi), toplam kart sayısı, yıldız hedefleri ve
/// "bağ koleksiyoncusu" (bütün kartlarına sahip olduğun efsane bağı sayısı). Her başarım bir kez ödül verir.
/// Ödüller ömür boyu toplam ~2.000 elmas: uzun vadeli hedef, ekonomiyi hızlandırmaz.
/// </summary>
public static class Koleksiyon
{
	public record Basarim(string Id, string Grup, string Ad, string Aciklama, Func<Profil, int> Ilerleme, int Hedef,
		int Elmas = 0, int Parsomen = 0, int Oz = 0, int Altin = 0)
	{
		public string OdulYazi()
		{
			var l = new List<string>();
			if (Elmas > 0) l.Add($"{Elmas} elmas");
			if (Parsomen > 0) l.Add($"{Parsomen} parşömen");
			if (Oz > 0) l.Add($"{Oz} ruh özü");
			if (Altin > 0) l.Add($"{Altin:N0} altın");
			return string.Join(", ", l);
		}
	}

	static List<Basarim> _liste;
	static int _kartSayisi = -1;

	/// <summary>Kart verisi yüklendikten sonra kurulur (kart sayısı değişirse yeniden).</summary>
	public static List<Basarim> Liste
	{
		get
		{
			if (_liste == null || _kartSayisi != Veri.Kartlar.Count) { _liste = Olustur(); _kartSayisi = Veri.Kartlar.Count; }
			return _liste;
		}
	}

	static int Sahip(Profil p, Func<KartTanim, bool> f) => p.Kartlar.Count(k => Veri.Harita.TryGetValue(k.Id, out var t) && f(t));
	public static int TamBagSayisi(Profil p)
	{
		var h = p.Kartlar.Select(k => k.Id).ToHashSet();
		return Veri.Baglar.Count(b => b.Kartlar.All(h.Contains));
	}

	static List<Basarim> Olustur()
	{
		var l = new List<Basarim>();
		foreach (var (m, ad) in Veri.Mitolojiler)
		{
			int n = Veri.Kartlar.Count(k => k.Mitoloji == m);
			if (n == 0) continue;
			string mit = m;
			l.Add(new($"mit_yari_{m}", "Mitolojiler", $"{ad} Efsaneleri", $"{ad} kartlarının yarısını topla",
				p => Sahip(p, t => t.Mitoloji == mit), (n + 1) / 2, Elmas: 30, Altin: 2000));
			l.Add(new($"mit_tam_{m}", "Mitolojiler", $"{ad} Panteonu", $"Bütün {ad} kartlarını topla",
				p => Sahip(p, t => t.Mitoloji == mit), n, Elmas: 120, Parsomen: 1, Oz: 40));
		}
		int toplam = Veri.Kartlar.Count;
		foreach (var (h, e, pa) in new[] { (15, 20, 0), (30, 40, 0), (50, 60, 1), (70, 100, 1) })
			if (h < toplam) l.Add(new($"toplam_{h}", "Koleksiyon", $"{h} Kahraman", $"{h} farklı kart topla", p => p.Kartlar.Count, h, Elmas: e, Parsomen: pa));
		l.Add(new("toplam_hepsi", "Koleksiyon", "Efsaneler Kitabı", "Oyundaki bütün kartları topla", p => p.Kartlar.Count, toplam, Elmas: 200, Parsomen: 3, Oz: 100));

		int bag = Veri.Baglar.Count;
		foreach (var (h, e) in new[] { (5, 20), (15, 40), (30, 60) })
			if (h < bag) l.Add(new($"bag_{h}", "Bağlar", $"Bağ Koleksiyoncusu {h}", $"{h} efsane bağının bütün kartlarına sahip ol", TamBagSayisi, h, Elmas: e, Oz: 20));
		l.Add(new("bag_hepsi", "Bağlar", "Simya Ustası", "Bütün efsane bağlarının kartlarına sahip ol", TamBagSayisi, bag, Elmas: 150, Parsomen: 1));

		foreach (var (y, h, e, oz) in new[] { (3, 1, 20, 0), (3, 5, 40, 20), (3, 15, 80, 40), (5, 1, 50, 30), (5, 5, 100, 60), (7, 1, 200, 100) })
		{
			int yy = y;
			l.Add(new($"yildiz_{y}_{h}", "Yıldızlar", h == 1 ? $"İlk {y}★" : $"{h} Kart {y}★", h == 1 ? $"Bir kartı {y} yıldıza çıkar" : $"{h} kartı {y} yıldıza çıkar",
				p => p.Kartlar.Count(k => k.Yildiz >= yy), h, Elmas: e, Oz: oz));
		}
		foreach (var (sv, h, e, pa, oz) in new[] { (3, 1, 10, 0, 0), (5, 1, 20, 0, 10), (3, 5, 25, 0, 15), (5, 5, 50, 1, 20), (5, 15, 100, 1, 40) })
		{
			int ss = sv;
			l.Add(new($"ustalik_{sv}_{h}", "Ustalık", h == 1 ? $"İlk {Ustalik.Unvanlar[sv]}" : $"{h} {Ustalik.Unvanlar[sv]}",
				h == 1 ? $"Bir kahramanı ustalık {sv}'e çıkar" : $"{h} kahramanı ustalık {sv}'e çıkar",
				p => Ustalik.UstaSayisi(p, ss), h, Elmas: e, Parsomen: pa, Oz: oz));
		}
		return l;
	}

	public static bool Alindi(Basarim b) => Kayit.P.Basarimlar.Contains(b.Id);
	public static bool Tamam(Basarim b) => b.Ilerleme(Kayit.P) >= b.Hedef;
	public static bool Alinabilir(Basarim b) => Tamam(b) && !Alindi(b);
	public static int AlinabilirSayi() => Liste.Count(Alinabilir);

	/// <summary>Ödülü verir; dönüş ödül yazısı (alınamıyorsa null). Kaydetmez.</summary>
	static string Ver(Basarim b)
	{
		if (!Alinabilir(b)) return null;
		var p = Kayit.P;
		p.Basarimlar.Add(b.Id);
		p.Elmas += b.Elmas; p.Parsomen += b.Parsomen; p.RuhOzu += b.Oz; p.Altin += b.Altin;
		return b.OdulYazi();
	}

	public static string Al(Basarim b)
	{
		var s = Ver(b);
		if (s != null) Kayit.Kaydet();
		return s;
	}

	/// <summary>Alınabilir bütün ödülleri verir. Dönüş: (adet, toplam elmas).</summary>
	public static (int adet, int elmas) HepsiniAl()
	{
		int n = 0, e = 0;
		foreach (var b in Liste.Where(Alinabilir).ToList()) { Ver(b); n++; e += b.Elmas; }
		if (n > 0) Kayit.Kaydet();
		return (n, e);
	}
}
