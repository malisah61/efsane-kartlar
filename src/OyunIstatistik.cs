using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Savaş sonu istatistikleri, ilk açılış hikâye girişi ve arena savunma dizilişi (Oyun partial, 2026-11-25).</summary>
public partial class Oyun
{
	Savas _sonSavas;   // son oynanan savaş (istatistik penceresi için)

	// ================================================================ SAVAŞ İSTATİSTİKLERİ
	/// <summary>Bir birimin savaştaki katkı puanı (MVP seçimi): hasar + iyileştirme + kalkanın yarısı + alınan hasarın üçte biri.</summary>
	static float KatkiPuani(Birim b) => b.IstHasar + b.IstIyiles + 0.5f * b.IstKalkan + 0.33f * b.IstAlinan;

	void IstatistikPenceresi()
	{
		var s = _sonSavas;
		if (s == null) return;
		var v = Ui.Dikey(6);
		v.AddChild(Ui.Etiket("Savaş İstatistikleri", 32, Ui.Altin, HorizontalAlignment.Center));
		float H(int t) => s.Birimler.Where(b => b.Taraf == t).Sum(b => b.IstHasar);
		v.AddChild(Ui.Etiket($"{Math.Min(s.Tur, Savas.MaxTur)} tur  •  toplam hasar: sen {Ui.Sayi((int)H(0))} – rakip {Ui.Sayi((int)H(1))}", 18, Ui.Soluk, HorizontalAlignment.Center));

		var lejant = Ui.Yatay(14);
		lejant.Alignment = BoxContainer.AlignmentMode.Center;
		foreach (var (ad, renk) in new[] { ("Verilen hasar", IstRenk[0]), ("İyileştirme + kalkan", IstRenk[1]), ("Alınan hasar", IstRenk[2]) })
		{
			var e = Ui.Etiket("■ " + ad, 16, renk);
			e.AutowrapMode = TextServer.AutowrapMode.Off;
			lejant.AddChild(e);
		}
		v.AddChild(lejant);

		float enHasar = Math.Max(1, s.Birimler.Max(b => b.IstHasar));
		float enDestek = Math.Max(1, s.Birimler.Max(b => b.IstIyiles + b.IstKalkan));
		float enAlinan = Math.Max(1, s.Birimler.Max(b => b.IstAlinan));
		var mvp = s.Birimler.Where(b => b.Taraf == (s.Kazanan == 1 ? 1 : 0)).OrderByDescending(KatkiPuani).FirstOrDefault();

		for (int taraf = 0; taraf < 2; taraf++)
		{
			bool kazanan = s.Kazanan == taraf;
			v.AddChild(Ui.Etiket(taraf == 0 ? "Senin Takımın" + (kazanan ? "  — Zafer" : "") : "Rakip" + (kazanan ? "  — Zafer" : ""), 21,
				taraf == 0 ? Ui.Elmas : Ui.Kirmizi));
			foreach (var b in s.Birimler.Where(b => b.Taraf == taraf).OrderBy(b => b.Konum))
			{
				var satir = Ui.Panel(b == mvp ? new Color("3a3320") : Ui.PanelRenk, 6, 10);
				var h = Ui.Yatay(8);
				satir.AddChild(h);
				var port = new TextureRect
				{
					Texture = Veri.Portre(b.T.Id), CustomMinimumSize = new Vector2(58, 58),
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					TextureFilter = TextureFilterEnum.Nearest,
				};
				if (!b.Canli) port.Modulate = new Color(0.55f, 0.55f, 0.6f);
				h.AddChild(port);
				var sag = Ui.Genis(Ui.Dikey(3));
				h.AddChild(sag);
				var ust = Ui.Yatay(6);
				sag.AddChild(ust);
				var ad = Ui.Etiket($"{b.T.Ad}  Sv {b.Seviye}", 18, Veri.NadirlikRenk(b.T.Nadirlik));
				ad.AutowrapMode = TextServer.AutowrapMode.Off;
				ad.ClipText = true;
				ust.AddChild(Ui.Genis(ad));
				string ek = (b.IstOldurme > 0 ? $"{b.IstOldurme} alt etme" : "") + (!b.Canli ? (b.IstOldurme > 0 ? "  •  " : "") + "düştü" : "");
				if (ek != "")
				{
					var el = Ui.Etiket(ek, 15, b.Canli ? Ui.Soluk : Ui.Kirmizi, HorizontalAlignment.Right);
					el.AutowrapMode = TextServer.AutowrapMode.Off;
					ust.AddChild(el);
				}
				if (b == mvp)
				{
					var m = Ui.Etiket("MVP", 17, Ui.Altin);
					m.AutowrapMode = TextServer.AutowrapMode.Off;
					ust.AddChild(m);
				}
				var cub = Ui.Yatay(6);
				sag.AddChild(cub);
				cub.AddChild(Ui.Genis(new IstCubuk(b.IstHasar / enHasar, IstRenk[0], Ui.Sayi((int)b.IstHasar))));
				cub.AddChild(Ui.Genis(new IstCubuk((b.IstIyiles + b.IstKalkan) / enDestek, IstRenk[1], Ui.Sayi((int)(b.IstIyiles + b.IstKalkan)))));
				cub.AddChild(Ui.Genis(new IstCubuk(b.IstAlinan / enAlinan, IstRenk[2], Ui.Sayi((int)b.IstAlinan))));
				v.AddChild(satir);
			}
		}
		if (mvp != null)
			v.AddChild(Ui.Etiket($"Savaşın yıldızı: {mvp.T.Ad} ({Veri.SinifAd(mvp.T.Sinif)})", 19, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22, 58));
		Pencere(v);
	}

