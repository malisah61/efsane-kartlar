using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>
/// Google Play Faturalandırma (GodotGooglePlayBilling 3.x eklentisi) köprüsü.
/// Eklenti yalnızca Gradle ile derlenen Google Play sürümünde bulunur; yoksa satın alma kapalıdır.
/// Akış: bağlan → ürün fiyatlarını sorgula → bekleyen satın almaları işle →
/// satın al → on_purchase_updated → ödülü ver (Magaza.Ver) → consumePurchase.
/// Aynı satın alma iki kez ödüllendirilmesin diye işlenen jetonlar profilde saklanır.
/// </summary>
public partial class Odeme : Node
{
	static Odeme _ben;
	GodotObject _eklenti;
	public static bool Hazir { get; private set; }
	public static bool EklentiVar => _ben?._eklenti != null;
	static readonly Dictionary<string, string> _fiyatlar = new();
	/// <summary>Satın alma tamamlandığında (ödül verildikten sonra) çağrılır: ürün kimliği, özet.</summary>
	public static Action<string, string> Tamamlandi;
	public static Action<string> Bilgi;

	const string EklentiAdi = "GodotGooglePlayBilling";

	public static void Kur(Node ebeveyn)
	{
		if (_ben != null) return;
		_ben = new Odeme { Name = "Odeme" };
		ebeveyn.AddChild(_ben);
	}

	public override void _Ready()
	{
		if (!Engine.HasSingleton(EklentiAdi)) { GD.Print("Ödeme eklentisi yok (Google Play dışı sürüm)."); return; }
		_eklenti = Engine.GetSingleton(EklentiAdi);
		_eklenti.Call("initPlugin");
		_eklenti.Connect("connected", Callable.From(Baglandi));
		_eklenti.Connect("disconnected", Callable.From(() => { Hazir = false; }));
		_eklenti.Connect("connect_error", Callable.From<int, string>((kod, mesaj) => { Hazir = false; GD.PrintErr($"Ödeme bağlantı hatası {kod}: {mesaj}"); }));
		_eklenti.Connect("query_product_details_response", Callable.From<Godot.Collections.Dictionary>(UrunDetaylari));
		_eklenti.Connect("query_purchases_response", Callable.From<Godot.Collections.Dictionary>(d => SatinAlmalariIsle(d)));
		_eklenti.Connect("on_purchase_updated", Callable.From<Godot.Collections.Dictionary>(d => SatinAlmalariIsle(d)));
		_eklenti.Connect("consume_purchase_response", Callable.From<Godot.Collections.Dictionary>(d =>
		{
			if (d.ContainsKey("response_code") && d["response_code"].AsInt32() != 0) GD.PrintErr("Tüketme hatası: " + d);
		}));
		_eklenti.Call("startConnection");
	}

	public override void _Notification(int what)
	{
		// uygulamaya geri dönünce (ödeme ekranı vb.) bekleyen satın almaları tekrar kontrol et
		if (what == NotificationApplicationResumed && Hazir) _eklenti?.Call("queryPurchases", "inapp", false);
	}

	void Baglandi()
	{
		Hazir = true;
		var ids = new string[Magaza.Urunler.Count];
		for (int i = 0; i < ids.Length; i++) ids[i] = Magaza.Urunler[i].Id;
		_eklenti.Call("queryProductDetails", ids, "inapp");
		_eklenti.Call("queryPurchases", "inapp", false);
	}

	void UrunDetaylari(Godot.Collections.Dictionary d)
	{
		if (!d.ContainsKey("product_details")) return;
		foreach (var v in d["product_details"].AsGodotArray())
		{
			var urun = v.AsGodotDictionary();
			string id = urun["product_id"].AsString();
			if (urun.ContainsKey("one_time_purchase_offer_details_list") && urun["one_time_purchase_offer_details_list"].VariantType == Variant.Type.Array)
			{
				var liste = urun["one_time_purchase_offer_details_list"].AsGodotArray();
				if (liste.Count > 0)
				{
					var teklif = liste[0].AsGodotDictionary();
					if (teklif.ContainsKey("formatted_price")) _fiyatlar[id] = teklif["formatted_price"].AsString();
				}
			}
		}
	}

	/// <summary>Play'den gelen yerel fiyat (ör. "₺89,99"); gelmediyse önerilen fiyat.</summary>
	public static string Fiyat(Urun u) => _fiyatlar.TryGetValue(u.Id, out var f) ? f : u.OnerilenFiyat;

	public static void SatinAl(string urunId)
	{
		var u = Magaza.Bul(urunId);
		if (u == null) return;
		if (!Magaza.SatinAlinabilir(u)) { Bilgi?.Invoke("Bu ürünü şu an alamazsın."); return; }
		if (_ben?._eklenti == null)
		{
			Bilgi?.Invoke("Satın alma yalnızca Google Play'den indirilen sürümde çalışır.");
			return;
		}
		if (!Hazir) { Bilgi?.Invoke("Google Play'e bağlanılamadı, biraz sonra tekrar dene."); _ben._eklenti.Call("startConnection"); return; }
		var sonuc = _ben._eklenti.Call("purchase", urunId, "", "", false).AsGodotDictionary();
		if (sonuc.ContainsKey("response_code") && sonuc["response_code"].AsInt32() != 0)
			Bilgi?.Invoke("Ödeme başlatılamadı (kod " + sonuc["response_code"].AsInt32() + ").");
	}

	void SatinAlmalariIsle(Godot.Collections.Dictionary d)
	{
		int kod = d.ContainsKey("response_code") ? d["response_code"].AsInt32() : -99;
		if (kod == 1) { Bilgi?.Invoke("Satın alma iptal edildi."); return; }
		if (kod != 0) { if (kod != -99) GD.PrintErr("Satın alma yanıtı: " + d); return; }
		if (!d.ContainsKey("purchases")) return;
		foreach (var v in d["purchases"].AsGodotArray())
		{
			var s = v.AsGodotDictionary();
			int durum = s["purchase_state"].AsInt32();       // 1 = satın alındı, 2 = bekliyor
			string jeton = s["purchase_token"].AsString();
			var urunler = s["product_ids"].AsStringArray();
			if (durum == 2) { Bilgi?.Invoke("Ödemen onay bekliyor; onaylanınca ödülün otomatik verilecek."); continue; }
			if (durum != 1) continue;
			bool kaliciZatenVar = urunler.All(id => Magaza.Bul(id)?.Kalici == true && Kayit.P.AlinanUrunler.Contains(id));
			if (!Kayit.P.IslenenJetonlar.Contains(jeton) && !kaliciZatenVar)
			{
				Kayit.P.IslenenJetonlar.Add(jeton);
				if (Kayit.P.IslenenJetonlar.Count > 200) Kayit.P.IslenenJetonlar.RemoveAt(0);
				foreach (var id in urunler)
				{
					int adet = Math.Max(1, s.ContainsKey("quantity") ? s["quantity"].AsInt32() : 1);
					string ozet = "";
					for (int i = 0; i < adet; i++) ozet = Magaza.Ver(id);
					Tamamlandi?.Invoke(id, ozet);
				}
				Kayit.Kaydet();
			}
			// kalıcı ürün (reklam kaldırma) onaylanır; diğerleri tekrar alınabilsin diye tüketilir
			if (urunler.Any(id => Magaza.Bul(id)?.Kalici == true))
			{
				bool onayli = s.ContainsKey("is_acknowledged") && s["is_acknowledged"].AsBool();
				if (!onayli) _eklenti.Call("acknowledgePurchase", jeton);
			}
			else _eklenti.Call("consumePurchase", jeton);
		}
	}
}
