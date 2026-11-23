"""Efsane Kartlar — kart verisi üreticisi.

Çalıştır:  python3 tasarim/kartlar.py
Çıktı:     assets/veri/kartlar.json  (oyun bu dosyayı okur)

Her kart: id, ad, mitoloji, nadirlik, sinif, element, temel statlar (seviye 1, 1 yıldız),
ulti (enerji 100 olunca otomatik kullanılır) ve kısa açıklama.
Yetenek etkileri (sırayla uygulanır):
  hasar      hedef, carpan [, can_cal]           (saldırı gücü x çarpan)
  iyilestir  hedef, carpan                       (saldırı gücü x çarpan)
  kalkan     hedef, oran, tur                    (kullananın max canı x oran)
  sersemlet  hedef, sans, tur                    (sıra kaçırır)
  yanma      hedef, oran, tur                    (her tur saldırı gücü x oran hasar)
  guclendir  hedef, oran, tur                    (saldırı +%)
  zayiflat   hedef, oran, tur                    (savunma -%)
  alay       hedef=kendi, tur                    (düşmanlar bu karta vurur)
  enerji     hedef, miktar
  kacinma    hedef, sans, tur                    (saldırıları sans ihtimalle boşa çıkarır; Japon mitolojisi)
Hedefler: tek, tum, arka, en_zayif, rastgele(adet), son (önceki hasarın hedefleri),
          kendi, tum_dost, en_zayif_dost
"""
import json, os, hashlib, sys
sys.path.insert(0, os.path.dirname(__file__))
from hikaye import HIKAYE, BAGLAR

MITOLOJI = {"turk": "Türk", "yunan": "Yunan", "iskandinav": "İskandinav", "misir": "Mısır", "japon": "Japon"}
NADIRLIK = {"siradan": 1.04, "nadir": 1.15, "epik": 1.30, "efsanevi": 1.40}
SINIF = {  # can, saldırı, savunma, hız
    "tank":    (1200, 62, 60, 92),
    "savasci": (820, 110, 34, 112),
    "buyucu":  (660, 120, 24, 100),
    "destek":  (840, 90, 38, 108),
}

def H(hedef, carpan, **k): return {"tip": "hasar", "hedef": hedef, "carpan": carpan, **k}
def I(hedef, carpan): return {"tip": "iyilestir", "hedef": hedef, "carpan": carpan}
def K(hedef, oran, tur=2): return {"tip": "kalkan", "hedef": hedef, "oran": oran, "tur": tur}
def S(hedef, sans, tur=1): return {"tip": "sersemlet", "hedef": hedef, "sans": sans, "tur": tur}
def Y(hedef, oran, tur=3): return {"tip": "yanma", "hedef": hedef, "oran": oran, "tur": tur}
def G(hedef, oran, tur=2): return {"tip": "guclendir", "hedef": hedef, "oran": oran, "tur": tur}
def Z(hedef, oran, tur=2): return {"tip": "zayiflat", "hedef": hedef, "oran": oran, "tur": tur}
def A(tur=2): return {"tip": "alay", "hedef": "kendi", "tur": tur}
def E(hedef, miktar): return {"tip": "enerji", "hedef": hedef, "miktar": miktar}
def KC(hedef, sans, tur=2): return {"tip": "kacinma", "hedef": hedef, "sans": sans, "tur": tur}   # Japon: saldırıdan kaçma

