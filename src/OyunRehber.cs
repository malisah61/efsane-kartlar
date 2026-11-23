using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Başlangıç Yolu paneli, etkinlik mağazası ve mitoloji muhafızı savaşı (Oyun partial).</summary>
public partial class Oyun
{
	// ================================================================ BAŞLANGIÇ YOLU
	/// <summary>Ana sayfanın tepesindeki rehber paneli (rehber bittiyse null).</summary>
	Control RehberPaneli()
	{
		if (Rehber.Bitti) return null;
		var a = Rehber.Simdiki;
		bool hazir = Rehber.OdulHazir;
		int no = Kayit.P.RehberAdim + 1;
		var renk = hazir ? new Color("2f6a3f") : new Color("3a2f6a");
		var panel = Ui.Panel(renk, 12);
		panel.AddThemeStyleboxOverride("panel", Ui.Kutu(renk, 16, Ui.Altin, 3, 12));
		var v = Ui.Dikey(6);
		panel.AddChild(v);
		var ust = Ui.Yatay(8);
		v.AddChild(ust);
		ust.AddChild(Ui.Genis(Ui.Etiket($"Başlangıç Yolu  {no}/{Rehber.Adimlar.Length}", 20, Ui.Altin)));
		ust.AddChild(Ui.Etiket("Ödül: " + a.OdulYazi(), 18, Ui.Elmas, HorizontalAlignment.Right));
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		var yazi = Ui.Dikey(2);
		alt.AddChild(Ui.Genis(yazi));
		yazi.AddChild(Ui.Etiket(a.Ad, 24, Colors.White));
		yazi.AddChild(Ui.Etiket(hazir ? "Tamamlandı! Ödülünü al." : a.Ipucu, 17, Ui.Soluk));
		Button b = null;
		b = Ui.Dugme(hazir ? "Al" : "Git", () =>
		{
			if (hazir)
			{
				var s = Rehber.OdulAl();
				if (s == null) return;
				Ses.Efekt("para");
				Mesaj(Rehber.Bitti ? $"Başlangıç Yolu tamamlandı! +{s}" : $"+{s}");
				Yenile();
				return;
			}
			RehberGit(a.Git);
		}, hazir ? Ui.Yesil.Darkened(0.15f) : new Color("c0582f"), 22, 64);
		b.CustomMinimumSize = new Vector2(120, 64);
		b.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
		alt.AddChild(b);
		// dikkat çekmek için düğme yavaşça parlar
		var tw = b.CreateTween().SetLoops();
		tw.TweenProperty(b, "modulate", new Color(1.35f, 1.3f, 1.1f), 0.6f).SetTrans(Tween.TransitionType.Sine);
		tw.TweenProperty(b, "modulate", Colors.White, 0.6f).SetTrans(Tween.TransitionType.Sine);
		return panel;
	}

	void RehberGit(string hedef)
	{
		switch (hedef)
		{
			case "hikaye": HikayeSavasi(Math.Min(Kayit.P.EnIyiAsama + 1, Rakipler.AsamaSayisi)); break;
			case "kartlar": Goster(Ekran.Kartlar); Mesaj("Bir karta dokun"); break;
			case "takim": Goster(Ekran.Takim); break;
			case "cagir": Goster(Ekran.Cagir); break;
			case "arena": Goster(Ekran.Arena); break;
			case "kule": KulePenceresi(); break;
			default: EtkinlikPenceresi(); break;
		}
	}

