# Sinema Rezervasyon Sistemi

Veri Tabanı Yönetim Sistemleri dersi projesi.

> **Not:** Projenin ilk (MySQL) sürümü `v1/` klasöründe, güncel MS Access (`.accdb`) sürümü ve proje raporu ana dizindedir.

## Senaryo

Sinemaların benzersiz bir numarası, adı, adresi ve birden fazla telefon numarası vardır. Her sinemada birden fazla salon bulunur ve her salonun benzersiz bir numarası, adı ve oturma kapasitesi vardır. Filmlerin benzersiz bir numarası, adı, türü, süresi ve gösterime giriş tarihi bulunur. Filmler sinema salonlarında gösterilir ve her gösterim için tarih, başlangıç saati ve bilet fiyatı kaydedilir. Müşterilerin benzersiz bir kimlik numarası, adı, soyadı, e-posta adresi ve birden fazla telefon numarası vardır. Müşteriler gösterimler için rezervasyon yapar. Her rezervasyonun benzersiz bir numarası, rezervasyon tarihi, koltuk numarası ve ödeme durumu bulunur.

## İlişkisel Şema (3NF)

- **SINEMA** (<u>sinema_no</u>, ad, adres)
- **SINEMA_TELEFON** (<u>sinema_no</u>, <u>telefon</u>)
- **SALON** (<u>sinema_no</u>, <u>salon_no</u>, ad, kapasite)
- **FILM** (<u>film_no</u>, ad, tur, sure_dk, vizyon_tarihi)
- **GOSTERIM** (<u>gosterim_no</u>, film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati)
- **MUSTERI** (<u>musteri_no</u>, ad, soyad, eposta)
- **MUSTERI_TELEFON** (<u>musteri_no</u>, <u>telefon</u>)
- **REZERVASYON** (<u>rezervasyon_no</u>, musteri_no, gosterim_no, rezervasyon_tarihi, koltuk_no, odeme_durumu)

## Klasör Yapısı

```
Proje_Raporu_Sinema_Rezervasyon.docx   proje raporu (Sorular 1-6)
veritabani/
    SinemaRezervasyon.accdb            MS Access veritabanı dosyası
    01_create_tables.sql               CREATE TABLE komutları
    02_insert_data.sql                 INSERT INTO komutları
    03_select.sql                      SELECT sorguları
    04_update.sql                      UPDATE sorguları
    05_delete.sql                      DELETE sorguları
dokumanlar/
    er_diyagrami.drawio
    er_diyagrami.svg / .png
    iliskisel_sema.svg / .png
    iliskisel_sema.md
web/SinemaApp/                         C# ASP.NET Core (.NET 5.0) web arayüzü
v1/                                    projenin ilk (MySQL) sürümü
```

## Web Arayüzünü Çalıştırma

Visual Studio ile `web/SinemaApp/SinemaApp.sln` dosyasını açıp çalıştırabilir veya komut satırından:

```
cd web/SinemaApp
dotnet run
```

komutuyla başlatabilirsiniz. Uygulama `veritabani/SinemaRezervasyon.accdb` dosyasına `System.Data.OleDb` (`Microsoft.ACE.OLEDB.12.0`) ile bağlanır.
