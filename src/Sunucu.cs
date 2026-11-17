using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

public class SunucuHatasi : Exception { public SunucuHatasi(string m) : base(m) { } }

/// <summary>
/// Çevrimiçi sunucu (Supabase) istemcisi: hesap, oturum, bulut kaydı ve tüm sosyal özellikler.
/// Bütün yazma işlemleri sunucudaki güvenli fonksiyonlarla (RPC) yapılır; tablolar doğrudan yazılamaz.
/// İnternet yoksa veya giriş yapılmamışsa oyun tamamen çevrimdışı çalışmaya devam eder.
/// </summary>
public partial class Sunucu : Node
{
	public const string Url = "https://gqswczthqabfgiwtfege.supabase.co";
	// Herkese açık (yayımlanabilir) anahtar; gizli değildir, güvenlik sunucudaki kurallarla sağlanır.
	public const string Anahtar = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imdxc3djenRocWFiZmdpd3RmZWdlIiwicm9sZSI6ImFub24iLCJpYXQiOjE3OTQ5MTk1ODEsImV4cCI6MjExMDQ5NTU4MX0.sq9cZ1T8XV1gLzM3lFAKniNk1bMPKuD5ICTG2nZeLrY";
	const string OturumYolu = "user://oturum.json";

	static Sunucu _ben;
	static string _erisim = "", _yenileme = "";
	static double _bitis;   // erişim jetonunun bitiş zamanı (unix sn)
	public static string OyuncuId { get; private set; } = "";
	public static string KullaniciAdi { get; private set; } = "";
	public static Godot.Collections.Dictionary Ben { get; private set; } = new();
	public static bool Girisli => _erisim != "" && OyuncuId != "";
	/// <summary>Cihazda kayıtlı bir oturum var (giriş henüz tamamlanmamış olabilir).</summary>
	public static bool OturumVar => Girisli || (FileAccess.FileExists(OturumYolu) && FileAccess.GetFileAsString(OturumYolu).Contains("\"r\":\"") && !FileAccess.GetFileAsString(OturumYolu).Contains("\"r\":\"\""));
	public static Action Degisti;      // giriş/çıkış/profil değişince arayüz yenilensin

	double _kayitSayac = -1;          // bulut kaydı için gecikmeli gönderim

	public static void Kur(Node ebeveyn)
	{
		if (_ben != null) return;
		_ben = new Sunucu { Name = "Sunucu" };
		ebeveyn.AddChild(_ben);
	}

	public override void _Ready()
	{
		Kayit.Degisti += () => { if (Girisli && _kayitSayac < 0) _kayitSayac = 20; };
		_ = OturumuYukle();
	}

	public override void _Process(double delta)
	{
		if (_kayitSayac < 0) return;
		_kayitSayac -= delta;
		if (_kayitSayac <= 0) { _kayitSayac = -1; _ = BulutaGonder(); }
	}

	public override void _Notification(int what)
	{
		if (what == NotificationApplicationPaused || what == NotificationWMCloseRequest)
			if (Girisli && _kayitSayac >= 0) { _kayitSayac = -1; _ = BulutaGonder(); }
	}

	// ------------------------------------------------------------ HTTP
	async Task<(int kod, string govde)> Istek(string yol, HttpClient.Method yontem, string govde, bool yetkili)
	{
		var h = new HttpRequest { Timeout = 20 };
		AddChild(h);
		var basliklar = new[]
		{
			"Content-Type: application/json",
			"apikey: " + Anahtar,
			"Authorization: Bearer " + (yetkili && _erisim != "" ? _erisim : Anahtar),
		};
		var hata = h.Request(Url + yol, basliklar, yontem, govde ?? "");
		if (hata != Error.Ok) { h.QueueFree(); return (0, ""); }
		var r = await ToSignal(h, HttpRequest.SignalName.RequestCompleted);
		h.QueueFree();
		int sonuc = r[0].AsInt32();
		if (sonuc != (int)HttpRequest.Result.Success) return (0, "");
		return (r[1].AsInt32(), Encoding.UTF8.GetString(r[3].AsByteArray()));
	}

	static string Metin(Variant v) => v.VariantType == Variant.Type.Nil ? "" : v.AsString();

