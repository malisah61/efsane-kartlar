using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

public class SahipKart
{
	public string Id { get; set; }
	public int Seviye { get; set; } = 1;
	public int Yildiz { get; set; } = 1;
	public int Kopya { get; set; }
}

/// <summary>Oyuncunun kalıcı verisi (user://kayit.json).</summary>
public class Profil
{
	public string Ad { get; set; } = "Kahraman";
	public int Altin { get; set; }
	public int Elmas { get; set; }
	public int Parsomen { get; set; }
	public int Kupa { get; set; }
	public int ArenaHak { get; set; } = 5;
	public int ArenaYenileme { get; set; }
	public int ArenaGalibiyet { get; set; }
	public int ArenaMaglubiyet { get; set; }
	public int EnIyiAsama { get; set; }          // geçilen en yüksek hikâye aşaması
	public int ElmasCekimSayaci { get; set; }    // efsanevi garantisi için
	public string SonGun { get; set; } = "";
	public int GirisSerisi { get; set; }         // 0-6
	public bool GirisOdulAlindi { get; set; }
	public int GunlukSavas { get; set; }
	public int GunlukCekim { get; set; }
	public int GunlukYukseltme { get; set; }
	public List<int> AlinanGorevler { get; set; } = new();
	public List<SahipKart> Kartlar { get; set; } = new();
	public List<string> Takim { get; set; } = new() { null, null, null, null, null };
	public int SonKayitSurumu { get; set; } = 1;
	public int Enerji { get; set; } = 60;
	public long EnerjiZaman { get; set; }
	public int EnerjiAlim { get; set; }          // bugün elmasla alınan enerji sayısı
	public int Sezon { get; set; } = -1;         // hafta numarası
	public int GunlukHikaye { get; set; }
	public int ArenaGunlukSavas { get; set; }
	public bool MuzikAcik { get; set; } = true;
	public bool EfektAcik { get; set; } = true;
	public List<string> AlinanUrunler { get; set; } = new();
	public string AylikKartBitis { get; set; } = "";
	public string AylikKartSonAlim { get; set; } = "";
	public long ToplamHarcamaKurus { get; set; }
	public List<string> IslenenJetonlar { get; set; } = new();
	public int RuhOzu { get; set; }
	// reklam: günlük sayaçlar (elmas, ödül katlama, enerji, ücretsiz çağrı) ve geçiş reklamı sayacı
	public int[] ReklamGunluk { get; set; } = new int[4];
	public int SavasSayaci { get; set; }
	// Efsaneler Kulesi ve aylık sezon kartı (Efsane Yolu)
	public int KuleKat { get; set; }
	public string SezonKartiKimlik { get; set; } = "";
	public int SezonXp { get; set; }
	public string SezonPremium { get; set; } = "";
	public List<int> SezonAlinan { get; set; } = new();
	public List<int> SezonAlinanP { get; set; } = new();
	// haftalık mitoloji etkinliği
	public int EtkinlikHafta { get; set; } = -1;
	public int EtkinlikPuan { get; set; }
	public List<int> EtkinlikAlinan { get; set; } = new();
	public bool Reklamsiz => AlinanUrunler.Contains("reklam_kaldir");
}

public static class Kayit
{
	const string Yol = "user://kayit.json";
	public static Profil P = new();
	public static bool KayitKapali;   // önizleme/test modunda diske yazma

	public const int ElmasCekimFiyat = 300;
	public const int ElmasOnluFiyat = 2700;
	// Oyun sonu altın harcama yeri: altınla parşömen (günde sınırlı).
	public const int AltinParsomenFiyat = 25000, GunlukTakasHak = 3;
	public static int TakasKalan => GunlukTakasHak - P.AlinanGorevler.Count(x => x >= 200 && x < 200 + GunlukTakasHak);
	public static bool AltinlaParsomen()
	{
		if (TakasKalan <= 0 || P.Altin < AltinParsomenFiyat) return false;
		P.Altin -= AltinParsomenFiyat; P.Parsomen++;
		P.AlinanGorevler.Add(200 + GunlukTakasHak - TakasKalan);
		Kaydet();
		return true;
	}
	public const int EfsaneviGaranti = 30;

