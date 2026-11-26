using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace EfsaneKartlar;

/// <summary>Kervansaray ekranı (boşta kazanç) ve ana sayfa düğmesi.</summary>
public partial class Oyun
{
	static string SureYazi(long sn)
	{
		long s = sn / 3600, d = sn % 3600 / 60;
		return s > 0 ? $"{s} sa {d} dk" : $"{d} dk";
	}

	Control KervanDugmesi()
	{
		var b = Kervan.Biriken();
		bool dolu = Kervan.Dolu;
		string alt = Kervan.Gecen() < Kervan.EnAzSaniye ? "Kervan yolda…" : $"+{Ui.Sayi(b.altin)} altın" + (b.oz > 0 ? $", {b.oz} ruh özü" : "") + (dolu ? "  •  DOLU!" : "");
		var d = Ui.Dugme($"Kervansaray\n{alt}", KervanPenceresi, dolu ? new Color("b0782a") : new Color("6a5130"), 21, 78);
		return d;
	}

	void KervanPenceresi()
	{
		var p = Kayit.P;
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket("Kervansaray", 36, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket("Sen yokken kervanın İpek Yolu'nda ganimet toplar; takımındaki kahramanlar da yolda ustalık kazanır. En fazla 12 saat birikir.", 18, Ui.Soluk, HorizontalAlignment.Center));

		long gecen = Kervan.Gecen();
		var b = Kervan.Biriken();
		var pan = Ui.Panel(Ui.PanelAcik, 14);
		v.AddChild(pan);
		var pv = Ui.Dikey(8);
		pan.AddChild(pv);
		pv.AddChild(Ui.Etiket($"Yolda geçen: {SureYazi(gecen)} / {Kervan.MaxSaat} sa" + (Kervan.Dolu ? "  (dolu — ganimet artık birikmiyor)" : ""), 20, Kervan.Dolu ? Ui.Altin : Colors.White));
		var bar = new ProgressBar { MinValue = 0, MaxValue = Kervan.MaxSaat * 3600, Value = gecen, ShowPercentage = false, CustomMinimumSize = new Vector2(0, 16) };
		bar.AddThemeStyleboxOverride("background", Ui.Kutu(new Color("15182a"), 8, null, 0, 0));
		bar.AddThemeStyleboxOverride("fill", Ui.Kutu(new Color("d9a441"), 8, null, 0, 0));
		pv.AddChild(bar);
		var izgara = new GridContainer { Columns = 3 };
		izgara.AddThemeConstantOverride("h_separation", 10);
		pv.AddChild(izgara);
		void Kutu(string bas, string deger, Color renk)
		{
			var k = Ui.Panel(new Color("1d2340"), 8, 10);
			k.CustomMinimumSize = new Vector2(190, 0);
			var kv = Ui.Dikey(2);
			k.AddChild(kv);
			kv.AddChild(Ui.Etiket(bas, 16, Ui.Soluk, HorizontalAlignment.Center));
			kv.AddChild(Ui.Etiket(deger, 24, renk, HorizontalAlignment.Center));
			izgara.AddChild(k);
		}
		Kutu("Altın", Ui.Sayi(b.altin), Ui.Altin);
		Kutu("Ruh özü", b.oz.ToString(), new Color("9fd0ff"));
		Kutu("Ustalık", $"+{b.xp} puan", new Color("c9a5f5"));
		pv.AddChild(Ui.Etiket($"Saatte {Kervan.AltinSaat(p)} altın, {Kervan.OzSaat(p):0.##} ruh özü, takımdaki her kahramana 1 ustalık puanı. Hikâyede ve kulede ilerledikçe kervan büyür.", 16, Ui.Soluk));

		var topla = Ui.Dugme(gecen < Kervan.EnAzSaniye ? "Kervan yeni yola çıktı" : "Ganimeti Topla", () =>
		{
			var s = Kervan.Topla();
			if (s == null) return;
			Ses.Efekt("para");
			PencereKapat(); Yenile();
			KervanSonuc("Ganimet toplandı!", s.Value.g, s.Value.satir);
		}, new Color("2f8a57"), 26, 70);
		topla.Disabled = gecen < Kervan.EnAzSaniye;
		v.AddChild(topla);

		var hg = Kervan.Ganimet(p, Kervan.HizliSaat);
		var hizli = Ui.Dugme($"Hızlı Sefer: {Kervan.HizliSaat} saatlik ganimet hemen ({Kervan.HizliFiyat} elmas)\n+{Ui.Sayi(hg.altin)} altın, {hg.oz} ruh özü, +{hg.xp} ustalık  •  bugün {Kervan.HizliKalan} hak", () =>
		{
			var s = Kervan.HizliSefer();
			if (s == null) { Mesaj(Kervan.HizliKalan <= 0 ? "Bugünkü hızlı sefer hakkın bitti" : "Yeterli elmas yok"); return; }
			Ses.Efekt("para");
			PencereKapat(); Yenile();
			KervanSonuc("Hızlı sefer döndü!", s.Value.g, s.Value.satir);
		}, new Color("2f6f9a"), 18, 70);
		hizli.Disabled = Kervan.HizliKalan <= 0 || p.Elmas < Kervan.HizliFiyat;
		v.AddChild(hizli);
		v.AddChild(Ui.Dugme("Kapat", PencereKapat, new Color("444c6a"), 22));
		Pencere(v);
	}

	void KervanSonuc(string baslik, (int altin, int oz, int xp) g, List<string> satir)
	{
		var v = Ui.Dikey(10);
		v.AddChild(Ui.Etiket(baslik, 34, Ui.Altin, HorizontalAlignment.Center));
		v.AddChild(Ui.Etiket($"+{g.altin:N0} altın", 26, Ui.Altin, HorizontalAlignment.Center));
		if (g.oz > 0) v.AddChild(Ui.Etiket($"+{g.oz} ruh özü", 24, new Color("9fd0ff"), HorizontalAlignment.Center));
		if (g.xp > 0) v.AddChild(Ui.Etiket($"Takımdaki kahramanlara +{g.xp} ustalık puanı", 21, new Color("c9a5f5"), HorizontalAlignment.Center));
		foreach (var s in satir.Take(4)) v.AddChild(Ui.Etiket(s, 19, Colors.White, HorizontalAlignment.Center));
		if (satir.Count > 4) v.AddChild(Ui.Etiket($"…ve {satir.Count - 4} ustalık atlaması daha!", 19, Colors.White, HorizontalAlignment.Center));
		v.AddChild(Ui.Dugme("Tamam", PencereKapat, new Color("3b6fd8"), 24));
		Pencere(v);
	}
}
