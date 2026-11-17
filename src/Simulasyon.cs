using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Godot;

namespace EfsaneKartlar;

/// <summary>
/// Bot oyuncu ilerleme simülasyonu: --ilerleme=dosya
/// Farklı oyuncu tiplerini 365 gün oynatır (gerçek savaş simülasyonu, gerçek ekonomi kuralları)
/// ve kart toplama, hikâye, güç, lig ilerlemesini gün gün raporlar.
/// </summary>
public static class Simulasyon
{
	class Tip
	{
		public string Ad;
		public int HikayeSavas;        // günde kaç hikâye savaşı (enerji: 6 dk'da 1 → günde en çok ~40)
		public bool Reklam = true;
		public bool Baslangic, Aylik;
		public int HaftalikHazine;     // haftada kaç elmas_6500
		public int EnerjiAlim;         // günde kaç kez 50 elmasa +30 enerji (5 savaş)
		public int EkArena;            // günde kaç ek arena hakkı (50 elmas)
	}

	class Kayitlar
	{
		public Dictionary<string, int> Ilk = new();
		public List<string> Satirlar = new();
		public void Olay(string ad, int gun) { if (!Ilk.ContainsKey(ad)) Ilk[ad] = gun; }
	}

	static readonly int[] Kontrol = { 1, 3, 7, 14, 30, 60, 90, 120, 180, 270, 365 };

	public static void Calistir(string yol)
	{
		var tipler = new[]
		{
			new Tip { Ad = "Gündelik oyuncu (ücretsiz, günde ~20 hikâye savaşı)", HikayeSavas = 20 },
			new Tip { Ad = "Aktif oyuncu (ücretsiz, enerjinin tamamı ~40 savaş)", HikayeSavas = 40 },
			new Tip { Ad = "Az harcayan (başlangıç paketi + aylık kart ≈ ₺90/ay)", HikayeSavas = 40, Baslangic = true, Aylik = true },
			new Tip { Ad = "Çok harcayan (+ haftada 1 elmas hazinesi ≈ ₺2.000/ay)", HikayeSavas = 40, Baslangic = true, Aylik = true, HaftalikHazine = 1, EnerjiAlim = 3, EkArena = 5 },
		};
		var sb = new StringBuilder();
		sb.AppendLine("# İlerleme simülasyonu (bot oyuncular, 365 gün)");
		sb.AppendLine();
		sb.AppendLine("Her gün: giriş ödülü, hikâye savaşları (sıradaki aşama; yenilirse son geçilen aşamada altın toplar), 5 arena savaşı,");
		sb.AppendLine("günlük görevler, reklam ödülü, elmas 2.700'e ulaşınca 10'lu çağrı, tüm parşömenlerle çağrı, altınla takım yükseltme, kopyalarla yıldız.");
		sb.AppendLine();
		var ozet = new List<(string tip, Kayitlar k, long kurus)>();
		foreach (var t in tipler)
		{
			var (k, kurus) = Oyna(t, 365, 11);
			ozet.Add((t.Ad, k, kurus));
			sb.AppendLine($"## {t.Ad}");
			sb.AppendLine();
			sb.AppendLine("| Gün | Kart (S/N/E/Ef) | Takım gücü | Ort. seviye | Hikâye | Kupa / Lig | Elmas | Altın |");
			sb.AppendLine("|---|---|---|---|---|---|---|---|");
			foreach (var s in k.Satirlar) sb.AppendLine(s);
			sb.AppendLine();
			sb.AppendLine("Önemli anlar (gün):");
			foreach (var (ad, gun) in k.Ilk.OrderBy(x => x.Value)) sb.AppendLine($"- {ad}: {gun}. gün");
			string[] beklenen = { "İlk epik kart", "İlk efsanevi kart", "Tüm 48 kart", "4 efsanevinin hepsi", "Hikâye bitti (60. aşama)", "Gümüş lig", "Altın lig", "Elmas lig", "Efsane lig", "İlk 5 yıldızlı kart" };
			foreach (var b in beklenen) if (!k.Ilk.ContainsKey(b)) sb.AppendLine($"- {b}: 365 günde ulaşılamadı");
			sb.AppendLine($"- Toplam harcama: ₺{kurus / 100m:N2}");
			sb.AppendLine();
		}
		var metin = sb.ToString();
		GD.Print(metin);
		try { File.WriteAllText(yol, metin); } catch (Exception e) { GD.PrintErr(e.Message); }
	}

