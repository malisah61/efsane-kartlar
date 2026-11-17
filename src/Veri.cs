using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

namespace EfsaneKartlar;

/// <summary>Bir yetenek etkisi (kartlar.json → ulti.etkiler).</summary>
public class Etki
{
	public string Tip = "";
	public string Hedef = "tek";
	public float Carpan, Oran, Sans, CanCal;
	public int Tur, Adet = 1, Miktar;
}

/// <summary>Kartın değişmeyen tanımı (seviye 1, 1 yıldız değerleri).</summary>
public class KartTanim
{
	public string Id, Ad, Mitoloji, Nadirlik, Sinif, Element, UltiAd;
	public float Can, Saldiri, Savunma, Hiz;
	public List<Etki> Etkiler = new();
	public int NadirlikSira => Veri.NadirlikSirasi(Nadirlik);
}

public static class Veri
{
	public static List<KartTanim> Kartlar = new();
	public static Dictionary<string, KartTanim> Harita = new();
	public static Dictionary<string, string> Mitolojiler = new();
	static readonly Dictionary<string, Texture2D> _portreler = new();

	public static readonly string[] NadirlikSirasiDizi = { "siradan", "nadir", "epik", "efsanevi" };
	public static int NadirlikSirasi(string n) => Array.IndexOf(NadirlikSirasiDizi, n);

	public static string NadirlikAd(string n) => n switch
	{
		"siradan" => "Sıradan", "nadir" => "Nadir", "epik" => "Epik", "efsanevi" => "Efsanevi", _ => n
	};
	public static Color NadirlikRenk(string n) => n switch
	{
		"siradan" => new Color("9aa3ad"), "nadir" => new Color("3f8fe8"), "epik" => new Color("a64fe0"),
		"efsanevi" => new Color("f2b233"), _ => Colors.White
	};
	public static string SinifAd(string s) => s switch
	{
		"tank" => "Tank", "savasci" => "Savaşçı", "buyucu" => "Büyücü", "destek" => "Destek", _ => s
	};
	public static string SinifHarf(string s) => s switch
	{
		"tank" => "T", "savasci" => "S", "buyucu" => "B", "destek" => "D", _ => "?"
	};
	public static string ElementAd(string e) => e switch
	{
		"ates" => "Ateş", "su" => "Su", "doga" => "Doğa", _ => e
	};
	public static Color ElementRenk(string e) => e switch
	{
		"ates" => new Color("e8552f"), "su" => new Color("2f9be8"), "doga" => new Color("4cbf4a"), _ => Colors.White
	};
	/// <summary>Ateş > Doğa > Su > Ateş. Dönüş: 1 avantaj, -1 dezavantaj, 0 nötr.</summary>
	public static int ElementUstunluk(string saldiran, string hedef)
	{
		if (saldiran == hedef) return 0;
		bool ustun = (saldiran == "ates" && hedef == "doga") || (saldiran == "doga" && hedef == "su") || (saldiran == "su" && hedef == "ates");
		return ustun ? 1 : -1;
	}
	public static string MitolojiAd(string m) => Mitolojiler.TryGetValue(m, out var a) ? a : m;

	public static void Yukle()
	{
		if (Kartlar.Count > 0) return;
		using var f = FileAccess.Open("res://assets/veri/kartlar.json", FileAccess.ModeFlags.Read);
		YukleMetin(f.GetAsText());
	}