	// ------------------------------------------------------------ reklam ödülleri
	// 0: +elmas, 1: hikâye ödülünü 2 kat, 2: +enerji, 3: ücretsiz çağrı
	public static readonly int[] ReklamHak = { 6, 8, 3, 1 };
	public const int ReklamElmas = 25, ReklamEnerji = 12;
	public static int ReklamKalan(int tur) => Math.Max(0, ReklamHak[tur] - P.ReklamGunluk[tur]);
	public static bool ReklamOdulu(int tur)
	{
		if (ReklamKalan(tur) <= 0) return false;
		P.ReklamGunluk[tur]++;
		if (tur == 0) P.Elmas += ReklamElmas;
		else if (tur == 2) { EnerjiGuncelle(); P.Enerji += ReklamEnerji; }
		Kaydet();
		return true;
	}
	public const int ArenaGunlukHak = 5;
	public const int ArenaEkHakFiyat = 80;
	public const int EnerjiMax = 60;
	public const int EnerjiSaniye = 360;          // 6 dakikada 1 enerji
	public const int HikayeEnerji = 6;
	public const int EnerjiAlimFiyat = 80;        // 30 enerji
	public const int EnerjiAlimMiktar = 30;
	public const int EnerjiAlimGunluk = 3;

	public static readonly (string ad, int altin, int elmas, int parsomen)[] GirisOdulleri =
	{
		("1. Gün", 800, 0, 0), ("2. Gün", 0, 20, 0), ("3. Gün", 0, 0, 1), ("4. Gün", 1500, 0, 0),
		("5. Gün", 0, 40, 0), ("6. Gün", 0, 0, 1), ("7. Gün", 0, 100, 0),
	};

	public static readonly (string ad, Func<Profil, int> ilerleme, int hedef, int elmas)[] Gorevler =
	{
		("5 hikâye savaşı yap", p => p.GunlukHikaye, 5, 15),
		("3 arena savaşı yap", p => p.ArenaGunlukSavas, 3, 15),
		("1 kart çağır", p => p.GunlukCekim, 1, 10),
		("2 kez kart yükselt", p => p.GunlukYukseltme, 2, 10),
		("2 reklam izle", p => p.ReklamGunluk[0] + p.ReklamGunluk[1] + p.ReklamGunluk[2] + p.ReklamGunluk[3], 2, 20),
	};

	// ------------------------------------------------------------ kaydet / yükle
	public static void Yukle()
	{
		if (FileAccess.FileExists(Yol))
		{
			try
			{
				P = Coz(FileAccess.GetFileAsString(Yol));
				P.Kartlar.RemoveAll(k => !Veri.Harita.ContainsKey(k.Id));
				while (P.Takim.Count < 5) P.Takim.Add(null);
				for (int i = 0; i < 5; i++) if (P.Takim[i] != null && Sahip(P.Takim[i]) == null) P.Takim[i] = null;
				if (P.Kartlar.Count > 0) { GunKontrol(); return; }
			}
			catch (Exception e) { GD.PrintErr("Kayıt okunamadı: " + e.Message); }
		}
		YeniOyun();
	}

	/// <summary>Her kayıttan sonra çağrılır (bulut kaydı bunu dinler).</summary>
	public static event Action Degisti;

	public static void Kaydet()
	{
		if (KayitKapali) return;
		try
		{
			using var f = FileAccess.Open(Yol, FileAccess.ModeFlags.Write);
			f?.StoreString(Json.Stringify(Kodla(P)));
		}
		catch (Exception e) { GD.PrintErr("Kayıt yazılamadı: " + e.Message); }
		Degisti?.Invoke();
	}

	// ------------------------------------------------------------ bulut kaydı yardımcıları
	public static string Metin() => Json.Stringify(Kodla(P));

	public static Profil MetindenProfil(string metin)
	{
		try
		{
			var p = Coz(metin);
			p.Kartlar.RemoveAll(k => !Veri.Harita.ContainsKey(k.Id));
			while (p.Takim.Count < 5) p.Takim.Add(null);
			for (int i = 0; i < 5; i++) if (p.Takim[i] != null && !p.Kartlar.Any(k => k.Id == p.Takim[i])) p.Takim[i] = null;
			return p.Kartlar.Count > 0 ? p : null;
		}
		catch (Exception e) { GD.PrintErr("Bulut kaydı çözülemedi: " + e.Message); return null; }
	}

	public static string Ozet(Profil p) =>
		$"{p.Kartlar.Count} kart • aşama {p.EnIyiAsama}/60 • {p.Elmas:N0} elmas • {p.Altin:N0} altın";

