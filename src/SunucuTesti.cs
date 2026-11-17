using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>
/// Canlı sunucu testi (CI): --sunucu-testi=dosya. İki test hesabıyla gerçek istemci kodunu (Sunucu.cs) uçtan uca dener.
/// Test hesapları geçicidir; test bittikten sonra sunucudan silinir.
/// </summary>
public static class SunucuTesti
{
	const string K1 = "TestBot1", K2 = "TestBot2", Sifre = "Deneme-1234";

	public static async Task Calistir(Node kok, string dosya)
	{
		var r = new StringBuilder();
		int ok = 0, hata = 0;
		async Task Dene(string ad, Func<Task<string>> f)
		{
			try { string s = await f(); r.AppendLine($"[OK]   {ad}: {s}"); ok++; }
			catch (Exception e) { r.AppendLine($"[HATA] {ad}: {e.Message}"); hata++; }
		}
		Kayit.KayitKapali = true;
		Kayit.YeniOyun();
		Kayit.P.Takim = new List<string> { "tepegoz", "akhilleus", "zeus", "medusa", "umay" };
		foreach (var id in Kayit.P.Takim) if (Kayit.Sahip(id) == null) Kayit.KartEkle(id);
		Kayit.P.EnIyiAsama = 5;
		await kok.ToSignal(kok.GetTree().CreateTimer(1), SceneTreeTimer.SignalName.Timeout);

		await Dene("kullanıcı adı müsait mi (yeni)", async () => (await Sunucu.Rpc("kullanici_adi_musait", new() { ["ad"] = "HicKimse" + GD.Randi() % 1000 })).ToString());
		await Dene("kullanıcı adı müsait mi (alınmış)", async () => (await Sunucu.Rpc("kullanici_adi_musait", new() { ["ad"] = K1 })).ToString());
		await Dene("yanlış şifreyle giriş reddedilmeli", async () => { try { await Sunucu.GirisYap(K1, "yanlis"); return "BEKLENMEDİK: giriş yapıldı"; } catch (SunucuHatasi e) { return "reddedildi → " + e.Message; } });
		await Dene("giriş 2 (kullanıcı adıyla)", async () => { await Sunucu.GirisYap(K2, Sifre); return $"{Sunucu.KullaniciAdi} {Sunucu.OyuncuId}"; });
		await Dene("bulut kaydı + savunma (2)", async () => { await Sunucu.BulutaGonder(); var d = await Sunucu.Sozluk("kayit_oku"); return d != null ? "kaydedildi" : "YOK"; });
		Sunucu.Cikis();
		await Dene("giriş 1 (e-postayla)", async () => { await Sunucu.GirisYap("testbot1@efsane.test", Sifre); return $"{Sunucu.KullaniciAdi}"; });
		await Dene("bulut kaydı + savunma (1)", async () => { await Sunucu.BulutaGonder(); return "ok"; });
		await Dene("ben", async () => Json.Stringify(await Sunucu.Sozluk("ben")));
		string id2 = "";
		await Dene("oyuncu ara", async () => { var l = await Sunucu.Liste("oyuncu_ara", new() { ["p_q"] = "TestBot" }); foreach (var x in l) if (Sunucu.S(x.AsGodotDictionary(), "ad") == K2) id2 = Sunucu.S(x.AsGodotDictionary(), "id"); return $"{l.Count} sonuç, TestBot2={id2}"; });
		await Dene("profil", async () => Sunucu.S(await Sunucu.Sozluk("oyuncu_profil", new() { ["p_id"] = id2 }), "ad"));
		await Dene("arkadaş isteği", async () => (await Sunucu.Rpc("arkadas_istek", new() { ["p_id"] = id2 })).AsString());
		await Dene("lonca kur", async () => (await Sunucu.Rpc("lonca_kur", new() { ["p_ad"] = "TestLonca", ["p_etiket"] = "TST", ["p_aciklama"] = "deneme", ["p_min_kupa"] = 0 })).ToString());
		await Dene("lonca mesajı", async () => (await Sunucu.Rpc("mesaj_gonder", new() { ["p_alici"] = default(Variant), ["p_lonca"] = true, ["p_metin"] = "Merhaba lonca" })).ToString());
		await Dene("gerçek rakip bul", async () => { var l = await Sunucu.Liste("rakip_bul"); return $"{l.Count} rakip"; });
		await Dene("arena sonucu (gerçek oyuncu)", async () => Json.Stringify(await Sunucu.Sozluk("arena_sonuc", new() { ["p_rakip"] = id2, ["p_kazandi"] = true })));
		await Dene("arena sonucu (bot)", async () => Json.Stringify(await Sunucu.Sozluk("bot_arena_sonuc", new() { ["p_kazandi"] = true, ["p_zorluk"] = 2 })));
		await Dene("sıralama dünya", async () => $"{(await Sunucu.Liste("siralama", new() { ["p_tur"] = "genel", ["p_adet"] = 10 })).Count} satır");
		await Dene("sıralama haftalık", async () => Json.Stringify(await Sunucu.Liste("siralama", new() { ["p_tur"] = "hafta", ["p_adet"] = 10 })));
		await Dene("benim sıram (ay)", async () => Json.Stringify(await Sunucu.Sozluk("benim_siram", new() { ["p_tur"] = "ay" })));
		await Dene("ikili rastgele (1)", async () => (await Sunucu.Rpc("ikili_rastgele")).AsString());
		Sunucu.Cikis();

		await Dene("giriş 2 tekrar", async () => { await Sunucu.GirisYap(K2, Sifre); return Sunucu.KullaniciAdi; });
		string id1 = "";
		await Dene("arkadaşlarım (gelen istek)", async () => { var l = await Sunucu.Liste("arkadaslarim"); foreach (var x in l) id1 = Sunucu.S(x.AsGodotDictionary(), "id"); return Json.Stringify(l); });
		await Dene("isteği kabul", async () => { await Sunucu.Rpc("arkadas_yanit", new() { ["p_id"] = id1, ["p_kabul"] = true }); return "ok"; });
		await Dene("birebir mesaj (küfür filtresi)", async () => (await Sunucu.Rpc("mesaj_gonder", new() { ["p_alici"] = id1, ["p_lonca"] = false, ["p_metin"] = "selam amk nasılsın" })).ToString());
		await Dene("mesajları getir", async () => Json.Stringify(await Sunucu.Liste("mesajlar_getir", new() { ["p_kisi"] = id1, ["p_lonca"] = false, ["p_sonra"] = 0 })));
		await Dene("loncaya katıl", async () => { var l = await Sunucu.Liste("lonca_ara", new() { ["p_q"] = "TestLonca" }); var lid = l[0].AsGodotDictionary()["id"]; await Sunucu.Rpc("lonca_katil", new() { ["p_id"] = lid }); return Json.Stringify(await Sunucu.Sozluk("lonca_detay", new() { ["p_id"] = lid })); });
		await Dene("lonca sohbeti oku", async () => $"{(await Sunucu.Liste("mesajlar_getir", new() { ["p_kisi"] = default(Variant), ["p_lonca"] = true, ["p_sonra"] = 0 })).Count} mesaj");
		await Dene("ikili rastgele (2) → eşleşme", async () => (await Sunucu.Rpc("ikili_rastgele")).AsString());
		await Dene("ikili takımım", async () => Sunucu.S(await Sunucu.Sozluk("ikili_takimim"), "durum"));
		await Dene("savunma kayıtları", async () => Json.Stringify(await Sunucu.Liste("savunma_kayitlari")));
		await Dene("sıralama lonca", async () => Json.Stringify(await Sunucu.Liste("siralama", new() { ["p_tur"] = "lonca", ["p_adet"] = 5 })));
		await Dene("engelle + mesaj reddi", async () =>
		{
			await Sunucu.Rpc("engelle", new() { ["p_id"] = id1, ["p_engel"] = true });
			return "engellendi";
		});
		Sunucu.Cikis();
		await Dene("oturumsuz korumalı çağrı reddedilmeli", async () => { try { await Sunucu.Rpc("ben"); return "BEKLENMEDİK: çalıştı"; } catch (SunucuHatasi e) { return "reddedildi → " + e.Message; } });

		r.Insert(0, $"Sunucu testi: {ok} başarılı, {hata} hatalı\n\n");
		GD.Print(r.ToString());
		try { System.IO.File.WriteAllText(dosya, r.ToString()); } catch (Exception e) { GD.PrintErr(e.Message); }
	}
}
