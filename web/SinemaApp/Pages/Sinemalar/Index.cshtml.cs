using System;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            Sinemalar = db.Tablo(@"
                SELECT si.sinema_no, si.ad AS sinema_adi, si.adres,
                       (SELECT COUNT(*) FROM SALON s WHERE s.sinema_no = si.sinema_no) AS salon_sayisi,
                       (SELECT IIf(IsNull(SUM(s.kapasite)), 0, SUM(s.kapasite)) FROM SALON s WHERE s.sinema_no = si.sinema_no) AS toplam_kapasite,
                       (SELECT COUNT(*) FROM GOSTERIM g WHERE g.sinema_no = si.sinema_no) AS gosterim_sayisi,
                       (SELECT IIf(IsNull(SUM(g.bilet_fiyati)), 0, SUM(g.bilet_fiyati))
                        FROM GOSTERIM g INNER JOIN REZERVASYON r ON g.gosterim_no = r.gosterim_no
                        WHERE g.sinema_no = si.sinema_no AND r.odeme_durumu = 'Odendi') AS hasilat
                FROM SINEMA AS si
                ORDER BY si.ad");

            var teller = db.CokDegerliHarita("SINEMA_TELEFON", "sinema_no", "telefon");
            Sinemalar.Columns.Add("telefonlar", typeof(string));
            foreach (DataRow r in Sinemalar.Rows)
            {
                int sno = Convert.ToInt32(r["sinema_no"]);
                r["telefonlar"] = teller.TryGetValue(sno, out var t) ? t : "-";
            }
        }

        public IActionResult OnPostSil(int id)
        {
            try
            {
                db.Calistir("DELETE FROM SINEMA_TELEFON WHERE sinema_no = ?", id);
                db.Calistir("DELETE FROM SALON WHERE sinema_no = ?", id);
                db.Calistir("DELETE FROM SINEMA WHERE sinema_no = ?", id);
                TempData["Mesaj"] = "Sinema silindi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage();
        }
    }
}