	public static void ProfiliDegistir(Profil p)
	{
		P = p;
		GunKontrol();
		Kaydet();
	}

	// Godot Json ile elle (Android'de yansıma tabanlı serileştirmeye güvenmemek için)
	static Godot.Collections.Dictionary Kodla(Profil p)
	{
		var kartlar = new Godot.Collections.Array();
		foreach (var k in p.Kartlar)
			kartlar.Add(new Godot.Collections.Dictionary { ["id"] = k.Id, ["sv"] = k.Seviye, ["y"] = k.Yildiz, ["k"] = k.Kopya });
		var takim = new Godot.Collections.Array();
		foreach (var t in p.Takim) takim.Add(t ?? "");
		var gorev = new Godot.Collections.Array();
		foreach (var g in p.AlinanGorevler) gorev.Add(g);
		var urunler = new Godot.Collections.Array();
		foreach (var u in p.AlinanUrunler) urunler.Add(u);
		var jetonlar = new Godot.Collections.Array();
		foreach (var j in p.IslenenJetonlar) jetonlar.Add(j);
		return new Godot.Collections.Dictionary
		{
			["ad"] = p.Ad, ["altin"] = p.Altin, ["elmas"] = p.Elmas, ["parsomen"] = p.Parsomen, ["kupa"] = p.Kupa,
			["arenaHak"] = p.ArenaHak, ["arenaYenileme"] = p.ArenaYenileme, ["arenaG"] = p.ArenaGalibiyet, ["arenaM"] = p.ArenaMaglubiyet,
			["enIyiAsama"] = p.EnIyiAsama, ["cekimSayaci"] = p.ElmasCekimSayaci, ["sonGun"] = p.SonGun, ["girisSerisi"] = p.GirisSerisi,
			["girisAlindi"] = p.GirisOdulAlindi, ["gSavas"] = p.GunlukSavas, ["gCekim"] = p.GunlukCekim, ["gYukseltme"] = p.GunlukYukseltme,
			["gorevler"] = gorev, ["kartlar"] = kartlar, ["takim"] = takim, ["surum"] = p.SonKayitSurumu,
			["enerji"] = p.Enerji, ["enerjiZaman"] = p.EnerjiZaman, ["enerjiAlim"] = p.EnerjiAlim, ["sezon"] = p.Sezon,
			["gHikaye"] = p.GunlukHikaye, ["gArena"] = p.ArenaGunlukSavas, ["muzik"] = p.MuzikAcik, ["efekt"] = p.EfektAcik,
			["urunler"] = urunler, ["oz"] = p.RuhOzu, ["jetonlar"] = jetonlar, ["aylikBitis"] = p.AylikKartBitis,
			["aylikSon"] = p.AylikKartSonAlim, ["harcama"] = p.ToplamHarcamaKurus,
			["reklam"] = new Godot.Collections.Array { p.ReklamGunluk[0], p.ReklamGunluk[1], p.ReklamGunluk[2], p.ReklamGunluk[3] },
			["savasSayaci"] = p.SavasSayaci, ["kule"] = p.KuleKat,
			["skKimlik"] = p.SezonKartiKimlik, ["skXp"] = p.SezonXp, ["skPremium"] = p.SezonPremium,
			["skAlinan"] = Dizi(p.SezonAlinan), ["skAlinanP"] = Dizi(p.SezonAlinanP),
			["etHafta"] = p.EtkinlikHafta, ["etPuan"] = p.EtkinlikPuan, ["etAlinan"] = Dizi(p.EtkinlikAlinan),
		};
	}

	static Godot.Collections.Array Dizi(List<int> l)
	{
		var a = new Godot.Collections.Array();
		foreach (var x in l) a.Add(x);
		return a;
	}

