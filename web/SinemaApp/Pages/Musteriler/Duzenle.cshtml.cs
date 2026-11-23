using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinemaApp.Data;

namespace SinemaApp.Pages.Musteriler
{
    public class DuzenleModel : PageModel
    {
        private readonly Db db;

        public DuzenleModel(Db db)
        {
            this.db = db;
        }

        [BindProperty]
        public MusteriForm Musteri { get; set; } = new MusteriForm();

        public DataTable Telefonlar { get; set; }
        public DataTable Rezervasyonlar { get; set; }

        public bool YeniKayit => Musteri.MusteriNo == 0;

        public IActionResult OnGet(int? id)
        {
            if (id == null) return Page();

            var satir = db.Satir("SELECT * FROM MUSTERI WHERE musteri_no = ?", id.Value);
            if (satir == null) return NotFound();

            Musteri.MusteriNo = Convert.ToInt32(satir["musteri_no"]);
            Musteri.Ad = satir["ad"].ToString();
            Musteri.Soyad = satir["soyad"].ToString();
            Musteri.Eposta = satir["eposta"].ToString();
            AltTablolariYukle();
            return Page();
        }

        private void AltTablolariYukle()
        {
            Telefonlar = db.Tablo("SELECT * FROM MUSTERI_TELEFON WHERE musteri_no = ? ORDER BY telefon", Musteri.MusteriNo);

            Rezervasyonlar = db.Tablo(
                $"SELECT * FROM ({Db.RezervasyonDetaySql}) AS q WHERE musteri_no = ? ORDER BY tarih DESC, baslangic_saati DESC",
                Musteri.MusteriNo);
        }

        public IActionResult OnPostKaydet(string ilkTelefon)
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
                        "INSERT INTO MUSTERI (ad, soyad, eposta) VALUES (?, ?, ?)",
                        Musteri.Ad.Trim(), Musteri.Soyad.Trim(), Musteri.Eposta.Trim().ToLower());

                    if (!string.IsNullOrWhiteSpace(ilkTelefon))
                    {
                        db.Calistir("INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (?, ?)",
                            (int)yeniNo, ilkTelefon.Trim());
                    }

                    TempData["Mesaj"] = "Müşteri kaydedildi.";
                    return RedirectToPage(new { id = yeniNo });
                }

                db.Calistir(
                    "UPDATE MUSTERI SET ad = ?, soyad = ?, eposta = ? WHERE musteri_no = ?",
                    Musteri.Ad.Trim(), Musteri.Soyad.Trim(), Musteri.Eposta.Trim().ToLower(), Musteri.MusteriNo);
                TempData["Mesaj"] = "Müşteri güncellendi.";
                return RedirectToPage(new { id = Musteri.MusteriNo });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", Db.HataMesaji(ex));
                if (!YeniKayit) AltTablolariYukle();
                return Page();
            }
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
                db.Calistir("INSERT INTO MUSTERI_TELEFON (musteri_no, telefon) VALUES (?, ?)", id, telefon.Trim());
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostTelefonSil(int id, string telefon)
        {
            db.Calistir("DELETE FROM MUSTERI_TELEFON WHERE musteri_no = ? AND telefon = ?", id, telefon);
            return RedirectToPage(new { id });
        }

        public class MusteriForm
        {
            public int MusteriNo { get; set; }

            [Required(ErrorMessage = "Ad boş olamaz")]
            [StringLength(50)]
            public string Ad { get; set; }

            [Required(ErrorMessage = "Soyad boş olamaz")]
            [StringLength(50)]
            public string Soyad { get; set; }

            [Required(ErrorMessage = "E-posta boş olamaz")]
            [EmailAddress(ErrorMessage = "Geçerli bir e-posta giriniz")]
            [StringLength(100)]
            public string Eposta { get; set; }
        }
    }
}
