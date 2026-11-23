using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
            var kosullar = new List<string> { "1=1" };
            var prm = new List<object>();

            if (!string.IsNullOrEmpty(Durum))
            {
                kosullar.Add("odeme_durumu = ?");
                prm.Add(Durum);
            }
            if (!string.IsNullOrWhiteSpace(Ara))
            {
                var a = "%" + Ara.Trim() + "%";
                if (int.TryParse(Ara.Trim(), out int rno))
                {
                    kosullar.Add("(musteri LIKE ? OR film_adi LIKE ? OR rezervasyon_no = ?)");
                    prm.Add(a); prm.Add(a); prm.Add(rno);
                }
                else
                {
                    kosullar.Add("(musteri LIKE ? OR film_adi LIKE ?)");
                    prm.Add(a); prm.Add(a);
                }
            }

            Rezervasyonlar = db.Tablo(
                $"SELECT * FROM ({Db.RezervasyonDetaySql}) AS q WHERE {string.Join(" AND ", kosullar)} ORDER BY rezervasyon_no DESC",
                prm.ToArray());
        }

        public IActionResult OnPostOdeme(int id)
        {
            try
            {
                db.Calistir("UPDATE REZERVASYON SET odeme_durumu = 'Odendi' WHERE rezervasyon_no = ? AND odeme_durumu = 'Beklemede'", id);
                TempData["Mesaj"] = $"{id} numaralı rezervasyonun ödemesi alındı.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { Durum, Ara });
        }

        public IActionResult OnPostIptal(int id)
        {
            try
            {
                db.Calistir("UPDATE REZERVASYON SET odeme_durumu = 'Iptal' WHERE rezervasyon_no = ?", id);
                TempData["Mesaj"] = $"{id} numaralı rezervasyon iptal edildi.";
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
            }
            return RedirectToPage(new { Durum, Ara });
        }
    }
}
