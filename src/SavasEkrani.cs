using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>Alt panelde bir kartın küçük durumu: portre, can ve enerji çubuğu.</summary>
public partial class BirimKutu : Control
{
	public Birim B;
	public float Can, MaxCan, Kalkan, Enerji;
	public bool Olu, Aktif;

	public override void _Draw()
	{
		if (B == null) return;
		var w = Size.X;
		var t = B.T;
		var sb = new StyleBoxFlat { BgColor = Olu ? new Color("30303a") : Aktif ? Ui.Altin : Veri.NadirlikRenk(t.Nadirlik) };
		sb.SetCornerRadiusAll(8);
		DrawStyleBox(sb, new Rect2(0, 0, w, w));
		var pr = new Rect2(3, 3, w - 6, w - 6);
		var tex = Veri.Portre(t.Id);
		if (tex != null) DrawTextureRect(tex, pr, false, Olu ? new Color(0.25f, 0.25f, 0.28f) : Colors.White);
		DrawCircle(new Vector2(11, 11), 7, Veri.ElementRenk(t.Element));
		float y = w + 3;
		DrawRect(new Rect2(0, y, w, 9), new Color("0b0d18"));
		if (!Olu)
		{
			float oran = Mathf.Clamp(Can / MaxCan, 0, 1);
			var renk = B.Taraf == 0 ? (oran > 0.5f ? Ui.Yesil : oran > 0.25f ? new Color("e8b33a") : Ui.Kirmizi) : (oran > 0.25f ? new Color("e0603f") : Ui.Kirmizi);
			DrawRect(new Rect2(1, y + 1, (w - 2) * oran, 7), renk);
			if (Kalkan > 0) DrawRect(new Rect2(1, y + 1, (w - 2) * Mathf.Clamp(Kalkan / MaxCan, 0, 1), 3), new Color("bfe6ff"));
			DrawRect(new Rect2(0, y + 11, w, 6), new Color("0b0d18"));
			DrawRect(new Rect2(1, y + 12, (w - 2) * Mathf.Clamp(Enerji / 100f, 0, 1), 4), Enerji >= 100 ? Ui.Altin : new Color("6a8cff"));
		}
	}
}

/// <summary>Sahnedeki tam boy savaşçı: nefes alma, saldırı, vurulma, ölüm animasyonları.</summary>
public partial class Savasci : Node2D
{
	public Birim B;
	public Sprite2D Govde;
	public float Can, MaxCan, Kalkan, Enerji;
	public bool Olu;
	public Vector2 Ev;          // yerindeki konum (ayak noktası)
	public float Boy = 110;     // ekranda görünen boy
	float _faz, _zaman;
	public bool Sersem;

	public int Yon => B.Taraf == 0 ? 1 : -1;   // oyuncu sağa, düşman sola bakar
	public Vector2 Merkez => GlobalPosition + new Vector2(0, -Boy * 0.5f);
	public Vector2 Bas => GlobalPosition + new Vector2(0, -Boy - 8);

	public override void _Ready()
	{
		_faz = (float)GD.RandRange(0, Mathf.Tau);
		var tex = Veri.Beden(B.T.Id);
		Govde = new Sprite2D { Centered = false, TextureFilter = TextureFilterEnum.Nearest };
		if (tex != null)
		{
			Govde.Texture = tex;
			float olcek = Boy / tex.GetHeight();
			Govde.Scale = new Vector2(olcek * Yon, olcek);
			Govde.Offset = new Vector2(-tex.GetWidth() / 2f, -tex.GetHeight());
		}
		AddChild(Govde);
	}

	public override void _Process(double delta)
	{
		_zaman += (float)delta;
		if (!Olu && Govde.Texture != null)
		{
			float olcek = Boy / Govde.Texture.GetHeight();
			float nefes = 1f + 0.025f * Mathf.Sin(_zaman * 2.6f + _faz);
			Govde.Scale = new Vector2(olcek * Yon * (2f - nefes), olcek * nefes);
		}
		QueueRedraw();
	}

