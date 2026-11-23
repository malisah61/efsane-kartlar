using System;
using Godot;

namespace EfsaneKartlar;

/// <summary>Koddan arayüz kurmak için kısa yardımcılar.</summary>
public static class Ui
{
	public static readonly Color Arkaplan = new("12162a");
	public static readonly Color PanelRenk = new("1d2340");
	public static readonly Color PanelAcik = new("2a3259");
	public static readonly Color Altin = new("f2c14e");
	public static readonly Color Elmas = new("5fd3f3");
	public static readonly Color Yesil = new("4cc76a");
	public static readonly Color Kirmizi = new("e5534b");
	public static readonly Color Soluk = new("9aa3c7");
	public static Font Yazi => ThemeDB.FallbackFont;

	public static StyleBoxFlat Kutu(Color renk, int kose = 12, Color? kenar = null, int kenarKalinlik = 0, int ic = 10)
	{
		var s = new StyleBoxFlat { BgColor = renk };
		s.SetCornerRadiusAll(kose);
		s.SetContentMarginAll(ic);
		if (kenar.HasValue) { s.BorderColor = kenar.Value; s.SetBorderWidthAll(kenarKalinlik); }
		return s;
	}

	public static Label Etiket(string metin, int boyut = 24, Color? renk = null, HorizontalAlignment hiza = HorizontalAlignment.Left)
	{
		var l = new Label { Text = metin, HorizontalAlignment = hiza, AutowrapMode = TextServer.AutowrapMode.WordSmart };
		l.AddThemeFontSizeOverride("font_size", boyut);
		l.AddThemeColorOverride("font_color", renk ?? Colors.White);
		l.AddThemeColorOverride("font_outline_color", new Color(0, 0, 0, 0.6f));
		l.AddThemeConstantOverride("outline_size", boyut >= 28 ? 4 : 0);
		return l;
	}

	public static Button Dugme(string metin, Action tik, Color? renk = null, int boyut = 24, int yukseklik = 64)
	{
		var b = new Button { Text = metin, CustomMinimumSize = new Vector2(0, yukseklik), FocusMode = Control.FocusModeEnum.None };
		var c = renk ?? new Color("3b6fd8");
		b.AddThemeStyleboxOverride("normal", Kutu(c, 14));
		b.AddThemeStyleboxOverride("hover", Kutu(c.Lightened(0.12f), 14));
		b.AddThemeStyleboxOverride("pressed", Kutu(c.Darkened(0.2f), 14));
		b.AddThemeStyleboxOverride("disabled", Kutu(new Color("3a3f55"), 14));
		b.AddThemeFontSizeOverride("font_size", boyut);
		b.AddThemeColorOverride("font_color", Colors.White);
		b.AddThemeColorOverride("font_disabled_color", new Color("8088a0"));
		b.Pressed += () => Ses.Efekt("tik", -6, 1, 0.03f);
		if (tik != null) b.Pressed += tik;
		return b;
	}

	public static PanelContainer Panel(Color? renk = null, int ic = 14, int kose = 16)
	{
		var p = new PanelContainer();
		p.AddThemeStyleboxOverride("panel", Kutu(renk ?? PanelRenk, kose, null, 0, ic));
		return p;
	}

	public static VBoxContainer Dikey(int ara = 10) { var v = new VBoxContainer(); v.AddThemeConstantOverride("separation", ara); return v; }
	public static HBoxContainer Yatay(int ara = 10) { var h = new HBoxContainer(); h.AddThemeConstantOverride("separation", ara); return h; }
	public static Control Bosluk(float y, float x = 0) => new Control { CustomMinimumSize = new Vector2(x, y) };

	public static T Genis<T>(T c, float oran = 1) where T : Control
	{
		c.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		c.SizeFlagsStretchRatio = oran;
		return c;
	}

	public static T Uzun<T>(T c) where T : Control { c.SizeFlagsVertical = Control.SizeFlags.ExpandFill; return c; }

	public static ScrollContainer Kaydirma(Control icerik)
	{
		var s = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
		s.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		icerik.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		s.AddChild(icerik);
		return s;
	}

	public static string Sayi(int n) => n >= 1_000_000 ? $"{n / 1_000_000f:0.#}M" : n >= 10_000 ? $"{n / 1000f:0.#}B" : n.ToString("N0");

