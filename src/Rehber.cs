using System;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>
/// Başlangıç Yolu (yeni oyuncu rehberi): ilk saatlerde oyunun bütün sistemlerini sırayla tanıtan
/// 10 adımlık görev zinciri. Her adım oyuncunun kaydından anlaşılır; ödül sırayla alınır.
/// Ana sayfanın en üstünde parlayan bir panel olarak görünür, bitince kaybolur.
/// </summary>
public static class Rehber
{
	public record Adim(string Ad, string Ipucu, Func<Profil, bool> Tamam, string Git, int Altin = 0, int Elmas = 0, int Parsomen = 0)
	{
		public string OdulYazi()
		{
			var l = new System.Collections.Generic.List<string>();
			if (Altin > 0) l.Add($"{Altin:N0} altın");
			if (Elmas > 0) l.Add($"{Elmas} elmas");
			if (Parsomen > 0) l.Add($"{Parsomen} parşömen");
			return string.Join(", ", l);
		}
	}

	// Git: "hikaye", "kartlar", "takim", "cagir", "arena", "kule", "etkinlik"
	public static readonly Adim[] Adimlar =
	{
		new("İlk hikâye savaşını kazan", "Aşağıdaki 'Savaşa Başla' düğmesine bas; savaş kendiliğinden oynar.",
			p => p.EnIyiAsama >= 1, "hikaye", Altin: 500),
		new("Bir kartını seviye atlat", "Kartlar sekmesinde bir karta dokun, 'Seviye Atlat'a bas.",
			p => p.Kartlar.Any(k => k.Seviye >= 2), "kartlar", Elmas: 30),
		new("Yeni bir kart çağır", "Çağır sekmesinde parşömenle ya da elmasla kart çağır.",
			p => p.Kartlar.Count >= 7, "cagir", Altin: 1000),
		new("Arenada bir savaş yap", "Arena'da bir rakip seç. Rakip çevrimiçi olmasa da savunma takımıyla savaşırsın.",
			p => p.ArenaGalibiyet + p.ArenaMaglubiyet >= 1, "arena", Elmas: 50),
		new("Efsaneler Kulesi'nin 1. katını geç", "Kule enerji harcamaz; her yeni kat ödül verir.",
			p => p.KuleKat >= 1, "kule", Altin: 1500),
		new("İlk boss'u yen (Aşama 6)", "Kartlarını yükselt; element üstünlüğü: Ateş > Doğa > Su > Ateş.",
			p => p.EnIyiAsama >= 6, "hikaye", Parsomen: 1),
		new("Takımında bir efsane bağı kur", "Takım ekranındaki 'Takım Simyası' panelinde eksik kartı gör, birlikte diz.",
			p => Veri.AktifBaglar(p.Takim.Where(x => x != null)).Count > 0, "takim", Elmas: 80),
		new("Bir kartını 2★ yap", "Kopyası biriken kartın detayında 'Yıldız Atlat'a bas (ruh özüyle de kopya alınır).",
			p => p.Kartlar.Any(k => k.Yildiz >= 2), "kartlar", Elmas: 100),
		new("Haftalık etkinlikte 100 puana ulaş", "Zaferler etkinlik puanı verir; takımdaki etkinlik kartları puanı artırır.",
			p => p.EtkinlikPuan >= 100, "etkinlik", Altin: 2000),
		new("Aşama 12'yi geç", "Başlangıç Yolu'nun son adımı: büyük ödül seni bekliyor!",
			p => p.EnIyiAsama >= 12, "hikaye", Elmas: 150, Parsomen: 2),
	};

	public static bool Bitti => Kayit.P.RehberAdim >= Adimlar.Length;
	public static Adim Simdiki => Bitti ? null : Adimlar[Kayit.P.RehberAdim];
	public static bool OdulHazir => !Bitti && Simdiki.Tamam(Kayit.P);

	/// <summary>Sıradaki adımın ödülünü verir. Dönüş: ödül yazısı (hazır değilse null).</summary>
	public static string OdulAl()
	{
		if (!OdulHazir) return null;
		var a = Simdiki;
		var p = Kayit.P;
		p.Altin += a.Altin; p.Elmas += a.Elmas; p.Parsomen += a.Parsomen;
		p.RehberAdim++;
		Kayit.Kaydet();
		return a.OdulYazi();
	}
}
