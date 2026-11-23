"""Kart hikâyeleri (unvan + kısa biyografi) ve takım bağları (simya).

kartlar.py bu dosyayı içe aktarır; JSON'a "unvan", "biyo" ve kökte "baglar" olarak yazılır.
Bağ: listedeki kartların HEPSİ takımdaysa yalnızca o kartlar bonusu alır.
stat: can | saldiri | savunma | hiz
"""

HIKAYE = {
    # ---------------- TÜRK
    "ulgen": ("Gök Tanrısı",
        "Altay ve Yakut inanışında göğün en üst katında oturan yaratıcı iyilik tanrısı. Yeri, göğü ve insanı yarattığına inanılır; "
        "kamlar ona ancak göğün katlarını tek tek aşarak ulaşabilir. Kardeşi Erlik ile ezelî bir çekişme içindedir."),
    "asena": ("Bozkurt Ana",
        "Göktürk destanında, düşman baskınından sağ kurtulan tek çocuğu besleyip büyüten dişi bozkurt. "
        "Bir mağarada doğurduğu on oğuldan Türk boyları türemiştir. Türklerin atası ve koruyucusu sayılır."),
    "umay": ("Ana Tanrıça",
        "Kadınların, çocukların ve doğumun koruyucusu olan bereket tanrıçası. Bebeklerin beşiğini gözetir, "
        "yeni doğanlara can verir. Orhun Yazıtları'nda bile adı geçen en eski Türk tanrıçalarındandır."),
    "tepegoz": ("Tek Gözlü Dev",
        "Dede Korkut Kitabı'nda Oğuz illerine dehşet salan, alnında tek gözü olan dev. Kılıç işlemeyen bedeniyle "
        "her gün insan ve koyun yerdi. Sonunda yiğit Basat gözüne kızgın şiş saplayarak onu yendi."),
    "sahmeran": ("Yılanların Şahı",
        "Belden yukarısı kadın, aşağısı yılan olan bilge yılanlar kraliçesi. Yeraltında bütün şifa sırlarını bilir. "
        "Ona ihanet edildiğinde bile bilgisini insanlığa bırakmıştır; Anadolu'da hâlâ resmi evlere asılır."),
    "alp_er_tunga": ("Turan Hükümdarı",
        "İran kaynaklarında Afrasiyab diye anılan efsanevi Saka-Turan hükümdarı. Yıllarca İran'a karşı savaşan bu cesur "
        "kağanın ölümüne bütün Türk illeri ağıt yakmıştır; o ağıtlar Türkçenin en eski şiirlerindendir."),
    "dede_korkut": ("Bilge Ozan",
        "Oğuzların bilge ozanı ve danışmanı. Kopuzuyla boyları dolaşır, yiğitlere ad koyar, beylere öğüt verir. "
        "Anlattığı on iki hikâye Dede Korkut Kitabı'nda toplanmıştır."),
    "demir_alp": ("Demirci Yiğit",
        "Ergenekon'dan çıkışta demir dağı eriten demircilerin soyundan gelen yiğit. Örsünde dövdüğü zırhı hiçbir kılıç "
        "delemez; savaşta takımının önünde demirden bir duvar gibi durur."),
    "akinci": ("Sınır Akıncısı",
        "Sınır boylarında hızlı atlarıyla düşman topraklarına baskın yapan cesur süvari. Ok atar, kılıç sallar ve "
        "geldiği gibi rüzgâr hızıyla kaybolur."),
    "bahadir": ("Kalkan Taşıyıcı",
        "Ordunun ön safında büyük kalkanıyla duran sadık asker. Kendi canı pahasına arkadaşlarını korumayı şeref sayar."),
    "kam": ("Ruh Yolcusu",
        "Davulunun ritmiyle ruhlar dünyasına yolculuk eden Türk şamanı. Hastaları iyileştirir, kötü ruhları kovar, "
        "göğün ve yerin tanrılarıyla konuşur."),
    "su_iyesi": ("Suların Sahibi",
        "Türk inanışında her gölün, ırmağın ve pınarın bir iyesi (koruyucu ruhu) vardır. Suya saygı gösterene bereket, "
        "kirletene felaket getirir."),
    "koroglu": ("Çamlıbel Yiğidi",
        "Babasının gözlerine mil çektiren zalim beyden öcünü almak için dağa çıkan halk kahramanı. Kır atı ve "
        "sazıyla Çamlıbel'de zalimlere karşı direnir, yoksulun yanında durur."),
    "erlik": ("Yeraltı Hanı",
        "Altay inanışında yeraltı dünyasının hükümdarı ve ölülerin yargıcı. Ülgen'in kardeşiyken ondan üstün olmak "
        "isteyince yeraltına sürüldü. Kötü ruhlar ve hastalıklar onun emrindedir."),
    "al_karisi": ("Lohusa Cini",
        "Anadolu ve Orta Asya inanışında lohusa kadınlara ve yeni doğanlara musallat olan kötü ruh. "
        "Ateş gibi kızıl saçlıdır; demirden ve kırmızı kurdeleden korktuğuna inanılır."),
    "battal_gazi": ("Malatya'nın Gazisi",
        "Emevî-Bizans savaşlarında yaşamış, adı destanlara karışmış Malatyalı kahraman. Battalname'de dev ve "
        "cadılarla dövüşen, kılık değiştirip düşman kalelerine sızan yenilmez bir gazi olarak anlatılır."),
    # ---------------- YUNAN
    "zeus": ("Tanrıların Kralı",
        "Babası Kronos'u ve Titanları yenerek Olimpos'un hâkimi olan gök ve şimşek tanrısı. Kardeşleriyle dünyayı "
        "paylaşırken gökler ona düştü. Fırlattığı şimşekleri Kükloplar dövmüştür."),
    "akhilleus": ("Truva'nın Kahramanı",
        "Annesi Thetis onu Styx ırmağına batırdığı için yalnızca topuğundan yaralanabilen yarı tanrı savaşçı. "
        "Truva Savaşı'nın en büyük kahramanıydı; Paris'in oku topuğuna isabet edince öldü."),
    "athena": ("Bilgelik Tanrıçası",
        "Zeus'un başından zırhıyla doğan bilgelik ve savaş stratejisi tanrıçası. Atina şehrinin koruyucusudur. "
        "Kahramanlara yol gösterir; Perseus'a Medusa'yı yenmesi için parlak kalkanını vermiştir."),
    "apollon": ("Güneş ve Müzik Tanrısı",
        "Işığın, müziğin, şiirin ve kehanetin tanrısı. Delfi kâhinliği onun tapınağıdır. Gümüş yayıyla hastalık da "
        "gönderebilir, şifa da. İkiz kız kardeşi Artemis'tir."),
    "medusa": ("Gorgon",
        "Saçları yılan olan, bakışı insanı taşa çeviren Gorgon. Bir zamanlar güzel bir rahibeydi; Athena'nın lanetiyle "
        "canavara dönüştü. Perseus kalkandaki yansımasına bakarak onun başını kesti."),
    "herakles": ("Yarı Tanrı Kahraman",
        "Zeus'un oğlu, dünyanın en güçlü ölümlüsü. Hera'nın laneti yüzünden on iki imkânsız görevi yerine getirdi: "
        "Nemea aslanını boğdu, Hidra'yı yendi, Kerberos'u yeraltından çıkardı."),
    "minotor": ("Labirentin Canavarı",
        "Girit kralı Minos'un sarayının altındaki labirentte yaşayan boğa başlı dev. Her yıl Atina'dan gelen gençlerle "
        "beslenirdi; sonunda Theseus onu labirentin derinliklerinde yendi."),
    "asklepios": ("Şifa Tanrısı",
        "Apollon'un oğlu, hekimliğin babası. Ölüleri bile diriltecek kadar ustalaşınca Zeus onu şimşekle cezalandırdı. "
        "Yılana sarılı asası bugün hâlâ tıbbın simgesidir."),
    "hoplit": ("Falanks Askeri",
        "Yunan şehir devletlerinin ağır zırhlı piyadesi. Yuvarlak kalkanı ve uzun mızrağıyla sıkı falanks düzeninde "
        "omuz omuza savaşır."),
    "amazon": ("Savaşçı Kadınlar",
        "Karadeniz kıyılarında yaşadığı anlatılan efsanevi kadın savaşçılar ulusu. At üstünden ok atmada eşsizdirler; "
        "Truva'da bile kraliçeleri Penthesileia ile savaşmışlardır."),
    "siren": ("Deniz Şarkıcısı",
        "Kayalık adalarda oturup şarkılarıyla denizcileri ölüme çeken yaratıklar. Odysseus onları duyabilmek için kendini "
        "geminin direğine bağlatmıştı."),
    "nimf": ("Doğa Perisi",
        "Ormanların, pınarların ve dağların genç ve ölümsüz perileri. Doğanın canlılığını korur, yaralı yolculara şifa "
        "verirler."),
    "perseus": ("Gorgon Avcısı",
        "Zeus'un ölümlü oğlu. Tanrıların hediyeleriyle — Athena'nın kalkanı, Hermes'in kanatlı sandaletleri — Medusa'nın "
        "başını kesti ve deniz canavarından prenses Andromeda'yı kurtardı."),
    "hades": ("Yeraltının Efendisi",
        "Zeus ve Poseidon'un ağabeyi; dünya paylaşılırken ona ölüler diyarı düştü. Adil ama acımasızdır: yeraltına "
        "giren kimsenin geri dönmesine izin vermez. Bekçisi üç başlı Kerberos'tur."),
    "artemis": ("Av Tanrıçası",
        "Apollon'un ikizi; avın, vahşi doğanın ve ayın tanrıçası. Gümüş yayından çıkan ok hedefini asla şaşırmaz. "
        "Genç kızların ve hayvanların koruyucusudur."),
    "kerberos": ("Yeraltı Bekçisi",
        "Hades'in kapısını bekleyen üç başlı, yılan kuyruklu dev köpek. Ölüleri içeri alır ama kimseyi dışarı bırakmaz. "
        "Herakles on ikinci görevinde onu çıplak elleriyle yakaladı."),
    # ---------------- İSKANDİNAV
    "odin": ("Her Şeyin Babası",
        "Asgard'ın kralı, bilgelik ve savaş tanrısı. Mimir'in kuyusundan içmek için bir gözünü feda etti, rünlerin "
        "sırrını öğrenmek için Dünya Ağacı'na dokuz gece asılı kaldı. Kuzgunları Hugin ve Munin ona her şeyi anlatır."),
    "thor": ("Gök Gürültüsü Tanrısı",
        "Odin'in oğlu, insanların ve tanrıların koruyucusu. Çekici Mjölnir'i fırlattığında gök gürler ve şimşek çakar. "
        "Devlerin baş düşmanıdır; Ragnarök'te Dünya Yılanı ile savaşacaktır."),
    "freya": ("Aşk ve Savaş Tanrıçası",
        "Vanir soyundan gelen aşk, bereket ve büyü tanrıçası. Savaşta ölenlerin yarısını kendi sarayına alır. "
        "Kedilerin çektiği arabasıyla dolaşır, şahin tüyünden pelerini ona uçma gücü verir."),
    "ymir": ("İlk Dev",
        "Dünyanın başında buzla ateşin buluştuğu boşluktan doğan ilk buz devi. Odin ve kardeşleri onu öldürüp bedeninden "
        "dünyayı yarattı: etinden toprak, kanından denizler, kafatasından gök kubbe oldu."),
    "loki": ("Hilebaz Tanrı",
        "Kılık değiştirmekte usta, zekâsı kadar kötülüğü de olan ateş tanrısı. Tanrıların başını belaya sokar, sonra yine "
        "hilesiyle kurtarır. Fenrir, Dünya Yılanı ve Hel onun çocuklarıdır."),
    "valkur": ("Savaşçı Bakire",
        "Odin'in kanatlı miğferli savaşçı kızları. Savaş alanında ölen en cesur savaşçıları seçip Valhalla'ya taşırlar."),
    "heimdall": ("Gökkuşağı Bekçisi",
        "Asgard'a giden Bifröst köprüsünün bekçisi. Çimenlerin büyüdüğünü duyacak kadar keskin kulaklıdır, hiç uyumaz. "
        "Ragnarök geldiğinde Gjallarhorn borusunu öttürecektir."),
    "idun": ("Gençlik Tanrıçası",
        "Tanrıları genç tutan altın elmaların bekçisi. Bir kez devlerce kaçırıldığında bütün tanrılar yaşlanmaya başlamıştı."),
    "kalkanci": ("Kalkan Duvarı",
        "Ahşap yuvarlak kalkanlarını birbirine kenetleyip kalkan duvarı kuran Viking savaşçısı. Duvar yıkılmadıkça "
        "geri adım atmaz."),
    "berserker": ("Öfkeli Savaşçı",
        "Ayı postuna bürünüp savaş öfkesine kapılan Viking savaşçısı. Öfke anında acı hissetmez, kalkanını ısırır ve "
        "zırhsız düşmana atılır."),
    "volva": ("Kâhin Kadın",
        "Asasıyla köy köy dolaşıp geleceği gören İskandinav büyücüsü. Odin bile Ragnarök'ün kehanetini bir völvadan "
        "dinlemiştir."),
    "runa_ustasi": ("Rün Oymacısı",
        "Taşlara ve silahlara güç veren rünleri kazıyan bilge. Doğru rün kalkanı sağlamlaştırır, yanlış rün ise lanet getirir."),
    "fenrir": ("Dev Kurt",
        "Loki'nin oğlu, büyüdükçe büyüyen korkunç kurt. Tanrılar onu büyülü Gleipnir zinciriyle bağladı; bu sırada "
        "Tyr elini kurdun ağzında kaybetti. Ragnarök'te zincirlerini kırıp Odin'i yutacaktır."),
    "hel": ("Ölüler Kraliçesi",
        "Loki'nin kızı; bedeninin yarısı canlı, yarısı ölü. Odin onu savaşta ölmeyenlerin gittiği soğuk Helheim'a "
        "kraliçe yaptı. Güzel tanrı Baldur'u bile geri vermemiştir."),
    "tyr": ("Savaş ve Adalet Tanrısı",
        "Tanrıların en cesuru. Fenrir bağlanırken güven teminatı olarak elini kurdun ağzına koydu ve onu feda etti. "
        "Yeminlerin ve adaletin koruyucusudur."),
    # ---------------- MISIR
    "ra": ("Güneş Tanrısı",
        "Mısır'ın yaratıcı güneş tanrısı. Gündüz güneş kayığıyla göğü geçer, gece yeraltında kaos yılanı Apep ile savaşır. "
        "Firavunlar kendilerini Ra'nın oğlu sayardı."),
    "anubis": ("Ölülerin Rehberi",
        "Çakal başlı mumyalama ve mezarlık tanrısı. Ölülerin kalbini Maat'ın tüyüyle tartar; kalbi temiz olanı ahirete "
        "geçirir, ağır olanı canavar Ammit'e verir."),
    "isis": ("Büyü Tanrıçası",
        "Mısır'ın en güçlü büyücü tanrıçası ve anaların koruyucusu. Kocası Osiris'i Set öldürüp parçalayınca parçaları "
        "toplayıp onu yeniden diriltti. Oğlu Horus'tur."),
    "sfenks": ("Bilmececi Bekçi",
        "Aslan gövdeli, insan başlı dev bekçi. Gize'deki büyük heykeli firavunların mezarlarını korur. "
        "Söylencelere göre bilmecesini bilemeyen yolcuyu yutar."),
    "thoth": ("Bilgi Tanrısı",
        "İbis başlı yazı, bilgi ve ay tanrısı. Hiyeroglifleri icat ettiğine, ölülerin yargısında kaydı tuttuğuna inanılır."),
    "horus": ("Gök Şahini",
        "Şahin başlı gök tanrısı, Osiris ile İsis'in oğlu. Babasının öcünü almak için amcası Set ile savaştı ve bir gözünü "
        "kaybetti. Horus'un Gözü koruma ve şifa simgesidir."),
    "sobek": ("Nil'in Timsahı",
        "Timsah başlı Nil ve güç tanrısı. Nil'in taşkınlarını ve bereketini yönetir; ordunun ve firavunun koruyucusudur."),
    "bastet": ("Kedi Tanrıça",
        "Kedi başlı ev, bereket ve koruma tanrıçası. Evleri kötü ruhlardan ve hastalıklardan korur. "
        "Mısırlılar kedileri onun kutsal hayvanı olarak çok severdi."),
    "mumya": ("Uyanan Ölü",
        "Ahiret için sargılara sarılarak korunmuş eski bir firavun muhafızı. Mezarına giren hırsızlara karşı uyanır."),
    "akrep": ("Akrep Muhafızı",
        "Akrep tanrıçası Serket'e bağlı çöl savaşçısı. Kıskaçları kadar zehirli mızrağı da korkulur."),
    "kahin": ("Nil Kâhini",
        "Tapınakta kutsal ateşi besleyen ve yıldızlardan Nil'in taşkınını okuyan rahip."),
    "ankh_rahibesi": ("Hayat Rahibesi",
        "Elindeki ankh, yani hayatın anahtarıyla yaralılara can veren tapınak rahibesi."),
    "sekhmet": ("Aslan Tanrıça",
        "Ra'nın gözünden doğan aslan başlı savaş ve salgın tanrıçası. İnsanlığı yok etmek üzereyken tanrılar onu kırmızıya "
        "boyanmış birayla sarhoş ederek durdurdu."),
    "set": ("Kaos ve Çöl Tanrısı",
        "Fırtınanın, çölün ve kaosun tanrısı. Tahta geçmek için kardeşi Osiris'i öldürdü, Horus ile yıllarca savaştı. "
        "Yine de her gece Ra'nın kayığında Apep yılanıyla savaşan da odur."),
    "hathor": ("Sevgi ve Müzik Tanrıçası",
        "İnek boynuzlu, güneş taçlı sevgi, müzik ve sevinç tanrıçası. Ra'nın kızıdır; öfkelendiğinde Sekhmet'e dönüştüğü "
        "anlatılır."),
    # ---------------- JAPON
    "amaterasu": ("Güneş Tanrıçası",
        "Şinto'nun en yüce tanrıçası, güneşin ve evrenin ışığı. Kardeşi Susanoo'nun taşkınlıklarına kızıp bir mağaraya "
        "saklandığında dünya karanlığa gömüldü; tanrılar onu bir aynayla dışarı çıkardı. Japon imparatorlarının atası sayılır."),
    "susanoo": ("Fırtına Tanrısı",
        "Amaterasu'nun asi kardeşi, denizlerin ve fırtınaların tanrısı. Göklerden sürüldükten sonra sekiz başlı ejderha "
        "Yamata no Orochi'yi öldürdü ve kuyruğundan efsanevi Kusanagi kılıcını çıkardı."),
    "raijin": ("Şimşek Tanrısı",
        "Sırtındaki davulları çalarak gök gürültüsü çıkaran şimşek tanrısı. Rüzgâr tanrısı Fujin ile birlikte "
        "fırtınaları getirir; çocuklara göbeklerini saklamalarını, yoksa Raijin'in yiyeceğini söylerler."),
    "ryujin": ("Deniz Ejderha Kralı",
        "Denizin dibindeki mercan sarayda yaşayan ejderha kral. Gelgitleri yöneten mücevherlere sahiptir; "
        "denizcilerin hem korkusu hem koruyucusudur."),
    "kitsune": ("Tilki Ruhu",
        "Yaşı ilerledikçe yeni kuyruklar çıkaran, en fazla dokuz kuyruğa ulaşan bilge tilki. İnsan kılığına girer, "
        "göz yanılsamaları yaratır. Pirinç tanrısı Inari'nin habercisidir."),
    "oni": ("İblis",
        "Kırmızı ya da mavi tenli, boynuzlu, demir gürzlü Japon iblisi. Dağlarda yaşar, köylere baskın yapar. "
        "Setsubun bayramında fasulye atılarak kovulur."),
    "tengu": ("Dağ Ruhu",
        "Uzun burunlu, kanatlı dağ ruhu. Kılıç ustalığında eşsizdir; efsaneye göre ünlü savaşçı Minamoto no Yoshitsune'ye "
        "kılıç kullanmayı tengular öğretmiştir."),
    "yuki_onna": ("Kar Kadını",
        "Karlı dağlarda beyaz kimonosuyla beliren solgun hayalet kadın. Nefesiyle yolcuları dondurur; ama bazı "
        "hikâyelerde merhamet gösterdiği de olur."),
    "samuray": ("Kılıç Ustası",
        "Buşido yolunu izleyen Japon savaşçısı. Sadakati, onuru ve katanasıyla tanınır; ustası için canını vermekten çekinmez."),
    "ninja": ("Gölge Savaşçısı",
        "Casusluk, gizlenme ve suikast sanatında eğitilmiş shinobi. Gece karanlığında iz bırakmadan gelir ve gider."),
    "miko": ("Tapınak Rahibesi",
        "Şinto tapınaklarında hizmet eden rahibe. Kutsal danslarıyla ve gohei asasıyla kötü ruhları arındırır."),
    "kappa": ("Nehir Yaratığı",
        "Nehirlerde yaşayan kaplumbağa kabuklu yaramaz su yaratığı. Gücü başındaki su çanağından gelir; eğilip selam "
        "verirse su dökülür ve gücünü kaybeder. Salatalığa bayılır."),
    # ---------------- ÇİN
    "sun_wukong": ("Maymun Kral",
        "Bir taştan doğan, ölümsüzlüğü çalıp göklerde kıyamet koparan Maymun Kral. Yetmiş iki dönüşüm bilir, tek takla ile "
        "binlerce kilometre aşar; altın çubuğunu iğne kadar küçültüp dağ kadar büyütebilir. Batı'ya Yolculuk'ta keşiş Tang'ı korudu."),
    "yesim_imparator": ("Göklerin Hükümdarı",
        "Çin halk inancında cennetin imparatoru; tanrıları, ruhları ve insanları bir saray düzeni gibi yönetir. "
        "Yıl sonunda her evin mutfak tanrısı ona ailenin davranışlarını rapor eder."),
    "guan_yu": ("Savaş Tanrısı",
        "Üç Krallık döneminin efsanevi generali; kızıl yüzü, uzun sakalı ve Yeşil Ejder palasıyla tanınır. Sadakati ve "
        "onuru yüzünden öldükten sonra savaş ve kardeşlik tanrısı olarak tapınaklara konmuştur."),
    "long": ("Yağmur Ejderhası",
        "Çin ejderhaları korkunç değil, bilge ve uğurlu varlıklardır. Yağmuru, ırmakları ve denizleri yönetir; "
        "pençesindeki inci bilgeliğin ve gücün simgesidir. İmparatorlar ejderhanın oğlu sayılırdı."),
    "nezha": ("Ateş Tekerlekli Çocuk",
        "Ejderha Kral'ın oğlunu yendiği için ailesini kurtarmak adına kendini feda eden çocuk tanrı. Bir ustanın büyüsüyle "
        "nilüferlerden yeniden yaratıldı; ateş tekerlekleri ve kızıl kuşağıyla iblislerle savaşır."),
    "nuwa": ("Yaratıcı Ana",
        "Belden aşağısı yılan olan yaratıcı tanrıça. İnsanları sarı kilden şekillendirdi; gök kubbe çatlayınca beş renkli "
        "taşları eriterek göğü onardı ve dünyayı tufandan kurtardı."),
    "change": ("Ay Tanrıçası",
        "Kocası okçu Hou Yi'nin ölümsüzlük iksirini içince aya yükselen tanrıça. Orada yeşim tavşanıyla yalnız yaşar; "
        "Ay Festivali'nde insanlar ona ay çörekleri sunar."),
    "zhong_kui": ("İblis Avcısı",
        "Sınavda birinci olduğu hâlde çirkin yüzü yüzünden unvanı elinden alınan bilgin. Öldükten sonra iblislerin kralı "
        "oldu; resmi kötü ruhları kovmak için kapılara asılır."),
    "terakota": ("Toprak Ordusu",
        "İlk imparator Qin Shi Huang'ın mezarını sonsuza dek korumak için pişmiş topraktan yapılmış binlerce askerden biri. "
        "Her birinin yüzü farklıdır."),
    "saolin": ("Kung Fu Ustası",
        "Şaolin Tapınağı'nda yıllarca kung fu ve meditasyon çalışmış keşiş. Silahsız elleriyle bir orduya karşı durabilir."),
    "taocu": ("Tılsım Ustası",
        "Yin ve yang dengesini bilen Taocu büyücü. Kırmızı yazılı sarı kâğıt tılsımlarıyla hayaletleri bağlar, ateş çağırır."),
    "qilin": ("Uğur Getiren",
        "Ejderha pullu, geyik gövdeli kutsal yaratık. Yalnızca bilge bir hükümdar ya da büyük bir bilge doğacağı zaman "
        "görünür; çimenleri bile ezmeyecek kadar naziktir."),
}

