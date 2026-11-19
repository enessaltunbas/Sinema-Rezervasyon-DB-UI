using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Sinemalar
{
    public class DuzenleModel : PageModel
    {
        private readonly Db db;

        public DuzenleModel(Db db)
        {
            this.db = db;
        }

        [BindProperty]
        public SinemaForm Sinema { get; set; } = new SinemaForm();

        public DataTable Telefonlar { get; set; }
        public DataTable Salonlar { get; set; }

        public bool YeniKayit => Sinema.SinemaNo == 0;

        public IActionResult OnGet(int? id)
        {
            if (id == null) return Page();

            var satir = db.Satir("SELECT * FROM sinema WHERE sinema_no = @id", Db.P("@id", id));
            if (satir == null) return NotFound();

            Sinema.SinemaNo = Convert.ToInt32(satir["sinema_no"]);
            Sinema.Ad = satir["ad"].ToString();
            Sinema.Adres = satir["adres"].ToString();
            Sinema.Sehir = satir["sehir"].ToString();
            AltTablolariYukle();
            return Page();
        }

        private void AltTablolariYukle()
        {
            Telefonlar = db.Tablo("SELECT * FROM sinema_telefon WHERE sinema_no = @id ORDER BY telefon",
                Db.P("@id", Sinema.SinemaNo));

            Salonlar = db.Tablo(
                @"SELECT s.*, (SELECT COUNT(*) FROM gosterim g
                               WHERE g.sinema_no = s.sinema_no AND g.salon_no = s.salon_no) AS gosterim_sayisi
                  FROM salon s WHERE s.sinema_no = @id ORDER BY s.salon_no",
                Db.P("@id", Sinema.SinemaNo));
        }

        // ---------- sinema bilgileri ----------
        public IActionResult OnPostKaydet()
        {
            if (!ModelState.IsValid)
            {
                if (!YeniKayit) AltTablolariYukle();
                return Page();
            }

            try
            {
                if (YeniKayit)
                {
                    long yeniNo = db.Ekle(
                        "INSERT INTO sinema (ad, adres, sehir) VALUES (@ad, @adres, @sehir)",
                        Db.P("@ad", Sinema.Ad.Trim()),
                        Db.P("@adres", Sinema.Adres.Trim()),
                        Db.P("@sehir", Sinema.Sehir.Trim()));
                    TempData["Mesaj"] = "Sinema eklendi. Şimdi telefon ve salon ekleyebilirsiniz.";
                    return RedirectToPage(new { id = yeniNo });
                }

                db.Calistir(
                    "UPDATE sinema SET ad = @ad, adres = @adres, sehir = @sehir WHERE sinema_no = @id",
                    Db.P("@ad", Sinema.Ad.Trim()),
                    Db.P("@adres", Sinema.Adres.Trim()),
                    Db.P("@sehir", Sinema.Sehir.Trim()),
                    Db.P("@id", Sinema.SinemaNo));
                TempData["Mesaj"] = "Sinema bilgileri güncellendi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id = Sinema.SinemaNo == 0 ? (int?)null : Sinema.SinemaNo });
        }

        // ---------- telefonlar ----------
        public IActionResult OnPostTelefonEkle(int id, string telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon))
            {
                TempData["Hata"] = "Telefon numarası boş olamaz.";
                return RedirectToPage(new { id });
            }

            try
            {
                db.Calistir("INSERT INTO sinema_telefon (sinema_no, telefon) VALUES (@id, @tel)",
                    Db.P("@id", id), Db.P("@tel", telefon.Trim()));
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostTelefonSil(int id, string telefon)
        {
            db.Calistir("DELETE FROM sinema_telefon WHERE sinema_no = @id AND telefon = @tel",
                Db.P("@id", id), Db.P("@tel", telefon));
            return RedirectToPage(new { id });
        }

        // ---------- salonlar ----------
        public IActionResult OnPostSalonEkle(int id, string salonAd, int kapasite)
        {
            try
            {
                // salon_no her sinemada 1'den başlayarak artıyor
                db.Calistir(
                    @"INSERT INTO salon (sinema_no, salon_no, ad, kapasite)
                      SELECT @id, IFNULL(MAX(salon_no), 0) + 1, @ad, @kap
                      FROM salon WHERE sinema_no = @id",
                    Db.P("@id", id),
                    Db.P("@ad", string.IsNullOrWhiteSpace(salonAd) ? "Salon" : salonAd.Trim()),
                    Db.P("@kap", kapasite));
                TempData["Mesaj"] = "Salon eklendi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostSalonGuncelle(int id, int salonNo, string salonAd, int kapasite)
        {
            try
            {
                db.Calistir(
                    "UPDATE salon SET ad = @ad, kapasite = @kap WHERE sinema_no = @id AND salon_no = @no",
                    Db.P("@ad", salonAd?.Trim()),
                    Db.P("@kap", kapasite),
                    Db.P("@id", id),
                    Db.P("@no", salonNo));
                TempData["Mesaj"] = "Salon güncellendi.";
            }
            catch (MySqlException ex)
            {
                // kapasite düşürülürken trg_salon_kapasite hata verebilir
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostSalonSil(int id, int salonNo)
        {
            try
            {
                db.Calistir("DELETE FROM salon WHERE sinema_no = @id AND salon_no = @no",
                    Db.P("@id", id), Db.P("@no", salonNo));
                TempData["Mesaj"] = "Salon silindi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public class SinemaForm
        {
            public int SinemaNo { get; set; }

            [Required(ErrorMessage = "Sinema adı boş olamaz")]
            [StringLength(100)]
            public string Ad { get; set; }

            [Required(ErrorMessage = "Adres boş olamaz")]
            [StringLength(255)]
            public string Adres { get; set; }

            [Required(ErrorMessage = "Şehir boş olamaz")]
            [StringLength(50)]
            public string Sehir { get; set; }
        }
    }
}
