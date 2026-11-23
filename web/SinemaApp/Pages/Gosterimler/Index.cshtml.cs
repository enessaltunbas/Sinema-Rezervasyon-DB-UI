using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public void OnGet()
        {
            Sinemalar = db.Tablo("SELECT sinema_no, ad FROM SINEMA ORDER BY ad");
            Filmler = db.Tablo("SELECT film_no, ad FROM FILM ORDER BY ad");

            var kosullar = new List<string> { "1=1" };
            var prm = new List<object>();
            if (Tarih != null) { kosullar.Add("tarih = ?"); prm.Add(Tarih.Value.Date); }
            if (SinemaNo != null) { kosullar.Add("sinema_no = ?"); prm.Add(SinemaNo.Value); }
            if (FilmNo != null) { kosullar.Add("film_no = ?"); prm.Add(FilmNo.Value); }

            Gosterimler = db.Tablo(
                $"SELECT * FROM ({Db.GosterimDetaySql}) AS q WHERE {string.Join(" AND ", kosullar)} ORDER BY tarih, baslangic_saati, sinema_adi",
                prm.ToArray());

            Gosterimler.Columns.Add("doluluk_yuzde", typeof(double));
            foreach (DataRow r in Gosterimler.Rows)
            {
                int kap = Convert.ToInt32(r["kapasite"]);
                int sat = Convert.ToInt32(r["satilan_koltuk"]);
                r["doluluk_yuzde"] = kap > 0 ? Math.Round(sat * 100.0 / kap, 1) : 0.0;
            }
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM GOSTERIM WHERE gosterim_no = ?", id);
                TempData["Mesaj"] = "Gösterim silindi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
