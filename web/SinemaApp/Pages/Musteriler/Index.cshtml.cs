using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            Musteriler = db.Tablo(@"
                SELECT m.musteri_no, m.ad, m.soyad, m.eposta,
                       (SELECT COUNT(*) FROM REZERVASYON r WHERE r.musteri_no = m.musteri_no AND r.odeme_durumu = 'Odendi') AS bilet_sayisi,
                       (SELECT IIf(IsNull(SUM(g.bilet_fiyati)), 0, SUM(g.bilet_fiyati))
                        FROM REZERVASYON r INNER JOIN GOSTERIM g ON r.gosterim_no = g.gosterim_no
                        WHERE r.musteri_no = m.musteri_no AND r.odeme_durumu = 'Odendi') AS toplam_harcama
                FROM MUSTERI AS m
                ORDER BY m.ad, m.soyad");

            var teller = db.CokDegerliHarita("MUSTERI_TELEFON", "musteri_no", "telefon");
            Musteriler.Columns.Add("telefonlar", typeof(string));
            for (int i = Musteriler.Rows.Count - 1; i >= 0; i--)
            {
                var r = Musteriler.Rows[i];
                int mno = Convert.ToInt32(r["musteri_no"]);
                r["telefonlar"] = teller.TryGetValue(mno, out var t) ? t : "-";
                if (!string.IsNullOrWhiteSpace(Ara))
                {
                    var a = Ara.Trim();
                    var adSoyad = $"{r["ad"]} {r["soyad"]}";
                    if (!adSoyad.Contains(a, StringComparison.OrdinalIgnoreCase) &&
                        !r["eposta"].ToString().Contains(a, StringComparison.OrdinalIgnoreCase) &&
                        !r["telefonlar"].ToString().Contains(a, StringComparison.OrdinalIgnoreCase))
                        Musteriler.Rows.RemoveAt(i);
                }
            }
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM MUSTERI_TELEFON WHERE musteri_no = ?", id);
                db.Calistir("DELETE FROM MUSTERI WHERE musteri_no = ?", id);
                TempData["Mesaj"] = "Müşteri silindi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
