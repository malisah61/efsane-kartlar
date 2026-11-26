using System;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>
/// Kervansaray (AFK Arena'daki boşta kazanç benzeri): oyuncu yokken kervan altın ve ruh özü toplar,
/// takımdaki kahramanlar da yolda ustalık puanı kazanır. En fazla 12 saat birikir.
/// Kazanç hikâye ilerlemesine (ve biraz kuleye) bağlıdır; ekonomi bilerek yavaş: tam ilerlemiş oyuncuda
/// günde ~3.000 altın (hikâye tekrarlarıyla kazanılanın %5-10'u).
/// Elmaslı "Hızlı Sefer": 2 saatlik ganimet anında, günde 2 kez.
/// </summary>
public static class Kervan
{
	public const int MaxSaat = 12;
	public const int HizliFiyat = 60, HizliGunluk = 2, HizliSaat = 2;
	public const int EnAzSaniye = 60;

	public static int AltinSaat(Profil p) => 15 + 3 * p.EnIyiAsama / 2 + p.KuleKat / 4;
	public static float OzSaat(Profil p) => 0.1f + p.EnIyiAsama / 400f;

	/// <summary>Biriken süre (saniye, en fazla 12 saat).</summary>
	public static long Gecen()
	{
		var p = Kayit.P;
		long simdi = Kayit.Simdi();
		if (p.KervanZaman <= 0 || p.KervanZaman > simdi) { p.KervanZaman = simdi; return 0; }
		return Math.Min(MaxSaat * 3600L, simdi - p.KervanZaman);
	}

	public static bool Dolu => Gecen() >= MaxSaat * 3600L;

	public static (int altin, int oz, int xp) Ganimet(Profil p, double saat) =>
		((int)(AltinSaat(p) * saat), (int)(OzSaat(p) * saat), (int)saat);

	public static (int altin, int oz, int xp) Biriken() => Ganimet(Kayit.P, Gecen() / 3600.0);

	/// <summary>Ganimeti verir (kaydetmez). Dönüş: ustalık atlama satırları.</summary>
	public static System.Collections.Generic.List<string> Ver((int altin, int oz, int xp) g)
	{
		var p = Kayit.P;
		p.Altin += g.altin; p.RuhOzu += g.oz;
		return Ustalik.XpEkle(p.Takim.Where(x => x != null), g.xp);
	}

	/// <summary>Biriken ganimeti toplar. Dönüş null: henüz birikmedi.</summary>
	public static ((int altin, int oz, int xp) g, System.Collections.Generic.List<string> satir)? Topla()
	{
		long gecen = Gecen();
		if (gecen < EnAzSaniye) return null;
		var g = Ganimet(Kayit.P, gecen / 3600.0);
		Kayit.P.KervanZaman = Kayit.Simdi();
		var satir = Ver(g);
		Kayit.Kaydet();
		return (g, satir);
	}

	public static int HizliKalan => Math.Max(0, HizliGunluk - Kayit.P.KervanHizli);

	public static ((int altin, int oz, int xp) g, System.Collections.Generic.List<string> satir)? HizliSefer()
	{
		var p = Kayit.P;
		if (HizliKalan <= 0 || p.Elmas < HizliFiyat) return null;
		p.Elmas -= HizliFiyat; p.KervanHizli++;
		var g = Ganimet(p, HizliSaat);
		var satir = Ver(g);
		Kayit.Kaydet();
		return (g, satir);
	}
}
