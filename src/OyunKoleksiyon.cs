using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Koleksiyon başarımları penceresi, kayıtlı takım dizilişleri ve rehber oku (Oyun partial).</summary>
public partial class Oyun
{
	// ================================================================ KOLEKSİYON BAŞARIMLARI
	void KoleksiyonPenceresi()
	{
		var p = Kayit.P;
		var liste = Koleksiyon.Liste;
		int alinan = liste.Count(Koleksiyon.Alindi);
		var v = Ui.Dikey(8);
		v.AddChild(Ui.Etiket("Koleksiyon Başarımları", 34, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"{alinan}/{liste.Count} başarım   •   {p.Kartlar.Count}/{Veri.Kartlar.Count} kart   •   {Koleksiyon.TamBagSayisi(p)}/{Veri.Baglar.Count} bağ", 19, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Cubuk(alinan, liste.Count, Ui.Altin, 14));

		var ic = Ui.Dikey(6);
		// alınabilenler en üstte, sonra devam edenler, en sonda alınanlar
		foreach (var g in liste.GroupBy(b => b.Grup))
		{
			ic.AddChild(Ui.Etiket(g.Key, 22, Colors.White));
			foreach (var b in g.OrderBy(b => Koleksiyon.Alinabilir(b) ? 0 : Koleksiyon.Alindi(b) ? 2 : 1))
				ic.AddChild(BasarimSatiri(b));
		}
		var kay = Ui.Kaydirma(ic);
		kay.CustomMinimumSize = new Vector2(0, 760);
		v.AddChild(kay);

		int n = Koleksiyon.AlinabilirSayi();
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		var hb = Ui.Genis(Ui.Dugme(n > 0 ? $"Hepsini Al ({n})" : "Alınacak ödül yok", () =>
		{
			var (adet, elmas) = Koleksiyon.HepsiniAl();
			if (adet == 0) return;
			Ses.Efekt("para"); PencereKapat(); Yenile(); KoleksiyonPenceresi();
			Mesaj($"{adet} başarım ödülü alındı" + (elmas > 0 ? $" (+{elmas} elmas)" : ""));
		}, new Color("2f8a57"), 22));
		hb.Disabled = n == 0;
		alt.AddChild(hb);
		alt.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22)));
		Pencere(v);
	}

	Control BasarimSatiri(Koleksiyon.Basarim b)
	{
		bool alindi = Koleksiyon.Alindi(b), olur = Koleksiyon.Alinabilir(b);
		int il = Math.Min(b.Ilerleme(Kayit.P), b.Hedef);
		var sp = Ui.Panel(olur ? new Color("2f4a3a") : Ui.PanelRenk, 10, 12);
		var h = Ui.Yatay(10);
		sp.AddChild(h);
		var sol = Ui.Dikey(3);
		h.AddChild(Ui.Genis(sol));
		sol.AddChild(Ui.Etiket(b.Ad, 20, alindi ? Ui.Soluk : olur ? Ui.Altin : Colors.White));
		sol.AddChild(Ui.Etiket($"{b.Aciklama}  ({il}/{b.Hedef})", 16, Ui.Soluk));
		if (!alindi) sol.AddChild(Cubuk(il, b.Hedef, olur ? Ui.Yesil : new Color("5a6ab8"), 8));
		sol.AddChild(Ui.Etiket("Ödül: " + b.OdulYazi(), 15, alindi ? Ui.Soluk : Ui.Elmas));
		if (olur)
		{
			var al = Ui.Dugme("Al", () =>
			{
				var s = Koleksiyon.Al(b);
				if (s == null) return;
				Ses.Efekt("para"); PencereKapat(); Yenile(); KoleksiyonPenceresi(); Mesaj($"{b.Ad}: +{s}");
			}, Ui.Yesil.Darkened(0.2f), 19, 48);
			al.CustomMinimumSize = new Vector2(90, 48);
			al.SizeFlagsVertical = Control.SizeFlags.ShrinkCenter;
			h.AddChild(al);
		}
		else
		{
			var d = Ui.Etiket(alindi ? "Alındı" : "", 16, Ui.Yesil, HorizontalAlignment.Center);
			d.CustomMinimumSize = new Vector2(90, 0);
			d.VerticalAlignment = VerticalAlignment.Center;
			d.SizeFlagsVertical = Control.SizeFlags.Fill;
			h.AddChild(d);
		}
		return sp;
	}

	// ================================================================ KAYITLI DİZİLİŞLER
	static readonly string[] DizilisOneri = { "ör. hikâye", "ör. arena", "ör. kule" };

	void DizilisPenceresi()
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Kayıtlı Dizilişler", 34, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket("Hikâye, arena ve kule için farklı takımlarını kaydet; tek dokunuşla geri yükle. Arenadaki savunman her zaman o anki takımındır.", 17, Ui.Soluk, HorizontalAlignment.Center));
		for (int i = 0; i < Kayit.DizilisSayisi; i++)
		{
			int di = i;
			bool bos = Kayit.DizilisBos(i);
			var uyeler = Kayit.DizilisUyeleri(i);
			bool ayni = Kayit.P.Dizilisler[i].SequenceEqual(Kayit.P.Takim);
			var dp = Ui.Panel(ayni && !bos ? new Color("2a3f5f") : Ui.PanelRenk, 12);
			v.AddChild(dp);
			var dv = Ui.Dikey(6);
			dp.AddChild(dv);
			var bas = Ui.Yatay(8);
			dv.AddChild(bas);
			bas.AddChild(Ui.Genis(Ui.Etiket($"Diziliş {i + 1}" + (ayni && !bos ? "  (şu an kullanılıyor)" : ""), 22, Colors.White)));
			var gl = Ui.Etiket(bos ? DizilisOneri[i] : $"Güç {Kayit.TakimGucu(uyeler):N0}", 18, bos ? Ui.Soluk : Ui.Altin, HorizontalAlignment.Right);
			gl.AutowrapMode = TextServer.AutowrapMode.Off;
			bas.AddChild(gl);
			if (bos) dv.AddChild(Ui.Etiket("Boş — şu anki takımını buraya kaydedebilirsin.", 17, Ui.Soluk, HorizontalAlignment.Center));
			else dv.AddChild(MiniTakim(uyeler.Where(u => u != null).ToList(), 92));
			var sat = Ui.Yatay(10);
			dv.AddChild(sat);
			var yb = Ui.Genis(Ui.Dugme("Yükle", () =>
			{
				if (!Kayit.DizilisYukle(di)) return;
				PencereKapat(); Yenile(); Mesaj($"Diziliş {di + 1} yüklendi");
			}, new Color("3478c8"), 20, 52));
			yb.Disabled = bos || ayni;
			sat.AddChild(yb);
			var kb = Ui.Genis(Ui.Dugme(bos ? "Takımı Kaydet" : "Üzerine Kaydet", () =>
			{
				if (!Kayit.TakimHazir) { Mesaj("Takımın boş"); return; }
				Kayit.DizilisKaydet(di);
				PencereKapat(); DizilisPenceresi(); Mesaj($"Takım Diziliş {di + 1}'e kaydedildi");
			}, new Color("2f8a57"), 20, 52));
			kb.Disabled = !Kayit.TakimHazir || ayni;
			sat.AddChild(kb);
		}
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	// ================================================================ REHBER OKU (alt menü)
	Control _rehberOk;

	/// <summary>Başlangıç Yolu'nun gösterdiği sekmenin üstünde zıplayan bir ok gösterir.</summary>
	void RehberOkuGuncelle()
	{
		if (_rehberOk != null && IsInstanceValid(_rehberOk)) _rehberOk.QueueFree();
		_rehberOk = null;
		if (Rehber.Bitti || _menuDugmeleri.Count < 6) return;
		int hedef = Rehber.OdulHazir ? 0 : Rehber.Simdiki.Git switch
		{
			"kartlar" => 1, "takim" => 2, "cagir" => 3, "arena" => 4, _ => 0,
		};
		if (hedef == (int)_ekran) return;
		var b = _menuDugmeleri[hedef];
		var ok = new Control { MouseFilter = MouseFilterEnum.Ignore };
		ok.AnchorLeft = 0.5f; ok.AnchorRight = 0.5f; ok.AnchorTop = 0; ok.AnchorBottom = 0;
		ok.OffsetTop = -30; ok.OffsetBottom = -30;
		var ucgen = new Polygon2D
		{
			Polygon = new[] { new Vector2(-22, -12), new Vector2(22, -12), new Vector2(0, 16) },
			Color = Ui.Altin,
		};
		var kenar = new Line2D { Points = new[] { new Vector2(-22, -12), new Vector2(22, -12), new Vector2(0, 16), new Vector2(-22, -12) }, Width = 5, DefaultColor = new Color("0c0f1e"), JointMode = Line2D.LineJointMode.Round };
		ok.AddChild(kenar);
		ok.AddChild(ucgen);
		// sekmenin etrafında parlayan altın çerçeve
		var cerceve = new Panel { MouseFilter = MouseFilterEnum.Ignore };
		cerceve.AddThemeStyleboxOverride("panel", Ui.Kutu(new Color(0, 0, 0, 0), 14, Ui.Altin, 3, 0));
		cerceve.SetAnchorsPreset(LayoutPreset.FullRect);
		cerceve.AddChild(ok);
		b.AddChild(cerceve);
		_rehberOk = cerceve;
		var ctw = cerceve.CreateTween().SetLoops();
		ctw.TweenProperty(cerceve, "modulate:a", 0.35f, 0.45f).SetTrans(Tween.TransitionType.Sine);
		ctw.TweenProperty(cerceve, "modulate:a", 1f, 0.45f).SetTrans(Tween.TransitionType.Sine);
		var tw = ok.CreateTween().SetLoops();
		tw.TweenProperty(ok, "position:y", -40f, 0.45f).SetTrans(Tween.TransitionType.Sine);
		tw.TweenProperty(ok, "position:y", -28f, 0.45f).SetTrans(Tween.TransitionType.Sine);
	}
}
