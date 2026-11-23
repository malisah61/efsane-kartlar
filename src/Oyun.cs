using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>Ana düğüm: üst kaynak çubuğu, ekranlar, alt menü, açılır pencereler ve savaşlar.</summary>
public partial class Oyun : Control
{
	enum Ekran { Ana, Kartlar, Takim, Cagir, Arena, Sosyal }
	Ekran _ekran = Ekran.Ana;
	MarginContainer _icerik;
	Label _altinL, _elmasL, _parsL, _kupaL, _enerjiL;
	double _saat;
	readonly List<Button> _menuDugmeleri = new();
	readonly List<Control> _rozetler = new();
	Control _katman;
	string _filtre = "hepsi";
	List<ArenaRakip> _rakipler;

	public override void _Ready()
	{
		SetAnchorsPreset(LayoutPreset.FullRect);
		try { Basla(); }
		catch (Exception e)
		{
			GD.PrintErr("BASLATMA HATASI: " + e);
			foreach (var c in GetChildren()) c.QueueFree();
			var l = Ui.Etiket("Başlatma hatası:\n" + e.GetType().Name + ": " + e.Message + "\n\n" + e.StackTrace, 18, Colors.White);
			l.Position = new Vector2(20, 40); l.Size = new Vector2(680, 1200);
			AddChild(l);
		}
	}

	void Basla()
	{
		Veri.Yukle();
		foreach (var arg in OS.GetCmdlineUserArgs())
		{
			if (arg.StartsWith("--denge=")) { DengeTesti.Calistir(arg.Substring(8)); GetTree().Quit(); return; }
			if (arg.StartsWith("--ilerleme=")) { Simulasyon.Calistir(arg.Substring(11)); GetTree().Quit(); return; }
			if (arg.StartsWith("--onizleme=")) { Kayit.KayitKapali = true; Kayit.YeniOyun(); _ = Onizleme.Cek(this, arg.Substring(11)); }
			if (arg.StartsWith("--sunucu-testi="))
			{
				Sunucu.Kur(this);
				string dosya = arg.Substring(15);
				async void Test() { await SunucuTesti.Calistir(this, dosya); GetTree().Quit(); }
				Callable.From(Test).CallDeferred();
				return;
			}
		}
		if (!Kayit.KayitKapali) Kayit.Yukle();
		Ses.Kur(this);
		Odeme.Kur(this);
		Reklam.Kur(this);
		Sunucu.Kur(this);
		PlayGames.Kur(this);
		PlayGames.Bilgi = m => Mesaj(m);
		Sunucu.Degisti = () => { if (IsInstanceValid(this)) { _rakipler = null; Yenile(); } };
		Sunucu.KayitSecimi = KayitSecimPenceresi;
		Reklam.Bilgi = m => Mesaj(m);
		Odeme.Bilgi = m => Mesaj(m);
		Odeme.Tamamlandi = (id, ozet) => { Ses.Efekt("efsanevi"); PencereKapat(); Yenile(); SatinAlmaPenceresi(id, ozet); };
		Ses.MuzikAcik = Kayit.P.MuzikAcik; Ses.EfektAcik = Kayit.P.EfektAcik;
		Ses.Muzik("menu");
		Kur();
		Goster(Ekran.Ana);
		if (!Kayit.KayitKapali)
		{
			// çevrimiçi oyuncularda haftalık ödülleri sunucu dağıtır
			var sezon = Sunucu.OturumVar ? null : Kayit.SezonKontrol();
			if (sezon.HasValue) SezonOdulPenceresi(sezon.Value.sira, sezon.Value.elmas, sezon.Value.eskiKupa);
			else if (!Kayit.P.GirisOdulAlindi) GunlukOdulPenceresi();
		}
	}

	public override void _Notification(int what)
	{
		if (what == NotificationWMGoBackRequest && _katman != null && _katman.GetChildCount() > 0)
			PencereKapat();
	}

	// ================================================================ iskelet
	void Kur()
	{
		AddChild(new ColorRect { Color = Ui.Arkaplan, AnchorRight = 1, AnchorBottom = 1 });
		var kok = Ui.Dikey(0);
		kok.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(kok);

		// üst çubuk
		var ust = Ui.Panel(new Color("0c0f1e"), 10, 0);
		kok.AddChild(ust);
		var uh = Ui.Yatay(6);
		ust.AddChild(uh);
		_altinL = Kaynak(uh, Ui.Altin, "Altın");
		_elmasL = Kaynak(uh, Ui.Elmas, "Elmas");
		_parsL = Kaynak(uh, new Color("e2c79a"), "Parşömen");
		_enerjiL = Kaynak(uh, new Color("8fe36a"), "Enerji");
		_kupaL = Kaynak(uh, new Color("ffb36b"), "Kupa");

		_icerik = new MarginContainer();
		_icerik.SizeFlagsVertical = SizeFlags.ExpandFill;
		foreach (var y in new[] { "left", "right", "top", "bottom" }) _icerik.AddThemeConstantOverride("margin_" + y, 14);
		kok.AddChild(_icerik);

		// alt menü
		var alt = Ui.Panel(new Color("0c0f1e"), 8, 0);
		kok.AddChild(alt);
		var ah = Ui.Yatay(6);
		alt.AddChild(ah);
		var adlar = new[] { ("Ana Sayfa", Ekran.Ana), ("Kartlar", Ekran.Kartlar), ("Takım", Ekran.Takim), ("Çağır", Ekran.Cagir), ("Arena", Ekran.Arena), ("Sosyal", Ekran.Sosyal) };
		foreach (var (ad, ek) in adlar)
		{
			var b = Ui.Genis(Ui.Dugme(ad, () => Goster(ek), new Color("252b4a"), 17, 78));
			_menuDugmeleri.Add(b);
			_rozetler.Add(Rozet(b));
			ah.AddChild(b);
		}

		_katman = new Control { MouseFilter = MouseFilterEnum.Ignore };
		_katman.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(_katman);
	}

	Label Kaynak(HBoxContainer h, Color renk, string ad)
	{
		var p = Ui.Genis(Ui.Panel(new Color("1b2038"), 6, 10));
		var v = Ui.Dikey(0);
		p.AddChild(v);
		v.AddChild(Ui.Etiket(ad, 15, Ui.Soluk, HorizontalAlignment.Center));
		var l = Ui.Etiket("0", 23, renk, HorizontalAlignment.Center);
		v.AddChild(l);
		h.AddChild(p);
		return l;
	}

	void UstGuncelle()
	{
		var p = Kayit.P;
		_altinL.Text = Ui.Sayi(p.Altin);
		_elmasL.Text = Ui.Sayi(p.Elmas);
		_parsL.Text = p.Parsomen.ToString();
		_kupaL.Text = p.Kupa.ToString();
		Kayit.EnerjiGuncelle();
		int kalan = Kayit.SonrakiEnerji();
		_enerjiL.Text = $"{p.Enerji}/{Kayit.EnerjiMax}" + (kalan > 0 ? $" {kalan / 60}:{kalan % 60:00}" : "");
		_enerjiL.AddThemeFontSizeOverride("font_size", kalan > 0 ? 18 : 23);
		RozetleriGuncelle();
	}

	void Goster(Ekran e)
	{
		_ekran = e;
		foreach (var c in _icerik.GetChildren()) c.QueueFree();
		for (int i = 0; i < _menuDugmeleri.Count; i++)
		{
			var c = i == (int)e ? new Color("3b6fd8") : new Color("252b4a");
			_menuDugmeleri[i].AddThemeStyleboxOverride("normal", Ui.Kutu(c, 14));
		}
		Control ekran = e switch
		{
			Ekran.Ana => AnaEkran(),
			Ekran.Kartlar => KartlarEkrani(),
			Ekran.Takim => TakimEkrani(),
			Ekran.Cagir => CagirEkrani(),
			Ekran.Sosyal => SosyalEkrani(),
			_ => ArenaEkrani(),
		};
		_icerik.AddChild(ekran);
		UstGuncelle();
		RehberOkuGuncelle();
	}

	void Yenile() => Goster(_ekran);

	public override void _Process(double delta)
	{
		_saat += delta;
		if (_saat < 1 || _enerjiL == null) return;
		_saat = 0;
		UstGuncelle();
	}

