using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace EfsaneKartlar;

/// <summary>Çevrimiçi ekranlar: giriş/kayıt, sıralamalar, arkadaşlar, oyuncu arama, mesajlaşma, lonca, ikili savaş, ödül kutusu.</summary>
public partial class Oyun
{
	static readonly Color SosyalMavi = new("2f6fb0"), SosyalMor = new("6a4fc0"), SosyalYesil = new("2f8a57"), Gri = new("444c6a");

	// ------------------------------------------------------------ yardımcılar
	static LineEdit Alan(string ipucu, bool gizli = false, int sinir = 64)
	{
		var a = new LineEdit { PlaceholderText = ipucu, Secret = gizli, MaxLength = sinir, CustomMinimumSize = new Vector2(0, 64) };
		a.AddThemeFontSizeOverride("font_size", 24);
		a.AddThemeStyleboxOverride("normal", Ui.Kutu(new Color("252b4a"), 10, new Color("3b4470"), 2, 12));
		a.AddThemeStyleboxOverride("focus", Ui.Kutu(new Color("2a3156"), 10, new Color("5f8be0"), 2, 12));
		return a;
	}

	/// <summary>Sunucu işlemini çalıştırır; hata olursa mesaj gösterir.</summary>
	async void Is(Func<Task> f)
	{
		try { await f(); }
		catch (Exception e) { Mesaj(e is SunucuHatasi ? e.Message : "Hata: " + e.Message); GD.PrintErr(e); }
	}

