using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Bir savaşı yeniden oynatmak için gereken her şey: tohum + iki takım + etkinlik bonusu.
/// Savaş deterministik olduğu için aynı girdiler aynı savaşı üretir.</summary>
public class SavasKaydi
{
	public string Baslik = "", Arka = "turk", Tur = "hikaye", Zaman = "", BonusMit;
	public bool Boss, Kazandi, Favori;
	public ulong Tohum;
	public float BonusOran;
	public List<TakimUyesi> A = new(), B = new();

	public Savas Kur() => new(A, B, Tohum, BonusMit, BonusOran);

	public Godot.Collections.Dictionary Kodla() => new()
	{
		["b"] = Baslik, ["a"] = Arka, ["t"] = Tur, ["z"] = Zaman, ["m"] = BonusMit ?? "", ["o"] = BonusOran,
		["boss"] = Boss, ["k"] = Kazandi, ["f"] = Favori, ["s"] = Tohum.ToString(),
		["ta"] = TakimKodla(A), ["tb"] = TakimKodla(B),
	};

	public static Godot.Collections.Array TakimKodla(List<TakimUyesi> t)
	{
		var a = new Godot.Collections.Array();
		foreach (var u in t) a.Add(u == null ? new Godot.Collections.Array() : new Godot.Collections.Array { u.Id, u.Seviye, u.Yildiz });
		return a;
	}

	public static List<TakimUyesi> TakimCoz(Godot.Collections.Array a)
	{
		var l = new List<TakimUyesi>();
		foreach (var x in a)
		{
			var u = x.AsGodotArray();
			l.Add(u.Count < 3 ? null : new TakimUyesi(u[0].AsString(), u[1].AsInt32(), u[2].AsInt32()));
		}
		return l;
	}

	public static SavasKaydi Coz(Godot.Collections.Dictionary d)
	{
		string S(string k) => d.ContainsKey(k) ? d[k].AsString() : "";
		var k = new SavasKaydi
		{
			Baslik = S("b"), Arka = S("a") == "" ? "turk" : S("a"), Tur = S("t"), Zaman = S("z"),
			BonusMit = S("m") == "" ? null : S("m"), BonusOran = d.ContainsKey("o") ? (float)d["o"].AsDouble() : 0f,
			Boss = d.ContainsKey("boss") && d["boss"].AsBool(), Kazandi = d.ContainsKey("k") && d["k"].AsBool(),
			Favori = d.ContainsKey("f") && d["f"].AsBool(),
		};
		ulong.TryParse(S("s"), out k.Tohum);
		if (d.ContainsKey("ta")) k.A = TakimCoz(d["ta"].AsGodotArray());
		if (d.ContainsKey("tb")) k.B = TakimCoz(d["tb"].AsGodotArray());
		return k;
	}
}

/// <summary>Son savaşların tekrarları (yalnızca bu cihazda, user://tekrarlar.json; bulut kaydını şişirmesin diye ayrı dosya).
/// En fazla 10 son savaş + 5 favori saklanır.</summary>
public static class Tekrar
{
	const string Yol = "user://tekrarlar.json";
	public const int SonSayi = 10, FavoriSayi = 5;
	static List<SavasKaydi> _liste;

	public static List<SavasKaydi> Liste { get { Yukle(); return _liste; } }

	public static readonly Dictionary<string, string> TurAd = new()
	{
		["hikaye"] = "Hikâye", ["arena"] = "Arena", ["kule"] = "Kule", ["muhur"] = "Mühürlü Kat",
		["muhafiz"] = "Muhafız", ["dostluk"] = "Dostluk", ["ikili"] = "İkili Savaş", ["diger"] = "Savaş",
	};

	static void Yukle()
	{
		if (_liste != null) return;
		_liste = new();
		if (!FileAccess.FileExists(Yol)) return;
		try
		{
			foreach (var x in Json.ParseString(FileAccess.GetFileAsString(Yol)).AsGodotArray())
				_liste.Add(SavasKaydi.Coz(x.AsGodotDictionary()));
		}
		catch (Exception e) { GD.PrintErr("Tekrarlar okunamadı: " + e.Message); _liste.Clear(); }
	}

	static void Kaydet()
	{
		if (Kayit.KayitKapali) return;
		try
		{
			var a = new Godot.Collections.Array();
			foreach (var k in _liste) a.Add(k.Kodla());
			using var f = FileAccess.Open(Yol, FileAccess.ModeFlags.Write);
			f?.StoreString(Json.Stringify(a));
		}
		catch (Exception e) { GD.PrintErr("Tekrarlar yazılamadı: " + e.Message); }
	}

	/// <summary>Başlığa göre savaş türü (rozet ve filtre için).</summary>
	public static string TurBul(string baslik)
	{
		var b = baslik.ToLowerInvariant();
		if (b.Contains("mühür")) return "muhur";
		if (b.Contains("kule") || b.Contains("kat ")) return "kule";
		if (b.Contains("muhafız")) return "muhafiz";
		if (b.Contains("arena")) return "arena";
		if (b.Contains("dostluk")) return "dostluk";
		if (b.Contains("ikili")) return "ikili";
		if (b.Contains("aşama") || b.Contains("bölüm") || b.Contains("hikâye")) return "hikaye";
		return "diger";
	}

	public static void Ekle(SavasKaydi k)
	{
		Yukle();
		k.Zaman = DateTime.Now.ToString("dd.MM HH:mm");
		_liste.Insert(0, k);
		// favoriler korunur; favori olmayanlardan en eski fazlalar silinir
		var sil = _liste.Where(x => !x.Favori).Skip(SonSayi).ToList();
		foreach (var x in sil) _liste.Remove(x);
		Kaydet();
	}

	public static int FavoriAdet => Liste.Count(x => x.Favori);

	/// <summary>Favori yap/kaldır. Favori sınırı doluysa false.</summary>
	public static bool FavoriDegistir(SavasKaydi k)
	{
		if (!k.Favori && FavoriAdet >= FavoriSayi) return false;
		k.Favori = !k.Favori;
		if (!k.Favori)
		{
			var sil = _liste.Where(x => !x.Favori).Skip(SonSayi).ToList();
			foreach (var x in sil) _liste.Remove(x);
		}
		Kaydet();
		return true;
	}

	public static void Sil(SavasKaydi k) { Yukle(); _liste.Remove(k); Kaydet(); }
}