	void SezonOdulPenceresi(int sira, int elmas, int eskiKupa)
	{
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket("Sezon Bitti!", 40, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Geçen hafta lig grubunda {sira}. oldun ({eskiKupa} kupa).", 24, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Ödül: +{elmas} elmas", 28, Ui.Elmas, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Yeni sezon başladı, kupan {Kayit.P.Kupa} oldu.", 20, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Harika!", () => { PencereKapat(); Yenile(); if (!Kayit.P.GirisOdulAlindi) GunlukOdulPenceresi(); }, new Color("2f8a57"), 26));
		Pencere(v);
	}

	void EnerjiPenceresi()
	{
		var p = Kayit.P;
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket("Enerjin Bitti", 34, Ui.Altin, HorizontalAlignment.Center));
		int kalan = Kayit.SonrakiEnerji();
		v.AddChild(Ui.Etiket($"Enerji: {p.Enerji}/{Kayit.EnerjiMax}. Her 6 dakikada 1 enerji dolar" + (kalan > 0 ? $" (sonraki {kalan / 60}:{kalan % 60:00})." : "."), 21, Colors.White, HorizontalAlignment.Center));
		var b = Ui.Dugme($"+{Kayit.EnerjiAlimMiktar} enerji  ({Kayit.EnerjiAlimFiyat} elmas)  —  bugün {Kayit.EnerjiAlimGunluk - p.EnerjiAlim} hak", () =>
		{
			if (Kayit.EnerjiSatinAl()) { PencereKapat(); UstGuncelle(); Mesaj($"+{Kayit.EnerjiAlimMiktar} enerji"); }
			else Mesaj(p.EnerjiAlim >= Kayit.EnerjiAlimGunluk ? "Bugünkü alım hakkın bitti" : "Yeterli elmas yok");
		}, new Color("3478c8"), 21);
		b.Disabled = p.EnerjiAlim >= Kayit.EnerjiAlimGunluk;
		v.AddChild(b);
		var rb = Ui.Dugme($"Reklam izle: +{Kayit.ReklamEnerji} enerji  ({Kayit.ReklamKalan(2)} hak)", () =>
			Reklam.Odullu(() => { if (Kayit.ReklamOdulu(2)) { PencereKapat(); UstGuncelle(); Mesaj($"+{Kayit.ReklamEnerji} enerji"); } }),
			new Color("2f8a57"), 21);
		rb.Disabled = Kayit.ReklamKalan(2) <= 0;
		v.AddChild(rb);
		v.AddChild(Ui.Etiket("Bu arada arenada savaşabilir veya kartlarını yükseltebilirsin.", 18, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	// ================================================================ ANA SAYFA
	Control AnaEkran()
	{
		var v = Ui.Dikey(14);
		var baslik = Ui.Etiket("EFSANE KARTLAR", 46, Ui.Altin, HorizontalAlignment.Center);
		v.AddChild(baslik);
		var p = Kayit.P;
		v.AddChild(Ui.Etiket($"Takım gücü {Kayit.TakimGucu():N0}   •   {Rakipler.Lig(p.Kupa)} Ligi   •   {p.Kartlar.Count}/{Veri.Kartlar.Count} kart", 20, Ui.Soluk, HorizontalAlignment.Center));

		// yeni oyuncu rehberi
		var rehber = RehberPaneli();
		if (rehber != null) v.AddChild(rehber);

		// hikâye
		int asama = Math.Min(p.EnIyiAsama + 1, Rakipler.AsamaSayisi);
		var hp = Ui.Panel(Ui.PanelRenk, 16);
		v.AddChild(hp);
		var hv = Ui.Dikey(10);
		hp.AddChild(hv);
		var hb = Ui.Yatay();
		hv.AddChild(hb);
		hb.AddChild(Ui.Genis(Ui.Etiket("Hikâye", 30, Colors.White), 0.5f));
		hb.AddChild(Ui.Genis(Ui.Etiket($"{Rakipler.BolumAd(asama)} — Aşama {asama}/{Rakipler.AsamaSayisi}" + (Rakipler.BossAsamasi(asama) ? "  (BOSS)" : ""), 20, Rakipler.BossAsamasi(asama) ? Ui.Kirmizi : Ui.Soluk, HorizontalAlignment.Right)));
		var dusman = Rakipler.AsamaTakimi(asama);
		hv.AddChild(MiniTakim(dusman, 100));
		var odul = Rakipler.AsamaOdulu(asama, asama > p.EnIyiAsama);
		hv.AddChild(Ui.Etiket($"Düşman gücü {Kayit.TakimGucu(dusman):N0}   •   Ödül: {odul.altin} altın" + (odul.elmas > 0 ? $", {odul.elmas} elmas" : "") + (odul.parsomen > 0 ? $", {odul.parsomen} parşömen" : ""), 19, Ui.Soluk));
		bool hepsiBitti = p.EnIyiAsama >= Rakipler.AsamaSayisi;
		hv.AddChild(Ui.Dugme((hepsiBitti ? "Son aşamayı tekrar oyna" : "Savaşa Başla") + $"   ({Kayit.HikayeEnerji} enerji)", () => HikayeSavasi(asama), new Color("c0582f"), 28, 76));
		if (p.EnIyiAsama >= 1 && !hepsiBitti)
		{
			int tekrar = p.EnIyiAsama;
			var to = Rakipler.AsamaOdulu(tekrar, false);
			hv.AddChild(Ui.Dugme($"Altın topla: Aşama {tekrar} tekrar  (+{to.altin} altın, {Kayit.HikayeEnerji} enerji)", () => HikayeSavasi(tekrar), new Color("6a5a2f"), 19, 54));
		}
		if (p.EnIyiAsama >= 1) hv.AddChild(HizliGecSatiri(p.EnIyiAsama));

		// haftalık etkinlik
		v.AddChild(EtkinlikDugmesi());

		// kule + sezon kartı
		var ks = Ui.Yatay(12);
		v.AddChild(ks);
		ks.AddChild(Ui.Genis(Ui.Dugme(Kule.Bitti ? "Efsaneler Kulesi\nZirve fethedildi" : $"Efsaneler Kulesi\nKat {Kule.SiradakiKat}/{Kule.KatSayisi}", KulePenceresi, new Color("5a3a9a"), 21, 84)));
		int sOdul = SezonKarti.AlinabilirSayi();
		ks.AddChild(Ui.Genis(Ui.Dugme($"Efsane Yolu\nSeviye {SezonKarti.Seviye}/{SezonKarti.SeviyeSayisi}" + (sOdul > 0 ? $"  •  {sOdul} ödül!" : ""), SezonPenceresi, new Color("9a6a2a"), 21, 84)));

		// günlük ödül + mağaza
		var sat = Ui.Yatay(12);
		v.AddChild(sat);
		var go = Ui.Dugme(p.GirisOdulAlindi ? "Günlük ödül alındı" : $"Günlük Ödül ({Kayit.GirisOdulu.ad})", GunlukOdulPenceresi, new Color("2f8a57"), 22);
		go.Disabled = p.GirisOdulAlindi;
		sat.AddChild(Ui.Genis(go));
		sat.AddChild(Ui.Genis(Ui.Dugme("Mağaza", MagazaPenceresi, new Color("8a4fc0"), 22)));
		var sesSat = Ui.Yatay(12);
		v.AddChild(sesSat);
		sesSat.AddChild(Ui.Genis(Ui.Dugme(p.MuzikAcik ? "Müzik: Açık" : "Müzik: Kapalı", () =>
		{
			p.MuzikAcik = !p.MuzikAcik; Ses.MuzikAyarla(p.MuzikAcik); Kayit.Kaydet(); Yenile();
		}, p.MuzikAcik ? new Color("2f6fb0") : new Color("4a4f66"), 19, 50)));
		sesSat.AddChild(Ui.Genis(Ui.Dugme(p.EfektAcik ? "Sesler: Açık" : "Sesler: Kapalı", () =>
		{
			p.EfektAcik = !p.EfektAcik; Ses.EfektAcik = p.EfektAcik; Kayit.Kaydet(); Yenile();
		}, p.EfektAcik ? new Color("2f6fb0") : new Color("4a4f66"), 19, 50)));

		// görevler
		var gp = Ui.Panel(Ui.PanelRenk, 16);
		v.AddChild(gp);
		var gv = Ui.Dikey(8);
		gp.AddChild(gv);
		gv.AddChild(Ui.Etiket("Günlük Görevler", 26));
		for (int i = 0; i < Kayit.Gorevler.Length; i++)
		{
			int gi = i;
			var g = Kayit.Gorevler[i];
			var satir = Ui.Yatay();
			gv.AddChild(satir);
			int il = Math.Min(g.ilerleme(p), g.hedef);
			satir.AddChild(Ui.Genis(Ui.Etiket($"{g.ad}  ({il}/{g.hedef})", 21, il >= g.hedef ? Colors.White : Ui.Soluk)));
			bool alindi = p.AlinanGorevler.Contains(i);
			var b = Ui.Dugme(alindi ? "Alındı" : $"{g.elmas} elmas", () => { if (Kayit.GorevOdulAl(gi)) { Ses.Efekt("para"); Mesaj($"+{Kayit.Gorevler[gi].elmas} elmas"); Yenile(); } }, Ui.Yesil.Darkened(0.2f), 19, 48);
			b.CustomMinimumSize = new Vector2(160, 48);
			b.Disabled = alindi || !Kayit.GorevTamam(i);
			satir.AddChild(b);
		}
		return Ui.Kaydirma(v);
	}

	Control MiniTakim(IList<TakimUyesi> takim, float g)
	{
		var h = Ui.Yatay(6);
		h.Alignment = BoxContainer.AlignmentMode.Center;
		foreach (var u in takim)
		{
			if (u == null) { h.AddChild(new Control { CustomMinimumSize = new Vector2(g, g * 1.38f) }); continue; }
			var k = new KartKutu(Veri.Harita[u.Id], g) { Seviye = u.Seviye, Yildiz = u.Yildiz };
			k.Tiklandi = () => KartDetayi(Veri.Harita[u.Id], u);
			h.AddChild(k);
		}
		return h;
	}

	// ================================================================ KARTLAR
	Control KartlarEkrani()
	{
		var v = Ui.Dikey(10);
		var p = Kayit.P;
		var bas = Ui.Yatay(10);
		v.AddChild(bas);
		var bl = Ui.Genis(Ui.Etiket($"Koleksiyon {p.Kartlar.Count}/{Veri.Kartlar.Count}\nRuh özü {p.RuhOzu}", 24));
		bl.VerticalAlignment = VerticalAlignment.Center;
		bas.AddChild(bl);
		int bn = Koleksiyon.AlinabilirSayi();
		var bd = Ui.Dugme(bn > 0 ? $"Başarımlar ({bn}!)" : "Başarımlar", KoleksiyonPenceresi, bn > 0 ? new Color("2f8a57") : new Color("5a4a2a"), 21, 60);
		bd.CustomMinimumSize = new Vector2(230, 60);
		bas.AddChild(bd);
		if (bn > 0)
		{
			var tw = bd.CreateTween().SetLoops();
			tw.TweenProperty(bd, "modulate", new Color(1.3f, 1.3f, 1.1f), 0.6f).SetTrans(Tween.TransitionType.Sine);
			tw.TweenProperty(bd, "modulate", Colors.White, 0.6f).SetTrans(Tween.TransitionType.Sine);
		}
		// mitoloji filtresi: 4 sütunlu ızgara (mitoloji sayısı arttıkça satır ekler)
		var f = new GridContainer { Columns = 4 };
		f.AddThemeConstantOverride("h_separation", 6);
		f.AddThemeConstantOverride("v_separation", 6);
		v.AddChild(f);
		foreach (var (k, ad) in new[] { ("hepsi", "Tümü") }.Concat(Veri.Mitolojiler.Select(m => (m.Key, m.Value))))
		{
			int sahip = p.Kartlar.Count(x => k == "hepsi" || Veri.Harita[x.Id].Mitoloji == k);
			int hepsi = Veri.Kartlar.Count(x => k == "hepsi" || x.Mitoloji == k);
			var b = Ui.Genis(Ui.Dugme($"{ad}\n{sahip}/{hepsi}", () => { _filtre = k; Yenile(); }, _filtre == k ? new Color("3b6fd8") : sahip >= hepsi ? new Color("5a4a2a") : new Color("2a3052"), 17, 60));
			f.AddChild(b);
		}
		var izgara = new GridContainer { Columns = 4 };
		izgara.AddThemeConstantOverride("h_separation", 10);
		izgara.AddThemeConstantOverride("v_separation", 10);
		var liste = Veri.Kartlar.Where(k => _filtre == "hepsi" || k.Mitoloji == _filtre)
			.OrderByDescending(k => Kayit.Sahip(k.Id) != null).ThenByDescending(k => k.NadirlikSira)
			.ThenByDescending(k => Kayit.Sahip(k.Id)?.Seviye ?? 0).ThenBy(k => k.Ad);
		foreach (var t in liste)
		{
			var s = Kayit.Sahip(t.Id);
			var kk = s != null ? KartKutu.Sahipten(s, 163) : new KartKutu(t, 163) { Kilitli = true };
			kk.KopyaGoster = true;
			if (s != null && (s.Kopya >= Kayit.YildizKopya(s.Yildiz) && s.Yildiz < Kayit.MaxYildiz)) kk.Secili = true;
			kk.Tiklandi = () => KartDetayi(t, null);
			izgara.AddChild(kk);
		}
		v.AddChild(izgara);
		v.AddChild(Ui.Etiket("Altın çerçeve: yıldız atlatmaya hazır.", 17, Ui.Soluk));
		return Ui.Kaydirma(v);
	}

	void KartDetayi(KartTanim t, TakimUyesi gosterim, string kutlama = null)
	{
		var s = Kayit.Sahip(t.Id);
		bool rakip = gosterim != null && (s == null || s.Seviye != gosterim.Seviye || s.Yildiz != gosterim.Yildiz || !Kayit.P.Takim.Contains(t.Id));
		int sev = rakip ? gosterim.Seviye : s?.Seviye ?? 1;
		int yil = rakip ? gosterim.Yildiz : s?.Yildiz ?? 1;

		var v = Ui.Dikey(10);
		var ust = Ui.Yatay(16);
		v.AddChild(ust);
		var kk = new KartKutu(t, 250) { Seviye = sev, Yildiz = yil, Kilitli = s == null && !rakip };
		ust.AddChild(kk);
		var bilgi = Ui.Genis(Ui.Dikey(6));
		ust.AddChild(bilgi);
		bilgi.AddChild(Ui.Etiket(t.Ad, 34, Veri.NadirlikRenk(t.Nadirlik)));
		bilgi.AddChild(Ui.Etiket($"{Veri.NadirlikAd(t.Nadirlik)} • {Veri.SinifAd(t.Sinif)}", 21, Ui.Soluk));
		bilgi.AddChild(Ui.Etiket($"{Veri.MitolojiAd(t.Mitoloji)} mitolojisi", 21, Ui.Soluk));
		bilgi.AddChild(Ui.Etiket($"Element: {Veri.ElementAd(t.Element)}", 21, Veri.ElementRenk(t.Element)));
		var st = Veri.Stat(t, sev, yil);
		bilgi.AddChild(Ui.Etiket($"Can {st.can:0}\nSaldırı {st.sal:0}\nSavunma {st.sav:0}\nHız {st.hiz:0}", 22));
		bilgi.AddChild(Ui.Etiket($"Güç {Veri.Guc(t, sev, yil):N0}", 22, Ui.Altin));

		var up = Ui.Panel(Ui.PanelAcik, 12);
		v.AddChild(up);
		var uv = Ui.Dikey(4);
		up.AddChild(uv);
		uv.AddChild(Ui.Etiket($"Ulti: {t.UltiAd}", 24, Ui.Altin));
		uv.AddChild(Ui.Etiket(Veri.UltiAciklama(t), 20));
		uv.AddChild(Ui.Etiket("Enerji 100 olunca otomatik kullanılır.", 16, Ui.Soluk));
		if (t.Unvan != "") bilgi.AddChild(Ui.Etiket(t.Unvan, 20, new Color("e8d9a8")));
		int bagSay = Veri.KartBaglari(t.Id).Count();
		v.AddChild(Ui.Dugme(bagSay > 0 ? $"Efsanesi ve Bağları ({bagSay})" : "Efsanesi", () => KartHikaye(t, gosterim), new Color("5a4a2a"), 21, 56));

		if (s != null && !rakip)
		{
			int maxS = Kayit.MaxSeviye(s.Yildiz);
			int mal = Kayit.SeviyeMaliyet(s.Seviye);
			var sbSat = Ui.Yatay(8);
			v.AddChild(sbSat);
			var sb = Ui.Genis(Ui.Dugme(s.Seviye >= maxS ? $"En yüksek seviye ({maxS})" : $"Seviye Atlat → {s.Seviye + 1}   ({mal:N0} altın)", () =>
			{
				if (Kayit.SeviyeAtlat(s)) { PencereKapat(); KartDetayi(t, null, $"Seviye {s.Seviye}!"); Yenile(); }
				else Mesaj("Yeterli altın yok");
			}, new Color("c0912f"), 22));
			sb.Disabled = s.Seviye >= maxS;
			sbSat.AddChild(sb);
			// çoklu seviye: altının yettiği kadar (en fazla 5)
			int coklu = 0, cokluMal = 0;
			for (int sv = s.Seviye; sv < maxS && coklu < 5 && cokluMal + Kayit.SeviyeMaliyet(sv) <= Kayit.P.Altin; sv++) { cokluMal += Kayit.SeviyeMaliyet(sv); coklu++; }
			if (s.Seviye < maxS && coklu > 1)
			{
				var cb = Ui.Dugme($"+{coklu} sv\n{Ui.Sayi(cokluMal)} altın", () =>
				{
					int n = 0;
					for (int i = 0; i < coklu; i++) if (Kayit.SeviyeAtlat(s)) n++;
					if (n > 0) { PencereKapat(); KartDetayi(t, null, $"+{n} Seviye!"); Yenile(); }
				}, new Color("a0702a"), 18, 64);
				cb.CustomMinimumSize = new Vector2(150, 64);
				sbSat.AddChild(cb);
			}
			int gk = Kayit.YildizKopya(s.Yildiz);
			var yb = Ui.Dugme(s.Yildiz >= Kayit.MaxYildiz ? "En yüksek yıldız" : $"Yıldız Atlat → {s.Yildiz + 1}★  (kopya {s.Kopya}/{gk}, {Kayit.YildizAltin(s.Yildiz):N0} altın)", () =>
			{
				if (Kayit.YildizAtlat(s)) { PencereKapat(); KartDetayi(t, null, $"{s.Yildiz} Yıldız!"); Yenile(); }
			}, new Color("8a4fc0"), 22);
			yb.Text = yb.Text.Replace("★", " yıldız");
			yb.Disabled = s.Yildiz >= Kayit.MaxYildiz || s.Kopya < gk || Kayit.P.Altin < Kayit.YildizAltin(s.Yildiz);
			v.AddChild(yb);
			int ozf = Kayit.OzKopyaFiyat(t.Nadirlik);
			var ozb = Ui.Dugme($"Ruh özüyle +1 kopya ({ozf} öz — sende {Kayit.P.RuhOzu})", () =>
			{
				if (Kayit.OzleKopyaAl(s)) { PencereKapat(); KartDetayi(t, null); Yenile(); }
				else Mesaj("Yeterli ruh özü yok");
			}, new Color("4a6a8a"), 19, 52);
			ozb.Disabled = s.Yildiz >= Kayit.MaxYildiz || Kayit.P.RuhOzu < ozf;
			v.AddChild(ozb);
			bool takimda = Kayit.P.Takim.Contains(t.Id);
			v.AddChild(Ui.Dugme(takimda ? "Takımdan Çıkar" : "Takıma Ekle", () =>
			{
				var tk = Kayit.P.Takim;
				if (takimda) tk[tk.IndexOf(t.Id)] = null;
				else
				{
					int bos = tk.IndexOf(null);
					if (bos < 0) { Mesaj("Takım dolu, önce bir kart çıkar"); return; }
					tk[bos] = t.Id;
				}
				Kayit.Kaydet();
				PencereKapat(); Yenile();
			}, new Color("2f6fb0"), 22));
		}
		else if (s == null)
			v.AddChild(Ui.Etiket(rakip ? "Bu kart sende yok. Çağırarak kazanabilirsin." : "Bu kart henüz sende yok. Çağır ekranından kazanabilirsin.", 19, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
		if (kutlama != null) Kutla(kk, kutlama, kutlama.Contains("Yıldız") ? new Color("c58bff") : Ui.Altin);
	}

	/// <summary>Kartın biyografisi ve takım bağları (simya).</summary>
	void KartHikaye(KartTanim t, TakimUyesi gosterim)
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket(t.Ad, 34, Veri.NadirlikRenk(t.Nadirlik), HorizontalAlignment.Center));
		if (t.Unvan != "") v.AddChild(Ui.Etiket($"{t.Unvan} • {Veri.MitolojiAd(t.Mitoloji)} mitolojisi", 20, new Color("e8d9a8"), HorizontalAlignment.Center));
		var bp = Ui.Panel(Ui.PanelAcik, 14);
		v.AddChild(bp);
		bp.AddChild(Ui.Etiket(t.Biyo == "" ? "Bu kahramanın hikâyesi henüz yazılmadı." : t.Biyo, 20));

		var baglar = Veri.KartBaglari(t.Id).ToList();
		v.AddChild(Ui.Etiket("Takım Bağları", 26, Ui.Altin));
		if (baglar.Count == 0)
			v.AddChild(Ui.Etiket("Bu kartın özel bir bağı yok; mitoloji uyumundan yine de yararlanır.", 18, Ui.Soluk));
		var takim = Kayit.P.Takim;
		foreach (var b in baglar)
		{
			bool aktif = b.Kartlar.All(k => takim.Contains(k));
			bool hepsiVar = b.Kartlar.All(k => Kayit.Sahip(k) != null);
			var pp = Ui.Panel(aktif ? new Color("1f4a32") : Ui.PanelRenk, 10);
			v.AddChild(pp);
			var pv = Ui.Dikey(2);
			pp.AddChild(pv);
			pv.AddChild(Ui.Etiket(b.Ad + (aktif ? "  (takımda AKTİF)" : ""), 21, aktif ? Ui.Yesil : Colors.White));
			pv.AddChild(Ui.Etiket(b.Ozet, 18, Ui.Altin));
			pv.AddChild(Ui.Etiket(b.Aciklama + (aktif || hepsiVar ? "" : "  Eksik: " + string.Join(", ", b.Kartlar.Where(k => Kayit.Sahip(k) == null).Select(k => Veri.Harita[k].Ad))), 16, Ui.Soluk));
		}
		v.AddChild(Ui.Etiket("Bağ bonusunu yalnızca bağdaki kartlar alır; hepsi aynı takımda olmalı.", 16, Ui.Soluk, HorizontalAlignment.Center));
		var sat = Ui.Yatay(10);
		v.AddChild(sat);
		sat.AddChild(Ui.Genis(Ui.Dugme("Geri", () => KartDetayi(t, gosterim), new Color("444c6a"), 22)));
		sat.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22)));
		Pencere(v);
	}

	/// <summary>Takım ekranındaki simya paneli: aktif bağlar + bir kartı eksik olanlar.</summary>
	Control SimyaPaneli(List<TakimUyesi> dolu)
	{
		var ids = dolu.Select(u => u.Id).ToList();
		var aktif = Veri.AktifBaglar(ids);
		var yakin = Veri.Baglar.Where(b => !aktif.Contains(b) && b.Kartlar.Count(ids.Contains) >= 1 && b.Kartlar.Count(k => !ids.Contains(k)) == 1
			&& Kayit.Sahip(b.Kartlar.First(k => !ids.Contains(k))) != null).Take(3).ToList();
		var p = Ui.Panel(Ui.PanelRenk, 10);
		var v = Ui.Dikey(4);
		p.AddChild(v);
		v.AddChild(Ui.Etiket(aktif.Count > 0 ? $"Takım Simyası: {aktif.Count} bağ aktif" : "Takım Simyası", 21, Ui.Altin));
		foreach (var b in aktif) v.AddChild(Ui.Etiket($"{b.Ad} — {b.Ozet}", 17, Ui.Yesil));
		foreach (var b in yakin)
			v.AddChild(Ui.Etiket($"{b.Ad}: {Veri.Harita[b.Kartlar.First(k => !ids.Contains(k))].Ad} eklersen {b.StatAd} +%{b.Oran * 100:0}", 17, Ui.Soluk));
		if (aktif.Count == 0 && yakin.Count == 0)
			v.AddChild(Ui.Etiket("Efsanelerde birbirine bağlı kahramanları birlikte koy (ör. Zeus + Hades, Amaterasu + Susanoo). Kartın Efsanesi ekranında bağlarını gör.", 17, Ui.Soluk));
		return p;
	}

	// ================================================================ TAKIM
	Control TakimEkrani()
	{
		var v = Ui.Dikey(10);
		var p = Kayit.P;
		var uyeler = Kayit.TakimUyeleri();
		var dolu = uyeler.Where(u => u != null).ToList();
		int enCok = dolu.Count == 0 ? 0 : dolu.GroupBy(u => Veri.Harita[u.Id].Mitoloji).Max(g => g.Count());
		string enCokAd = dolu.Count == 0 ? "" : Veri.MitolojiAd(dolu.GroupBy(u => Veri.Harita[u.Id].Mitoloji).OrderByDescending(g => g.Count()).First().Key);
		v.AddChild(Ui.Etiket($"Takım Gücü {Kayit.TakimGucu():N0}", 30, Ui.Altin));
		float bonus = Savas.MitolojiBonus(enCok);
		v.AddChild(Ui.Etiket(bonus > 0 ? $"{enCokAd} uyumu ({enCok} kart): can ve saldırı +%{bonus * 100:0}" : "Aynı mitolojiden 2+ kart koyarsan bonus kazanırsın.", 19, bonus > 0 ? Ui.Yesil : Ui.Soluk));
		v.AddChild(SimyaPaneli(dolu));

		var dizilis = Ui.Panel(Ui.PanelRenk, 12);
		v.AddChild(dizilis);
		var dv = Ui.Dikey(8);
		dizilis.AddChild(dv);
		dv.AddChild(Ui.Etiket("Ön sıra (önce bunlar vurulur)", 18, Ui.Soluk, HorizontalAlignment.Center));
		dv.AddChild(Slotlar(new[] { 0, 1 }));
		dv.AddChild(Ui.Etiket("Arka sıra", 18, Ui.Soluk, HorizontalAlignment.Center));
		dv.AddChild(Slotlar(new[] { 2, 3, 4 }));

		var sat = Ui.Yatay(10);
		v.AddChild(sat);
		sat.AddChild(Ui.Genis(Ui.Dugme("Otomatik Diz", () => { OtomatikDiz(); Yenile(); }, new Color("2f8a57"), 22)));
		sat.AddChild(Ui.Genis(Ui.Dugme("Dizilişler", DizilisPenceresi, new Color("3478c8"), 22)));
		sat.AddChild(Ui.Genis(Ui.Dugme("Temizle", () => { for (int i = 0; i < 5; i++) p.Takim[i] = null; Kayit.Kaydet(); Yenile(); }, new Color("6a4c4c"), 22)));

		v.AddChild(Ui.Etiket("Kartların (özellikleri görmek ve eklemek için dokun)", 22));
		var izgara = new GridContainer { Columns = 5 };
		izgara.AddThemeConstantOverride("h_separation", 8);
		izgara.AddThemeConstantOverride("v_separation", 8);
		foreach (var s in p.Kartlar.Where(k => !p.Takim.Contains(k.Id))
			.OrderByDescending(k => Veri.Guc(Veri.Harita[k.Id], k.Seviye, k.Yildiz)))
		{
			var kk = KartKutu.Sahipten(s, 128);
			kk.Tiklandi = () => TakimKartPenceresi(s);
			izgara.AddChild(kk);
		}
		v.AddChild(izgara);
		return Ui.Kaydirma(v);
	}

	Control Slotlar(int[] idx)
	{
		var h = Ui.Yatay(12);
		h.Alignment = BoxContainer.AlignmentMode.Center;
		foreach (int i in idx)
		{
			var id = Kayit.P.Takim[i];
			var s = id == null ? null : Kayit.Sahip(id);
			if (s == null)
			{
				var bos = Ui.Panel(new Color("252b4a"), 4, 12);
				bos.CustomMinimumSize = new Vector2(140, 193);
				var l = Ui.Etiket("Boş", 22, Ui.Soluk, HorizontalAlignment.Center);
				l.VerticalAlignment = VerticalAlignment.Center;
				bos.AddChild(l);
				h.AddChild(bos);
				continue;
			}
			var kk = KartKutu.Sahipten(s, 140);
			int si = i;
			kk.Tiklandi = () => TakimKartPenceresi(s);
			h.AddChild(kk);
		}
		return h;
	}

	static void OtomatikDiz() => Kayit.OtomatikDiz();

	/// <summary>Takım ekranında karta dokununca: özellikler, ulti, mitoloji, bağlar ve ekle/çıkar/yer değiştir.</summary>
	void TakimKartPenceresi(SahipKart s)
	{
		var t = Veri.Harita[s.Id];
		var tk = Kayit.P.Takim;
		bool takimda = tk.Contains(s.Id);
		var v = Ui.Dikey(8);
		var ust = Ui.Yatay(14);
		v.AddChild(ust);
		ust.AddChild(KartKutu.Sahipten(s, 170));
		var bilgi = Ui.Genis(Ui.Dikey(4));
		ust.AddChild(bilgi);
		bilgi.AddChild(Ui.Etiket(t.Ad, 30, Veri.NadirlikRenk(t.Nadirlik)));
		if (t.Unvan != "") bilgi.AddChild(Ui.Etiket(t.Unvan, 18, new Color("e8d9a8")));
		bilgi.AddChild(Ui.Etiket($"{Veri.MitolojiAd(t.Mitoloji)} mitolojisi", 21, Veri.MitolojiRenk(t.Mitoloji).Lightened(0.35f)));
		bilgi.AddChild(Ui.Etiket($"{Veri.NadirlikAd(t.Nadirlik)} {Veri.SinifAd(t.Sinif)} • {Veri.ElementAd(t.Element)}", 19, Veri.ElementRenk(t.Element)));
		bilgi.AddChild(Ui.Etiket($"Seviye {s.Seviye} • {s.Yildiz} yıldız • Güç {Veri.Guc(t, s.Seviye, s.Yildiz):N0}", 19, Ui.Altin));
		var st = Veri.Stat(t, s.Seviye, s.Yildiz);
		bilgi.AddChild(Ui.Etiket($"Can {st.can:0}   Saldırı {st.sal:0}\nSavunma {st.sav:0}   Hız {st.hiz:0}", 20));
		string yer = t.Sinif switch
		{
			"tank" => "Ön sırada düşman saldırılarını karşılar.",
			"savasci" => "Ön sırada ya da arkada yüksek tek hedef hasarı verir.",
			"buyucu" => "Arka sırada; rastgele hedeflere büyü yapar.",
			_ => "Arka sırada takımı iyileştirir ve güçlendirir.",
		};
		bilgi.AddChild(Ui.Etiket(yer, 16, Ui.Soluk));

		var up = Ui.Panel(Ui.PanelAcik, 10);
		v.AddChild(up);
		var uv = Ui.Dikey(2);
		up.AddChild(uv);
		uv.AddChild(Ui.Etiket($"Ulti: {t.UltiAd}", 22, Ui.Altin));
		uv.AddChild(Ui.Etiket(Veri.UltiAciklama(t), 18));

		// bu kart takıma girince/çıkınca bağlar ve uyum
		var digerleri = tk.Where(x => x != null && x != s.Id).ToList();
		var ileTakim = digerleri.Append(s.Id).ToList();
		var yeniBag = Veri.AktifBaglar(ileTakim).Where(b => b.Kartlar.Contains(s.Id)).ToList();
		int ayniMit = ileTakim.Count(x => Veri.Harita[x].Mitoloji == t.Mitoloji);
		var bp = Ui.Panel(Ui.PanelRenk, 10);
		v.AddChild(bp);
		var bv = Ui.Dikey(2);
		bp.AddChild(bv);
		bv.AddChild(Ui.Etiket(takimda ? "Takımdaki katkısı" : "Takıma girerse", 19, Colors.White));
		bv.AddChild(Ui.Etiket(ayniMit >= 2 ? $"• {Veri.MitolojiAd(t.Mitoloji)} uyumu: {ayniMit} kart (+%{Savas.MitolojiBonus(ayniMit) * 100:0} can ve saldırı)" : $"• Takımda başka {Veri.MitolojiAd(t.Mitoloji)} kartı yok (uyum için 2+ gerekir)", 17, ayniMit >= 2 ? Ui.Yesil : Ui.Soluk));
		if (yeniBag.Count > 0)
			foreach (var b in yeniBag) bv.AddChild(Ui.Etiket($"• Bağ: {b.Ad} (+%{b.Oran * 100:0} {b.StatAd})", 17, Ui.Yesil));
		else
		{
			var kartBag = Veri.KartBaglari(s.Id).Take(2).ToList();
			bv.AddChild(Ui.Etiket(kartBag.Count == 0 ? "• Aktif bağ yok" : "• Bağ için gerekenler: " + string.Join("; ", kartBag.Select(b => string.Join(" + ", b.Kartlar.Where(k => k != s.Id).Select(k => Veri.Harita[k].Ad)))), 17, Ui.Soluk));
		}

		if (takimda)
			v.AddChild(Ui.Dugme("Takımdan Çıkar", () => { tk[tk.IndexOf(s.Id)] = null; Kayit.Kaydet(); PencereKapat(); Yenile(); }, new Color("8a4c4c"), 22, 58));
		else if (tk.Contains(null))
			v.AddChild(Ui.Dugme("Takıma Ekle", () =>
			{
				int bos = tk.IndexOf(null);
				if (bos < 0) return;
				tk[bos] = s.Id; Kayit.Kaydet(); PencereKapat(); Yenile();
			}, new Color("2f8a57"), 22, 58));
		else
		{
			v.AddChild(Ui.Etiket("Takım dolu. Kimin yerine girsin?", 18, Ui.Soluk, HorizontalAlignment.Center));
			var yer2 = Ui.Yatay(6);
			yer2.Alignment = BoxContainer.AlignmentMode.Center;
			v.AddChild(yer2);
			for (int i = 0; i < 5; i++)
			{
				int si = i;
				var ds = Kayit.Sahip(tk[i]);
				if (ds == null) continue;
				var dk = KartKutu.Sahipten(ds, 112);
				dk.Tiklandi = () => { tk[si] = s.Id; Kayit.Kaydet(); PencereKapat(); Yenile(); Mesaj($"{t.Ad} takıma girdi"); };
				yer2.AddChild(dk);
			}
		}
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		alt.AddChild(Ui.Genis(Ui.Dugme("Tüm Ayrıntılar", () => KartDetayi(t, null), new Color("5a4a2a"), 20, 52)));
		alt.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 20, 52)));
		Pencere(v);
	}

	// ================================================================ ÇAĞIR
	Control CagirEkrani()
	{
		var v = Ui.Dikey(14);
		var p = Kayit.P;
		v.AddChild(Ui.Etiket("Kahraman Çağır", 34, Ui.Altin, HorizontalAlignment.Center));

		var ep = Ui.Panel(new Color("2b2350"), 18);
		v.AddChild(ep);
		var ev = Ui.Dikey(10);
		ep.AddChild(ev);
		ev.AddChild(Ui.Etiket("Efsane Çağrısı", 30, Ui.Elmas));
		ev.AddChild(Ui.Etiket("Efsanevi %3  •  Epik %13  •  Nadir %34  •  Sıradan %50", 19, Ui.Soluk));
		int kalan = Kayit.EfsaneviGaranti - p.ElmasCekimSayaci;
		ev.AddChild(Ui.Etiket($"Efsanevi garantisine {kalan} çekim kaldı", 21, Veri.NadirlikRenk("efsanevi")));
		var eb = Ui.Yatay(10);
		ev.AddChild(eb);
		var b1 = Ui.Genis(Ui.Dugme($"1x  ({Kayit.ElmasCekimFiyat} elmas)", () => Cagir("elmas", 1), new Color("3478c8"), 22, 72));
		b1.Disabled = p.Elmas < Kayit.ElmasCekimFiyat;
		eb.AddChild(b1);
		var b10 = Ui.Genis(Ui.Dugme($"10x  ({Kayit.ElmasOnluFiyat} elmas)", () => Cagir("elmas", 10), new Color("8a4fc0"), 22, 72));
		b10.Disabled = p.Elmas < Kayit.ElmasOnluFiyat;
		eb.AddChild(b10);

		var pp = Ui.Panel(new Color("3a2f22"), 18);
		v.AddChild(pp);
		var pv = Ui.Dikey(10);
		pp.AddChild(pv);
		pv.AddChild(Ui.Etiket("Parşömen Çağrısı", 30, new Color("e2c79a")));
		pv.AddChild(Ui.Etiket("Epik %4  •  Nadir %26  •  Sıradan %70", 19, Ui.Soluk));
		pv.AddChild(Ui.Etiket($"Parşömenin: {p.Parsomen}  (hikâye ve günlük ödüllerden kazanılır)", 19, Ui.Soluk));
		var pb = Ui.Yatay(10);
		pv.AddChild(pb);
		var p1 = Ui.Genis(Ui.Dugme("1x  (1 parşömen)", () => Cagir("parsomen", 1), new Color("a0743a"), 22, 72));
		p1.Disabled = p.Parsomen < 1;
		pb.AddChild(p1);
		var p5 = Ui.Genis(Ui.Dugme("5x  (5 parşömen)", () => Cagir("parsomen", 5), new Color("a0743a"), 22, 72));
		p5.Disabled = p.Parsomen < 5;
		pb.AddChild(p5);
		var rc = Ui.Dugme(Kayit.ReklamKalan(3) > 0 ? "Reklam izle: ücretsiz parşömen çağrısı (günde 1)" : "Ücretsiz çağrı yarın yenilenir", () =>
			Reklam.Odullu(() => { if (Kayit.ReklamOdulu(3)) { p.Parsomen++; Cagir("parsomen", 1); } }), new Color("2f8a57"), 20);
		rc.Disabled = Kayit.ReklamKalan(3) <= 0;
		pv.AddChild(rc);

		v.AddChild(Ui.Etiket("10'lu çağrıda en az 1 epik garanti. Aynı kartı tekrar çekersen kopya ve ruh özü kazanırsın; ruh özüyle istediğin kartın kopyasını alabilirsin.", 18, Ui.Soluk, HorizontalAlignment.Center));
		return Ui.Kaydirma(v);
	}

	void Cagir(string tur, int adet)
	{
		var oncedenSahip = Kayit.P.Kartlar.Select(k => k.Id).ToHashSet();
		var sonuc = Kayit.Cagir(tur, adet);
		if (sonuc.Count == 0) { Mesaj("Yeterli kaynak yok"); return; }
		Yenile();
		CagirSonucu(sonuc, oncedenSahip);
	}

	void CagirSonucu(List<KartTanim> sonuc, HashSet<string> oncedenSahip)
	{
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket("Çağrı Sonucu", 34, Ui.Altin, HorizontalAlignment.Center));
		bool tek = sonuc.Count == 1;
		var izgara = new GridContainer { Columns = tek ? 1 : 5 };
		izgara.AddThemeConstantOverride("h_separation", 8);
		izgara.AddThemeConstantOverride("v_separation", 8);
		var c = new CenterContainer();
		c.AddChild(izgara);
		v.AddChild(c);
		var gorulen = new HashSet<string>(oncedenSahip);
		var acicilar = new List<Action>();
		var tweenler = new List<Tween>();
		float gecikme = 0.35f;
		foreach (var t in sonuc)
		{
			bool yeni = gorulen.Add(t.Id);
			float w = tek ? 240 : 118;
			var kut = Ui.Dikey(2);
			var kk = new KartKutu(t, w) { Arka = true, PivotOffset = new Vector2(w / 2, w * 1.38f / 2) };
			kut.AddChild(kk);
			var et = Ui.Etiket(yeni ? "YENİ!" : "+1 kopya", tek ? 24 : 16, yeni ? Ui.Yesil : Ui.Soluk, HorizontalAlignment.Center);
			et.Modulate = new Color(1, 1, 1, 0);
			kut.AddChild(et);
			izgara.AddChild(kut);
			int nad = t.NadirlikSira;
			var renk = Veri.NadirlikRenk(t.Nadirlik);
			void Ac()
			{
				if (!IsInstanceValid(kk)) return;
				kk.Arka = false; kk.QueueRedraw();
				kk.Scale = Vector2.One; kk.Modulate = Colors.White; et.Modulate = Colors.White;
				kk.Tiklandi = () => KartDetayi(t, null);
			}
			var tw = kk.CreateTween();
			tw.TweenInterval(gecikme);
			if (nad >= 2)
			{
				// epik ve efsanevi: kart açılmadan önce nadirlik renginde parlar ve titrer
				tw.TweenCallback(Callable.From(() => Ses.Efekt("kart", -2, 0.7f, 0)));
				tw.TweenProperty(kk, "modulate", new Color(renk.R * 1.9f, renk.G * 1.9f, renk.B * 1.9f), 0.3f);
				tw.Parallel().TweenProperty(kk, "rotation", 0.06f, 0.08f);
				tw.TweenProperty(kk, "rotation", -0.06f, 0.08f);
				tw.TweenProperty(kk, "rotation", 0f, 0.08f);
			}
			tw.TweenProperty(kk, "scale:x", 0f, 0.12f).SetTrans(Tween.TransitionType.Sine);
			tw.TweenCallback(Callable.From(() =>
			{
				kk.Arka = false; kk.QueueRedraw();
				kk.Tiklandi = () => KartDetayi(t, null);
				Ses.Efekt(nad >= 3 ? "efsanevi" : "kart", nad >= 2 ? 2 : 0, 1f + nad * 0.08f, 0.03f);
				if (nad >= 2) Efekt.Patlama(kut, kk.Position + kk.Size / 2, renk, nad >= 3 ? 60 : 30, nad >= 3 ? 340 : 240, 6, 0.8f, 150);
				if (nad >= 3) Flas(renk);
			}));
			tw.TweenProperty(kk, "scale:x", 1f, 0.14f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
			tw.Parallel().TweenProperty(kk, "modulate", Colors.White, 0.35f);
			tw.TweenProperty(et, "modulate:a", 1f, 0.2f);
			tweenler.Add(tw);
			acicilar.Add(Ac);
			gecikme += nad >= 2 ? 0.75f : 0.16f;
		}
		var en = sonuc.OrderByDescending(x => x.NadirlikSira).First();
		Label enL = null;
		if (en.NadirlikSira >= 2)
		{
			enL = Ui.Etiket($"{Veri.NadirlikAd(en.Nadirlik)} kart: {en.Ad}!", 26, Veri.NadirlikRenk(en.Nadirlik), HorizontalAlignment.Center);
			enL.Modulate = new Color(1, 1, 1, 0);
			v.AddChild(enL);
			var etw = enL.CreateTween();
			etw.TweenInterval(gecikme + 0.2f);
			etw.TweenProperty(enL, "modulate:a", 1f, 0.3f);
			tweenler.Add(etw);
		}
		var sat = Ui.Yatay(10);
		v.AddChild(sat);
		Button hepsi = null;
		hepsi = Ui.Genis(Ui.Dugme("Hepsini Aç", () =>
		{
			foreach (var x in tweenler) if (x.IsValid()) x.Kill();
			foreach (var a in acicilar) a();
			if (enL != null) enL.Modulate = Colors.White;
			hepsi.Disabled = true;
		}, new Color("6a4fa0"), 22));
		sat.AddChild(hepsi);
		var stw = hepsi.CreateTween();
		stw.TweenInterval(gecikme + 0.4f);
		stw.TweenCallback(Callable.From(() => hepsi.Disabled = true));
		sat.AddChild(Ui.Genis(Ui.Dugme("Tamam", PencereKapat, new Color("3b6fd8"), 24)));
		Pencere(v);
	}

	/// <summary>Efsanevi çekimde tüm ekranı kısa süre parlatır.</summary>
	void Flas(Color renk)
	{
		var f = new ColorRect { Color = new Color(renk, 0.55f), MouseFilter = MouseFilterEnum.Ignore };
		f.SetAnchorsPreset(LayoutPreset.FullRect);
		AddChild(f);
		var tw = f.CreateTween();
		tw.TweenProperty(f, "color:a", 0f, 0.6f);
		tw.TweenCallback(Callable.From(f.QueueFree));
	}

	// ================================================================ ARENA
	Control ArenaEkrani()
	{
		var v = Ui.Dikey(12);
		var p = Kayit.P;
		if (_rakipler == null)
		{
			if (Sunucu.Girisli) { CevrimiciRakipleriYukle(); v.AddChild(Ui.Etiket("Rakipler aranıyor…", 24, Ui.Soluk)); return Ui.Kaydirma(v); }
			_rakipler = Rakipler.ArenaRakipleri();
		}
		v.AddChild(Ui.Etiket($"Arena — {Rakipler.Lig(p.Kupa)} Ligi", 32, Ui.Altin));
		int sonraki = Rakipler.SonrakiLigKupa(p.Kupa);
		v.AddChild(Ui.Etiket($"Kupa {p.Kupa}" + (sonraki > 0 ? $"  (sonraki lig: {sonraki})" : "") + $"   •   G/M {p.ArenaGalibiyet}/{p.ArenaMaglubiyet}", 20, Ui.Soluk));
		var kalanS = Rakipler.SezonKalan();
		if (Sunucu.Girisli)
			v.AddChild(Ui.Etiket($"Gerçek oyunculara karşı savaşıyorsun   •   Haftalık sıralama bitimine {kalanS.Days}g {kalanS.Hours}s", 20, Ui.Elmas));
		else
		{
			int sira = Rakipler.OyuncuSirasi();
			v.AddChild(Ui.Etiket($"Grup sıran: {sira}/{Rakipler.GrupBoyu}  (ödül {Rakipler.SezonOdulu(sira)} elmas)   •   Sezon bitimine {kalanS.Days}g {kalanS.Hours}s", 20, Ui.Elmas));
			v.AddChild(Ui.Etiket("Gerçek oyuncularla savaşmak ve dünya sıralamasına girmek için Sosyal sekmesinden giriş yap.", 17, Ui.Soluk));
		}
		var hs = Ui.Yatay(10);
		v.AddChild(hs);
		hs.AddChild(Ui.Genis(Ui.Etiket($"Kalan savaş hakkı: {p.ArenaHak}", 22, p.ArenaHak > 0 ? Colors.White : Ui.Kirmizi)));
		var hak = Ui.Dugme($"+1 hak ({Kayit.ArenaEkHakFiyat} elmas)", () =>
		{
			if (p.Elmas < Kayit.ArenaEkHakFiyat) { Mesaj("Yeterli elmas yok"); return; }
			p.Elmas -= Kayit.ArenaEkHakFiyat; p.ArenaHak++; Kayit.Kaydet(); Yenile();
		}, new Color("3478c8"), 18, 50);
		hak.CustomMinimumSize = new Vector2(220, 50);
		hs.AddChild(hak);
		v.AddChild(Ui.Etiket("Rakiplerin kayıtlı savunma takımlarıyla savaşırsın; onların çevrimiçi olması gerekmez.", 17, Ui.Soluk));

		string[] zad = { "Kolay", "Denk", "Zor" };
		Color[] zrenk = { Ui.Yesil, Ui.Altin, Ui.Kirmizi };
		foreach (var r in _rakipler)
		{
			var rp = Ui.Panel(Ui.PanelRenk, 12);
			v.AddChild(rp);
			var rv = Ui.Dikey(6);
			rp.AddChild(rv);
			var rb = Ui.Yatay();
			rv.AddChild(rb);
			rb.AddChild(Ui.Genis(Ui.Etiket($"{r.Ad}   ({r.Kupa} kupa)" + (Sunucu.Girisli && r.OyuncuId == null ? "  • bot" : ""), 23)));
			rb.AddChild(Ui.Genis(Ui.Etiket($"Güç {r.Guc:N0}  •  {zad[r.Zorluk]}", 20, zrenk[r.Zorluk], HorizontalAlignment.Right), 0.8f));
			var alt = Ui.Yatay(8);
			rv.AddChild(alt);
			alt.AddChild(Ui.Genis(MiniTakim(r.Takim, 84)));
			var od = Rakipler.ArenaOdulu(r, true);
			var sb = Ui.Dugme(Sunucu.Girisli && r.OyuncuId != null ? "Savaş" : $"Savaş\n+{od.kupa}", () => ArenaSavasi(r), new Color("c0582f"), 21, 110);
			sb.CustomMinimumSize = new Vector2(110, 110);
			sb.Disabled = p.ArenaHak <= 0;
			alt.AddChild(sb);
		}
		var alt2 = Ui.Yatay(10);
		v.AddChild(alt2);
		alt2.AddChild(Ui.Genis(Ui.Dugme("Rakipleri Yenile", () => { p.ArenaYenileme++; Kayit.Kaydet(); _rakipler = null; Yenile(); }, new Color("444c78"), 21)));
		alt2.AddChild(Ui.Genis(Ui.Dugme("Sıralamalar", () => { if (Sunucu.Girisli) CevrimiciSiralama("hafta"); else SiralamaPenceresi(); }, new Color("8a4fc0"), 21)));
		return Ui.Kaydirma(v);
	}

	void SiralamaPenceresi()
	{
		var v = Ui.Dikey(6);
		v.AddChild(Ui.Etiket("Haftalık Sıralama", 32, Ui.Altin, HorizontalAlignment.Center));
		var ks = Rakipler.SezonKalan();
		v.AddChild(Ui.Etiket($"50 kişilik lig grubu • sezon bitimine {ks.Days}g {ks.Hours}s", 18, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket("Sezon ödülü: 1. → 400  •  2-3. → 250  •  4-10. → 120  •  11-25. → 60  •  diğer → 25 elmas", 17, Ui.Elmas, HorizontalAlignment.Center));
		var liste = Ui.Dikey(4);
		var sira = Rakipler.Siralama();
		for (int i = 0; i < sira.Count; i++)
		{
			var (ad, kupa, oyuncu) = sira[i];
			var satir = Ui.Panel(oyuncu ? new Color("3b5a2f") : (i < 3 ? new Color("3a3320") : new Color("1b2038")), 8, 8);
			var h = Ui.Yatay();
			satir.AddChild(h);
			var no = Ui.Etiket($"{i + 1}.", 21, i < 3 ? Ui.Altin : Ui.Soluk);
			no.CustomMinimumSize = new Vector2(56, 0);
			h.AddChild(no);
			h.AddChild(Ui.Genis(Ui.Etiket(oyuncu ? ad + " (sen)" : ad, 21)));
			h.AddChild(Ui.Genis(Ui.Etiket($"{kupa}  {Rakipler.Lig(kupa)}", 19, Ui.Soluk, HorizontalAlignment.Right), 0.7f));
			liste.AddChild(satir);
		}
		var kay = Ui.Kaydirma(liste);
		kay.CustomMinimumSize = new Vector2(0, 760);
		v.AddChild(kay);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	// ================================================================ SAVAŞLAR
	void HikayeSavasi(int asama)
	{
		if (!Kayit.TakimHazir) { Mesaj("Önce takımına kart ekle"); Goster(Ekran.Takim); return; }
		if (!Kayit.EnerjiHarca(Kayit.HikayeEnerji)) { EnerjiPenceresi(); return; }
		Kayit.P.GunlukHikaye++;
		SezonKarti.XpEkle(SezonKarti.XpHikaye);
		var dusman = Rakipler.AsamaTakimi(asama);
		SavasBaslat(dusman, $"Aşama {asama}", Rakipler.BolumArka(asama), kazandi =>
		{
			var p = Kayit.P;
			bool ilk = asama > p.EnIyiAsama;
			var satirlar = new List<string>();
			if (kazandi)
			{
				var o = Rakipler.AsamaOdulu(asama, ilk);
				p.Altin += o.altin; p.Elmas += o.elmas; p.Parsomen += o.parsomen;
				if (ilk) p.EnIyiAsama = asama;
				_katlaAltin = o.altin;
				satirlar.Add($"+{o.altin} altın");
				if (o.elmas > 0) satirlar.Add($"+{o.elmas} elmas");
				if (o.parsomen > 0) satirlar.Add($"+{o.parsomen} parşömen");
				satirlar.Add($"+{Etkinlik.PuanEkle(Etkinlik.ZaferPuani(false))} etkinlik puanı ({Etkinlik.Ad})");
			}
			else satirlar.Add("İpucu: kartlarını yükselt, takımı yeniden diz veya element avantajını kullan (Ateş > Doğa > Su > Ateş).");
			satirlar.Add($"+{SezonKarti.XpHikaye} sezon puanı");
			Kayit.Kaydet();
			return satirlar;
		}, Rakipler.BossAsamasi(asama), Etkinlik.Mitoloji);
	}

	void ArenaSavasi(ArenaRakip r)
	{
		var p = Kayit.P;
		if (p.ArenaHak <= 0) { Mesaj("Savaş hakkın bitti"); return; }
		if (!Kayit.TakimHazir) { Mesaj("Önce takımına kart ekle"); Goster(Ekran.Takim); return; }
		p.ArenaHak--;
		p.ArenaGunlukSavas++;
		SezonKarti.XpEkle(SezonKarti.XpArena);
		SavasBaslat(r.Takim, $"Arena: {r.Ad}", Rakipler.TakimArka(r.Takim), async kazandi =>
		{
			var o = Rakipler.ArenaOdulu(r, kazandi);
			int kupaDegisim = o.kupa;
			if (Sunucu.Girisli)
			{
				try
				{
					var d = r.OyuncuId != null
						? await Sunucu.Sozluk("arena_sonuc", new() { ["p_rakip"] = r.OyuncuId, ["p_kazandi"] = kazandi })
						: await Sunucu.Sozluk("bot_arena_sonuc", new() { ["p_kazandi"] = kazandi, ["p_zorluk"] = r.Zorluk });
					kupaDegisim = Sunucu.I(d, "kupa_degisim");
					p.Kupa = Sunucu.I(d, "kupa");
				}
				catch (Exception e) { Mesaj(e.Message); kupaDegisim = 0; }
			}
			else p.Kupa = Math.Max(0, p.Kupa + o.kupa);
			o = (kupaDegisim, o.altin, o.elmas);
			p.Altin += o.altin; p.Elmas += o.elmas;
			int etPuan = kazandi ? Etkinlik.PuanEkle(Etkinlik.ZaferPuani(true)) : 0;
			if (kazandi) p.ArenaGalibiyet++; else p.ArenaMaglubiyet++;
			_rakipler = null;
			Kayit.Kaydet();
			var l = new List<string> { $"{(o.kupa >= 0 ? "+" : "")}{o.kupa} kupa", $"+{o.altin} altın" };
			if (o.elmas > 0) l.Add($"+{o.elmas} elmas");
			l.Add($"+{SezonKarti.XpArena} sezon puanı");
			if (etPuan > 0) l.Add($"+{etPuan} etkinlik puanı");
			return l;
		}, false, null);
	}

	int _katlaAltin;   // son hikâye zaferinin altını (reklamla 2 kat)

	void SavasBaslat(List<TakimUyesi> dusman, string baslik, string arka, Func<bool, List<string>> sonuc, bool boss = false, string bonusMit = null)
		=> SavasBaslat(dusman, baslik, arka, k => Task.FromResult(sonuc(k)), boss, null, bonusMit);

	void SavasBaslat(List<TakimUyesi> dusman, string baslik, string arka, Func<bool, Task<List<string>>> sonuc, bool boss, List<TakimUyesi> bizim, string bonusMit = null)
	{
		if (!Kayit.TakimHazir) { Mesaj("Önce takımına kart ekle"); Goster(Ekran.Takim); return; }
		var s = new Savas(bizim ?? Kayit.TakimUyeleri(), dusman, (ulong)Time.GetTicksUsec(), bonusMit, bonusMit != null ? Etkinlik.GucBonus : 0f);
		s.Oyna();
		Kayit.P.GunlukSavas++;
		_katlaAltin = 0;
		var ekran = new SavasEkrani { S = s, Baslik = baslik, Arka = arka, Boss = boss };
		ekran.Bitti = async kazandi =>
		{
			Ses.Muzik("menu");
			if (kazandi) Ses.Efekt("para", 0, 1, 0);
			List<string> satirlar;
			try { satirlar = await sonuc(kazandi); }
			catch (Exception e) { satirlar = new List<string> { e.Message }; }
			ekran.QueueFree();
			Yenile();
			SonucPenceresi(kazandi, satirlar);
		};
		AddChild(ekran);
	}

	void SonucPenceresi(bool kazandi, List<string> satirlar)
	{
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket(kazandi ? "ZAFER!" : "YENİLGİ", 52, kazandi ? Ui.Altin : Ui.Kirmizi, HorizontalAlignment.Center));
		foreach (var s in satirlar) v.AddChild(Ui.Etiket(s, 24, Colors.White, HorizontalAlignment.Center));
		int ek = _katlaAltin;
		if (kazandi && ek > 0 && Kayit.ReklamKalan(1) > 0)
		{
			v.AddChild(Ui.Dugme($"Reklam izle: altını 2 kat yap (+{ek} altın)  •  {Kayit.ReklamKalan(1)} hak", () =>
				Reklam.Odullu(() =>
				{
					if (!Kayit.ReklamOdulu(1)) return;
					Kayit.P.Altin += ek; _katlaAltin = 0; Kayit.Kaydet();
					Ses.Efekt("para"); PencereKapat(); Yenile(); Mesaj($"+{ek} altın");
				}), new Color("2f8a57"), 22));
		}
		int gk = Reklam.GeciseKalan;
		if (gk > 0 && gk <= 2) v.AddChild(Ui.Etiket(gk == 1 ? "Sonraki savaştan sonra reklam var — Mağaza'dan reklamları kaldırabilirsin." : $"{gk} savaş sonra reklam", 16, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Devam", () => { PencereKapat(); Reklam.SavasSonrasi(() => { }); }, new Color("3b6fd8"), 26));
		Pencere(v);
	}

	// ================================================================ ÖDÜL / MAĞAZA
	void GunlukOdulPenceresi()
	{
		var p = Kayit.P;
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket("Günlük Giriş Ödülü", 32, Ui.Altin, HorizontalAlignment.Center));
		var izgara = new GridContainer { Columns = 4 };
		izgara.AddThemeConstantOverride("h_separation", 8);
		izgara.AddThemeConstantOverride("v_separation", 8);
		for (int i = 0; i < 7; i++)
		{
			var o = Kayit.GirisOdulleri[i];
			bool bugun = i == p.GirisSerisi;
			bool gecti = i < p.GirisSerisi || (bugun && p.GirisOdulAlindi);
			var kut = Ui.Panel(bugun ? new Color("3b5a2f") : gecti ? new Color("252a3a") : Ui.PanelAcik, 8, 10);
			kut.CustomMinimumSize = new Vector2(140, 110);
			var kv = Ui.Dikey(2);
			kut.AddChild(kv);
			kv.AddChild(Ui.Etiket(o.ad, 18, Ui.Soluk, HorizontalAlignment.Center));
			string od = o.altin > 0 ? $"{o.altin} altın" : o.elmas > 0 ? $"{o.elmas} elmas" : $"{o.parsomen} parşömen";
			kv.AddChild(Ui.Etiket(od, 20, o.altin > 0 ? Ui.Altin : o.elmas > 0 ? Ui.Elmas : new Color("e2c79a"), HorizontalAlignment.Center));
			if (gecti) kv.AddChild(Ui.Etiket("Alındı", 16, Ui.Yesil, HorizontalAlignment.Center));
			izgara.AddChild(kut);
		}
		var c = new CenterContainer();
		c.AddChild(izgara);
		v.AddChild(c);
		var b = Ui.Dugme(p.GirisOdulAlindi ? "Yarın tekrar gel" : "Ödülü Al", () =>
		{
			if (Kayit.GirisOdulAl()) { Ses.Efekt("para"); PencereKapat(); Yenile(); Mesaj("Ödül alındı!"); }
		}, new Color("2f8a57"), 26);
		b.Disabled = p.GirisOdulAlindi;
		v.AddChild(b);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	void SatinAlmaPenceresi(string id, string ozet)
	{
		var u = Magaza.Bul(id);
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket("Teşekkürler!", 40, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket(u?.Ad ?? id, 26, Ui.Elmas, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket(ozet, 22, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Harika", PencereKapat, new Color("2f8a57"), 24));
		Pencere(v);
	}

	void MagazaPenceresi()
	{
		var p = Kayit.P;
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Mağaza", 34, Ui.Altin, HorizontalAlignment.Center));

		int rk = Kayit.ReklamKalan(0);
		var rb = Ui.Dugme(rk > 0 ? $"Reklam izle: +{Kayit.ReklamElmas} elmas  ({rk}/{Kayit.ReklamHak[0]} hak)" : "Bugünkü reklam hakların bitti", () =>
			Reklam.Odullu(() =>
			{
				if (Kayit.ReklamOdulu(0)) { Ses.Efekt("para"); PencereKapat(); Yenile(); MagazaPenceresi(); Mesaj($"+{Kayit.ReklamElmas} elmas"); }
			}), new Color("2f8a57"), 22);
		rb.Disabled = rk <= 0;
		v.AddChild(rb);
		var tb = Ui.Dugme($"1 parşömen  ←  {Kayit.AltinParsomenFiyat:N0} altın  (bugün {Kayit.TakasKalan} hak)", () =>
		{
			if (Kayit.AltinlaParsomen()) { Ses.Efekt("para"); PencereKapat(); Yenile(); MagazaPenceresi(); Mesaj("+1 parşömen"); }
			else Mesaj("Yeterli altın yok");
		}, new Color("8a6a2f"), 20);
		tb.Disabled = Kayit.TakasKalan <= 0 || p.Altin < Kayit.AltinParsomenFiyat;
		v.AddChild(tb);
		v.AddChild(Ui.Dugme("5.000 altın  ←  100 elmas", () =>
		{
			if (p.Elmas < 100) { Mesaj("Yeterli elmas yok"); return; }
			p.Elmas -= 100; p.Altin += 5000; Kayit.Kaydet(); UstGuncelle(); Mesaj("+5.000 altın");
		}, new Color("c0912f"), 22));
		v.AddChild(Ui.Dugme("1 parşömen  ←  100 elmas", () =>
		{
			if (p.Elmas < 100) { Mesaj("Yeterli elmas yok"); return; }
			p.Elmas -= 100; p.Parsomen++; Kayit.Kaydet(); UstGuncelle(); Mesaj("+1 parşömen");
		}, new Color("a0743a"), 22));

		// aylık kart günlük ödülü
		int kalan = Magaza.AylikKartKalan();
		if (kalan > 0)
		{
			var ab = Ui.Dugme(Magaza.AylikKartAlinabilir() ? $"Aylık kart: bugünkü {Magaza.AylikKartGunluk} elmasını al ({kalan} gün kaldı)" : $"Aylık kart: bugün alındı ({kalan} gün kaldı)", () =>
			{
				if (Magaza.AylikKartAl()) { Ses.Efekt("para"); PencereKapat(); Yenile(); MagazaPenceresi(); }
			}, new Color("2f6fb0"), 20);
			ab.Disabled = !Magaza.AylikKartAlinabilir();
			v.AddChild(ab);
		}

		v.AddChild(Ui.Etiket(Odeme.EklentiVar ? "Elmas paketleri (Google Play ile güvenli ödeme)" : "Elmas paketleri (satın alma Google Play sürümünde açılır)", 19, Ui.Soluk, HorizontalAlignment.Center));
		foreach (var u in Magaza.Urunler)
		{
			var pp = Ui.Panel(u.TekSeferlik || u.AylikKart ? new Color("3a2f55") : Ui.PanelAcik, 12);
			var h = Ui.Yatay();
			pp.AddChild(h);
			var t = Ui.Genis(Ui.Dikey(2));
			h.AddChild(t);
			bool bonus = Magaza.IlkAlimBonusu(u);
			t.AddChild(Ui.Etiket(u.Ad + (bonus ? "  •  İLK ALIM 2 KAT!" : ""), 22, bonus ? Ui.Altin : Ui.Elmas));
			t.AddChild(Ui.Etiket(u.Aciklama, 17, Ui.Soluk));
			bool alinabilir = Magaza.SatinAlinabilir(u);
			var id = u.Id;
			var b = Ui.Dugme(alinabilir ? Odeme.Fiyat(u) : (u.TekSeferlik ? "Alındı" : "Aktif"), () => Odeme.SatinAl(id), new Color("2f8a57"), 19, 56);
			b.Disabled = !alinabilir;
			b.CustomMinimumSize = new Vector2(140, 56);
			h.AddChild(b);
			v.AddChild(pp);
		}
		v.AddChild(Ui.Etiket("Fiyatlara vergiler dahildir. Satın alınan elmaslar iade edilmez; ödeme sorunları için Google Play destek.", 15, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	// ================================================================ pencere / mesaj
	void Pencere(Control icerik)
	{
		PencereKapat();
		var arka = new ColorRect { Color = new Color(0, 0, 0, 0.72f), MouseFilter = MouseFilterEnum.Stop };
		arka.SetAnchorsPreset(LayoutPreset.FullRect);
		_katman.AddChild(arka);
		var panel = Ui.Panel(new Color("1a1f3a"), 20, 20);
		panel.Position = new Vector2(24, 0);
		panel.CustomMinimumSize = new Vector2(672, 0);
		panel.Size = new Vector2(672, 0);
		icerik.CustomMinimumSize = new Vector2(632, 0);
		panel.AddChild(icerik);
		_katman.AddChild(panel);
		// içerik ölçüldükten sonra dikeyde ortala
		Callable.From(() =>
		{
			if (!IsInstanceValid(panel)) return;
			float yuk = Mathf.Min(panel.GetCombinedMinimumSize().Y, GetViewportRect().Size.Y - 40);
			panel.Size = new Vector2(672, yuk);
			panel.Position = new Vector2((GetViewportRect().Size.X - 672) / 2, (GetViewportRect().Size.Y - yuk) / 2);
		}).CallDeferred();
	}

	void PencereKapat()
	{
		foreach (var c in _katman.GetChildren()) { _katman.RemoveChild(c); c.QueueFree(); }
	}

	void Mesaj(string metin)
	{
		var l = Ui.Etiket(metin, 26, Colors.White, HorizontalAlignment.Center);
		var p = Ui.Panel(new Color(0, 0, 0, 0.85f), 14, 14);
		p.AddChild(l);
		p.MouseFilter = MouseFilterEnum.Ignore;
		l.MouseFilter = MouseFilterEnum.Ignore;
		p.Position = new Vector2(60, 1040);
		p.Size = new Vector2(600, 0);
		p.CustomMinimumSize = new Vector2(600, 0);
		AddChild(p);
		var tw = p.CreateTween();
		tw.TweenInterval(1.4f);
		tw.TweenProperty(p, "modulate:a", 0f, 0.4f);
		tw.TweenCallback(Callable.From(p.QueueFree));
	}

	// ================================================================ önizleme için erişim
	public void OnizlemeEkran(int i) => Goster((Ekran)i);
	public void OnizlemeDetay(string id) => KartDetayi(Veri.Harita[id], null);
	public void OnizlemeHikaye(string id) => KartHikaye(Veri.Harita[id], null);
	public void OnizlemeCagir() { var once = Kayit.P.Kartlar.Select(k => k.Id).ToHashSet(); Kayit.P.Elmas += 2700; CagirSonucu(Kayit.Cagir("elmas", 10), once); }
	public void OnizlemeKapat() => PencereKapat();
	public void OnizlemeGunluk() => GunlukOdulPenceresi();
	public void OnizlemeMagaza() => MagazaPenceresi();
	public void OnizlemeKule() => KulePenceresi();
	public void OnizlemeSezon() => SezonPenceresi();
	public void OnizlemeEtkinlik() => EtkinlikPenceresi();
	public void OnizlemeTakimKart(string id) { var s = Kayit.Sahip(id); if (s != null) TakimKartPenceresi(s); }
	public void OnizlemeEtkinlikMagaza() => EtkinlikMagazasi();
	public void OnizlemeMuhafiz() => MuhafizPenceresi();
	public void OnizlemeKoleksiyon() => KoleksiyonPenceresi();
	public void OnizlemeDizilis() => DizilisPenceresi();
	public void OnizlemeKutlama(string id) => KartDetayi(Veri.Harita[id], null, "Seviye 12!");
	public SavasEkrani OnizlemeSavas(int asama)
	{
		HikayeSavasi(asama);
		return GetChildren().OfType<SavasEkrani>().LastOrDefault();
	}
}
