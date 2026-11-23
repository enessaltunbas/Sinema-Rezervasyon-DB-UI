-- =========================================================
--  SINEMA REZERVASYON SISTEMI
--  Veri Tabani Yonetim Sistemleri Dersi - Proje 21
--
--  Bu dosya veritabanini sifirdan kurar:
--    1. Tablolar ve kisitlar
--    2. Fonksiyonlar
--    3. Triggerlar
--    4. Viewlar
--    5. Stored procedure'ler
--    6. Ornek veriler
--    7. Web uygulamasinin baglandigi kullanici
--
--  Calistirma: MySQL Workbench'te dosyayi acip tamamini calistirin
--  ya da komut satirindan:  mysql -u root -p < sinema_db.sql
-- =========================================================

-- =========================================================
--  Sinema Rezervasyon Sistemi - Tablolar
-- =========================================================

DROP DATABASE IF EXISTS sinema_db;
CREATE DATABASE sinema_db CHARACTER SET utf8mb4 COLLATE utf8mb4_turkish_ci;
USE sinema_db;

-- ---------------------------------------------------------
-- SINEMA
-- ---------------------------------------------------------
CREATE TABLE sinema (
    sinema_no   INT          NOT NULL AUTO_INCREMENT,
    ad          VARCHAR(100) NOT NULL,
    adres       VARCHAR(255) NOT NULL,
    sehir       VARCHAR(50)  NOT NULL,
    PRIMARY KEY (sinema_no),
    UNIQUE KEY uq_sinema_ad (ad)
);

-- sinemanın birden fazla telefonu olabilir (çok değerli nitelik)
CREATE TABLE sinema_telefon (
    sinema_no   INT         NOT NULL,
    telefon     VARCHAR(20) NOT NULL,
    PRIMARY KEY (sinema_no, telefon),
    CONSTRAINT fk_stel_sinema FOREIGN KEY (sinema_no)
        REFERENCES sinema (sinema_no) ON DELETE CASCADE ON UPDATE CASCADE
);

-- ---------------------------------------------------------
-- SALON (zayıf varlık: sinema olmadan salon olmaz)
-- salon_no her sinemanın kendi içinde benzersiz -> (sinema_no, salon_no)
-- ---------------------------------------------------------
CREATE TABLE salon (
    sinema_no   INT         NOT NULL,
    salon_no    INT         NOT NULL,
    ad          VARCHAR(50) NOT NULL,
    kapasite    INT         NOT NULL,
    PRIMARY KEY (sinema_no, salon_no),
    CONSTRAINT fk_salon_sinema FOREIGN KEY (sinema_no)
        REFERENCES sinema (sinema_no) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT chk_salon_no CHECK (salon_no > 0),
    CONSTRAINT chk_kapasite CHECK (kapasite BETWEEN 1 AND 500)
);

-- ---------------------------------------------------------
-- FILM
-- ---------------------------------------------------------
CREATE TABLE film (
    film_no         INT          NOT NULL AUTO_INCREMENT,
    ad              VARCHAR(150) NOT NULL,
    tur             VARCHAR(50)  NOT NULL,
    sure_dk         INT          NOT NULL,
    vizyon_tarihi   DATE         NOT NULL,
    PRIMARY KEY (film_no),
    CONSTRAINT chk_sure CHECK (sure_dk BETWEEN 1 AND 400)
);

-- ---------------------------------------------------------
-- GOSTERIM (film - salon arasındaki ilişki, kendi nitelikleri olduğu için tablo)
-- ---------------------------------------------------------
CREATE TABLE gosterim (
    gosterim_no      INT          NOT NULL AUTO_INCREMENT,
    film_no          INT          NOT NULL,
    sinema_no        INT          NOT NULL,
    salon_no         INT          NOT NULL,
    tarih            DATE         NOT NULL,
    baslangic_saati  TIME         NOT NULL,
    bilet_fiyati     DECIMAL(8,2) NOT NULL,
    PRIMARY KEY (gosterim_no),
    -- aynı salonda aynı anda başlayan iki gösterim olamaz
    UNIQUE KEY uq_gosterim_salon_zaman (sinema_no, salon_no, tarih, baslangic_saati),
    CONSTRAINT fk_gos_film FOREIGN KEY (film_no)
        REFERENCES film (film_no) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_gos_salon FOREIGN KEY (sinema_no, salon_no)
        REFERENCES salon (sinema_no, salon_no) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_fiyat CHECK (bilet_fiyati >= 0)
);

CREATE INDEX ix_gosterim_tarih ON gosterim (tarih);

