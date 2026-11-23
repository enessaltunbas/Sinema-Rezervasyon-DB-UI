using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
using SinemaApp.Data;

namespace SinemaApp.Pages.Rezervasyonlar
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public DataTable Rezervasyonlar { get; set; }

        [BindProperty(SupportsGet = true)] public string Durum { get; set; }
        [BindProperty(SupportsGet = true)] public string Ara { get; set; }
        [BindProperty(SupportsGet = true)] public int? Vurgula { get; set; }

        public void OnGet()
        {
            Rezervasyonlar = db.Tablo(
                @"SELECT * FROM v_rezervasyon_detay
                  WHERE (@durum IS NULL OR odeme_durumu = @durum)
                    AND (@ara IS NULL
                         OR musteri LIKE CONCAT('%', @ara, '%')
                         OR film_adi LIKE CONCAT('%', @ara, '%')
                         OR rezervasyon_no = @ara)
                  ORDER BY rezervasyon_no DESC",
                Db.P("@durum", string.IsNullOrEmpty(Durum) ? null : Durum),
                Db.P("@ara", string.IsNullOrWhiteSpace(Ara) ? null : Ara.Trim()));
        }

        // Ödeme ve iptal işlemleri stored procedure ile yapılıyor,
        // durum değişikliği trigger ile rezervasyon_log tablosuna yazılıyor.
        public IActionResult OnPostOdeme(int id)
        {
            try
            {
                db.Calistir("CALL sp_odeme_al(@id)", Db.P("@id", id));
                TempData["Mesaj"] = $"{id} numaralı rezervasyonun ödemesi alındı.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { Durum, Ara });
        }

        public IActionResult OnPostIptal(int id)
        {
            try
            {
                db.Calistir("CALL sp_rezervasyon_iptal(@id)", Db.P("@id", id));
                TempData["Mesaj"] = $"{id} numaralı rezervasyon iptal edildi.";
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { Durum, Ara });
        }
    }
}
