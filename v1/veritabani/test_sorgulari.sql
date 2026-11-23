-- =========================================================
--  Test sorguları
--  Hata vermesi gereken satırlar tek tek çalıştırılıp
--  trigger mesajları gözlenebilir (Workbench'te satır seçip Ctrl+Enter).
-- =========================================================
USE sinema_db;

-- ---------- VIEW'LAR ----------
SELECT * FROM v_gosterim_detay ORDER BY tarih, baslangic_saati;
SELECT * FROM v_rezervasyon_detay ORDER BY rezervasyon_no;
SELECT * FROM v_film_hasilat ORDER BY hasilat DESC;
SELECT * FROM v_musteri_ozet ORDER BY toplam_harcama DESC;
SELECT * FROM v_sinema_ozet;

-- ---------- FONKSİYONLAR ----------
SELECT gosterim_no, fn_bos_koltuk(gosterim_no) AS bos FROM gosterim;

-- ---------- PROSEDÜRLER ----------
CALL sp_rezervasyon_yap(3, 1, 100, @yeni);
SELECT @yeni AS yeni_rezervasyon_no;
CALL sp_odeme_al(@yeni);
CALL sp_tarih_araligi_rapor(CURDATE(), CURDATE() + INTERVAL 10 DAY);
SELECT * FROM rezervasyon_log WHERE rezervasyon_no = @yeni;

-- ---------- TRIGGER TESTLERİ (hepsi HATA vermeli) ----------

-- 1) dolu koltuk (gösterim 1, koltuk 15 seed'de satıldı)
CALL sp_rezervasyon_yap(5, 1, 15, @x);

-- 2) kapasiteden büyük koltuk (Salon 1 kapasite 120)
CALL sp_rezervasyon_yap(5, 1, 500, @x);

-- 3) salon çakışması (gösterim 1: yarın 14:30, Kayıp Şehir 135 dk -> 17:00'ye kadar dolu)
INSERT INTO gosterim (film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati)
VALUES (2, 1, 1, CURDATE() + INTERVAL 1 DAY, '16:00', 250);

-- 4) vizyondan önce gösterim
INSERT INTO gosterim (film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati)
VALUES (8, 3, 2, '2026-11-20', '11:00', 250);

-- 5) iptal edilmiş rezervasyonu tekrar aktif etmek
UPDATE rezervasyon SET odeme_durumu = 'Odendi' WHERE rezervasyon_no = 8;

-- 6) kapasiteyi satılmış koltukların altına düşürmek (salon 1-1'de 88 nolu koltuk satıldı)
UPDATE salon SET kapasite = 50 WHERE sinema_no = 1 AND salon_no = 1;

-- 7) CHECK kısıtı: geçersiz e-posta
INSERT INTO musteri (ad, soyad, eposta) VALUES ('Test', 'Kisi', 'gecersiz-eposta');

-- 8) zaten ödenmiş rezervasyonun ödemesini tekrar almak
CALL sp_odeme_al(1);