-- ---------------------------------------------------------
-- MUSTERI
-- ---------------------------------------------------------
CREATE TABLE musteri (
    musteri_no  INT          NOT NULL AUTO_INCREMENT,
    ad          VARCHAR(50)  NOT NULL,
    soyad       VARCHAR(50)  NOT NULL,
    eposta      VARCHAR(100) NOT NULL,
    kayit_tarihi DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (musteri_no),
    UNIQUE KEY uq_musteri_eposta (eposta),
    CONSTRAINT chk_eposta CHECK (eposta LIKE '_%@_%._%')
);

CREATE TABLE musteri_telefon (
    musteri_no  INT         NOT NULL,
    telefon     VARCHAR(20) NOT NULL,
    PRIMARY KEY (musteri_no, telefon),
    CONSTRAINT fk_mtel_musteri FOREIGN KEY (musteri_no)
        REFERENCES musteri (musteri_no) ON DELETE CASCADE ON UPDATE CASCADE
);

-- ---------------------------------------------------------
-- REZERVASYON
-- ucret: bilet alındığı andaki fiyat (gösterim fiyatı sonradan değişse de bozulmasın diye)
-- ---------------------------------------------------------
CREATE TABLE rezervasyon (
    rezervasyon_no      INT          NOT NULL AUTO_INCREMENT,
    musteri_no          INT          NOT NULL,
    gosterim_no         INT          NOT NULL,
    rezervasyon_tarihi  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    koltuk_no           INT          NOT NULL,
    odeme_durumu        ENUM('Beklemede', 'Odendi', 'Iptal') NOT NULL DEFAULT 'Beklemede',
    ucret               DECIMAL(8,2) NOT NULL DEFAULT 0,
    PRIMARY KEY (rezervasyon_no),
    CONSTRAINT fk_rez_musteri FOREIGN KEY (musteri_no)
        REFERENCES musteri (musteri_no) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT fk_rez_gosterim FOREIGN KEY (gosterim_no)
        REFERENCES gosterim (gosterim_no) ON DELETE RESTRICT ON UPDATE CASCADE,
    CONSTRAINT chk_koltuk CHECK (koltuk_no > 0)
);

CREATE INDEX ix_rez_gosterim_koltuk ON rezervasyon (gosterim_no, koltuk_no);

-- ---------------------------------------------------------
-- REZERVASYON_LOG (trigger ile doldurulur, ödeme durumu geçmişi)
-- ---------------------------------------------------------
CREATE TABLE rezervasyon_log (
    log_no          INT         NOT NULL AUTO_INCREMENT,
    rezervasyon_no  INT         NOT NULL,
    eski_durum      VARCHAR(20),
    yeni_durum      VARCHAR(20) NOT NULL,
    islem_zamani    DATETIME    NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (log_no),
    CONSTRAINT fk_log_rez FOREIGN KEY (rezervasyon_no)
        REFERENCES rezervasyon (rezervasyon_no) ON DELETE CASCADE
);


-- =========================================================
--  Fonksiyonlar
-- =========================================================
USE sinema_db;

DROP FUNCTION IF EXISTS fn_bos_koltuk;
DROP FUNCTION IF EXISTS fn_salon_dolu_mu;

DELIMITER $$

-- Bir gösterimde kaç koltuk boş kaldı (iptal edilenler boş sayılır)
CREATE FUNCTION fn_bos_koltuk(p_gosterim_no INT)
RETURNS INT
READS SQL DATA
BEGIN
    DECLARE v_kapasite INT DEFAULT 0;
    DECLARE v_dolu INT DEFAULT 0;

    SELECT s.kapasite INTO v_kapasite
    FROM gosterim g
    JOIN salon s ON s.sinema_no = g.sinema_no AND s.salon_no = g.salon_no
    WHERE g.gosterim_no = p_gosterim_no;

    SELECT COUNT(*) INTO v_dolu
    FROM rezervasyon
    WHERE gosterim_no = p_gosterim_no AND odeme_durumu <> 'Iptal';

    RETURN v_kapasite - v_dolu;
END$$

-- Verilen salonda, verilen zaman aralığında başka bir gösterim var mı?
-- Film süresine ek olarak 15 dk salon temizlik/hazırlık payı bırakılıyor.
-- p_haric: güncelleme yaparken gösterimin kendisiyle çakışmasın diye
CREATE FUNCTION fn_salon_dolu_mu(
    p_sinema_no INT,
    p_salon_no  INT,
    p_tarih     DATE,
    p_saat      TIME,
    p_sure_dk   INT,
    p_haric     INT
)
RETURNS TINYINT
READS SQL DATA
BEGIN
    DECLARE v_bas DATETIME;
    DECLARE v_bit DATETIME;
    DECLARE v_adet INT DEFAULT 0;

    SET v_bas = TIMESTAMP(p_tarih, p_saat);
    SET v_bit = v_bas + INTERVAL (p_sure_dk + 15) MINUTE;

    SELECT COUNT(*) INTO v_adet
    FROM gosterim g
    JOIN film f ON f.film_no = g.film_no
    WHERE g.sinema_no = p_sinema_no
      AND g.salon_no  = p_salon_no
      AND g.gosterim_no <> IFNULL(p_haric, 0)
      AND g.tarih BETWEEN p_tarih - INTERVAL 1 DAY AND p_tarih + INTERVAL 1 DAY
      AND TIMESTAMP(g.tarih, g.baslangic_saati) < v_bit
      AND TIMESTAMP(g.tarih, g.baslangic_saati) + INTERVAL (f.sure_dk + 15) MINUTE > v_bas;

    RETURN IF(v_adet > 0, 1, 0);
