using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>Gerçek parayla satılan bir ürün. Kimlik Google Play Console'daki ürün kimliğiyle AYNI olmalı.</summary>
public class Urun
{
	public string Id, Ad, Aciklama, OnerilenFiyat;   // fiyatı Play Console belirler; burada yalnızca öneri/yedek gösterim
	public int Elmas, Parsomen, Altin;
	public bool TekSeferlik, AylikKart, EpikKart;
	public decimal TlFiyat;                          // simülasyon ve raporlar için
}

/// <summary>Ürün kataloğu ve satın alma sonrası ödül verme (Google Play ödemesi onaylandıktan sonra çağrılır).</summary>
public static class Magaza
{
	public const int AylikKartGunluk = 100;
	public const int AylikKartGun = 30;

	public static readonly List<Urun> Urunler = new()
	{
		new Urun { Id = "baslangic_paketi", Ad = "Başlangıç Paketi", Aciklama = "1.000 elmas + 10 parşömen + 20.000 altın + 1 epik kahraman (tek seferlik)",
			Elmas = 1000, Parsomen = 10, Altin = 20000, EpikKart = true, TekSeferlik = true, OnerilenFiyat = "₺39,99", TlFiyat = 39.99m },
		new Urun { Id = "aylik_kart", Ad = "Aylık Kart", Aciklama = "Hemen 300 elmas + 30 gün boyunca her gün 100 elmas (toplam 3.300)",
			Elmas = 300, AylikKart = true, OnerilenFiyat = "₺89,99", TlFiyat = 89.99m },
		new Urun { Id = "elmas_300", Ad = "Elmas Kesesi", Aciklama = "300 elmas", Elmas = 300, OnerilenFiyat = "₺29,99", TlFiyat = 29.99m },
		new Urun { Id = "elmas_1100", Ad = "Elmas Torbası", Aciklama = "1.100 elmas (+%10 bonus)", Elmas = 1100, OnerilenFiyat = "₺99,99", TlFiyat = 99.99m },
		new Urun { Id = "elmas_2400", Ad = "Elmas Sandığı", Aciklama = "2.400 elmas (+%20 bonus)", Elmas = 2400, OnerilenFiyat = "₺199,99", TlFiyat = 199.99m },
		new Urun { Id = "elmas_6500", Ad = "Elmas Hazinesi", Aciklama = "6.500 elmas (+%30 bonus)", Elmas = 6500, OnerilenFiyat = "₺449,99", TlFiyat = 449.99m },
	};

	public static Urun Bul(string id) => Urunler.FirstOrDefault(u => u.Id == id);

	/// <summary>Elmas paketinin ilk alımında elmas iki katı verilir.</summary>
	public static bool IlkAlimBonusu(Urun u) => u.Id.StartsWith("elmas_") && !Kayit.P.AlinanUrunler.Contains(u.Id);

	public static bool SatinAlinabilir(Urun u) => !(u.TekSeferlik && Kayit.P.AlinanUrunler.Contains(u.Id))
		&& !(u.AylikKart && AylikKartKalan() > 3);

	/// <summary>Ödeme onaylandı: ödülü ver. Dönüş: kullanıcıya gösterilecek özet.</summary>
	public static string Ver(string id, Random r = null)
	{
		var u = Bul(id);
		if (u == null) return "";
		var p = Kayit.P;
		var parca = new List<string>();
		int elmas = u.Elmas * (IlkAlimBonusu(u) ? 2 : 1);
		p.Elmas += elmas; if (elmas > 0) parca.Add($"+{elmas:N0} elmas");
		p.Parsomen += u.Parsomen; if (u.Parsomen > 0) parca.Add($"+{u.Parsomen} parşömen");
		p.Altin += u.Altin; if (u.Altin > 0) parca.Add($"+{u.Altin:N0} altın");
		if (u.EpikKart)
		{
			r ??= new Random();
			var havuz = Veri.Kartlar.Where(k => k.Nadirlik == "epik" && Kayit.Sahip(k.Id) == null).ToList();
			if (havuz.Count == 0) havuz = Veri.Kartlar.Where(k => k.Nadirlik == "epik").ToList();
			var k = havuz[r.Next(havuz.Count)];
			Kayit.KartEkle(k.Id);
			parca.Add($"Epik kahraman: {k.Ad}");
		}
		if (u.AylikKart)
		{
			int baslangic = Math.Max(GunNo(Kayit.Bugun()), GunNo(p.AylikKartBitis));
			p.AylikKartBitis = GunYazi(baslangic + AylikKartGun);
			parca.Add($"Aylık kart {AylikKartKalan()} gün aktif");
		}
		if (!p.AlinanUrunler.Contains(u.Id)) p.AlinanUrunler.Add(u.Id);
		p.ToplamHarcamaKurus += (long)(u.TlFiyat * 100);
		Kayit.Kaydet();
		return string.Join(", ", parca);
	}

	// ------------------------------------------------------------ aylık kart
	public static int AylikKartKalan() => Math.Max(0, GunNo(Kayit.P.AylikKartBitis) - GunNo(Kayit.Bugun()));
	public static bool AylikKartAlinabilir() => AylikKartKalan() > 0 && Kayit.P.AylikKartSonAlim != Kayit.Bugun();

	public static bool AylikKartAl()
	{
		if (!AylikKartAlinabilir()) return false;
		Kayit.P.AylikKartSonAlim = Kayit.Bugun();
		Kayit.P.Elmas += AylikKartGunluk;
		Kayit.Kaydet();
		return true;
	}

	static readonly DateTime Sifir = new(2020, 1, 1);
	public static int GunNo(string tarih) =>
		DateTime.TryParseExact(tarih ?? "", "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var t)
			? (int)(t - Sifir).TotalDays : 0;
	public static string GunYazi(int gun) => Sifir.AddDays(gun).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
