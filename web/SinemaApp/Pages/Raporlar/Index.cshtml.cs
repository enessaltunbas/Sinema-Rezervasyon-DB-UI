using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinemaApp.Data;

namespace SinemaApp.Pages.Raporlar
{
    public class IndexModel : PageModel
    {
        private readonly Db db;

        public IndexModel(Db db)
        {
            this.db = db;
        }

        public DataTable FilmHasilat { get; set; }
        public DataTable SinemaOzet { get; set; }
        public DataTable EnIyiMusteriler { get; set; }
        public DataTable GunlukRapor { get; set; }
        public DataTable SonIslemler { get; set; }

        [BindProperty(SupportsGet = true)] public DateTime? Baslangic { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? Bitis { get; set; }

        public void OnGet()
        {
            if (Baslangic == null) Baslangic = DateTime.Today;
            if (Bitis == null) Bitis = DateTime.Today.AddDays(14);

            FilmHasilat = db.Tablo("SELECT * FROM v_film_hasilat ORDER BY hasilat DESC");
            SinemaOzet = db.Tablo("SELECT * FROM v_sinema_ozet ORDER BY hasilat DESC");
            EnIyiMusteriler = db.Tablo(
                "SELECT * FROM v_musteri_ozet WHERE bilet_sayisi > 0 ORDER BY toplam_harcama DESC LIMIT 5");

            GunlukRapor = db.Tablo("CALL sp_tarih_araligi_rapor(@bas, @bit)",
                Db.P("@bas", Baslangic.Value.Date),
                Db.P("@bit", Bitis.Value.Date));

            // trigger'ın doldurduğu log tablosu
            SonIslemler = db.Tablo(
                @"SELECT l.*, CONCAT(m.ad, ' ', m.soyad) AS musteri
                  FROM rezervasyon_log l
                  JOIN rezervasyon r ON r.rezervasyon_no = l.rezervasyon_no
                  JOIN musteri m ON m.musteri_no = r.musteri_no
                  ORDER BY l.log_no DESC
                  LIMIT 10");
        }
    }
}
