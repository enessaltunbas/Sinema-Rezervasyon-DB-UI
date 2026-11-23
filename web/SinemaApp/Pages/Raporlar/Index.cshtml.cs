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

        [BindProperty(SupportsGet = true)] public DateTime? Baslangic { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? Bitis { get; set; }

        public void OnGet()
        {
            if (Baslangic == null) Baslangic = new DateTime(2026, 12, 1);
            if (Bitis == null) Bitis = new DateTime(2026, 12, 15);

            FilmHasilat = db.Tablo(@"
                SELECT f.film_no, f.ad AS film_adi, f.tur,
                       (SELECT COUNT(*) FROM GOSTERIM g WHERE g.film_no = f.film_no) AS gosterim_sayisi,
                       (SELECT COUNT(*) FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.film_no = f.film_no AND r.odeme_durumu = 'Odendi') AS odenen_bilet,
                       (SELECT COUNT(*) FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.film_no = f.film_no AND r.odeme_durumu = 'Beklemede') AS bekleyen_bilet,
                       (SELECT IIf(IsNull(SUM(g.bilet_fiyati)), 0, SUM(g.bilet_fiyati))
                        FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.film_no = f.film_no AND r.odeme_durumu = 'Odendi') AS hasilat
                FROM FILM AS f
                ORDER BY 7 DESC");

            SinemaOzet = db.Tablo(@"
                SELECT si.sinema_no, si.ad AS sinema_adi,
                       (SELECT COUNT(*) FROM SALON s WHERE s.sinema_no = si.sinema_no) AS salon_sayisi,
                       (SELECT IIf(IsNull(SUM(s.kapasite)), 0, SUM(s.kapasite)) FROM SALON s WHERE s.sinema_no = si.sinema_no) AS toplam_kapasite,
                       (SELECT IIf(IsNull(SUM(g.bilet_fiyati)), 0, SUM(g.bilet_fiyati))
                        FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.sinema_no = si.sinema_no AND r.odeme_durumu = 'Odendi') AS hasilat
                FROM SINEMA AS si
                ORDER BY 5 DESC");

            EnIyiMusteriler = db.Tablo(@"
                SELECT TOP 5 m.ad, m.soyad, COUNT(r.rezervasyon_no) AS bilet_sayisi,
                       SUM(g.bilet_fiyati) AS toplam_harcama
                FROM (MUSTERI AS m
                      INNER JOIN REZERVASYON AS r ON m.musteri_no = r.musteri_no)
                      INNER JOIN GOSTERIM AS g ON r.gosterim_no = g.gosterim_no
                WHERE r.odeme_durumu = 'Odendi'
                GROUP BY m.musteri_no, m.ad, m.soyad
                ORDER BY SUM(g.bilet_fiyati) DESC");

            GunlukRapor = db.Tablo(@"
                SELECT g.tarih,
                       COUNT(g.gosterim_no) AS gosterim_sayisi,
                       (SELECT COUNT(*) FROM GOSTERIM g2 INNER JOIN REZERVASYON r2 ON g2.gosterim_no = r2.gosterim_no
                        WHERE g2.tarih = g.tarih AND r2.odeme_durumu <> 'Iptal') AS satilan_bilet,
                       (SELECT IIf(IsNull(SUM(g3.bilet_fiyati)), 0, SUM(g3.bilet_fiyati))
                        FROM GOSTERIM g3 INNER JOIN REZERVASYON r3 ON g3.gosterim_no = r3.gosterim_no
                        WHERE g3.tarih = g.tarih AND r3.odeme_durumu = 'Odendi') AS hasilat
                FROM GOSTERIM AS g
                WHERE g.tarih BETWEEN ? AND ?
                GROUP BY g.tarih
                ORDER BY g.tarih",
                Baslangic.Value.Date, Bitis.Value.Date);
        }
    }
}
