using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Oyun ekranlarının ek bölümleri: menü rozetleri, hızlı geçiş, Efsaneler Kulesi, Efsane Yolu (sezon kartı).</summary>
public partial class Oyun
{
	// ================================================================ menü rozetleri
	static Control Rozet(Button b)
	{
		var rz = new Panel { MouseFilter = MouseFilterEnum.Ignore, Visible = false };
		rz.AddThemeStyleboxOverride("panel", Ui.Kutu(Ui.Kirmizi, 9, new Color("ffffff"), 2, 0));
		rz.AnchorLeft = 1; rz.AnchorRight = 1; rz.AnchorTop = 0; rz.AnchorBottom = 0;
		rz.OffsetLeft = -24; rz.OffsetRight = -6; rz.OffsetTop = 6; rz.OffsetBottom = 24;
		b.AddChild(rz);
		return rz;
	}

	/// <summary>Yapılacak bir şey olan sekmelerde kırmızı nokta gösterir.</summary>
	void RozetleriGuncelle()
	{
		if (_rozetler.Count < 6) return;
		var p = Kayit.P;
		bool gorev = Enumerable.Range(0, Kayit.Gorevler.Length).Any(i => Kayit.GorevTamam(i) && !p.AlinanGorevler.Contains(i));
		_rozetler[0].Visible = !p.GirisOdulAlindi || gorev || SezonKarti.AlinabilirSayi() > 0 || Magaza.AylikKartAlinabilir();
		_rozetler[1].Visible = p.Kartlar.Any(s => s.Yildiz < Kayit.MaxYildiz && s.Kopya >= Kayit.YildizKopya(s.Yildiz) && p.Altin >= Kayit.YildizAltin(s.Yildiz));
		_rozetler[2].Visible = p.Takim.Contains(null) && p.Kartlar.Count > p.Takim.Count(x => x != null);
		_rozetler[3].Visible = p.Parsomen > 0 || Kayit.ReklamKalan(3) > 0;
		_rozetler[4].Visible = p.ArenaHak > 0 && Kayit.TakimHazir;
		_rozetler[5].Visible = false;
	}

	// ================================================================ hızlı geçiş (süpürme)
	Control HizliGecSatiri(int asama)
	{
		var h = Ui.Yatay(10);
		int e = Kayit.HikayeEnerji;
		h.AddChild(Ui.Genis(Ui.Dugme($"Hızlı Geç x1  ({e} enerji)", () => HizliGec(asama, 1), new Color("3f5a3a"), 18, 50)));
		h.AddChild(Ui.Genis(Ui.Dugme($"Hızlı Geç x5  ({e * 5} enerji)", () => HizliGec(asama, 5), new Color("3f5a3a"), 18, 50)));
		return h;
	}

	/// <summary>Geçilmiş aşamayı savaşmadan tekrar ödülüyle geçer (aynı enerji, aynı tekrar ödülü).</summary>
	void HizliGec(int asama, int adet)
	{
		var p = Kayit.P;
		int yapilan = 0, altin = 0;
		for (int i = 0; i < adet; i++)
		{
			if (!Kayit.EnerjiHarca(Kayit.HikayeEnerji)) break;
			var o = Rakipler.AsamaOdulu(asama, false);
			p.Altin += o.altin; altin += o.altin;
			p.GunlukHikaye++; p.GunlukSavas++;
			SezonKarti.XpEkle(SezonKarti.XpHikaye);
			yapilan++;
		}
		if (yapilan == 0) { EnerjiPenceresi(); return; }
		Kayit.Kaydet();
		Ses.Efekt("para");
		Yenile();
		Mesaj($"Aşama {asama} x{yapilan}: +{altin:N0} altın, +{yapilan * SezonKarti.XpHikaye} sezon puanı");
	}

