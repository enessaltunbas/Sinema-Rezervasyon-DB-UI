-- Sinema Rezervasyon Sistemi - MS Access Tablo Olusturma (CREATE TABLE)

CREATE TABLE SINEMA (
    sinema_no   COUNTER      NOT NULL,
    ad          TEXT(100)    NOT NULL,
    adres       TEXT(255)    NOT NULL,
    CONSTRAINT PK_SINEMA PRIMARY KEY (sinema_no),
    CONSTRAINT UQ_SINEMA_AD UNIQUE (ad)
);

CREATE TABLE SINEMA_TELEFON (
    sinema_no   LONG         NOT NULL,
    telefon     TEXT(20)     NOT NULL,
    CONSTRAINT PK_SINEMA_TELEFON PRIMARY KEY (sinema_no, telefon),
    CONSTRAINT FK_STEL_SINEMA FOREIGN KEY (sinema_no) REFERENCES SINEMA (sinema_no)
);

CREATE TABLE SALON (
    sinema_no   LONG         NOT NULL,
    salon_no    LONG         NOT NULL,
    ad          TEXT(50)     NOT NULL,
    kapasite    LONG         NOT NULL,
    CONSTRAINT PK_SALON PRIMARY KEY (sinema_no, salon_no),
    CONSTRAINT FK_SALON_SINEMA FOREIGN KEY (sinema_no) REFERENCES SINEMA (sinema_no)
);

CREATE TABLE FILM (
    film_no        COUNTER     NOT NULL,
    ad             TEXT(150)   NOT NULL,
    tur            TEXT(50)    NOT NULL,
    sure_dk        LONG        NOT NULL,
    vizyon_tarihi  DATETIME    NOT NULL,
    CONSTRAINT PK_FILM PRIMARY KEY (film_no)
);

CREATE TABLE GOSTERIM (
    gosterim_no      COUNTER     NOT NULL,
    film_no          LONG        NOT NULL,
    sinema_no        LONG        NOT NULL,
    salon_no         LONG        NOT NULL,
    tarih            DATETIME    NOT NULL,
    baslangic_saati  DATETIME    NOT NULL,
    bilet_fiyati     CURRENCY    NOT NULL,
    CONSTRAINT PK_GOSTERIM PRIMARY KEY (gosterim_no),
    CONSTRAINT UQ_GOSTERIM_SEANS UNIQUE (sinema_no, salon_no, tarih, baslangic_saati),
    CONSTRAINT FK_GOS_FILM FOREIGN KEY (film_no) REFERENCES FILM (film_no),
    CONSTRAINT FK_GOS_SALON FOREIGN KEY (sinema_no, salon_no) REFERENCES SALON (sinema_no, salon_no)
);

CREATE TABLE MUSTERI (
    musteri_no    COUNTER      NOT NULL,
    ad            TEXT(50)     NOT NULL,
    soyad         TEXT(50)     NOT NULL,
    eposta        TEXT(100)    NOT NULL,
    CONSTRAINT PK_MUSTERI PRIMARY KEY (musteri_no),
    CONSTRAINT UQ_MUSTERI_EPOSTA UNIQUE (eposta)
);

CREATE TABLE MUSTERI_TELEFON (
    musteri_no  LONG         NOT NULL,
    telefon     TEXT(20)     NOT NULL,
    CONSTRAINT PK_MUSTERI_TELEFON PRIMARY KEY (musteri_no, telefon),
    CONSTRAINT FK_MTEL_MUSTERI FOREIGN KEY (musteri_no) REFERENCES MUSTERI (musteri_no)
);

CREATE TABLE REZERVASYON (
    rezervasyon_no      COUNTER     NOT NULL,
    musteri_no          LONG        NOT NULL,
    gosterim_no         LONG        NOT NULL,
    rezervasyon_tarihi  DATETIME    NOT NULL,
    koltuk_no           LONG        NOT NULL,
    odeme_durumu        TEXT(10)    NOT NULL,
    CONSTRAINT PK_REZERVASYON PRIMARY KEY (rezervasyon_no),
    CONSTRAINT FK_REZ_MUSTERI FOREIGN KEY (musteri_no) REFERENCES MUSTERI (musteri_no),
    CONSTRAINT FK_REZ_GOSTERIM FOREIGN KEY (gosterim_no) REFERENCES GOSTERIM (gosterim_no)
);
