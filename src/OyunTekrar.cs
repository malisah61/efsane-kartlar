using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Savaş tekrarları: son 10 savaş + 5 favori yeniden izlenir (Oyun partial, 2026-11-25 gece).</summary>
public partial class Oyun
{
	SavasKaydi _sonKayit;   // son oynanan savaşın tekrar kaydı (sonuç penceresindeki "Tekrar İzle")
	string _tekrarFiltre = "hepsi";

	static List<TakimUyesi> GecerliTakim(List<TakimUyesi> t) =>
		t.Select(u => u != null && Veri.Harita.ContainsKey(u.Id) ? u : null).ToList();

	void TekrarlarPenceresi()
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Savaş Tekrarları", 34, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"Son {Tekrar.SonSayi} savaşın saklanır. Yıldızla en fazla {Tekrar.FavoriSayi} savaşı kalıcı yap.", 17, Ui.Soluk, HorizontalAlignment.Center));

		// filtre: hepsi / zaferler / yenilgiler / favoriler
		var f = Ui.Yatay(6);
		foreach (var (kod, ad) in new[] { ("hepsi", "Hepsi"), ("zafer", "Zafer"), ("yenilgi", "Yenilgi"), ("favori", "★ Favori") })
		{
			var k = kod;
			f.AddChild(Ui.Genis(Ui.Dugme(ad, () => { _tekrarFiltre = k; TekrarlarPenceresi(); },
				_tekrarFiltre == k ? new Color("3b6fd8") : new Color("444c6a"), 18, 46)));
		}
		v.AddChild(f);

		var liste = Ui.Dikey(8);
		var secili = Tekrar.Liste.Where(k => _tekrarFiltre switch
		{
			"zafer" => k.Kazandi, "yenilgi" => !k.Kazandi, "favori" => k.Favori, _ => true,
		}).ToList();
		if (secili.Count == 0)
			liste.AddChild(Ui.Etiket(Tekrar.Liste.Count == 0 ? "Henüz kayıtlı savaş yok. Bir savaş yap, burada belirsin." : "Bu filtrede savaş yok.", 21, Ui.Soluk, HorizontalAlignment.Center));
		foreach (var k in secili)
		{
			var kk = k;
			var p = Ui.Panel(k.Kazandi ? new Color("1f3324") : new Color("3a2222"), 10, 12);
			var pv = Ui.Dikey(6);
			p.AddChild(pv);
			var ust = Ui.Yatay(6);
			pv.AddChild(ust);
			string tur = Tekrar.TurAd.TryGetValue(k.Tur ?? "", out var ta) ? ta : "Savaş";
			ust.AddChild(Ui.Genis(Ui.Etiket($"{(k.Favori ? "★ " : "")}{k.Baslik}", 20, Colors.White)));
			var zl = Ui.Etiket($"{tur} • {k.Zaman}", 16, Ui.Soluk, HorizontalAlignment.Right);
			zl.AutowrapMode = TextServer.AutowrapMode.Off;
			ust.AddChild(zl);
			var takimlar = Ui.Yatay(4);
			takimlar.Alignment = BoxContainer.AlignmentMode.Center;
			takimlar.AddChild(MiniTakim(GecerliTakim(k.A), 48));
			var vs = Ui.Etiket("vs", 18, Ui.Soluk);
			vs.AutowrapMode = TextServer.AutowrapMode.Off;
			takimlar.AddChild(vs);
			takimlar.AddChild(MiniTakim(GecerliTakim(k.B), 48));
			pv.AddChild(takimlar);
			var alt = Ui.Yatay(8);
			pv.AddChild(alt);
			alt.AddChild(Ui.Genis(Ui.Etiket(k.Kazandi ? "ZAFER" : "YENİLGİ", 20, k.Kazandi ? Ui.Yesil : Ui.Kirmizi), 0.6f));
			alt.AddChild(Ui.Genis(Ui.Dugme("İzle", () => { PencereKapat(); TekrarIzle(kk, false); }, new Color("2f8a57"), 19, 48)));
			alt.AddChild(Ui.Genis(Ui.Dugme(k.Favori ? "★ Kaldır" : "☆ Sakla", () =>
			{
				if (!Tekrar.FavoriDegistir(kk)) Mesaj($"En fazla {Tekrar.FavoriSayi} favori");
				TekrarlarPenceresi();
			}, new Color("8a6a2a"), 18, 48)));
			alt.AddChild(Ui.Genis(Ui.Dugme("Sil", () => { Tekrar.Sil(kk); TekrarlarPenceresi(); }, new Color("6a4c4c"), 18, 48), 0.6f));
			liste.AddChild(p);
		}
		var kay = Ui.Kaydirma(liste);
		kay.CustomMinimumSize = new Vector2(0, Math.Min(820, Math.Max(160, secili.Count * 210)));
		v.AddChild(kay);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22, 58));
		Pencere(v);
	}

	/// <summary>Kayıtlı bir savaşı aynı tohum ve takımlarla yeniden oynatır. Ödül, enerji, görev yok.</summary>
	void TekrarIzle(SavasKaydi k, bool sonuctan)
	{
		var a = GecerliTakim(k.A); var b = GecerliTakim(k.B);
		if (a.All(x => x == null) || b.All(x => x == null)) { Mesaj("Bu tekrar artık oynatılamıyor"); return; }
		var s = new Savas(a, b, k.Tohum, k.BonusMit, k.BonusOran);
		s.Oyna();
		var ekran = new SavasEkrani { S = s, Baslik = "Tekrar • " + k.Baslik, Arka = k.Arka, Boss = k.Boss };
		ekran.Bitti = kazandi =>
		{
			Ses.Muzik("menu");
			ekran.QueueFree();
			_sonSavas = s;
			_sonSonuc = null;
			var v = Ui.Dikey(12);
			v.AddChild(Ui.Etiket("Tekrar Bitti", 40, Ui.Altin, HorizontalAlignment.Center));
			v.AddChild(Ui.Etiket($"{k.Baslik} — {(kazandi ? "ZAFER" : "YENİLGİ")}", 24, kazandi ? Ui.Yesil : Ui.Kirmizi, HorizontalAlignment.Center));
			if (kazandi != k.Kazandi)
				v.AddChild(Ui.Etiket("Oyun güncellendiği için (kart değerleri değişti) bu tekrar asıl savaştan farklı oynandı.", 17, Ui.Soluk, HorizontalAlignment.Center));
			var sa = Ui.Yatay(10);
			sa.AddChild(Ui.Genis(Ui.Dugme("İstatistikler", () => { IstatistikPenceresi(); }, new Color("6a4a8a"), 21, 58)));
			sa.AddChild(Ui.Genis(Ui.Dugme("Yeniden İzle", () => { PencereKapat(); TekrarIzle(k, sonuctan); }, new Color("2f6f7a"), 21, 58)));
			v.AddChild(sa);
			var sb = Ui.Yatay(10);
			sb.AddChild(Ui.Genis(Ui.Dugme("Tüm Tekrarlar", TekrarlarPenceresi, new Color("3b6fd8"), 21, 58)));
			sb.AddChild(Ui.Genis(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 21, 58)));
			v.AddChild(sb);
			Pencere(v);
		};
		AddChild(ekran);
	}

	public void OnizlemeTekrarlar() => TekrarlarPenceresi();
}