END$$

DELIMITER ;


-- =========================================================
--  Triggerlar
-- =========================================================
USE sinema_db;

DROP TRIGGER IF EXISTS trg_gosterim_ekle;
DROP TRIGGER IF EXISTS trg_gosterim_guncelle;
DROP TRIGGER IF EXISTS trg_rezervasyon_ekle;
DROP TRIGGER IF EXISTS trg_rezervasyon_guncelle;
DROP TRIGGER IF EXISTS trg_rezervasyon_log_ekle;
DROP TRIGGER IF EXISTS trg_rezervasyon_log_guncelle;
DROP TRIGGER IF EXISTS trg_salon_kapasite;

DELIMITER $$

-- ---------------------------------------------------------
-- 1) Gösterim eklenirken:
--    - film vizyona girmeden gösterim olamaz
--    - aynı salonda saatler çakışamaz
-- ---------------------------------------------------------
CREATE TRIGGER trg_gosterim_ekle
BEFORE INSERT ON gosterim
FOR EACH ROW
BEGIN
    DECLARE v_sure INT;
    DECLARE v_vizyon DATE;

    SELECT sure_dk, vizyon_tarihi INTO v_sure, v_vizyon
    FROM film WHERE film_no = NEW.film_no;

    IF NEW.tarih < v_vizyon THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Film vizyon tarihinden once gosterime konamaz.';
    END IF;

    IF fn_salon_dolu_mu(NEW.sinema_no, NEW.salon_no, NEW.tarih,
                        NEW.baslangic_saati, v_sure, NULL) = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Bu salonda secilen saatte baska bir gosterim var.';
    END IF;
END$$

-- ---------------------------------------------------------
-- 2) Gösterim güncellenirken aynı kontroller (kendisi hariç)
-- ---------------------------------------------------------
CREATE TRIGGER trg_gosterim_guncelle
BEFORE UPDATE ON gosterim
FOR EACH ROW
BEGIN
    DECLARE v_sure INT;
    DECLARE v_vizyon DATE;

    SELECT sure_dk, vizyon_tarihi INTO v_sure, v_vizyon
    FROM film WHERE film_no = NEW.film_no;

    IF NEW.tarih < v_vizyon THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Film vizyon tarihinden once gosterime konamaz.';
    END IF;

    IF fn_salon_dolu_mu(NEW.sinema_no, NEW.salon_no, NEW.tarih,
                        NEW.baslangic_saati, v_sure, NEW.gosterim_no) = 1 THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Bu salonda secilen saatte baska bir gosterim var.';
    END IF;
END$$

-- ---------------------------------------------------------
-- 3) Rezervasyon eklenirken:
--    - geçmiş gösterime bilet satılmaz
--    - koltuk numarası salon kapasitesini aşamaz
--    - aynı koltuk iki kez satılamaz (iptaller hariç)
--    - ücret gösterimin o anki fiyatından alınır
-- ---------------------------------------------------------
CREATE TRIGGER trg_rezervasyon_ekle
BEFORE INSERT ON rezervasyon
FOR EACH ROW
BEGIN
    DECLARE v_kapasite INT;
    DECLARE v_fiyat DECIMAL(8,2);
    DECLARE v_baslangic DATETIME;
    DECLARE v_dolu INT DEFAULT 0;

    SELECT s.kapasite, g.bilet_fiyati, TIMESTAMP(g.tarih, g.baslangic_saati)
      INTO v_kapasite, v_fiyat, v_baslangic
    FROM gosterim g
    JOIN salon s ON s.sinema_no = g.sinema_no AND s.salon_no = g.salon_no
    WHERE g.gosterim_no = NEW.gosterim_no;

    IF v_baslangic < NOW() THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Baslamis veya gecmis bir gosterime rezervasyon yapilamaz.';
    END IF;

    IF NEW.koltuk_no > v_kapasite THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Koltuk numarasi salon kapasitesinden buyuk olamaz.';
    END IF;

    IF NEW.odeme_durumu <> 'Iptal' THEN
        SELECT COUNT(*) INTO v_dolu
        FROM rezervasyon
        WHERE gosterim_no = NEW.gosterim_no
          AND koltuk_no = NEW.koltuk_no
          AND odeme_durumu <> 'Iptal';

        IF v_dolu > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Bu koltuk bu gosterim icin zaten dolu.';
        END IF;
    END IF;

    SET NEW.ucret = v_fiyat;
