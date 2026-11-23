using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SinemaApp.Data;

namespace SinemaApp.Pages.Gosterimler
{
    public class BiletModel : PageModel
    {
        private readonly Db db;

        public BiletModel(Db db)
        {
            this.db = db;
        }

        public DataRow Gosterim { get; set; }
        public HashSet<int> DoluKoltuklar { get; set; } = new HashSet<int>();
        public DataTable Musteriler { get; set; }
        public bool Gecmis { get; set; }

        [BindProperty] public int MusteriNo { get; set; }
        [BindProperty] public int KoltukNo { get; set; }

        private bool Yukle(int id)
        {
            Gosterim = db.Satir($"SELECT * FROM ({Db.GosterimDetaySql}) AS q WHERE gosterim_no = ?", id);
            if (Gosterim == null) return false;

            var dolu = db.Tablo(
                "SELECT koltuk_no FROM REZERVASYON WHERE gosterim_no = ? AND odeme_durumu <> 'Iptal'", id);
            foreach (DataRow r in dolu.Rows)
                DoluKoltuklar.Add(Convert.ToInt32(r["koltuk_no"]));

            Musteriler = db.Tablo("SELECT musteri_no, ad, soyad, eposta FROM MUSTERI ORDER BY ad, soyad");
            Gecmis = false;
            return true;
        }

        public IActionResult OnGet(int id)
        {
            if (!Yukle(id)) return NotFound();
            return Page();
        }

        public IActionResult OnPost(int id)
        {
            if (MusteriNo == 0 || KoltukNo == 0)
            {
                TempData["Hata"] = "Müşteri ve koltuk seçmelisiniz.";
                return RedirectToPage(new { id });
            }

            try
            {
                long yeniNo = db.RezervasyonYap(MusteriNo, id, KoltukNo);
                TempData["Mesaj"] = $"Rezervasyon oluşturuldu. Rezervasyon no: {yeniNo}, koltuk: {Bicim.Koltuk(KoltukNo)}. Ödeme bekleniyor.";
                return RedirectToPage("/Rezervasyonlar/Index", new { vurgula = yeniNo });
            }
            catch (Exception ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
                return RedirectToPage(new { id });
            }
        }
    }
}
