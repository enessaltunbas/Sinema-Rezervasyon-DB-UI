using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Filmler
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public DataTable Filmler { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Ara { get; set; }

        public void OnGet()
        {
            Filmler = db.Tablo(
                @"SELECT f.*, h.gosterim_sayisi, h.odenen_bilet, h.hasilat
                  FROM film f
                  JOIN v_film_hasilat h ON h.film_no = f.film_no
                  WHERE (@ara IS NULL OR f.ad LIKE CONCAT('%', @ara, '%') OR f.tur LIKE CONCAT('%', @ara, '%'))
                  ORDER BY f.vizyon_tarihi DESC",
                Db.P("@ara", string.IsNullOrWhiteSpace(Ara) ? null : Ara.Trim()));
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM film WHERE film_no = @id", Db.P("@id", id));
                TempData["Mesaj"] = "Film silindi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