# id, ad, kartlar, stat, oran, açıklama
BAGLAR = [
    # mitoloji içi
    ("gok_yer", "Gök ve Yer", ["ulgen", "erlik"], "saldiri", 0.10, "İki kardeş tanrının ezelî rekabeti birbirlerini kamçılar."),
    ("korkut_tepegoz", "Dede Korkut Hikâyesi", ["dede_korkut", "tepegoz"], "can", 0.10, "Ozan, devin hikâyesini anlattıkça ikisi de efsaneleşir."),
    ("gaziler", "Gazi Ruhu", ["battal_gazi", "koroglu", "akinci"], "saldiri", 0.08, "Halk kahramanları aynı sancak altında."),
    ("bozkurt", "Bozkurt Soyu", ["asena", "alp_er_tunga"], "hiz", 0.10, "Bozkurt ananın torunu, atasıyla birlikte koşar."),
    ("olimpos", "Olimpos Kardeşleri", ["zeus", "hades"], "saldiri", 0.12, "Dünyayı paylaşan iki kardeş yan yana."),
    ("ikizler", "Delos İkizleri", ["apollon", "artemis"], "hiz", 0.10, "Güneş ve ayın ikiz okçuları."),
    ("gorgon_avi", "Gorgon Avı", ["perseus", "athena"], "savunma", 0.10, "Athena'nın kalkanı Perseus'un elinde."),
    ("yeralti", "Yeraltı Kapısı", ["hades", "kerberos"], "can", 0.12, "Efendi ve sadık bekçisi."),
    ("on_iki_gorev", "On İki Görev", ["herakles", "kerberos"], "saldiri", 0.08, "Herakles'in son görevi."),
    ("truva", "Truva Seferi", ["akhilleus", "hoplit"], "savunma", 0.08, "Akhilleus ve falanksı."),
    ("asgard", "Asgard Hanedanı", ["odin", "thor"], "saldiri", 0.10, "Baba ve oğul Asgard'ı korur."),
    ("loki_cocuklari", "Loki'nin Çocukları", ["loki", "fenrir", "hel"], "saldiri", 0.12, "Ragnarök'ü getirecek aile."),
    ("gleipnir", "Gleipnir Yemini", ["tyr", "fenrir"], "savunma", 0.10, "Feda edilen elin hatırası."),
    ("valhalla", "Valhalla'nın Seçilmişleri", ["valkur", "berserker"], "hiz", 0.10, "Valkür, en cesur savaşçıyı seçer."),
    ("ra_gozu", "Ra'nın Gözü", ["ra", "sekhmet", "hathor"], "saldiri", 0.10, "Güneş tanrısı ve iki yüzlü kızı."),
    ("osiris_ailesi", "Osiris'in Ailesi", ["isis", "horus"], "can", 0.12, "Ana ve oğul, babanın öcü için."),
    ("taht_kavgasi", "Taht Kavgası", ["horus", "set"], "saldiri", 0.10, "Amca ile yeğenin bitmeyen savaşı."),
    ("ahiret", "Ahiret Yolu", ["anubis", "mumya"], "savunma", 0.10, "Rehber ve koruduğu ölü."),
    ("gunes_firtina", "Güneş ve Fırtına", ["amaterasu", "susanoo"], "saldiri", 0.10, "Küs kardeşler aynı safta."),
    ("firtina", "Fırtına Getirenler", ["raijin", "susanoo"], "hiz", 0.08, "Davullar ve dalgalar birlikte."),
    ("bushido", "Kılıç ve Gölge", ["samuray", "ninja"], "hiz", 0.10, "Açık savaşın ustası ve gizli savaşın ustası."),
    ("inari", "Inari Tapınağı", ["miko", "kitsune"], "can", 0.12, "Rahibe ve tapınağın tilki habercisi."),
    ("deniz_sarayi", "Deniz Sarayı", ["ryujin", "kappa"], "savunma", 0.10, "Ejderha kral ve nehirlerin yaramazı."),
    # mitolojiler arası
    ("simsek", "Şimşek Tanrıları", ["zeus", "thor", "raijin"], "saldiri", 0.10, "Üç diyarın gök gürültüsü bir arada."),
    ("gunes_tanrilari", "Güneş Tanrıları", ["ra", "apollon", "amaterasu"], "can", 0.10, "Üç güneş aynı gökte."),
    ("olum", "Ölümün Efendileri", ["hades", "hel", "anubis"], "saldiri", 0.10, "Üç ölüler diyarının hükümdarları."),
    ("yeralti_hanlari", "Karanlık Hanlar", ["erlik", "hades"], "savunma", 0.08, "Yeraltının iki hükümdarı."),
    ("kurtlar", "Kurt Kardeşliği", ["asena", "fenrir"], "saldiri", 0.10, "Bozkurt ile dev kurt aynı sürüde."),
    ("ana_tanricalar", "Ana Tanrıçalar", ["umay", "isis", "freya"], "can", 0.10, "Koruyan, büyüten anaların gücü."),
    ("avcilar", "Avcı Okları", ["artemis", "amazon"], "hiz", 0.10, "Tanrıça ve onun savaşçı kızları."),
    ("tek_goz", "Tek Gözlüler", ["tepegoz", "odin"], "savunma", 0.10, "Biri doğuştan, biri bilgelik için tek gözlü."),
    ("yilanlar", "Yılan Kraliçeleri", ["sahmeran", "medusa"], "saldiri", 0.10, "İki yılan saçlı kraliçe."),
    ("devler", "Devler Ordusu", ["ymir", "tepegoz", "oni"], "can", 0.10, "Üç diyarın en iri canavarları."),
    ("kediler", "Kedi Tanrıçalar", ["bastet", "sekhmet"], "hiz", 0.10, "Ev kedisi ile çöl aslanı."),
    ("sifacilar", "Şifacılar", ["asklepios", "idun"], "can", 0.10, "Hekimlik ve gençlik bir arada."),
    ("bilgeler", "Bilgeler Meclisi", ["thoth", "dede_korkut", "athena"], "savunma", 0.10, "Yazının, sözün ve aklın ustaları."),
    ("bati_yolculugu", "Batı'ya Yolculuk", ["sun_wukong", "long"], "hiz", 0.10, "Maymun Kral ve atına dönüşen ejderha prens."),
    ("cennet_sarayi", "Cennet Sarayı", ["yesim_imparator", "sun_wukong"], "saldiri", 0.12, "Maymun Kral'ın göklerde kopardığı kıyamet."),
    ("ejder_ve_nezha", "Ejder Kral'ın Öcü", ["nezha", "long"], "saldiri", 0.10, "Ezeli düşmanlar aynı safta."),
    ("ay_ve_tavsan", "Ay Sarayı", ["change", "qilin"], "can", 0.10, "Ay tanrıçası ve uğur yaratığı."),
    ("iblis_avcilari", "İblis Avcıları", ["zhong_kui", "taocu"], "savunma", 0.10, "İki usta, bir hayalet avı."),
    ("imparator_ordusu", "İmparatorun Ordusu", ["guan_yu", "terakota"], "savunma", 0.10, "General ve toprak askerleri."),
    ("yilan_tanricalar", "Yılan Gövdeli Ana", ["nuwa", "sahmeran"], "can", 0.10, "İki yılan gövdeli bilge kadın."),
    ("dovus_ustalari", "Dövüş Ustaları", ["saolin", "samuray"], "hiz", 0.08, "Şaolin ve Buşido."),
    ("ejderhalar", "Ejderha Kralları", ["long", "ryujin"], "can", 0.10, "Doğu'nun iki deniz ejderhası."),
    ("ay_tanricalari", "Ay Işığı", ["change", "artemis"], "hiz", 0.10, "İki ay tanrıçası."),
    ("savas_tanrilari", "Savaş Tanrıları", ["guan_yu", "tyr", "akhilleus"], "saldiri", 0.08, "Üç diyarın onurlu savaşçıları."),
    ("hilebazlar", "Hilebazlar", ["sun_wukong", "loki", "kitsune"], "hiz", 0.10, "Kılık değiştiren üç düzenbaz."),
    # 2026-11-24: bağı olmayan kartlar için yeni bağlar
    ("ergenekon", "Ergenekon'dan Çıkış", ["asena", "demir_alp"], "savunma", 0.10, "Demir dağı eriten demirci ve yolu gösteren bozkurt."),
    ("gok_yolculugu", "Gök Yolculuğu", ["kam", "ulgen"], "can", 0.10, "Şaman davuluyla göğün katlarına çıkıp Ülgen'e ulaşır."),
    ("lohusa_bekcisi", "Lohusa Bekçisi", ["umay", "al_karisi"], "savunma", 0.10, "Umay Ana anneleri ve bebekleri Al Karısı'ndan korur; ezeli düşmanlar birbirini sınar."),
    ("ragnarok_duellosu", "Ragnarök Düellosu", ["heimdall", "loki"], "saldiri", 0.10, "Kıyamet günü birbirini yok edecek iki ezeli düşman."),
    ("seidr", "Seiðr Büyüsü", ["freya", "volva"], "saldiri", 0.10, "Freya'nın öğrettiği kehanet büyüsü kâhinin elinde."),
    ("runler", "Rünlerin Sırrı", ["odin", "runa_ustasi"], "can", 0.08, "Dünya ağacında asılı kalıp rünleri bulan Odin ve öğrencisi."),
    ("nil_bekcileri", "Nil'in Bekçileri", ["sobek", "kahin"], "savunma", 0.10, "Timsah tanrı ve Nil'in taşkınlarını okuyan kâhin."),
    ("yedi_akrep", "Yedi Akrep", ["isis", "akrep"], "savunma", 0.10, "İsis'i saklandığı bataklıkta yedi akrep korumuştu."),
    ("hathor_rahibesi", "Hathor'un Rahibesi", ["hathor", "ankh_rahibesi"], "can", 0.10, "Tanrıça ve tapınağında ankh taşıyan rahibesi."),
    ("kurama_dagi", "Kurama Dağı", ["tengu", "samuray"], "hiz", 0.10, "Efsaneye göre Kurama Dağı'nın tengusu genç samuraya kılıç öğretti."),
    ("kalkan_duvari", "Kalkan Duvarı", ["bahadir", "kalkanci", "hoplit"], "savunma", 0.10, "Üç diyarın kalkancıları omuz omuza."),
    ("su_ruhlari", "Su Ruhları", ["su_iyesi", "kappa", "nimf"], "hiz", 0.10, "Göllerin, nehirlerin ve pınarların sahipleri."),
    ("olumcul_ezgi", "Ölümcül Ezgi", ["siren", "yuki_onna"], "saldiri", 0.08, "Yolcuyu güzelliğiyle çağırıp ölüme götürenler."),
    ("kadim_bekciler", "Kadim Bekçiler", ["sfenks", "minotor"], "savunma", 0.10, "Biri piramitlerin, biri labirentin yarı insan bekçisi."),
]
