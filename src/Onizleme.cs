using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>Bulutta (GitHub Actions) ekran görüntüsü alır: --onizleme=klasör</summary>
public static class Onizleme
{
	public static async Task Cek(Oyun o, string klasor)
	{
		DirAccess.MakeDirRecursiveAbsolute(klasor);
		var p = Kayit.P;
		// zengin bir profil: kartların çoğu, farklı seviyeler
		var r = new Random(7);
		foreach (var k in Veri.Kartlar.Where((k, i) => i % 5 != 3)) Kayit.KartEkle(k.Id);
		foreach (var s in p.Kartlar) { s.Yildiz = 1 + r.Next(3); s.Seviye = 5 + r.Next(Kayit.MaxSeviye(s.Yildiz) - 5); s.Kopya = r.Next(4); }
		p.Altin = 48250; p.Elmas = 3120; p.Parsomen = 7; p.Kupa = 742; p.EnIyiAsama = 11; p.ElmasCekimSayaci = 23;
		p.GunlukSavas = 3; p.GunlukCekim = 1;
		p.Takim = new List<string> { "tepegoz", "akhilleus", "zeus", "medusa", "umay" };
		await Bekle(o, 0.6f);

		string[] adlar = { "1_ana", "2_kartlar", "3_takim", "4_cagir", "5_arena", "6_sosyal" };
		for (int i = 0; i < 6; i++)
		{
			o.OnizlemeEkran(i);
			await Bekle(o, 0.6f);
			await Kare(o, $"{klasor}/{adlar[i]}.png");
		}
		o.OnizlemeEkran(1);
		o.OnizlemeEkran(2);
		o.OnizlemeTakimKart(p.Kartlar.First(k => !p.Takim.Contains(k.Id)).Id);
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/3b_takim_kart.png");
		o.OnizlemeKapat();
		o.OnizlemeEkran(1);
		o.OnizlemeDetay("zeus");
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/6_kart_detay.png");
		o.OnizlemeHikaye("susanoo");
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/6b_kart_hikaye.png");
		o.OnizlemeKapat();
		o.OnizlemeCagir();
		await Bekle(o, 0.9f);
		await Kare(o, $"{klasor}/7a_cagri_acilis.png");
		await Bekle(o, 4.5f);
		await Kare(o, $"{klasor}/7_cagri.png");
		o.OnizlemeKapat();
		o.OnizlemeGunluk();
		await Bekle(o, 0.4f);
		await Kare(o, $"{klasor}/8_gunluk.png");
		o.OnizlemeKapat();
		o.OnizlemeMagaza();
		await Bekle(o, 0.4f);
		await Kare(o, $"{klasor}/9_magaza.png");
		o.OnizlemeKapat();
		p.KuleKat = 9;
		o.OnizlemeKule();
		await Bekle(o, 0.4f);
		await Kare(o, $"{klasor}/12_kule.png");
		o.OnizlemeKapat();
		o.OnizlemeMuhur();
		await Bekle(o, 0.4f);
		await Kare(o, $"{klasor}/12b_muhur.png");
		o.OnizlemeKapat();
		SezonKarti.Kontrol();
		p.SezonXp = 7 * SezonKarti.SeviyeXp + 140;
		p.SezonAlinan.AddRange(new[] { 1, 2, 3, 4 });
		o.OnizlemeSezon();
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/13_sezon.png");
		o.OnizlemeKapat();
		Etkinlik.Kontrol();
		p.EtkinlikPuan = 520;
		p.EtkinlikAlinan.Add(0);
		o.OnizlemeEtkinlik();
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/14_etkinlik.png");
		o.OnizlemeKapat();
		p.EtkinlikJeton = 640;
		p.EtkinlikAlim.Add(0);
		o.OnizlemeEtkinlikMagaza();
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/14b_etkinlik_magaza.png");
		o.OnizlemeKapat();
		o.OnizlemeMuhafiz();
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/14c_muhafiz.png");
		o.OnizlemeKapat();
		p.Basarimlar.Add("toplam_15");
		o.OnizlemeKoleksiyon();
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/17_koleksiyon.png");
		o.OnizlemeKapat();
		Kayit.DizilisKaydet(0);
		p.Dizilisler[1] = new List<string> { "athena", "akhilleus", "zeus", "medusa", "umay" };
		o.OnizlemeDizilis();
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/18_dizilis.png");
		o.OnizlemeKapat();
		o.OnizlemeKutlama(p.Kartlar[0].Id);
		await Bekle(o, 0.35f);
		await Kare(o, $"{klasor}/15_seviye_atlama.png");
		o.OnizlemeKapat();
		o.OnizlemeDetay("koroglu");
		await Bekle(o, 0.4f);
		await Kare(o, $"{klasor}/16_yeni_kart.png");
		o.OnizlemeKapat();
		o.OnizlemeEkran(0);
		p.Takim = new List<string> { "athena", "akhilleus", "zeus", "medusa", "umay" };
		foreach (var id in p.Takim) if (Kayit.Sahip(id) == null) Kayit.KartEkle(id);
		var se = o.OnizlemeSavas(8);
		await Bekle(o, 1.6f);
		await Kare(o, $"{klasor}/10_savas.png");
		await Bekle(o, 1.1f);
		await Kare(o, $"{klasor}/10b_savas.png");
		await Bekle(o, 1.3f);
		await Kare(o, $"{klasor}/10c_savas.png");
		if (se != null) { se.OnizlemeUlti(); await Bekle(o, 0.3f); await Kare(o, $"{klasor}/10d_ulti.png"); }
		if (se != null) se.Hiz = 3;
		for (int i = 0; i < 80 && IsInstanceValid(se); i++) await Bekle(o, 0.5f);
		await Bekle(o, 0.5f);
		await Kare(o, $"{klasor}/11_sonuc.png");
		GD.Print($"ONIZLEME tamam fps={Engine.GetFramesPerSecond()}");
		o.GetTree().Quit();
	}

