using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinemaApp.Data;

namespace SinemaApp.Pages.Filmler
{
    public class DuzenleModel : PageModel
    {
        private readonly Db db;

        public DuzenleModel(Db db)
        {
            this.db = db;
        }

        [BindProperty]
        public FilmForm Film { get; set; } = new FilmForm();

        public bool YeniKayit => Film.FilmNo == 0;

        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                Film.VizyonTarihi = DateTime.Today;
                return Page();
            }

            var satir = db.Satir("SELECT * FROM FILM WHERE film_no = ?", id.Value);
            if (satir == null) return NotFound();

            Film.FilmNo = Convert.ToInt32(satir["film_no"]);
            Film.Ad = satir["ad"].ToString();
            Film.Tur = satir["tur"].ToString();
            Film.SureDk = Convert.ToInt32(satir["sure_dk"]);
            Film.VizyonTarihi = Convert.ToDateTime(satir["vizyon_tarihi"]);
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                if (YeniKayit)
                {
                    db.Calistir(
                        "INSERT INTO FILM (ad, tur, sure_dk, vizyon_tarihi) VALUES (?, ?, ?, ?)",
                        Film.Ad.Trim(), Film.Tur.Trim(), Film.SureDk, Film.VizyonTarihi.Date);
                    TempData["Mesaj"] = "Film eklendi.";
                }
                else
                {
                    db.Calistir(
                        "UPDATE FILM SET ad = ?, tur = ?, sure_dk = ?, vizyon_tarihi = ? WHERE film_no = ?",
                        Film.Ad.Trim(), Film.Tur.Trim(), Film.SureDk, Film.VizyonTarihi.Date, Film.FilmNo);
                    TempData["Mesaj"] = "Film güncellendi.";
                }
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", Db.HataMesaji(ex));
                return Page();
            }
        }

        public class FilmForm
        {
            public int FilmNo { get; set; }

            [Required(ErrorMessage = "Film adı boş olamaz")]
            [StringLength(150)]
            public string Ad { get; set; }

            [Required(ErrorMessage = "Tür seçiniz")]
            public string Tur { get; set; }

            [Range(1, 400, ErrorMessage = "Süre 1-400 dakika arasında olmalı")]
            public int SureDk { get; set; }

            [DataType(DataType.Date)]
            public DateTime VizyonTarihi { get; set; }
        }
    }
}