# id, ad, mitoloji, nadirlik, sınıf, element, ulti adı, etkiler, görsel tarifi (İngilizce, çizim için)
K_ = [
    # ---------------- TÜRK
    ("ulgen", "Ülgen", "turk", "efsanevi", "buyucu", "ates", "Gök Ateşi",
     [H("tum", 1.25), Y("tum", 0.2, 2), E("tum_dost", 10)],
     "Turkic sky god Ulgen, old man with long white beard, glowing golden sun crown, blue robes, fire in hands"),
    ("asena", "Asena", "turk", "epik", "savasci", "doga", "Bozkurt Pençesi",
     [H("en_zayif", 2.1, can_cal=0.2)],
     "Asena the grey she-wolf warrior woman, wolf pelt hood, twin curved swords, fierce yellow eyes"),
    ("umay", "Umay Ana", "turk", "epik", "destek", "su", "Kutsal Koruma",
     [I("tum_dost", 1.4), K("tum_dost", 0.10, 2)],
     "Umay mother goddess, kind woman with white veil and silver headdress, glowing blue light, holding a cradle charm"),
    ("tepegoz", "Tepegöz", "turk", "epik", "tank", "doga", "Dev Yumruğu",
     [A(2), H("tek", 1.5), S("son", 0.8, 1), K("kendi", 0.25, 2)],
     "Tepegoz one-eyed giant cyclops from Turkish legend, huge brown-skinned brute, single big eye, club, fur loincloth"),
    ("sahmeran", "Şahmeran", "turk", "nadir", "buyucu", "doga", "Yılan Zehri",
     [H("rastgele", 1.1, adet=3), Y("son", 0.35, 3)],
     "Sahmeran snake queen, beautiful woman with crown and serpent body, green and gold scales"),
    ("alp_er_tunga", "Alp Er Tunga", "turk", "nadir", "savasci", "ates", "Hakan Darbesi",
     [H("tek", 2.3), G("kendi", 0.25, 2)],
     "Alp Er Tunga Turkic khan warrior king, ornate golden helmet, red cape, sword raised"),
    ("dede_korkut", "Dede Korkut", "turk", "nadir", "destek", "su", "Kopuz Ezgisi",
     [I("en_zayif_dost", 2.4), E("tum_dost", 20)],
     "Dede Korkut old wise bard with long grey beard playing a kopuz lute, blue felt hat"),
    ("demir_alp", "Demir Alp", "turk", "nadir", "tank", "ates", "Demir Kalkan",
     [A(2), K("kendi", 0.35, 2)],
     "Demir Alp armored Turkic heavy cavalryman, full iron lamellar armor, big round shield"),
    ("akinci", "Akıncı", "turk", "siradan", "savasci", "ates", "Akın",
     [H("tek", 2.0)],
     "Ottoman akinci light cavalry raider, feathered cap, curved sword, red tunic"),
    ("bahadir", "Kalkanlı Bahadır", "turk", "siradan", "tank", "su", "Siper",
     [A(1), K("kendi", 0.25, 2)],
     "Turkic shield warrior, blue tunic, large wooden shield with wolf emblem, spear"),
    ("kam", "Kam", "turk", "siradan", "destek", "doga", "Davul Ayini",
     [I("tum_dost", 1.25), E("tum_dost", 15)],
     "Turkic shaman kam with feathered headdress and frame drum, colorful ribbons"),
    ("su_iyesi", "Su İyesi", "turk", "siradan", "buyucu", "su", "Girdap",
     [H("arka", 1.55)],
     "water spirit iyesi, translucent blue woman made of water with long flowing hair"),
    # ---------------- YUNAN
    ("zeus", "Zeus", "yunan", "efsanevi", "buyucu", "ates", "Yıldırım Fırtınası",
     [H("tum", 1.2), S("tum", 0.3, 1)],
     "Greek god Zeus, muscular old king with white beard, golden laurel crown, holding a lightning bolt"),
    ("akhilleus", "Akhilleus", "yunan", "epik", "savasci", "ates", "Truva Hücumu",
     [H("tek", 3.0), Z("son", 0.3, 2)],
     "Achilles Greek hero, bronze crested helmet, round shield, spear, red cape"),
    ("athena", "Athena", "yunan", "epik", "tank", "doga", "Aegis",
     [A(2), K("tum_dost", 0.12, 2), G("tum_dost", 0.15, 2)],
     "Athena goddess of war, golden crested helmet, owl emblem shield aegis, spear, white robes"),
    ("apollon", "Apollon", "yunan", "epik", "destek", "ates", "Güneş Ezgisi",
     [I("tum_dost", 1.25), G("tum_dost", 0.2, 2)],
     "Apollo sun god, young man with golden curly hair, laurel wreath, golden lyre, glowing sun"),
    ("medusa", "Medusa", "yunan", "nadir", "buyucu", "doga", "Taş Bakış",
     [H("tek", 2.2), S("son", 0.75, 1)],
     "Medusa gorgon with snakes for hair, glowing green eyes, scaly skin"),
    ("herakles", "Herakles", "yunan", "nadir", "savasci", "doga", "On İki Görev",
     [H("tek", 2.1), H("rastgele", 0.8, adet=2)],
     "Heracles strong hero wearing a lion pelt as hood, big wooden club, beard"),
    ("minotor", "Minotor", "yunan", "nadir", "tank", "ates", "Labirent Öfkesi",
     [A(2), H("tek", 1.2), Z("son", 0.25, 2)],
     "Minotaur bull-headed giant, big horns, double axe, dark red fur"),
    ("asklepios", "Asklepios", "yunan", "nadir", "destek", "su", "Şifa Asası",
     [I("en_zayif_dost", 2.3), I("tum_dost", 0.6)],
     "Asclepius god of medicine, bearded man in white toga holding a staff with a snake"),
    ("hoplit", "Hoplit", "yunan", "siradan", "tank", "su", "Falanks",
     [A(1), K("kendi", 0.22, 2)],
     "Greek hoplite soldier, bronze corinthian helmet, big round blue shield, spear"),
    ("amazon", "Amazon Okçu", "yunan", "siradan", "savasci", "doga", "Ok Yağmuru",
     [H("rastgele", 0.9, adet=3)],
     "Amazon warrior archer woman, braided hair, leather armor, longbow"),
    ("siren", "Siren", "yunan", "siradan", "buyucu", "su", "Büyülü Şarkı",
     [H("tek", 2.5), S("son", 0.45, 1)],
     "siren sea creature woman with fins and seaweed hair singing, ocean colors"),
    ("nimf", "Nimf", "yunan", "siradan", "destek", "doga", "Orman Nefesi",
     [I("en_zayif_dost", 2.4), K("en_zayif_dost", 0.1, 2)],
     "forest nymph dryad, green skin, leaves in hair, flowers, gentle smile"),
    # ---------------- İSKANDİNAV
    ("odin", "Odin", "iskandinav", "efsanevi", "buyucu", "su", "Ragnarök Rünleri",
     [H("tum", 1.15), Z("tum", 0.25, 2), G("tum_dost", 0.15, 2)],
     "Norse god Odin, one-eyed old king with eyepatch, long grey beard, winged helmet, spear Gungnir, raven"),
    ("thor", "Thor", "iskandinav", "epik", "savasci", "ates", "Mjölnir",
     [H("tek", 1.7), H("tum", 0.4), S("son", 0.2, 1)],
     "Norse god Thor, red beard, winged helmet, holding hammer Mjolnir with lightning"),
    ("freya", "Freya", "iskandinav", "epik", "destek", "doga", "Altın Gözyaşı",
     [I("tum_dost", 1.35), E("tum_dost", 30), G("tum_dost", 0.1, 2)],
     "Freya norse goddess, long blonde braided hair, golden necklace, falcon feather cloak"),
    ("ymir", "Buz Devi Ymir", "iskandinav", "epik", "tank", "su", "Buzul Duvarı",
     [A(2), K("kendi", 0.45, 2), S("tum", 0.2, 1)],
     "frost giant Ymir, huge pale blue giant with icicle beard and ice armor"),
    ("loki", "Loki", "iskandinav", "nadir", "buyucu", "ates", "Hile Ateşi",
     [H("rastgele", 1.2, adet=3), Y("son", 0.3, 2)],
     "Loki trickster god, sly grin, horned green and gold helmet, green flames in hand"),
    ("valkur", "Valkür", "iskandinav", "nadir", "savasci", "su", "Kanatlı Hüküm",
     [H("en_zayif", 2.3)],
     "Valkyrie warrior maiden, winged helmet, silver armor, spear, white wings"),
    ("heimdall", "Heimdall", "iskandinav", "nadir", "tank", "doga", "Gjallarhorn",
     [A(2), K("tum_dost", 0.08, 2), E("tum_dost", 15)],
     "Heimdall guardian god, golden armor, holding the horn Gjallarhorn, rainbow bridge glow"),
    ("idun", "Idun", "iskandinav", "nadir", "destek", "su", "Ölümsüzlük Elması",
     [I("tum_dost", 0.9), K("en_zayif_dost", 0.15, 2)],
     "Idun goddess with golden apples basket, flower crown, spring green dress"),
    ("kalkanci", "Viking Kalkancı", "iskandinav", "siradan", "tank", "ates", "Kalkan Duvarı",
     [A(1), K("kendi", 0.25, 2)],
     "viking shield warrior, horned-free iron helmet, red and white round shield, axe, braided beard"),
    ("berserker", "Berserker", "iskandinav", "siradan", "savasci", "ates", "Kudurma",
     [H("tek", 1.7), G("kendi", 0.3, 2)],
     "viking berserker wearing a bear pelt, wild eyes, two axes, war paint"),
    ("volva", "Völva", "iskandinav", "siradan", "buyucu", "doga", "Kehanet",
     [H("tek", 2.3), Z("son", 0.35, 2)],
     "norse seeress volva, hooded cloak, staff with runes, glowing eyes"),
    ("runa_ustasi", "Rün Ustası", "iskandinav", "siradan", "destek", "doga", "Koruma Rünü",
     [K("en_zayif_dost", 0.25, 2), I("en_zayif_dost", 1.3)],
     "viking rune master, bald tattooed head, carved rune stones glowing blue"),
    # ---------------- MISIR
    ("ra", "Ra", "misir", "efsanevi", "buyucu", "ates", "Güneş Diski",
     [H("tum", 1.1), Y("tum", 0.25, 3)],
     "Egyptian sun god Ra, falcon head with a glowing sun disk crown, gold collar, staff"),
    ("anubis", "Anubis", "misir", "epik", "savasci", "su", "Ruh Terazisi",
     [H("en_zayif", 2.1, can_cal=0.2), Z("son", 0.3, 2)],
     "Egyptian god Anubis, black jackal head, gold collar and armlets, was-scepter"),
    ("isis", "İsis", "misir", "epik", "destek", "su", "Hayat Nefesi",
     [I("tum_dost", 1.3), K("tum_dost", 0.08, 2)],
     "Egyptian goddess Isis, colorful outstretched wings, throne headdress, gold jewelry"),
    ("sfenks", "Sfenks", "misir", "epik", "tank", "doga", "Bilmece",
     [A(2), K("kendi", 0.3, 2), S("tum", 0.25, 1)],
     "Sphinx creature with lion body and pharaoh head with nemes headdress, sandstone"),
    ("thoth", "Thoth", "misir", "nadir", "buyucu", "su", "Bilgelik Yazısı",
     [H("arka", 1.4), E("tum_dost", 10)],
     "Egyptian god Thoth with ibis bird head, holding a scroll and reed pen, blue glow"),
    ("horus", "Horus", "misir", "nadir", "savasci", "ates", "Şahin Dalışı",
     [H("tek", 2.2), H("rastgele", 0.6, adet=1)],
     "Egyptian god Horus with falcon head, double crown, khopesh sword"),
    ("sobek", "Sobek", "misir", "nadir", "tank", "su", "Nil Çenesi",
     [A(2), H("tek", 1.1, can_cal=0.5)],
     "Egyptian god Sobek with crocodile head, green scales, gold armor, spear"),
    ("bastet", "Bastet", "misir", "nadir", "destek", "doga", "Kedi Lütfu",
     [I("tum_dost", 0.8), G("tum_dost", 0.2, 2)],
     "Egyptian goddess Bastet with black cat head, gold earrings, sistrum instrument"),
    ("mumya", "Mumya", "misir", "siradan", "tank", "doga", "Lanetli Sargı",
     [A(1), K("kendi", 0.2, 2), Z("tum", 0.1, 1)],
     "mummy wrapped in old bandages with glowing green eyes"),
    ("akrep", "Akrep Savaşçısı", "misir", "siradan", "savasci", "doga", "Zehirli İğne",
     [H("tek", 1.6), Y("son", 0.25, 2)],
     "scorpion warrior, man with scorpion tail and pincers, bronze armor"),
    ("kahin", "Nil Kâhini", "misir", "siradan", "buyucu", "ates", "Kum Fırtınası",
     [H("tum", 1.2)],
     "Egyptian priest mage with shaved head, gold collar, glowing hieroglyphs, staff"),
    ("ankh_rahibesi", "Ankh Rahibesi", "misir", "siradan", "destek", "ates", "Ankh Işığı",
     [I("tum_dost", 1.05), G("tum_dost", 0.1, 2)],
     "Egyptian priestess holding a glowing ankh, black bob hair, white linen dress"),
    # ---------------- EK KARTLAR (2026-11-19)
    ("koroglu", "Köroğlu", "turk", "epik", "savasci", "ates", "Kır At Hücumu",
     [H("tek", 2.2), G("tum_dost", 0.15, 2)],
     "Koroglu Turkish folk hero bard-warrior, thick black mustache, fur-trimmed red kaftan, saz lute and curved sword"),
    ("perseus", "Perseus", "yunan", "epik", "tank", "su", "Ayna Kalkan",
     [A(2), K("kendi", 0.3, 2), S("tek", 0.6, 1)],
     "Perseus young Greek hero, bronze winged helmet, polished mirror shield, short sword"),
    ("fenrir", "Fenrir", "iskandinav", "epik", "savasci", "doga", "Zincir Kıran",
     [H("en_zayif", 2.0, can_cal=0.25), G("kendi", 0.2, 2)],
     "Fenrir giant grey wolf, glowing yellow eyes, broken iron chain, snarling fangs"),
    ("sekhmet", "Sekhmet", "misir", "epik", "buyucu", "ates", "Kızıl Öfke",
     [H("tum", 0.85), Y("tum", 0.2, 2)],
     "Sekhmet lioness-headed Egyptian war goddess, golden sun disc crown, red linen dress"),
    # ---------------- YENİ KARTLAR (2026-11-23, görseller Sprite Atölye)
    ("erlik", "Erlik Han", "turk", "efsanevi", "buyucu", "su", "Yeraltı Hükmü",
     [H("tum", 1.1), Z("tum", 0.2, 2), S("tum", 0.15, 1)],
     "Erlik Khan Turkic god of the underworld, old king with black beard, horned black iron crown, dark robes, staff with blue flames"),
    ("al_karisi", "Al Karısı", "turk", "nadir", "buyucu", "ates", "Kâbus Ateşi",
     [H("rastgele", 1.0, adet=3), Y("son", 0.3, 2)],
     "Al Karisi Turkic nightmare demon woman, wild red hair, pale skin, glowing red eyes, ragged red dress, fire in hand"),
    ("battal_gazi", "Battal Gazi", "turk", "epik", "savasci", "doga", "Gazi Kılıcı",
     [H("tek", 2.4), G("kendi", 0.2, 2)],
     "Battal Gazi legendary Turkish warrior, turban helmet with plume, chainmail, green cape, curved sword"),
    ("hades", "Hades", "yunan", "efsanevi", "tank", "su", "Ölüler Diyarı",
     [A(2), K("kendi", 0.4, 2), Z("tum", 0.2, 2)],
     "Hades Greek god of the underworld, pale grim king with black beard, dark armor, black crown, bident, green ghost flames"),
    ("artemis", "Artemis", "yunan", "epik", "savasci", "doga", "Ay Okları",
     [H("rastgele", 1.0, adet=3), Z("son", 0.2, 2)],
     "Artemis goddess of the hunt, silver hair, crescent moon tiara, white tunic, silver bow"),
    ("kerberos", "Kerberos", "yunan", "nadir", "tank", "ates", "Üç Başlı Öfke",
     [A(2), H("tek", 1.0), Y("son", 0.25, 2), K("kendi", 0.25, 2)],
     "Cerberus three-headed black hellhound, fiery eyes, spiked collar, flames"),
    ("hel", "Hel", "iskandinav", "epik", "destek", "su", "Ölüm Sisi",
     [I("tum_dost", 1.35), Z("tum", 0.25, 2), E("tum_dost", 10)],
     "Hel Norse goddess of the dead, face half pale half dark blue, long black hair, dark hooded cloak, bone crown"),
    ("tyr", "Tyr", "iskandinav", "nadir", "savasci", "su", "Kurban Eli",
     [H("tek", 2.2), Z("son", 0.25, 2)],
     "Tyr Norse god of war, one-handed bearded warrior, iron helmet, chainmail, sword, round shield"),
    ("set", "Set", "misir", "efsanevi", "savasci", "ates", "Kaos Fırtınası",
     [H("en_zayif", 2.4, can_cal=0.15), S("son", 0.4, 1)],
     "Set Egyptian god of chaos, red-skinned man with a desert animal head with tall square ears, dark armor, golden was-scepter"),
    ("hathor", "Hathor", "misir", "nadir", "destek", "doga", "Sistrum Ezgisi",
     [I("tum_dost", 1.15), E("tum_dost", 20)],
     "Hathor Egyptian goddess of love and music, cow horns with golden sun disc, red dress, gold collar, sistrum"),
    # ---------------- JAPON (2026-11-23; mekanik: kaçınma — kitsune yanılsaması)
    ("amaterasu", "Amaterasu", "japon", "efsanevi", "buyucu", "ates", "Güneş Aynası",
     [H("tum", 1.1), KC("tum_dost", 0.2, 2)],
     "Amaterasu Japanese sun goddess, serene woman with long black hair, golden sun disc halo behind her head, white and red layered kimono robes, holding the sacred bronze mirror Yata no Kagami"),
    ("susanoo", "Susanoo", "japon", "epik", "savasci", "su", "Fırtına Kılıcı",
     [H("tek", 1.8), H("tum", 0.35)],
     "Susanoo Japanese storm god, wild long black hair and beard, dark blue armored kimono with storm clouds, holding the long straight sword Kusanagi, lightning and rain"),
    ("raijin", "Raijin", "japon", "epik", "buyucu", "ates", "Gök Gürültüsü Davulu",
     [H("rastgele", 1.0, adet=4), S("son", 0.25, 1)],
     "Raijin Japanese thunder god, red-skinned muscular oni-like god with wild hair and horns, ring of taiko drums behind his back, drumsticks, lightning bolts"),
    ("ryujin", "Ryujin", "japon", "epik", "tank", "su", "Gelgit Kalesi",
     [A(2), K("tum_dost", 0.1, 2), KC("kendi", 0.3, 2)],
     "Ryujin Japanese dragon king of the sea, long blue-green eastern dragon with golden horns and whiskers, holding a glowing tide jewel pearl, ocean waves"),
    ("kitsune", "Kitsune", "japon", "nadir", "destek", "ates", "Dokuz Kuyruk",
     [KC("tum_dost", 0.3, 2), I("en_zayif_dost", 1.2)],
     "Kitsune Japanese fox spirit woman, white fox ears, nine fluffy white fox tails, red and white shrine kimono, blue foxfire flames floating around her"),
    ("oni", "Oni", "japon", "nadir", "tank", "ates", "Demir Gürz",
     [A(2), H("tek", 1.2), S("son", 0.4, 1)],
     "Oni Japanese ogre demon, big red-skinned brute with two horns, tiger skin loincloth, huge spiked iron club kanabo, fangs"),
    ("tengu", "Tengu", "japon", "nadir", "savasci", "doga", "Kanat Fırtınası",
     [H("arka", 1.3), KC("kendi", 0.3, 2)],
     "Tengu Japanese mountain spirit warrior, red face with a very long nose, black feathered wings, yamabushi monk clothes with small black hat tokin, feather fan and katana"),
    ("yuki_onna", "Yuki-onna", "japon", "nadir", "buyucu", "su", "Kar Fırtınası",
     [H("tum", 0.8), Z("tum", 0.2, 2)],
     "Yuki-onna Japanese snow woman spirit, very pale skin, long straight black hair, pure white kimono, icy blue breath, snowflakes swirling"),
    ("samuray", "Samuray", "japon", "siradan", "savasci", "doga", "Iaijutsu",
     [H("tek", 2.1)],
     "Japanese samurai warrior in red lacquered o-yoroi armor, horned kabuto helmet with menpo face mask, drawing a katana"),
    ("ninja", "Ninja", "japon", "siradan", "savasci", "doga", "Gölge Hançeri",
     [H("en_zayif", 1.7), KC("kendi", 0.35, 2)],
     "Japanese ninja shinobi in dark navy shinobi shozoku outfit and face mask, short ninjato sword on the back, shuriken in hand"),
    ("miko", "Miko", "japon", "siradan", "destek", "su", "Arınma Ayini",
     [I("tum_dost", 1.1), KC("en_zayif_dost", 0.3, 2)],
     "Japanese shrine maiden miko, long black hair tied with white ribbon, white kimono top and red hakama trousers, holding a gohei purification wand with paper streamers"),
    ("kappa", "Kappa", "japon", "siradan", "tank", "doga", "Su Kabuğu",
     [A(1), K("kendi", 0.25, 2)],
     "Kappa Japanese river creature, green turtle-like humanoid with a turtle shell on its back, beak, water-filled dish on top of its head, webbed hands"),
]