	static (Kayitlar, long) Oyna(Tip t, int gunSayisi, int tohum)
	{
		var r = new Random(tohum);
		var k = new Kayitlar();
		Kayit.KayitKapali = true;
		int bas = Magaza.GunNo("2027-01-04");   // pazartesi
		Kayit.BugunFn = () => Magaza.GunYazi(bas);
		Kayit.YeniOyun();
		var p = Kayit.P;
		ulong savasTohum = 1;

		for (int gun = 1; gun <= gunSayisi; gun++)
		{
			int g = gun;
			Kayit.BugunFn = () => Magaza.GunYazi(bas + g - 1);
			Kayit.GunKontrol();
			Kayit.SezonKontrol();
			Kayit.GirisOdulAl();

			// satın almalar
			if (t.Baslangic && gun == 1) Magaza.Ver("baslangic_paketi", r);
			if (t.Aylik && Magaza.AylikKartKalan() == 0) Magaza.Ver("aylik_kart", r);
			if (t.HaftalikHazine > 0 && gun % 7 == 1) for (int i = 0; i < t.HaftalikHazine; i++) Magaza.Ver("elmas_6500", r);
			Magaza.AylikKartAl();
			if (t.Reklam) p.Elmas += 40;

			Kayit.OtomatikDiz();

			// hikâye
			int savas = t.HikayeSavas;
			for (int i = 0; i < t.EnerjiAlim; i++)
				if (p.Elmas >= Kayit.EnerjiAlimFiyat) { p.Elmas -= Kayit.EnerjiAlimFiyat; savas += 5; }
			bool takildi = false;
			for (int i = 0; i < savas; i++)
			{
				int hedef = takildi || p.EnIyiAsama >= Rakipler.AsamaSayisi ? Math.Max(1, p.EnIyiAsama) : p.EnIyiAsama + 1;
				var s = new Savas(Kayit.TakimUyeleri(), Rakipler.AsamaTakimi(hedef), savasTohum++);
				s.Oyna();
				p.GunlukSavas++; p.GunlukHikaye++;
				if (s.Kazanan == 0)
				{
					bool ilk = hedef > p.EnIyiAsama;
					var o = Rakipler.AsamaOdulu(hedef, ilk);
					p.Altin += o.altin; p.Elmas += o.elmas; p.Parsomen += o.parsomen;
					if (ilk) { p.EnIyiAsama = hedef; takildi = false; }
				}
				else if (hedef > p.EnIyiAsama) takildi = true;
				// takılınca araya yükseltme yap, sonra tekrar dene
				if (takildi && i % 8 == 7) { Yukselt(); takildi = false; }
			}
			if (p.EnIyiAsama >= Rakipler.AsamaSayisi) k.Olay("Hikâye bitti (60. aşama)", gun);

			// arena
			for (int i = 0; i < t.EkArena; i++) if (p.Elmas >= Kayit.ArenaEkHakFiyat) { p.Elmas -= Kayit.ArenaEkHakFiyat; p.ArenaHak++; }
			while (p.ArenaHak > 0)
			{
				p.ArenaHak--;
				var rakipler = Rakipler.ArenaRakipleri();
				var rk = rakipler[1];
				var s = new Savas(Kayit.TakimUyeleri(), rk.Takim, savasTohum++);
				s.Oyna();
				bool kazandi = s.Kazanan == 0;
				var o = Rakipler.ArenaOdulu(rk, kazandi);
				p.Kupa = Math.Max(0, p.Kupa + o.kupa); p.Altin += o.altin; p.Elmas += o.elmas;
				if (kazandi) p.ArenaGalibiyet++; else p.ArenaMaglubiyet++;
				p.ArenaGunlukSavas++; p.GunlukSavas++;
			}
			string lig = Rakipler.Lig(p.Kupa);
			if (lig != "Bronz") k.Olay(lig + " lig", gun);

			// çağırma
			while (Kayit.TakasKalan > 0 && p.Altin >= Kayit.AltinParsomenFiyat + 20000) Kayit.AltinlaParsomen();
			while (p.Parsomen > 0) { p.Parsomen--; Kayit.CekimYap(false, r); }
			while (p.Elmas >= Kayit.ElmasOnluFiyat) { p.Elmas -= Kayit.ElmasOnluFiyat; Kayit.OnluCekim(r); }

			Yukselt();
			for (int i = 0; i < Kayit.Gorevler.Length; i++) Kayit.GorevOdulAl(i);

			// olaylar
			if (p.Kartlar.Any(x => Veri.Harita[x.Id].Nadirlik == "epik")) k.Olay("İlk epik kart", gun);
			if (p.Kartlar.Any(x => Veri.Harita[x.Id].Nadirlik == "efsanevi")) k.Olay("İlk efsanevi kart", gun);
			if (p.Kartlar.Count(x => Veri.Harita[x.Id].Nadirlik == "efsanevi") >= 4) k.Olay("4 efsanevinin hepsi", gun);
			if (p.Kartlar.Count >= Veri.Kartlar.Count) k.Olay("Tüm 48 kart", gun);
			if (p.Kartlar.Any(x => x.Yildiz >= 5)) k.Olay("İlk 5 yıldızlı kart", gun);
			if (p.Kartlar.Any(x => x.Yildiz >= 3)) k.Olay("İlk 3 yıldızlı kart", gun);

			if (Kontrol.Contains(gun))
			{
				int Say(string n) => p.Kartlar.Count(x => Veri.Harita[x.Id].Nadirlik == n);
				var tk = Kayit.TakimUyeleri().Where(u => u != null).ToList();
				double ort = tk.Count == 0 ? 0 : tk.Average(u => u.Seviye);
				k.Satirlar.Add($"| {gun} | {p.Kartlar.Count} ({Say("siradan")}/{Say("nadir")}/{Say("epik")}/{Say("efsanevi")}) | {Kayit.TakimGucu():N0} | {ort:0.0} | {p.EnIyiAsama}/60 | {p.Kupa} {Rakipler.Lig(p.Kupa)} | {p.Elmas:N0} | {p.Altin:N0} |");
			}
		}
		Kayit.BugunFn = () => Time.GetDateStringFromSystem();
		return (k, Kayit.P.ToplamHarcamaKurus);
	}

