using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
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

        // id yoksa yeni kayıt, varsa düzenleme
        public IActionResult OnGet(int? id)
        {
            if (id == null)
            {
                Film.VizyonTarihi = DateTime.Today;
                return Page();
            }

            var satir = db.Satir("SELECT * FROM film WHERE film_no = @id", Db.P("@id", id));
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
                        @"INSERT INTO film (ad, tur, sure_dk, vizyon_tarihi)
                          VALUES (@ad, @tur, @sure, @vizyon)",
                        Db.P("@ad", Film.Ad.Trim()),
                        Db.P("@tur", Film.Tur.Trim()),
                        Db.P("@sure", Film.SureDk),
                        Db.P("@vizyon", Film.VizyonTarihi));
                    TempData["Mesaj"] = "Film eklendi.";
                }
                else
                {
                    db.Calistir(
                        @"UPDATE film SET ad = @ad, tur = @tur, sure_dk = @sure, vizyon_tarihi = @vizyon
                          WHERE film_no = @id",
                        Db.P("@ad", Film.Ad.Trim()),
                        Db.P("@tur", Film.Tur.Trim()),
                        Db.P("@sure", Film.SureDk),
                        Db.P("@vizyon", Film.VizyonTarihi),
                        Db.P("@id", Film.FilmNo));
                    TempData["Mesaj"] = "Film güncellendi.";
                }
                return RedirectToPage("Index");
            }
            catch (MySqlException ex)
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
