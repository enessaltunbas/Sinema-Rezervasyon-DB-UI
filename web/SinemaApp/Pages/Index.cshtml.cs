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
        public long GosterimSayisi { get; set; }
        public long AktifRezervasyon { get; set; }
        public decimal ToplamHasilat { get; set; }
        public DataTable YaklasanGosterimler { get; set; }
        public DataTable SonRezervasyonlar { get; set; }

        public void OnGet()
        {
            FilmSayisi = Convert.ToInt64(db.Deger("SELECT COUNT(*) FROM FILM") ?? 0);
            GosterimSayisi = Convert.ToInt64(db.Deger("SELECT COUNT(*) FROM GOSTERIM") ?? 0);

            AktifRezervasyon = Convert.ToInt64(db.Deger(
                "SELECT COUNT(*) FROM REZERVASYON WHERE odeme_durumu <> 'Iptal'") ?? 0);

            var hasilat = db.Deger(@"
                SELECT SUM(g.bilet_fiyati)
                FROM GOSTERIM AS g INNER JOIN REZERVASYON AS r ON g.gosterim_no = r.gosterim_no
                WHERE r.odeme_durumu = 'Odendi'");
            ToplamHasilat = hasilat == null ? 0 : Convert.ToDecimal(hasilat, System.Globalization.CultureInfo.InvariantCulture);

            YaklasanGosterimler = db.Tablo(
                $"SELECT TOP 8 * FROM ({Db.GosterimDetaySql}) AS q ORDER BY tarih, baslangic_saati");

            SonRezervasyonlar = db.Tablo(
                $"SELECT TOP 6 * FROM ({Db.RezervasyonDetaySql}) AS q ORDER BY rezervasyon_tarihi DESC");
        }
    }
}