def varyans(id_, i):
    h = hashlib.md5(f"{id_}-{i}".encode()).digest()[0]
    return 0.93 + (h / 255) * 0.14   # 0.93–1.07

def uret():
    kartlar = []
    for (id_, ad, mit, nad, sin, el, ulti, etk, tarif) in K_:
        can, sal, sav, hiz = SINIF[sin]
        m = NADIRLIK[nad]
        kartlar.append({
            "id": id_, "ad": ad, "mitoloji": mit, "nadirlik": nad, "sinif": sin, "element": el,
            "can": round(can * m * varyans(id_, 0)),
            "saldiri": round(sal * m * varyans(id_, 1)),
            "savunma": round(sav * m * varyans(id_, 2)),
            "hiz": round(hiz * (1 + (m - 1) * 0.25) * varyans(id_, 3)),
            "ulti": {"ad": ulti, "etkiler": etk},
            "unvan": HIKAYE[id_][0], "biyo": HIKAYE[id_][1],
            "tarif": tarif,
        })
    return kartlar

if __name__ == "__main__":
    k = uret()
    yol = os.path.join(os.path.dirname(__file__), "..", "assets", "veri", "kartlar.json")
    os.makedirs(os.path.dirname(yol), exist_ok=True)
    ids = {x["id"] for x in k}
    for b in BAGLAR: assert all(c in ids for c in b[2]), b
    baglar = [{"id": a, "ad": ad, "kartlar": ks, "stat": st, "oran": o, "aciklama": ac} for (a, ad, ks, st, o, ac) in BAGLAR]
    with open(yol, "w", encoding="utf-8") as f:
        json.dump({"mitolojiler": MITOLOJI, "kartlar": k, "baglar": baglar}, f, ensure_ascii=False, indent=1)
    from collections import Counter
    print(len(k), "kart")
    print(Counter(x["nadirlik"] for x in k), Counter(x["sinif"] for x in k), Counter(x["element"] for x in k))
    assert len({x["id"] for x in k}) == len(k)