	static readonly Color[] IstRenk = { new("e0703a"), new("4cc76a"), new("7f8bb8") };

	// ================================================================ HİKÂYE GİRİŞİ (ilk açılış)
	static readonly (string baslik, string arka, string[] kartlar, string metin)[] GirisSayfalari =
	{
		("Dünya Ağacı", "turk", new[] { "ulgen", "zeus", "odin", "ra" },
			"Evvel zaman içinde Dünya Ağacı'nın kökleri bütün diyarları birbirine bağlardı. Bozkırdan Olimpos'a, Asgard'dan Nil kıyısına, Takamagahara'dan Gök Saray'a kadar tanrılar ve kahramanlar gücünü bu köklerden alırdı."),
		("Kaos'un Gölgesi", "iskandinav", new[] { "tepegoz", "fenrir", "kerberos", "oni" },
			"Bir gün Kaos'un Gölgesi köklere sızdı. Efsaneler birbirine karıştı, kahramanlar diyar diyar dağıldı; Tepegöz'den Fenrir'e nice canavar yollara düştü. Uzaktan Ragnarök'ün davulları duyulmaya başladı."),
		("Ozanın Çağrısı", "turk", new[] { "dede_korkut" },
			"Ben Dede Korkut'um. Kopuzumun her teli bir kahramanın adını bilir. Ezgimi duyan ruhlar kartlara bağlanır; onları sen çağıracak, sen yükselteceksin, Efsane Bekçisi."),
		("Yol Başlıyor", "turk", new[] { "bahadir", "akinci", "medusa", "siren", "kam" },
			"Takımını kur: önde dayanıklı tanklar, arkada büyücüler ve destekler. Ateş Doğa'yı, Doğa Su'yu, Su Ateş'i yener. Aynı mitolojiden kahramanlar ve efsanede bağı olanlar birlikte daha güçlüdür. İlk adımın Bozkır Yolu'nda!"),
	};

