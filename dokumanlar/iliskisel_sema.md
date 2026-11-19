# Sinema Rezervasyon Sistemi - İlişkisel Şema ve Açıklamalar

## 1. ER Diyagramından Tablolara Dönüşüm

ER diyagramı: `er_diyagrami.drawio` (draw.io / diagrams.net ile açılır), resmi: `er_diyagrami.svg`

Gösterim (Chen):
- Dikdörtgen: varlık, çift dikdörtgen: zayıf varlık (SALON)
- Elips: nitelik, altı çizili: anahtar, kesikli altı çizili: kısmi anahtar
- Çift elips: çok değerli nitelik (telefon)
- Eşkenar dörtgen: ilişki, çift eşkenar dörtgen: tanımlayıcı ilişki (SAHİPTİR)
- Çift çizgi: tam katılım (ör. her salon bir sinemaya ait olmak zorunda)

| Varlık / İlişki | Tablo | Açıklama |
|---|---|---|
| SİNEMA | `sinema` | |
| SİNEMA.telefon (çok değerli) | `sinema_telefon` | 1NF gereği ayrı tablo, PK (sinema_no, telefon) |
| SALON (zayıf) | `salon` | PK (sinema_no, salon_no). Salon numarası sadece kendi sinemasında benzersiz, bu yüzden sahibinin anahtarıyla birlikte tanımlanır |
| FİLM | `film` | |
| GÖSTERİM | `gosterim` | Film ile salon arasında bir ilişki ama kendi nitelikleri (tarih, saat, fiyat) var ve rezervasyonlar ona bağlanıyor, bu yüzden varlık olarak modellendi |
| MÜŞTERİ | `musteri` | |
| MÜŞTERİ.telefon (çok değerli) | `musteri_telefon` | PK (musteri_no, telefon) |
| REZERVASYON | `rezervasyon` | YAPAR ve AİTTİR ilişkileri 1-N olduğu için FK olarak N tarafına (rezervasyon) eklendi |
| - | `rezervasyon_log` | Ödeme durumu geçmişi, trigger ile doluyor (ek) |

## 2. İlişkisel Şema

(PK **kalın**, FK *italik*)

- **sinema**(**sinema_no**, ad, adres, sehir)
- **sinema_telefon**(***sinema_no***, **telefon**)
- **salon**(***sinema_no***, **salon_no**, ad, kapasite)
- **film**(**film_no**, ad, tur, sure_dk, vizyon_tarihi)
- **gosterim**(**gosterim_no**, *film_no*, *sinema_no*, *salon_no*, tarih, baslangic_saati, bilet_fiyati)
  - (sinema_no, salon_no) → salon
- **musteri**(**musteri_no**, ad, soyad, eposta, kayit_tarihi)
- **musteri_telefon**(***musteri_no***, **telefon**)
- **rezervasyon**(**rezervasyon_no**, *musteri_no*, *gosterim_no*, rezervasyon_tarihi, koltuk_no, odeme_durumu, ucret)
- **rezervasyon_log**(**log_no**, *rezervasyon_no*, eski_durum, yeni_durum, islem_zamani)

Metinde olmayıp eklenenler: `sinema.sehir`, `musteri.kayit_tarihi`, `rezervasyon.ucret`, `rezervasyon_log` tablosu.

## 3. Kısıtlar

| Kısıt | Nerede | Amaç |
|---|---|---|
| UNIQUE | `sinema.ad` | Aynı isimde iki sinema olmasın |
| UNIQUE | `musteri.eposta` | Bir e-posta ile tek müşteri |
| UNIQUE | `gosterim(sinema_no, salon_no, tarih, baslangic_saati)` | Aynı salonda aynı anda başlayan iki gösterim olmasın |
| CHECK | `salon.kapasite BETWEEN 1 AND 500` | |
| CHECK | `film.sure_dk BETWEEN 1 AND 400` | |
| CHECK | `gosterim.bilet_fiyati >= 0` | |
| CHECK | `musteri.eposta LIKE '_%@_%._%'` | Basit e-posta format kontrolü |
| CHECK | `rezervasyon.koltuk_no > 0` | |
| ENUM | `rezervasyon.odeme_durumu` | Sadece Beklemede / Odendi / Iptal |
| FK ON DELETE CASCADE | salon, telefon tabloları | Sinema/müşteri silinirse bunlar da silinir |
| FK ON DELETE RESTRICT | gosterim, rezervasyon | Gösterimi olan film/salon, rezervasyonu olan müşteri silinemez |

## 4. Normalizasyon

