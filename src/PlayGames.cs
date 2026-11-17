using System;
using Godot;

namespace EfsaneKartlar;

/// <summary>
/// Google Play Games (v2) ile otomatik giriş. Eklenti (godot-play-game-services) yalnızca Google Play sürümünde vardır.
/// Akış: oyun açılınca Play Games oturumu kontrol edilir → sunucu yetki kodu istenir →
/// sunucu (play_giris) kodu Google'da doğrular ve oyuncunun hesabını bulur/oluşturur → oturum açılır.
/// Oyuncu hiçbir şey yazmaz; telefonu değişse de aynı Google hesabıyla ilerlemesi geri gelir.
/// </summary>
public partial class PlayGames : Node
{
	// Play Console → Play Games Services → Yapılandırma → Kimlik bilgileri → "Oyun sunucusu" (web) OAuth istemci kimliği.
	// Boşsa Play Games girişi kapalıdır.
	public const string WebIstemciKimligi = "";

	const string Eklenti = "GodotPlayGameServices";
	static PlayGames _ben;
	GodotObject _e;
	bool _kodIstendi;
	public static bool Var => _ben?._e != null && WebIstemciKimligi != "";
	public static Action<string> Bilgi;

	public static void Kur(Node ebeveyn)
	{
		if (_ben != null) return;
		_ben = new PlayGames { Name = "PlayGames" };
		ebeveyn.AddChild(_ben);
	}

	public override void _Ready()
	{
		if (!Engine.HasSingleton(Eklenti) || WebIstemciKimligi == "") return;
		try
		{
			_e = Engine.GetSingleton(Eklenti);
			_e.Call("initialize");
			_e.Connect("userAuthenticated", Callable.From<bool>(Dogrulandi));
			_e.Connect("serverSideAccessRequested", Callable.From<string>(KodGeldi));
			// v2: oyun açılışında Play Games otomatik oturum açar; sonucu soruyoruz
			if (!Sunucu.OturumVar) _e.Call("isAuthenticated");
		}
		catch (Exception ex) { GD.PrintErr("Play Games başlatılamadı: " + ex.Message); _e = null; }
	}

	void Dogrulandi(bool tamam)
	{
		if (tamam && !Sunucu.Girisli && !_kodIstendi)
		{
			_kodIstendi = true;
			_e.Call("requestServerSideAccess", WebIstemciKimligi, false);
		}
	}

	async void KodGeldi(string kod)
	{
		_kodIstendi = false;
		if (string.IsNullOrEmpty(kod)) { Bilgi?.Invoke("Google Play Games girişi başarısız"); return; }
		try { await Sunucu.PlayGamesGiris(kod); Bilgi?.Invoke($"Google Play Games ile giriş yapıldı: {Sunucu.KullaniciAdi}"); }
		catch (Exception ex) { Bilgi?.Invoke(ex.Message); }
	}

	/// <summary>"Google Play Games ile giriş" düğmesi: gerekirse Play Games oturum ekranını açar.</summary>
	public static void GirisYap()
	{
		if (!Var) { Bilgi?.Invoke("Google Play Games girişi yalnızca Google Play'den indirilen sürümde çalışır."); return; }
		_ben._kodIstendi = false;
		_ben._e.Call("signIn");
	}
}