	/// <summary>Başlıklı, kaydırılabilir liste içeren büyük pencere iskeleti.</summary>
	(VBoxContainer v, VBoxContainer liste) Kabuk(string baslik, float yukseklik = 760)
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket(baslik, 32, Ui.Altin, HorizontalAlignment.Center));
		var liste = Ui.Dikey(6);
		var k = Ui.Kaydirma(liste);
		k.CustomMinimumSize = new Vector2(0, yukseklik);
		v.AddChild(k);
		return (v, liste);
	}

	static Control Satir(Color renk, params Control[] ogeler)
	{
		var p = Ui.Panel(renk, 10, 10);
		var h = Ui.Yatay(8);
		p.AddChild(h);
		foreach (var o in ogeler) h.AddChild(o);
		return p;
	}

	static Button Kucuk(string metin, Action tik, Color renk, float gen = 130)
	{
		var b = Ui.Dugme(metin, tik, renk, 18, 52);
		b.CustomMinimumSize = new Vector2(gen, 52);
		return b;
	}

	static string Zaman(string iso)
	{
		if (!DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var t)) return "";
		var fark = DateTime.UtcNow - t.ToUniversalTime();
		if (fark.TotalMinutes < 1) return "şimdi";
		if (fark.TotalHours < 1) return $"{(int)fark.TotalMinutes} dk";
		if (fark.TotalDays < 1) return $"{(int)fark.TotalHours} sa";
		return $"{(int)fark.TotalDays} g";
	}

	// ================================================================ SOSYAL SEKMESİ
	Control SosyalEkrani()
	{
		var v = Ui.Dikey(12);
		if (!Sunucu.Girisli) { GirisFormu(v); return Ui.Kaydirma(v); }

		var b = Sunucu.Ben;
		var ust = Ui.Panel(new Color("1d2a4a"), 16);
		v.AddChild(ust);
		var uv = Ui.Dikey(4);
		ust.AddChild(uv);
		uv.AddChild(Ui.Etiket(Sunucu.KullaniciAdi, 34, Ui.Altin));
		string lonca = Sunucu.S(b, "lonca_ad");
		uv.AddChild(Ui.Etiket($"{Kayit.P.Kupa} kupa  •  Güç {Kayit.TakimGucu():N0}" + (lonca != "" ? $"  •  [{Sunucu.S(b, "lonca_etiket")}] {lonca}" : "  •  Loncasız"), 20, Ui.Soluk));

		int odul = Sunucu.I(b, "odul_sayisi"), istek = Sunucu.I(b, "istek_sayisi"), saldiri = Sunucu.I(b, "saldiri_sayisi");
		void Dugme(string ad, Action tik, Color renk, int rozet = 0)
		{
			var d = Ui.Dugme(rozet > 0 ? $"{ad}  ({rozet})" : ad, tik, renk, 24, 84);
			v.AddChild(d);
		}
		if (odul > 0) Dugme("Ödül Kutusu — ödüllerin var!", OdulKutusu, new Color("b8862f"), odul);
		Dugme("Sıralamalar (dünya, haftalık, aylık)", () => CevrimiciSiralama("genel"), SosyalMor);
		Dugme("Arkadaşlar ve Mesajlar", ArkadaslarPenceresi, SosyalMavi, istek);
		Dugme("Oyuncu Ara", OyuncuAraPenceresi, SosyalMavi);
		Dugme("Lonca", LoncaPenceresi, new Color("8a4f2f"));
		Dugme("İkili Savaş (2'ye 2)", IkiliPenceresi, new Color("a03f5f"));
		Dugme("Savunma Kayıtları", SavunmaKayitlari, Gri, saldiri);
		if (odul == 0) Dugme("Ödül Kutusu", OdulKutusu, Gri);
		v.AddChild(Ui.Bosluk(10));
		if (Sunucu.B(b, "ad_degisebilir"))
			v.AddChild(Ui.Dugme("Kullanıcı Adını Değiştir", () =>
			{
				var pv = Ui.Dikey(10);
				pv.AddChild(Ui.Etiket("Yeni Kullanıcı Adı", 30, Ui.Altin, HorizontalAlignment.Center));
				var ad = Alan("3-16 harf/rakam", false, 16);
				ad.Text = Sunucu.KullaniciAdi;
				pv.AddChild(ad);
				pv.AddChild(Ui.Etiket("Haftada bir değiştirebilirsin.", 17, Ui.Soluk, HorizontalAlignment.Center));
				pv.AddChild(Ui.Dugme("Kaydet", () => Is(async () =>
				{
					await Sunucu.Rpc("ad_degistir", new() { ["p_ad"] = ad.Text.Trim() });
					await Sunucu.BenYenile();
					Kayit.P.Ad = Sunucu.KullaniciAdi; Kayit.Kaydet();
					PencereKapat(); Yenile(); Mesaj("Kullanıcı adın değişti");
				}), SosyalYesil, 22));
				pv.AddChild(Ui.Dugme("Vazgeç", PencereKapat, Gri, 22));
				Pencere(pv);
			}, Gri, 20, 56));
		var alt = Ui.Yatay(10);
		v.AddChild(alt);
		alt.AddChild(Ui.Genis(Ui.Dugme("Buluta Kaydet", () => Is(async () => { await Sunucu.BulutaGonder(); Mesaj("İlerlemen buluta kaydedildi"); }), SosyalYesil, 20, 60)));
		alt.AddChild(Ui.Genis(Ui.Dugme("Çıkış Yap", () =>
		{
			Onay("Hesabından çıkılsın mı? İlerlemen bulutta kayıtlı kalır.", () => { _ = Sunucu.BulutaGonder(); Sunucu.Cikis(); });
		}, new Color("6a3a3a"), 20, 60)));
		// profil bilgisi tazele
		Is(async () => { await Sunucu.BenYenile(); });
		return Ui.Kaydirma(v);
	}

	void Onay(string soru, Action evet)
	{
		var v = Ui.Dikey(14);
		v.AddChild(Ui.Etiket(soru, 24, Colors.White, HorizontalAlignment.Center));
		var h = Ui.Yatay(10);
		v.AddChild(h);
		h.AddChild(Ui.Genis(Ui.Dugme("Evet", () => { PencereKapat(); evet(); }, new Color("c0582f"), 22)));
		h.AddChild(Ui.Genis(Ui.Dugme("Vazgeç", PencereKapat, Gri, 22)));
		Pencere(v);
	}

	// ------------------------------------------------------------ giriş / kayıt
	bool _kayitModu;

	void GirisFormu(VBoxContainer v)
	{
		v.AddChild(Ui.Etiket(_kayitModu ? "Hesap Oluştur" : "Giriş Yap", 36, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket("Hesapla ilerlemen buluta kaydedilir; gerçek oyuncularla savaşır, arkadaş ekler, loncaya katılır ve dünya sıralamasına girersin.", 19, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Google Play Games ile Giriş", PlayGames.GirisYap, new Color("01875f"), 26, 84));
		v.AddChild(Ui.Etiket("— veya kullanıcı adı / e-posta ile —", 18, Ui.Soluk, HorizontalAlignment.Center));
		var sekme = Ui.Yatay(10);
		v.AddChild(sekme);
		sekme.AddChild(Ui.Genis(Ui.Dugme("Giriş", () => { _kayitModu = false; Yenile(); }, _kayitModu ? Gri : SosyalMavi, 22)));
		sekme.AddChild(Ui.Genis(Ui.Dugme("Kayıt Ol", () => { _kayitModu = true; Yenile(); }, _kayitModu ? SosyalMavi : Gri, 22)));

		if (_kayitModu)
		{
			var ad = Alan("Kullanıcı adı (3-16 harf/rakam)", false, 16);
			var ep = Alan("E-posta", false, 120);
			var s1 = Alan("Şifre (en az 6 karakter)", true);
			var s2 = Alan("Şifre (tekrar)", true);
			foreach (var a in new[] { ad, ep, s1, s2 }) v.AddChild(a);
			v.AddChild(Ui.Etiket("Kayıt olarak oyun kurallarını ve gizlilik politikasını kabul etmiş olursun. Mesajlarda küfür ve hakaret yasaktır.", 16, Ui.Soluk, HorizontalAlignment.Center));
			v.AddChild(Ui.Dugme("Hesap Oluştur", () => Is(async () =>
			{
				string a = ad.Text.Trim(), e = ep.Text.Trim();
				if (a.Length < 3) { Mesaj("Kullanıcı adı en az 3 karakter"); return; }
				if (!e.Contains('@')) { Mesaj("Geçerli bir e-posta yaz"); return; }
				if (s1.Text.Length < 6) { Mesaj("Şifre en az 6 karakter"); return; }
				if (s1.Text != s2.Text) { Mesaj("Şifreler aynı değil"); return; }
				Mesaj("Hesap oluşturuluyor…");
				bool girdi = await Sunucu.KayitOl(e, s1.Text, a);
				if (girdi) { Mesaj($"Hoş geldin {a}!"); Yenile(); }
				else
				{
					_kayitModu = false; Yenile();
					BilgiPenceresi("E-postanı onayla", $"{e} adresine bir onay bağlantısı gönderdik. Bağlantıya tıkladıktan sonra kullanıcı adın ve şifrenle giriş yap.");
				}
			}), SosyalYesil, 26, 72));
		}
		else
		{
			var k = Alan("Kullanıcı adı veya e-posta", false, 120);
			var s = Alan("Şifre", true);
			v.AddChild(k);
			v.AddChild(s);
			v.AddChild(Ui.Dugme("Giriş Yap", () => Is(async () =>
			{
				if (k.Text.Trim() == "" || s.Text == "") { Mesaj("Bilgileri doldur"); return; }
				Mesaj("Giriş yapılıyor…");
				await Sunucu.GirisYap(k.Text.Trim(), s.Text);
				Mesaj($"Hoş geldin {Sunucu.KullaniciAdi}!");
				Yenile();
			}), SosyalYesil, 26, 72));
			v.AddChild(Ui.Dugme("Şifremi unuttum", () =>
			{
				var pv = Ui.Dikey(12);
				pv.AddChild(Ui.Etiket("Şifre Sıfırlama", 30, Ui.Altin, HorizontalAlignment.Center));
				var ep = Alan("Hesabının e-postası", false, 120);
				pv.AddChild(ep);
				pv.AddChild(Ui.Dugme("Sıfırlama bağlantısı gönder", () => Is(async () =>
				{
					await Sunucu.SifreSifirla(ep.Text.Trim());
					PencereKapat();
					BilgiPenceresi("Gönderildi", "E-postana şifre sıfırlama bağlantısı gönderdik.");
				}), SosyalMavi, 22));
				pv.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
				Pencere(pv);
			}, Gri, 20, 56));
		}
	}

	void BilgiPenceresi(string baslik, string metin)
	{
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket(baslik, 30, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket(metin, 22, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Tamam", PencereKapat, SosyalMavi, 22));
		Pencere(v);
	}

	/// <summary>Giriş sonrası buluttaki kayıt ile cihazdaki farklıysa oyuncuya sorar.</summary>
	Task<bool> KayitSecimPenceresi(string bulut, string cihaz)
	{
		var tcs = new TaskCompletionSource<bool>();
		var v = Ui.Dikey(12);
		v.AddChild(Ui.Etiket("Hangi ilerleme kullanılsın?", 30, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket("Bu hesapta buluta kayıtlı bir ilerleme var ve bu cihazdakinden farklı.", 20, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Buluttaki\n" + bulut, () => { PencereKapat(); tcs.TrySetResult(true); }, SosyalMavi, 20, 100));
		v.AddChild(Ui.Dugme("Bu cihazdaki\n" + cihaz, () => { PencereKapat(); tcs.TrySetResult(false); }, SosyalYesil, 20, 100));
		v.AddChild(Ui.Etiket("Seçmediğin ilerleme silinir.", 18, Ui.Kirmizi, HorizontalAlignment.Center));
		Pencere(v);
		return tcs.Task;
	}

	// ================================================================ ARENA: GERÇEK RAKİPLER
	bool _rakipYukleniyor;

	void CevrimiciRakipleriYukle()
	{
		if (_rakipYukleniyor) return;
		_rakipYukleniyor = true;
		Is(async () =>
		{
			var l = new List<ArenaRakip>();
			try
			{
				var gelen = await Sunucu.Liste("rakip_bul");
				foreach (var x in gelen)
				{
					var d = x.AsGodotDictionary();
					var takim = Sunucu.TakimCoz(d["savunma"]);
					if (takim.Count < 5) continue;
					int kupa = Sunucu.I(d, "kupa");
					int fark = kupa - Kayit.P.Kupa;
					l.Add(new ArenaRakip
					{
						Ad = Sunucu.S(d, "ad") + (Sunucu.S(d, "etiket") != "" ? $" [{Sunucu.S(d, "etiket")}]" : ""),
						OyuncuId = Sunucu.S(d, "id"), Kupa = kupa, Takim = takim, Guc = Kayit.TakimGucu(takim),
						Zorluk = fark < -30 ? 0 : fark > 50 ? 2 : 1, Tohum = (ulong)GD.Randi(),
					});
				}
			}
			finally { _rakipYukleniyor = false; }
			// yeterli gerçek oyuncu yoksa botlarla tamamla
			if (l.Count < 3)
			{
				var botlar = Rakipler.ArenaRakipleri();
				foreach (var b in botlar) if (l.Count < 3 && !l.Any(x => x.Zorluk == b.Zorluk)) l.Add(b);
				foreach (var b in botlar) if (l.Count < 3 && !l.Contains(b)) l.Add(b);
			}
			_rakipler = l.OrderBy(x => x.Zorluk).ToList();
			if (_ekran == Ekran.Arena) Yenile();
		});
	}

	// ================================================================ SIRALAMALAR
	void CevrimiciSiralama(string tur)
	{
		var (v, liste) = Kabuk("Sıralamalar", 700);
		var sekme = Ui.Yatay(6);
		v.AddChild(sekme);
		v.MoveChild(sekme, 1);
		foreach (var (ad, t) in new[] { ("Dünya", "genel"), ("Haftalık", "hafta"), ("Aylık", "ay"), ("İkili", "ikili"), ("Lonca", "lonca") })
			sekme.AddChild(Ui.Genis(Ui.Dugme(ad, () => CevrimiciSiralama(t), t == tur ? SosyalMavi : Gri, 18, 54)));
		string aciklama = tur switch
		{
			"genel" => "Tüm zamanlar kupa sıralaması.",
			"hafta" => $"Bu hafta kazanılan kupa. Pazartesi 00:00'da ödüller: 1. → 1000 elmas + 10 parşömen • 2-3. → 600 • 4-10. → 300 • 11-50. → 150 • 51-100. → 80 • 101-200. → 40",
			"ay" => "Bu ay kazanılan kupa. Ay sonunda haftalık ödüllerin 2 katı dağıtılır.",
			"ikili" => "İki kişilik takımların puanı.",
			_ => "Lonca üyelerinin bu hafta kazandığı kupaların toplamı. İlk 20 loncanın tüm üyeleri ödül alır.",
		};
		var acik = Ui.Etiket(aciklama, 17, Ui.Elmas, HorizontalAlignment.Center);
		v.AddChild(acik);
		v.MoveChild(acik, 2);
		var benim = Ui.Etiket("", 21, Ui.Yesil, HorizontalAlignment.Center);
		v.AddChild(benim);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
		liste.AddChild(Ui.Etiket("Yükleniyor…", 22, Ui.Soluk, HorizontalAlignment.Center));
		Pencere(v);
		Is(async () =>
		{
			var l = await Sunucu.Liste("siralama", new() { ["p_tur"] = tur, ["p_adet"] = 100 });
			if (!IsInstanceValid(liste)) return;
			foreach (var c in liste.GetChildren()) c.QueueFree();
			if (l.Count == 0) liste.AddChild(Ui.Etiket("Henüz kimse yok. İlk sen ol!", 22, Ui.Soluk, HorizontalAlignment.Center));
			foreach (var x in l)
			{
				var d = x.AsGodotDictionary();
				int sira = Sunucu.I(d, "sira");
				bool ben = Sunucu.B(d, "ben");
				string id = Sunucu.S(d, "id"), et = Sunucu.S(d, "etiket");
				var no = Ui.Etiket($"{sira}.", 21, sira <= 3 ? Ui.Altin : Ui.Soluk);
				no.CustomMinimumSize = new Vector2(60, 0);
				var ad = Ui.Genis(Ui.Etiket((et != "" && tur != "lonca" ? $"[{et}] " : "") + Sunucu.S(d, "ad") + (ben ? " (sen)" : ""), 21));
				var puan = Ui.Genis(Ui.Etiket($"{Sunucu.I(d, "puan"):N0}" + (tur == "lonca" ? " puan" : tur == "ikili" ? " puan" : " kupa"), 19, Ui.Soluk, HorizontalAlignment.Right), 0.6f);
				var satir = Satir(ben ? new Color("3b5a2f") : sira <= 3 ? new Color("3a3320") : new Color("1b2038"), no, ad, puan);
				if (tur is "genel" or "hafta" or "ay" && !ben)
				{
					var git = Kucuk("Profil", () => OyuncuProfili(id), SosyalMavi, 110);
					((HBoxContainer)satir.GetChild(0)).AddChild(git);
				}
				else if (tur == "lonca")
				{
					var git = Kucuk("Göster", () => LoncaDetay(long.Parse(id)), SosyalMavi, 110);
					((HBoxContainer)satir.GetChild(0)).AddChild(git);
				}
				liste.AddChild(satir);
			}
			if (tur is "genel" or "hafta" or "ay")
			{
				var m = await Sunucu.Sozluk("benim_siram", new() { ["p_tur"] = tur });
				if (IsInstanceValid(benim)) benim.Text = $"Senin sıran: {Sunucu.I(m, "sira")}.  ({Sunucu.I(m, "puan"):N0} kupa)";
			}
		});
	}

	// ================================================================ ÖDÜL KUTUSU
	void OdulKutusu()
	{
		var (v, liste) = Kabuk("Ödül Kutusu", 600);
		liste.AddChild(Ui.Etiket("Yükleniyor…", 22, Ui.Soluk, HorizontalAlignment.Center));
		var al = Ui.Dugme("Hepsini Al", () => Is(async () =>
		{
			var d = await Sunucu.Sozluk("odulleri_al");
			int e = Sunucu.I(d, "elmas"), a = Sunucu.I(d, "altin"), p = Sunucu.I(d, "parsomen");
			if (Sunucu.I(d, "adet") == 0) { Mesaj("Alınacak ödül yok"); return; }
			Kayit.P.Elmas += e; Kayit.P.Altin += a; Kayit.P.Parsomen += p;
			Kayit.Kaydet();
			Ses.Efekt("para");
			await Sunucu.BulutaGonder();
			await Sunucu.BenYenile();
			PencereKapat(); Yenile();
			BilgiPenceresi("Ödüller alındı!", $"+{e:N0} elmas" + (a > 0 ? $"  +{a:N0} altın" : "") + (p > 0 ? $"  +{p} parşömen" : ""));
		}), new Color("b8862f"), 24);
		v.AddChild(al);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
		Pencere(v);
		Is(async () =>
		{
			var l = await Sunucu.Liste("oduller");
			if (!IsInstanceValid(liste)) return;
			foreach (var c in liste.GetChildren()) c.QueueFree();
			if (l.Count == 0) liste.AddChild(Ui.Etiket("Ödül kutun boş. Haftalık ve aylık sıralamada yüksel, ödüller buraya gelir.", 21, Ui.Soluk, HorizontalAlignment.Center));
			foreach (var x in l)
			{
				var d = x.AsGodotDictionary();
				string od = $"{Sunucu.I(d, "elmas")} elmas" + (Sunucu.I(d, "parsomen") > 0 ? $" + {Sunucu.I(d, "parsomen")} parşömen" : "") + (Sunucu.I(d, "altin") > 0 ? $" + {Sunucu.I(d, "altin")} altın" : "");
				liste.AddChild(Satir(new Color("2a2a1e"), Ui.Genis(Ui.Etiket(Sunucu.S(d, "baslik"), 19)), Ui.Genis(Ui.Etiket(od, 19, Ui.Elmas, HorizontalAlignment.Right), 0.7f)));
			}
			if (IsInstanceValid(al)) al.Disabled = l.Count == 0;
		});
	}

	// ================================================================ ARKADAŞLAR
	void ArkadaslarPenceresi()
	{
		var (v, liste) = Kabuk("Arkadaşlar", 760);
		liste.AddChild(Ui.Etiket("Yükleniyor…", 22, Ui.Soluk, HorizontalAlignment.Center));
		var h = Ui.Yatay(10);
		v.AddChild(h);
		h.AddChild(Ui.Genis(Ui.Dugme("Oyuncu Ara / Ekle", OyuncuAraPenceresi, SosyalMavi, 21)));
		h.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, Gri, 21)));
		Pencere(v);
		Is(async () =>
		{
			var l = await Sunucu.Liste("arkadaslarim");
			if (!IsInstanceValid(liste)) return;
			foreach (var c in liste.GetChildren()) c.QueueFree();
			if (l.Count == 0) liste.AddChild(Ui.Etiket("Henüz arkadaşın yok. Oyuncu ara ve arkadaşlık isteği gönder.", 21, Ui.Soluk, HorizontalAlignment.Center));
			foreach (var x in l)
			{
				var d = x.AsGodotDictionary();
				string id = Sunucu.S(d, "id"), ad = Sunucu.S(d, "ad"), durum = Sunucu.S(d, "durum");
				bool cev = Sunucu.B(d, "cevrimici");
				var bilgi = Ui.Genis(Ui.Etiket($"{(cev ? "● " : "")}{ad}\n{Sunucu.I(d, "kupa")} kupa • Güç {Sunucu.I(d, "guc"):N0}", 19, cev ? Ui.Yesil : Colors.White));
				Control satir;
				if (durum == "gelen")
					satir = Satir(new Color("2f3a22"), bilgi,
						Kucuk("Kabul", () => Is(async () => { await Sunucu.Rpc("arkadas_yanit", new() { ["p_id"] = id, ["p_kabul"] = true }); Mesaj($"{ad} artık arkadaşın"); ArkadaslarPenceresi(); }), SosyalYesil, 110),
						Kucuk("Reddet", () => Is(async () => { await Sunucu.Rpc("arkadas_yanit", new() { ["p_id"] = id, ["p_kabul"] = false }); ArkadaslarPenceresi(); }), new Color("6a3a3a"), 110));
				else if (durum == "giden")
					satir = Satir(new Color("1b2038"), bilgi, Ui.Etiket("İstek gönderildi", 17, Ui.Soluk));
				else
				{
					int ok = Sunucu.I(d, "okunmamis");
					satir = Satir(new Color("1b2038"), bilgi,
						Kucuk(ok > 0 ? $"Mesaj ({ok})" : "Mesaj", () => SohbetPenceresi(id, ad, false), SosyalMavi, 130),
						Kucuk("Profil", () => OyuncuProfili(id), Gri, 100));
				}
				liste.AddChild(satir);
			}
			await Sunucu.BenYenile();
		});
	}

	void OyuncuAraPenceresi()
	{
		var (v, liste) = Kabuk("Oyuncu Ara", 640);
		var h = Ui.Yatay(8);
		var arama = Ui.Genis(Alan("Kullanıcı adı (en az 2 harf)", false, 16));
		h.AddChild(arama);
		void Ara() => Is(async () =>
		{
			if (arama.Text.Trim().Length < 2) { Mesaj("En az 2 harf yaz"); return; }
			var l = await Sunucu.Liste("oyuncu_ara", new() { ["p_q"] = arama.Text.Trim() });
			if (!IsInstanceValid(liste)) return;
			foreach (var c in liste.GetChildren()) c.QueueFree();
			if (l.Count == 0) liste.AddChild(Ui.Etiket("Oyuncu bulunamadı", 21, Ui.Soluk, HorizontalAlignment.Center));
			foreach (var x in l)
			{
				var d = x.AsGodotDictionary();
				string id = Sunucu.S(d, "id"), et = Sunucu.S(d, "etiket");
				liste.AddChild(Satir(new Color("1b2038"),
					Ui.Genis(Ui.Etiket($"{(Sunucu.B(d, "cevrimici") ? "● " : "")}{(et != "" ? $"[{et}] " : "")}{Sunucu.S(d, "ad")}\n{Sunucu.I(d, "kupa")} kupa • Güç {Sunucu.I(d, "guc"):N0}", 19)),
					Kucuk("Profil", () => OyuncuProfili(id), SosyalMavi, 110)));
			}
		});
		arama.TextSubmitted += _ => Ara();
		h.AddChild(Kucuk("Ara", Ara, SosyalYesil, 110));
		v.AddChild(h);
		v.MoveChild(h, 1);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
		Pencere(v);
		arama.CallDeferred(Control.MethodName.GrabFocus);
	}

	void OyuncuProfili(string id)
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Yükleniyor…", 24, Ui.Soluk, HorizontalAlignment.Center));
		Pencere(v);
		Is(async () =>
		{
			var d = await Sunucu.Sozluk("oyuncu_profil", new() { ["p_id"] = id });
			if (d == null) { PencereKapat(); Mesaj("Oyuncu bulunamadı"); return; }
			var w = Ui.Dikey(10);
			string ad = Sunucu.S(d, "ad"), ark = Sunucu.S(d, "arkadaslik");
			bool engelli = Sunucu.B(d, "engelli");
			w.AddChild(Ui.Etiket((Sunucu.B(d, "cevrimici") ? "● " : "") + ad, 34, Ui.Altin, HorizontalAlignment.Center));
			string lonca = Sunucu.S(d, "lonca_ad");
			w.AddChild(Ui.Etiket($"{Sunucu.I(d, "kupa")} kupa  •  Güç {Sunucu.I(d, "guc"):N0}  •  Hikâye {Sunucu.I(d, "asama")}/60  •  {Sunucu.I(d, "galibiyet")} arena galibiyeti", 19, Ui.Soluk, HorizontalAlignment.Center));
			if (lonca != "") w.AddChild(Ui.Etiket($"Lonca: [{Sunucu.S(d, "lonca_etiket")}] {lonca}", 20, new Color("e2a56a"), HorizontalAlignment.Center));
			var takim = Sunucu.TakimCoz(d["savunma"]);
			w.AddChild(Ui.Etiket("Savunma takımı", 22));
			if (takim.Count > 0) w.AddChild(MiniTakim(takim, 112)); else w.AddChild(Ui.Etiket("Takım kurmamış", 19, Ui.Soluk));

			var h1 = Ui.Yatay(8);
			w.AddChild(h1);
			if (ark == "kabul_giden" || ark == "kabul_gelen")
			{
				h1.AddChild(Ui.Genis(Ui.Dugme("Mesaj", () => SohbetPenceresi(id, ad, false), SosyalMavi, 21)));
				h1.AddChild(Ui.Genis(Ui.Dugme("Arkadaşlıktan çıkar", () => Onay($"{ad} arkadaş listenden çıkarılsın mı?", () => Is(async () => { await Sunucu.Rpc("arkadas_sil", new() { ["p_id"] = id }); Mesaj("Çıkarıldı"); })), new Color("6a3a3a"), 19)));
			}
			else if (ark == "bekliyor_giden") h1.AddChild(Ui.Genis(Ui.Etiket("Arkadaşlık isteği gönderildi", 20, Ui.Soluk, HorizontalAlignment.Center)));
			else if (!engelli)
				h1.AddChild(Ui.Genis(Ui.Dugme(ark == "bekliyor_gelen" ? "İsteği Kabul Et" : "Arkadaş Ekle", () => Is(async () =>
				{
					var r = (await Sunucu.Rpc("arkadas_istek", new() { ["p_id"] = id })).AsString();
					Mesaj(r == "kabul" ? $"{ad} artık arkadaşın" : r == "gonderildi" ? "Arkadaşlık isteği gönderildi" : r == "liste_dolu" ? "Arkadaş listen dolu (100)" : "Gönderilemedi");
					OyuncuProfili(id);
				}), SosyalYesil, 21)));

			if (takim.Count == 5)
				w.AddChild(Ui.Dugme("Dostluk Maçı (kupa yok)", () =>
				{
					PencereKapat();
					SavasBaslat(takim, $"Dostluk: {ad}", Rakipler.TakimArka(takim), async kazandi =>
					{
						try { await Sunucu.Rpc("dostluk_sonuc", new() { ["p_rakip"] = id, ["p_kazandi"] = kazandi }); } catch { }
						return new List<string> { kazandi ? $"{ad} adlı oyuncunun savunmasını yendin!" : $"{ad} adlı oyuncunun savunması kazandı." };
					}, false, null);
				}, new Color("c0582f"), 22));
			if (ark is "kabul_giden" or "kabul_gelen")
				w.AddChild(Ui.Dugme("İkili Savaşa Davet Et", () => Is(async () => { await Sunucu.Rpc("ikili_davet", new() { ["p_arkadas"] = id }); Mesaj($"{ad} ikili takıma davet edildi"); }), new Color("a03f5f"), 21));

			var h2 = Ui.Yatay(8);
			w.AddChild(h2);
			h2.AddChild(Ui.Genis(Ui.Dugme(engelli ? "Engeli Kaldır" : "Engelle", () => Is(async () =>
			{
				await Sunucu.Rpc("engelle", new() { ["p_id"] = id, ["p_engel"] = !engelli });
				Mesaj(engelli ? "Engel kaldırıldı" : $"{ad} engellendi; sana mesaj atamaz ve istek gönderemez");
				OyuncuProfili(id);
			}), Gri, 19, 56)));
			h2.AddChild(Ui.Genis(Ui.Dugme("Şikâyet Et", () => SikayetPenceresi(id, ad, 0), new Color("6a3a3a"), 19, 56)));
			w.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
			Pencere(w);
		});
	}

	void SikayetPenceresi(string id, string ad, long mesajId)
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket($"{ad} — Şikâyet", 28, Ui.Altin, HorizontalAlignment.Center));
		foreach (var sebep in new[] { "Küfür / hakaret", "Uygunsuz kullanıcı adı", "Spam / reklam", "Hile şüphesi", "Diğer" })
			v.AddChild(Ui.Dugme(sebep, () => Is(async () =>
			{
				await Sunucu.Rpc("sikayet_et", new() { ["p_id"] = id, ["p_mesaj"] = mesajId > 0 ? mesajId : default(Variant), ["p_sebep"] = sebep });
				PencereKapat();
				Mesaj("Şikâyetin alındı, teşekkürler");
			}), Gri, 21));
		v.AddChild(Ui.Dugme("Vazgeç", PencereKapat, new Color("333a55"), 21));
		Pencere(v);
	}

	// ================================================================ MESAJLAŞMA
	/// <summary>Birebir (arkadaş) veya lonca sohbeti. Pencere açıkken 4 saniyede bir yeni mesajları çeker.</summary>
	void SohbetPenceresi(string kisiId, string baslik, bool lonca)
	{
		var v = Ui.Dikey(8);
		v.AddChild(Ui.Etiket(lonca ? $"Lonca Sohbeti — {baslik}" : $"Sohbet — {baslik}", 28, Ui.Altin, HorizontalAlignment.Center));
		var liste = Ui.Dikey(6);
		var kaydir = Ui.Kaydirma(liste);
		kaydir.CustomMinimumSize = new Vector2(0, 760);
		v.AddChild(kaydir);
		var h = Ui.Yatay(8);
		v.AddChild(h);
		var yaz = Ui.Genis(Alan("Mesaj yaz…", false, 300));
		h.AddChild(yaz);
		long son = 0;
		bool cekiliyor = false;

		async Task Cek()
		{
			if (cekiliyor || !IsInstanceValid(liste)) return;
			cekiliyor = true;
			try
			{
				var l = await Sunucu.Liste("mesajlar_getir", new() { ["p_kisi"] = lonca ? default(Variant) : kisiId, ["p_lonca"] = lonca, ["p_sonra"] = son });
				if (!IsInstanceValid(liste)) return;
				if (son == 0) foreach (var c in liste.GetChildren()) c.QueueFree();
				if (son == 0 && l.Count == 0) liste.AddChild(Ui.Etiket("Henüz mesaj yok. İlk mesajı sen yaz!", 20, Ui.Soluk, HorizontalAlignment.Center));
				foreach (var x in l)
				{
					var d = x.AsGodotDictionary();
					long mid = (long)d["id"].AsDouble();
					son = Math.Max(son, mid);
					bool benim = Sunucu.B(d, "benim");
					string gid = Sunucu.S(d, "gonderen"), gad = Sunucu.S(d, "ad");
					var balon = Ui.Panel(benim ? new Color("2f4f7a") : new Color("2a2f48"), 10, 12);
					var bv = Ui.Dikey(2);
					balon.AddChild(bv);
					bv.AddChild(Ui.Etiket((benim ? "Sen" : gad) + "  •  " + Zaman(Sunucu.S(d, "zaman")), 15, Ui.Soluk));
					var m = Ui.Etiket(Sunucu.S(d, "metin"), 21);
					m.AutowrapMode = TextServer.AutowrapMode.WordSmart;
					m.CustomMinimumSize = new Vector2(460, 0);
					bv.AddChild(m);
					var sat = Ui.Yatay(6);
					if (benim) sat.AddChild(Ui.Genis(new Control()));
					sat.AddChild(balon);
					if (!benim)
					{
						sat.AddChild(Kucuk("!", () => SikayetPenceresi(gid, gad, mid), new Color("553333"), 52));
						sat.AddChild(Ui.Genis(new Control()));
					}
					liste.AddChild(sat);
				}
				if (l.Count > 0) Callable.From(() => { if (IsInstanceValid(kaydir)) kaydir.ScrollVertical = (int)kaydir.GetVScrollBar().MaxValue; }).CallDeferred();
			}
			catch (Exception e) { GD.PrintErr(e.Message); }
			finally { cekiliyor = false; }
		}

		void Gonder() => Is(async () =>
		{
			string t = yaz.Text.Trim();
			if (t == "") return;
			yaz.Text = "";
			await Sunucu.Rpc("mesaj_gonder", new() { ["p_alici"] = lonca ? default(Variant) : kisiId, ["p_lonca"] = lonca, ["p_metin"] = t });
			await Cek();
		});
		yaz.TextSubmitted += _ => Gonder();
		h.AddChild(Kucuk("Gönder", Gonder, SosyalYesil, 130));
		v.AddChild(Ui.Etiket("Küfür ve hakaret otomatik gizlenir. Rahatsız eden oyuncuyu ! ile şikâyet edebilir veya profilinden engelleyebilirsin.", 15, Ui.Soluk, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
		Pencere(v);
		liste.AddChild(Ui.Etiket("Yükleniyor…", 20, Ui.Soluk, HorizontalAlignment.Center));

		async void Dongu()
		{
			while (IsInstanceValid(liste) && liste.IsInsideTree())
			{
				await Cek();
				await ToSignal(GetTree().CreateTimer(4), SceneTreeTimer.SignalName.Timeout);
			}
		}
		Dongu();
	}

	// ================================================================ LONCA
	void LoncaPenceresi()
	{
		long lid = Sunucu.I(Sunucu.Ben, "lonca_id");
		if (lid > 0) { LoncaDetay(lid); return; }
		var (v, liste) = Kabuk("Loncalar", 620);
		var h = Ui.Yatay(8);
		var arama = Ui.Genis(Alan("Lonca adı veya etiketi", false, 20));
		h.AddChild(arama);
		void Ara() => Is(async () =>
		{
			var l = await Sunucu.Liste("lonca_ara", new() { ["p_q"] = arama.Text.Trim() });
			if (!IsInstanceValid(liste)) return;
			foreach (var c in liste.GetChildren()) c.QueueFree();
			if (l.Count == 0) liste.AddChild(Ui.Etiket("Lonca bulunamadı. Kendi loncanı kur!", 21, Ui.Soluk, HorizontalAlignment.Center));
			foreach (var x in l)
			{
				var d = x.AsGodotDictionary();
				long id = (long)d["id"].AsDouble();
				liste.AddChild(Satir(new Color("2a2218"),
					Ui.Genis(Ui.Etiket($"[{Sunucu.S(d, "etiket")}] {Sunucu.S(d, "ad")}\n{Sunucu.I(d, "uye")}/30 üye • en az {Sunucu.I(d, "min_kupa")} kupa • bu hafta {Sunucu.I(d, "puan")} puan", 18)),
					Kucuk("Göster", () => LoncaDetay(id), SosyalMavi, 110)));
			}
		});
		arama.TextSubmitted += _ => Ara();
		h.AddChild(Kucuk("Ara", Ara, SosyalYesil, 100));
		v.AddChild(h);
		v.MoveChild(h, 1);
		v.AddChild(Ui.Dugme($"Lonca Kur ({LoncaKurmaFiyati} elmas)", LoncaKurPenceresi, new Color("8a4f2f"), 22));
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
		Pencere(v);
		Ara();
	}

	const int LoncaKurmaFiyati = 500;

	void LoncaKurPenceresi()
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Lonca Kur", 32, Ui.Altin, HorizontalAlignment.Center));
		var ad = Alan("Lonca adı (3-20 karakter)", false, 20);
		var et = Alan("Etiket (2-4 harf, ör. BOZ)", false, 4);
		var ac = Alan("Açıklama (isteğe bağlı)", false, 200);
		var mk = Alan("En az kupa (ör. 0)", false, 5);
		foreach (var a in new[] { ad, et, ac, mk }) v.AddChild(a);
		v.AddChild(Ui.Dugme($"Kur ({LoncaKurmaFiyati} elmas)", () => Is(async () =>
		{
			if (Kayit.P.Elmas < LoncaKurmaFiyati) { Mesaj("Yeterli elmas yok"); return; }
			if (ad.Text.Trim().Length < 3 || et.Text.Trim().Length < 2) { Mesaj("Ad en az 3, etiket en az 2 karakter"); return; }
			int.TryParse(mk.Text.Trim(), out int min);
			await Sunucu.Rpc("lonca_kur", new() { ["p_ad"] = ad.Text.Trim(), ["p_etiket"] = et.Text.Trim(), ["p_aciklama"] = ac.Text.Trim(), ["p_min_kupa"] = min });
			Kayit.P.Elmas -= LoncaKurmaFiyati;
			Kayit.Kaydet();
			await Sunucu.BenYenile();
			Mesaj("Loncan kuruldu!");
			Yenile();
			LoncaPenceresi();
		}), SosyalYesil, 24));
		v.AddChild(Ui.Dugme("Vazgeç", LoncaPenceresi, Gri, 22));
		Pencere(v);
	}

	void LoncaDetay(long id)
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Yükleniyor…", 24, Ui.Soluk, HorizontalAlignment.Center));
		Pencere(v);
		Is(async () =>
		{
			var d = await Sunucu.Sozluk("lonca_detay", new() { ["p_id"] = id });
			if (d == null) { PencereKapat(); Mesaj("Lonca bulunamadı"); await Sunucu.BenYenile(); return; }
			long benimLonca = Sunucu.I(Sunucu.Ben, "lonca_id");
			bool uyeyim = benimLonca == id;
			string rutbem = uyeyim ? Sunucu.S(Sunucu.Ben, "lonca_rutbe") : "";
			var (w, liste) = Kabuk($"[{Sunucu.S(d, "etiket")}] {Sunucu.S(d, "ad")}", 560);
			var uyeler = d["uyeler"].AsGodotArray();
			var bilgi = Ui.Etiket($"{uyeler.Count}/30 üye  •  Bu hafta {Sunucu.I(d, "puan"):N0} puan  •  En az {Sunucu.I(d, "min_kupa")} kupa", 19, Ui.Soluk, HorizontalAlignment.Center);
			w.AddChild(bilgi); w.MoveChild(bilgi, 1);
			string acik = Sunucu.S(d, "aciklama");
			if (acik != "") { var a = Ui.Etiket(acik, 19, Colors.White, HorizontalAlignment.Center); a.AutowrapMode = TextServer.AutowrapMode.WordSmart; w.AddChild(a); w.MoveChild(a, 2); }
			string[] rutbeAd = { "lider", "yardimci", "uye" };
			foreach (var x in uyeler)
			{
				var u = x.AsGodotDictionary();
				string uid = Sunucu.S(u, "id"), uad = Sunucu.S(u, "ad"), r = Sunucu.S(u, "rutbe");
				string rt = r == "lider" ? "Lider" : r == "yardimci" ? "Yardımcı" : "Üye";
				var satir = Satir(r == "lider" ? new Color("3a3320") : new Color("1b2038"),
					Ui.Genis(Ui.Etiket($"{(Sunucu.B(u, "cevrimici") ? "● " : "")}{uad}  ({rt})\n{Sunucu.I(u, "kupa")} kupa • bu hafta +{Sunucu.I(u, "haftalik")}", 18)));
				var hb = (HBoxContainer)satir.GetChild(0);
				if (uid != Sunucu.OyuncuId)
				{
					hb.AddChild(Kucuk("Profil", () => OyuncuProfili(uid), Gri, 96));
					if (rutbem == "lider")
					{
						if (r == "uye") hb.AddChild(Kucuk("Terfi", () => Is(async () => { await Sunucu.Rpc("lonca_rutbe", new() { ["p_id"] = uid, ["p_rutbe"] = "yardimci" }); LoncaDetay(id); }), SosyalMavi, 96));
						else if (r == "yardimci") hb.AddChild(Kucuk("Lider yap", () => Onay($"Liderlik {uad} adlı oyuncuya devredilsin mi?", () => Is(async () => { await Sunucu.Rpc("lonca_rutbe", new() { ["p_id"] = uid, ["p_rutbe"] = "lider" }); await Sunucu.BenYenile(); LoncaDetay(id); })), SosyalMavi, 110));
					}
					if (rutbem == "lider" || (rutbem == "yardimci" && r == "uye"))
						hb.AddChild(Kucuk("At", () => Onay($"{uad} loncadan atılsın mı?", () => Is(async () => { await Sunucu.Rpc("lonca_at", new() { ["p_id"] = uid }); LoncaDetay(id); })), new Color("6a3a3a"), 70));
				}
				liste.AddChild(satir);
			}
			if (uyeyim)
			{
				var h = Ui.Yatay(8);
				w.AddChild(h);
				h.AddChild(Ui.Genis(Ui.Dugme("Lonca Sohbeti", () => SohbetPenceresi("", Sunucu.S(d, "ad"), true), SosyalMavi, 21)));
				h.AddChild(Ui.Genis(Ui.Dugme("Ayrıl", () => Onay(rutbem == "lider" ? "Loncadan ayrılırsan liderlik başka üyeye geçer (son üyeysen lonca kapanır). Emin misin?" : "Loncadan ayrılmak istiyor musun?",
					() => Is(async () => { await Sunucu.Rpc("lonca_ayril"); await Sunucu.BenYenile(); Mesaj("Loncadan ayrıldın"); Yenile(); })), new Color("6a3a3a"), 21)));
				if (rutbem is "lider" or "yardimci")
					w.AddChild(Ui.Dugme("Açıklamayı / kupa şartını düzenle", () =>
					{
						var pv = Ui.Dikey(10);
						pv.AddChild(Ui.Etiket("Lonca Ayarları", 28, Ui.Altin, HorizontalAlignment.Center));
						var ac = Alan("Açıklama", false, 200); ac.Text = acik;
						var mk = Alan("En az kupa", false, 5); mk.Text = Sunucu.I(d, "min_kupa").ToString();
						pv.AddChild(ac); pv.AddChild(mk);
						pv.AddChild(Ui.Dugme("Kaydet", () => Is(async () =>
						{
							int.TryParse(mk.Text.Trim(), out int min);
							await Sunucu.Rpc("lonca_duzenle", new() { ["p_aciklama"] = ac.Text.Trim(), ["p_min_kupa"] = min });
							LoncaDetay(id);
						}), SosyalYesil, 22));
						pv.AddChild(Ui.Dugme("Vazgeç", () => LoncaDetay(id), Gri, 22));
						Pencere(pv);
					}, Gri, 19, 54));
			}
			else if (benimLonca == 0)
				w.AddChild(Ui.Dugme("Loncaya Katıl", () => Is(async () =>
				{
					await Sunucu.Rpc("lonca_katil", new() { ["p_id"] = id });
					await Sunucu.BenYenile();
					Mesaj("Loncaya katıldın!");
					Yenile();
					LoncaDetay(id);
				}), SosyalYesil, 24));
			w.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
			Pencere(w);
		});
	}

	// ================================================================ İKİLİ SAVAŞ
	/// <summary>Bizim taraf: kendi takımının en güçlü 3 kartı + eşinin savunmasının en güçlü 2 kartı (aynı kart iki kez olmaz).</summary>
	static List<TakimUyesi> IkiliTakim(List<TakimUyesi> a, List<TakimUyesi> b)
	{
		int G(TakimUyesi u) => Kayit.TakimGucu(new[] { u });
		var l = a.OrderByDescending(G).Take(3).ToList();
		foreach (var u in b.OrderByDescending(G)) if (l.Count < 5 && !l.Any(x => x.Id == u.Id)) l.Add(u);
		foreach (var u in a.OrderByDescending(G)) if (l.Count < 5 && !l.Any(x => x.Id == u.Id)) l.Add(u);
		return l;
	}

	void IkiliPenceresi()
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Yükleniyor…", 24, Ui.Soluk, HorizontalAlignment.Center));
		Pencere(v);
		Is(async () =>
		{
			var t = await Sunucu.Sozluk("ikili_takimim");
			var w = Ui.Dikey(12);
			w.AddChild(Ui.Etiket("İkili Savaş (2'ye 2)", 32, Ui.Altin, HorizontalAlignment.Center));
			w.AddChild(Ui.Etiket("Bir eşle takım ol: senin en güçlü 3 kartın + eşinin en güçlü 2 kartı başka bir ikiliye karşı savaşır. Eşinin çevrimiçi olması gerekmez.", 18, Ui.Soluk, HorizontalAlignment.Center));
			string durum = Sunucu.S(t, "durum");
			if (t == null || durum == "" || durum == "davet")
			{
				if (durum == "davet") w.AddChild(Ui.Etiket("Davetin gönderildi, eşinin kabul etmesi bekleniyor.", 20, Ui.Elmas, HorizontalAlignment.Center));
				var davetler = await Sunucu.Liste("ikili_davetlerim");
				foreach (var x in davetler)
				{
					var d = x.AsGodotDictionary();
					long did = (long)d["id"].AsDouble();
					w.AddChild(Satir(new Color("3a2232"), Ui.Genis(Ui.Etiket($"{Sunucu.S(d, "ad")} seni ikili takıma çağırıyor (güç {Sunucu.I(d, "guc"):N0})", 19)),
						Kucuk("Kabul", () => Is(async () => { await Sunucu.Rpc("ikili_yanit", new() { ["p_id"] = did, ["p_kabul"] = true }); IkiliPenceresi(); }), SosyalYesil, 100),
						Kucuk("Hayır", () => Is(async () => { await Sunucu.Rpc("ikili_yanit", new() { ["p_id"] = did, ["p_kabul"] = false }); IkiliPenceresi(); }), new Color("6a3a3a"), 90)));
				}
				w.AddChild(Ui.Dugme("Rastgele Eş Bul", () => Is(async () =>
				{
					var r = (await Sunucu.Rpc("ikili_rastgele")).AsString();
					Mesaj(r == "eslesti" ? "Bir eş bulundu!" : "Sıraya girdin; biri katılınca takım olacaksınız");
					IkiliPenceresi();
				}), new Color("a03f5f"), 24));
				w.AddChild(Ui.Dugme("Arkadaşını Davet Et", () => Is(async () =>
				{
					var l = await Sunucu.Liste("arkadaslarim");
					var (pv, liste) = Kabuk("Kimi davet edeceksin?", 600);
					int n = 0;
					foreach (var x in l)
					{
						var d = x.AsGodotDictionary();
						if (Sunucu.S(d, "durum") != "kabul") continue;
						n++;
						string id = Sunucu.S(d, "id"), ad = Sunucu.S(d, "ad");
						liste.AddChild(Satir(new Color("1b2038"), Ui.Genis(Ui.Etiket($"{ad}  •  Güç {Sunucu.I(d, "guc"):N0}", 20)),
							Kucuk("Davet", () => Is(async () => { await Sunucu.Rpc("ikili_davet", new() { ["p_arkadas"] = id }); Mesaj($"{ad} davet edildi"); IkiliPenceresi(); }), SosyalYesil, 110)));
					}
					if (n == 0) liste.AddChild(Ui.Etiket("Önce arkadaş eklemelisin.", 21, Ui.Soluk, HorizontalAlignment.Center));
					pv.AddChild(Ui.Dugme("Geri", IkiliPenceresi, Gri, 22));
					Pencere(pv);
				}), SosyalMavi, 22));
				w.AddChild(Ui.Dugme("İkili Sıralaması", () => CevrimiciSiralama("ikili"), SosyalMor, 21));
			}
			else if (durum == "bekliyor")
			{
				w.AddChild(Ui.Etiket("Eş aranıyor… Biri sıraya girdiğinde otomatik eşleşeceksiniz. Bu pencereyi kapatabilirsin.", 21, Ui.Elmas, HorizontalAlignment.Center));
				w.AddChild(Ui.Dugme("Yenile", IkiliPenceresi, SosyalMavi, 22));
				w.AddChild(Ui.Dugme("Aramayı İptal Et", () => Is(async () => { await Sunucu.Rpc("ikili_ayril"); IkiliPenceresi(); }), new Color("6a3a3a"), 21));
			}
			else
			{
				var u1 = t["uye1"].AsGodotDictionary();
				var u2 = t["uye2"].AsGodotDictionary();
				bool benBir = Sunucu.S(u1, "id") == Sunucu.OyuncuId;
				var es = benBir ? u2 : u1;
				var esTakim = Sunucu.TakimCoz(es["savunma"]);
				var bizim = IkiliTakim(Kayit.TakimUyeleri().Where(u => u != null).ToList(), esTakim);
				w.AddChild(Ui.Etiket($"Takımın: {Sunucu.KullaniciAdi} & {Sunucu.S(es, "ad")}   •   {Sunucu.I(t, "puan")} puan", 23, Ui.Yesil, HorizontalAlignment.Center));
				w.AddChild(Ui.Etiket("Savaşacak 5 kart", 20));
				w.AddChild(MiniTakim(bizim, 112));
				w.AddChild(Ui.Dugme("Rakip Bul ve Savaş", () => Is(async () =>
				{
					if (bizim.Count < 5) { Mesaj("Takımında en az 3 kart olmalı"); return; }
					var r = await Sunucu.Sozluk("ikili_rakip_bul");
					if (r == null) { Mesaj("Şu an rakip ikili yok, biraz sonra tekrar dene"); return; }
					var karsi = IkiliTakim(Sunucu.TakimCoz(r["savunma1"]), Sunucu.TakimCoz(r["savunma2"]));
					long rid = (long)r["id"].AsDouble();
					string radi = $"{Sunucu.S(r, "ad1")} & {Sunucu.S(r, "ad2")}";
					PencereKapat();
					SavasBaslat(karsi, $"İkili: {radi}", Rakipler.TakimArka(karsi), async kazandi =>
					{
						var s = await Sunucu.Sozluk("ikili_sonuc", new() { ["p_rakip"] = rid, ["p_kazandi"] = kazandi });
						int pd = Sunucu.I(s, "puan_degisim");
						var l = new List<string> { $"{(pd >= 0 ? "+" : "")}{pd} ikili puan  (toplam {Sunucu.I(s, "puan")})" };
						if (kazandi) l.Add("Haftalık sıralamaya +5 kupa puanı");
						return l;
					}, false, bizim);
				}), new Color("c0582f"), 26, 80));
				w.AddChild(Ui.Dugme("İkili Sıralaması", () => CevrimiciSiralama("ikili"), SosyalMor, 21));
				w.AddChild(Ui.Dugme("Takımı Dağıt", () => Onay("İkili takım dağıtılsın mı? Puanınız silinir.", () => Is(async () => { await Sunucu.Rpc("ikili_ayril"); IkiliPenceresi(); })), new Color("6a3a3a"), 19, 54));
			}
			w.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
			Pencere(w);
		});
	}

	// ================================================================ SAVUNMA KAYITLARI
	void SavunmaKayitlari()
	{
		var (v, liste) = Kabuk("Savunma Kayıtları", 700);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, Gri, 22));
		liste.AddChild(Ui.Etiket("Yükleniyor…", 22, Ui.Soluk, HorizontalAlignment.Center));
		Pencere(v);
		Is(async () =>
		{
			var l = await Sunucu.Liste("savunma_kayitlari");
			if (!IsInstanceValid(liste)) return;
			foreach (var c in liste.GetChildren()) c.QueueFree();
			if (l.Count == 0) liste.AddChild(Ui.Etiket("Henüz kimse sana saldırmadı.", 21, Ui.Soluk, HorizontalAlignment.Center));
			foreach (var x in l)
			{
				var d = x.AsGodotDictionary();
				bool onlarKazandi = Sunucu.B(d, "kazandi");
				string tur = Sunucu.S(d, "tur") == "dostluk" ? "dostluk maçı" : "arena";
				string sonuc = onlarKazandi ? (tur == "arena" ? "kazandı (-6 kupa)" : "kazandı") : (tur == "arena" ? "savunman kazandı (+3 kupa)" : "savunman kazandı");
				liste.AddChild(Satir(onlarKazandi ? new Color("3a2222") : new Color("223a22"),
					Ui.Genis(Ui.Etiket($"{Sunucu.S(d, "ad")} • {tur}\n{sonuc}", 19)), Ui.Etiket(Zaman(Sunucu.S(d, "zaman")), 17, Ui.Soluk)));
			}
		});
	}
}