END$$

-- ---------------------------------------------------------
-- 4) Rezervasyon güncellenirken:
--    - iptal edilmiş rezervasyon tekrar aktif yapılamaz
--    - koltuk değiştiriliyorsa yeni koltuk boş olmalı
-- ---------------------------------------------------------
CREATE TRIGGER trg_rezervasyon_guncelle
BEFORE UPDATE ON rezervasyon
FOR EACH ROW
BEGIN
    DECLARE v_kapasite INT;
    DECLARE v_dolu INT DEFAULT 0;

    IF OLD.odeme_durumu = 'Iptal' AND NEW.odeme_durumu <> 'Iptal' THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Iptal edilmis rezervasyon tekrar aktif edilemez.';
    END IF;

    IF NEW.koltuk_no <> OLD.koltuk_no OR NEW.gosterim_no <> OLD.gosterim_no THEN
        SELECT s.kapasite INTO v_kapasite
        FROM gosterim g
        JOIN salon s ON s.sinema_no = g.sinema_no AND s.salon_no = g.salon_no
        WHERE g.gosterim_no = NEW.gosterim_no;

        IF NEW.koltuk_no > v_kapasite THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Koltuk numarasi salon kapasitesinden buyuk olamaz.';
        END IF;

        SELECT COUNT(*) INTO v_dolu
        FROM rezervasyon
        WHERE gosterim_no = NEW.gosterim_no
          AND koltuk_no = NEW.koltuk_no
          AND odeme_durumu <> 'Iptal'
          AND rezervasyon_no <> NEW.rezervasyon_no;

        IF v_dolu > 0 THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Bu koltuk bu gosterim icin zaten dolu.';
        END IF;
    END IF;
END$$

-- ---------------------------------------------------------
-- 5) Ödeme durumu geçmişini log tablosuna yaz
-- ---------------------------------------------------------
CREATE TRIGGER trg_rezervasyon_log_ekle
AFTER INSERT ON rezervasyon
FOR EACH ROW
BEGIN
    INSERT INTO rezervasyon_log (rezervasyon_no, eski_durum, yeni_durum)
    VALUES (NEW.rezervasyon_no, NULL, NEW.odeme_durumu);
END$$

CREATE TRIGGER trg_rezervasyon_log_guncelle
AFTER UPDATE ON rezervasyon
FOR EACH ROW
BEGIN
    IF NEW.odeme_durumu <> OLD.odeme_durumu THEN
        INSERT INTO rezervasyon_log (rezervasyon_no, eski_durum, yeni_durum)
        VALUES (NEW.rezervasyon_no, OLD.odeme_durumu, NEW.odeme_durumu);
    END IF;
END$$

-- ---------------------------------------------------------
-- 6) Salon kapasitesi, ileri tarihli gösterimlerde satılmış
--    koltuk numaralarının altına düşürülemez
-- ---------------------------------------------------------
CREATE TRIGGER trg_salon_kapasite
BEFORE UPDATE ON salon
FOR EACH ROW
BEGIN
    DECLARE v_en_buyuk INT DEFAULT 0;

    IF NEW.kapasite < OLD.kapasite THEN
        SELECT IFNULL(MAX(r.koltuk_no), 0) INTO v_en_buyuk
        FROM rezervasyon r
        JOIN gosterim g ON g.gosterim_no = r.gosterim_no
        WHERE g.sinema_no = OLD.sinema_no
          AND g.salon_no = OLD.salon_no
          AND g.tarih >= CURDATE()
          AND r.odeme_durumu <> 'Iptal';

        IF NEW.kapasite < v_en_buyuk THEN
            SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT = 'Ileri tarihli gosterimlerde bu kapasitenin disinda satilmis koltuk var.';
        END IF;
    END IF;
END$$

DELIMITER ;


-- =========================================================
--  Viewlar
-- =========================================================
USE sinema_db;

-- ---------------------------------------------------------
-- Gösterim detayları + doluluk bilgisi
-- ---------------------------------------------------------
CREATE OR REPLACE VIEW v_gosterim_detay AS
SELECT
    g.gosterim_no,
    g.tarih,
    g.baslangic_saati,
    ADDTIME(g.baslangic_saati, SEC_TO_TIME(f.sure_dk * 60)) AS bitis_saati,
    f.film_no,
    f.ad            AS film_adi,
    f.tur,
    f.sure_dk,
    si.sinema_no,
    si.ad           AS sinema_adi,
    si.sehir,
    s.salon_no,
    s.ad            AS salon_adi,
    s.kapasite,
    g.bilet_fiyati,
    COUNT(r.rezervasyon_no)                       AS satilan_koltuk,
    s.kapasite - COUNT(r.rezervasyon_no)          AS bos_koltuk,
    ROUND(COUNT(r.rezervasyon_no) * 100 / s.kapasite, 1) AS doluluk_yuzde