	public override void _Draw()
	{
		// gölge
		DrawSetTransform(Vector2.Zero, 0, new Vector2(1, 0.3f));
		DrawCircle(Vector2.Zero, Boy * 0.32f, new Color(0, 0, 0, Olu ? 0.15f : 0.35f));
		DrawSetTransform(Vector2.Zero, 0, Vector2.One);
		if (Olu) return;
		// can çubuğu başın üstünde
		float w = 64, y = -Boy - 18;
		DrawRect(new Rect2(-w / 2 - 1, y - 1, w + 2, 9), new Color(0, 0, 0, 0.75f));
		float oran = Mathf.Clamp(Can / MaxCan, 0, 1);
		DrawRect(new Rect2(-w / 2, y, w * oran, 7), B.Taraf == 0 ? Ui.Yesil : new Color("e5533b"));
		if (Kalkan > 0) DrawRect(new Rect2(-w / 2, y, w * Mathf.Clamp(Kalkan / MaxCan, 0, 1), 3), new Color("cfeeff"));
		DrawRect(new Rect2(-w / 2, y + 8, w * Mathf.Clamp(Enerji / 100f, 0, 1), 3), Enerji >= 100 ? Ui.Altin : new Color("7aa0ff"));
		if (Sersem)
			for (int i = 0; i < 3; i++)
			{
				float a = _zaman * 4 + i * Mathf.Tau / 3;
				Ui.Yildiz(this, new Vector2(Mathf.Cos(a) * 20, y - 12 + Mathf.Sin(a) * 5), 5, Ui.Altin);
			}
	}

	public void Parla(Color c, float sure)
	{
		Govde.Modulate = c;
		var tw = CreateTween();
		tw.TweenProperty(Govde, "modulate", Colors.White, sure);
	}

	public void Geri(float hiz)
	{
		var tw = CreateTween();
		tw.TweenProperty(this, "position", Ev + new Vector2(-Yon * 14, 0), 0.06f / hiz);
		tw.TweenProperty(this, "position", Ev, 0.14f / hiz);
	}

	public void Dus(float hiz)
	{
		Olu = true;
		var tw = CreateTween().SetParallel();
		tw.TweenProperty(Govde, "rotation", -Yon * 1.45f, 0.35f / hiz).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
		tw.TweenProperty(Govde, "position", new Vector2(-Yon * 20, -6), 0.35f / hiz);
		tw.TweenProperty(this, "modulate", new Color(0.55f, 0.55f, 0.6f, 0.45f), 0.6f / hiz);
	}
}

/// <summary>Savaş simülasyonunun olay kaydını tam boy karakterler, arka plan ve efektlerle oynatır.</summary>
public partial class SavasEkrani : Control
{
	public Savas S;
	public string Baslik = "";
	public string Arka = "turk";   // assets/arka/<ad>.png
	public bool Boss;
	public Action<bool> Bitti;
	public float Hiz = 1f;
	public bool Atla;

	const float SahneUst = 70, SahneBoy = 730;
	readonly Dictionary<int, Savasci> _savascilar = new();
	readonly Dictionary<int, BirimKutu> _kutular = new();
	Node2D _sahne, _efekt;
	Label _turEtiket, _ultiEtiket;
	int _ultiKaynak = -1;
	bool _ultiYakinYapildi;