	static string HataCevir(string govde, int kod)
	{
		if (kod == 0) return "Sunucuya bağlanılamadı. İnternet bağlantını kontrol et.";
		string m = govde;
		try
		{
			var d = Json.ParseString(govde);
			if (d.VariantType == Variant.Type.Dictionary)
			{
				var s = d.AsGodotDictionary();
				foreach (var a in new[] { "hata", "message", "msg", "error_description", "error" })
					if (s.ContainsKey(a)) { m = Metin(s[a]); break; }
			}
		}
		catch { }
		var tablo = new Dictionary<string, string>
		{
			["gunluk arena siniri"] = "Bugünlük arena sınırına ulaştın.",
			["gunluk ikili siniri"] = "Bugünlük ikili savaş sınırına ulaştın.",
			["rakip yok"] = "Rakip bulunamadı.",
			["zaten loncadasin"] = "Zaten bir loncadasın.",
			["kupa yetersiz"] = "Bu loncaya katılmak için kupan yetersiz.",
			["lonca dolu"] = "Lonca dolu (30 üye).",
			["lonca yok"] = "Lonca bulunamadı.",
			["yetki yok"] = "Bunu yapmaya yetkin yok.",
			["loncadasin degilsin"] = "Bir loncada değilsin.",
			["loncada degilsin"] = "Bir loncada değilsin.",
			["yalnizca arkadaslara"] = "Yalnızca arkadaşlarına mesaj atabilirsin.",
			["yalnizca arkadaslar"] = "Yalnızca arkadaşlarını davet edebilirsin.",
			["gonderilemedi"] = "Gönderilemedi.",
			["cok hizli"] = "Çok hızlı yazıyorsun, biraz bekle.",
			["bos mesaj"] = "Boş mesaj gönderilemez.",
			["zaten takimin var"] = "Zaten bir ikili takımın var.",
			["takimin yok"] = "Önce bir ikili takım kur.",
			["kayit cok buyuk"] = "Kayıt çok büyük.",
			["ad haftada bir"] = "Kullanıcı adını haftada bir değiştirebilirsin.",
			["ad alinmis"] = "Bu kullanıcı adı alınmış ya da geçersiz (3-16 harf/rakam).",
			["User already registered"] = "Bu e-posta ile zaten bir hesap var.",
			["Database error saving new user"] = "Bu kullanıcı adı alınmış, başka bir ad dene.",
			["duplicate key"] = "Bu ad zaten alınmış.",
			["Password should be at least 6 characters"] = "Şifre en az 6 karakter olmalı.",
			["Unable to validate email address: invalid format"] = "E-posta adresi geçersiz.",
			["Email rate limit exceeded"] = "Çok fazla deneme yapıldı, biraz sonra tekrar dene.",
			["email rate limit exceeded"] = "Çok fazla deneme yapıldı, biraz sonra tekrar dene.",
		};
		foreach (var (k, v) in tablo) if (m.Contains(k, StringComparison.OrdinalIgnoreCase)) return v;
		if (kod == 401) return "Oturumun sona erdi, tekrar giriş yap.";
		return string.IsNullOrWhiteSpace(m) ? $"Sunucu hatası ({kod})" : m;
	}

	/// <summary>Sunucu fonksiyonu çağırır. Hata olursa Türkçe mesajlı SunucuHatasi fırlatır.</summary>
	public static async Task<Variant> Rpc(string ad, Godot.Collections.Dictionary arg = null)
	{
		if (_ben == null) throw new SunucuHatasi("Sunucu hazır değil");
		await JetonTazele();
		string govde = Json.Stringify(arg ?? new Godot.Collections.Dictionary());
		var (kod, metin) = await _ben.Istek("/rest/v1/rpc/" + ad, HttpClient.Method.Post, govde, true);
		if (kod == 401 && await Yenile()) (kod, metin) = await _ben.Istek("/rest/v1/rpc/" + ad, HttpClient.Method.Post, govde, true);
		if (kod < 200 || kod >= 300) throw new SunucuHatasi(HataCevir(metin, kod));
		if (string.IsNullOrWhiteSpace(metin)) return default;
		return Json.ParseString(metin);
	}

	public static async Task<Godot.Collections.Array> Liste(string ad, Godot.Collections.Dictionary arg = null)
	{
		var v = await Rpc(ad, arg);
		return v.VariantType == Variant.Type.Array ? v.AsGodotArray() : new Godot.Collections.Array();
	}

	public static async Task<Godot.Collections.Dictionary> Sozluk(string ad, Godot.Collections.Dictionary arg = null)
	{
		var v = await Rpc(ad, arg);
		return v.VariantType == Variant.Type.Dictionary ? v.AsGodotDictionary() : null;
	}

	// ------------------------------------------------------------ oturum
	static double Simdi() => Time.GetUnixTimeFromSystem();

	static void OturumKaydet()
	{
		try
		{
			using var f = FileAccess.Open(OturumYolu, FileAccess.ModeFlags.Write);
			f?.StoreString(Json.Stringify(new Godot.Collections.Dictionary { ["r"] = _yenileme, ["id"] = OyuncuId, ["ad"] = KullaniciAdi }));
		}
		catch (Exception e) { GD.PrintErr("Oturum yazılamadı: " + e.Message); }
	}

