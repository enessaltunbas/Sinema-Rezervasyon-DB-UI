using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Sinemalar
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public DataTable Sinemalar { get; set; }

        public void OnGet()
        {
            Sinemalar = db.Tablo(
                @"SELECT o.*, s.adres,
                         (SELECT GROUP_CONCAT(telefon SEPARATOR ', ')
                            FROM sinema_telefon t WHERE t.sinema_no = s.sinema_no) AS telefonlar
                  FROM v_sinema_ozet o
                  JOIN sinema s ON s.sinema_no = o.sinema_no
                  ORDER BY o.sehir, o.sinema_adi");
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                // salonlar ve telefonlar CASCADE ile silinir,
                // gösterimi olan salon varsa FK (RESTRICT) izin vermez
                db.Calistir("DELETE FROM sinema WHERE sinema_no = @id", Db.P("@id", id));
                TempData["Mesaj"] = "Sinema silindi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