FROM gosterim g
JOIN film f    ON f.film_no = g.film_no
JOIN salon s   ON s.sinema_no = g.sinema_no AND s.salon_no = g.salon_no
JOIN sinema si ON si.sinema_no = g.sinema_no
LEFT JOIN rezervasyon r
       ON r.gosterim_no = g.gosterim_no AND r.odeme_durumu <> 'Iptal'
GROUP BY g.gosterim_no, g.tarih, g.baslangic_saati, f.film_no, f.ad, f.tur, f.sure_dk,
         si.sinema_no, si.ad, si.sehir, s.salon_no, s.ad, s.kapasite, g.bilet_fiyati;

-- ---------------------------------------------------------
-- Rezervasyon detayları (müşteri + film + salon bilgisi tek yerde)
-- ---------------------------------------------------------
CREATE OR REPLACE VIEW v_rezervasyon_detay AS
SELECT
    r.rezervasyon_no,
    r.rezervasyon_tarihi,
    r.odeme_durumu,
    r.ucret,
    r.koltuk_no,
    m.musteri_no,
    CONCAT(m.ad, ' ', m.soyad) AS musteri,
    m.eposta,
    g.gosterim_no,
    g.tarih,
    g.baslangic_saati,
    f.ad  AS film_adi,
    si.ad AS sinema_adi,
    s.ad  AS salon_adi
FROM rezervasyon r
JOIN musteri m  ON m.musteri_no = r.musteri_no
JOIN gosterim g ON g.gosterim_no = r.gosterim_no
JOIN film f     ON f.film_no = g.film_no
JOIN salon s    ON s.sinema_no = g.sinema_no AND s.salon_no = g.salon_no
JOIN sinema si  ON si.sinema_no = g.sinema_no;

-- ---------------------------------------------------------
-- Film bazında satış / hasılat (sadece ödenmiş biletler hasılata girer)
-- ---------------------------------------------------------
CREATE OR REPLACE VIEW v_film_hasilat AS
SELECT
    f.film_no,
    f.ad AS film_adi,
    f.tur,
    COUNT(DISTINCT g.gosterim_no) AS gosterim_sayisi,
    SUM(CASE WHEN r.odeme_durumu = 'Odendi'    THEN 1 ELSE 0 END) AS odenen_bilet,
    SUM(CASE WHEN r.odeme_durumu = 'Beklemede' THEN 1 ELSE 0 END) AS bekleyen_bilet,
    IFNULL(SUM(CASE WHEN r.odeme_durumu = 'Odendi' THEN r.ucret END), 0) AS hasilat
FROM film f
LEFT JOIN gosterim g    ON g.film_no = f.film_no
LEFT JOIN rezervasyon r ON r.gosterim_no = g.gosterim_no
GROUP BY f.film_no, f.ad, f.tur;

-- ---------------------------------------------------------
-- Müşteri özeti (kaç bilet aldı, ne kadar harcadı)
-- ---------------------------------------------------------
CREATE OR REPLACE VIEW v_musteri_ozet AS
SELECT
    m.musteri_no,
    m.ad,
    m.soyad,
    m.eposta,
    (SELECT GROUP_CONCAT(t.telefon SEPARATOR ', ')
       FROM musteri_telefon t WHERE t.musteri_no = m.musteri_no) AS telefonlar,
    COUNT(r.rezervasyon_no) AS bilet_sayisi,
    IFNULL(SUM(r.ucret), 0) AS toplam_harcama,
    MAX(r.rezervasyon_tarihi) AS son_rezervasyon
FROM musteri m
LEFT JOIN rezervasyon r
       ON r.musteri_no = m.musteri_no AND r.odeme_durumu = 'Odendi'
GROUP BY m.musteri_no, m.ad, m.soyad, m.eposta;

-- ---------------------------------------------------------
-- Sinema bazında özet
-- ---------------------------------------------------------
CREATE OR REPLACE VIEW v_sinema_ozet AS
SELECT
    si.sinema_no,
    si.ad AS sinema_adi,
    si.sehir,
    (SELECT COUNT(*) FROM salon s WHERE s.sinema_no = si.sinema_no) AS salon_sayisi,
    (SELECT IFNULL(SUM(s.kapasite), 0) FROM salon s WHERE s.sinema_no = si.sinema_no) AS toplam_kapasite,
    (SELECT COUNT(*) FROM gosterim g WHERE g.sinema_no = si.sinema_no) AS gosterim_sayisi,
    (SELECT IFNULL(SUM(r.ucret), 0)
       FROM rezervasyon r JOIN gosterim g ON g.gosterim_no = r.gosterim_no
      WHERE g.sinema_no = si.sinema_no AND r.odeme_durumu = 'Odendi') AS hasilat