	// ================================================================ ETKİNLİK MAĞAZASI
	void EtkinlikMagazasi()
	{
		Etkinlik.Kontrol();
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Etkinlik Mağazası", 36, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Jetonun: {Etkinlik.Jeton:N0}", 26, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket("Kazandığın her etkinlik puanı 1 jeton da verir. Jetonlar ve alım sınırları hafta bitince sıfırlanır.", 17, Ui.Soluk, HorizontalAlignment.Center));
		var urunler = Etkinlik.Urunler;
		for (int i = 0; i < urunler.Length; i++)
		{
			int ui = i;
			var u = urunler[i];
			int alinan = Etkinlik.Alinan(i);
			bool tukendi = alinan >= u.Limit;
			bool olur = Etkinlik.Alinabilir(u, i);
			var sp = Ui.Panel(u.Tur is "epik" or "kahraman" ? new Color("3a2f55") : Ui.PanelRenk, 10, 12);
			v.AddChild(sp);
			var h = Ui.Yatay(10);
			sp.AddChild(h);
			var sol = Ui.Dikey(2);
			h.AddChild(Ui.Genis(sol));
			sol.AddChild(Ui.Etiket(u.Ad, 21, tukendi ? Ui.Soluk : Colors.White));
			sol.AddChild(Ui.Etiket($"Haftalık sınır: {alinan}/{u.Limit}", 16, Ui.Soluk));
			var b = Ui.Dugme(tukendi ? "Tükendi" : $"{u.Fiyat:N0} jeton", () =>
			{
				var s = Etkinlik.UrunAl(ui);
				if (s == null) return;
				Ses.Efekt(u.Tur is "epik" or "kahraman" ? "efsanevi" : "para");
				PencereKapat(); Yenile(); EtkinlikMagazasi(); Mesaj("+" + s);
			}, olur ? new Color("2f8a57") : new Color("3a4060"), 19, 52);
			b.CustomMinimumSize = new Vector2(170, 52);
			b.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
			b.Disabled = !olur;
			h.AddChild(b);
		}
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		alt.AddChild(Ui.Genis(Ui.Dugme("Etkinliğe Dön", () => { PencereKapat(); EtkinlikPenceresi(); }, new Color("5a3a9a"), 22)));
		alt.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22)));
		Pencere(v);
	}

	// ================================================================ MİTOLOJİ MUHAFIZI
	void MuhafizPenceresi()
	{
		Etkinlik.Kontrol();
		var m = Etkinlik.Mitoloji;
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket($"{Veri.MitolojiAd(m)} Muhafızları", 36, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Günde {Etkinlik.MuhafizHak} kez, enerjisiz. Muhafızlar senin takımının seviyesinde ve tam mitoloji uyumuyla savaşır.", 18, Ui.Soluk, HorizontalAlignment.Center));
		var dusman = Etkinlik.MuhafizTakimi();
		v.AddChild(MiniTakim(dusman, 104));
		int dg = Kayit.TakimGucu(dusman), bg = Kayit.TakimGucu();
		v.AddChild(Ui.Etiket($"Muhafız gücü {dg:N0}   •   Senin gücün {bg:N0}", 19, bg >= dg ? Ui.Yesil : Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Zafer: {Etkinlik.ZaferPuani(false) * 4} etkinlik puanı + jeton, 800 altın   •   Yenilgi: 10 puan", 19, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Kalan hak bugün: {Etkinlik.MuhafizKalan}/{Etkinlik.MuhafizHak}", 20, Colors.White, HorizontalAlignment.Center));
		var b = Ui.Dugme("Muhafızlara Saldır", MuhafizSavasi, new Color("c0582f"), 26, 72);
		b.Disabled = Etkinlik.MuhafizKalan <= 0;
		v.AddChild(b);
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		alt.AddChild(Ui.Genis(Ui.Dugme("Etkinliğe Dön", () => { PencereKapat(); EtkinlikPenceresi(); }, new Color("5a3a9a"), 22)));
		alt.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22)));
		Pencere(v);
	}

	void MuhafizSavasi()
	{
		if (!Kayit.TakimHazir) { PencereKapat(); Mesaj("Önce takımına kart ekle"); Goster(Ekran.Takim); return; }
		if (Etkinlik.MuhafizKalan <= 0) return;
		PencereKapat();
		Kayit.P.MuhafizSavas++;
		Kayit.Kaydet();
		var m = Etkinlik.Mitoloji;
		SavasBaslat(Etkinlik.MuhafizTakimi(), $"{Veri.MitolojiAd(m)} Muhafızları", m, Etkinlik.MuhafizSonuc, true, m);
	}
}