	public static void YukleMetin(string json)
	{
		Kartlar.Clear(); Harita.Clear(); Mitolojiler.Clear();
		using var doc = JsonDocument.Parse(json);
		foreach (var p in doc.RootElement.GetProperty("mitolojiler").EnumerateObject())
			Mitolojiler[p.Name] = p.Value.GetString();
		foreach (var k in doc.RootElement.GetProperty("kartlar").EnumerateArray())
		{
			var t = new KartTanim
			{
				Id = k.GetProperty("id").GetString(),
				Ad = k.GetProperty("ad").GetString(),
				Mitoloji = k.GetProperty("mitoloji").GetString(),
				Nadirlik = k.GetProperty("nadirlik").GetString(),
				Sinif = k.GetProperty("sinif").GetString(),
				Element = k.GetProperty("element").GetString(),
				Can = k.GetProperty("can").GetSingle(),
				Saldiri = k.GetProperty("saldiri").GetSingle(),
				Savunma = k.GetProperty("savunma").GetSingle(),
				Hiz = k.GetProperty("hiz").GetSingle(),
			};
			var u = k.GetProperty("ulti");
			t.UltiAd = u.GetProperty("ad").GetString();
			foreach (var e in u.GetProperty("etkiler").EnumerateArray())
			{
				var et = new Etki { Tip = e.GetProperty("tip").GetString(), Hedef = e.GetProperty("hedef").GetString() };
				if (e.TryGetProperty("carpan", out var v)) et.Carpan = v.GetSingle();
				if (e.TryGetProperty("oran", out v)) et.Oran = v.GetSingle();
				if (e.TryGetProperty("sans", out v)) et.Sans = v.GetSingle();
				if (e.TryGetProperty("can_cal", out v)) et.CanCal = v.GetSingle();
				if (e.TryGetProperty("tur", out v)) et.Tur = v.GetInt32();
				if (e.TryGetProperty("adet", out v)) et.Adet = v.GetInt32();
				if (e.TryGetProperty("miktar", out v)) et.Miktar = v.GetInt32();
				t.Etkiler.Add(et);
			}
			Kartlar.Add(t);
			Harita[t.Id] = t;
		}
	}

	// ------------------------------------------------------------ statlar
	public static float SeviyeCarpan(int seviye, int yildiz) => (1f + 0.07f * (seviye - 1)) * (1f + 0.12f * (yildiz - 1));

	public static (float can, float sal, float sav, float hiz) Stat(KartTanim t, int seviye, int yildiz)
	{
		float m = SeviyeCarpan(seviye, yildiz);
		return (t.Can * m, t.Saldiri * m, t.Savunma * m, t.Hiz * (1f + 0.003f * (seviye - 1) + 0.01f * (yildiz - 1)));
	}

	/// <summary>Eşleştirme ve gösterim için tek sayılık güç.</summary>
	public static int Guc(KartTanim t, int seviye, int yildiz)
	{
		var s = Stat(t, seviye, yildiz);
		return (int)(s.can * 0.1f + s.sal * 1.6f + s.sav * 1.4f + s.hiz * 0.5f);
	}

	// ------------------------------------------------------------ ulti açıklaması
	public static string UltiAciklama(KartTanim t)
	{
		var parca = new List<string>();
		foreach (var e in t.Etkiler)
		{
			string h = HedefAd(e.Hedef, e.Adet);
			parca.Add(e.Tip switch
			{
				"hasar" => $"{h} %{e.Carpan * 100:0} hasar" + (e.CanCal > 0 ? $" (%{e.CanCal * 100:0} can çalar)" : ""),
				"iyilestir" => $"{h} %{e.Carpan * 100:0} iyileştirme",
				"kalkan" => $"{h} canının %{e.Oran * 100:0}'i kadar kalkan ({e.Tur} tur)",
				"sersemlet" => $"{h} %{e.Sans * 100:0} şansla sersemletir",
				"yanma" => $"{h} {e.Tur} tur boyunca yakar/zehirler",
				"guclendir" => $"{h} saldırı +%{e.Oran * 100:0} ({e.Tur} tur)",
				"zayiflat" => $"{h} savunma -%{e.Oran * 100:0} ({e.Tur} tur)",
				"alay" => $"{e.Tur} tur düşmanları kendine çeker",
				"enerji" => $"{h} +{e.Miktar} enerji",
				_ => e.Tip
			});
		}
		return string.Join(". ", parca) + ".";
	}

	static string HedefAd(string h, int adet) => h switch
	{
		"tek" => "Bir düşmana", "tum" => "Tüm düşmanlara", "arka" => "Arka sıraya",
		"en_zayif" => "En zayıf düşmana", "rastgele" => $"Rastgele {adet} düşmana", "son" => "Vurduklarına",
		"kendi" => "Kendine", "tum_dost" => "Tüm takıma", "en_zayif_dost" => "En zayıf dosta", _ => h
	};

	// ------------------------------------------------------------ portreler
	public static Texture2D Portre(string id)
	{
		if (_portreler.TryGetValue(id, out var t)) return t;
		string yol = $"res://assets/kartlar/{id}.png";
		t = ResourceLoader.Exists(yol) ? GD.Load<Texture2D>(yol) : null;
		_portreler[id] = t;
		return t;
	}
}