	static bool IsInstanceValid(GodotObject g) => g != null && GodotObject.IsInstanceValid(g);

	static async Task Bekle(Node n, float s) => await n.ToSignal(n.GetTree().CreateTimer(s), SceneTreeTimer.SignalName.Timeout);

	static async Task Kare(Node n, string yol)
	{
		await n.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
		n.GetViewport().GetTexture().GetImage().SavePng(yol);
		GD.Print("kare: " + yol);
	}
}

/// <summary>Denge testi: binlerce otomatik savaş, sonuçları dosyaya yazar. --denge=dosya</summary>
public static class DengeTesti
{
	public static void Calistir(string yol)
	{
		var sb = new StringBuilder();
		var r = new Random(1234);
		var kartlar = Veri.Kartlar;
		var kazanc = kartlar.ToDictionary(k => k.Id, _ => 0);
		var oyun = kartlar.ToDictionary(k => k.Id, _ => 0);
		int turToplam = 0, zamanAsimi = 0, saldiranKazandi = 0;
		const int N = 6000;
		for (int i = 0; i < N; i++)
		{
			var a = RastgeleTakim(r, 20); var b = RastgeleTakim(r, 20);
			var s = new Savas(a, b, (ulong)(i + 1));
			s.Oyna();
			turToplam += Math.Min(s.Tur - 1, Savas.MaxTur);
			if (s.Tur > Savas.MaxTur) zamanAsimi++;
			if (s.Kazanan == 0) saldiranKazandi++;
			foreach (var u in a) { oyun[u.Id]++; if (s.Kazanan == 0) kazanc[u.Id]++; }
			foreach (var u in b) { oyun[u.Id]++; if (s.Kazanan == 1) kazanc[u.Id]++; }
		}
		sb.AppendLine($"Rastgele takımlar, seviye 20, {N} savaş");
		sb.AppendLine($"Ortalama tur: {turToplam / (float)N:0.0}   zaman aşımı: %{100f * zamanAsimi / N:0.0}   saldıran kazanma: %{100f * saldiranKazandi / N:0.0}");
		sb.AppendLine();
		sb.AppendLine("Nadirlik kazanma oranı:");
		foreach (var n in Veri.NadirlikSirasiDizi)
		{
			var ks = kartlar.Where(k => k.Nadirlik == n).ToList();
			sb.AppendLine($"  {n,-9} %{100f * ks.Sum(k => kazanc[k.Id]) / Math.Max(1, ks.Sum(k => oyun[k.Id])):0.0}");
		}
		sb.AppendLine("Sınıf kazanma oranı:");
		foreach (var g in kartlar.GroupBy(k => k.Sinif))
			sb.AppendLine($"  {g.Key,-9} %{100f * g.Sum(k => kazanc[k.Id]) / Math.Max(1, g.Sum(k => oyun[k.Id])):0.0}");
		sb.AppendLine();
		sb.AppendLine("Kart kazanma oranı (yüksekten düşüğe):");
		foreach (var k in kartlar.OrderByDescending(k => kazanc[k.Id] / (float)Math.Max(1, oyun[k.Id])))
			sb.AppendLine($"  {k.Ad,-18} {k.Nadirlik,-9} {k.Sinif,-8} %{100f * kazanc[k.Id] / Math.Max(1, oyun[k.Id]):0.0}  ({oyun[k.Id]} savaş)");

		// hikâye ilerlemesi: başlangıç takımı aşama aşama seviye atlarsa
		sb.AppendLine();
		sb.AppendLine("Hikâye: başlangıç takımı (sıradan/nadir) seviye L iken hangi aşamaya kadar %50+ kazanır");
		string[] baslangic = { "bahadir", "akinci", "medusa", "siren", "kam" };
		foreach (int L in new[] { 1, 5, 10, 15, 20, 30 })
		{
			int son = 0;
			for (int asama = 1; asama <= Rakipler.AsamaSayisi; asama++)
			{
				int w = 0;
				for (int t = 0; t < 20; t++)
				{
					var s = new Savas(baslangic.Select(id => new TakimUyesi(id, L, L > 20 ? 2 : 1)).ToList(), Rakipler.AsamaTakimi(asama), (ulong)(asama * 100 + t));
					s.Oyna();
					if (s.Kazanan == 0) w++;
				}
				if (w >= 10) son = asama; else break;
			}
			sb.AppendLine($"  seviye {L,2}: aşama {son}  (aşama seviyesi ~{Rakipler.AsamaSeviye(Math.Max(1, son))})");
		}
		var metin = sb.ToString();
		GD.Print(metin);
		try { File.WriteAllText(yol, metin); } catch (Exception e) { GD.PrintErr(e.Message); }
	}

	static List<TakimUyesi> RastgeleTakim(Random r, int seviye)
	{
		var secilen = Veri.Kartlar.OrderBy(_ => r.Next()).Take(5)
			.OrderBy(k => k.Sinif switch { "tank" => 0, "savasci" => 1, "buyucu" => 2, _ => 3 }).ToList();
		return secilen.Select(k => new TakimUyesi(k.Id, seviye, 1)).ToList();
	}
}
