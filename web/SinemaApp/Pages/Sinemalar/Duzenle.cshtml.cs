using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

            var satir = db.Satir("SELECT * FROM SINEMA WHERE sinema_no = ?", id.Value);
            if (satir == null) return NotFound();

            Sinema.SinemaNo = Convert.ToInt32(satir["sinema_no"]);
            Sinema.Ad = satir["ad"].ToString();
            Sinema.Adres = satir["adres"].ToString();
            AltTablolariYukle();
            return Page();
        }

        private void AltTablolariYukle()
        {
            Telefonlar = db.Tablo("SELECT * FROM SINEMA_TELEFON WHERE sinema_no = ? ORDER BY telefon", Sinema.SinemaNo);

            Salonlar = db.Tablo(@"
                SELECT s.sinema_no, s.salon_no, s.ad, s.kapasite,
                       (SELECT COUNT(*) FROM GOSTERIM g WHERE g.sinema_no = s.sinema_no AND g.salon_no = s.salon_no) AS gosterim_sayisi
                FROM SALON AS s WHERE s.sinema_no = ? ORDER BY s.salon_no", Sinema.SinemaNo);
        }

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
                        "INSERT INTO SINEMA (ad, adres) VALUES (?, ?)",
                        Sinema.Ad.Trim(), Sinema.Adres.Trim());
                    TempData["Mesaj"] = "Sinema eklendi. Şimdi telefon ve salon ekleyebilirsiniz.";
                    return RedirectToPage(new { id = yeniNo });
                }

                db.Calistir(
                    "UPDATE SINEMA SET ad = ?, adres = ? WHERE sinema_no = ?",
                    Sinema.Ad.Trim(), Sinema.Adres.Trim(), Sinema.SinemaNo);
                TempData["Mesaj"] = "Sinema bilgileri güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id = Sinema.SinemaNo == 0 ? (int?)null : Sinema.SinemaNo });
        }

        public IActionResult OnPostTelefonEkle(int id, string telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon))
            {
                TempData["Hata"] = "Telefon numarası boş olamaz.";
                return RedirectToPage(new { id });
            }

            try
            {
                db.Calistir("INSERT INTO SINEMA_TELEFON (sinema_no, telefon) VALUES (?, ?)", id, telefon.Trim());
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostTelefonSil(int id, string telefon)
        {
            db.Calistir("DELETE FROM SINEMA_TELEFON WHERE sinema_no = ? AND telefon = ?", id, telefon);
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostSalonEkle(int id, string salonAd, int kapasite)
        {
            try
            {
                var max = db.Deger("SELECT MAX(salon_no) FROM SALON WHERE sinema_no = ?", id);
                int yeniSalonNo = (max == null ? 0 : Convert.ToInt32(max)) + 1;
                db.Calistir(
                    "INSERT INTO SALON (sinema_no, salon_no, ad, kapasite) VALUES (?, ?, ?, ?)",
                    id, yeniSalonNo, string.IsNullOrWhiteSpace(salonAd) ? $"Salon {yeniSalonNo}" : salonAd.Trim(), kapasite);
                TempData["Mesaj"] = "Salon eklendi.";
            }
            catch (Exception ex)
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
                    "UPDATE SALON SET ad = ?, kapasite = ? WHERE sinema_no = ? AND salon_no = ?",
                    salonAd?.Trim(), kapasite, id, salonNo);
                TempData["Mesaj"] = "Salon güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostSalonSil(int id, int salonNo)
        {
            try
            {
                db.Calistir("DELETE FROM SALON WHERE sinema_no = ? AND salon_no = ?", id, salonNo);
                TempData["Mesaj"] = "Salon silindi.";
            }
            catch (Exception ex)
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
        }
    }
}
