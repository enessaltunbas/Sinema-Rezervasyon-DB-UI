using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Gosterimler
{
    public class DuzenleModel : PageModel
    {
        private readonly Db db;

        public DuzenleModel(Db db)
        {
            this.db = db;
        }

        [BindProperty]
        public GosterimForm Gosterim { get; set; } = new GosterimForm();

        public DataTable Filmler { get; set; }
        public DataTable Salonlar { get; set; }
        public bool YeniKayit => Gosterim.GosterimNo == 0;

        private void ListeleriYukle()
        {
            Filmler = db.Tablo("SELECT film_no, ad, sure_dk, vizyon_tarihi FROM film ORDER BY ad");
            Salonlar = db.Tablo(
                @"SELECT CONCAT(s.sinema_no, '-', s.salon_no) AS deger,
                         si.ad AS sinema_adi, s.ad AS salon_adi, s.kapasite
                  FROM salon s JOIN sinema si ON si.sinema_no = s.sinema_no
                  ORDER BY si.ad, s.salon_no");
        }

        public IActionResult OnGet(int? id)
        {
            ListeleriYukle();

            if (id == null)
            {
                Gosterim.Tarih = DateTime.Today.AddDays(1);
                Gosterim.Saat = new TimeSpan(20, 0, 0);
                return Page();
            }

            var satir = db.Satir("SELECT * FROM gosterim WHERE gosterim_no = @id", Db.P("@id", id));
            if (satir == null) return NotFound();

            Gosterim.GosterimNo = Convert.ToInt32(satir["gosterim_no"]);
            Gosterim.FilmNo = Convert.ToInt32(satir["film_no"]);
            Gosterim.Salon = satir["sinema_no"] + "-" + satir["salon_no"];
            Gosterim.Tarih = Convert.ToDateTime(satir["tarih"]);
            Gosterim.Saat = (TimeSpan)satir["baslangic_saati"];
            Gosterim.Fiyat = Convert.ToDecimal(satir["bilet_fiyati"]);
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ListeleriYukle();
                return Page();
            }

            // "2-3" -> sinema 2, salon 3
            var parca = Gosterim.Salon.Split('-');
            int sinemaNo = int.Parse(parca[0]);
            int salonNo = int.Parse(parca[1]);

            try
            {
                if (YeniKayit)
                {
                    db.Calistir(
                        @"INSERT INTO gosterim (film_no, sinema_no, salon_no, tarih, baslangic_saati, bilet_fiyati)
                          VALUES (@film, @sinema, @salon, @tarih, @saat, @fiyat)",
                        Db.P("@film", Gosterim.FilmNo),
                        Db.P("@sinema", sinemaNo),
                        Db.P("@salon", salonNo),
                        Db.P("@tarih", Gosterim.Tarih.Date),
                        Db.P("@saat", Gosterim.Saat),
                        Db.P("@fiyat", Gosterim.Fiyat));
                    TempData["Mesaj"] = "Gösterim eklendi.";
                }
                else
                {
                    db.Calistir(
                        @"UPDATE gosterim SET film_no = @film, sinema_no = @sinema, salon_no = @salon,
                                 tarih = @tarih, baslangic_saati = @saat, bilet_fiyati = @fiyat
                          WHERE gosterim_no = @id",
                        Db.P("@film", Gosterim.FilmNo),
                        Db.P("@sinema", sinemaNo),
                        Db.P("@salon", salonNo),
                        Db.P("@tarih", Gosterim.Tarih.Date),
                        Db.P("@saat", Gosterim.Saat),
                        Db.P("@fiyat", Gosterim.Fiyat),
                        Db.P("@id", Gosterim.GosterimNo));
                    TempData["Mesaj"] = "Gösterim güncellendi.";
                }
                return RedirectToPage("Index");
            }
            catch (MySqlException ex)
            {
                // salon çakışması / vizyon tarihi kontrolleri trigger'dan gelir
                ModelState.AddModelError("", Db.HataMesaji(ex));
                ListeleriYukle();
                return Page();
            }
        }

        public class GosterimForm
        {
            public int GosterimNo { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Film seçiniz")]
            public int FilmNo { get; set; }

            [Required(ErrorMessage = "Salon seçiniz")]
            public string Salon { get; set; }

            [DataType(DataType.Date)]
            public DateTime Tarih { get; set; }

            [DataType(DataType.Time)]
            public TimeSpan Saat { get; set; }

            [Range(0, 10000, ErrorMessage = "Geçerli bir fiyat giriniz")]
            public decimal Fiyat { get; set; } = 250;
        }
    }
}
