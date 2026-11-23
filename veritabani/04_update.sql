-- Sinema Rezervasyon Sistemi - UPDATE Sorgulari

-- 1) 3 numarali rezervasyonun odeme durumunu 'Odendi' yap
UPDATE REZERVASYON
SET odeme_durumu = 'Odendi'
WHERE rezervasyon_no = 3;

-- 2) 2 numarali sinemanin (Atlas Park AVM) gosterim fiyatlarina %10 zam yap
UPDATE GOSTERIM
SET bilet_fiyati = bilet_fiyati * 1.10
WHERE sinema_no = 2;

-- 3) 2 numarali musterinin e-posta adresini guncelle
UPDATE MUSTERI
SET eposta = 'elif.kaya@gmail.com'
WHERE musteri_no = 2;

-- 4) Kadikoy Sinemalari VIP salonunun kapasitesini 50 yap
UPDATE SALON
SET kapasite = 50
WHERE sinema_no = 1 AND salon_no = 2;

-- 5) 19 Kasim 2026 oncesi yapilmis ve hala beklemede olan rezervasyonlari iptal et
UPDATE REZERVASYON
SET odeme_durumu = 'Iptal'
WHERE odeme_durumu = 'Beklemede' AND rezervasyon_tarihi < #2026-11-19#;