FROM sinema si;


-- =========================================================
--  Stored Procedure'ler
-- =========================================================
USE sinema_db;

DROP PROCEDURE IF EXISTS sp_rezervasyon_yap;
DROP PROCEDURE IF EXISTS sp_odeme_al;
DROP PROCEDURE IF EXISTS sp_rezervasyon_iptal;
DROP PROCEDURE IF EXISTS sp_tarih_araligi_rapor;

DELIMITER $$

-- ---------------------------------------------------------
-- Rezervasyon yap
-- Kontrollerin çoğu trigger'da. Burada transaction açıp gösterim
-- satırını kilitliyoruz ki aynı anda iki kişi aynı koltuğu alamasın.
-- ---------------------------------------------------------
CREATE PROCEDURE sp_rezervasyon_yap(
    IN  p_musteri_no  INT,
    IN  p_gosterim_no INT,
    IN  p_koltuk_no   INT,
    OUT p_rez_no      INT
)
BEGIN
    DECLARE v_var INT DEFAULT 0;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        RESIGNAL;
    END;

    START TRANSACTION;

    SELECT COUNT(*) INTO v_var
    FROM gosterim WHERE gosterim_no = p_gosterim_no
    FOR UPDATE;

    IF v_var = 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Gosterim bulunamadi.';
    END IF;

    INSERT INTO rezervasyon (musteri_no, gosterim_no, koltuk_no)
    VALUES (p_musteri_no, p_gosterim_no, p_koltuk_no);

    SET p_rez_no = LAST_INSERT_ID();

    COMMIT;
END$$

-- ---------------------------------------------------------
-- Ödeme al: sadece "Beklemede" olan rezervasyon ödenebilir
-- ---------------------------------------------------------
CREATE PROCEDURE sp_odeme_al(IN p_rez_no INT)
BEGIN
    DECLARE v_durum VARCHAR(20);

    SELECT odeme_durumu INTO v_durum
    FROM rezervasyon WHERE rezervasyon_no = p_rez_no;

    IF v_durum IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Rezervasyon bulunamadi.';
    ELSEIF v_durum <> 'Beklemede' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Sadece beklemedeki rezervasyonun odemesi alinabilir.';
    END IF;

    UPDATE rezervasyon SET odeme_durumu = 'Odendi'
    WHERE rezervasyon_no = p_rez_no;
END$$

-- ---------------------------------------------------------
-- İptal: başlamış gösterimin bileti iptal edilemez
-- ---------------------------------------------------------
CREATE PROCEDURE sp_rezervasyon_iptal(IN p_rez_no INT)
BEGIN
    DECLARE v_durum VARCHAR(20);
    DECLARE v_baslangic DATETIME;

    SELECT r.odeme_durumu, TIMESTAMP(g.tarih, g.baslangic_saati)
      INTO v_durum, v_baslangic
    FROM rezervasyon r
    JOIN gosterim g ON g.gosterim_no = r.gosterim_no
    WHERE r.rezervasyon_no = p_rez_no;

    IF v_durum IS NULL THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Rezervasyon bulunamadi.';
    ELSEIF v_durum = 'Iptal' THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Rezervasyon zaten iptal edilmis.';
    ELSEIF v_baslangic < NOW() THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Baslamis gosterimin bileti iptal edilemez.';
    END IF;

    UPDATE rezervasyon SET odeme_durumu = 'Iptal'
    WHERE rezervasyon_no = p_rez_no;
END$$

-- ---------------------------------------------------------
-- Tarih aralığına göre günlük satış raporu
-- ---------------------------------------------------------
CREATE PROCEDURE sp_tarih_araligi_rapor(IN p_baslangic DATE, IN p_bitis DATE)
BEGIN
    SELECT
        g.tarih,
        COUNT(DISTINCT g.gosterim_no) AS gosterim_sayisi,
        COUNT(r.rezervasyon_no)       AS satilan_bilet,
        IFNULL(SUM(CASE WHEN r.odeme_durumu = 'Odendi' THEN r.ucret END), 0) AS hasilat
    FROM gosterim g
    LEFT JOIN rezervasyon r
           ON r.gosterim_no = g.gosterim_no AND r.odeme_durumu <> 'Iptal'
    WHERE g.tarih BETWEEN p_baslangic AND p_bitis
    GROUP BY g.tarih
    ORDER BY g.tarih;
END$$

DELIMITER ;


-- =========================================================
--  Örnek veriler
--  Gösterim tarihleri CURDATE()'e göre verildi, script ne zaman
--  çalıştırılırsa çalıştırılsın gösterimler ileri tarihte olur.
-- =========================================================
USE sinema_db;

