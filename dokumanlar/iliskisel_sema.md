# Sinema Rezervasyon Sistemi — 7 Adımlı Dönüşüm ve 3NF Normalizasyon Özeti

## 1. 7 Adımlı ER'den İlişkisel Modele Dönüşüm Algoritması (8 Tablo)

- **Adım 1 (Güçlü Varlıklar):**
  - **SINEMA**(**<u>sinema_no</u>**, ad, adres)
  - **FILM**(**<u>film_no</u>**, ad, tur, sure_dk, vizyon_tarihi)
  - **GOSTERIM**(**<u>gosterim_no</u>**, tarih, baslangic_saati, bilet_fiyati) — *türetilmiş `bitis_saati` ve `bos_koltuk` saklanmaz.*
  - **MUSTERI**(**<u>musteri_no</u>**, ad, soyad, eposta)
  - **REZERVASYON**(**<u>rezervasyon_no</u>**, rezervasyon_tarihi, koltuk_no, odeme_durumu)
- **Adım 2 (Zayıf Varlık):**
  - **SALON**(**<u>*sinema_no*</u>**, **<u>salon_no</u>**, ad, kapasite) — *sahibi `SINEMA`'nın PK'sı + kısmi anahtar `salon_no`.*
- **Adım 3 (1:1 İlişkiler):** Diyagramda 1:1 ilişki yoktur.
- **Adım 4 (1:N İlişkiler):**
  - `GOSTERIM` tablosuna `*film_no* → FILM(film_no)` ve `(*sinema_no*, *salon_no*) → SALON(sinema_no, salon_no)` yabancı anahtarları eklendi.
  - `REZERVASYON` tablosuna `*musteri_no* → MUSTERI(musteri_no)` ve `*gosterim_no* → GOSTERIM(gosterim_no)` yabancı anahtarları eklendi.
- **Adım 5 (M:N İlişkiler):** Film ile salon arasındaki gösterim ilişkisi, kendisine rezervasyon bağlandığı için `GOSTERIM` varlığı olarak modellenmiştir.
- **Adım 6 (Çok Değerli Nitelikler):**
  - **SINEMA_TELEFON**(**<u>*sinema_no*</u>**, **<u>telefon</u>**)
  - **MUSTERI_TELEFON**(**<u>*musteri_no*</u>**, **<u>telefon</u>**)
- **Adım 7 (n-li İlişkiler):** Derecesi > 2 olan ilişki yoktur.

## 2. 1NF → 2NF → 3NF Normalleştirme Özeti

- **1NF:** Tüm nitelikler atomiktir. Çok değerli telefon nitelikleri `SINEMA_TELEFON` ve `MUSTERI_TELEFON` tablolarına ayrılmıştır.
- **2NF:** Bileşik birincil anahtarlı `SALON(sinema_no, salon_no)` tablosunda `ad` ve `kapasite` anahtarın tamamına tam fonksiyonel bağımlıdır; kısmi bağımlılık yoktur.
- **3NF:** Hiçbir tabloda anahtar olmayan bir nitelik başka bir anahtar olmayan nitelik üzerinden geçişli bağımlılık (`PK → X → Y`) göstermez; türetilmiş nitelikler (`bitis_saati`, `bos_koltuk`) tablolarda sütun olarak tutulmaz, sorgularda hesaplanır.
