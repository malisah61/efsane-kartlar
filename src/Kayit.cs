using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
}

public static class Kayit
{
	const string Yol = "user://kayit.json";
	public static Profil P = new();
	public static bool KayitKapali;   // önizleme/test modunda diske yazma

	public const int ElmasCekimFiyat = 300;
	public const int ElmasOnluFiyat = 2700;
	public const int EfsaneviGaranti = 40;
	public const int ArenaGunlukHak = 5;
	public const int ArenaEkHakFiyat = 50;

	public static readonly (string ad, int altin, int elmas, int parsomen)[] GirisOdulleri =
	{
		("1. Gün", 1000, 0, 0), ("2. Gün", 0, 60, 0), ("3. Gün", 0, 0, 2), ("4. Gün", 2500, 0, 0),
		("5. Gün", 0, 120, 0), ("6. Gün", 0, 0, 3), ("7. Gün", 0, 300, 0),
	};

	public static readonly (string ad, Func<Profil, int> ilerleme, int hedef, int elmas)[] Gorevler =
	{
		("3 savaş yap", p => p.GunlukSavas, 3, 40),
		("1 kart çağır", p => p.GunlukCekim, 1, 30),
		("1 kart yükselt", p => p.GunlukYukseltme, 1, 30),
	};

	// ------------------------------------------------------------ kaydet / yükle
	public static void Yukle()
	{
		if (FileAccess.FileExists(Yol))
		{
			try
			{
				using var f = FileAccess.Open(Yol, FileAccess.ModeFlags.Read);
				P = JsonSerializer.Deserialize<Profil>(f.GetAsText()) ?? new Profil();
				P.Kartlar.RemoveAll(k => !Veri.Harita.ContainsKey(k.Id));
				while (P.Takim.Count < 5) P.Takim.Add(null);
				for (int i = 0; i < 5; i++) if (P.Takim[i] != null && Sahip(P.Takim[i]) == null) P.Takim[i] = null;
				if (P.Kartlar.Count > 0) { GunKontrol(); return; }
			}
			catch (Exception e) { GD.PrintErr("Kayıt okunamadı: " + e.Message); }
		}
		YeniOyun();
	}

	public static void Kaydet()
	{
		if (KayitKapali) return;
		using var f = FileAccess.Open(Yol, FileAccess.ModeFlags.Write);
		f.StoreString(JsonSerializer.Serialize(P));
	}

	public static void YeniOyun()
	{
		P = new Profil { Altin = 3000, Elmas = 600, Parsomen = 5 };
		foreach (var id in new[] { "akinci", "bahadir", "kam", "siren", "berserker", "medusa" }) KartEkle(id);
		P.Takim = new List<string> { "bahadir", "akinci", "medusa", "siren", "kam" };
		GunKontrol();
		Kaydet();
	}

	// ------------------------------------------------------------ günlük sıfırlama
	public static string Bugun() => Time.GetDateStringFromSystem();

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
		P.GunlukSavas = P.GunlukCekim = P.GunlukYukseltme = 0;
		P.AlinanGorevler.Clear();
		Kaydet();
	}

	static bool DunMu(string once, string sonra)
	{
		return DateTime.TryParse(once, out var a) && DateTime.TryParse(sonra, out var b) && (b - a).TotalDays is > 0.5 and < 1.5;
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
		Kaydet();
		return true;
	}

	// ------------------------------------------------------------ kartlar
	public static SahipKart Sahip(string id) => P.Kartlar.FirstOrDefault(k => k.Id == id);

	/// <summary>Kartı ekler; zaten varsa kopya sayısını artırır. Dönüş: yeni mi?</summary>
	public static bool KartEkle(string id)
	{
		var s = Sahip(id);
		if (s != null) { s.Kopya++; return false; }
		P.Kartlar.Add(new SahipKart { Id = id });
		return true;
	}

	public static int MaxSeviye(int yildiz) => 10 + 10 * yildiz;     // 1★:20 … 5★:60
	public static int SeviyeMaliyet(int seviye) => (int)(Math.Round(60 * Math.Pow(seviye, 1.55) / 10.0) * 10);
	public static int YildizKopya(int yildiz) => yildiz switch { 1 => 1, 2 => 2, 3 => 4, 4 => 8, _ => 999 };

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

	public static bool YildizAtlat(SahipKart k)
	{
		if (k.Yildiz >= 5) return false;
		int m = YildizKopya(k.Yildiz);
		if (k.Kopya < m) return false;
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
		var kart = havuz[r.Next(havuz.Count)];
		KartEkle(kart.Id);
		P.GunlukCekim++;
		return kart;
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
			for (int i = 0; i < adet; i++) l.Add(CekimYap(true));
		}
		Kaydet();
		return l;
	}
}
