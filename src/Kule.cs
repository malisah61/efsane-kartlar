using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>
/// Efsaneler Kulesi: 100 katlı, enerjisiz meydan okuma. Her 10 kat bir mitolojinin katlarıdır
/// (düşmanlar çoğunlukla o mitolojiden, uyum bonusuyla). Ödül yalnızca katı ilk geçişte verilir;
/// her 10. kat boss'tur. İlerleme takımın gücüyle sınırlıdır, bu yüzden ekonomiyi hızlandırmaz.
/// </summary>
public static class Kule
{
	public const int KatSayisi = 100;
	static readonly string[] Mitler = { "turk", "yunan", "iskandinav", "misir" };

	public static string KatMitoloji(int kat) => Mitler[((Math.Max(1, kat) - 1) / 10) % 4];
	public static string KatBolum(int kat) => $"{Veri.MitolojiAd(KatMitoloji(kat))} Katları";
	public static bool BossKati(int kat) => kat % 10 == 0;
	public static int KatYildiz(int kat) => 1 + Math.Min(4, (kat - 1) / 20);
	public static int KatSeviye(int kat) => Math.Min(Kayit.MaxSeviye(KatYildiz(kat)), Math.Max(2, (int)MathF.Round(1 + kat * 0.8f)));
	public static int SiradakiKat => Math.Min(Kayit.P.KuleKat + 1, KatSayisi);
	public static bool Bitti => Kayit.P.KuleKat >= KatSayisi;

	public static List<TakimUyesi> KatTakimi(int kat)
	{
		var r = new Rng((ulong)(kat * 104729 + 3));
		string mit = KatMitoloji(kat);
		int sev = KatSeviye(kat), yil = KatYildiz(kat);
		float efs = kat >= 50 ? 0.06f + (kat - 50) / 600f : 0f;
		float epik = Math.Clamp((kat - 12) / 55f, 0, 0.5f);
		float nadir = Math.Clamp(0.15f + kat / 80f, 0, 0.45f);
		string[] sinifSira = { "tank", "savasci", "buyucu", "destek", "buyucu" };
		var takim = new List<TakimUyesi>();
		var kullanilan = new HashSet<string>();
		for (int i = 0; i < 5; i++)
		{
			string nad;
			if (BossKati(kat) && i == 2) nad = kat >= 30 ? "efsanevi" : "epik";
			else { float x = r.Kesir(); nad = x < efs ? "efsanevi" : x < efs + epik ? "epik" : x < efs + epik + nadir ? "nadir" : "siradan"; }
			string sinif = sinifSira[i];
			if (r.Kesir() < 0.3f) sinif = i < 2 ? (sinif == "tank" ? "savasci" : "tank") : (r.Kesir() < 0.5f ? "buyucu" : "savasci");
			bool kendiMit = r.Kesir() < 0.7f;
			IEnumerable<KartTanim> Havuz(bool mitSart, bool sinifSart) => Veri.Kartlar.Where(x => x.Nadirlik == nad && !kullanilan.Contains(x.Id)
				&& (!mitSart || x.Mitoloji == mit) && (!sinifSart || x.Sinif == sinif));
			var havuz = Havuz(kendiMit, true).ToList();
			if (havuz.Count == 0) havuz = Havuz(kendiMit, false).ToList();
			if (havuz.Count == 0) havuz = Havuz(false, true).ToList();
			if (havuz.Count == 0) havuz = Havuz(false, false).ToList();
			if (havuz.Count == 0) havuz = Veri.Kartlar.Where(x => !kullanilan.Contains(x.Id)).ToList();
			var k = havuz[r.Aralik(havuz.Count)];
			kullanilan.Add(k.Id);
			takim.Add(new TakimUyesi(k.Id, sev, yil));
		}
		return takim;
	}

	public static (int altin, int elmas, int parsomen, int oz) KatOdulu(int kat) =>
		(150 + kat * 30, kat % 5 == 0 ? 25 : 0, BossKati(kat) ? 1 : 0, BossKati(kat) ? 30 : 0);

	public static string OdulYazi((int altin, int elmas, int parsomen, int oz) o)
	{
		var l = new List<string> { $"{o.altin:N0} altın" };
		if (o.elmas > 0) l.Add($"{o.elmas} elmas");
		if (o.parsomen > 0) l.Add($"{o.parsomen} parşömen");
		if (o.oz > 0) l.Add($"{o.oz} ruh özü");
		return string.Join(", ", l);
	}

	/// <summary>Savaş sonucu: kazanıldıysa ve kat ilk kez geçildiyse ödülü verir. Dönüş: sonuç satırları.</summary>
	public static List<string> Sonuc(int kat, bool kazandi)
	{
		var p = Kayit.P;
		var l = new List<string>();
		if (kazandi && kat == p.KuleKat + 1)
		{
			var o = KatOdulu(kat);
			p.Altin += o.altin; p.Elmas += o.elmas; p.Parsomen += o.parsomen; p.RuhOzu += o.oz;
			p.KuleKat = kat;
			SezonKarti.XpEkle(SezonKarti.XpKule);
			l.Add($"{kat}. kat geçildi!");
			l.Add("+" + OdulYazi(o).Replace(", ", ", +"));
			l.Add($"+{SezonKarti.XpKule} sezon puanı");
		}
		else if (kazandi) l.Add("Kat zaten geçilmişti.");
		else l.Add("Kule enerji harcamaz: takımını güçlendirip tekrar dene.");
		Kayit.Kaydet();
		return l;
	}
}
