using System;
using System.Collections.Generic;
using System.Linq;
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
	static readonly Dictionary<string, Texture2D> _bedenler = new();

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
		YukleMetin(FileAccess.GetFileAsString("res://assets/veri/kartlar.json"));
	}

	public static void YukleMetin(string json)
	{
		Kartlar.Clear(); Harita.Clear(); Mitolojiler.Clear();
		var kok = Json.ParseString(json).AsGodotDictionary();
		foreach (var (k, v) in kok["mitolojiler"].AsGodotDictionary())
			Mitolojiler[k.AsString()] = v.AsString();
		foreach (var kv in kok["kartlar"].AsGodotArray())
		{
			var k = kv.AsGodotDictionary();
			var t = new KartTanim
			{
				Id = k["id"].AsString(),
				Ad = k["ad"].AsString(),
				Mitoloji = k["mitoloji"].AsString(),
				Nadirlik = k["nadirlik"].AsString(),
				Sinif = k["sinif"].AsString(),
				Element = k["element"].AsString(),
				Can = k["can"].AsSingle(),
				Saldiri = k["saldiri"].AsSingle(),
				Savunma = k["savunma"].AsSingle(),
				Hiz = k["hiz"].AsSingle(),
			};
			var u = k["ulti"].AsGodotDictionary();
			t.UltiAd = u["ad"].AsString();
			foreach (var ev in u["etkiler"].AsGodotArray())
			{
				var e = ev.AsGodotDictionary();
				var et = new Etki { Tip = e["tip"].AsString(), Hedef = e["hedef"].AsString() };
				if (e.ContainsKey("carpan")) et.Carpan = e["carpan"].AsSingle();
				if (e.ContainsKey("oran")) et.Oran = e["oran"].AsSingle();
				if (e.ContainsKey("sans")) et.Sans = e["sans"].AsSingle();
				if (e.ContainsKey("can_cal")) et.CanCal = e["can_cal"].AsSingle();
				if (e.ContainsKey("tur")) et.Tur = e["tur"].AsInt32();
				if (e.ContainsKey("adet")) et.Adet = e["adet"].AsInt32();
				if (e.ContainsKey("miktar")) et.Miktar = e["miktar"].AsInt32();
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
	/// <summary>Savaş sahnesindeki tam boy görünüm (assets/beden/<id>.png).</summary>
	public static Texture2D Beden(string id)
	{
		if (_bedenler.TryGetValue(id, out var t)) return t;
		string yol = $"res://assets/beden/{id}.png";
		t = ResourceLoader.Exists(yol) ? GD.Load<Texture2D>(yol) : Portre(id);
		_bedenler[id] = t;
		return t;
	}

	public static Texture2D Portre(string id)
	{
		if (_portreler.TryGetValue(id, out var t)) return t;
		string yol = $"res://assets/kartlar/{id}.png";
		t = ResourceLoader.Exists(yol) ? GD.Load<Texture2D>(yol) : null;
		_portreler[id] = t;
		return t;
	}
}
