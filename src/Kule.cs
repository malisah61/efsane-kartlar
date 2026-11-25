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

	/// <summary>10. mühürlü katın boss'u: mühür mitolojisinin kendi karanlık yüzü (efsanelerdeki kötüler / isyancılar).
	/// Oyuncu yalnızca o mitolojiyle savaştığı için ayna maçı olur; iki taraf da aynı uyum bonusunu alır.</summary>
	public static readonly Dictionary<string, (string ad, string aciklama, string[] kartlar)> MuhurBosslari = new()
	{
		["turk"] = ("Erlik'in Ordusu", "Yeraltının hanı Erlik, Tepegöz ve Al Karısı ile mührü bekliyor.", new[] { "tepegoz", "sahmeran", "erlik", "al_karisi", "su_iyesi" }),
		["yunan"] = ("Hades'in Mahkemesi", "Ölüler diyarının efendisi; Kerberos kapıda, Medusa ve Siren yanında.", new[] { "hades", "kerberos", "medusa", "siren", "minotor" }),
		["iskandinav"] = ("Ragnarök", "Loki'nin çocukları zincirlerini kırdı: Fenrir, Hel ve buz devleri.", new[] { "ymir", "fenrir", "loki", "hel", "volva" }),
		["misir"] = ("Kaosun Yılanı", "Set ve Sekhmet'in öfkesi, Sobek'in dişleri: Maat'ı yık ya da koru.", new[] { "sobek", "mumya", "set", "akrep", "sekhmet" }),
		["japon"] = ("Yüz Yokai Geçidi", "Gece yürüyen yokai alayı: Oni, Kappa, Tengu, Yuki-onna ve Raijin.", new[] { "oni", "kappa", "tengu", "yuki_onna", "raijin" }),
		["cin"] = ("Göklerin İsyanı", "Maymun Kral göğe başkaldırdı; Ejderha Kral ve Nezha da saflarında.", new[] { "long", "nezha", "sun_wukong", "change", "taocu" }),
	};

	public static (string ad, string aciklama, string[] kartlar)? MuhurBoss =>
		MuhurBosslari.TryGetValue(MuhurMitoloji, out var b) && b.kartlar.All(id => Veri.Harita.ContainsKey(id)) ? b : null;

	public static List<TakimUyesi> MuhurTakimi(int kat)
	{
		if (kat == MuhurKatSayisi && MuhurBoss is { } boss)
		{
			// boss takımı çoğunlukla epik/efsanevi: dengelemek için onlar 3 seviye düşük
			int z = MuhurZorluk(kat), sev = KatSeviye(z), yil = KatYildiz(z);
			return boss.kartlar.Select(id =>
			{
				var n = Veri.Harita[id].Nadirlik;
				return new TakimUyesi(id, n is "epik" or "efsanevi" ? Math.Max(1, sev - 3) : sev + 1, yil);
			}).ToList();
		}
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