	public override void _Ready()
	{
		Size = GetParent<Control>().Size;
		MouseFilter = MouseFilterEnum.Stop;
		ClipContents = true;

		// sahne: arka plan + savaşçılar
		_sahne = new Node2D { Position = new Vector2(0, SahneUst) };
		AddChild(_sahne);
		var arka = GD.Load<Texture2D>($"res://assets/arka/{Arka}.png");
		if (arka != null)
		{
			float olc = Mathf.Max(720f / arka.GetWidth(), SahneBoy / arka.GetHeight());
			_sahne.AddChild(new Sprite2D
			{
				Texture = arka, Centered = false, Scale = new Vector2(olc, olc), TextureFilter = TextureFilterEnum.Nearest,
				Position = new Vector2((720 - arka.GetWidth() * olc) / 2, 0),
			});
		}
		// sahnenin alt kenarına koyu geçiş
		var gecis = new ColorRect { Color = new Color(0, 0, 0, 0.0f), Size = new Vector2(720, SahneBoy) };
		_sahne.AddChild(gecis);

		foreach (var b in S.Birimler.OrderBy(b => Konum(b).Y))
		{
			var s = new Savasci
			{
				B = b, Can = b.MaxCan, MaxCan = b.MaxCan, Enerji = b.Enerji,
				Boy = (b.T.Sinif == "tank" ? 172 : 152) + (b.T.Nadirlik == "efsanevi" ? 12 : 0),
			};
			s.Ev = Konum(b);
			s.Position = s.Ev;
			_savascilar[b.Idx] = s;
			_sahne.AddChild(s);
		}
		_efekt = new Node2D();
		_sahne.AddChild(_efekt);

		// üst şerit
		var ust = new ColorRect { Color = new Color("0b0e1c"), Size = new Vector2(720, SahneUst) };
		AddChild(ust);
		var baslik = Ui.Etiket(Baslik, 26, Ui.Altin);
		baslik.Position = new Vector2(16, 16); baslik.Size = new Vector2(480, 40);
		AddChild(baslik);
		_turEtiket = Ui.Etiket("Tur 1", 24, Colors.White, HorizontalAlignment.Right);
		_turEtiket.Position = new Vector2(500, 16); _turEtiket.Size = new Vector2(204, 40);
		AddChild(_turEtiket);

		_ultiEtiket = Ui.Etiket("", 32, Ui.Altin, HorizontalAlignment.Center);
		_ultiEtiket.AddThemeConstantOverride("outline_size", 8);
		_ultiEtiket.Position = new Vector2(0, SahneUst + 20); _ultiEtiket.Size = new Vector2(720, 50);
		AddChild(_ultiEtiket);

		// alt panel: iki sıra durum kutusu
		var panel = new ColorRect { Color = new Color("10142a"), Position = new Vector2(0, SahneUst + SahneBoy), Size = new Vector2(720, Size.Y - SahneUst - SahneBoy) };
		AddChild(panel);
		AddChild(new ColorRect { Color = new Color("2a3259"), Position = new Vector2(0, SahneUst + SahneBoy), Size = new Vector2(720, 3) });
		for (int taraf = 1; taraf >= 0; taraf--)
		{
			var et = Ui.Etiket(taraf == 0 ? "Takımın" : "Düşman", 18, taraf == 0 ? Ui.Yesil : Ui.Kirmizi);
			float y = SahneUst + SahneBoy + 14 + (1 - taraf) * 168;
			et.Position = new Vector2(16, y); et.Size = new Vector2(120, 24);
			AddChild(et);
			foreach (var b in S.Birimler.Where(b => b.Taraf == taraf))
			{
				var k = new BirimKutu { B = b, Can = b.MaxCan, MaxCan = b.MaxCan, Enerji = b.Enerji };
				k.Size = new Vector2(108, 130);
				k.Position = new Vector2(104 + b.Konum * 122, y);
				_kutular[b.Idx] = k;
				AddChild(k);
			}
		}

		var alt = Ui.Yatay(12);
		alt.Position = new Vector2(16, Size.Y - 88);
		alt.Size = new Vector2(688, 70);
		AddChild(alt);
		Button hizB = null;
		hizB = Ui.Dugme("Hız 1x", () => { Hiz = Hiz >= 3 ? 1 : Hiz + 1; hizB.Text = $"Hız {Hiz:0}x"; }, new Color("444c78"));
		alt.AddChild(Ui.Genis(hizB));
		alt.AddChild(Ui.Genis(Ui.Dugme("Atla", () => Atla = true, new Color("6a4c8a"))));

		Ses.Muzik(Boss ? "boss" : Arka switch { "yunan" or "japon" => "savas3", "iskandinav" or "cin" => "savas2", _ => "savas1" });
		Ses.Konus("anons_basla");
		_ = Oynat();
	}

	/// <summary>Oyuncu solda, düşman sağda. Ön sıra (0-1) ortaya yakın, arka sıra (2-4) kenarda.</summary>
	static Vector2 Konum(Birim b)
	{
		bool on = b.Konum < 2;
		float x = on ? 262 : 100;
		float y = on ? (b.Konum == 0 ? 548 : 672) : 472 + (b.Konum - 2) * 112;
		if (b.Taraf == 1) x = 720 - x;
		return new Vector2(x, y);
	}

	float Sure(float s) => Atla ? 0 : s / Hiz;

	async Task Bekle(float s)
	{
		float t = Sure(s);
		if (t <= 0.001f) return;
		await ToSignal(GetTree().CreateTimer(t), SceneTreeTimer.SignalName.Timeout);
	}

