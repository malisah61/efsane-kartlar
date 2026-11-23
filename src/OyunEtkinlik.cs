using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Haftalık mitoloji etkinliği ekranı ve yükseltme kutlaması (Oyun partial).</summary>
public partial class Oyun
{
	static Color MitolojiRenk(string m) => Veri.MitolojiRenk(m);

	Control EtkinlikDugmesi()
	{
		Etkinlik.Kontrol();
		int n = Etkinlik.AlinabilirSayi();
		var k = Etkinlik.Kalan;
		string ust = $"Etkinlik: {Etkinlik.Ad}";
		string alt = n > 0 ? $"{n} ödül hazır!" : Etkinlik.MuhafizKalan > 0 ? $"{Etkinlik.Puan} puan  •  Muhafızlar {Etkinlik.MuhafizKalan}/{Etkinlik.MuhafizHak}  •  {k.Days}g kaldı" : $"{Etkinlik.Puan} puan  •  {k.Days}g {k.Hours}s kaldı";
		return Ui.Dugme($"{ust}\n{alt}", EtkinlikPenceresi, MitolojiRenk(Etkinlik.Mitoloji).Darkened(0.15f), 21, 84);
	}

	void EtkinlikPenceresi()
	{
		Etkinlik.Kontrol();
		var p = Kayit.P;
		string m = Etkinlik.Mitoloji;
		var renk = MitolojiRenk(m);
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket(Etkinlik.Ad, 38, Ui.Altin, HorizontalAlignment.Center));
		var k = Etkinlik.Kalan;
		v.AddChild(Ui.Etiket($"Bitimine {k.Days} gün {k.Hours} saat   •   her hafta başka bir mitoloji", 18, Ui.Soluk, HorizontalAlignment.Center));

		var bp = Ui.Panel(renk.Darkened(0.55f), 12);
		v.AddChild(bp);
		var bv = Ui.Dikey(4);
		bp.AddChild(bv);
		bv.AddChild(Ui.Etiket("Bu hafta", 22, Colors.White, HorizontalAlignment.Center));
		bv.AddChild(Ui.Etiket($"• {Veri.MitolojiAd(m)} kartları hikâyede ve kulede can/saldırı +%{Etkinlik.GucBonus * 100:0}", 18, Ui.Yesil));
		bv.AddChild(Ui.Etiket($"• Çağırmada {Veri.MitolojiAd(m)} kartları 2 kat sık çıkar", 18, Ui.Elmas));
		bv.AddChild(Ui.Etiket("• Zafer: hikâye/kule 10, arena 15 puan (+5/etkinlik kartı); puan kadar jeton", 18, Colors.White));
		int sayi = Etkinlik.TakimdakiSayi(Kayit.TakimUyeleri());
		bv.AddChild(Ui.Etiket(sayi > 0 ? $"Takımında {sayi} {Veri.MitolojiAd(m)} kartı var: zafer başına {Etkinlik.ZaferPuani(false)} puan" : $"Takımına {Veri.MitolojiAd(m)} kartı koyarsan daha çok puan kazanırsın!", 18, sayi > 0 ? Ui.Altin : Ui.Soluk));

		// etkinlik kahramanı
		var kah = Etkinlik.Kahraman;
		if (kah != null)
		{
			var kp = Ui.Panel(Ui.PanelAcik, 10);
			v.AddChild(kp);
			var kh = Ui.Yatay(12);
			kp.AddChild(kh);
			var kk = new KartKutu(kah, 96) { Kilitli = Kayit.Sahip(kah.Id) == null };
			kk.Tiklandi = () => KartDetayi(kah, null);
			kh.AddChild(kk);
			var kv = Ui.Genis(Ui.Dikey(4));
			kh.AddChild(kv);
			kv.AddChild(Ui.Etiket("Etkinlik Kahramanı", 18, Ui.Soluk));
			kv.AddChild(Ui.Etiket(kah.Ad, 28, Veri.NadirlikRenk(kah.Nadirlik)));
			kv.AddChild(Ui.Etiket($"{Veri.NadirlikAd(kah.Nadirlik)} {Veri.SinifAd(kah.Sinif)} • {Veri.ElementAd(kah.Element)}", 17, Ui.Soluk));
			kv.AddChild(Ui.Etiket($"Ulti: {kah.UltiAd}", 18, Ui.Altin));
			kv.AddChild(Ui.Etiket($"{Etkinlik.Oduller[^1].Puan} puanda ücretsiz!", 18, Ui.Yesil));
		}

