-- Sinema Rezervasyon Sistemi - Ornek Veriler (INSERT INTO)

-- SINEMA
INSERT INTO SINEMA (sinema_no, ad, adres) VALUES (1, 'Kadıköy Sinemaları', 'Caferağa Mah. Moda Cad. No:42 Kadıköy / İstanbul');
INSERT INTO SINEMA (sinema_no, ad, adres) VALUES (2, 'Atlas Park AVM', 'Kemalpaşa Mah. Atlas Cad. No:7 Bağcılar / İstanbul');
INSERT INTO SINEMA (sinema_no, ad, adres) VALUES (3, 'Kızılay Sinema', 'Atatürk Bulvarı No:110 Çankaya / Ankara');

-- SINEMA_TELEFON
INSERT INTO SINEMA_TELEFON (sinema_no, telefon) VALUES (1, '0216 345 12 12');
INSERT INTO SINEMA_TELEFON (sinema_no, telefon) VALUES (1, '0216 345 12 13');
INSERT INTO SINEMA_TELEFON (sinema_no, telefon) VALUES (2, '0212 655 40 40');
INSERT INTO SINEMA_TELEFON (sinema_no, telefon) VALUES (3, '0312 418 77 01');
INSERT INTO SINEMA_TELEFON (sinema_no, telefon) VALUES (3, '0312 418 77 02');

-- SALON
INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (1, 1, 'Salon 1', 120);
INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (1, 2, 'VIP Salon', 40);
INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (2, 1, 'IMAX', 150);
INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (2, 2, 'Salon 2', 100);
INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (3, 1, 'Salon 1', 90);
INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (3, 2, 'Salon 2', 70);

-- FILM
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (1, 'Son Durak', 'Dram', 118, #2026-10-02#);
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (2, 'Gece Yarısı Treni', 'Gerilim', 104, #2026-10-16#);
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (3, 'Uzay Kampı', 'Animasyon', 92, #2026-10-30#);
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (4, 'Kayıp Şehir', 'Macera', 135, #2026-11-06#);
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (5, 'Annemin Bahçesi', 'Komedi', 101, #2026-11-13#);
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (6, 'Yıldızlar Arası Mektup', 'Bilim Kurgu', 152, #2026-12-04#);
INSERT INTO FILM (film_no, ad, tur, sure_dk, vizyon_tarihi) VALUES (7, 'Sessiz Kıyı', 'Dram', 97, #2026-11-20#);

-- MUSTERI
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (1, 'Ahmet', 'Yılmaz', 'ahmet.yilmaz@gmail.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (2, 'Elif', 'Kaya', 'elifkaya@hotmail.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (3, 'Mehmet', 'Demir', 'mehmet.demir@outlook.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (4, 'Zeynep', 'Çelik', 'zeynepcelik@gmail.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (5, 'Burak', 'Şahin', 'burak.sahin@yahoo.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (6, 'Selin', 'Arslan', 'selin.arslan@gmail.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (7, 'Emre', 'Koç', 'emrekoc@gmail.com');
INSERT INTO MUSTERI (musteri_no, ad, soyad, eposta) VALUES (8, 'Ayşe', 'Öztürk', 'ayse.ozturk@hotmail.com');

-- MUSTERI_TELEFON
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (1, '0532 111 22 33');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (1, '0216 444 55 66');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (2, '0541 222 33 44');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (3, '0555 333 44 55');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (4, '0533 444 55 66');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (4, '0507 123 45 67');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (5, '0542 555 66 77');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (6, '0536 666 77 88');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (7, '0530 777 88 99');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (8, '0545 888 99 00');
INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (8, '0212 333 22 11');

-- GOSTERIM
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (1, 4, 1, 1, #2026-12-05#, #14:30:00#, 260);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (2, 4, 1, 1, #2026-12-05#, #18:00:00#, 300);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (3, 3, 1, 2, #2026-12-05#, #11:00:00#, 220);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (4, 5, 1, 2, #2026-12-05#, #20:00:00#, 280);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (5, 6, 2, 1, #2026-12-05#, #21:00:00#, 340);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (6, 1, 2, 2, #2026-12-06#, #14:30:00#, 250);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (7, 2, 3, 1, #2026-12-06#, #18:00:00#, 240);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (8, 6, 1, 1, #2026-12-06#, #21:00:00#, 320);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (9, 5, 3, 2, #2026-12-07#, #15:00:00#, 220);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (10, 4, 2, 1, #2026-12-07#, #18:30:00#, 320);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (11, 3, 2, 2, #2026-12-08#, #11:00:00#, 200);
INSERT INTO GOSTERIM (gosterim_no, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES (12, 2, 1, 2, #2026-12-08#, #21:30:00#, 260);

-- REZERVASYON
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (1, 1, 1, #2026-11-15 10:15:00#, 15, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (2, 2, 1, #2026-11-15 10:20:00#, 16, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (3, 3, 1, #2026-11-16 19:05:00#, 45, 'Beklemede');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (4, 4, 2, #2026-11-17 12:40:00#, 30, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (5, 5, 2, #2026-11-17 12:45:00#, 31, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (6, 6, 3, #2026-11-18 09:30:00#, 5, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (7, 7, 3, #2026-11-18 09:32:00#, 6, 'Iptal');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (8, 8, 4, #2026-11-18 14:10:00#, 12, 'Beklemede');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (9, 1, 5, #2026-11-19 21:00:00#, 70, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (10, 2, 5, #2026-11-19 21:02:00#, 71, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (11, 3, 6, #2026-11-20 11:11:00#, 22, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (12, 4, 7, #2026-11-20 16:45:00#, 40, 'Iptal');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (13, 5, 7, #2026-11-20 16:50:00#, 41, 'Beklemede');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (14, 6, 8, #2026-11-21 10:00:00#, 55, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (15, 7, 9, #2026-11-21 13:25:00#, 18, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (16, 8, 10, #2026-11-22 18:30:00#, 100, 'Beklemede');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (17, 1, 11, #2026-11-22 20:00:00#, 9, 'Odendi');
INSERT INTO REZERVASYON (rezervasyon_no, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu) VALUES (18, 2, 10, #2026-11-22 20:45:00#, 101, 'Odendi');