	void Guncelle(int idx, float can, float kalkan)
	{
		var s = _savascilar[idx]; s.Can = can; s.Kalkan = kalkan;
		var k = _kutular[idx]; k.Can = can; k.Kalkan = kalkan; k.QueueRedraw();
	}

	static bool Yakin(Birim b) => b.T.Sinif is "tank" or "savasci";
	static bool Yildirimli(Birim b) => b.T.Id is "zeus" or "thor" or "odin";

	async Task Oynat()
	{
		await Bekle(0.6f);
		foreach (var o in S.Olaylar)
		{
			if (!IsInsideTree()) return;
			if (Atla) { HizliUygula(o); if (o.Tip == "bitis") { Ses.Efekt(o.Deger == 0 ? "zafer" : "yenilgi", 0, 1, 0); Ses.Konus(o.Deger == 0 ? "anons_zafer" : "anons_yenilgi"); Bitti?.Invoke(o.Deger == 0); return; } continue; }
			switch (o.Tip)
			{
				case "tur":
					_turEtiket.Text = $"Tur {o.Deger:0}/{Savas.MaxTur}";
					await Bekle(0.15f);
					break;

				case "ulti":
				{
					var s = _savascilar[o.Kaynak];
					_ultiKaynak = o.Kaynak; _ultiYakinYapildi = false;
					Ses.Konus(s.B.T.Id);
					Ses.Efekt("buyu", -4, 0.7f);
					foreach (var k in _kutular.Values) { k.Aktif = k.B.Idx == o.Kaynak; k.QueueRedraw(); }
					_ultiEtiket.Text = $"{s.B.T.Ad}: {o.Metin}!";
					_ultiEtiket.Modulate = Colors.White;
					var tw = CreateTween();
					tw.TweenInterval(1.1f / Hiz);
					tw.TweenProperty(_ultiEtiket, "modulate:a", 0f, 0.3f / Hiz);
					var renk = Efekt.ElementRengi(s.B.T.Element);
					s.Parla(new Color(2.2f, 2.0f, 1.4f), 0.5f / Hiz);
					Efekt.Yukselen(_efekt, s.Position + new Vector2(0, -10), renk, 26, 30);
					Efekt.Halka(_efekt, s.Position + new Vector2(0, -s.Boy * 0.5f), renk, 10, 90, 0.5f / Hiz, 4);
					var st = s.CreateTween();
					st.TweenProperty(s, "position", s.Ev + new Vector2(0, -16), 0.15f / Hiz);
					st.TweenProperty(s, "position", s.Ev, 0.15f / Hiz);
					await Bekle(0.45f);
					break;
				}

				case "saldiri":
				{
					var s = _savascilar[o.Kaynak]; var h = _savascilar[o.Hedef];
					bool ulti = o.Metin == "ulti";
					var renk = Efekt.ElementRengi(s.B.T.Element);
					if (ulti && Yildirimli(s.B))
					{
						Efekt.Yildirim(_efekt, h.Position + new Vector2(0, -h.Boy * 0.4f), new Color("bfe4ff"));
						Ses.Efekt("yildirim", 0, 1, 0.15f);
						Efekt.Sarsinti(_sahne, 6, 0.2f);
						await Bekle(0.12f);
					}
					else if (Yakin(s.B) && (!ulti || !_ultiYakinYapildi))
					{
						if (ulti) _ultiYakinYapildi = true;
						await Hucum(s, h, renk);
					}
					else if (ulti && Yakin(s.B))
					{
						Efekt.Kesik(_efekt, h.Position + new Vector2(0, -h.Boy * 0.5f), renk, s.Yon > 0);
						Ses.Efekt("kilic", -3, 1.1f);
						await Bekle(0.08f);
					}
					else
					{
						// büyücü/destek: büyü topu
						s.Parla(new Color(1.6f, 1.6f, 1.6f), 0.2f / Hiz);
						var bas = s.Position + new Vector2(s.Yon * 24, -s.Boy * 0.6f);
						var son = h.Position + new Vector2(0, -h.Boy * 0.5f);
						Ses.Efekt("buyu", ulti ? 0 : -5, ulti ? 1f : 1.25f);
						await Efekt.Mermi(_efekt, bas, son, renk, Sure(ulti ? 0.22f : 0.3f), ulti ? 12 : 8);
						Efekt.Patlama(_efekt, son, renk, ulti ? 30 : 14, ulti ? 300 : 180, ulti ? 8 : 5, 0.45f, 250);
						Ses.Efekt(Ses.ElementEfekti(s.B.T.Element), ulti ? 0 : -7, 1, 0.1f);
					}
					break;
				}

				case "hasar":
				{
					var h = _savascilar[o.Hedef];
					Guncelle(o.Hedef, o.KalanCan, o.KalanKalkan);
					if (o.Metin == "yanma")
					{
						Efekt.Yukselen(_efekt, h.Position, new Color("ff8a2a"), 12, 20);
						h.Parla(new Color(1.8f, 0.9f, 0.6f), 0.25f / Hiz);
					}
					else
					{
						h.Parla(new Color(3, 3, 3), 0.18f / Hiz);
						h.Geri(Hiz);
						var kaynak = o.Kaynak >= 0 ? _savascilar[o.Kaynak] : null;
						var renk = kaynak != null ? Efekt.ElementRengi(kaynak.B.T.Element) : Colors.White;
						Efekt.Patlama(_efekt, h.Position + new Vector2(0, -h.Boy * 0.5f), renk.Lerp(Colors.White, 0.3f), o.Kritik ? 22 : 10, 200, 5, 0.35f, 400,
							80, new Vector2(kaynak != null ? kaynak.Yon : 1, -0.4f));
						if (o.Kritik) { Efekt.Sarsinti(_sahne, 9, 0.22f); Ses.Efekt("kritik", 0, 1, 0.1f); }
					}
					var yr = o.Metin == "yanma" ? new Color("ff9a3a") : o.Kritik ? Ui.Altin : o.Ustunluk > 0 ? new Color("ff7b7b") : Colors.White;
					Yazi(h, $"-{o.Deger:0}" + (o.Kritik ? "!" : ""), yr, o.Kritik ? 38 : 28);
					await Bekle(0.2f);
					break;
				}

				case "iyiles":
				{
					var h = _savascilar[o.Hedef];
					Guncelle(o.Hedef, o.KalanCan, o.KalanKalkan);
					Efekt.Yukselen(_efekt, h.Position, Ui.Yesil, 18, 26);
					Ses.Efekt("iyiles", -3, 1, 0.25f);
					h.Parla(new Color(1.2f, 2f, 1.2f), 0.3f / Hiz);
					Yazi(h, $"+{o.Deger:0}", Ui.Yesil, 26);
					await Bekle(0.12f);
					break;
				}

				case "kalkan":
				{
					var h = _savascilar[o.Hedef];
					Guncelle(o.Hedef, o.KalanCan, o.KalanKalkan);
					Efekt.Halka(_efekt, h.Position + new Vector2(0, -h.Boy * 0.5f), new Color("9fdcff"), h.Boy * 0.3f, h.Boy * 0.62f, 0.5f / Hiz, 6);
					Ses.Efekt("kalkan", -4, 1, 0.2f);
					Yazi(h, "Kalkan", new Color("9fdcff"), 22);
					await Bekle(0.12f);
					break;
				}

				case "durum":
				{
					if (o.Hedef < 0) { _ultiEtiket.Text = o.Metin; _ultiEtiket.Modulate = Colors.White; await Bekle(0.8f); break; }
					var h = _savascilar[o.Hedef];
					var merkez = h.Position + new Vector2(0, -h.Boy * 0.5f);
					switch (o.Metin)
					{
						case "Sersem": Ses.Efekt("sersem", -4, 1, 0.2f); h.Sersem = true; Efekt.Halka(_efekt, h.Position + new Vector2(0, -h.Boy), Ui.Altin, 4, 34, 0.4f / Hiz, 3); break;
						case "Yanma": Efekt.Yukselen(_efekt, h.Position, new Color("ff6a1a"), 16, 22); break;
						case "Zehir": Efekt.Yukselen(_efekt, h.Position, new Color("8be04a"), 16, 22); break;
						case "Alay!": Efekt.Halka(_efekt, merkez, new Color("ff5a4a"), 20, 110, 0.5f / Hiz, 5); break;
						default:
							if (o.Metin.StartsWith("Saldırı")) Efekt.Yukselen(_efekt, h.Position, Ui.Altin, 14, 26);
							else if (o.Metin.StartsWith("Savunma")) Efekt.Patlama(_efekt, merkez, new Color("b06aff"), 12, 120, 5, 0.6f, 300);
							break;
					}
					Yazi(h, o.Metin, new Color("e7c4ff"), 20);
					await Bekle(0.1f);
					break;
				}

				case "atla":
					_savascilar[o.Kaynak].Sersem = true;
					Yazi(_savascilar[o.Kaynak], o.Metin, Ui.Altin, 24);
					await Bekle(0.35f);
					_savascilar[o.Kaynak].Sersem = false;
					break;

				case "enerji":
					_savascilar[o.Hedef].Enerji = o.Deger;
					_kutular[o.Hedef].Enerji = o.Deger;
					_kutular[o.Hedef].QueueRedraw();
					break;

				case "olum":
				{
					var h = _savascilar[o.Hedef];
					h.Can = 0; h.Sersem = false;
					Ses.Olum(h.B.T.Id);
					h.Dus(Hiz);
					_kutular[o.Hedef].Olu = true; _kutular[o.Hedef].QueueRedraw();
					Efekt.Patlama(_efekt, h.Position, new Color("c8b89a"), 16, 120, 6, 0.6f, 80, 70);
					await Bekle(0.3f);
					break;
				}

				case "bitis":
					Ses.Efekt(o.Deger == 0 ? "zafer" : "yenilgi", 0, 1, 0);
					Ses.Konus(o.Deger == 0 ? "anons_zafer" : "anons_yenilgi");
					await Bekle(1.2f);
					Bitti?.Invoke(o.Deger == 0);
					return;
			}
		}
	}

