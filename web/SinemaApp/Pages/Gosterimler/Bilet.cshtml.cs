using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySqlConnector;
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
            Gosterim = db.Satir("SELECT * FROM v_gosterim_detay WHERE gosterim_no = @id", Db.P("@id", id));
            if (Gosterim == null) return false;

            var dolu = db.Tablo(
                "SELECT koltuk_no FROM rezervasyon WHERE gosterim_no = @id AND odeme_durumu <> 'Iptal'",
                Db.P("@id", id));
            foreach (DataRow r in dolu.Rows)
                DoluKoltuklar.Add(Convert.ToInt32(r["koltuk_no"]));

            Musteriler = db.Tablo("SELECT musteri_no, ad, soyad, eposta FROM musteri ORDER BY ad, soyad");

            var baslangic = Convert.ToDateTime(Gosterim["tarih"]) + (TimeSpan)Gosterim["baslangic_saati"];
            Gecmis = baslangic < DateTime.Now;
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
                // asıl kontroller sp_rezervasyon_yap + trg_rezervasyon_ekle içinde
                using var con = db.BaglantiAc();
                using var cmd = new MySqlCommand("sp_rezervasyon_yap", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_musteri_no", MusteriNo);
                cmd.Parameters.AddWithValue("p_gosterim_no", id);
                cmd.Parameters.AddWithValue("p_koltuk_no", KoltukNo);
                var cikis = new MySqlParameter("p_rez_no", MySqlDbType.Int32)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(cikis);
                cmd.ExecuteNonQuery();

                TempData["Mesaj"] = $"Rezervasyon oluşturuldu. Rezervasyon no: {cikis.Value}, koltuk: {Bicim.Koltuk(KoltukNo)}. Ödeme bekleniyor.";
                return RedirectToPage("/Rezervasyonlar/Index", new { vurgula = cikis.Value });
            }
            catch (MySqlException ex)
            {
                TempData["Hata"] = Db.HataMesaji(ex);
                return RedirectToPage(new { id });
            }
        }
    }
}
