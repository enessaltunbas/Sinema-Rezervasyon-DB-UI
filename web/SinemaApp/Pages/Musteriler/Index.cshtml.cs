using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Musteriler
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public DataTable Musteriler { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Ara { get; set; }

        public void OnGet()
        {
            Musteriler = db.Tablo(
                @"SELECT * FROM v_musteri_ozet
                  WHERE @ara IS NULL
                     OR CONCAT(ad, ' ', soyad) LIKE CONCAT('%', @ara, '%')
                     OR eposta LIKE CONCAT('%', @ara, '%')
                     OR telefonlar LIKE CONCAT('%', @ara, '%')
                  ORDER BY ad, soyad",
                Db.P("@ara", string.IsNullOrWhiteSpace(Ara) ? null : Ara.Trim()));
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM musteri WHERE musteri_no = @id", Db.P("@id", id));
                TempData["Mesaj"] = "Müşteri silindi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