- **1NF:** Bütün nitelikler atomik. Birden fazla telefon numarası tek alanda tutulmuyor, ayrı tablolarda.
- **2NF:** Bileşik anahtarlı tablolar `salon`, `sinema_telefon`, `musteri_telefon`. `salon` tablosunda ad ve kapasite anahtarın tamamına (sinema_no + salon_no) bağlı. Sadece sinema_no'ya bağlı bir bilgi (ör. sinema adı) bu tabloda yok. Telefon tablolarında anahtar dışında nitelik yok. Kısmi bağımlılık bulunmuyor.
- **3NF:** Geçişli bağımlılık yok. Örneğin `gosterim` tablosunda film adı veya salon kapasitesi tutulmuyor, bunlar FK üzerinden JOIN ile alınıyor (view'larda).
- **Bilinçli tekrar:** `rezervasyon.ucret`, gosterim.bilet_fiyati'nin bir kopyası gibi görünse de aslında bilet satıldığı andaki fiyat. Sonradan gösterimin fiyatı değişirse eski satışların tutarı bozulmasın diye ayrı tutuluyor (tarihsel veri).

## 5. Fonksiyonlar

| Ad | Ne yapar |
|---|---|
| `fn_bos_koltuk(gosterim_no)` | Kapasite - iptal edilmemiş rezervasyon sayısı |
| `fn_salon_dolu_mu(sinema, salon, tarih, saat, süre, hariç)` | Verilen aralıkta (film süresi + 15 dk temizlik) salonda başka gösterim var mı |

## 6. Triggerlar

| Trigger | Olay | Kontrol |
|---|---|---|
| `trg_gosterim_ekle` | BEFORE INSERT gosterim | Film vizyon tarihinden önce gösterilemez. Salonda saat çakışması olamaz |
| `trg_gosterim_guncelle` | BEFORE UPDATE gosterim | Aynı kontroller (kendisi hariç tutularak) |
| `trg_rezervasyon_ekle` | BEFORE INSERT rezervasyon | Geçmiş gösterime satış yapılamaz, koltuk numarası kapasiteyi aşamaz, koltuk dolu olamaz. Ücret gösterimden otomatik alınır |
| `trg_rezervasyon_guncelle` | BEFORE UPDATE rezervasyon | İptal geri alınamaz. Koltuk değişirse yeni koltuk boş olmalı |
| `trg_rezervasyon_log_ekle` | AFTER INSERT rezervasyon | Log tablosuna "oluşturuldu" kaydı |
| `trg_rezervasyon_log_guncelle` | AFTER UPDATE rezervasyon | Durum değiştiyse log tablosuna eski → yeni |
| `trg_salon_kapasite` | BEFORE UPDATE salon | Kapasite, ileri tarihli gösterimlerde satılmış koltuk numaralarının altına düşürülemez |

Neden UNIQUE(gosterim_no, koltuk_no) yerine trigger kullanıldı: İptal edilen rezervasyonlar silinmiyor, tabloda "Iptal" durumunda kalıyor. Düz bir UNIQUE kısıtı, iptal edilmiş koltuğun tekrar satılmasını engellerdi. Bu yüzden kontrol, iptalleri hariç tutan bir trigger ile yapıldı.

## 7. View'lar

| View | İçerik | Kullanıldığı yer |
|---|---|---|
| `v_gosterim_detay` | Gösterim + film + salon + sinema, satılan/boş koltuk, doluluk % | Gösterimler sayfası, bilet satışı, ana sayfa |
| `v_rezervasyon_detay` | Rezervasyon + müşteri + film + salon | Rezervasyonlar, müşteri geçmişi |
| `v_film_hasilat` | Film başına gösterim sayısı, ödenen/bekleyen bilet, hasılat | Filmler, raporlar |
| `v_musteri_ozet` | Müşteri telefonları, bilet sayısı, toplam harcama | Müşteriler, raporlar |
| `v_sinema_ozet` | Salon sayısı, toplam kapasite, gösterim sayısı, hasılat | Sinemalar, raporlar |

## 8. Stored Procedure'ler

| Prosedür | Açıklama |
|---|---|
| `sp_rezervasyon_yap(musteri, gosterim, koltuk, OUT rez_no)` | Transaction içinde gösterim satırını `FOR UPDATE` ile kilitleyip rezervasyon ekler. Aynı anda iki kişinin aynı koltuğu almasını engeller. Hata olursa ROLLBACK |
| `sp_odeme_al(rez_no)` | Sadece "Beklemede" durumundakini "Odendi" yapar |
| `sp_rezervasyon_iptal(rez_no)` | Başlamış gösterimin bileti iptal edilemez |
| `sp_tarih_araligi_rapor(baslangic, bitis)` | Günlük gösterim, bilet, hasılat raporu |