	void GirisHikayesi(int sayfa = 0)
	{
		PencereKapat();
		var (baslik, arka, kartlar, metin) = GirisSayfalari[sayfa];
		var kok = new Control { MouseFilter = MouseFilterEnum.Stop };
		kok.SetAnchorsPreset(LayoutPreset.FullRect);
		_katman.AddChild(kok);
		var zemin = new ColorRect { Color = new Color("070912") };
		zemin.SetAnchorsPreset(LayoutPreset.FullRect);
		kok.AddChild(zemin);
		var yol = $"res://assets/arka/{arka}.png";
		if (ResourceLoader.Exists(yol))
		{
			var resim = new TextureRect
			{
				Texture = GD.Load<Texture2D>(yol), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
				StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, TextureFilter = TextureFilterEnum.Nearest,
				Modulate = new Color(0.55f, 0.55f, 0.7f),
			};
			resim.SetAnchorsPreset(LayoutPreset.FullRect);
			kok.AddChild(resim);
		}
		var karart = new ColorRect { Color = new Color(0, 0, 0.05f, 0.45f) };
		karart.SetAnchorsPreset(LayoutPreset.FullRect);
		kok.AddChild(karart);

		var v = Ui.Dikey(18);
		v.SetAnchorsPreset(LayoutPreset.FullRect);
		v.OffsetLeft = 28; v.OffsetRight = -28; v.OffsetTop = 70; v.OffsetBottom = -50;
		kok.AddChild(v);
		v.AddChild(Ui.Etiket($"{sayfa + 1} / {GirisSayfalari.Length}", 18, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket(baslik, 46, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Bosluk(20));

		var sira = Ui.Yatay(10);
		sira.Alignment = BoxContainer.AlignmentMode.Center;
		float g = kartlar.Length == 1 ? 300 : kartlar.Length >= 5 ? 120 : 150;
		for (int i = 0; i < kartlar.Length; i++)
		{
			var pr = new TextureRect
			{
				Texture = Veri.Portre(kartlar[i]), CustomMinimumSize = new Vector2(g, g),
				ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
				TextureFilter = TextureFilterEnum.Nearest, Modulate = new Color(1, 1, 1, 0),
			};
			var cer = Ui.Panel(new Color("1a1f3a"), 4, 10);
			cer.AddThemeStyleboxOverride("panel", Ui.Kutu(new Color("1a1f3a"), 10, Ui.Altin.Darkened(0.3f), 3, 4));
			cer.AddChild(pr);
			sira.AddChild(cer);
			var tw = pr.CreateTween();
			tw.TweenInterval(0.15f + 0.18f * i);
			tw.TweenProperty(pr, "modulate:a", 1f, 0.45f);
		}
		v.AddChild(sira);
		v.AddChild(Ui.Bosluk(16));

		var kutu = Ui.Panel(new Color(0.06f, 0.07f, 0.15f, 0.88f), 22, 18);
		var yazi = Ui.Etiket(metin, 26, Colors.White, HorizontalAlignment.Center);
		yazi.VisibleRatio = 0;
		kutu.AddChild(yazi);
		v.AddChild(kutu);
		var ytw = yazi.CreateTween();
		ytw.TweenInterval(0.3f);
		ytw.TweenProperty(yazi, "visible_ratio", 1f, Math.Clamp(metin.Length / 70f, 1.2f, 3.2f));

		v.AddChild(Ui.Uzun(new Control()));
		var alt = Ui.Yatay(14);
		v.AddChild(alt);
		bool son = sayfa == GirisSayfalari.Length - 1;
		if (!son) alt.AddChild(Ui.Genis(Ui.Dugme("Atla", GirisBitir, new Color("444c6a"), 22, 70), 0.6f));
		alt.AddChild(Ui.Genis(Ui.Dugme(son ? "Maceraya Başla" : "İleri", () =>
		{
			if (yazi.VisibleRatio < 1f) { ytw.Kill(); yazi.VisibleRatio = 1f; return; }   // önce yazıyı tamamla
			if (son) GirisBitir(); else GirisHikayesi(sayfa + 1);
		}, son ? new Color("c0582f") : new Color("3478c8"), 26, 70)));
	}

	void GirisBitir()
	{
		Kayit.P.GirisHikayesi = true;
		Kayit.Kaydet();
		PencereKapat();
		if (!Kayit.KayitKapali && !Kayit.P.GirisOdulAlindi) GunlukOdulPenceresi();
	}

	// ================================================================ ARENA SAVUNMASI
	/// <summary>Arena savunma takımı seçimi: güncel takım ya da kayıtlı dizilişlerden biri.</summary>
	Control SavunmaSecici()
	{
		var h = Ui.Yatay(8);
		var e = Ui.Etiket("Savunma:", 19, Ui.Soluk);
		e.AutowrapMode = TextServer.AutowrapMode.Off;
		e.VerticalAlignment = VerticalAlignment.Center;
		h.AddChild(e);
		int secili = Kayit.SavunmaAyri ? Kayit.P.SavunmaDizilis : -1;
		for (int i = -1; i < Kayit.DizilisSayisi; i++)
		{
			int di = i;
			bool bos = i >= 0 && Kayit.DizilisBos(i);
			bool sec = secili == i;
			string ad = i < 0 ? "Takımım" : bos ? $"{i + 1} (boş)" : $"{i + 1}. diziliş";
			var b = Ui.Genis(Ui.Dugme(ad, () =>
			{
				Kayit.P.SavunmaDizilis = di;
				Kayit.Kaydet(); Yenile();
				Mesaj(di < 0 ? "Savunman artık güncel takımın" : $"Savunman artık Diziliş {di + 1}");
			}, sec ? new Color("2f8a57") : new Color("6a4a8a"), 17, 44));
			b.Disabled = bos || sec;
			h.AddChild(b);
		}
		return h;
	}

	public void OnizlemeIstatistik() => IstatistikPenceresi();
	public void OnizlemeGiris(int sayfa) => GirisHikayesi(sayfa);
}

/// <summary>İstatistik çubuğu: oranı kadar dolu, değer üstünde yazılı.</summary>
public partial class IstCubuk : Control
{
	readonly float _oran; readonly Color _renk; readonly string _metin;
	public IstCubuk() { }
	public IstCubuk(float oran, Color renk, string metin)
	{
		_oran = Mathf.Clamp(oran, 0, 1); _renk = renk; _metin = metin;
		CustomMinimumSize = new Vector2(0, 26);
		MouseFilter = MouseFilterEnum.Ignore;
	}
	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), new Color("0e1226"));
		if (_oran > 0) DrawRect(new Rect2(0, 0, Mathf.Max(3, Size.X * _oran), Size.Y), _renk.Darkened(0.15f));
		var f = Ui.Yazi;
		const int boyut = 16;
		var olcu = f.GetStringSize(_metin, HorizontalAlignment.Left, -1, boyut);
		var poz = new Vector2((Size.X - olcu.X) / 2, (Size.Y - olcu.Y) / 2 + f.GetAscent(boyut));
		DrawStringOutline(f, poz, _metin, HorizontalAlignment.Left, -1, boyut, 3, new Color(0, 0, 0, 0.8f));
		DrawString(f, poz, _metin, HorizontalAlignment.Left, -1, boyut, Colors.White);
	}
}