	/// <summary>Beş köşeli yıldız çizer.</summary>
	public static void Yildiz(CanvasItem c, Vector2 merkez, float r, Color renk)
	{
		var p = new Vector2[10];
		for (int i = 0; i < 10; i++)
		{
			float a = -Mathf.Pi / 2 + i * Mathf.Pi / 5;
			float rr = i % 2 == 0 ? r : r * 0.45f;
			p[i] = merkez + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rr;
		}
		c.DrawColoredPolygon(p, renk);
	}

	/// <summary>Elmas simgesi.</summary>
	public static void ElmasSekli(CanvasItem c, Vector2 m, float r, Color renk)
	{
		c.DrawColoredPolygon(new[] { m + new Vector2(0, -r), m + new Vector2(r * 0.8f, 0), m + new Vector2(0, r), m + new Vector2(-r * 0.8f, 0) }, renk);
	}
}

/// <summary>Bir kartın küçük görünümü: çerçeve, portre, ad, seviye, yıldız, element ve sınıf.</summary>
public partial class KartKutu : Control
{
	public KartTanim T;
	public int Seviye = 1, Yildiz = 1, Kopya;
	public bool Kilitli, Secili, KopyaGoster;
	public bool Arka;   // kart arkası (çağırma animasyonunda açılmadan önce)
	public Action Tiklandi;
	Vector2 _basma;
	bool _basili;

	public KartKutu() { }
	public KartKutu(KartTanim t, float genislik = 150)
	{
		T = t;
		CustomMinimumSize = new Vector2(genislik, genislik * 1.38f);
		MouseFilter = MouseFilterEnum.Pass;
	}

	public static KartKutu Sahipten(SahipKart s, float genislik = 150)
	{
		return new KartKutu(Veri.Harita[s.Id], genislik) { Seviye = s.Seviye, Yildiz = s.Yildiz, Kopya = s.Kopya };
	}

