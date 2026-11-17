using System;
using Godot;

namespace EfsaneKartlar;

/// <summary>
/// AdMob reklam köprüsü (godot-sdk-integrations/godot-admob 7.x eklentisinin Admob düğümü).
/// Eklenti yalnızca Google Play (gradle) sürümünde vardır. Yoksa:
///  - ödüllü reklam: ödül "deneme sürümü" olarak hemen verilir (geliştirme/yan yükleme APK'sı),
///  - geçiş reklamı: atlanır.
/// "Reklamları Kaldır" (VIP) alanlar geçiş reklamı görmez, ödüllü reklamların ödülünü izlemeden alır.
/// </summary>
public partial class Reklam : Node
{
	// ---- AdMob kimlikleri: AdMob hesabı açılınca gerçek kimlikleri buraya yaz ve Gercek = true yap.
	// Şu an Google'ın resmî TEST kimlikleri kullanılıyor (gerçek reklam/para yok, hesap riski yok).
	public const bool Gercek = false;
	public const string GecisId = "";        // ör. ca-app-pub-XXXXXXXX/YYYYYYYY
	public const string OdulluId = "";
	public const int GecisAraligi = 7;       // her 7 savaşta bir geçiş reklamı
	public const int GunlukReklamsizSavas = 10;   // günün ilk 10 savaşında geçiş reklamı yok

	const string Yol = "res://addons/AdmobPlugin/Admob.gd";
	static Reklam _ben;
	Node _admob;
	bool _gecisYuklu, _odulluYuklu, _odulKazanildi;
	Action _odulCb, _gecisCb;

	public static bool EklentiVar => _ben?._admob != null;
	public static Action<string> Bilgi;

	public static void Kur(Node ebeveyn)
	{
		if (_ben != null) return;
		_ben = new Reklam { Name = "Reklam" };
		ebeveyn.AddChild(_ben);
	}

	public override void _Ready()
	{
		if (!OS.HasFeature("android") || !ResourceLoader.Exists(Yol)) { GD.Print("Reklam eklentisi yok."); return; }
		try
		{
			var betik = GD.Load<Script>(Yol);
			var n = new Node { Name = "Admob" };
			n.SetScript(betik);
			n.Set("is_real", Gercek);
			if (Gercek)
			{
				n.Set("android_real_interstitial_id", GecisId);
				n.Set("android_real_rewarded_id", OdulluId);
			}
			AddChild(n);
			_admob = n;
			n.Connect("initialization_completed", Callable.From<GodotObject>(_ => { Yukle(true); Yukle(false); }));
			n.Connect("interstitial_ad_loaded", Callable.From<GodotObject, GodotObject>((_, _) => _gecisYuklu = true));
			n.Connect("interstitial_ad_failed_to_load", Callable.From<GodotObject, GodotObject>((_, _) => { _gecisYuklu = false; SonraYukle(true); }));
			n.Connect("interstitial_ad_failed_to_show_full_screen_content", Callable.From<GodotObject, GodotObject>((_, _) => GecisBitti()));
			n.Connect("interstitial_ad_dismissed_full_screen_content", Callable.From<GodotObject>(_ => GecisBitti()));
			n.Connect("rewarded_ad_loaded", Callable.From<GodotObject, GodotObject>((_, _) => _odulluYuklu = true));
			n.Connect("rewarded_ad_failed_to_load", Callable.From<GodotObject, GodotObject>((_, _) => { _odulluYuklu = false; SonraYukle(false); }));
			n.Connect("rewarded_ad_user_earned_reward", Callable.From<GodotObject, GodotObject>((_, _) => _odulKazanildi = true));
			n.Connect("rewarded_ad_failed_to_show_full_screen_content", Callable.From<GodotObject, GodotObject>((_, _) => OdulluBitti()));
			n.Connect("rewarded_ad_dismissed_full_screen_content", Callable.From<GodotObject>(_ => OdulluBitti()));
			n.Call("initialize");
		}
		catch (Exception e) { GD.PrintErr("Reklam başlatılamadı: " + e.Message); _admob = null; }
	}

	void Yukle(bool gecis)
	{
		if (_admob == null) return;
		if (gecis) { if (!Kayit.P.Reklamsiz) _admob.Call("load_interstitial_ad"); }
		else _admob.Call("load_rewarded_ad");
	}

	void SonraYukle(bool gecis) => GetTree().CreateTimer(30).Timeout += () => Yukle(gecis);

	void GecisBitti()
	{
		_gecisYuklu = false;
		var cb = _gecisCb; _gecisCb = null;
		cb?.Invoke();
		Yukle(true);
	}

	void OdulluBitti()
	{
		_odulluYuklu = false;
		var cb = _odulCb; _odulCb = null;
		if (_odulKazanildi) cb?.Invoke();
		else Bilgi?.Invoke("Ödül için reklamı sonuna kadar izlemelisin.");
		_odulKazanildi = false;
		Yukle(false);
	}

	/// <summary>Ödüllü reklam: izlenirse odul çağrılır. VIP veya eklentisiz sürümde hemen çağrılır.</summary>
	public static void Odullu(Action odul)
	{
		if (Kayit.P.Reklamsiz || _ben?._admob == null) { odul(); return; }
		if (!_ben._odulluYuklu) { Bilgi?.Invoke("Reklam hazırlanıyor, birkaç saniye sonra tekrar dene."); _ben.Yukle(false); return; }
		_ben._odulKazanildi = false;
		_ben._odulCb = odul;
		_ben._admob.Call("show_rewarded_ad", "");
	}

	/// <summary>Savaş bittiğinde çağrılır; her GecisAraligi savaşta bir geçiş reklamı gösterir, sonra devam eder.</summary>
	public static void SavasSonrasi(Action devam)
	{
		var p = Kayit.P;
		p.SavasSayaci++;
		bool goster = !p.Reklamsiz && p.GunlukSavas > GunlukReklamsizSavas && p.SavasSayaci % GecisAraligi == 0 && _ben?._admob != null && _ben._gecisYuklu;
		if (!goster) { devam(); return; }
		_ben._gecisCb = devam;
		_ben._admob.Call("show_interstitial_ad", "");
	}

	/// <summary>Sıradaki geçiş reklamına kaç savaş kaldı (VIP için -1).</summary>
	public static int GeciseKalan => Kayit.P.Reklamsiz || Kayit.P.GunlukSavas < GunlukReklamsizSavas ? -1 : GecisAraligi - Kayit.P.SavasSayaci % GecisAraligi;
}