	static Profil Coz(string metin)
	{
		var d = Json.ParseString(metin).AsGodotDictionary();
		int I(string a) => d.ContainsKey(a) ? d[a].AsInt32() : 0;
		var p = new Profil
		{
			Ad = d.ContainsKey("ad") ? d["ad"].AsString() : "Kahraman",
			Altin = I("altin"), Elmas = I("elmas"), Parsomen = I("parsomen"), Kupa = I("kupa"),
			ArenaHak = I("arenaHak"), ArenaYenileme = I("arenaYenileme"), ArenaGalibiyet = I("arenaG"), ArenaMaglubiyet = I("arenaM"),
			EnIyiAsama = I("enIyiAsama"), ElmasCekimSayaci = I("cekimSayaci"),
			SonGun = d.ContainsKey("sonGun") ? d["sonGun"].AsString() : "", GirisSerisi = I("girisSerisi"),
			GirisOdulAlindi = d.ContainsKey("girisAlindi") && d["girisAlindi"].AsBool(),
			GunlukSavas = I("gSavas"), GunlukCekim = I("gCekim"), GunlukYukseltme = I("gYukseltme"), SonKayitSurumu = I("surum"),
			Enerji = d.ContainsKey("enerji") ? I("enerji") : EnerjiMax, EnerjiZaman = d.ContainsKey("enerjiZaman") ? (long)d["enerjiZaman"].AsDouble() : 0,
			EnerjiAlim = I("enerjiAlim"), Sezon = d.ContainsKey("sezon") ? I("sezon") : -1, GunlukHikaye = I("gHikaye"), ArenaGunlukSavas = I("gArena"),
			MuzikAcik = !d.ContainsKey("muzik") || d["muzik"].AsBool(), EfektAcik = !d.ContainsKey("efekt") || d["efekt"].AsBool(),
		};
		if (d.ContainsKey("gorevler")) foreach (var g in d["gorevler"].AsGodotArray()) p.AlinanGorevler.Add(g.AsInt32());
		if (d.ContainsKey("urunler")) foreach (var u in d["urunler"].AsGodotArray()) p.AlinanUrunler.Add(u.AsString());
		if (d.ContainsKey("oz")) p.RuhOzu = d["oz"].AsInt32();
		if (d.ContainsKey("jetonlar")) foreach (var j in d["jetonlar"].AsGodotArray()) p.IslenenJetonlar.Add(j.AsString());
		if (d.ContainsKey("aylikBitis")) p.AylikKartBitis = d["aylikBitis"].AsString();
		if (d.ContainsKey("aylikSon")) p.AylikKartSonAlim = d["aylikSon"].AsString();
		if (d.ContainsKey("harcama")) p.ToplamHarcamaKurus = (long)d["harcama"].AsDouble();
		if (d.ContainsKey("reklam")) { var ra = d["reklam"].AsGodotArray(); for (int i = 0; i < Math.Min(4, ra.Count); i++) p.ReklamGunluk[i] = ra[i].AsInt32(); }
		p.SavasSayaci = I("savasSayaci");
		p.KuleKat = I("kule");
		if (d.ContainsKey("skKimlik")) p.SezonKartiKimlik = d["skKimlik"].AsString();
		p.SezonXp = I("skXp");
		if (d.ContainsKey("skPremium")) p.SezonPremium = d["skPremium"].AsString();
		if (d.ContainsKey("skAlinan")) foreach (var x in d["skAlinan"].AsGodotArray()) p.SezonAlinan.Add(x.AsInt32());
		if (d.ContainsKey("skAlinanP")) foreach (var x in d["skAlinanP"].AsGodotArray()) p.SezonAlinanP.Add(x.AsInt32());
		p.EtkinlikHafta = d.ContainsKey("etHafta") ? I("etHafta") : -1;
		p.EtkinlikPuan = I("etPuan");
		if (d.ContainsKey("etAlinan")) foreach (var x in d["etAlinan"].AsGodotArray()) p.EtkinlikAlinan.Add(x.AsInt32());
		if (d.ContainsKey("kartlar"))
			foreach (var kv in d["kartlar"].AsGodotArray())
			{
				var k = kv.AsGodotDictionary();
				p.Kartlar.Add(new SahipKart { Id = k["id"].AsString(), Seviye = k["sv"].AsInt32(), Yildiz = k["y"].AsInt32(), Kopya = k["k"].AsInt32() });
			}
		p.Takim = new List<string>();
		if (d.ContainsKey("takim")) foreach (var t in d["takim"].AsGodotArray()) { var s = t.AsString(); p.Takim.Add(s == "" ? null : s); }
		return p;
	}

	public static void YeniOyun()
	{
		P = new Profil { Altin = 3000, Elmas = 2700, Parsomen = 3, Enerji = EnerjiMax, EnerjiZaman = Simdi(), Sezon = Rakipler.Hafta() };
		foreach (var id in new[] { "akinci", "bahadir", "kam", "siren", "berserker", "medusa" }) KartEkle(id);
		P.Takim = new List<string> { "bahadir", "akinci", "medusa", "siren", "kam" };
		GunKontrol();
		Kaydet();
	}