	/// <summary>Takımı dizer, yıldız atlatır, altınla en düşük seviyeli takım kartını yükseltir.</summary>
	static void Yukselt()
	{
		var p = Kayit.P;
		// takım kartlarına ruh özüyle eksik kopyayı al, sonra yıldız atlat
		foreach (var id in p.Takim.Where(x => x != null).ToList())
		{
			var s = Kayit.Sahip(id);
			while (s.Yildiz < 5 && s.Kopya < Kayit.YildizKopya(s.Yildiz) && Kayit.OzleKopyaAl(s)) { }
		}
		// önce takım; takım dışı kartlara yalnızca altın bolsa yıldız ver
		foreach (var s in p.Kartlar.Where(x => p.Takim.Contains(x.Id)).ToList())
			while (Kayit.YildizAtlat(s)) { }
		foreach (var s in p.Kartlar.Where(x => !p.Takim.Contains(x.Id)).OrderByDescending(x => Veri.Harita[x.Id].Nadirlik == "efsanevi" ? 3 : Veri.Harita[x.Id].Nadirlik == "epik" ? 2 : 0).ToList())
			while (p.Altin > 150000 && Kayit.YildizAtlat(s)) { }
		Kayit.OtomatikDiz();
		for (int adim = 0; adim < 500; adim++)
		{
			var aday = p.Takim.Where(id => id != null).Select(Kayit.Sahip)
				.Where(s => s.Seviye < Kayit.MaxSeviye(s.Yildiz)).OrderBy(s => s.Seviye).FirstOrDefault();
			if (aday == null || !Kayit.SeviyeAtlat(aday)) break;
		}
		Kayit.OtomatikDiz();
	}
}