	// ================================================================ EFSANELER KULESİ
	void KulePenceresi()
	{
		var p = Kayit.P;
		int kat = Kule.SiradakiKat;
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Efsaneler Kulesi", 36, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Geçilen kat: {p.KuleKat}/{Kule.KatSayisi}   •   Enerji harcamaz, ödül ilk geçişte verilir", 18, Ui.Soluk, HorizontalAlignment.Center));
		var bar = Cubuk(p.KuleKat, Kule.KatSayisi, new Color("9a6ad8"));
		v.AddChild(bar);
		if (Kule.Bitti)
		{
			v.AddChild(Ui.Etiket("Kulenin zirvesine ulaştın! Yeni katlar yakında.", 24, Ui.Altin, HorizontalAlignment.Center));
		}
		else
		{
			bool boss = Kule.BossKati(kat);
			var kp = Ui.Panel(boss ? new Color("3a1f2a") : Ui.PanelRenk, 14);
			v.AddChild(kp);
			var kv = Ui.Dikey(8);
			kp.AddChild(kv);
			kv.AddChild(Ui.Etiket($"{kat}. Kat — {Kule.KatBolum(kat)}" + (boss ? "  (BOSS)" : ""), 26, boss ? Ui.Kirmizi : Colors.White, HorizontalAlignment.Center));
			var dusman = Kule.KatTakimi(kat);
			kv.AddChild(MiniTakim(dusman, 104));
			int dg = Kayit.TakimGucu(dusman), bg = Kayit.TakimGucu();
			kv.AddChild(Ui.Etiket($"Düşman gücü {dg:N0}   •   Senin gücün {bg:N0}", 19, bg >= dg ? Ui.Yesil : Ui.Soluk, HorizontalAlignment.Center));
			kv.AddChild(Ui.Etiket($"Ödül: {Kule.OdulYazi(Kule.KatOdulu(kat))}, {SezonKarti.XpKule} sezon puanı", 19, Ui.Altin, HorizontalAlignment.Center));
			kv.AddChild(Ui.Dugme($"{kat}. Kata Saldır", () => KuleSavasi(kat), new Color("c0582f"), 26, 72));
			int sb = ((kat - 1) / 10 + 1) * 10;
			if (sb != kat) v.AddChild(Ui.Etiket($"Sıradaki boss: {sb}. kat  —  {Kule.OdulYazi(Kule.KatOdulu(sb))}", 18, Ui.Elmas, HorizontalAlignment.Center));
			v.AddChild(Ui.Etiket($"Her 10 kat bir mitolojinin katlarıdır; düşmanlar o mitolojinin uyum bonusunu alır. Element üstünlüğüyle karşı takım kur!", 16, Ui.Soluk, HorizontalAlignment.Center));
		}
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	void KuleSavasi(int kat)
	{
		if (!Kayit.TakimHazir) { PencereKapat(); Mesaj("Önce takımına kart ekle"); Goster(Ekran.Takim); return; }
		PencereKapat();
		SavasBaslat(Kule.KatTakimi(kat), $"Kule — {kat}. Kat", Kule.KatMitoloji(kat), k => Kule.Sonuc(kat, k), Kule.BossKati(kat));
	}

	static Control Cubuk(float deger, float max, Color renk, float yukseklik = 18)
	{
		var bar = new ProgressBar { MinValue = 0, MaxValue = Math.Max(1, max), Value = deger, ShowPercentage = false, CustomMinimumSize = new Vector2(0, yukseklik) };
		bar.AddThemeStyleboxOverride("background", Ui.Kutu(new Color("2a3052"), 8, null, 0, 0));
		bar.AddThemeStyleboxOverride("fill", Ui.Kutu(renk, 8, null, 0, 0));
		return bar;
	}

	// ================================================================ EFSANE YOLU (sezon kartı)
	void SezonPenceresi()
	{
		SezonKarti.Kontrol();
		int sv = SezonKarti.Seviye;
		var v = Ui.Dikey(8);
		v.AddChild(Ui.Etiket("Efsane Yolu", 36, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"{SezonKarti.AyAdi()} sezonu   •   bitimine {SezonKarti.KalanGun()} gün", 19, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket(sv >= SezonKarti.SeviyeSayisi ? $"Seviye {sv}/{SezonKarti.SeviyeSayisi} — yolun sonuna ulaştın!" : $"Seviye {sv}/{SezonKarti.SeviyeSayisi}   •   sonraki seviye: {SezonKarti.SeviyeIciXp}/{SezonKarti.SeviyeXp} puan", 22, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Cubuk(SezonKarti.SeviyeIciXp, SezonKarti.SeviyeXp, Ui.Altin));
		v.AddChild(Ui.Etiket($"Puan: hikâye savaşı +{SezonKarti.XpHikaye}  •  arena +{SezonKarti.XpArena}  •  yeni kule katı +{SezonKarti.XpKule}  •  günlük görev +{SezonKarti.XpGorev}", 16, Ui.Soluk, HorizontalAlignment.Center));
		if (!SezonKarti.Premium)
		{
			var u = Magaza.Bul(SezonKarti.UrunId);
			var t = SezonKarti.Toplam(true);
			v.AddChild(Ui.Dugme($"Altın Yol'u Aç  —  {Odeme.Fiyat(u)}", () => Odeme.SatinAl(SezonKarti.UrunId), new Color("8a4fc0"), 22, 60));
			v.AddChild(Ui.Etiket($"Altın Yol: {t.Elmas:N0} elmas, {t.Parsomen} parşömen, {t.Oz} ruh özü, {t.Altin:N0} altın ve epik kahraman. Ulaştığın seviyelerin ödülleri hemen alınabilir.", 16, Ui.Elmas, HorizontalAlignment.Center));
		}
		else v.AddChild(Ui.Etiket("Altın Yol bu ay açık", 20, Ui.Altin, HorizontalAlignment.Center));

		var bas = Ui.Yatay(8);
		var bl = Ui.Etiket("Sv", 17, Ui.Soluk, HorizontalAlignment.Center); bl.CustomMinimumSize = new Vector2(44, 0);
		bas.AddChild(bl);
		bas.AddChild(Ui.Genis(Ui.Etiket("Ücretsiz Yol", 18, Colors.White, HorizontalAlignment.Center)));
		bas.AddChild(Ui.Genis(Ui.Etiket("Altın Yol", 18, Ui.Altin, HorizontalAlignment.Center)));
		v.AddChild(bas);

		var liste = Ui.Dikey(6);
		for (int i = 1; i <= SezonKarti.SeviyeSayisi; i++)
		{
			var satir = Ui.Yatay(8);
			var no = Ui.Etiket(i.ToString(), 22, i <= sv ? Ui.Altin : Ui.Soluk, HorizontalAlignment.Center);
			no.CustomMinimumSize = new Vector2(44, 0);
			no.VerticalAlignment = VerticalAlignment.Center;
			no.SizeFlagsVertical = Control.SizeFlags.Fill;
			satir.AddChild(no);
			satir.AddChild(Ui.Genis(OdulHucre(i, false)));
			satir.AddChild(Ui.Genis(OdulHucre(i, true)));
			liste.AddChild(satir);
		}
		var kay = Ui.Kaydirma(liste);
		kay.CustomMinimumSize = new Vector2(0, 560);
		v.AddChild(kay);
		int hedef = Math.Max(0, Math.Min(sv, SezonKarti.SeviyeSayisi) - 2);
		Callable.From(() =>
		{
			if (!IsInstanceValid(kay) || hedef >= liste.GetChildCount()) return;
			kay.ScrollVertical = (int)((Control)liste.GetChild(hedef)).Position.Y;
		}).CallDeferred();

		int n = SezonKarti.AlinabilirSayi();
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		var hb = Ui.Genis(Ui.Dugme(n > 0 ? $"Hepsini Al ({n})" : "Alınacak ödül yok", () =>
		{
			var l = SezonKarti.HepsiniAl();
			if (l.Count == 0) return;
			Ses.Efekt("para"); PencereKapat(); Yenile(); SezonPenceresi();
			Mesaj($"{l.Count} ödül alındı!");
		}, new Color("2f8a57"), 22));
		hb.Disabled = n == 0;
		alt.AddChild(hb);
		alt.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22)));
		Pencere(v);
	}

	Control OdulHucre(int sv, bool altinYol)
	{
		var o = altinYol ? SezonKarti.AltinYol(sv) : SezonKarti.Ucretsiz(sv);
		bool alindi = SezonKarti.Alindi(sv, altinYol);
		bool acik = sv <= SezonKarti.Seviye;
		var p = Ui.Panel(altinYol ? new Color("3a2f55") : Ui.PanelAcik, 8, 10);
		p.CustomMinimumSize = new Vector2(0, 56);
		var h = Ui.Yatay(6);
		p.AddChild(h);
		var et = Ui.Genis(Ui.Etiket(o.ToString(), 16, alindi ? Ui.Soluk : acik ? Colors.White : new Color("c0c6e0"), HorizontalAlignment.Left));
		et.VerticalAlignment = VerticalAlignment.Center;
		et.SizeFlagsVertical = Control.SizeFlags.Fill;
		h.AddChild(et);
		if (SezonKarti.Alinabilir(sv, altinYol))
		{
			var b = Ui.Dugme("Al", () =>
			{
				var s = SezonKarti.Al(sv, altinYol);
				if (s == null) return;
				Kayit.Kaydet(); Ses.Efekt("para"); PencereKapat(); Yenile(); SezonPenceresi(); Mesaj("+" + s);
			}, Ui.Yesil.Darkened(0.2f), 17, 40);
			b.CustomMinimumSize = new Vector2(60, 40);
			h.AddChild(b);
		}
		else
		{
			string d = alindi ? "Alındı" : altinYol && !SezonKarti.Premium ? "Kilitli" : "";
			if (d != "")
			{
				var dl = Ui.Etiket(d, 14, alindi ? Ui.Yesil : Ui.Soluk, HorizontalAlignment.Center);
				dl.CustomMinimumSize = new Vector2(60, 0);
				dl.VerticalAlignment = VerticalAlignment.Center;
				dl.SizeFlagsVertical = Control.SizeFlags.Fill;
				h.AddChild(dl);
			}
		}
		return p;
	}
}
