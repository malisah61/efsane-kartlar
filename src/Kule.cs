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
	static readonly string[] Mitler = { "turk", "yunan", "iskandinav", "misir", "japon", "cin" };

	public static string KatMitoloji(int kat) => Mitler[((Math.Max(1, kat) - 1) / 10) % Mitler.Length];
	public static string KatBolum(int kat) => $"{Veri.MitolojiAd(KatMitoloji(kat))} Katları";
	public static bool BossKati(int kat) => kat % 10 == 0;
	public static int KatYildiz(int kat) => 1 + Math.Min(4, (kat - 1) / 20);
	public static int KatSeviye(int kat) => Math.Min(Kayit.MaxSeviye(KatYildiz(kat)), Math.Max(2, (int)MathF.Round(1 + kat * 0.8f)));
	public static int SiradakiKat => Math.Min(Kayit.P.KuleKat + 1, KatSayisi);
	public static bool Bitti => Kayit.P.KuleKat >= KatSayisi;

	public static List<TakimUyesi> KatTakimi(int kat) => TakimUret(kat, KatMitoloji(kat), (ulong)(kat * 104729 + 3), BossKati(kat));

	static List<TakimUyesi> TakimUret(int kat, string mit, ulong tohum, bool boss)
	{
		var r = new Rng(tohum);
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
			if (boss && i == 2) nad = kat >= 30 ? "efsanevi" : "epik";
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

	// ============================================================ MÜHÜRLÜ KATLAR
	// Haftalık, mitoloji kısıtlı meydan okuma: 10 kat, yalnızca haftanın mühür mitolojisinden kartlar savaşabilir.
	// Zorluk hafta başında ana kuledeki ilerlemeye göre sabitlenir; ödüller her hafta bir kez alınır.
	public const int MuhurKatSayisi = 10;
	public static int MuhurHafta => Rakipler.Hafta();
	/// <summary>Mühür mitolojisi, etkinlik mitolojisinden 3 hafta ileride döner (aynı hafta ikisi çakışmaz).</summary>
	public static string MuhurMitoloji => Mitler[(((MuhurHafta + 3) % Mitler.Length) + Mitler.Length) % Mitler.Length];

	public static void MuhurKontrol()
	{
		var p = Kayit.P;
		if (p.MuhurHafta == MuhurHafta) return;
		p.MuhurHafta = MuhurHafta;
		p.MuhurKat = 0;
		p.MuhurTaban = Math.Clamp((int)MathF.Round(p.KuleKat * 0.7f), 2, 85);
	}

	public static int MuhurGecilen { get { MuhurKontrol(); return Kayit.P.MuhurKat; } }
	public static bool MuhurBitti => MuhurGecilen >= MuhurKatSayisi;
	public static int MuhurSiradaki => Math.Min(MuhurGecilen + 1, MuhurKatSayisi);
	/// <summary>Mühürlü katın denk geldiği kule katı (zorluk).</summary>
	public static int MuhurZorluk(int kat) { MuhurKontrol(); return Math.Min(KatSayisi, Kayit.P.MuhurTaban + (int)MathF.Round(kat * 1.5f)); }

	public static List<TakimUyesi> MuhurTakimi(int kat)
	{
		var r = new Rng((ulong)(MuhurHafta * 7919 + kat * 31 + 11));
		var digerleri = Mitler.Where(m => m != MuhurMitoloji).ToArray();
		string mit = digerleri[r.Aralik(digerleri.Length)];
		return TakimUret(MuhurZorluk(kat), mit, (ulong)(MuhurHafta * 15485863L + kat * 104729 + 5), kat == MuhurKatSayisi);
	}

	/// <summary>Oyuncunun takımı: mühür mitolojisinden olmayan kartlar savaşa giremez (yerleri boş kalır).</summary>
	public static List<TakimUyesi> MuhurUyeleri() =>
		Kayit.TakimUyeleri().Select(u => u != null && Veri.Harita.TryGetValue(u.Id, out var t) && t.Mitoloji == MuhurMitoloji ? u : null).ToList();

	public static int MuhurUygunSayi => MuhurUyeleri().Count(u => u != null);

	public static (int altin, int elmas, int parsomen, int oz) MuhurOdulu(int kat)
	{
		MuhurKontrol();
		int altin = 250 + kat * 50 + Kayit.P.MuhurTaban * 8;
		return kat switch
		{
			MuhurKatSayisi => (altin, 60, 1, 40),
			5 => (altin, 30, 0, 15),
			_ => (altin, 0, 0, 15),
		};
	}

	public static List<string> MuhurSonuc(int kat, bool kazandi)
	{
		MuhurKontrol();
		var p = Kayit.P;
		var l = new List<string>();
		if (kazandi && kat == p.MuhurKat + 1)
		{
			var o = MuhurOdulu(kat);
			p.Altin += o.altin; p.Elmas += o.elmas; p.Parsomen += o.parsomen; p.RuhOzu += o.oz;
			p.MuhurKat = kat;
			l.Add(kat == MuhurKatSayisi ? "Mühürlü Katlar bu hafta tamamlandı!" : $"Mühürlü {kat}. kat açıldı!");
			l.Add("+" + OdulYazi(o).Replace(", ", ", +"));
		}
		else if (kazandi) l.Add("Bu kat bu hafta zaten geçilmişti.");
		else l.Add($"Daha çok {Veri.MitolojiAd(MuhurMitoloji)} kartını yükselt ya da takımına ekle.");
		Kayit.Kaydet();
		return l;
	}
}
