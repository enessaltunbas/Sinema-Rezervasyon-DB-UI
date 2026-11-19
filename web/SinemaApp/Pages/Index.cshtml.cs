using System;
using System.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinemaApp.Data;

namespace SinemaApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public long FilmSayisi { get; set; }
        public long BugunkuGosterim { get; set; }
        public long AktifRezervasyon { get; set; }
        public decimal ToplamHasilat { get; set; }
        public DataTable YaklasanGosterimler { get; set; }
        public DataTable SonRezervasyonlar { get; set; }

        public void OnGet()
        {
            FilmSayisi = Convert.ToInt64(db.Deger(
                "SELECT COUNT(*) FROM film WHERE vizyon_tarihi <= CURDATE()"));

            BugunkuGosterim = Convert.ToInt64(db.Deger(
                "SELECT COUNT(*) FROM gosterim WHERE tarih BETWEEN CURDATE() AND CURDATE() + INTERVAL 7 DAY"));

            AktifRezervasyon = Convert.ToInt64(db.Deger(
                @"SELECT COUNT(*) FROM rezervasyon r
                  JOIN gosterim g ON g.gosterim_no = r.gosterim_no
                  WHERE r.odeme_durumu <> 'Iptal' AND g.tarih >= CURDATE()"));

            ToplamHasilat = Convert.ToDecimal(db.Deger(
                "SELECT IFNULL(SUM(hasilat), 0) FROM v_film_hasilat"));

            YaklasanGosterimler = db.Tablo(
                @"SELECT * FROM v_gosterim_detay
                  WHERE TIMESTAMP(tarih, baslangic_saati) >= NOW()
                  ORDER BY tarih, baslangic_saati
                  LIMIT 8");

            SonRezervasyonlar = db.Tablo(
                @"SELECT * FROM v_rezervasyon_detay
                  ORDER BY rezervasyon_tarihi DESC
                  LIMIT 6");
        }
    }
}
