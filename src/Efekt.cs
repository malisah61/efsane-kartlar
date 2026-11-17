using System;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>Koddan üretilen savaş efektleri (parçacık, mermi, halka, yıldırım). Harici dosya gerektirmez.</summary>
public static class Efekt
{
	public static Color ElementRengi(string e) => e switch
	{
		"ates" => new Color("ff7a2a"), "su" => new Color("4fc3ff"), "doga" => new Color("7be05a"), _ => Colors.White
	};

	static Gradient Solma(Color c)
	{
		var g = new Gradient();
		g.SetColor(0, c);
		g.SetColor(1, new Color(c, 0));
		return g;
	}

	/// <summary>Tek seferlik parçacık patlaması.</summary>
	public static CPUParticles2D Patlama(Node ebeveyn, Vector2 konum, Color renk, int adet = 24, float hiz = 220, float boyut = 6,
		float omur = 0.5f, float yercekimi = 300, float yayilma = 180, Vector2? yon = null)
	{
		var p = new CPUParticles2D
		{
			Position = konum, Emitting = false, OneShot = true, Explosiveness = 0.95f, Amount = Math.Max(1, adet),
			Lifetime = omur, Direction = yon ?? Vector2.Up, Spread = yayilma,
			InitialVelocityMin = hiz * 0.4f, InitialVelocityMax = hiz, Gravity = new Vector2(0, yercekimi),
			ScaleAmountMin = boyut * 0.5f, ScaleAmountMax = boyut, ColorRamp = Solma(renk), ZIndex = 20,
			LocalCoords = false,
		};
		ebeveyn.AddChild(p);
		p.Emitting = true;
		p.GetTree().CreateTimer(omur + 0.3f).Timeout += p.QueueFree;
		return p;
	}

	/// <summary>Yukarı süzülen parçacıklar (iyileşme, güçlenme, aura).</summary>
	public static void Yukselen(Node ebeveyn, Vector2 konum, Color renk, int adet = 16, float genislik = 40)
	{
		var p = new CPUParticles2D
		{
			Position = konum, Emitting = false, OneShot = true, Explosiveness = 0.6f, Amount = adet, Lifetime = 0.9f,
			EmissionShape = CPUParticles2D.EmissionShapeEnum.Rectangle, EmissionRectExtents = new Vector2(genislik, 10),
			Direction = Vector2.Up, Spread = 15, InitialVelocityMin = 40, InitialVelocityMax = 110, Gravity = new Vector2(0, -60),
			ScaleAmountMin = 3, ScaleAmountMax = 7, ColorRamp = Solma(renk), ZIndex = 20, LocalCoords = false,
		};
		ebeveyn.AddChild(p);
		p.Emitting = true;
		p.GetTree().CreateTimer(1.3f).Timeout += p.QueueFree;
	}

	/// <summary>Genişleyen halka (kalkan, alay, sersemletme).</summary>
	public static void Halka(Node ebeveyn, Vector2 konum, Color renk, float r0 = 10, float r1 = 70, float sure = 0.45f, float kalinlik = 5)
	{
		var h = new HalkaDugum { Position = konum, Renk = renk, R = r0, Kalinlik = kalinlik, ZIndex = 19 };
		ebeveyn.AddChild(h);
		var tw = h.CreateTween().SetParallel();
		tw.TweenProperty(h, "R", r1, sure).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
		tw.TweenProperty(h, "modulate:a", 0f, sure);
		tw.Chain().TweenCallback(Callable.From(h.QueueFree));
	}

	/// <summary>Bir noktadan diğerine uçan enerji topu; vardığında tamamlanır.</summary>
	public static async Task Mermi(Node ebeveyn, Vector2 bas, Vector2 son, Color renk, float sure, float boyut = 9)
	{
		var m = new MermiDugum { Position = bas, Renk = renk, Boyut = boyut, ZIndex = 21 };
		ebeveyn.AddChild(m);
		var iz = new CPUParticles2D
		{
			Amount = 18, Lifetime = 0.25f, Gravity = Vector2.Zero, InitialVelocityMin = 0, InitialVelocityMax = 15, Spread = 180,
			ScaleAmountMin = boyut * 0.3f, ScaleAmountMax = boyut * 0.7f, ColorRamp = Solma(renk), LocalCoords = false,
		};
		m.AddChild(iz);
		var tw = m.CreateTween();
		var orta = (bas + son) / 2 + new Vector2(0, -60);
		tw.TweenMethod(Callable.From<float>(t =>
		{
			// hafif kavisli yol
			var a = bas.Lerp(orta, t); var b = orta.Lerp(son, t);
			m.Position = a.Lerp(b, t);
		}), 0f, 1f, Math.Max(0.05f, sure));
		await m.ToSignal(tw, Tween.SignalName.Finished);
		iz.Emitting = false;
		m.Renk = new Color(0, 0, 0, 0);
		m.GetTree().CreateTimer(0.3f).Timeout += m.QueueFree;
	}

