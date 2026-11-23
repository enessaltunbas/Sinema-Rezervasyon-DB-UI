-- Sinema Rezervasyon Sistemi - DELETE Sorgulari

-- 1) Iptal edilmis rezervasyonlari sil
DELETE FROM REZERVASYON
WHERE odeme_durumu = 'Iptal';

-- 2) 1 numarali musterinin ikinci telefon numarasini sil
DELETE FROM MUSTERI_TELEFON
WHERE musteri_no = 1 AND telefon = '0216 444 55 66';

-- 3) Hic rezervasyonu olmayan gosterimleri sil
DELETE FROM GOSTERIM
WHERE gosterim_no NOT IN (SELECT r.gosterim_no FROM REZERVASYON AS r);

-- 4) Hic gosterimi olmayan filmleri sil
DELETE FROM FILM
WHERE film_no NOT IN (SELECT g.film_no FROM GOSTERIM AS g);
