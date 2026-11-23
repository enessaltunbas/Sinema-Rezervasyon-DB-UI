-- Sinema Rezervasyon Sistemi - SELECT Sorgulari

-- 1) 5 Aralik 2026 tarihindeki gosterimleri film, sinema, salon ve bitis saati ile listele
SELECT g.gosterim_no, f.ad AS film_adi, s.ad AS sinema_adi, sa.ad AS salon_adi,
       g.baslangic_saati, DateAdd("n", f.sure_dk, g.baslangic_saati) AS bitis_saati, g.bilet_fiyati
FROM ((GOSTERIM AS g
      INNER JOIN FILM AS f ON g.film_no = f.film_no)
      INNER JOIN SALON AS sa ON (g.sinema_no = sa.sinema_no AND g.salon_no = sa.salon_no))
      INNER JOIN SINEMA AS s ON sa.sinema_no = s.sinema_no
WHERE g.tarih = #2026-12-05#
ORDER BY g.baslangic_saati;

-- 2) Her gosterim icin satilan ve kalan bos koltuk sayisi
SELECT g.gosterim_no, f.ad AS film_adi, sa.ad AS salon_adi, sa.kapasite,
       COUNT(r.rezervasyon_no) AS satilan_koltuk,
       sa.kapasite - COUNT(r.rezervasyon_no) AS bos_koltuk
FROM ((GOSTERIM AS g
      INNER JOIN FILM AS f ON g.film_no = f.film_no)
      INNER JOIN SALON AS sa ON (g.sinema_no = sa.sinema_no AND g.salon_no = sa.salon_no))
      LEFT JOIN (SELECT rezervasyon_no, gosterim_no FROM REZERVASYON WHERE odeme_durumu <> 'Iptal') AS r
             ON g.gosterim_no = r.gosterim_no
GROUP BY g.gosterim_no, f.ad, sa.ad, sa.kapasite
ORDER BY g.gosterim_no;

-- 3) Film bazinda odenen bilet sayisi ve toplam hasilat
SELECT f.ad AS film_adi, f.tur, COUNT(r.rezervasyon_no) AS odenen_bilet, SUM(g.bilet_fiyati) AS hasilat
FROM (FILM AS f
     INNER JOIN GOSTERIM AS g ON f.film_no = g.film_no)
     INNER JOIN REZERVASYON AS r ON g.gosterim_no = r.gosterim_no
WHERE r.odeme_durumu = 'Odendi'
GROUP BY f.ad, f.tur
ORDER BY SUM(g.bilet_fiyati) DESC;

-- 4) En az 2 aktif bileti olan musteriler ve toplam bilet tutarlari
SELECT m.musteri_no, m.ad & ' ' & m.soyad AS musteri, m.eposta,
       COUNT(r.rezervasyon_no) AS bilet_sayisi,
       SUM(g.bilet_fiyati) AS toplam_tutar
FROM (MUSTERI AS m
     INNER JOIN REZERVASYON AS r ON m.musteri_no = r.musteri_no)
     INNER JOIN GOSTERIM AS g ON r.gosterim_no = g.gosterim_no
WHERE r.odeme_durumu <> 'Iptal'
GROUP BY m.musteri_no, m.ad, m.soyad, m.eposta
HAVING COUNT(r.rezervasyon_no) >= 2
ORDER BY SUM(g.bilet_fiyati) DESC, m.musteri_no;

-- 5) Sinema bazinda salon sayisi ve toplam kapasite
SELECT s.sinema_no, s.ad AS sinema_adi, COUNT(sa.salon_no) AS salon_sayisi, SUM(sa.kapasite) AS toplam_kapasite
FROM SINEMA AS s
     INNER JOIN SALON AS sa ON s.sinema_no = sa.sinema_no
GROUP BY s.sinema_no, s.ad
ORDER BY SUM(sa.kapasite) DESC;