	/// <summary>Gökten inen yıldırım.</summary>
	public static void Yildirim(Node ebeveyn, Vector2 hedef, Color renk)
	{
		var y = new YildirimDugum { Hedef = hedef, Renk = renk, ZIndex = 22 };
		ebeveyn.AddChild(y);
		var tw = y.CreateTween();
		tw.TweenProperty(y, "modulate:a", 0f, 0.35f).SetDelay(0.08f);
		tw.TweenCallback(Callable.From(y.QueueFree));
		Patlama(ebeveyn, hedef, new Color("fff7a8"), 20, 260, 6, 0.4f, 200);
	}

	/// <summary>Kılıç savuruşu izi.</summary>
	public static void Kesik(Node ebeveyn, Vector2 konum, Color renk, bool sagaDogru)
	{
		var k = new KesikDugum { Position = konum, Renk = renk, Yon = sagaDogru ? 1 : -1, ZIndex = 21 };
		ebeveyn.AddChild(k);
		var tw = k.CreateTween().SetParallel();
		tw.TweenProperty(k, "Ilerleme", 1f, 0.18f);
		tw.TweenProperty(k, "modulate:a", 0f, 0.25f).SetDelay(0.1f);
		tw.Chain().TweenCallback(Callable.From(k.QueueFree));
	}

	/// <summary>Düğümü kısa süre sarsar.</summary>
	public static void Sarsinti(Node2D d, float guc = 8, float sure = 0.25f)
	{
		var bas = d.Position;
		var tw = d.CreateTween();
		int n = 6;
		for (int i = 0; i < n; i++)
		{
			float k = guc * (1f - i / (float)n);
			tw.TweenProperty(d, "position", bas + new Vector2((float)GD.RandRange(-k, k), (float)GD.RandRange(-k, k)), sure / n);
		}
		tw.TweenProperty(d, "position", bas, sure / n);
	}
}

public partial class HalkaDugum : Node2D
{
	[Export] public float R { get => _r; set { _r = value; QueueRedraw(); } }
	float _r;
	public Color Renk; public float Kalinlik = 5;
	public override void _Draw()
	{
		DrawArc(Vector2.Zero, _r, 0, Mathf.Tau, 40, Renk, Kalinlik);
		DrawCircle(Vector2.Zero, _r, new Color(Renk, 0.12f));
	}
}

public partial class MermiDugum : Node2D
{
	public Color Renk; public float Boyut = 9;
	public override void _Process(double delta) => QueueRedraw();
	public override void _Draw()
	{
		if (Renk.A <= 0) return;
		DrawCircle(Vector2.Zero, Boyut * 1.6f, new Color(Renk, 0.25f));
		DrawCircle(Vector2.Zero, Boyut, Renk);
		DrawCircle(Vector2.Zero, Boyut * 0.45f, Colors.White);
	}
}

public partial class YildirimDugum : Node2D
{
	public Vector2 Hedef; public Color Renk;
	public override void _Draw()
	{
		var r = new Random();
		var p = Hedef + new Vector2(0, -700);
		var noktalar = new System.Collections.Generic.List<Vector2> { p };
		for (int i = 1; i <= 9; i++)
			noktalar.Add(new Vector2(Hedef.X + (i == 9 ? 0 : r.Next(-30, 30)), p.Y + (Hedef.Y - p.Y) * i / 9f));
		var dizi = noktalar.ToArray();
		DrawPolyline(dizi, new Color(Renk, 0.5f), 14);
		DrawPolyline(dizi, Renk, 6);
		DrawPolyline(dizi, Colors.White, 2);
	}
}

public partial class KesikDugum : Node2D
{
	[Export] public float Ilerleme { get => _i; set { _i = value; QueueRedraw(); } }
	float _i;
	public Color Renk; public int Yon = 1;
	public override void _Draw()
	{
		float bas = -1.2f, son = bas + 2.2f * _i;
		var s = new Vector2(Yon, 1);
		var pts = new System.Collections.Generic.List<Vector2>();
		for (int i = 0; i <= 12; i++)
		{
			float a = Mathf.Lerp(bas, son, i / 12f);
			pts.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 46 * s);
		}
		if (pts.Count > 1)
		{
			DrawPolyline(pts.ToArray(), new Color(Renk, 0.5f), 10);
			DrawPolyline(pts.ToArray(), Colors.White, 3);
		}
	}
}
