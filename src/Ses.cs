using System.Collections.Generic;
using Godot;

namespace EfsaneKartlar;

/// <summary>
/// Müzik, efekt ve karakter sesleri. Oyun açılışta tek bir örnek ekler; her yerden statik çağrılır.
/// Dosyalar: assets/ses/muzik/*.ogg (döngü), assets/ses/efekt/*.ogg, assets/ses/ses/&lt;kart id&gt;.ogg (ulti repliği).
/// </summary>
public partial class Ses : Node
{
	static Ses _ben;
	AudioStreamPlayer _muzik, _konusma;
	readonly List<AudioStreamPlayer> _havuz = new();
	readonly Dictionary<string, AudioStream> _onbellek = new();
	readonly Dictionary<string, double> _sonCalma = new();
	string _calanMuzik = "";

	public static bool MuzikAcik = true, EfektAcik = true;

	/// <summary>Kadın karakterler (ölüm sesi için).</summary>
	static readonly HashSet<string> Kadinlar = new()
	{
		"asena", "umay", "sahmeran", "su_iyesi", "athena", "medusa", "amazon", "siren", "nimf",
		"freya", "valkur", "idun", "volva", "isis", "bastet", "ankh_rahibesi", "sekhmet",
		"al_karisi", "artemis", "hel", "hathor", "amaterasu", "kitsune", "yuki_onna", "miko",
		"nuwa", "change",
	};
	static readonly HashSet<string> Canavarlar = new() { "tepegoz", "minotor", "ymir", "sobek", "mumya", "sfenks", "akrep", "fenrir", "kerberos", "ryujin", "long", "qilin" };

	public static void Kur(Node ebeveyn)
	{
		if (_ben != null) return;
		_ben = new Ses { Name = "Ses" };
		ebeveyn.AddChild(_ben);
	}

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		_muzik = new AudioStreamPlayer { VolumeDb = -8 };
		_konusma = new AudioStreamPlayer { VolumeDb = 0 };
		AddChild(_muzik); AddChild(_konusma);
		for (int i = 0; i < 10; i++) { var p = new AudioStreamPlayer { VolumeDb = -4 }; _havuz.Add(p); AddChild(p); }
	}

	AudioStream Yukle(string yol)
	{
		if (_onbellek.TryGetValue(yol, out var s)) return s;
		s = ResourceLoader.Exists(yol) ? GD.Load<AudioStream>(yol) : null;
		_onbellek[yol] = s;
		return s;
	}

	// ------------------------------------------------------------ müzik
	public static void Muzik(string ad)
	{
		if (_ben == null) return;
		_ben.MuzikCal(ad);
	}

	void MuzikCal(string ad)
	{
		if (!MuzikAcik) { _muzik.Stop(); _calanMuzik = ad; return; }
		if (_calanMuzik == ad && _muzik.Playing) return;
		var s = Yukle($"res://assets/ses/muzik/{ad}.ogg");
		_calanMuzik = ad;
		if (s == null) { _muzik.Stop(); return; }
		if (s is AudioStreamOggVorbis ogg) ogg.Loop = true;
		var tw = CreateTween();
		if (_muzik.Playing) tw.TweenProperty(_muzik, "volume_db", -40f, 0.4f);
		tw.TweenCallback(Callable.From(() => { _muzik.Stream = s; _muzik.VolumeDb = -30; _muzik.Play(); }));
		tw.TweenProperty(_muzik, "volume_db", -8f, 0.8f);
	}

	public static void MuzikAyarla(bool acik)
	{
		MuzikAcik = acik;
		if (_ben == null) return;
		if (!acik) _ben._muzik.Stop();
		else { var ad = _ben._calanMuzik; _ben._calanMuzik = ""; _ben.MuzikCal(ad == "" ? "menu" : ad); }
	}

	// ------------------------------------------------------------ efekt
	/// <summary>Kısa efekt çalar. aralik: aynı efektin en az kaç saniye arayla çalacağı (üst üste binmeyi önler).</summary>
	public static void Efekt(string ad, float db = 0, float perde = 1, float aralik = 0.06f)
	{
		if (_ben == null || !EfektAcik) return;
		double simdi = Time.GetTicksMsec() / 1000.0;
		if (_ben._sonCalma.TryGetValue(ad, out var son) && simdi - son < aralik) return;
		_ben._sonCalma[ad] = simdi;
		var s = _ben.Yukle($"res://assets/ses/efekt/{ad}.ogg");
		if (s == null) return;
		AudioStreamPlayer bos = null;
		foreach (var p in _ben._havuz) if (!p.Playing) { bos = p; break; }
		bos ??= _ben._havuz[0];
		bos.Stream = s;
		bos.VolumeDb = -4 + db;
		bos.PitchScale = perde * (float)GD.RandRange(0.95, 1.05);
		bos.Play();
	}

	/// <summary>Karakter repliği (ulti) veya anons. Bir önceki konuşmayı keser.</summary>
	public static void Konus(string ad, float perde = 1)
	{
		if (_ben == null || !EfektAcik) return;
		var s = _ben.Yukle($"res://assets/ses/ses/{ad}.ogg");
		if (s == null) return;
		_ben._konusma.Stream = s;
		_ben._konusma.PitchScale = perde;
		_ben._konusma.Play();
	}

	public static void Olum(string kartId, float perde = 1)
	{
		if (Canavarlar.Contains(kartId)) { Efekt("dusus", 2, 0.8f); return; }
		Efekt("dusus", -2);
		if (_ben == null || !EfektAcik) return;
		var s = _ben.Yukle($"res://assets/ses/ses/{(Kadinlar.Contains(kartId) ? "olum_kadin" : "olum_erkek")}.ogg");
		if (s == null) return;
		AudioStreamPlayer bos = null;
		foreach (var p in _ben._havuz) if (!p.Playing) { bos = p; break; }
		bos ??= _ben._havuz[1];
		bos.Stream = s; bos.VolumeDb = -3; bos.PitchScale = perde * (float)GD.RandRange(0.92, 1.08);
		bos.Play();
	}

	public static string ElementEfekti(string element) => element switch { "ates" => "ates", "su" => "su", _ => "doga" };
}