	// ------------------------------------------------------------ günlük sıfırlama
	/// <summary>Bugünün tarihi (yyyy-MM-dd). Simülasyon bunu değiştirerek günleri ilerletir.</summary>
	public static Func<string> BugunFn = () => Time.GetDateStringFromSystem();
	public static string Bugun() => BugunFn();

	public static void GunKontrol()
	{
		string bugun = Bugun();
		if (P.SonGun == bugun) return;
		bool ardisik = P.SonGun != "" && DunMu(P.SonGun, bugun);
		if (P.SonGun != "")
			P.GirisSerisi = ardisik && P.GirisOdulAlindi ? (P.GirisSerisi + 1) % 7 : (ardisik ? P.GirisSerisi : 0);
		P.SonGun = bugun;
		P.GirisOdulAlindi = false;
		P.ArenaHak = Math.Max(P.ArenaHak, ArenaGunlukHak);
		P.ArenaYenileme = 0;
		P.GunlukSavas = P.GunlukCekim = P.GunlukYukseltme = P.GunlukHikaye = P.EnerjiAlim = P.ArenaGunlukSavas = 0;
		P.AlinanGorevler.Clear();
		P.ReklamGunluk = new int[4];
		Kaydet();
	}

	static bool DunMu(string once, string sonra)
	{
		var ci = System.Globalization.CultureInfo.InvariantCulture;
		return DateTime.TryParseExact(once, "yyyy-MM-dd", ci, System.Globalization.DateTimeStyles.None, out var a)
			&& DateTime.TryParseExact(sonra, "yyyy-MM-dd", ci, System.Globalization.DateTimeStyles.None, out var b)
			&& (b - a).TotalDays is > 0.5 and < 1.5;
	}

	public static (string ad, int altin, int elmas, int parsomen) GirisOdulu => GirisOdulleri[P.GirisSerisi % 7];

	public static bool GirisOdulAl()
	{
		if (P.GirisOdulAlindi) return false;
		var o = GirisOdulu;
		P.Altin += o.altin; P.Elmas += o.elmas; P.Parsomen += o.parsomen;
		P.GirisOdulAlindi = true;
		Kaydet();
		return true;
	}

	public static bool GorevTamam(int i) => Gorevler[i].ilerleme(P) >= Gorevler[i].hedef;

	public static bool GorevOdulAl(int i)
	{
		if (!GorevTamam(i) || P.AlinanGorevler.Contains(i)) return false;
		P.AlinanGorevler.Add(i);
		P.Elmas += Gorevler[i].elmas;
		SezonKarti.XpEkle(SezonKarti.XpGorev);
		Kaydet();
		return true;
	}

	// ------------------------------------------------------------ enerji
	public static long Simdi() => (long)Time.GetUnixTimeFromSystem();

	public static void EnerjiGuncelle()
	{
		long simdi = Simdi();
		if (P.EnerjiZaman <= 0) P.EnerjiZaman = simdi;
		if (P.Enerji >= EnerjiMax) { P.EnerjiZaman = simdi; return; }
		long gecen = Math.Max(0, simdi - P.EnerjiZaman);
		int kazanc = (int)(gecen / EnerjiSaniye);
		if (kazanc <= 0) return;
		P.Enerji = Math.Min(EnerjiMax, P.Enerji + kazanc);
		P.EnerjiZaman = P.Enerji >= EnerjiMax ? simdi : P.EnerjiZaman + kazanc * EnerjiSaniye;
	}

	/// <summary>Bir sonraki enerjiye kalan saniye (doluysa 0).</summary>
	public static int SonrakiEnerji() => P.Enerji >= EnerjiMax ? 0 : (int)Math.Max(0, EnerjiSaniye - (Simdi() - P.EnerjiZaman));

	public static bool EnerjiHarca(int n)
	{
		EnerjiGuncelle();
		if (P.Enerji < n) return false;
		bool doluydu = P.Enerji >= EnerjiMax;
		P.Enerji -= n;
		if (doluydu) P.EnerjiZaman = Simdi();
		Kaydet();
		return true;
	}

	public static bool EnerjiSatinAl()
	{
		if (P.EnerjiAlim >= EnerjiAlimGunluk || P.Elmas < EnerjiAlimFiyat) return false;
		P.Elmas -= EnerjiAlimFiyat; P.EnerjiAlim++;
		P.Enerji += EnerjiAlimMiktar;   // en fazla sınırı aşabilir
		Kaydet();
		return true;
	}

