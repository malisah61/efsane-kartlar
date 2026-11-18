using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>Sezon kartının bir seviyesindeki ödül.</summary>
public struct SezonOdul
{
	public int Altin, Elmas, Parsomen, Oz;
	public bool Epik;
	public override string ToString()
	{
		var l = new List<string>();
		if (Epik) l.Add("Epik kahraman");
		if (Elmas > 0) l.Add($"{Elmas} elmas");
		if (Parsomen > 0) l.Add($"{Parsomen} parşömen");
		if (Oz > 0) l.Add($"{Oz} ruh özü");
		if (Altin > 0) l.Add($"{Altin:N0} altın");
		return string.Join(" + ", l);
	}
}

/// <summary>
/// Efsane Yolu: aylık sezon kartı. Savaşlar ve görevler sezon puanı (XP) verir; her 250 puan bir seviye.
/// Ücretsiz yol herkese açık, Altın Yol "sezon_karti" ürünüyle o ay için açılır. Her ayın 1'inde sıfırlanır.
/// </summary>
public static class SezonKarti
{
	public const int SeviyeSayisi = 30, SeviyeXp = 250;
	public const int XpHikaye = 5, XpArena = 10, XpKule = 20, XpGorev = 20;
	public const string UrunId = "sezon_karti";

	static readonly string[] Aylar = { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" };

	public static string Kimlik() { var b = Kayit.Bugun(); return b.Length >= 7 ? b.Substring(0, 7) : b; }
	public static string AyAdi()
	{
		var k = Kimlik();
		return k.Length == 7 && int.TryParse(k.Substring(5, 2), out int a) && a is >= 1 and <= 12 ? $"{Aylar[a - 1]} {k.Substring(0, 4)}" : k;
	}

	public static int KalanGun()
	{
		if (!DateTime.TryParseExact(Kayit.Bugun(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var g)) return 0;
		var sonraki = new DateTime(g.Year, g.Month, 1).AddMonths(1);
		return (int)(sonraki - g.Date).TotalDays;
	}

	/// <summary>Ay değiştiyse ilerlemeyi sıfırlar.</summary>
	public static void Kontrol()
	{
		var p = Kayit.P;
		string k = Kimlik();
		if (p.SezonKartiKimlik == k) return;
		p.SezonKartiKimlik = k;
		p.SezonXp = 0;
		p.SezonAlinan.Clear();
		p.SezonAlinanP.Clear();
	}

	public static bool Premium { get { Kontrol(); return Kayit.P.SezonPremium == Kimlik(); } }
	public static int Xp { get { Kontrol(); return Kayit.P.SezonXp; } }
	public static int Seviye => Math.Min(SeviyeSayisi, Xp / SeviyeXp);
	public static int SeviyeIciXp => Seviye >= SeviyeSayisi ? SeviyeXp : Xp % SeviyeXp;

	public static void XpEkle(int n)
	{
		Kontrol();
		Kayit.P.SezonXp = Math.Min(SeviyeSayisi * SeviyeXp, Kayit.P.SezonXp + n);
	}

	public static SezonOdul Ucretsiz(int sv) =>
		sv % 10 == 0 ? new SezonOdul { Elmas = 100, Parsomen = 1 }
		: sv % 5 == 0 ? new SezonOdul { Parsomen = 1 }
		: sv % 2 == 0 ? new SezonOdul { Altin = 1000 + sv * 100 }
		: new SezonOdul { Elmas = 15 };

	public static SezonOdul AltinYol(int sv) =>
		sv == SeviyeSayisi ? new SezonOdul { Epik = true, Elmas = 300 }
		: sv % 10 == 0 ? new SezonOdul { Elmas = 300, Parsomen = 3 }
		: sv % 5 == 0 ? new SezonOdul { Parsomen = 2, Oz = 50 }
		: sv % 2 == 0 ? new SezonOdul { Elmas = 40 }
		: new SezonOdul { Altin = 3000 + sv * 200 };

	public static SezonOdul Toplam(bool altinYol)
	{
		var t = new SezonOdul();
		for (int i = 1; i <= SeviyeSayisi; i++)
		{
			var o = altinYol ? AltinYol(i) : Ucretsiz(i);
			t.Altin += o.Altin; t.Elmas += o.Elmas; t.Parsomen += o.Parsomen; t.Oz += o.Oz; t.Epik |= o.Epik;
		}
		return t;
	}

	public static bool Alindi(int sv, bool altinYol) { Kontrol(); return (altinYol ? Kayit.P.SezonAlinanP : Kayit.P.SezonAlinan).Contains(sv); }
	public static bool Alinabilir(int sv, bool altinYol) => sv <= Seviye && !Alindi(sv, altinYol) && (!altinYol || Premium);

	public static int AlinabilirSayi()
	{
		int n = 0;
		for (int i = 1; i <= Seviye; i++) { if (Alinabilir(i, false)) n++; if (Alinabilir(i, true)) n++; }
		return n;
	}

	/// <summary>Ödülü verir; dönüş açıklama (alınamazsa null).</summary>
	public static string Al(int sv, bool altinYol, Random r = null)
	{
		if (!Alinabilir(sv, altinYol)) return null;
		var p = Kayit.P;
		var o = altinYol ? AltinYol(sv) : Ucretsiz(sv);
		p.Altin += o.Altin; p.Elmas += o.Elmas; p.Parsomen += o.Parsomen; p.RuhOzu += o.Oz;
		string ek = "";
		if (o.Epik) ek = " (" + Magaza.EpikVer(r) + ")";
		(altinYol ? p.SezonAlinanP : p.SezonAlinan).Add(sv);
		return o + ek;
	}

	/// <summary>Alınabilir tüm ödülleri toplar.</summary>
	public static List<string> HepsiniAl(Random r = null)
	{
		var l = new List<string>();
		for (int i = 1; i <= Seviye; i++)
			foreach (bool ay in new[] { false, true })
			{
				var s = Al(i, ay, r);
				if (s != null) l.Add(s);
			}
		Kayit.Kaydet();
		return l;
	}
}