		int puan = Etkinlik.Puan;
		int son = Etkinlik.Oduller[^1].Puan;
		v.AddChild(Ui.Etiket($"Etkinlik puanı: {puan:N0} / {son:N0}", 24, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Cubuk(Math.Min(puan, son), son, renk.Lightened(0.2f)));

		var liste = Ui.Dikey(6);
		for (int i = 0; i < Etkinlik.Oduller.Length; i++)
		{
			int oi = i;
			var o = Etkinlik.Oduller[i];
			bool alindi = Etkinlik.Alindi(i);
			bool hazir = Etkinlik.Alinabilir(i);
			var sp = Ui.Panel(o.KahramanKopya ? new Color("3a2f55") : Ui.PanelRenk, 5, 10);
			liste.AddChild(sp);
			var h = Ui.Yatay(8);
			sp.AddChild(h);
			var pl = Ui.Etiket($"{o.Puan:N0}", 20, puan >= o.Puan ? Ui.Altin : Ui.Soluk, HorizontalAlignment.Center);
			pl.CustomMinimumSize = new Vector2(80, 0);
			pl.VerticalAlignment = VerticalAlignment.Center;
			pl.SizeFlagsVertical = Control.SizeFlags.Fill;
			h.AddChild(pl);
			var ol = Ui.Genis(Ui.Etiket(o.Yazi(), 19, alindi ? Ui.Soluk : Colors.White));
			ol.VerticalAlignment = VerticalAlignment.Center;
			ol.SizeFlagsVertical = Control.SizeFlags.Fill;
			h.AddChild(ol);
			var b = Ui.Dugme(alindi ? "Alındı" : hazir ? "Al" : "—", () =>
			{
				var s = Etkinlik.Al(oi);
				if (s == null) return;
				Ses.Efekt(o.KahramanKopya ? "efsanevi" : "para");
				PencereKapat(); Yenile(); EtkinlikPenceresi(); Mesaj("+" + s);
			}, hazir ? Ui.Yesil.Darkened(0.2f) : new Color("3a4060"), 18, 40);
			b.CustomMinimumSize = new Vector2(110, 40);
			b.Disabled = !hazir;
			h.AddChild(b);
		}
		v.AddChild(liste);

		// muhafız savaşı + etkinlik mağazası
		var ek = Ui.Yatay(10);
		v.AddChild(ek);
		ek.AddChild(Ui.Genis(Ui.Dugme($"Muhafızlar ({Etkinlik.MuhafizKalan}/{Etkinlik.MuhafizHak})", MuhafizPenceresi, new Color("a0402f"), 21, 58)));
		ek.AddChild(Ui.Genis(Ui.Dugme($"Mağaza ({Etkinlik.Jeton:N0} jeton)", EtkinlikMagazasi, new Color("8a4fc0"), 21, 58)));

		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		int n = Etkinlik.AlinabilirSayi();
		var hb = Ui.Genis(Ui.Dugme(n > 0 ? $"Hepsini Al ({n})" : "Savaşa Git", () =>
		{
			if (n == 0) { PencereKapat(); Goster(Ekran.Ana); return; }
			var l = Etkinlik.HepsiniAl();
			Ses.Efekt("para"); PencereKapat(); Yenile(); EtkinlikPenceresi();
			Mesaj($"{l.Count} ödül alındı!");
		}, new Color("2f8a57"), 22));
		alt.AddChild(hb);
		alt.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22)));
		Pencere(v);
	}

	/// <summary>Seviye/yıldız atlatınca kartın üstünde parlama, parçacık ve süzülen yazı.</summary>
	void Kutla(Control kart, string yazi, Color renk)
	{
		Ses.Efekt("kart", 2, 1.25f, 0);
		// pencere yerleşimi bitsin diye kısa bekle (kartın boyutu ancak o zaman belli olur)
		GetTree().CreateTimer(0.06).Timeout += () =>
		{
			if (!IsInstanceValid(kart)) return;
			var boyut = kart.Size.X > 1 ? kart.Size : kart.CustomMinimumSize;
			var merkez = boyut / 2;
			kart.PivotOffset = merkez;
			var tw = kart.CreateTween();
			tw.TweenProperty(kart, "scale", new Vector2(1.08f, 1.08f), 0.12f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			tw.Parallel().TweenProperty(kart, "modulate", new Color(1.6f, 1.5f, 1.2f), 0.12f);
			tw.TweenProperty(kart, "scale", Vector2.One, 0.25f);
			tw.Parallel().TweenProperty(kart, "modulate", Colors.White, 0.35f);
			Efekt.Patlama(kart, merkez, renk, 40, 280, 7, 0.8f, 200);
			Efekt.Yukselen(kart, new Vector2(merkez.X, boyut.Y - 10), renk, 24, merkez.X * 0.8f);
			Efekt.Halka(kart, merkez, renk, 20, boyut.X * 0.75f, 0.5f, 6);
			var l = Ui.Etiket(yazi, 34, renk, HorizontalAlignment.Center);
			l.AddThemeColorOverride("font_outline_color", Colors.Black);
			l.AddThemeConstantOverride("outline_size", 8);
			l.MouseFilter = MouseFilterEnum.Ignore;
			l.Size = new Vector2(boyut.X + 120, 50);
			l.Position = new Vector2(-60, merkez.Y - 25);
			l.ZIndex = 30;
			kart.AddChild(l);
			var lt = l.CreateTween();
			lt.TweenProperty(l, "position:y", merkez.Y - 110, 1.1f).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
			lt.Parallel().TweenProperty(l, "modulate:a", 0f, 0.5f).SetDelay(0.9f);
			lt.TweenCallback(Callable.From(l.QueueFree));
		};
	}
}