	// ------------------------------------------------------------ haftalık sezon
	/// <summary>Hafta değiştiyse eski sezonun sıralama ödülünü verir ve kupayı kısmen sıfırlar.</summary>
	public static (int sira, int elmas, int eskiKupa)? SezonKontrol()
	{
		int h = Rakipler.Hafta();
		if (P.Sezon < 0) { P.Sezon = h; Kaydet(); return null; }
		if (P.Sezon == h) return null;
		int sira = Rakipler.OyuncuSirasi(P.Sezon, 1f, P.Kupa);
		int elmas = Rakipler.SezonOdulu(sira);
		int eski = P.Kupa;
		P.Elmas += elmas;
		if (P.Kupa > 400) P.Kupa = 400 + (P.Kupa - 400) * 3 / 4;
		P.Sezon = h;
		Kaydet();
		return (sira, elmas, eski);
	}

	// ------------------------------------------------------------ kartlar
	public static SahipKart Sahip(string id) => P.Kartlar.FirstOrDefault(k => k.Id == id);

	/// <summary>Kartı ekler; zaten varsa kopya sayısını artırır. Dönüş: yeni mi?</summary>
	public static bool KartEkle(string id)
	{
		var s = Sahip(id);
		if (s != null) { s.Kopya++; P.RuhOzu += OzDegeri(Veri.Harita[id].Nadirlik); return false; }
		P.Kartlar.Add(new SahipKart { Id = id });
		return true;
	}

	public const int MaxYildiz = 7;   // 6★ ve 7★ oyun sonu hedefi
	public static int MaxSeviye(int yildiz) => 15 + 10 * yildiz;     // 1★:25 … 5★:65 … 7★:85
	public static int SeviyeMaliyet(int seviye) => (int)(Math.Round(40 * Math.Pow(seviye, 1.6) / 10.0) * 10);

	// ------------------------------------------------------------ ruh özü (fazla kopyalardan)
	/// <summary>Her kopya ayrıca nadirliğine göre ruh özü verir; öz ile istenen kartın kopyası alınır.</summary>
	public static int OzDegeri(string nadirlik) => nadirlik switch { "nadir" => 2, "epik" => 6, "efsanevi" => 20, _ => 1 };
	public static int OzKopyaFiyat(string nadirlik) => nadirlik switch { "nadir" => 25, "epik" => 70, "efsanevi" => 180, _ => 10 };

	public static bool OzleKopyaAl(SahipKart k)
	{
		int f = OzKopyaFiyat(Veri.Harita[k.Id].Nadirlik);
		if (P.RuhOzu < f) return false;
		P.RuhOzu -= f; k.Kopya++;
		Kaydet();
		return true;
	}
	public static int YildizKopya(int yildiz) => yildiz switch { 1 => 1, 2 => 2, 3 => 4, 4 => 6, 5 => 8, 6 => 10, _ => 999 };

	public static bool SeviyeAtlat(SahipKart k)
	{
		if (k.Seviye >= MaxSeviye(k.Yildiz)) return false;
		int m = SeviyeMaliyet(k.Seviye);
		if (P.Altin < m) return false;
		P.Altin -= m; k.Seviye++;
		P.GunlukYukseltme++;
		Kaydet();
		return true;
	}

	public static int YildizAltin(int yildiz) => 3000 * yildiz * yildiz;

	public static bool YildizAtlat(SahipKart k)
	{
		if (k.Yildiz >= MaxYildiz) return false;
		int m = YildizKopya(k.Yildiz);
		if (k.Kopya < m || P.Altin < YildizAltin(k.Yildiz)) return false;
		P.Altin -= YildizAltin(k.Yildiz);
		k.Kopya -= m; k.Yildiz++;
		P.GunlukYukseltme++;
		Kaydet();
		return true;
	}

	public static List<TakimUyesi> TakimUyeleri()
	{
		return P.Takim.Select(id => { var s = id == null ? null : Sahip(id); return s == null ? null : new TakimUyesi(s.Id, s.Seviye, s.Yildiz); }).ToList();
	}