	public override void _GuiInput(InputEvent e)
	{
		if (e is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
		{
			if (mb.Pressed) { _basili = true; _basma = mb.GlobalPosition; }
			else if (_basili)
			{
				_basili = false;
				if (mb.GlobalPosition.DistanceTo(_basma) < 18) Tiklandi?.Invoke();
			}
		}
	}

	public override void _Draw()
	{
		if (T == null) return;
		var w = Size.X; var h = Size.Y;
		if (Arka) { ArkaCiz(w, h); return; }
		var nadir = Veri.NadirlikRenk(T.Nadirlik);
		float k = MathF.Max(3, w * 0.035f);
		var dis = new StyleBoxFlat { BgColor = Kilitli ? new Color("2a2e40") : nadir };
		dis.SetCornerRadiusAll((int)(w * 0.08f));
		DrawStyleBox(dis, new Rect2(0, 0, w, h));
		var ic = new StyleBoxFlat { BgColor = new Color("161a2e") };
		ic.SetCornerRadiusAll((int)(w * 0.06f));
		DrawStyleBox(ic, new Rect2(k, k, w - 2 * k, h - 2 * k));

		// portre
		float pw = w - 2 * k - 4;
		var pr = new Rect2(k + 2, k + 2, pw, pw);
		var tex = Veri.Portre(T.Id);
		var mod = Kilitli ? new Color(0.15f, 0.15f, 0.2f) : Colors.White;
		if (tex != null) DrawTextureRect(tex, pr, false, mod);
		else
		{
			DrawRect(pr, Veri.ElementRenk(T.Element).Darkened(Kilitli ? 0.8f : 0.45f));
			DrawString(Ui.Yazi, pr.Position + new Vector2(0, pw * 0.68f), T.Ad.Substring(0, 1), HorizontalAlignment.Center, pw, (int)(pw * 0.55f), new Color(1, 1, 1, Kilitli ? 0.2f : 0.85f));
		}
		if (Kilitli)
			DrawString(Ui.Yazi, pr.Position + new Vector2(0, pw * 0.65f), "?", HorizontalAlignment.Center, pw, (int)(pw * 0.5f), new Color(1, 1, 1, 0.5f));

		// mitoloji şeridi: portrenin altında, kartın hangi mitolojiden olduğunu yazar
		if (!Kilitli && w >= 90)
		{
			float bh = MathF.Max(13, w * 0.12f);
			var br = new Rect2(pr.Position.X, pr.End.Y - bh, pw, bh);
			var mr = Veri.MitolojiRenk(T.Mitoloji);
			DrawRect(br, new Color(mr.R, mr.G, mr.B, 0.85f));
			int mfs = (int)MathF.Max(9, bh * 0.72f);
			DrawString(Ui.Yazi, new Vector2(br.Position.X, br.End.Y - bh * 0.22f), Veri.MitolojiKisa(T.Mitoloji), HorizontalAlignment.Center, pw, mfs, Colors.White);
		}

		// element ve sınıf rozetleri
		float r = w * 0.1f;
		var em = new Vector2(k + r + 2, k + r + 2);
		DrawCircle(em, r, Veri.ElementRenk(T.Element));
		DrawArc(em, r, 0, Mathf.Tau, 24, new Color(0, 0, 0, 0.6f), 2);
		var sm = new Vector2(w - k - r - 2, k + r + 2);
		DrawCircle(sm, r, new Color("20243a"));
		DrawArc(sm, r, 0, Mathf.Tau, 24, nadir, 2);
		DrawString(Ui.Yazi, sm + new Vector2(-r, r * 0.45f), Veri.SinifHarf(T.Sinif), HorizontalAlignment.Center, 2 * r, (int)(r * 1.2f), Colors.White);

		// ad
		int fs = (int)MathF.Max(12, w * 0.11f);
		float ady = pr.End.Y + fs * 1.05f;
		DrawString(Ui.Yazi, new Vector2(k, ady), T.Ad, HorizontalAlignment.Center, w - 2 * k, T.Ad.Length > 11 ? (int)(fs * 0.85f) : fs, Kilitli ? Ui.Soluk : Colors.White);

		if (!Kilitli)
		{
			// yıldızlar
			float yr = w * 0.055f;
			float bas = w / 2 - (Yildiz - 1) * yr * 1.1f;
			for (int i = 0; i < Yildiz; i++) Ui.Yildiz(this, new Vector2(bas + i * yr * 2.2f, h - k - yr * 3.3f), yr, Ui.Altin);
			// seviye
			DrawString(Ui.Yazi, new Vector2(k, h - k - 4), $"Sv {Seviye}", HorizontalAlignment.Center, w - 2 * k, (int)(fs * 0.85f), Ui.Soluk);
			if (KopyaGoster && Kopya > 0)
			{
				var kc = new Vector2(w - k - r - 2, pr.End.Y - r - 2);
				DrawCircle(kc, r, Ui.Yesil);
				DrawString(Ui.Yazi, kc + new Vector2(-r, r * 0.45f), "+" + Kopya, HorizontalAlignment.Center, 2 * r, (int)(r * 1.05f), Colors.White);
			}
		}
		if (Secili) DrawRect(new Rect2(1, 1, w - 2, h - 2), Ui.Altin, false, 4);
	}

	void ArkaCiz(float w, float h)
	{
		float k = MathF.Max(3, w * 0.035f);
		var dis = new StyleBoxFlat { BgColor = new Color("c79a3a") };
		dis.SetCornerRadiusAll((int)(w * 0.08f));
		DrawStyleBox(dis, new Rect2(0, 0, w, h));
		var ic = new StyleBoxFlat { BgColor = new Color("2a1d55") };
		ic.SetCornerRadiusAll((int)(w * 0.06f));
		DrawStyleBox(ic, new Rect2(k, k, w - 2 * k, h - 2 * k));
		// elmas desen (kartın içinde kalır)
		var desen = new Color(1, 1, 1, 0.07f);
		float ad = w * 0.16f;
		for (float y = k * 3 + ad / 2; y < h - k * 3; y += ad)
			for (float x = k * 3 + ad / 2; x < w - k * 3; x += ad)
				Ui.ElmasSekli(this, new Vector2(x, y), ad * 0.3f, desen);
		var m = new Vector2(w / 2, h / 2);
		DrawCircle(m, w * 0.3f, new Color("1a1238"));
		DrawArc(m, w * 0.3f, 0, Mathf.Tau, 40, new Color("c79a3a"), MathF.Max(2, w * 0.025f));
		Ui.ElmasSekli(this, m, w * 0.2f, new Color("f2c14e"));
		Ui.ElmasSekli(this, m, w * 0.1f, new Color("fff1b0"));
		float yr = w * 0.05f;
		foreach (var c in new[] { new Vector2(k * 3.5f, k * 3.5f), new Vector2(w - k * 3.5f, k * 3.5f), new Vector2(k * 3.5f, h - k * 3.5f), new Vector2(w - k * 3.5f, h - k * 3.5f) })
			Ui.Yildiz(this, c, yr, new Color("c79a3a"));
	}
}
