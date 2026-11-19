using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Gosterimler
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public DataTable Gosterimler { get; set; }
        public DataTable Sinemalar { get; set; }
        public DataTable Filmler { get; set; }

        [BindProperty(SupportsGet = true)] public DateTime? Tarih { get; set; }
        [BindProperty(SupportsGet = true)] public int? SinemaNo { get; set; }
        [BindProperty(SupportsGet = true)] public int? FilmNo { get; set; }
        [BindProperty(SupportsGet = true)] public bool Gecmis { get; set; }

        public void OnGet()
        {
            Sinemalar = db.Tablo("SELECT sinema_no, ad FROM sinema ORDER BY ad");
            Filmler = db.Tablo("SELECT film_no, ad FROM film ORDER BY ad");

            Gosterimler = db.Tablo(
                @"SELECT * FROM v_gosterim_detay
                  WHERE (@tarih IS NULL OR tarih = @tarih)
                    AND (@sinema IS NULL OR sinema_no = @sinema)
                    AND (@film IS NULL OR film_no = @film)
                    AND (@gecmis = 1 OR TIMESTAMP(tarih, baslangic_saati) >= NOW())
                  ORDER BY tarih, baslangic_saati, sinema_adi",
                Db.P("@tarih", Tarih?.Date),
                Db.P("@sinema", SinemaNo),
                Db.P("@film", FilmNo),
                Db.P("@gecmis", Gecmis ? 1 : 0));
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM gosterim WHERE gosterim_no = @id", Db.P("@id", id));
                TempData["Mesaj"] = "Gösterim silindi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