	void JetonlariAl(Godot.Collections.Dictionary d)
	{
		_erisim = Metin(d["access_token"]);
		_yenileme = Metin(d["refresh_token"]);
		_bitis = Simdi() + (d.ContainsKey("expires_in") ? d["expires_in"].AsDouble() : 3600) - 60;
		if (d.ContainsKey("user_id")) OyuncuId = Metin(d["user_id"]);
		else if (d.ContainsKey("user") && d["user"].VariantType == Variant.Type.Dictionary) OyuncuId = Metin(d["user"].AsGodotDictionary()["id"]);
		OturumKaydet();
	}

	async Task OturumuYukle()
	{
		if (!FileAccess.FileExists(OturumYolu)) return;
		try
		{
			var d = Json.ParseString(FileAccess.GetFileAsString(OturumYolu)).AsGodotDictionary();
			_yenileme = Metin(d["r"]);
			OyuncuId = d.ContainsKey("id") ? Metin(d["id"]) : "";
			KullaniciAdi = d.ContainsKey("ad") ? Metin(d["ad"]) : "";
		}
		catch { return; }
		if (_yenileme == "") return;
		if (await Yenile()) await GirisSonrasi(false);
	}

	static async Task<bool> Yenile()
	{
		if (_ben == null || _yenileme == "") return false;
		var (kod, metin) = await _ben.Istek("/auth/v1/token?grant_type=refresh_token", HttpClient.Method.Post,
			Json.Stringify(new Godot.Collections.Dictionary { ["refresh_token"] = _yenileme }), false);
		if (kod == 200) { _ben.JetonlariAl(Json.ParseString(metin).AsGodotDictionary()); return true; }
		if (kod >= 400 && kod < 500) { _erisim = ""; OyuncuId = ""; _yenileme = ""; OturumKaydet(); Degisti?.Invoke(); }
		return false;
	}

	static async Task JetonTazele()
	{
		if (_erisim != "" && Simdi() > _bitis) await Yenile();
	}

	/// <summary>Kayıt ol. Dönüş: true = hemen giriş yapıldı, false = e-posta onayı bekleniyor.</summary>
	public static async Task<bool> KayitOl(string eposta, string sifre, string ad)
	{
		var musait = await Rpc("kullanici_adi_musait", new() { ["ad"] = ad });
		if (!musait.AsBool()) throw new SunucuHatasi("Bu kullanıcı adı alınmış ya da geçersiz (3-16 harf/rakam).");
		var (kod, metin) = await _ben.Istek("/auth/v1/signup", HttpClient.Method.Post, Json.Stringify(new Godot.Collections.Dictionary
		{
			["email"] = eposta, ["password"] = sifre, ["data"] = new Godot.Collections.Dictionary { ["kullanici_adi"] = ad },
		}), false);
		if (kod < 200 || kod >= 300) throw new SunucuHatasi(HataCevir(metin, kod));
		var d = Json.ParseString(metin).AsGodotDictionary();
		if (d.ContainsKey("access_token") && Metin(d["access_token"]) != "")
		{
			_ben.JetonlariAl(d);
			KullaniciAdi = ad;
			await _ben.GirisSonrasi(true);
			return true;
		}
		return false;
	}

	/// <summary>Kullanıcı adı veya e-posta ile giriş.</summary>
	public static async Task GirisYap(string kimlik, string sifre)
	{
		var (kod, metin) = await _ben.Istek("/functions/v1/giris", HttpClient.Method.Post,
			Json.Stringify(new Godot.Collections.Dictionary { ["kimlik"] = kimlik, ["sifre"] = sifre }), false);
		if (kod != 200) throw new SunucuHatasi(HataCevir(metin, kod));
		_ben.JetonlariAl(Json.ParseString(metin).AsGodotDictionary());
		await _ben.GirisSonrasi(true);
	}

	/// <summary>Google Play Games sunucu yetki koduyla giriş (hesap yoksa sunucu oluşturur).</summary>
	public static async Task PlayGamesGiris(string kod)
	{
		var (k, metin) = await _ben.Istek("/functions/v1/play_giris", HttpClient.Method.Post,
			Json.Stringify(new Godot.Collections.Dictionary { ["kod"] = kod }), false);
		if (k != 200) throw new SunucuHatasi(HataCevir(metin, k));
		_ben.JetonlariAl(Json.ParseString(metin).AsGodotDictionary());
		await _ben.GirisSonrasi(true);
	}

	/// <summary>Hesabı ve sunucudaki tüm verileri kalıcı olarak siler.</summary>
	public static async Task HesabiSil()
	{
		await JetonTazele();
		var (k, metin) = await _ben.Istek("/functions/v1/hesap_sil", HttpClient.Method.Post, "{}", true);
		if (k != 200) throw new SunucuHatasi(HataCevir(metin, k));
		Cikis();
	}

