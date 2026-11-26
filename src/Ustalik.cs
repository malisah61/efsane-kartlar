using System;
using System.Collections.Generic;
using System.Linq;

namespace EfsaneKartlar;

/// <summary>
/// Kahraman Ustalığı (Raid'deki mastery benzeri, sade hâli): kart savaştıkça ustalık puanı (XP) kazanır.
/// 5 ustalık seviyesi; her seviye kalıcı +%1 can ve saldırı verir ve bir kez ödül açar.
/// Puan: zafer 3 (hikâye, arena, kule, mühür, muhafız), yenilgi 1 (yalnızca enerji/hak harcanan hikâye ve arena),
/// Kervansaray saat başına takımdaki her karta 1. Dostluk ve ikili savaşlar puan vermez.
/// </summary>
public static class Ustalik
{
	public const int MaxSeviye = 5;
	/// <summary>Seviye için gereken toplam puan (0. eleman seviye 0).</summary>
	public static readonly int[] Esik = { 0, 100, 300, 650, 1200, 2000 };
	public const float StatOran = 0.01f;
	public const int ZaferXp = 3, YenilgiXp = 1;

	public static readonly string[] Unvanlar = { "Acemi", "Çırak", "Kalfa", "Usta", "Üstat", "Efsane Ustası" };
	/// <summary>Seviye ödülleri (bir kez): altın, ruh özü, elmas, parşömen.</summary>
	public static readonly (int altin, int oz, int elmas, int parsomen)[] Oduller =
	{
		(0, 0, 0, 0), (400, 0, 0, 0), (0, 10, 0, 0), (800, 0, 10, 0), (0, 20, 0, 0), (0, 0, 25, 1),
	};

	public static int Seviye(int xp)
	{
		int s = 0;
		while (s < MaxSeviye && xp >= Esik[s + 1]) s++;
		return s;
	}
	public static int Seviye(SahipKart k) => k == null ? 0 : Seviye(k.UstalikXp);
	public static float Carpan(int seviye) => 1f + StatOran * Math.Clamp(seviye, 0, MaxSeviye);

	public static string OdulYazi(int sv)
	{
		var o = Oduller[Math.Clamp(sv, 0, MaxSeviye)];
		var l = new List<string>();
		if (o.altin > 0) l.Add($"{o.altin} altın");
		if (o.oz > 0) l.Add($"{o.oz} ruh özü");
		if (o.elmas > 0) l.Add($"{o.elmas} elmas");
		if (o.parsomen > 0) l.Add($"{o.parsomen} parşömen");
		return string.Join(", ", l);
	}

	/// <summary>Kartlara puan ekler, atlanan seviyelerin ödüllerini verir. Dönüş: seviye atlama satırları. Kaydetmez.</summary>
	public static List<string> XpEkle(IEnumerable<string> idler, int xp)
	{
		var satir = new List<string>();
		if (xp <= 0) return satir;
		var p = Kayit.P;
		foreach (var id in idler.Where(i => i != null).Distinct())
		{
			var s = Kayit.Sahip(id);
			if (s == null) continue;
			int once = Seviye(s.UstalikXp);
			s.UstalikXp = Math.Min(Esik[MaxSeviye], s.UstalikXp + xp);
			int sonra = Seviye(s.UstalikXp);
			for (int sv = once + 1; sv <= sonra; sv++)
			{
				var o = Oduller[sv];
				p.Altin += o.altin; p.RuhOzu += o.oz; p.Elmas += o.elmas; p.Parsomen += o.parsomen;
				string ad = Veri.Harita.TryGetValue(id, out var t) ? t.Ad : id;
				satir.Add($"{ad}: Ustalık {sv} — {Unvanlar[sv]}! (+{OdulYazi(sv)})");
			}
		}
		return satir;
	}

	/// <summary>Savaştan sonra takımdaki kartlara ustalık puanı. tur: Tekrar.TurBul değeri.</summary>
	public static List<string> SavasSonu(IEnumerable<TakimUyesi> takim, bool kazandi, string tur)
	{
		var l = new List<string>();
		if (tur is "dostluk" or "ikili") return l;
		bool enerjili = tur is "hikaye" or "arena";
		int xp = kazandi ? ZaferXp : enerjili ? YenilgiXp : 0;
		if (xp <= 0) return l;
		var idler = takim.Where(u => u != null && Kayit.Sahip(u.Id) != null).Select(u => u.Id).ToList();
		int gelisen = idler.Count(id => Kayit.Sahip(id).UstalikXp < Esik[MaxSeviye]);
		var seviye = XpEkle(idler, xp);
		if (gelisen > 0) l.Add($"+{xp} ustalık puanı ({gelisen} kahraman)");
		l.AddRange(seviye.Take(2));
		if (seviye.Count > 2) l.Add($"…ve {seviye.Count - 2} ustalık atlaması daha!");
		return l;
	}

	public static int UstaSayisi(Profil p, int seviye) => p.Kartlar.Count(k => Seviye(k.UstalikXp) >= seviye);
}