	public static int TakimGucu() => TakimGucu(TakimUyeleri());
	public static int TakimGucu(IEnumerable<TakimUyesi> t) =>
		t.Where(u => u != null && Veri.Harita.ContainsKey(u.Id)).Sum(u => Veri.Guc(Veri.Harita[u.Id], u.Seviye, u.Yildiz));

	public static bool TakimHazir => P.Takim.Count(x => x != null) > 0;

	/// <summary>En güçlü 5 kartı (en az bir tank ve bir destekle) dizer.</summary>
	public static void OtomatikDiz()
	{
		var p = P;
		var sirali = p.Kartlar.OrderByDescending(k => Veri.Guc(Veri.Harita[k.Id], k.Seviye, k.Yildiz)).ToList();
		var secilen = new List<SahipKart>();
		// en az bir tank ve bir destek almaya çalış
		foreach (var sinif in new[] { "tank", "destek" })
		{
			var k = sirali.FirstOrDefault(x => Veri.Harita[x.Id].Sinif == sinif && !secilen.Contains(x));
			if (k != null) secilen.Add(k);
		}
		foreach (var k in sirali) { if (secilen.Count >= 5) break; if (!secilen.Contains(k)) secilen.Add(k); }
		int Oncelik(SahipKart k) => Veri.Harita[k.Id].Sinif switch { "tank" => 0, "savasci" => 1, "destek" => 3, _ => 2 };
		var dizi = secilen.OrderBy(Oncelik).ToList();
		for (int i = 0; i < 5; i++) p.Takim[i] = i < dizi.Count ? dizi[i].Id : null;
		Kaydet();
	}

	// ------------------------------------------------------------ çağırma
	static readonly Random _r = new();

	/// <summary>elmasla=true: S %50, N %34, Ep %13, Ef %3, 40. çekimde efsanevi garanti. Parşömen: S %70, N %26, Ep %4.</summary>
	public static KartTanim CekimYap(bool elmasla, Random r = null)
	{
		r ??= _r;
		string nadirlik;
		double x = r.NextDouble();
		if (elmasla)
		{
			P.ElmasCekimSayaci++;
			if (P.ElmasCekimSayaci >= EfsaneviGaranti) nadirlik = "efsanevi";
			else nadirlik = x < 0.03 ? "efsanevi" : x < 0.16 ? "epik" : x < 0.50 ? "nadir" : "siradan";
			if (nadirlik == "efsanevi") P.ElmasCekimSayaci = 0;
		}
		else nadirlik = x < 0.04 ? "epik" : x < 0.30 ? "nadir" : "siradan";
		var havuz = Veri.Kartlar.Where(k => k.Nadirlik == nadirlik).ToList();
		var kart = Etkinlik.HavuzdanSec(havuz, r);
		KartEkle(kart.Id);
		P.GunlukCekim++;
		return kart;
	}

	/// <summary>10'lu elmas çağrısı: en az bir epik veya efsanevi garanti.</summary>
	public static List<KartTanim> OnluCekim(Random r = null)
	{
		var l = new List<KartTanim>();
		for (int i = 0; i < 9; i++) l.Add(CekimYap(true, r));
		if (l.Any(k => k.NadirlikSira >= 2)) l.Add(CekimYap(true, r));
		else
		{
			r ??= _r;
			P.ElmasCekimSayaci++;
			bool ef = P.ElmasCekimSayaci >= EfsaneviGaranti || r.NextDouble() < 0.12;
			var havuz = Veri.Kartlar.Where(k => k.Nadirlik == (ef ? "efsanevi" : "epik")).ToList();
			var kart = Etkinlik.HavuzdanSec(havuz, r);
			if (ef) P.ElmasCekimSayaci = 0;
			KartEkle(kart.Id);
			P.GunlukCekim++;
			l.Add(kart);
		}
		return l;
	}

	public static List<KartTanim> Cagir(string tur, int adet)
	{
		var l = new List<KartTanim>();
		if (tur == "parsomen")
		{
			if (P.Parsomen < adet) return l;
			P.Parsomen -= adet;
			for (int i = 0; i < adet; i++) l.Add(CekimYap(false));
		}
		else
		{
			int fiyat = adet >= 10 ? ElmasOnluFiyat : ElmasCekimFiyat * adet;
			if (P.Elmas < fiyat) return l;
			P.Elmas -= fiyat;
			if (adet >= 10) l.AddRange(OnluCekim());
			else for (int i = 0; i < adet; i++) l.Add(CekimYap(true));
		}
		Kaydet();
		return l;
	}
}