-- SINEMA
INSERT INTO sinema (ad, adres, sehir) VALUES
('Kadıköy Sinemaları',  'Caferağa Mah. Moda Cad. No:42 Kadıköy', 'İstanbul'),
('Atlas Park AVM',      'Kemalpaşa Mah. Atlas Cad. No:7 Bağcılar', 'İstanbul'),
('Kızılay Sinema',      'Atatürk Bulvarı No:110 Çankaya',        'Ankara');

INSERT INTO sinema_telefon (sinema_no, telefon) VALUES
(1, '0216 345 12 12'),
(1, '0216 345 12 13'),
(2, '0212 655 40 40'),
(3, '0312 418 77 01'),
(3, '0312 418 77 02'),
(3, '0532 410 22 33');

-- SALON
INSERT INTO salon (sinema_no, salon_no, ad, kapasite) VALUES
(1, 1, 'Salon 1',   120),
(1, 2, 'Salon 2',    80),
(1, 3, 'VIP Salon',  40),
(2, 1, 'IMAX',      150),
(2, 2, 'Salon 2',   100),
(2, 3, 'Salon 3',    60),
(3, 1, 'Salon 1',    90),
(3, 2, 'Salon 2',    70);

-- FILM
INSERT INTO film (ad, tur, sure_dk, vizyon_tarihi) VALUES
('Son Durak',               'Dram',         118, '2026-10-02'),
('Gece Yarısı Treni',       'Gerilim',      104, '2026-10-16'),
('Uzay Kampı',              'Animasyon',     92, '2026-10-30'),
('Kayıp Şehir',             'Macera',       135, '2026-11-06'),
('Annemin Bahçesi',         'Komedi',       101, '2026-11-06'),
('Kod Adı: Mavi',           'Aksiyon',      128, '2026-11-13'),
('Sessiz Kıyı',             'Korku',         97, '2026-11-13'),
('Yıldızlar Arası Mektup',  'Bilim Kurgu',  152, '2026-11-27');

-- GOSTERIM
-- seanslar: 11:00, 14:30, 18:00, 21:30
INSERT INTO gosterim (film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati) VALUES
(4, 1, 1, CURDATE() + INTERVAL 1 DAY, '14:30', 260),
(4, 1, 1, CURDATE() + INTERVAL 1 DAY, '18:00', 300),
(5, 1, 2, CURDATE() + INTERVAL 1 DAY, '14:30', 260),
(6, 1, 2, CURDATE() + INTERVAL 1 DAY, '21:30', 300),
(3, 1, 3, CURDATE() + INTERVAL 1 DAY, '11:00', 220),
(6, 2, 1, CURDATE() + INTERVAL 1 DAY, '18:00', 320),
(1, 2, 2, CURDATE() + INTERVAL 1 DAY, '14:30', 260),
(7, 2, 3, CURDATE() + INTERVAL 1 DAY, '21:30', 280),
(2, 3, 1, CURDATE() + INTERVAL 1 DAY, '18:00', 240),
(5, 3, 2, CURDATE() + INTERVAL 1 DAY, '14:30', 220),

(6, 1, 1, CURDATE() + INTERVAL 2 DAY, '18:00', 300),
(7, 1, 2, CURDATE() + INTERVAL 2 DAY, '21:30', 280),
(3, 1, 3, CURDATE() + INTERVAL 2 DAY, '11:00', 220),
(4, 2, 1, CURDATE() + INTERVAL 2 DAY, '14:30', 280),
(5, 2, 2, CURDATE() + INTERVAL 2 DAY, '18:00', 280),
(1, 3, 1, CURDATE() + INTERVAL 2 DAY, '18:00', 240),

(4, 1, 1, CURDATE() + INTERVAL 3 DAY, '21:30', 300),
(6, 2, 1, CURDATE() + INTERVAL 3 DAY, '21:30', 320),
(2, 2, 3, CURDATE() + INTERVAL 3 DAY, '18:00', 260),
(3, 3, 2, CURDATE() + INTERVAL 3 DAY, '11:00', 200),

(1, 1, 2, CURDATE() + INTERVAL 4 DAY, '14:30', 260),
(7, 2, 2, CURDATE() + INTERVAL 4 DAY, '21:30', 280),
(6, 3, 1, CURDATE() + INTERVAL 4 DAY, '18:00', 260),

(5, 1, 1, CURDATE() + INTERVAL 5 DAY, '14:30', 260),
(4, 2, 1, CURDATE() + INTERVAL 5 DAY, '18:00', 320),
(2, 3, 2, CURDATE() + INTERVAL 5 DAY, '21:30', 240),

