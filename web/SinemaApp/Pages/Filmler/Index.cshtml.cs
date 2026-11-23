using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            Filmler = db.Tablo(@"
                SELECT f.film_no, f.ad, f.tur, f.sure_dk, f.vizyon_tarihi,
                       (SELECT COUNT(*) FROM GOSTERIM g WHERE g.film_no = f.film_no) AS gosterim_sayisi,
                       (SELECT COUNT(*) FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.film_no = f.film_no AND r.odeme_durumu = 'Odendi') AS odenen_bilet,
                       (SELECT IIf(IsNull(SUM(g.bilet_fiyati)), 0, SUM(g.bilet_fiyati))
                        FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.film_no = f.film_no AND r.odeme_durumu = 'Odendi') AS hasilat
                FROM FILM AS f
                ORDER BY f.vizyon_tarihi DESC");

            if (!string.IsNullOrWhiteSpace(Ara))
            {
                var a = Ara.Trim();
                for (int i = Filmler.Rows.Count - 1; i >= 0; i--)
                {
                    var r = Filmler.Rows[i];
                    if (!r["ad"].ToString().Contains(a, StringComparison.OrdinalIgnoreCase) &&
                        !r["tur"].ToString().Contains(a, StringComparison.OrdinalIgnoreCase))
                        Filmler.Rows.RemoveAt(i);
                }
            }
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM FILM WHERE film_no = ?", id);
                TempData["Mesaj"] = "Film silindi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
