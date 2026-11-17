using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>Savaştaki bir kartın görüntüsü: portre, can ve enerji çubuğu, kalkan.</summary>
public partial class BirimKutu : Control
{
	public Birim B;
	public float Can, MaxCan, Kalkan, Enerji;
	public bool Olu;
	public float Parlama;   // vurulunca beyaz parlama
	public string Etiket = "";

	public override void _Draw()
	{
		if (B == null) return;
		var w = Size.X;
		var t = B.T;
		var nadir = Veri.NadirlikRenk(t.Nadirlik);
		var sb = new StyleBoxFlat { BgColor = Olu ? new Color("30303a") : nadir };
		sb.SetCornerRadiusAll(10);
		DrawStyleBox(sb, new Rect2(0, 0, w, w));
		var pr = new Rect2(4, 4, w - 8, w - 8);
		var tex = Veri.Portre(t.Id);
		var mod = Olu ? new Color(0.25f, 0.25f, 0.28f) : Colors.White.Lerp(new Color(3, 3, 3), Parlama);
		if (tex != null) DrawTextureRect(tex, pr, false, mod);
		else
		{
			DrawRect(pr, Veri.ElementRenk(t.Element).Darkened(Olu ? 0.8f : 0.4f));
			DrawString(Ui.Yazi, pr.Position + new Vector2(0, pr.Size.Y * 0.68f), t.Ad.Substring(0, 1), HorizontalAlignment.Center, pr.Size.X, (int)(pr.Size.X * 0.55f), Colors.White);
		}
		// element noktası + seviye
		DrawCircle(new Vector2(14, 14), 9, Veri.ElementRenk(t.Element));
		DrawString(Ui.Yazi, new Vector2(4, w - 8), $"{B.Seviye}", HorizontalAlignment.Left, -1, 15, Colors.White);
		if (B.Alayli && !Olu) DrawString(Ui.Yazi, new Vector2(w - 26, 22), "!", HorizontalAlignment.Left, -1, 22, Ui.Altin);
		// çubuklar
		float y = w + 4;
		DrawRect(new Rect2(0, y, w, 12), new Color("0b0d18"));
		if (!Olu)
		{
			float oran = Mathf.Clamp(Can / MaxCan, 0, 1);
			var renk = oran > 0.5f ? Ui.Yesil : oran > 0.25f ? new Color("e8b33a") : Ui.Kirmizi;
			if (B.Taraf == 1) renk = oran > 0.25f ? new Color("e0603f") : Ui.Kirmizi;
			DrawRect(new Rect2(1, y + 1, (w - 2) * oran, 10), renk);
			if (Kalkan > 0) DrawRect(new Rect2(1, y + 1, (w - 2) * Mathf.Clamp(Kalkan / MaxCan, 0, 1), 4), new Color("bfe6ff"));
			DrawRect(new Rect2(0, y + 15, w, 7), new Color("0b0d18"));
			DrawRect(new Rect2(1, y + 16, (w - 2) * Mathf.Clamp(Enerji / 100f, 0, 1), 5), Enerji >= 100 ? Ui.Altin : new Color("6a8cff"));
		}
		if (Etiket != "") DrawString(Ui.Yazi, new Vector2(0, y + 40), Etiket, HorizontalAlignment.Center, w, 15, Ui.Soluk);
	}
}

/// <summary>Savaş simülasyonunun olay kaydını animasyonla oynatır.</summary>
public partial class SavasEkrani : Control
{
	public Savas S;
	public string Baslik = "";
	public Action<bool> Bitti;    // oyuncu kazandı mı
	public float Hiz = 1f;
	public bool Atla;
	readonly Dictionary<int, BirimKutu> _kutular = new();
	Label _turEtiket, _ultiEtiket;
	Control _alan;