	public static async Task SifreSifirla(string eposta)
	{
		var (kod, metin) = await _ben.Istek("/auth/v1/recover", HttpClient.Method.Post,
			Json.Stringify(new Godot.Collections.Dictionary { ["email"] = eposta }), false);
		if (kod < 200 || kod >= 300) throw new SunucuHatasi(HataCevir(metin, kod));
	}

	public static void Cikis()
	{
		if (_ben != null && _erisim != "") _ = _ben.Istek("/auth/v1/logout", HttpClient.Method.Post, "{}", true);
		_erisim = ""; _yenileme = ""; OyuncuId = ""; KullaniciAdi = ""; Ben = new();
		OturumKaydet();
		Degisti?.Invoke();
	}

	/// <summary>Bulut kaydı karşılaştırması gerektiğinde arayüze sorulur: (bulut özeti, cihaz özeti) → true = buluttakini al.</summary>
	public static Func<string, string, Task<bool>> KayitSecimi;

	async Task GirisSonrasi(bool yeniGiris)
	{
		try
		{
			await BenYenile();
			if (yeniGiris)
			{
				var b = await Sozluk("kayit_oku");
				if (b != null && b.ContainsKey("veri"))
				{
					string bulut = Json.Stringify(b["veri"]);
					var bp = Kayit.MetindenProfil(bulut);
					bool cihazBos = Kayit.P.EnIyiAsama <= 1 && Kayit.P.ToplamHarcamaKurus == 0;
					bool al = cihazBos;
					if (!cihazBos && bp != null && KayitSecimi != null)
						al = await KayitSecimi(Kayit.Ozet(bp), Kayit.Ozet(Kayit.P));
					if (al && bp != null) Kayit.ProfiliDegistir(bp);
				}
			}
			if (Ben.ContainsKey("kupa")) { Kayit.P.Kupa = Ben["kupa"].AsInt32(); }
			Kayit.P.Ad = KullaniciAdi;
			Kayit.Kaydet();
			await BulutaGonder();
		}
		catch (Exception e) { GD.PrintErr("Giriş sonrası: " + e.Message); }
		Degisti?.Invoke();
	}

	public static async Task BenYenile()
	{
		var d = await Sozluk("ben");
		if (d == null) return;
		Ben = d;
		KullaniciAdi = Metin(d["kullanici_adi"]);
		OturumKaydet();
	}

	public static Godot.Collections.Array SavunmaJson()
	{
		var a = new Godot.Collections.Array();
		foreach (var u in Kayit.TakimUyeleri().Where(u => u != null))
			a.Add(new Godot.Collections.Dictionary { ["id"] = u.Id, ["sv"] = u.Seviye, ["y"] = u.Yildiz });
		return a;
	}

	public static async Task BulutaGonder()
	{
		if (!Girisli || Kayit.KayitKapali) return;
		try
		{
			await Rpc("kayit_yaz", new() { ["p_veri"] = Json.ParseString(Kayit.Metin()) });
			await Rpc("profil_guncelle", new() { ["p_savunma"] = SavunmaJson(), ["p_guc"] = Kayit.TakimGucu(), ["p_asama"] = Kayit.P.EnIyiAsama });
		}
		catch (Exception e) { GD.PrintErr("Bulut kaydı: " + e.Message); }
	}

	// ------------------------------------------------------------ yardımcılar
	/// <summary>Sunucudan gelen savunma takımını (id, sv, y) savaş takımına çevirir; geçersiz kartları atar.</summary>
	public static List<TakimUyesi> TakimCoz(Variant v)
	{
		var l = new List<TakimUyesi>();
		if (v.VariantType != Variant.Type.Array) return l;
		foreach (var x in v.AsGodotArray())
		{
			if (x.VariantType != Variant.Type.Dictionary) continue;
			var d = x.AsGodotDictionary();
			string id = Metin(d["id"]);
			if (!Veri.Harita.ContainsKey(id) || l.Any(u => u.Id == id)) continue;
			int y = Math.Clamp(d.ContainsKey("y") ? d["y"].AsInt32() : 1, 1, Kayit.MaxYildiz);
			int sv = Math.Clamp(d.ContainsKey("sv") ? d["sv"].AsInt32() : 1, 1, Kayit.MaxSeviye(y));
			l.Add(new TakimUyesi(id, sv, y));
			if (l.Count == 5) break;
		}
		return l;
	}

	public static string S(Godot.Collections.Dictionary d, string a) => d != null && d.ContainsKey(a) ? Metin(d[a]) : "";
	public static int I(Godot.Collections.Dictionary d, string a) => d != null && d.ContainsKey(a) && d[a].VariantType != Variant.Type.Nil ? (int)d[a].AsDouble() : 0;
	public static bool B(Godot.Collections.Dictionary d, string a) => d != null && d.ContainsKey(a) && d[a].VariantType == Variant.Type.Bool && d[a].AsBool();
}