	/// <summary>Yakın dövüşçü hedefin önüne koşar, vurur ve hemen döner (dönüş beklenmez).</summary>
	async Task Hucum(Savasci s, Savasci h, Color renk)
	{
		var hedefNokta = h.Position + new Vector2(-s.Yon * 62, 4);
		var tw = s.CreateTween();
		tw.TweenProperty(s, "position", hedefNokta, 0.16f / Hiz).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
		await ToSignal(tw, Tween.SignalName.Finished);
		Efekt.Kesik(_efekt, h.Position + new Vector2(-s.Yon * 10, -h.Boy * 0.55f), renk, s.Yon > 0);
		Ses.Efekt(s.B.T.Sinif == "tank" ? "darbe" : "kilic", -2, 1, 0.05f);
		var don = s.CreateTween();
		don.TweenInterval(0.12f / Hiz);
		don.TweenProperty(s, "position", s.Ev, 0.2f / Hiz).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
	}

	/// <summary>"Atla" basılınca olayları animasyonsuz uygular.</summary>
	void HizliUygula(Olay o)
	{
		switch (o.Tip)
		{
			case "hasar": case "iyiles": case "kalkan": Guncelle(o.Hedef, o.KalanCan, o.KalanKalkan); break;
			case "enerji": _savascilar[o.Hedef].Enerji = o.Deger; _kutular[o.Hedef].Enerji = o.Deger; _kutular[o.Hedef].QueueRedraw(); break;
			case "olum":
				var h = _savascilar[o.Hedef];
				if (!h.Olu) { h.Can = 0; h.Dus(8); _kutular[o.Hedef].Olu = true; _kutular[o.Hedef].QueueRedraw(); }
				break;
		}
	}

	void Yazi(Savasci s, string metin, Color renk, int boyut)
	{
		if (Atla) return;
		var l = Ui.Etiket(metin, boyut, renk, HorizontalAlignment.Center);
		l.AddThemeConstantOverride("outline_size", 7);
		l.Size = new Vector2(220, 44);
		l.Position = s.Position + new Vector2(-110, -s.Boy - 60 - GD.Randf() * 20) + new Vector2(0, SahneUst);
		l.MouseFilter = MouseFilterEnum.Ignore;
		AddChild(l);
		var tw = CreateTween().SetParallel();
		tw.TweenProperty(l, "position:y", l.Position.Y - 60, 0.8f / Hiz);
		tw.TweenProperty(l, "modulate:a", 0f, 0.8f / Hiz).SetDelay(0.35f / Hiz);
		tw.Chain().TweenCallback(Callable.From(l.QueueFree));
	}
}