	public override void _Ready()
	{
		SetAnchorsPreset(LayoutPreset.FullRect);
		MouseFilter = MouseFilterEnum.Stop;
		Size = GetParent<Control>().Size;

		_alan = new Control { AnchorRight = 1, AnchorBottom = 1 };
		AddChild(_alan);

		var ust = Ui.Yatay(8);
		ust.Position = new Vector2(16, 16);
		ust.Size = new Vector2(688, 60);
		AddChild(ust);
		ust.AddChild(Ui.Genis(Ui.Etiket(Baslik, 26, Ui.Altin)));
		_turEtiket = Ui.Etiket("Tur 1", 24, Colors.White, HorizontalAlignment.Right);
		ust.AddChild(Ui.Genis(_turEtiket, 0.6f));

		_ultiEtiket = Ui.Etiket("", 34, Ui.Altin, HorizontalAlignment.Center);
		_ultiEtiket.Position = new Vector2(0, 600);
		_ultiEtiket.Size = new Vector2(720, 60);
		AddChild(_ultiEtiket);

		var alt = Ui.Yatay(12);
		alt.Position = new Vector2(16, 1180);
		alt.Size = new Vector2(688, 70);
		AddChild(alt);
		Button hizB = null;
		hizB = Ui.Dugme("Hız 1x", () => { Hiz = Hiz >= 3 ? 1 : Hiz + 1; hizB.Text = $"Hız {Hiz:0}x"; }, new Color("444c78"));
		alt.AddChild(Ui.Genis(hizB));
		alt.AddChild(Ui.Genis(Ui.Dugme("Atla", () => Atla = true, new Color("6a4c8a"))));

		foreach (var b in S.Birimler)
		{
			var k = new BirimKutu { B = b, Can = b.MaxCan, MaxCan = b.MaxCan, Enerji = b.Enerji, Etiket = b.T.Ad };
			k.Size = new Vector2(118, 170);
			k.Position = Konum(b);
			_kutular[b.Idx] = k;
			_alan.AddChild(k);
		}
		_ = Oynat();
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), new Color("0e1222"));
		DrawRect(new Rect2(0, Size.Y / 2, Size.X, Size.Y / 2), new Color("161c38"));
		DrawLine(new Vector2(0, Size.Y / 2), new Vector2(Size.X, Size.Y / 2), new Color("2a3259"), 3);
	}

	/// <summary>Düşman üstte (ön sıra aşağıda), oyuncu altta (ön sıra yukarıda).</summary>
	static Vector2 Konum(Birim b)
	{
		bool on = b.Konum < 2;
		int sira = on ? b.Konum : b.Konum - 2;
		int adet = on ? 2 : 3;
		float aralik = 150;
		float x = 360 - (adet - 1) * aralik / 2 + sira * aralik - 59;
		float y = b.Taraf == 1 ? (on ? 380 : 170) : (on ? 750 : 965);
		return new Vector2(x, y);
	}

	float Bekleme(float s) => Atla ? 0 : s / Hiz;

	async Task Bekle(float s)
	{
		float t = Bekleme(s);
		if (t <= 0.001f) return;
		await ToSignal(GetTree().CreateTimer(t), SceneTreeTimer.SignalName.Timeout);
	}

	async Task Oynat()
	{
		await Bekle(0.6f);
		foreach (var o in S.Olaylar)
		{
			if (!IsInsideTree()) return;
			switch (o.Tip)
			{
				case "tur":
					_turEtiket.Text = $"Tur {o.Deger:0}/{Savas.MaxTur}";
					await Bekle(0.15f);
					break;
				case "ulti":
				{
					var k = _kutular[o.Kaynak];
					_ultiEtiket.Text = $"{k.B.T.Ad}: {o.Metin}!";
					_ultiEtiket.Modulate = Colors.White;
					if (!Atla)
					{
						var tw = CreateTween();
						tw.TweenProperty(k, "scale", new Vector2(1.15f, 1.15f), 0.12f / Hiz);
						tw.TweenProperty(k, "scale", Vector2.One, 0.12f / Hiz);
						var tw2 = CreateTween();
						tw2.TweenInterval(0.9f / Hiz);
						tw2.TweenProperty(_ultiEtiket, "modulate:a", 0f, 0.3f / Hiz);
					}
					await Bekle(0.35f);
					break;
				}
				case "saldiri":
				{
					var s = _kutular[o.Kaynak]; var h = _kutular[o.Hedef];
					if (!Atla)
					{
						var bas = Konum(s.B);
						var yon = (Konum(h.B) - bas).Normalized() * (o.Metin == "ulti" ? 30 : 46);
						var tw = CreateTween();
						tw.TweenProperty(s, "position", bas + yon, 0.1f / Hiz);
						tw.TweenProperty(s, "position", bas, 0.12f / Hiz);
					}
					await Bekle(o.Metin == "ulti" ? 0.12f : 0.2f);
					break;
				}
				case "hasar":
				{
					var h = _kutular[o.Hedef];
					h.Can = o.KalanCan; h.Kalkan = o.KalanKalkan;
					h.Parlama = 1;
					var renk = o.Metin == "yanma" ? new Color("ff9a3a") : o.Kritik ? Ui.Altin : o.Ustunluk > 0 ? new Color("ff7b7b") : Colors.White;
					string m = $"-{o.Deger:0}" + (o.Kritik ? "!" : "");
					Yazi(h, m, renk, o.Kritik ? 34 : 26);
					if (!Atla)
					{
						var tw = CreateTween();
						tw.TweenMethod(Callable.From<float>(v => { h.Parlama = v; h.QueueRedraw(); }), 1f, 0f, 0.25f / Hiz);
					}
					else h.Parlama = 0;
					h.QueueRedraw();
					await Bekle(0.22f);
					break;
				}
				case "iyiles":
				{
					var h = _kutular[o.Hedef];
					h.Can = o.KalanCan; h.Kalkan = o.KalanKalkan;
					Yazi(h, $"+{o.Deger:0}", Ui.Yesil, 26);
					h.QueueRedraw();
					await Bekle(0.15f);
					break;
				}
				case "kalkan":
				{
					var h = _kutular[o.Hedef];
					h.Kalkan = o.KalanKalkan;
					Yazi(h, $"Kalkan {o.Deger:0}", new Color("9fdcff"), 22);
					h.QueueRedraw();
					await Bekle(0.12f);
					break;
				}
				case "durum":
					if (o.Hedef < 0) { _ultiEtiket.Text = o.Metin; _ultiEtiket.Modulate = Colors.White; await Bekle(0.8f); break; }
					Yazi(_kutular[o.Hedef], o.Metin, new Color("e7c4ff"), 22);
					_kutular[o.Hedef].QueueRedraw();
					await Bekle(0.12f);
					break;
				case "atla":
					Yazi(_kutular[o.Kaynak], o.Metin, Ui.Altin, 24);
					await Bekle(0.3f);
					break;
				case "enerji":
					_kutular[o.Hedef].Enerji = o.Deger;
					_kutular[o.Hedef].QueueRedraw();
					break;
				case "olum":
				{
					var h = _kutular[o.Hedef];
					h.Olu = true; h.Can = 0;
					h.QueueRedraw();
					if (!Atla)
					{
						var tw = CreateTween();
						tw.TweenProperty(h, "modulate:a", 0.55f, 0.3f / Hiz);
					}
					await Bekle(0.25f);
					break;
				}
				case "bitis":
					await Bekle(0.5f);
					Bitti?.Invoke(o.Deger == 0);
					return;
			}
		}
	}

	void Yazi(BirimKutu k, string metin, Color renk, int boyut)
	{
		if (Atla) return;
		var l = Ui.Etiket(metin, boyut, renk, HorizontalAlignment.Center);
		l.AddThemeConstantOverride("outline_size", 6);
		l.Size = new Vector2(200, 40);
		l.Position = k.Position + new Vector2(k.Size.X / 2 - 100, 20 + GD.Randf() * 30);
		l.MouseFilter = MouseFilterEnum.Ignore;
		_alan.AddChild(l);
		var tw = CreateTween().SetParallel();
		tw.TweenProperty(l, "position:y", l.Position.Y - 70, 0.8f / Hiz);
		tw.TweenProperty(l, "modulate:a", 0f, 0.8f / Hiz).SetDelay(0.35f / Hiz);
		tw.Chain().TweenCallback(Callable.From(l.QueueFree));
	}
}