(8, 1, 1, GREATEST(CURDATE() + INTERVAL 8 DAY, '2026-11-27'), '18:00', 320),
(8, 2, 1, GREATEST(CURDATE() + INTERVAL 8 DAY, '2026-11-27'), '21:30', 340),
(8, 1, 1, GREATEST(CURDATE() + INTERVAL 9 DAY, '2026-11-28'), '14:30', 300),
(8, 3, 1, GREATEST(CURDATE() + INTERVAL 9 DAY, '2026-11-28'), '18:00', 280);

-- MUSTERI
INSERT INTO musteri (ad, soyad, eposta) VALUES
('Ahmet',   'Yılmaz',   'ahmet.yilmaz@gmail.com'),
('Elif',    'Kaya',     'elifkaya@hotmail.com'),
('Mehmet',  'Demir',    'mehmet.demir@outlook.com'),
('Zeynep',  'Çelik',    'zeynepcelik@gmail.com'),
('Burak',   'Şahin',    'burak.sahin@yahoo.com'),
('Selin',   'Arslan',   'selin.arslan@gmail.com'),
('Emre',    'Koç',      'emrekoc@gmail.com'),
('Ayşe',    'Öztürk',   'ayse.ozturk@hotmail.com'),
('Can',     'Aydın',    'can.aydin@gmail.com'),
('Merve',   'Kurt',     'merve.kurt@outlook.com'),
('Oğuz',    'Polat',    'oguzpolat@gmail.com'),
('Deniz',   'Erdoğan',  'deniz.erdogan@gmail.com'),
('Gizem',   'Aksoy',    'gizemaksoy@hotmail.com'),
('Hakan',   'Yıldız',   'hakan.yildiz@gmail.com'),
('Ece',     'Turan',    'ece.turan@gmail.com');

INSERT INTO musteri_telefon (musteri_no, telefon) VALUES
(1, '0532 111 22 33'), (1, '0216 444 55 66'),
(2, '0541 222 33 44'),
(3, '0555 333 44 55'),
(4, '0533 444 55 66'), (4, '0507 123 45 67'),
(5, '0542 555 66 77'),
(6, '0536 666 77 88'),
(7, '0530 777 88 99'),
(8, '0545 888 99 00'), (8, '0212 333 22 11'),
(9, '0539 999 00 11'),
(10, '0531 101 20 30'),
(11, '0544 202 30 40'),
(12, '0538 303 40 50'),
(13, '0506 404 50 60'),
(14, '0543 505 60 70'), (14, '0312 222 11 00'),
(15, '0535 606 70 80');

-- REZERVASYON (ücreti trigger dolduruyor)
INSERT INTO rezervasyon (musteri_no, gosterim_no, koltuk_no) VALUES
(1, 1, 15), (2, 1, 16), (3, 1, 45), (4, 1, 46), (5, 1, 47), (6, 1, 60), (7, 1, 61),
(8, 2, 30), (9, 2, 31), (10, 2, 55), (11, 2, 56), (12, 2, 57), (1, 2, 88),
(2, 3, 12), (3, 3, 13), (13, 3, 40),
(4, 4, 22), (5, 4, 23), (14, 4, 24),
(6, 5, 5), (7, 5, 6), (15, 5, 18),
(8, 6, 70), (9, 6, 71), (10, 6, 72), (11, 6, 73), (12, 6, 100), (13, 6, 101),
(14, 7, 33), (15, 7, 34),
(1, 8, 20), (2, 8, 21),
(3, 9, 44), (4, 9, 45),
(5, 11, 50), (6, 11, 51), (7, 11, 52),
(8, 14, 10), (9, 14, 11),
(10, 17, 60), (11, 17, 61), (12, 17, 62),
(13, 18, 75), (14, 18, 76),
(15, 25, 40), (1, 25, 41),
(2, 27, 55), (3, 27, 56),
(4, 28, 80),
(5, 30, 30);

-- rezervasyon tarihlerini biraz geriye dağıt (hepsi aynı saniye görünmesin)
UPDATE rezervasyon
SET rezervasyon_tarihi = NOW() - INTERVAL (rezervasyon_no % 6) DAY - INTERVAL (rezervasyon_no * 37 % 600) MINUTE;

-- ödemelerin çoğu alınmış olsun
UPDATE rezervasyon SET odeme_durumu = 'Odendi' WHERE rezervasyon_no % 4 <> 0;

-- birkaç iptal
CALL sp_rezervasyon_iptal(8);
CALL sp_rezervasyon_iptal(36);


-- Web uygulamasının bağlanacağı kullanıcı (root yerine)
-- Sadece sinema_db üzerinde yetkisi var.
CREATE USER IF NOT EXISTS 'sinema_app'@'localhost' IDENTIFIED BY 'Sinema2026!';
GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON sinema_db.* TO 'sinema_app'@'localhost';
FLUSH PRIVILEGES;


